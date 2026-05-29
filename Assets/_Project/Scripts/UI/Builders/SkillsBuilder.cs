using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte zone 5 — Skills row : VAGUE (cyan, jauge Élan intégrée) + SOUFFLE (vert menthe).
    ///
    /// Mockup (saga_target_spec.md §4 + Q6/Q8 coord) :
    ///   - 2 boutons énormes côte à côte sous la scène
    ///   - VAGUE bleu cyan : éclair + label + jauge interne "78%" (= Élan%)
    ///   - SOUFFLE vert menthe : icône zen + label
    ///   - PAS de barre Élan séparée (intégrée dans VAGUE)
    ///   - PAS de side-rail Souffle/Inventaire (Inventaire migre dans onglet Artifacts)
    ///
    /// Le bouton "Affronter Maître" devient un petit chip flottant top-droit de la row, visible
    /// seulement quand <c>maitreInvocationSlots ≥ 1</c>.
    /// </summary>
    public static class SkillsBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            Debug.Log("[SKILLS] Build called, parent: " + (ctx.UIRoot != null ? "UIRoot" : "Canvas") + ", tokens=" + (ctx.Tokens != null));
            var tokens = ctx.Tokens;
            if (ctx.Canvas == null || tokens == null) { Debug.LogWarning("[SKILLS] EARLY RETURN: Canvas=" + (ctx.Canvas != null) + " tokens=" + (tokens != null)); return; }
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;

            ctx.ElanRow = BuildSkillsRow(parent, tokens);
            BuildAffronterMaitreButton(ctx.ElanRow, modal: null);
        }

        public static void BindAffronterMaitre(BuilderContext ctx, AffronterMaitreModal modal)
        {
            if (ctx.ElanRow == null || modal == null) return;
            var btn = ctx.ElanRow.Find("AffronterMaitreButton");
            if (btn == null) return;
            var view = btn.GetComponent<AffronterMaitreButtonView>();
            if (view != null) view.Modal = modal;
        }

        // ====================================================================================
        //  ROW — 2 big puffy buttons VAGUE + SOUFFLE side-by-side.
        // ====================================================================================

        private static RectTransform BuildSkillsRow(Transform parent, DesignTokens tokens)
        {
            // Row positioned in the 34-44% band of the canvas (above upgrade cards, below the scene).
            // Anchor.x stretches 4-96% so the 28px padding rule matches the spec.
            var row = new GameObject("SkillsRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rt = (RectTransform)row.transform;
            rt.anchorMin = new Vector2(0.04f, 0.34f);
            rt.anchorMax = new Vector2(0.96f, 0.44f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Vague (left half).
            BuildVagueButton(rt, tokens, anchorX0: 0f, anchorX1: 0.49f);

            // Souffle (right half).
            BuildSouffleButton(rt, tokens, anchorX0: 0.51f, anchorX1: 1f);

            return rt;
        }

        // ====================================================================================
        //  VAGUE — cyan puffy + lightning icon + label + Élan gauge intégrée
        // ====================================================================================

        private static void BuildVagueButton(RectTransform parent, DesignTokens tokens, float anchorX0, float anchorX1)
        {
            var vagueCyan = new Color(0.169f, 0.776f, 1.000f, 1f);     // #2BC6FF
            var vagueDeep = new Color(0.122f, 0.576f, 0.761f, 1f);     // #1F93C2

            var btn = new GameObject("VagueButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(VagueButtonView));
            btn.transform.SetParent(parent, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(anchorX0, 0); rt.anchorMax = new Vector2(anchorX1, 1);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.blocksRaycasts = true; group.interactable = true;

            BuildPuffyButtonShell(rt, tokens, vagueCyan, vagueDeep, radius: 28, floorPx: 8,
                out var faceRt, out var fillImg);

            // Lightning bolt icon (procedural Z-shape) at top-left.
            BuildLightningBolt(faceRt, tokens);

            // Main label "VAGUE".
            var lbl = BuildBigLabel(faceRt, tokens, "VAGUE", topMargin: 14, bottomMargin: 28);

            // Élan inner gauge at bottom — thin horizontal bar inside the button.
            BuildElanGauge(faceRt, tokens, vagueDeep, out var gaugeFill, out var gaugeLabel, out var gaugeGlow);

            var view = btn.GetComponent<VagueButtonView>();
            view.Group = group;
            view.Background = fillImg;
            view.Label = lbl;
            view.Root = rt;
            view.AlwaysVisible = true;
            // VagueButtonView.Awake() ran with AlwaysVisible=false default → it called HideInstant()
            // which already zeroed the root scale. Reset to one so the puffy slot is painted.
            rt.localScale = Vector3.one;
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = false; // gated true again on Élan = max via the view's HandleElanChanged

            // Add ElanBarView on the same GO so it drives the inner gauge.
            var elanView = btn.AddComponent<ElanBarView>();
            elanView.FillImage = gaugeFill;
            elanView.Label = gaugeLabel;
            elanView.PulseTarget = rt;
            elanView.GlowImage = gaugeGlow;
        }

        /// <summary>Lightning bolt shape built from 2 stacked rotated rectangles (a Z-shape).</summary>
        private static void BuildLightningBolt(Transform parent, DesignTokens tokens)
        {
            var ic = new GameObject("Lightning", typeof(RectTransform));
            ic.transform.SetParent(parent, false);
            var rt = (RectTransform)ic.transform;
            rt.anchorMin = new Vector2(0.06f, 0.5f); rt.anchorMax = new Vector2(0.06f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(20, 6);
            rt.sizeDelta = new Vector2(36, 48);

            // Top diagonal bar of the bolt.
            var top = new GameObject("Top", typeof(RectTransform), typeof(Image));
            top.transform.SetParent(rt, false);
            var trt = (RectTransform)top.transform;
            trt.anchorMin = new Vector2(0.5f, 0.65f); trt.anchorMax = new Vector2(0.5f, 0.65f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(-3, 0);
            trt.sizeDelta = new Vector2(26, 8);
            trt.localEulerAngles = new Vector3(0, 0, -30f);
            var timg = top.GetComponent<Image>();
            timg.sprite = PuffySprite.RoundedFill(4); timg.type = Image.Type.Sliced;
            timg.color = tokens.accentPrimary; // jaune éclair
            timg.raycastTarget = false;

            // Bottom diagonal bar.
            var bot = new GameObject("Bot", typeof(RectTransform), typeof(Image));
            bot.transform.SetParent(rt, false);
            var brt = (RectTransform)bot.transform;
            brt.anchorMin = new Vector2(0.5f, 0.32f); brt.anchorMax = new Vector2(0.5f, 0.32f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(3, 0);
            brt.sizeDelta = new Vector2(26, 8);
            brt.localEulerAngles = new Vector3(0, 0, -30f);
            var bimg = bot.GetComponent<Image>();
            bimg.sprite = PuffySprite.RoundedFill(4); bimg.type = Image.Type.Sliced;
            bimg.color = tokens.accentPrimary;
            bimg.raycastTarget = false;
        }

        // ====================================================================================
        //  Élan inner gauge inside VAGUE button (bottom 28% strip).
        // ====================================================================================

        private static void BuildElanGauge(RectTransform parent, DesignTokens tokens, Color deepColor,
            out Image fillImg, out TextMeshProUGUI labelTmp, out Image glowImg)
        {
            var gauge = new GameObject("ElanGauge", typeof(RectTransform));
            gauge.transform.SetParent(parent, false);
            var rt = (RectTransform)gauge.transform;
            rt.anchorMin = new Vector2(0.10f, 0.08f); rt.anchorMax = new Vector2(0.92f, 0.30f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Glow halo behind.
            var glowGo = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glowGo.transform.SetParent(rt, false);
            var glowRt = (RectTransform)glowGo.transform;
            glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-6, -6); glowRt.offsetMax = new Vector2(6, 6);
            glowImg = glowGo.GetComponent<Image>();
            glowImg.sprite = PuffySprite.RoundedFill(10);
            glowImg.type = Image.Type.Sliced;
            glowImg.color = new Color(1f, 1f, 1f, 0f);
            glowImg.raycastTarget = false;

            // Track (deeper cyan).
            var bg = new GameObject("Track", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(rt, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = PuffySprite.RoundedFill(8);
            bgImg.type = Image.Type.Sliced;
            bgImg.color = deepColor;
            bgImg.raycastTarget = false;

            // Charcoal outline.
            var olGo = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            olGo.transform.SetParent(rt, false);
            var olRt = (RectTransform)olGo.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = olGo.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(8, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Fill (light cyan).
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rt, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2, 2); fillRt.offsetMax = new Vector2(-2, -2);
            fillImg = fill.GetComponent<Image>();
            fillImg.sprite = PuffySprite.RoundedFill(6);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.color = new Color(0.65f, 0.92f, 1f, 1f); // pale cyan glow
            fillImg.raycastTarget = false;

            // Label "78%".
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            labelTmp = lblGo.GetComponent<TextMeshProUGUI>();
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.font = tokens.NumbersFont;
            labelTmp.fontSize = 18;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.color = Color.white;
            labelTmp.text = "0%";
            labelTmp.outlineColor = tokens.navyContour;
            labelTmp.outlineWidth = 0.22f;
            labelTmp.raycastTarget = false;
        }

        // ====================================================================================
        //  SOUFFLE — vert menthe puffy + zen icon + label.
        // ====================================================================================

        private static void BuildSouffleButton(RectTransform parent, DesignTokens tokens, float anchorX0, float anchorX1)
        {
            var souffleMint = new Color(0.239f, 0.839f, 0.549f, 1f);   // #3DD68C
            var souffleDeep = new Color(0.118f, 0.612f, 0.353f, 1f);   // #1E9C5A

            var btn = new GameObject("SouffleButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(SouffleButtonView));
            btn.transform.SetParent(parent, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(anchorX0, 0); rt.anchorMax = new Vector2(anchorX1, 1);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.blocksRaycasts = true; group.interactable = true;

            BuildPuffyButtonShell(rt, tokens, souffleMint, souffleDeep, radius: 28, floorPx: 8,
                out var faceRt, out var fillImg);

            // Zen icon (3 concentric circles representing a meditation lotus).
            BuildZenIcon(faceRt, tokens);

            // Main label "SOUFFLE".
            var lbl = BuildBigLabel(faceRt, tokens, "SOUFFLE", topMargin: 14, bottomMargin: 14);

            var view = btn.GetComponent<SouffleButtonView>();
            view.Group = group;
            view.Background = fillImg;
            view.Label = lbl;
            view.Root = rt;
        }

        /// <summary>Zen lotus icon = white circle with smaller darker centered circle.</summary>
        private static void BuildZenIcon(Transform parent, DesignTokens tokens)
        {
            var ic = new GameObject("Zen", typeof(RectTransform), typeof(Image));
            ic.transform.SetParent(parent, false);
            var rt = (RectTransform)ic.transform;
            rt.anchorMin = new Vector2(0.06f, 0.5f); rt.anchorMax = new Vector2(0.06f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(28, 0);
            rt.sizeDelta = new Vector2(48, 48);
            var img = ic.GetComponent<Image>();
            img.sprite = PuffySprite.RoundedFill(24);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = false;

            // Outline.
            var ol = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            ol.transform.SetParent(ic.transform, false);
            var olRt = (RectTransform)ol.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = ol.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(24, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Inner dot.
            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(ic.transform, false);
            var innerRt = (RectTransform)inner.transform;
            innerRt.anchorMin = new Vector2(0.5f, 0.5f); innerRt.anchorMax = new Vector2(0.5f, 0.5f);
            innerRt.pivot = new Vector2(0.5f, 0.5f);
            innerRt.anchoredPosition = Vector2.zero;
            innerRt.sizeDelta = new Vector2(20, 20);
            var innerImg = inner.GetComponent<Image>();
            innerImg.sprite = PuffySprite.RoundedFill(10);
            innerImg.type = Image.Type.Sliced;
            innerImg.color = tokens.navyContour;
            innerImg.raycastTarget = false;
        }

        // ====================================================================================
        //  AFFRONTER MAÎTRE — gold chip, visible when slots ≥ 1.
        // ====================================================================================

        private static void BuildAffronterMaitreButton(RectTransform parent, AffronterMaitreModal modal)
        {
            if (parent == null) return;
            var tokens = DesignTokens.Get();

            var btn = new GameObject("AffronterMaitreButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AffronterMaitreButtonView));
            btn.transform.SetParent(parent, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 32);
            rt.sizeDelta = new Vector2(280, 44);

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false;

            BuildPuffyButtonShell(rt, tokens, tokens.accentPrimary, DesignTokens.Darken(tokens.accentPrimary, 0.28f),
                radius: 22, floorPx: 6, out var faceRt, out var fillImg);

            var lbl = BuildBigLabel(faceRt, tokens, "AFFRONTER UN MAÎTRE", topMargin: 4, bottomMargin: 4);
            lbl.fontSize = 18;

            var view = btn.GetComponent<AffronterMaitreButtonView>();
            view.Group = group;
            view.Background = fillImg;
            view.Label = lbl;
            view.Root = rt;
            view.Modal = modal;
        }

        // ====================================================================================
        //  HELPERS — puffy shell (floor + face + outline + gloss) for skill buttons.
        // ====================================================================================

        /// <summary>
        /// Shared shell : floor (darker bottom offset) + face (colored fill + charcoal outline + gloss).
        /// Returns the face RectTransform (parent for icon/label/etc.) and the main fill Image.
        /// </summary>
        private static void BuildPuffyButtonShell(RectTransform host, DesignTokens tokens,
            Color face, Color floor, int radius, int floorPx,
            out RectTransform faceRt, out Image fillImg)
        {
            // Floor (darker bottom border).
            var floorGo = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floorGo.transform.SetParent(host, false);
            var floorRt = (RectTransform)floorGo.transform;
            floorRt.anchorMin = Vector2.zero; floorRt.anchorMax = Vector2.one;
            floorRt.offsetMin = new Vector2(0, -floorPx); floorRt.offsetMax = Vector2.zero;
            var floorImg = floorGo.GetComponent<Image>();
            floorImg.sprite = PuffySprite.RoundedFill(radius);
            floorImg.type = Image.Type.Sliced;
            floorImg.color = floor;
            floorImg.raycastTarget = false;

            // Face (the visible colored top).
            var faceGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
            faceGo.transform.SetParent(host, false);
            faceRt = (RectTransform)faceGo.transform;
            faceRt.anchorMin = Vector2.zero; faceRt.anchorMax = Vector2.one;
            faceRt.offsetMin = Vector2.zero; faceRt.offsetMax = Vector2.zero;
            fillImg = faceGo.GetComponent<Image>();
            fillImg.sprite = PuffySprite.RoundedFill(radius);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = face;
            // Note: button view handles raycast (via its IPointerClickHandler on parent host)
            // — face image is decorative, no need to block raycasts here.
            fillImg.raycastTarget = false;

            // Outline.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(faceRt, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, 3);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Gloss bar top.
            var gloss = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            gloss.transform.SetParent(faceRt, false);
            var glRt = (RectTransform)gloss.transform;
            glRt.anchorMin = new Vector2(0.08f, 0.62f); glRt.anchorMax = new Vector2(0.55f, 0.92f);
            glRt.offsetMin = Vector2.zero; glRt.offsetMax = Vector2.zero;
            var glImg = gloss.GetComponent<Image>();
            glImg.sprite = PuffySprite.Gloss();
            glImg.color = new Color(1f, 1f, 1f, 0.42f);
            glImg.raycastTarget = false;
            ((RectTransform)gloss.transform).localRotation = Quaternion.Euler(0, 0, -10f);

            // Add a raycast catcher on the host so IPointerClickHandler fires (the views need
            // a Graphic somewhere in the host to receive pointer events). We use a transparent
            // Image on the host. This requires the host to NOT already have an Image — we add one
            // if missing, otherwise rely on the view's existing one.
            if (host.gameObject.GetComponent<Image>() == null)
            {
                var raycast = host.gameObject.AddComponent<Image>();
                raycast.color = new Color(0, 0, 0, 0);
                raycast.raycastTarget = true;
            }
        }

        /// <summary>Big puffy label centered in the face : Lilita One white outlined charcoal.</summary>
        private static TextMeshProUGUI BuildBigLabel(Transform parent, DesignTokens tokens, string text,
            int topMargin, int bottomMargin)
        {
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(parent, false);
            var rt = (RectTransform)lblGo.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0, bottomMargin); rt.offsetMax = new Vector2(0, -topMargin);

            var tmp = lblGo.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 38;
            tmp.color = Color.white;
            tmp.text = text;
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.28f;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }
    }
}
