using Saga.Data;
using Saga.Gameplay;
using Saga.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga.Core
{
    /// <summary>
    /// Sprint 1-3 stopgap: builds the Main scene at runtime if it's not already authored.
    /// Lets the gameplay loop run without manual Unity scene editing while the MCP bridge is down.
    ///
    /// Sprint 4+ : remove this and scene-author Main directly. See DESIGN_DECISIONS_LOG.md 2026-05-27.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainSceneBootstrap : MonoBehaviour
    {
        // -- Palette per 05_VISUAL_STYLE.md ------------------------------
        // Sprint 3: scene bg moves to #1a1a1a (Camera clear color), UI cards stay on #161616.
        private static readonly Color BgDojo       = new Color(0.102f, 0.102f, 0.102f, 1f);  // #1a1a1a Camera clear
        private static readonly Color TextPrimary  = new Color(0.98f, 0.98f, 0.98f, 1f);     // #fafafa
        private static readonly Color TextSecondary = new Color(0.53f, 0.53f, 0.53f, 1f);    // #888
        private static readonly Color BgModal       = new Color(0f, 0f, 0f, 1f);             // pure black overlay base, alpha controlled by CanvasGroup
        private static readonly Color MannequinWood = new Color(0.42f, 0.27f, 0.14f, 1f);    // bois sombre

        // -- World layout (orthographic camera, sizes in world units) ----
        private const float CameraOrthoSize = 3.0f; // tighter zoom per Sprint 3 fix #4
        private static readonly Vector3 CharacterPosition  = new Vector3(-1.8f, -0.6f, 0f);
        private static readonly Vector3 MannequinPosition  = new Vector3( 3.0f, -0.6f, 0f); // pushed right per Sprint 3 fix #2

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapIfMainScene()
        {
            var active = SceneManager.GetActiveScene();
            if (active.name != "Main") return;
            if (FindFirstObjectByType<MainSceneBootstrap>() != null) return;
            var go = new GameObject(nameof(MainSceneBootstrap));
            go.AddComponent<MainSceneBootstrap>();
        }

        public Canvas MainCanvas { get; private set; }
        public Transform CharacterTransform { get; private set; }
        public Transform MannequinTransform { get; private set; }

        private void Awake()
        {
            // Build only once. If user authors the scene later, this short-circuits cleanly.
            if (FindFirstObjectByType<ForceCounterView>() != null)
            {
                MainCanvas = FindFirstObjectByType<Canvas>();
                return;
            }

            // Sprint 3 fix #3 — sweep any pre-existing SpriteRenderer in the scene (likely from
            // scene templates / URP 2D defaults that may have shipped a placeholder background quad).
            // Runs BEFORE we create our own WorldRoot so it never destroys what we build.
            CleanLeftoverWorldSprites();

            EnsureMainCamera();
            var worldRoot = new GameObject("WorldRoot").transform;
            BuildCharacter(worldRoot);
            BuildMannequin(worldRoot);
            BuildSlashFxSpawner(worldRoot);

            BuildEventSystem();
            MainCanvas = BuildCanvas();
            BuildForceCounter(MainCanvas);
            BuildComboMeter(MainCanvas);
            BuildTapHandler(MainCanvas);
            BuildTapFxSpawner(MainCanvas);
            BuildUpgradePanel(MainCanvas);
            BuildStadeTransitionOverlay(MainCanvas);
        }

        private static void CleanLeftoverWorldSprites()
        {
            var existing = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existing == null || existing.Length == 0) return;
            foreach (var sr in existing)
            {
                if (sr == null) continue;
                Debug.LogWarning($"[MainSceneBootstrap] Destroying pre-existing SpriteRenderer '{sr.gameObject.name}' at {sr.transform.position} (sprite='{(sr.sprite != null ? sr.sprite.name : "null")}')");
                Destroy(sr.gameObject);
            }
        }

        // -------- Camera + world ----------------------------------------

        private static void EnsureMainCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera));
                go.tag = "MainCamera";
                cam = go.GetComponent<Camera>();
                go.transform.position = new Vector3(0, 0, -10);
            }
            cam.orthographic = true;
            cam.orthographicSize = CameraOrthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BgDojo;
            cam.nearClipPlane = -10f;
            cam.farClipPlane = 100f;
        }

        private void BuildCharacter(Transform parent)
        {
            var go = new GameObject("Character", typeof(SpriteRenderer), typeof(SpriteAnimator), typeof(CharacterView));
            go.transform.SetParent(parent, false);
            go.transform.position = CharacterPosition;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            var anim = go.GetComponent<SpriteAnimator>();
            anim.Renderer = sr;
            anim.Library = Resources.Load<SpriteAnimationLibrary>("Animations/AdventurerAnimationLibrary");

            var view = go.GetComponent<CharacterView>();
            view.Animator = anim;

            CharacterTransform = go.transform;
        }

        private void BuildMannequin(Transform parent)
        {
            var go = new GameObject("Mannequin", typeof(SpriteRenderer), typeof(MannequinView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = CreateMannequinSprite();
            sr.color = Color.white; // sprite carries the wood tone
            sr.sortingOrder = 5;

            MannequinTransform = go.transform;
        }

        private void BuildSlashFxSpawner(Transform parent)
        {
            var go = new GameObject("SlashFxSpawner", typeof(SlashFxSpawner));
            go.transform.SetParent(parent, false);
            var s = go.GetComponent<SlashFxSpawner>();
            s.CharacterTransform = CharacterTransform;
            s.MannequinTransform = MannequinTransform;
        }

        /// <summary>
        /// Procedural wooden-post placeholder (40×80 px, brown). Sprint 4+ replace with proper art.
        /// Sprint 3 fix #2: bumped from 16×40 → 40×80 for visibility.
        /// </summary>
        private static Sprite CreateMannequinSprite()
        {
            const int w = 40, h = 80;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            var wood = new Color32((byte)(MannequinWood.r * 255), (byte)(MannequinWood.g * 255), (byte)(MannequinWood.b * 255), 255);
            var darker = new Color32((byte)(wood.r * 0.7f), (byte)(wood.g * 0.7f), (byte)(wood.b * 0.7f), 255);
            var darkest = new Color32((byte)(wood.r * 0.5f), (byte)(wood.g * 0.5f), (byte)(wood.b * 0.5f), 255);
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    // Wider base (bottom 12px), narrower mid, capped top.
                    var inBase = y < 10;
                    var inCap  = y > h - 12;
                    var inMid  = !inBase && !inCap;

                    var distFromCenter = Mathf.Abs(x - w * 0.5f);
                    var insideMidSilhouette = distFromCenter < w * 0.32f;  // narrower
                    var insideBaseSilhouette = distFromCenter < w * 0.48f; // wider
                    var insideCapSilhouette  = distFromCenter < w * 0.40f;

                    var inside = (inMid && insideMidSilhouette) || (inBase && insideBaseSilhouette) || (inCap && insideCapSilhouette);
                    if (!inside) { pixels[y * w + x] = new Color32(0, 0, 0, 0); continue; } // transparent outside

                    var edge = distFromCenter > w * 0.30f && inMid;
                    var ringMark = (y == 30 || y == 50) && inMid;          // horizontal trim
                    var color = ringMark ? darkest : (edge ? darker : wood);
                    pixels[y * w + x] = color;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            // PPU 32 (was 16): with sprite 40×80, world size is now 1.25×2.5 units (was 2.5×5).
            // Brings the mannequin ratio to ~1.6× the character height instead of 3× — Sprint 3 fix #2.
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            sprite.name = "MannequinProcedural";
            return sprite;
        }

        // -------- UI Canvas ---------------------------------------------

        private static void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static Canvas BuildCanvas()
        {
            var go = new GameObject("MainCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void BuildForceCounter(Canvas canvas)
        {
            var go = new GameObject("ForceCounter", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ForceCounterView));
            go.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -160);
            rt.sizeDelta = new Vector2(900, 240); // taller to accommodate Sprint 3 magnitude size up

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.color = TextPrimary;
            label.fontSize = 96;
            label.enableAutoSizing = false;
            label.fontStyle = FontStyles.Normal;
            label.text = "0";

            go.GetComponent<ForceCounterView>().Label = label;
        }

        private static void BuildComboMeter(Canvas canvas)
        {
            var root = new GameObject("ComboMeter", typeof(RectTransform), typeof(CanvasGroup), typeof(ComboMeterView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-48, -48);
            rt.sizeDelta = new Vector2(220, 80);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(root.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.color = TextSecondary;
            label.fontSize = 56;
            label.text = "x1.0";

            var view = root.GetComponent<ComboMeterView>();
            view.Label = label;
            view.Group = group;
        }

        private static void BuildTapHandler(Canvas canvas)
        {
            var go = new GameObject("TapHandler", typeof(TapHandler));
            go.transform.SetParent(canvas.transform, false);
        }

        private static void BuildTapFxSpawner(Canvas canvas)
        {
            var go = new GameObject("TapFxSpawner", typeof(TapFxSpawner));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<TapFxSpawner>().Init(canvas);
        }

        private static void BuildUpgradePanel(Canvas canvas)
        {
            var gm = GameManager.Instance;
            if (gm?.Content == null) return;
            var upgrades = gm.Content.AllUpgrades;
            if (upgrades == null || upgrades.Count == 0)
            {
                Debug.LogWarning("[MainSceneBootstrap] No upgrades in ContentDatabase — run menu \"Saga > Sprint 2 > Generate Upgrade Assets\" then reload Play.");
                return;
            }

            var panel = new GameObject("UpgradePanel", typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0, 0);
            panelRt.anchorMax = new Vector2(1, 0);
            panelRt.pivot = new Vector2(0.5f, 0);
            panelRt.anchoredPosition = new Vector2(0, 96);
            panelRt.sizeDelta = new Vector2(0, 220);

            var count = upgrades.Count;
            var frac = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var card = new GameObject($"Card_{upgrades[i].UpgradeId}", typeof(RectTransform), typeof(UpgradeCardView));
                card.transform.SetParent(panelRt, false);
                var rt = (RectTransform)card.transform;
                rt.anchorMin = new Vector2(i * frac, 0);
                rt.anchorMax = new Vector2((i + 1) * frac, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(12, 8);
                rt.offsetMax = new Vector2(-12, -8);

                card.GetComponent<UpgradeCardView>().Init(upgrades[i]);
            }
        }

        private static void BuildStadeTransitionOverlay(Canvas canvas)
        {
            var root = new GameObject("StadeTransition",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(StadeTransitionView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var bg = root.GetComponent<Image>();
            bg.color = BgModal;
            bg.raycastTarget = false;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(root.transform, false);
            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0.1f, 0.45f);
            titleRt.anchorMax = new Vector2(0.9f, 0.6f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;
            var titleLabel = titleGo.GetComponent<TextMeshProUGUI>();
            titleLabel.alignment = TextAlignmentOptions.Center;
            titleLabel.color = TextPrimary;
            titleLabel.fontSize = 72;
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.text = "";

            var subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(root.transform, false);
            var subRt = (RectTransform)subGo.transform;
            subRt.anchorMin = new Vector2(0.1f, 0.38f);
            subRt.anchorMax = new Vector2(0.9f, 0.45f);
            subRt.offsetMin = Vector2.zero;
            subRt.offsetMax = Vector2.zero;
            var subLabel = subGo.GetComponent<TextMeshProUGUI>();
            subLabel.alignment = TextAlignmentOptions.Center;
            subLabel.color = TextSecondary;
            subLabel.fontSize = 36;
            subLabel.text = "";

            var view = root.GetComponent<StadeTransitionView>();
            view.Overlay = group;
            view.Background = bg;
            view.Title = titleLabel;
            view.Subtitle = subLabel;
        }
    }
}
