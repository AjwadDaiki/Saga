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
    /// Sprint 1 stopgap: builds the Main scene UI hierarchy at runtime if it's not already authored.
    /// Lets the gameplay loop run without manual Unity scene editing while the MCP bridge is down.
    ///
    /// Sprint 2+ : remove this and scene-author the UI directly. See DESIGN_DECISIONS_LOG.md 2026-05-27.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainSceneBootstrap : MonoBehaviour
    {
        // Visual constants from 05_VISUAL_STYLE.md
        private static readonly Color BgDeep = new Color(0.051f, 0.051f, 0.051f, 1f);   // #0d0d0d
        private static readonly Color TextPrimary = new Color(0.98f, 0.98f, 0.98f, 1f); // #fafafa
        private static readonly Color TextSecondary = new Color(0.53f, 0.53f, 0.53f, 1f); // #888

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

        private void Awake()
        {
            // Build only once. If user authors the scene later, this short-circuits cleanly.
            if (FindFirstObjectByType<ForceCounterView>() != null)
            {
                MainCanvas = FindFirstObjectByType<Canvas>();
                return;
            }

            BuildEventSystem();
            MainCanvas = BuildCanvas();
            BuildBackground(MainCanvas);
            BuildForceCounter(MainCanvas);
            BuildComboMeter(MainCanvas);
            BuildTapHandler(MainCanvas);
            BuildTapFxSpawner(MainCanvas);
            BuildUpgradePanel(MainCanvas);
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

            // Panel root: stretched horizontally, anchored to bottom with 96px dock clearance.
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

        private static void BuildTapFxSpawner(Canvas canvas)
        {
            var go = new GameObject("TapFxSpawner", typeof(TapFxSpawner));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<TapFxSpawner>().Init(canvas);
        }

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

        private static void BuildBackground(Canvas canvas)
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = BgDeep;
            img.raycastTarget = false;
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
            rt.sizeDelta = new Vector2(900, 200);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.color = TextPrimary;
            label.fontSize = 96;
            label.enableAutoSizing = false;
            label.fontStyle = FontStyles.Normal;
            label.text = "0";

            var view = go.GetComponent<ForceCounterView>();
            view.Label = label;
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
            // TapHandler doesn't need to be on the Canvas; could be a sibling. Keeping it as a child
            // of the Canvas root just for hierarchy tidiness.
            var go = new GameObject("TapHandler", typeof(TapHandler));
            go.transform.SetParent(canvas.transform, false);
        }
    }
}
