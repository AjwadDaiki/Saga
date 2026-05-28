using DG.Tweening;
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
        // Sprint 7.5 portrait pivot: world X positions tightened so character + mannequin both fit
        // inside a 9:16 ortho frustum (orthoSize 3 → ±1.69 horizontal). Was (-1.8, 3.0) for landscape.
        private static readonly Vector3 CharacterPosition  = new Vector3(-0.9f, -0.6f, 0f);
        private static readonly Vector3 MannequinPosition  = new Vector3( 1.1f, -0.6f, 0f);

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
            BuildAmbientBackground(worldRoot); // Sprint 7.5: 3-layer dojo background (gradient + particles + floor)
            BuildCharacter(worldRoot);
            BuildMannequin(worldRoot);
            BuildAdversaire(worldRoot);
            BuildCapitaine(worldRoot);
            BuildMaitre(worldRoot);
            BuildSlashFxSpawner(worldRoot);

            BuildEventSystem();
            MainCanvas = BuildCanvas();
            BuildForceCounter(MainCanvas);
            BuildComboMeter(MainCanvas);
            BuildTapHandler(MainCanvas);
            BuildTapFxSpawner(MainCanvas);
            BuildUpgradePanel(MainCanvas);
            BuildStadeTransitionOverlay(MainCanvas);

            // Sprint 4: combat active system UI
            BuildAdversaireProgressBar(MainCanvas);
            BuildCombatHud(MainCanvas);
            BuildAdversaireSpawnView(MainCanvas);
            BuildDeathOverlay(MainCanvas);

            // Sprint 5: Élan + Vague + Capitaine
            var elanRow = BuildElanRow(MainCanvas);
            BuildVagueFlashOverlay(MainCanvas);
            BuildCapitaineIntroOverlay(MainCanvas);
            BuildCapitaineDeathOverlay(MainCanvas);

            // Sprint 6: Souffle + Maître + Prestige
            BuildSouffleButton(MainCanvas);
            var citationModal = BuildCitationInputModal(MainCanvas);
            var affronterModal = BuildAffronterMaitreModal(MainCanvas);
            BuildAffronterMaitreButton(elanRow, affronterModal);
            BuildMaitreIntroOverlay(MainCanvas);
            BuildPrestigeCinematicOverlay(MainCanvas, citationModal);

            // Sprint 7: Inventaire
            var inventoryModal = BuildEquipmentInventoryModal(MainCanvas);
            BuildInventaireButton(MainCanvas, inventoryModal);
        }

        private void BuildMaitre(Transform parent)
        {
            var go = new GameObject("Maitre", typeof(MaitreWorldView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            var auraGo = new GameObject("Aura", typeof(SpriteRenderer));
            auraGo.transform.SetParent(go.transform, false);
            var aura = auraGo.GetComponent<SpriteRenderer>();
            aura.sortingOrder = 3;
            aura.color = new Color(1, 1, 1, 0);

            var bodyGo = new GameObject("Body", typeof(SpriteRenderer));
            bodyGo.transform.SetParent(go.transform, false);
            var body = bodyGo.GetComponent<SpriteRenderer>();
            body.sortingOrder = 6;
            body.color = new Color(1, 1, 1, 0);

            var view = go.GetComponent<MaitreWorldView>();
            view.BodyRenderer = body;
            view.AuraRenderer = aura;

            MaitreTransform = go.transform;
        }

        private void BuildCapitaine(Transform parent)
        {
            // Same world position as Adversaire (mutually exclusive). Aura is a child renderer.
            var go = new GameObject("Capitaine", typeof(CapitaineWorldView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            // Aura first (child), drawn behind body.
            var auraGo = new GameObject("Aura", typeof(SpriteRenderer));
            auraGo.transform.SetParent(go.transform, false);
            var aura = auraGo.GetComponent<SpriteRenderer>();
            aura.sortingOrder = 3;
            aura.color = new Color(1, 1, 1, 0);

            // Body
            var bodyGo = new GameObject("Body", typeof(SpriteRenderer));
            bodyGo.transform.SetParent(go.transform, false);
            var body = bodyGo.GetComponent<SpriteRenderer>();
            body.sortingOrder = 6;
            body.color = new Color(1, 1, 1, 0);

            var view = go.GetComponent<CapitaineWorldView>();
            view.BodyRenderer = body;
            view.AuraRenderer = aura;

            CapitaineTransform = go.transform;
        }

        private RectTransform BuildElanRow(Canvas canvas)
        {
            // Sprint 7.5 portrait pivot: bands now mirror the 9:16 phone layout.
            //   0-8%        : bottom safe area (iOS home indicator)
            //   8-34%       : Upgrade cards (3 cards stacked VERTICALLY)
            //   34-42%      : Élan row (bar + Vague button + Affronter Maître button)
            //   42-74%      : gameplay zone (character + mannequin + adversaire/capitaine/maitre)
            //   74-78%      : Prochain Adversaire bar
            //   78-92%      : Top HUD (Force counter, Combo, Souffle, Inventaire, CombatHud)
            //   92-100%     : top safe area (iOS notch)
            //
            // Row split inside the band: Élan bar (0..0.58), Vague (0.60..0.78), Affronter (0.80..1.0).
            var row = new GameObject("ElanRow", typeof(RectTransform));
            row.transform.SetParent(canvas.transform, false);
            var rowRt = (RectTransform)row.transform;
            rowRt.anchorMin = new Vector2(0.05f, 0.34f);
            rowRt.anchorMax = new Vector2(0.95f, 0.42f);
            rowRt.offsetMin = Vector2.zero;
            rowRt.offsetMax = Vector2.zero;

            var tokens = DesignTokens.Get();

            // Élan bar (left ~58% of the row)
            var bar = new GameObject("ElanBar",
                typeof(RectTransform), typeof(ElanBarView));
            bar.transform.SetParent(rowRt, false);
            var barRt = (RectTransform)bar.transform;
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(0.58f, 1);
            barRt.offsetMin = Vector2.zero;
            barRt.offsetMax = Vector2.zero;

            // Glow halo behind the bar — picks up the accent color when fill > 90%.
            var glowGo = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glowGo.transform.SetParent(barRt, false);
            var glowRt = (RectTransform)glowGo.transform;
            glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-12, -12); glowRt.offsetMax = new Vector2(12, 12);
            var glowImg = glowGo.GetComponent<Image>();
            glowImg.color = new Color(tokens.accentPrimary.r, tokens.accentPrimary.g, tokens.accentPrimary.b, 0f);
            glowImg.raycastTarget = false;

            // Bar background
            var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(barRt, false);
            var bgRt = (RectTransform)bgGo.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = tokens.surfaceLow;
            bgImg.raycastTarget = false;

            // Bar fill — gradient effect via accent_action (warm orange base).
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(barRt, false);
            var fillRt = (RectTransform)fillGo.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -4);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.color = tokens.accentAction;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.raycastTarget = false;

            // Centered label — JetBrains Mono Bold for the numeric percentage.
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(barRt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = tokens.textPrimary;
            lblTmp.font = tokens.NumbersFont;
            lblTmp.fontSize = 28;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.text = "ÉLAN 0%";
            lblTmp.raycastTarget = false;

            var barView = bar.GetComponent<ElanBarView>();
            barView.FillImage = fillImg;
            barView.Label = lblTmp;
            barView.PulseTarget = barRt;
            barView.GlowImage = glowImg;

            // Vague button (middle 60-78% of the row) — Primary CTA variant (gradient + pulse).
            var btn = new GameObject("VagueButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(VagueButtonView));
            btn.transform.SetParent(rowRt, false);
            var btnRt = (RectTransform)btn.transform;
            btnRt.anchorMin = new Vector2(0.60f, 0);
            btnRt.anchorMax = new Vector2(0.78f, 1);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;

            var btnImg = btn.GetComponent<Image>();
            btnImg.color = new Color(0.98f, 0.78f, 0.46f, 0.7f);
            btnImg.raycastTarget = true;

            var btnGroup = btn.GetComponent<CanvasGroup>();
            btnGroup.alpha = 0f;
            btnGroup.blocksRaycasts = false;
            btnGroup.interactable = false;

            var btnLblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnLblGo.transform.SetParent(btnRt, false);
            var btnLblRt = (RectTransform)btnLblGo.transform;
            btnLblRt.anchorMin = Vector2.zero; btnLblRt.anchorMax = Vector2.one;
            btnLblRt.offsetMin = Vector2.zero; btnLblRt.offsetMax = Vector2.zero;
            var btnLblTmp = btnLblGo.GetComponent<TextMeshProUGUI>();
            btnLblTmp.alignment = TextAlignmentOptions.Center;
            btnLblTmp.color = new Color(0.10f, 0.08f, 0.04f, 1f);
            btnLblTmp.fontSize = 40;
            btnLblTmp.fontStyle = FontStyles.Bold;
            btnLblTmp.text = "VAGUE";
            btnLblTmp.raycastTarget = false;

            var btnView = btn.GetComponent<VagueButtonView>();
            btnView.Group = btnGroup;
            btnView.Background = btnImg;
            btnView.Label = btnLblTmp;
            btnView.Root = btnRt;

            return rowRt;
        }

        // -- Sprint 6 builders -------------------------------------------

        private void BuildAffronterMaitreButton(RectTransform elanRow, AffronterMaitreModal modal)
        {
            if (elanRow == null) return;
            var btn = new GameObject("AffronterMaitreButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(AffronterMaitreButtonView));
            btn.transform.SetParent(elanRow, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(0.80f, 0);
            rt.anchorMax = new Vector2(1f, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = btn.GetComponent<Image>();
            img.color = new Color(0.85f, 0.65f, 0.28f, 0.6f); // gold-ish — distinct from Vague's ambre
            img.raycastTarget = true;

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = new Color(0.10f, 0.08f, 0.04f, 1f);
            lblTmp.fontSize = 22;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.text = "AFFRONTER\nUN MAÎTRE";
            lblTmp.raycastTarget = false;

            var view = btn.GetComponent<AffronterMaitreButtonView>();
            view.Group = group;
            view.Background = img;
            view.Label = lblTmp;
            view.Root = rt;
            view.Modal = modal;
        }

        private void BuildSouffleButton(Canvas canvas)
        {
            var btn = new GameObject("SouffleButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(SouffleButtonView));
            btn.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)btn.transform;
            // Top-left anchor, 24px from top + 24px from left
            rt.anchorMin = new Vector2(0, 1f);
            rt.anchorMax = new Vector2(0, 1f);
            rt.pivot = new Vector2(0, 1f);
            rt.anchoredPosition = new Vector2(32, -32);
            rt.sizeDelta = new Vector2(160, 100);

            var img = btn.GetComponent<Image>();
            img.color = new Color(0.20f, 0.18f, 0.32f, 0.85f); // cool blueish — meditation vibes
            img.raycastTarget = true;

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = new Color(0.98f, 0.98f, 0.98f, 1f);
            lblTmp.fontSize = 22;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.text = "SOUFFLE";
            lblTmp.raycastTarget = false;

            var view = btn.GetComponent<SouffleButtonView>();
            view.Group = group;
            view.Background = img;
            view.Label = lblTmp;
            view.Root = rt;
        }

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
            subTmp.color = TextSecondary;
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
            citTmp.color = TextPrimary;
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
                TextSecondary, 36, FontStyles.Normal);
            var maitreCit = AddCinematicLabel(rt, "MaitreCitation",
                new Vector2(0.1f, 0.40f), new Vector2(0.9f, 0.47f),
                TextPrimary, 30, FontStyles.Italic);

            var statsLine = AddCinematicLabel(rt, "StatsLine",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.60f),
                TextPrimary, 28, FontStyles.Normal);
            var echos = AddCinematicLabel(rt, "EchosLabel",
                new Vector2(0, 0.30f), new Vector2(1, 0.40f),
                new Color(0.98f, 0.78f, 0.46f, 1f), 60, FontStyles.Bold);

            var heritage = AddCinematicLabel(rt, "Heritage",
                new Vector2(0, 0.60f), new Vector2(1, 0.66f),
                TextSecondary, 28, FontStyles.Normal);
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
            subTmp.color = TextSecondary;
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
            citTmp.color = TextPrimary;
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
            citTmp.color = TextPrimary;
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

        private void BuildAdversaire(Transform parent)
        {
            // Same position as mannequin — they're mutually exclusive (phase-driven visibility).
            var go = new GameObject("Adversaire", typeof(SpriteRenderer), typeof(AdversaireWorldView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            sr.color = new Color(1, 1, 1, 0); // start invisible

            var view = go.GetComponent<AdversaireWorldView>();
            view.Renderer = sr;

            AdversaireTransform = go.transform;
        }

        private static void BuildAdversaireProgressBar(Canvas canvas)
        {
            var root = new GameObject("AdversaireProgressBar",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AdversaireProgressBarView));
            root.transform.SetParent(canvas.transform, false);
            var rootRt = (RectTransform)root.transform;
            rootRt.anchorMin = new Vector2(0.5f, 1f);
            rootRt.anchorMax = new Vector2(0.5f, 1f);
            rootRt.pivot = new Vector2(0.5f, 1f);
            rootRt.anchoredPosition = new Vector2(0, -32);
            rootRt.sizeDelta = new Vector2(800, 80);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;

            // Background plate
            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(rootRt, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            var bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0.10f, 0.10f, 0.10f, 0.85f);
            bgImage.raycastTarget = false;

            // Fill (horizontal)
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rootRt, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = new Vector2(0, 0); fillRt.anchorMax = new Vector2(1, 1);
            fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -28);
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.98f, 0.78f, 0.46f, 0.9f); // ambre
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;
            fillImage.raycastTarget = false;

            // Label
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(rootRt, false);
            var labelRt = (RectTransform)label.transform;
            labelRt.anchorMin = new Vector2(0, 1); labelRt.anchorMax = new Vector2(1, 1);
            labelRt.pivot = new Vector2(0.5f, 1f);
            labelRt.anchoredPosition = new Vector2(0, -4);
            labelRt.sizeDelta = new Vector2(0, 24);
            var labelTmp = label.GetComponent<TextMeshProUGUI>();
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = TextSecondary;
            labelTmp.fontSize = 22;
            labelTmp.text = "Prochain adversaire";
            labelTmp.raycastTarget = false;

            var view = root.GetComponent<AdversaireProgressBarView>();
            view.Group = group;
            view.FillImage = fillImage;
            view.Label = labelTmp;
            view.PulseTarget = rootRt;
        }

        private static void BuildCombatHud(Canvas canvas)
        {
            var root = new GameObject("CombatHud",
                typeof(RectTransform), typeof(CanvasGroup), typeof(CombatHudView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -132);
            rt.sizeDelta = new Vector2(900, 200);

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
            nameLabel.color = TextPrimary;
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
            hpLabelTmp.color = TextPrimary;
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
            chronoTmp.color = TextPrimary;
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

        private static void BuildAdversaireSpawnView(Canvas canvas)
        {
            var root = new GameObject("AdversaireSpawnView",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AdversaireSpawnView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0, 0.55f); rt.anchorMax = new Vector2(1, 0.7f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var label = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(rt, false);
            var labelRt = (RectTransform)label.transform;
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var labelTmp = label.GetComponent<TextMeshProUGUI>();
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f); // ambre
            labelTmp.fontSize = 96;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.text = "";
            labelTmp.raycastTarget = false;

            var view = root.GetComponent<AdversaireSpawnView>();
            view.Group = group;
            view.NameLabel = labelTmp;
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
            subTmp.color = TextSecondary;
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
            // Sprint 7.5: use bg_deep (#0a0a0a) so the radial gradient on top has somewhere to fade into.
            cam.backgroundColor = DesignTokens.Get().bgDeep;
            cam.nearClipPlane = -10f;
            cam.farClipPlane = 100f;
        }

        /// <summary>
        /// Sprint 7.5 ambient background — 3 visual layers built in world space, behind everything.
        ///   z=10 : radial gradient quad (procedural, dark center, deeper at edges — adds depth)
        ///   z=9  : floor strip (procedural, dark wood gradient at the bottom — grounds the scene)
        ///   z=8  : ambient ParticleSystem (subtle amber motes drifting up — "dojo qui respire")
        /// All purely procedural so no art assets are required.
        /// </summary>
        private void BuildAmbientBackground(Transform parent)
        {
            var tokens = DesignTokens.Get();
            var root = new GameObject("Ambient");
            root.transform.SetParent(parent, false);

            // Layer 1 — radial gradient backdrop.
            var bgGo = new GameObject("BgGradient", typeof(SpriteRenderer));
            bgGo.transform.SetParent(root.transform, false);
            bgGo.transform.position = new Vector3(0, 0, 10f);
            var bgSr = bgGo.GetComponent<SpriteRenderer>();
            bgSr.sprite = CreateRadialGradientSprite(tokens.bgMain, tokens.bgDeep);
            bgSr.sortingOrder = -10;
            // Stretch to cover the ortho frustum + a little extra so we never see clear color edges.
            // orthoSize 3 → 6 unit tall. width 6 × 16/9 / 2 ≈ 5.34. We use 8×14 unit cover.
            bgGo.transform.localScale = new Vector3(8f, 14f, 1f);

            // Layer 2 — floor strip.
            var floorGo = new GameObject("Floor", typeof(SpriteRenderer));
            floorGo.transform.SetParent(root.transform, false);
            floorGo.transform.position = new Vector3(0, -2.3f, 9f);
            var floorSr = floorGo.GetComponent<SpriteRenderer>();
            floorSr.sprite = CreateFloorSprite();
            floorSr.sortingOrder = -8;
            floorGo.transform.localScale = new Vector3(7f, 1f, 1f);

            // Layer 3 — ambient particles (amber motes drifting up).
            BuildAmbientParticles(root.transform, tokens);
        }

        private static void BuildAmbientParticles(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("AmbientParticles", typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0, -2.5f, 8f);

            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 8f;
            main.loop = true;
            main.startLifetime = 6f;
            main.startSpeed = 0.25f;
            main.startSize = 0.04f;
            main.startColor = new Color(tokens.accentPrimary.r, tokens.accentPrimary.g, tokens.accentPrimary.b, 0.13f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = 0f;
            main.gravityModifier = -0.02f; // very slight upward drift

            var emission = ps.emission;
            emission.rateOverTime = 4.5f; // ~25-30 particles in flight at a time

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(5.5f, 0.2f, 1f); // emit along a horizontal strip at the bottom

            // Fade in then out across lifetime.
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(tokens.accentPrimary, 0f),
                    new GradientColorKey(tokens.accentPrimary, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.15f, 0.3f),
                    new GradientAlphaKey(0.10f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = grad;

            // Renderer settings.
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sortingOrder = -7;
            r.material = new Material(Shader.Find("Sprites/Default"));
        }

        /// <summary>Procedural radial gradient sprite (256×256) — bright center, dark edges.</summary>
        private static Sprite CreateRadialGradientSprite(Color center, Color edge)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            var maxDist = size * 0.5f * 1.2f; // soft falloff
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - size * 0.5f;
                    var dy = y - size * 0.5f;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var t = Mathf.Clamp01(d / maxDist);
                    // Ease out so the bright center has more presence.
                    t = t * t;
                    var c = Color.Lerp(center, edge, t);
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            s.name = "RadialGradientBg";
            return s;
        }

        /// <summary>
        /// Procedural floor sprite: vertical gradient from dark wood to almost-black, with subtle
        /// vertical plank divisions. 256×64.
        /// </summary>
        private static Sprite CreateFloorSprite()
        {
            const int w = 256, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var top = new Color32(42, 26, 16, 255);     // #2a1a10
            var bottom = new Color32(26, 14, 8, 255);    // #1a0e08
            var plank = new Color32(18, 10, 6, 255);
            var pixels = new Color32[w * h];
            // 6 planks
            for (var y = 0; y < h; y++)
            {
                var t = 1f - (y / (float)h); // y=0 bottom -> t=1, y=h-1 top -> t=~0
                var rowColor = new Color32(
                    (byte)Mathf.Lerp(bottom.r, top.r, 1f - t),
                    (byte)Mathf.Lerp(bottom.g, top.g, 1f - t),
                    (byte)Mathf.Lerp(bottom.b, top.b, 1f - t),
                    255);
                for (var x = 0; x < w; x++)
                {
                    // Subtle plank divisions every ~42px.
                    var isDivider = (x % 42) < 2;
                    pixels[y * w + x] = isDivider ? plank : rowColor;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            s.name = "FloorProcedural";
            return s;
        }

        private void BuildCharacter(Transform parent)
        {
            // Sprint 7: layered character = 3 stacked SpriteRenderers (Body / Armor / Weapon),
            // driven by a single LayeredCharacterRenderer on the root. SaveService.Migrate guarantees
            // equippedBodyId defaults to body_chibi_neutral so the body slot is always visible.
            //
            // Sprint 7.5 fix: if the body SpriteLayerSet hasn't been generated yet (Editor utility
            // not run, or rvros sprites missing), fall back to a runtime placeholder so the player
            // is never invisible. Logs a warning telling the user how to fix it permanently.
            var go = new GameObject("Character", typeof(LayeredCharacterRenderer), typeof(CharacterView));
            go.transform.SetParent(parent, false);
            go.transform.position = CharacterPosition;
            // Sprint 7.5: 1.5× scale so the chibi reads well at the smaller portrait gameplay band.
            go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            // Ground shadow under the character.
            BuildGroundShadow(go.transform, scale: new Vector3(0.7f, 0.35f, 1f));

            var body = new GameObject("Body", typeof(SpriteRenderer));
            body.transform.SetParent(go.transform, false);
            var bodySr = body.GetComponent<SpriteRenderer>();
            bodySr.sortingOrder = 5;

            var armor = new GameObject("Armor", typeof(SpriteRenderer));
            armor.transform.SetParent(go.transform, false);
            var armorSr = armor.GetComponent<SpriteRenderer>();
            armorSr.sortingOrder = 6;

            var weapon = new GameObject("Weapon", typeof(SpriteRenderer));
            weapon.transform.SetParent(go.transform, false);
            var weaponSr = weapon.GetComponent<SpriteRenderer>();
            weaponSr.sortingOrder = 7;

            var renderer = go.GetComponent<LayeredCharacterRenderer>();
            renderer.BodyRenderer = bodySr;
            renderer.ArmorRenderer = armorSr;
            renderer.WeaponRenderer = weaponSr;

            var gm = GameManager.Instance;
            var content = gm?.Content;
            var state = gm?.State;

            // Resolve body layer with safety fallback.
            SpriteLayerSet bodyLayer = null;
            if (content != null && state != null)
                bodyLayer = content.GetSpriteLayerSet(state.equippedBodyId);
            if (bodyLayer == null || bodyLayer.SpriteIdle == null || bodyLayer.SpriteIdle.Length == 0)
            {
                Debug.LogWarning("[MainSceneBootstrap] body_chibi_neutral SpriteLayerSet missing or has no idle frames — using runtime placeholder. Run 'Saga > Sprint 7 > Generate Sprite Layer Sets' to populate proper sprites.");
                bodyLayer = CreatePlaceholderBodyLayerSet();
            }

            renderer.SetLayer(EquipmentSlot.Body, bodyLayer);
            renderer.SetLayer(EquipmentSlot.Armor, content?.GetSpriteLayerSet(state?.equippedArmorId));
            renderer.SetLayer(EquipmentSlot.Weapon, content?.GetSpriteLayerSet(state?.equippedWeaponId));

            var view = go.GetComponent<CharacterView>();
            view.Renderer = renderer;

            CharacterTransform = go.transform;
        }

        /// <summary>
        /// Runtime fallback used when the body SpriteLayerSet asset is missing. Produces a
        /// 64×96 ambre rectangle so the character is at least visible at the canonical
        /// CharacterPosition. Replaced as soon as the player equips a real body layer set.
        /// </summary>
        private static SpriteLayerSet CreatePlaceholderBodyLayerSet()
        {
            var sprite = CreatePlaceholderBodySprite();
            return SpriteLayerSet.CreateRuntime(
                id: EquipmentConstants.DefaultBodyId,
                slot: EquipmentSlot.Body,
                idle: new[] { sprite },
                displayName: "Corps (placeholder)");
        }

        private static Sprite CreatePlaceholderBodySprite()
        {
            // 64×96 ambre block with a subtle darker outline so the silhouette reads as
            // "humanoid placeholder" without faking detail.
            const int w = 64;
            const int h = 96;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var amber = new Color32(250, 199, 117, 255);     // 0.98, 0.78, 0.46
            var outline = new Color32(160, 110, 60, 255);
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var isEdge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    pixels[y * w + x] = isEdge ? outline : amber;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            sprite.name = "BodyPlaceholderProcedural";
            return sprite;
        }

        private void BuildMannequin(Transform parent)
        {
            var go = new GameObject("Mannequin", typeof(SpriteRenderer), typeof(MannequinView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = CreateMannequinSprite();
            sr.color = Color.white;
            sr.sortingOrder = 5;

            // Sprint 7.5: subtle ground shadow + gentle sway DOTween so the dojo feels alive.
            BuildGroundShadow(go.transform, scale: new Vector3(1.0f, 0.6f, 1f), yOffset: 0.02f);

            // Slow ±2° rotation, infinite yoyo. SetLink ensures the tween dies with the GO.
            go.transform.rotation = Quaternion.Euler(0, 0, -2f);
            go.transform.DORotate(new Vector3(0, 0, 2f), 2.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(go, LinkBehaviour.KillOnDestroy);

            MannequinTransform = go.transform;
        }

        /// <summary>
        /// Adds a soft elliptical shadow at the feet of a world entity. Sorted just above the floor
        /// (-7) and below all character/mannequin/adversaire renderers (5+).
        /// </summary>
        private static SpriteRenderer BuildGroundShadow(Transform parent, Vector3 scale, float yOffset = 0f)
        {
            var go = new GameObject("Shadow", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, yOffset, 0.1f);
            go.transform.localScale = scale;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = CreateShadowSprite();
            sr.sortingOrder = -5; // above floor (-8/-7), below characters (5+)
            return sr;
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
        /// Sprint 7.5 mannequin redesign — 3 distinct sections (head sphere / torso cylinder / wider socle)
        /// with cordage rings, dark wood gradient and subtle edge shadow. 80×140 procedural.
        /// </summary>
        private static Sprite CreateMannequinSprite()
        {
            const int w = 80, h = 140;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];

            // Wood palette (dark base, lighter highlights along the centerline).
            var woodBase = new Color32(58, 42, 24, 255);    // #3a2a18
            var woodLight = new Color32(90, 64, 40, 255);   // #5a4028 — center highlight
            var woodDark = new Color32(34, 22, 12, 255);    // edge shadow
            var cord = new Color32(15, 9, 5, 255);          // pitch black cordage

            // Section heights (from bottom).
            const int socleTop = 22;   // 0..22  : socle (wider)
            const int torsoTop = 100;  // 22..100 : torso (medium cylinder)
            // 100..140 : head (narrowest, capped silhouette)

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var dx = Mathf.Abs(x - w * 0.5f);

                    float halfWidth;
                    if (y < socleTop)
                    {
                        // Socle: widest at the bottom, tapers slightly up.
                        var t = y / (float)socleTop;
                        halfWidth = Mathf.Lerp(0.40f, 0.32f, t) * w;
                    }
                    else if (y < torsoTop)
                    {
                        // Torso: medium cylinder with a very subtle barrel.
                        var t = (y - socleTop) / (float)(torsoTop - socleTop);
                        halfWidth = (0.26f + 0.02f * Mathf.Sin(t * Mathf.PI)) * w;
                    }
                    else
                    {
                        // Head: smaller sphere capped on top.
                        var t = (y - torsoTop) / (float)(h - torsoTop);
                        // Half-circle outline.
                        var r = 0.22f * w;
                        var dyHead = (y - torsoTop) - r * 0.6f;
                        var dist = Mathf.Sqrt(dx * dx + dyHead * dyHead);
                        if (dist > r) { pixels[y * w + x] = new Color32(0, 0, 0, 0); continue; }
                        halfWidth = w; // already inside the circle test
                    }

                    var inside = dx <= halfWidth;
                    if (!inside) { pixels[y * w + x] = new Color32(0, 0, 0, 0); continue; }

                    // Vertical highlight along the centerline → 3D feel.
                    var centerWeight = 1f - Mathf.Clamp01(dx / Mathf.Max(1f, halfWidth));
                    var edgeWeight = 1f - centerWeight;
                    var r2 = (byte)Mathf.Lerp(woodBase.r, woodLight.r, centerWeight * 0.6f);
                    var g2 = (byte)Mathf.Lerp(woodBase.g, woodLight.g, centerWeight * 0.6f);
                    var b2 = (byte)Mathf.Lerp(woodBase.b, woodLight.b, centerWeight * 0.6f);
                    if (edgeWeight > 0.85f)
                    {
                        r2 = woodDark.r; g2 = woodDark.g; b2 = woodDark.b;
                    }

                    // Cordage rings: thin horizontal stripes at torso y=42, y=72.
                    var isCord = (y == 42 || y == 43 || y == 72 || y == 73) && y > socleTop && y < torsoTop;
                    if (isCord) { r2 = cord.r; g2 = cord.g; b2 = cord.b; }

                    pixels[y * w + x] = new Color32(r2, g2, b2, 255);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            // PPU 32: 80×140 px → 2.5×4.375 world units. Pivot bottom-center keeps the socle on the floor.
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            sprite.name = "MannequinProcedural";
            return sprite;
        }

        /// <summary>Soft black ellipse — used as ground shadow under the player + mannequin.</summary>
        private static Sprite CreateShadowSprite()
        {
            const int w = 96, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var dx = (x - w * 0.5f) / (w * 0.5f);
                    var dy = (y - h * 0.5f) / (h * 0.5f);
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var a = Mathf.Clamp01(1f - d);
                    a = a * a * 0.55f; // softer falloff
                    pixels[y * w + x] = new Color32(0, 0, 0, (byte)(a * 255));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit: 96);
            s.name = "ShadowEllipseProcedural";
            return s;
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
            // Sprint 7.5 polish: small "FORCE" label above + JetBrains-styled value below, drop shadow.
            var tokens = DesignTokens.Get();

            // Small "FORCE" label.
            var lblGo = new GameObject("ForceLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(canvas.transform, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = new Vector2(0.5f, 1f);
            lblRt.anchorMax = new Vector2(0.5f, 1f);
            lblRt.pivot = new Vector2(0.5f, 1f);
            lblRt.anchoredPosition = new Vector2(0, -110);
            lblRt.sizeDelta = new Vector2(400, 28);
            var lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = tokens.textSecondary;
            lblTmp.font = tokens.PrimaryFont;
            lblTmp.fontSize = tokens.fontCaption;
            lblTmp.fontStyle = FontStyles.SemiBold;
            lblTmp.text = "FORCE";
            lblTmp.characterSpacing = 8f;
            lblTmp.raycastTarget = false;

            // The big number itself.
            var go = new GameObject("ForceCounter", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ForceCounterView));
            go.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -140);
            rt.sizeDelta = new Vector2(900, 140);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.color = tokens.textPrimary;
            label.font = tokens.NumbersFont;
            label.fontSize = 84; // overwritten on first Refresh based on magnitude
            label.enableAutoSizing = false;
            label.fontStyle = FontStyles.Bold;
            label.text = "0";
            // Drop shadow: TMP underlay channel works on dark backgrounds.
            label.fontMaterial.SetFloat("_UnderlayOffsetX", 0f);
            label.fontMaterial.SetFloat("_UnderlayOffsetY", -1f);
            label.fontMaterial.SetFloat("_UnderlaySoftness", 0.4f);
            label.fontMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.45f));

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

        private void BuildTapFxSpawner(Canvas canvas)
        {
            var go = new GameObject("TapFxSpawner", typeof(TapFxSpawner));
            go.transform.SetParent(canvas.transform, false);
            var spawner = go.GetComponent<TapFxSpawner>();
            spawner.Init(canvas);
            // Combat "-X" damage numbers spawn from the enemy anchor. Adversaire + Capitaine share
            // the same world position, so either transform works — we pick Adversaire by convention.
            spawner.EnemyAnchor = AdversaireTransform;
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

            // Sprint 5 round-2: switched from absolute (sizeDelta 220px, anchoredPosition 96px)
            // Sprint 7.5 portrait pivot : cards stack VERTICALLY (was a 3-column row in landscape).
            // Band 8-34% from bottom of screen = ~500px on a 1920 canvas → ~165px per card.
            var panel = new GameObject("UpgradePanel", typeof(RectTransform));
            panel.transform.SetParent(canvas.transform, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.04f, 0.08f);
            panelRt.anchorMax = new Vector2(0.96f, 0.34f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var count = upgrades.Count;
            var frac = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var card = new GameObject($"Card_{upgrades[i].UpgradeId}", typeof(RectTransform), typeof(UpgradeCardView));
                card.transform.SetParent(panelRt, false);
                var rt = (RectTransform)card.transform;
                // Stack top->bottom: card 0 at top, card N-1 at bottom.
                // Unity anchors: y=1 is top, y=0 is bottom. Card i occupies (1 - (i+1)*frac) .. (1 - i*frac).
                rt.anchorMin = new Vector2(0, 1f - (i + 1) * frac);
                rt.anchorMax = new Vector2(1, 1f - i * frac);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(8, 6);
                rt.offsetMax = new Vector2(-8, -6);

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

        // -------- Sprint 7: Inventaire ----------------------------------

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

        private void BuildInventaireButton(Canvas canvas, EquipmentInventoryModal modal)
        {
            var tokens = DesignTokens.Get();
            var btn = new GameObject("InventaireButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Button));
            btn.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)btn.transform;
            // Sprint 7.5 portrait fix: live under the Souffle button (top-left column) so we don't
            // collide with the ComboMeter that sits at top-right.
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(32, -148);
            rt.sizeDelta = new Vector2(160, 80);

            var img = btn.GetComponent<Image>();
            img.color = tokens.surfaceMid;
            img.raycastTarget = true;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = tokens.textPrimary;
            lblTmp.font = tokens.PrimaryFont;
            lblTmp.fontSize = tokens.fontH3;
            lblTmp.fontStyle = FontStyles.SemiBold;
            lblTmp.text = "INVENTAIRE";
            lblTmp.raycastTarget = false;

            btn.GetComponent<Button>().onClick.AddListener(modal.Open);
            SagaButton.Wrap(btn, SagaButton.Variant.Standard);
        }
    }
}
