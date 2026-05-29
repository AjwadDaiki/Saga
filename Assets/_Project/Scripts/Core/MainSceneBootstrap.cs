using Saga.Data;
using Saga.Gameplay;
using Saga.UI;
using Saga.UI.Builders;
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
    ///
    /// Sprint 7.5 refonte (Phase 2) : the monolithic ~2.2k lines builder is split into 7 zone-scoped
    /// builders under <c>Saga.UI.Builders</c>. This file keeps only the orchestrator, scene-setup
    /// scaffolding (camera + canvas + EventSystem) and the overlays/modals/cinematics that don't fit
    /// a single zone.
    /// </summary>
    [DisallowMultipleComponent]
    public class MainSceneBootstrap : MonoBehaviour
    {
        // -- Palette per 05_VISUAL_STYLE.md ------------------------------
        // Sprint 3: scene bg moves to #1a1a1a (Camera clear color), UI cards stay on #161616.
        private static readonly Color BgDojo       = new Color(0.102f, 0.102f, 0.102f, 1f);  // #1a1a1a Camera clear
        internal static readonly Color TextPrimaryColor   = new Color(0.98f, 0.98f, 0.98f, 1f);     // #fafafa
        internal static readonly Color TextSecondaryColor = new Color(0.53f, 0.53f, 0.53f, 1f);    // #888
        internal static readonly Color BgModalColor       = new Color(0f, 0f, 0f, 1f);             // pure black overlay base, alpha controlled by CanvasGroup
        private static readonly Color MannequinWood = new Color(0.42f, 0.27f, 0.14f, 1f);    // bois sombre

        // -- World layout (orthographic camera, sizes in world units) ----
        private const float CameraOrthoSize = 3.0f; // tighter zoom per Sprint 3 fix #4

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
        public Transform AdversaireTransform { get; private set; }
        public Transform CapitaineTransform { get; private set; }
        public Transform MaitreTransform { get; private set; }

        private void Awake()
        {
            Debug.Log("[BOOT-PHASE3] Awake() entered, scene=" + SceneManager.GetActiveScene().name);
            // Build only once. If user authors the scene later, this short-circuits cleanly.
            if (FindFirstObjectByType<ForceCounterView>() != null)
            {
                Debug.LogWarning("[BOOT-PHASE3] SHORT-CIRCUITED — ForceCounterView already in scene, skipping all builders");
                MainCanvas = FindFirstObjectByType<Canvas>();
                return;
            }
            Debug.Log("[BOOT-PHASE3] Bootstrap starting (no short-circuit)");

            // Sprint 3 fix #3 — sweep any pre-existing SpriteRenderer in the scene (likely from
            // scene templates / URP 2D defaults that may have shipped a placeholder background quad).
            // Runs BEFORE we create our own WorldRoot so it never destroys what we build.
            CleanLeftoverWorldSprites();

            EnsureMainCamera();

            var ctx = new BuilderContext
            {
                WorldRoot = new GameObject("WorldRoot").transform,
                Tokens = DesignTokens.Get(),
            };

            BackgroundBuilder.Build(ctx);          // Sprint 7.5 zone 1 — dojo dusk background + particles.
            SceneBuilder.Build(ctx);               // Sprint 7.5 zone 4 — character + mannequin + adv/cap/maitre + slash FX.

            // Copy zone-4 transforms back to the public properties consumed by other scripts.
            CharacterTransform = ctx.CharacterTransform;
            MannequinTransform = ctx.MannequinTransform;
            AdversaireTransform = ctx.AdversaireTransform;
            CapitaineTransform = ctx.CapitaineTransform;
            MaitreTransform = ctx.MaitreTransform;

            BuildEventSystem();
            MainCanvas = BuildCanvas();
            ctx.Canvas = MainCanvas;
            ctx.UIRoot = BuildSafeAreaContainer(MainCanvas);
            Debug.Log("[BOOT] SafeAreaContainer created: " + (ctx.UIRoot != null) + ", name=" + (ctx.UIRoot != null ? ctx.UIRoot.name : "null"));

            SceneBuilder.BuildComboMeter(MainCanvas);
            BuildTapHandler(MainCanvas);
            SceneBuilder.BuildTapFxSpawner(MainCanvas, AdversaireTransform);
            UpgradesBuilder.Build(ctx);            // Sprint 7.5 zone 6 — upgrade card panel.
            BuildStadeTransitionOverlay(MainCanvas);

            // Sprint 4: combat active system UI
            BuildCombatHud(MainCanvas);
            SceneBuilder.BuildAdversaireSpawnView(MainCanvas);
            BuildDeathOverlay(MainCanvas);

            // Sprint 5/6: Élan + Vague + Souffle + Affronter Maître button (button is bound to its
            // modal below once the modal exists).
            SkillsBuilder.Build(ctx);              // Sprint 7.5 zone 5 — Élan row + Vague + Souffle + Affronter (modal=null).
            BuildVagueFlashOverlay(MainCanvas);
            BuildCapitaineIntroOverlay(MainCanvas);
            BuildCapitaineDeathOverlay(MainCanvas);

            // Sprint 6: Maître + Prestige
            var citationModal = BuildCitationInputModal(MainCanvas);
            var affronterModal = BuildAffronterMaitreModal(MainCanvas);
            SkillsBuilder.BindAffronterMaitre(ctx, affronterModal);
            BuildMaitreIntroOverlay(MainCanvas);
            BuildPrestigeCinematicOverlay(MainCanvas, citationModal);

            // Sprint 7: Inventaire
            // Sprint 7.5 zone 7 / Q8 decision : le side-rail INVENTAIRE legacy a été supprimé.
            // L'inventaire migre dans l'onglet Artifacts (placeholder Coming Soon Sprint 7.5,
            // full UI Sprint 8+). EquipmentInventoryModal reste built mais devient orphelin
            // — il sera re-wire dans l'onglet Artifacts post-7.5.
            BuildEquipmentInventoryModal(MainCanvas);

            // Sprint 7.5 refonte : top bar pills + bottom nav 5 onglets.
            StageBuilder.Build(ctx);               // Sprint 7.5 zone 3 — placeholder, Phase 3 stage chip.
            TopBarBuilder.Build(ctx);              // Sprint 7.5 zone 2 — currency pills + settings.
            BottomNavBuilder.Build(ctx);           // Sprint 7.5 zone 7 — 5-tab bottom nav.
        }

        // ============================================================
        //  Overlays & modals — kept here because they cut across zones.
        // ============================================================

        private AffronterMaitreModal BuildAffronterMaitreModal(Canvas canvas)
        {
            var root = new GameObject("AffronterMaitreModal",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(AffronterMaitreModal));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.transform.SetAsLastSibling();

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            bg.raycastTarget = true;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;

            // Title
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(rt, false);
            var titleRt = (RectTransform)title.transform;
            titleRt.anchorMin = new Vector2(0, 0.85f); titleRt.anchorMax = new Vector2(1, 0.93f);
            titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;
            var titleTmp = title.GetComponent<TextMeshProUGUI>();
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f);
            titleTmp.fontSize = 48;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.text = "CHOISIS TON MAÎTRE";
            titleTmp.raycastTarget = false;

            // Scrollable list container — simple VerticalLayoutGroup since we have at most 8 entries
            var listContainer = new GameObject("List",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            listContainer.transform.SetParent(rt, false);
            var listRt = (RectTransform)listContainer.transform;
            listRt.anchorMin = new Vector2(0.1f, 0.15f); listRt.anchorMax = new Vector2(0.9f, 0.82f);
            listRt.offsetMin = Vector2.zero; listRt.offsetMax = Vector2.zero;
            var vlg = listContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Reculer button (bottom)
            var back = new GameObject("Reculer",
                typeof(RectTransform), typeof(Image), typeof(Button));
            back.transform.SetParent(rt, false);
            var backRt = (RectTransform)back.transform;
            backRt.anchorMin = new Vector2(0.5f, 0.05f); backRt.anchorMax = new Vector2(0.5f, 0.05f);
            backRt.pivot = new Vector2(0.5f, 0.5f);
            backRt.anchoredPosition = new Vector2(0, 30);
            backRt.sizeDelta = new Vector2(220, 50);
            back.GetComponent<Image>().color = new Color(0.20f, 0.20f, 0.20f, 0.9f);
            var backLblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            backLblGo.transform.SetParent(back.transform, false);
            var blRt = (RectTransform)backLblGo.transform;
            blRt.anchorMin = Vector2.zero; blRt.anchorMax = Vector2.one;
            blRt.offsetMin = Vector2.zero; blRt.offsetMax = Vector2.zero;
            var blTmp = backLblGo.GetComponent<TextMeshProUGUI>();
            blTmp.alignment = TextAlignmentOptions.Center;
            blTmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            blTmp.fontSize = 22;
            blTmp.text = "Reculer";
            blTmp.raycastTarget = false;

            var view = root.GetComponent<AffronterMaitreModal>();
            view.Group = group;
            view.ListContainer = listRt;
            back.GetComponent<Button>().onClick.AddListener(view.Close);

            return view;
        }

        private CitationInputModal BuildCitationInputModal(Canvas canvas)
        {
            var root = new GameObject("CitationInputModal",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(CitationInputModal));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.transform.SetAsLastSibling();

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.9f);
            bg.raycastTarget = true;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;

            // Panel container
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(rt, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f); panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(720, 360);
            panel.GetComponent<Image>().color = new Color(0.12f, 0.10f, 0.08f, 0.96f);

            // Title
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(panel.transform, false);
            var titleRt = (RectTransform)title.transform;
            titleRt.anchorMin = new Vector2(0, 0.78f); titleRt.anchorMax = new Vector2(1, 0.95f);
            titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;
            var titleTmp = title.GetComponent<TextMeshProUGUI>();
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f);
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.text = "Écris ta dernière phrase…";
            titleTmp.raycastTarget = false;

            // InputField TMP
            var inputGo = new GameObject("Input",
                typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputGo.transform.SetParent(panel.transform, false);
            var inputRt = (RectTransform)inputGo.transform;
            inputRt.anchorMin = new Vector2(0.05f, 0.42f); inputRt.anchorMax = new Vector2(0.95f, 0.72f);
            inputRt.offsetMin = Vector2.zero; inputRt.offsetMax = Vector2.zero;
            inputGo.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 1f);

            // Input internal text + placeholder children (TMP_InputField needs them)
            var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputRt, false);
            var taRt = (RectTransform)textArea.transform;
            taRt.anchorMin = Vector2.zero; taRt.anchorMax = Vector2.one;
            taRt.offsetMin = new Vector2(12, 6); taRt.offsetMax = new Vector2(-12, -6);

            var placeholderGo = new GameObject("Placeholder",
                typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderGo.transform.SetParent(textArea.transform, false);
            var phRt = (RectTransform)placeholderGo.transform;
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero; phRt.offsetMax = Vector2.zero;
            var phTmp = placeholderGo.GetComponent<TextMeshProUGUI>();
            phTmp.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.fontSize = 24;
            phTmp.text = "(80 caractères max)";
            phTmp.raycastTarget = false;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(textArea.transform, false);
            var txRt = (RectTransform)textGo.transform;
            txRt.anchorMin = Vector2.zero; txRt.anchorMax = Vector2.one;
            txRt.offsetMin = Vector2.zero; txRt.offsetMax = Vector2.zero;
            var txTmp = textGo.GetComponent<TextMeshProUGUI>();
            txTmp.color = new Color(0.98f, 0.98f, 0.98f, 1f);
            txTmp.fontSize = 24;
            txTmp.raycastTarget = false;

            var input = inputGo.GetComponent<TMP_InputField>();
            input.textViewport = (RectTransform)textArea.transform;
            input.textComponent = txTmp;
            input.placeholder = phTmp;

            // Confirm button
            var confirm = new GameObject("Confirm",
                typeof(RectTransform), typeof(Image), typeof(Button));
            confirm.transform.SetParent(panel.transform, false);
            var crt = (RectTransform)confirm.transform;
            crt.anchorMin = new Vector2(0.5f, 0.08f); crt.anchorMax = new Vector2(0.5f, 0.08f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = new Vector2(0, 30);
            crt.sizeDelta = new Vector2(220, 50);
            confirm.GetComponent<Image>().color = new Color(0.98f, 0.78f, 0.46f, 0.95f);
            var cLblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            cLblGo.transform.SetParent(confirm.transform, false);
            var cLblRt = (RectTransform)cLblGo.transform;
            cLblRt.anchorMin = Vector2.zero; cLblRt.anchorMax = Vector2.one;
            cLblRt.offsetMin = Vector2.zero; cLblRt.offsetMax = Vector2.zero;
            var cLblTmp = cLblGo.GetComponent<TextMeshProUGUI>();
            cLblTmp.alignment = TextAlignmentOptions.Center;
            cLblTmp.color = new Color(0.05f, 0.04f, 0.02f, 1f);
            cLblTmp.fontSize = 22;
            cLblTmp.fontStyle = FontStyles.Bold;
            cLblTmp.text = "Confirmer";
            cLblTmp.raycastTarget = false;

            var view = root.GetComponent<CitationInputModal>();
            view.Group = group;
            view.TitleLabel = titleTmp;
            view.Input = input;
            view.ConfirmButton = confirm.GetComponent<Button>();

            return view;
        }

        private void BuildMaitreIntroOverlay(Canvas canvas)
        {
            var root = new GameObject("MaitreIntro",
                typeof(RectTransform), typeof(CanvasGroup), typeof(MaitreIntroView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;

            // Arena tint underlay (color from MaitreData.ArenaBackgroundColor)
            var arenaGo = new GameObject("ArenaTint", typeof(RectTransform), typeof(Image));
            arenaGo.transform.SetParent(rt, false);
            var arenaRt = (RectTransform)arenaGo.transform;
            arenaRt.anchorMin = Vector2.zero; arenaRt.anchorMax = Vector2.one;
            arenaRt.offsetMin = Vector2.zero; arenaRt.offsetMax = Vector2.zero;
            var arenaImg = arenaGo.GetComponent<Image>();
            arenaImg.color = new Color(0, 0, 0, 0.4f);
            arenaImg.raycastTarget = false;

            // Vignette (darker frame)
            var vignetteGo = new GameObject("Vignette", typeof(RectTransform), typeof(Image));
            vignetteGo.transform.SetParent(rt, false);
            var vRt = (RectTransform)vignetteGo.transform;
            vRt.anchorMin = Vector2.zero; vRt.anchorMax = Vector2.one;
            vRt.offsetMin = Vector2.zero; vRt.offsetMax = Vector2.zero;
            var vImg = vignetteGo.GetComponent<Image>();
            vImg.color = new Color(0f, 0f, 0f, 0.75f);
            vImg.raycastTarget = false;

            // Name MASSIVE
            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(rt, false);
            var nameRt = (RectTransform)nameGo.transform;
            nameRt.anchorMin = new Vector2(0, 0.55f); nameRt.anchorMax = new Vector2(1, 0.72f);
            nameRt.offsetMin = Vector2.zero; nameRt.offsetMax = Vector2.zero;
            var nameTmp = nameGo.GetComponent<TextMeshProUGUI>();
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f);
            nameTmp.fontSize = 110;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.text = "";
            nameTmp.raycastTarget = false;

            var subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(rt, false);
            var subRt = (RectTransform)subGo.transform;
            subRt.anchorMin = new Vector2(0, 0.48f); subRt.anchorMax = new Vector2(1, 0.55f);
            subRt.offsetMin = Vector2.zero; subRt.offsetMax = Vector2.zero;
            var subTmp = subGo.GetComponent<TextMeshProUGUI>();
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = TextSecondaryColor;
            subTmp.fontSize = 38;
            subTmp.text = "";
            subTmp.raycastTarget = false;

            var citGo = new GameObject("Citation", typeof(RectTransform), typeof(TextMeshProUGUI));
            citGo.transform.SetParent(rt, false);
            var citRt = (RectTransform)citGo.transform;
            citRt.anchorMin = new Vector2(0.1f, 0.36f); citRt.anchorMax = new Vector2(0.9f, 0.46f);
            citRt.offsetMin = Vector2.zero; citRt.offsetMax = Vector2.zero;
            var citTmp = citGo.GetComponent<TextMeshProUGUI>();
            citTmp.alignment = TextAlignmentOptions.Center;
            citTmp.color = TextPrimaryColor;
            citTmp.fontSize = 32;
            citTmp.fontStyle = FontStyles.Italic;
            citTmp.text = "";
            citTmp.raycastTarget = false;

            var view = root.GetComponent<MaitreIntroView>();
            view.Group = group;
            view.Vignette = vImg;
            view.ArenaTint = arenaImg;
            view.NameLabel = nameTmp;
            view.SubtitleLabel = subTmp;
            view.CitationLabel = citTmp;
        }

        private void BuildPrestigeCinematicOverlay(Canvas canvas, CitationInputModal citationModal)
        {
            var root = new GameObject("PrestigeCinematic",
                typeof(RectTransform), typeof(CanvasGroup), typeof(PrestigeCinematicView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.transform.SetAsLastSibling();

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;

            // Fade image (covers everything; alpha controlled by cinematic)
            var fadeGo = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            fadeGo.transform.SetParent(rt, false);
            var fadeRt = (RectTransform)fadeGo.transform;
            fadeRt.anchorMin = Vector2.zero; fadeRt.anchorMax = Vector2.one;
            fadeRt.offsetMin = Vector2.zero; fadeRt.offsetMax = Vector2.zero;
            var fadeImg = fadeGo.GetComponent<Image>();
            fadeImg.color = new Color(0f, 0f, 0f, 0f);
            fadeImg.raycastTarget = false;

            // Title "TU ES MORT"
            var title = AddCinematicLabel(rt, "Title",
                new Vector2(0, 0.55f), new Vector2(1, 0.72f),
                new Color(0.6f, 0.05f, 0.05f, 1f), 128, FontStyles.Bold);
            var subtitle = AddCinematicLabel(rt, "Subtitle",
                new Vector2(0, 0.48f), new Vector2(1, 0.55f),
                TextSecondaryColor, 36, FontStyles.Normal);
            var maitreCit = AddCinematicLabel(rt, "MaitreCitation",
                new Vector2(0.1f, 0.40f), new Vector2(0.9f, 0.47f),
                TextPrimaryColor, 30, FontStyles.Italic);

            var statsLine = AddCinematicLabel(rt, "StatsLine",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.60f),
                TextPrimaryColor, 28, FontStyles.Normal);
            var echos = AddCinematicLabel(rt, "EchosLabel",
                new Vector2(0, 0.30f), new Vector2(1, 0.40f),
                new Color(0.98f, 0.78f, 0.46f, 1f), 60, FontStyles.Bold);

            var heritage = AddCinematicLabel(rt, "Heritage",
                new Vector2(0, 0.60f), new Vector2(1, 0.66f),
                TextSecondaryColor, 28, FontStyles.Normal);
            var playerCit = AddCinematicLabel(rt, "PlayerCitation",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.58f),
                new Color(0.95f, 0.94f, 0.91f, 1f), 36, FontStyles.Italic);

            var renaissance = AddCinematicLabel(rt, "Renaissance",
                new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.55f),
                new Color(0.1f, 0.08f, 0.04f, 1f), 38, FontStyles.Italic);

            var view = root.GetComponent<PrestigeCinematicView>();
            view.Root = group;
            view.FadeImage = fadeImg;
            view.Title = title;
            view.Subtitle = subtitle;
            view.MaitreCitation = maitreCit;
            view.StatsLine = statsLine;
            view.EchosLabel = echos;
            view.HeritageLabel = heritage;
            view.PlayerCitation = playerCit;
            view.RenaissanceLabel = renaissance;
            view.CitationModal = citationModal;
        }

        private static TextMeshProUGUI AddCinematicLabel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color color, float fontSize, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.text = "";
            tmp.raycastTarget = false;
            go.SetActive(false);
            return tmp;
        }

        private void BuildVagueFlashOverlay(Canvas canvas)
        {
            var root = new GameObject("VagueFlash",
                typeof(RectTransform), typeof(Image), typeof(VagueVisualController));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            // Render behind death overlay but above gameplay.
            root.transform.SetSiblingIndex(canvas.transform.childCount - 2);

            var img = root.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = false;

            root.GetComponent<VagueVisualController>().FlashImage = img;
        }

        private void BuildCapitaineIntroOverlay(Canvas canvas)
        {
            var root = new GameObject("CapitaineIntro",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(CapitaineIntroView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var vignette = root.GetComponent<Image>();
            vignette.color = new Color(0f, 0f, 0f, 0.6f); // assombrissement subtil
            vignette.raycastTarget = false;

            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(rt, false);
            var nameRt = (RectTransform)nameGo.transform;
            nameRt.anchorMin = new Vector2(0, 0.55f); nameRt.anchorMax = new Vector2(1, 0.7f);
            nameRt.offsetMin = Vector2.zero; nameRt.offsetMax = Vector2.zero;
            var nameTmp = nameGo.GetComponent<TextMeshProUGUI>();
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f);
            nameTmp.fontSize = 96;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.text = "";
            nameTmp.raycastTarget = false;

            var subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(rt, false);
            var subRt = (RectTransform)subGo.transform;
            subRt.anchorMin = new Vector2(0, 0.48f); subRt.anchorMax = new Vector2(1, 0.55f);
            subRt.offsetMin = Vector2.zero; subRt.offsetMax = Vector2.zero;
            var subTmp = subGo.GetComponent<TextMeshProUGUI>();
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = TextSecondaryColor;
            subTmp.fontSize = 36;
            subTmp.text = "";
            subTmp.raycastTarget = false;

            var citGo = new GameObject("Citation", typeof(RectTransform), typeof(TextMeshProUGUI));
            citGo.transform.SetParent(rt, false);
            var citRt = (RectTransform)citGo.transform;
            citRt.anchorMin = new Vector2(0.1f, 0.36f); citRt.anchorMax = new Vector2(0.9f, 0.46f);
            citRt.offsetMin = Vector2.zero; citRt.offsetMax = Vector2.zero;
            var citTmp = citGo.GetComponent<TextMeshProUGUI>();
            citTmp.alignment = TextAlignmentOptions.Center;
            citTmp.color = TextPrimaryColor;
            citTmp.fontSize = 32;
            citTmp.fontStyle = FontStyles.Italic;
            citTmp.text = "";
            citTmp.raycastTarget = false;

            var view = root.GetComponent<CapitaineIntroView>();
            view.Group = group;
            view.Vignette = vignette;
            view.NameLabel = nameTmp;
            view.SubtitleLabel = subTmp;
            view.CitationLabel = citTmp;
        }

        private void BuildCapitaineDeathOverlay(Canvas canvas)
        {
            var root = new GameObject("CapitaineDeath",
                typeof(RectTransform), typeof(CanvasGroup), typeof(CapitaineDeathView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.transform.SetAsLastSibling();

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            // Flash image
            var flashGo = new GameObject("Flash", typeof(RectTransform), typeof(Image));
            flashGo.transform.SetParent(rt, false);
            var flashRt = (RectTransform)flashGo.transform;
            flashRt.anchorMin = Vector2.zero; flashRt.anchorMax = Vector2.one;
            flashRt.offsetMin = Vector2.zero; flashRt.offsetMax = Vector2.zero;
            var flashImg = flashGo.GetComponent<Image>();
            flashImg.color = new Color(1f, 1f, 1f, 0f);
            flashImg.raycastTarget = false;

            var citGo = new GameObject("Citation", typeof(RectTransform), typeof(TextMeshProUGUI));
            citGo.transform.SetParent(rt, false);
            var citRt = (RectTransform)citGo.transform;
            citRt.anchorMin = new Vector2(0.1f, 0.5f); citRt.anchorMax = new Vector2(0.9f, 0.6f);
            citRt.offsetMin = Vector2.zero; citRt.offsetMax = Vector2.zero;
            var citTmp = citGo.GetComponent<TextMeshProUGUI>();
            citTmp.alignment = TextAlignmentOptions.Center;
            citTmp.color = TextPrimaryColor;
            citTmp.fontSize = 38;
            citTmp.fontStyle = FontStyles.Italic;
            citTmp.text = "";
            citTmp.raycastTarget = false;

            var vicGo = new GameObject("Victory", typeof(RectTransform), typeof(TextMeshProUGUI));
            vicGo.transform.SetParent(rt, false);
            var vicRt = (RectTransform)vicGo.transform;
            vicRt.anchorMin = new Vector2(0, 0.38f); vicRt.anchorMax = new Vector2(1, 0.46f);
            vicRt.offsetMin = Vector2.zero; vicRt.offsetMax = Vector2.zero;
            var vicTmp = vicGo.GetComponent<TextMeshProUGUI>();
            vicTmp.alignment = TextAlignmentOptions.Center;
            vicTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f);
            vicTmp.fontSize = 84;
            vicTmp.fontStyle = FontStyles.Bold;
            vicTmp.text = "VICTOIRE";
            vicTmp.raycastTarget = false;

            var view = root.GetComponent<CapitaineDeathView>();
            view.Group = group;
            view.Flash = flashImg;
            view.CitationLabel = citTmp;
            view.VictoryLabel = vicTmp;
        }

        // ============================================================
        //  Combat overlays + bars
        // ============================================================

        private static void BuildCombatHud(Canvas canvas)
        {
            var root = new GameObject("CombatHud",
                typeof(RectTransform), typeof(CanvasGroup), typeof(CombatHudView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            // Sprint 7.5 fix (BUG 2): below the Force counter (which now ends ~-254). Combat HUD and the
            // Adversaire progress bar are mutually exclusive (combat vs training) so they share this band.
            rt.anchoredPosition = new Vector2(0, -262);
            rt.sizeDelta = new Vector2(940, 200);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            // Adversaire name
            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(rt, false);
            var nameRt = (RectTransform)nameGo.transform;
            nameRt.anchorMin = new Vector2(0, 1); nameRt.anchorMax = new Vector2(1, 1);
            nameRt.pivot = new Vector2(0.5f, 1f);
            nameRt.anchoredPosition = new Vector2(0, 0);
            nameRt.sizeDelta = new Vector2(0, 50);
            var nameLabel = nameGo.GetComponent<TextMeshProUGUI>();
            nameLabel.alignment = TextAlignmentOptions.Center;
            nameLabel.color = TextPrimaryColor;
            nameLabel.fontSize = 38;
            nameLabel.fontStyle = FontStyles.Bold;
            nameLabel.text = "";
            nameLabel.raycastTarget = false;

            // HP bar bg
            var hpBg = new GameObject("HpBg", typeof(RectTransform), typeof(Image));
            hpBg.transform.SetParent(rt, false);
            var hpBgRt = (RectTransform)hpBg.transform;
            hpBgRt.anchorMin = new Vector2(0.5f, 1f); hpBgRt.anchorMax = new Vector2(0.5f, 1f);
            hpBgRt.pivot = new Vector2(0.5f, 1f);
            hpBgRt.anchoredPosition = new Vector2(0, -55);
            hpBgRt.sizeDelta = new Vector2(700, 28);
            var hpBgImg = hpBg.GetComponent<Image>();
            hpBgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            hpBgImg.raycastTarget = false;

            // HP bar fill
            var hpFill = new GameObject("HpFill", typeof(RectTransform), typeof(Image));
            hpFill.transform.SetParent(hpBgRt, false);
            var hpFillRt = (RectTransform)hpFill.transform;
            hpFillRt.anchorMin = Vector2.zero; hpFillRt.anchorMax = Vector2.one;
            hpFillRt.offsetMin = new Vector2(3, 3); hpFillRt.offsetMax = new Vector2(-3, -3);
            var hpFillImg = hpFill.GetComponent<Image>();
            hpFillImg.color = new Color(0.85f, 0.30f, 0.25f, 0.95f); // sang
            hpFillImg.type = Image.Type.Filled;
            hpFillImg.fillMethod = Image.FillMethod.Horizontal;
            hpFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            hpFillImg.fillAmount = 1f;
            hpFillImg.raycastTarget = false;

            // HP numeric label inside the bar
            var hpLabel = new GameObject("HpLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            hpLabel.transform.SetParent(hpBgRt, false);
            var hpLabelRt = (RectTransform)hpLabel.transform;
            hpLabelRt.anchorMin = Vector2.zero; hpLabelRt.anchorMax = Vector2.one;
            hpLabelRt.offsetMin = Vector2.zero; hpLabelRt.offsetMax = Vector2.zero;
            var hpLabelTmp = hpLabel.GetComponent<TextMeshProUGUI>();
            hpLabelTmp.alignment = TextAlignmentOptions.Center;
            hpLabelTmp.color = TextPrimaryColor;
            hpLabelTmp.fontSize = 18;
            hpLabelTmp.text = "";
            hpLabelTmp.raycastTarget = false;

            // Chrono label
            var chrono = new GameObject("Chrono", typeof(RectTransform), typeof(TextMeshProUGUI));
            chrono.transform.SetParent(rt, false);
            var chronoRt = (RectTransform)chrono.transform;
            chronoRt.anchorMin = new Vector2(0.5f, 1f); chronoRt.anchorMax = new Vector2(0.5f, 1f);
            chronoRt.pivot = new Vector2(0.5f, 1f);
            chronoRt.anchoredPosition = new Vector2(0, -100);
            chronoRt.sizeDelta = new Vector2(200, 60);
            var chronoTmp = chrono.GetComponent<TextMeshProUGUI>();
            chronoTmp.alignment = TextAlignmentOptions.Center;
            chronoTmp.color = TextPrimaryColor;
            chronoTmp.fontSize = 48;
            chronoTmp.fontStyle = FontStyles.Bold;
            chronoTmp.text = "0:00";
            chronoTmp.raycastTarget = false;

            var view = root.GetComponent<CombatHudView>();
            view.Group = group;
            view.NameLabel = nameLabel;
            view.HpLabel = hpLabelTmp;
            view.HpFill = hpFillImg;
            view.ChronoLabel = chronoTmp;
        }

        private static void BuildDeathOverlay(Canvas canvas)
        {
            var root = new GameObject("DeathOverlay",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(DeathOverlayView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Render order: append last in canvas → drawn on top.
            root.transform.SetAsLastSibling();

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.95f);
            bg.raycastTarget = true;

            // Title "TU ES MORT"
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(rt, false);
            var titleRt = (RectTransform)title.transform;
            titleRt.anchorMin = new Vector2(0, 0.55f); titleRt.anchorMax = new Vector2(1, 0.7f);
            titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;
            var titleTmp = title.GetComponent<TextMeshProUGUI>();
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.6f, 0.05f, 0.05f, 1f); // rouge sombre
            titleTmp.fontSize = 128;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.text = "TU ES MORT";
            titleTmp.raycastTarget = false;

            // Subtitle quote
            var sub = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            sub.transform.SetParent(rt, false);
            var subRt = (RectTransform)sub.transform;
            subRt.anchorMin = new Vector2(0.1f, 0.42f); subRt.anchorMax = new Vector2(0.9f, 0.52f);
            subRt.offsetMin = Vector2.zero; subRt.offsetMax = Vector2.zero;
            var subTmp = sub.GetComponent<TextMeshProUGUI>();
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = TextSecondaryColor;
            subTmp.fontSize = 28;
            subTmp.fontStyle = FontStyles.Italic;
            subTmp.text = "";
            subTmp.raycastTarget = false;

            // Hint at bottom
            var hint = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hint.transform.SetParent(rt, false);
            var hintRt = (RectTransform)hint.transform;
            hintRt.anchorMin = new Vector2(0, 0.15f); hintRt.anchorMax = new Vector2(1, 0.22f);
            hintRt.offsetMin = Vector2.zero; hintRt.offsetMax = Vector2.zero;
            var hintTmp = hint.GetComponent<TextMeshProUGUI>();
            hintTmp.alignment = TextAlignmentOptions.Center;
            hintTmp.color = new Color(0.55f, 0.55f, 0.55f, 1f);
            hintTmp.fontSize = 28;
            hintTmp.text = "";
            hintTmp.raycastTarget = false;

            var view = root.GetComponent<DeathOverlayView>();
            view.Group = group;
            view.Title = titleTmp;
            view.Subtitle = subTmp;
            view.Hint = hintTmp;
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
            bg.color = BgModalColor;
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
            titleLabel.color = TextPrimaryColor;
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
            subLabel.color = TextSecondaryColor;
            subLabel.fontSize = 36;
            subLabel.text = "";

            var view = root.GetComponent<StadeTransitionView>();
            view.Overlay = group;
            view.Background = bg;
            view.Title = titleLabel;
            view.Subtitle = subLabel;
        }

        // ============================================================
        //  Tap handler (Force/Combo widgets now live in TopBarBuilder/SceneBuilder)
        // ============================================================

        private static void BuildTapHandler(Canvas canvas)
        {
            var go = new GameObject("TapHandler", typeof(TapHandler));
            go.transform.SetParent(canvas.transform, false);
        }

        // ============================================================
        //  Sprint 7 — equipment inventory modal (kept in Bootstrap; the
        //  bottom-nav-adjacent "Inventaire" button lives in BottomNavBuilder).
        // ============================================================

        private EquipmentInventoryModal BuildEquipmentInventoryModal(Canvas canvas)
        {
            var root = new GameObject("EquipmentInventoryModal",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(EquipmentInventoryModal));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.transform.SetAsLastSibling();

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            bg.raycastTarget = true;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;

            // Title.
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(rt, false);
            var titleRt = (RectTransform)title.transform;
            titleRt.anchorMin = new Vector2(0, 0.88f); titleRt.anchorMax = new Vector2(1, 0.95f);
            titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;
            var titleTmp = title.GetComponent<TextMeshProUGUI>();
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.98f, 0.85f, 0.55f, 1f);
            titleTmp.fontSize = 42;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.text = "INVENTAIRE";
            titleTmp.raycastTarget = false;

            // List container with vertical layout.
            var listContainer = new GameObject("List",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            listContainer.transform.SetParent(rt, false);
            var listRt = (RectTransform)listContainer.transform;
            listRt.anchorMin = new Vector2(0.07f, 0.13f); listRt.anchorMax = new Vector2(0.93f, 0.85f);
            listRt.offsetMin = Vector2.zero; listRt.offsetMax = Vector2.zero;
            var vlg = listContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Reculer button (bottom).
            var back = new GameObject("Reculer",
                typeof(RectTransform), typeof(Image), typeof(Button));
            back.transform.SetParent(rt, false);
            var brt = (RectTransform)back.transform;
            brt.anchorMin = new Vector2(0.5f, 0.02f); brt.anchorMax = new Vector2(0.5f, 0.10f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(180, 0);
            back.GetComponent<Image>().color = new Color(0.35f, 0.30f, 0.25f, 1f);
            var brl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            brl.transform.SetParent(back.transform, false);
            var brlRt = (RectTransform)brl.transform;
            brlRt.anchorMin = Vector2.zero; brlRt.anchorMax = Vector2.one;
            brlRt.offsetMin = Vector2.zero; brlRt.offsetMax = Vector2.zero;
            var brlTmp = brl.GetComponent<TextMeshProUGUI>();
            brlTmp.alignment = TextAlignmentOptions.Center;
            brlTmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            brlTmp.fontSize = 22;
            brlTmp.fontStyle = FontStyles.Bold;
            brlTmp.text = "Reculer";
            brlTmp.raycastTarget = false;

            var modal = root.GetComponent<EquipmentInventoryModal>();
            modal.Group = group;
            modal.ListContainer = listRt;
            back.GetComponent<Button>().onClick.AddListener(modal.Close);
            SagaButton.Wrap(back, SagaButton.Variant.Standard);

            return modal;
        }

        // ============================================================
        //  Camera + Canvas + EventSystem scaffolding
        // ============================================================

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
            // Sprint 7.5 refonte : dusk violet (pas de noir) — visible seulement dans d'éventuels gaps.
            cam.backgroundColor = new Color(0.23f, 0.14f, 0.31f, 1f);
            cam.nearClipPlane = -10f;
            cam.farClipPlane = 100f;
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

        /// <summary>
        /// Sprint 7.5 Polish Phase 3 — wraps HUD elements inside <see cref="Screen.safeArea"/>.
        /// Builders that want notch / home-indicator safety parent to this RectTransform via
        /// <see cref="BuilderContext.UIRoot"/>. Cinematics + modal backdrops still parent to the
        /// Canvas itself so they can bleed past the notch.
        /// </summary>
        private static RectTransform BuildSafeAreaContainer(Canvas canvas)
        {
            var go = new GameObject("SafeAreaContainer", typeof(RectTransform), typeof(SafeAreaScaler));
            go.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)go.transform;
            // SafeAreaScaler.Awake/Apply() will populate anchors from Screen.safeArea on the first frame.
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }
    }
}
