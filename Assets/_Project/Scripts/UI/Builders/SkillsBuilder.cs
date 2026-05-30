using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.6 zone 5 — Skills row : VAGUE (cyan, Élan jauge interne) + SOUFFLE (vert menthe).
    /// RhosGFX 3D Square buttons tinted palette DA + idle breathing + ready glow halo.
    ///
    /// SOUFFLE 280×180 anchored left   |   VAGUE 680×180 anchored right
    /// </summary>
    public static class SkillsBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            var tokens = ctx.Tokens;
            if (ctx.Canvas == null || tokens == null) return;
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            var catalog = RhosGFXAssetCatalog.Get();

            ctx.ElanRow = BuildSkillsRow(parent, tokens, catalog);
            BuildAffronterMaitreButton(ctx.ElanRow, modal: null, tokens, catalog);
        }

        public static void BindAffronterMaitre(BuilderContext ctx, AffronterMaitreModal modal)
        {
            if (ctx.ElanRow == null || modal == null) return;
            var btn = ctx.ElanRow.Find("AffronterMaitreButton");
            if (btn == null) return;
            var view = btn.GetComponent<AffronterMaitreButtonView>();
            if (view != null) view.Modal = modal;
        }

        private static RectTransform BuildSkillsRow(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var row = new GameObject("SkillsRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rt = (RectTransform)row.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 192);
            rt.sizeDelta = new Vector2(-64, 230);

            BuildSouffleButton(rt, tokens, catalog);
            BuildVagueButton(rt, tokens, catalog);
            return rt;
        }

        // ====================================================================================
        //  VAGUE — RhosGFX square 3D Blue tinted Sky DA + lightning + Élan gauge + ready glow
        // ====================================================================================

        private static void BuildVagueButton(RectTransform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            // P5 fix : VAGUE 680→620 (évite débordement sur SOUFFLE 280 + gap, total 920 < 1016 row).
            var btn = new GameObject("VagueButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(VagueButtonView));
            btn.transform.SetParent(parent, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(620, 180);

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.blocksRaycasts = true; group.interactable = true;

            // P6 fix : halo en PREMIER sibling + Face Image en CHILD → halo derrière, pas
            // d'empilement face×halo qui créait l'effet "ombre+contour doublonné". Face est
            // un child séparé avec Image dédiée + raycastTarget.
            var glow = BuildHalo(rt, catalog.square3DBlue25.standard, tokens.jauneReward);

            var faceGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
            faceGo.transform.SetParent(rt, false);
            var faceRt = (RectTransform)faceGo.transform;
            faceRt.anchorMin = Vector2.zero; faceRt.anchorMax = Vector2.one;
            faceRt.offsetMin = Vector2.zero; faceRt.offsetMax = Vector2.zero;
            var face = faceGo.GetComponent<Image>();
            face.sprite = catalog.square3DBlue25.standard;
            face.type = Image.Type.Sliced; // 9-slice borders 20,14,20,14 → corners ronds nets
            face.preserveAspect = false;
            face.color = tokens.skyBlue;
            face.raycastTarget = true;

            // Lightning bolt icon (procedural — pack has no lightning).
            BuildLightningBolt(faceRt, tokens);

            // Main label "VAGUE" (centered top).
            var lbl = BuildBigLabel(faceRt, tokens, "VAGUE", topMargin: 16, bottomMargin: 60);

            // Élan inner gauge bottom.
            BuildElanGauge(faceRt, tokens, catalog, out var gaugeFill, out var gaugeLabel, out var gaugeGlow);

            var view = btn.GetComponent<VagueButtonView>();
            view.Group = group;
            view.Background = face;
            view.Label = lbl;
            view.Root = rt;
            view.AlwaysVisible = true;
            rt.localScale = Vector3.one;
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = false; // gated true again on Élan = max via the view

            // ElanBarView on the same GO drives the inner gauge.
            var elanView = btn.AddComponent<ElanBarView>();
            elanView.FillImage = gaugeFill;
            elanView.Label = gaugeLabel;
            elanView.PulseTarget = rt;
            elanView.GlowImage = gaugeGlow;

            // ReadyGlowView wired to the halo (driven externally — for now subscribe to Élan via
            // a tiny ad-hoc bridge component listening to GameEvents.OnElanChanged threshold ≥ 1.0).
            var ready = btn.AddComponent<ReadyGlowView>();
            ready.Glow = glow;
            btn.AddComponent<VagueReadyGlowBridge>().Setup(ready);

            // Breathing pulse subtle on the button.
            var pulse = btn.AddComponent<BreathingPulseView>();
            pulse.Target = rt;
            pulse.Peak = 1.018f;
            pulse.Period = 2.6f;
        }

        /// <summary>Lightning bolt = 2 rotated rectangles (procedural — pack has no lightning).</summary>
        private static void BuildLightningBolt(Transform parent, DesignTokens tokens)
        {
            var ic = new GameObject("Lightning", typeof(RectTransform));
            ic.transform.SetParent(parent, false);
            var rt = (RectTransform)ic.transform;
            rt.anchorMin = new Vector2(0.06f, 0.5f); rt.anchorMax = new Vector2(0.06f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(40, 12);
            rt.sizeDelta = new Vector2(56, 80);

            var top = new GameObject("Top", typeof(RectTransform), typeof(Image));
            top.transform.SetParent(rt, false);
            var trt = (RectTransform)top.transform;
            trt.anchorMin = new Vector2(0.5f, 0.65f); trt.anchorMax = new Vector2(0.5f, 0.65f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(-4, 0);
            trt.sizeDelta = new Vector2(36, 12);
            trt.localEulerAngles = new Vector3(0, 0, -28f);
            var timg = top.GetComponent<Image>();
            timg.sprite = PuffySprite.RoundedFill(6); timg.type = Image.Type.Sliced;
            timg.color = tokens.jauneReward;
            timg.raycastTarget = false;

            var bot = new GameObject("Bot", typeof(RectTransform), typeof(Image));
            bot.transform.SetParent(rt, false);
            var brt = (RectTransform)bot.transform;
            brt.anchorMin = new Vector2(0.5f, 0.32f); brt.anchorMax = new Vector2(0.5f, 0.32f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(4, 0);
            brt.sizeDelta = new Vector2(36, 12);
            brt.localEulerAngles = new Vector3(0, 0, -28f);
            var bimg = bot.GetComponent<Image>();
            bimg.sprite = PuffySprite.RoundedFill(6); bimg.type = Image.Type.Sliced;
            bimg.color = tokens.jauneReward;
            bimg.raycastTarget = false;
        }

        // ====================================================================================
        //  Élan gauge inside VAGUE — RhosGFX Thin White bar overlay.
        // ====================================================================================

        private static void BuildElanGauge(RectTransform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog,
            out Image fillImg, out TextMeshProUGUI labelTmp, out Image glowImg)
        {
            var gauge = new GameObject("ElanGauge", typeof(RectTransform));
            gauge.transform.SetParent(parent, false);
            var rt = (RectTransform)gauge.transform;
            rt.anchorMin = new Vector2(0.08f, 0.08f); rt.anchorMax = new Vector2(0.92f, 0.30f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Glow halo behind (driven by ElanBarView.PulseTarget glow alpha).
            var glowGo = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glowGo.transform.SetParent(rt, false);
            var glowRt = (RectTransform)glowGo.transform;
            glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-8, -8); glowRt.offsetMax = new Vector2(8, 8);
            glowImg = glowGo.GetComponent<Image>();
            glowImg.sprite = catalog.barThinWhiteFill;
            glowImg.type = Image.Type.Simple;
            glowImg.preserveAspect = false;
            glowImg.color = new Color(tokens.jauneReward.r, tokens.jauneReward.g, tokens.jauneReward.b, 0f);
            glowImg.raycastTarget = false;

            // Track (cyan deep — bottom strip rounded).
            var bg = new GameObject("Track", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(rt, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = catalog.barThinWhiteFill;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
            bgImg.color = new Color(0.10f, 0.34f, 0.44f, 0.75f); // deep cyan empty channel
            bgImg.raycastTarget = false;

            // Fill (bright cyan) — Image.Filled horizontal.
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rt, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2, 2); fillRt.offsetMax = new Vector2(-2, -2);
            fillImg = fill.GetComponent<Image>();
            fillImg.sprite = catalog.barThinWhiteFill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.preserveAspect = false;
            fillImg.color = new Color(0.78f, 0.96f, 1f, 1f); // pale cyan glow
            fillImg.raycastTarget = false;

            // "78%" label centered.
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            labelTmp = lblGo.GetComponent<TextMeshProUGUI>();
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.font = tokens.NumbersFont;
            labelTmp.fontSize = 22;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.color = Color.white;
            labelTmp.text = "0%";
            labelTmp.outlineColor = tokens.navyContour;
            labelTmp.outlineWidth = 0.28f;
            labelTmp.raycastTarget = false;
        }

        // ====================================================================================
        //  SOUFFLE — RhosGFX square 3D Forest Green tinted Mint DA + Bell icon + breathing pulse.
        // ====================================================================================

        private static void BuildSouffleButton(RectTransform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var btn = new GameObject("SouffleButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(SouffleButtonView));
            btn.transform.SetParent(parent, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(280, 180);

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.blocksRaycasts = true; group.interactable = true;

            // P6 fix : Face en child séparé (cohérence avec VAGUE), Image.Type.Sliced (border
            // 20,14,20,14 du postprocessor) pour corners ronds nets.
            var faceGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
            faceGo.transform.SetParent(rt, false);
            var faceRt = (RectTransform)faceGo.transform;
            faceRt.anchorMin = Vector2.zero; faceRt.anchorMax = Vector2.one;
            faceRt.offsetMin = Vector2.zero; faceRt.offsetMax = Vector2.zero;
            var face = faceGo.GetComponent<Image>();
            face.sprite = catalog.square3DForestGreen25.standard;
            face.type = Image.Type.Sliced;
            face.preserveAspect = false;
            face.color = tokens.mintPositif;
            face.raycastTarget = true;

            // Bell icon (RhosGFX, méditation zen).
            var iconGo = new GameObject("Bell", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(faceRt, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0.5f, 1f); iconRt.anchorMax = new Vector2(0.5f, 1f);
            iconRt.pivot = new Vector2(0.5f, 1f);
            iconRt.anchoredPosition = new Vector2(0, -18);
            iconRt.sizeDelta = new Vector2(72, 72);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = catalog.iconBellOutline != null ? catalog.iconBellOutline : catalog.iconBell;
            iconImg.type = Image.Type.Simple;
            iconImg.preserveAspect = true;
            iconImg.color = tokens.cremeText;
            iconImg.raycastTarget = false;

            // Label "SOUFFLE" bottom.
            var lbl = BuildBigLabel(faceRt, tokens, "SOUFFLE", topMargin: 100, bottomMargin: 18);
            lbl.fontSize = 32;

            var view = btn.GetComponent<SouffleButtonView>();
            view.Group = group;
            view.Background = face;
            view.Label = lbl;
            view.Root = rt;

            var pulse = btn.AddComponent<BreathingPulseView>();
            pulse.Target = rt;
            pulse.Peak = 1.025f;
            pulse.Period = 2.5f;
        }

        // ====================================================================================
        //  AFFRONTER MAÎTRE — gold chip RhosGFX, visible when slots ≥ 1.
        // ====================================================================================

        private static void BuildAffronterMaitreButton(RectTransform parent, AffronterMaitreModal modal,
            DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            if (parent == null) return;

            var btn = new GameObject("AffronterMaitreButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AffronterMaitreButtonView));
            btn.transform.SetParent(parent, false);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 32);
            rt.sizeDelta = new Vector2(320, 56);

            var group = btn.GetComponent<CanvasGroup>();
            group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false;

            var faceGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
            faceGo.transform.SetParent(rt, false);
            var faceRt = (RectTransform)faceGo.transform;
            faceRt.anchorMin = Vector2.zero; faceRt.anchorMax = Vector2.one;
            faceRt.offsetMin = Vector2.zero; faceRt.offsetMax = Vector2.zero;
            var face = faceGo.GetComponent<Image>();
            face.sprite = catalog.round3DYellow25.standard;
            face.type = Image.Type.Sliced;
            face.preserveAspect = false;
            face.color = tokens.jauneReward;
            face.raycastTarget = true;

            var lbl = BuildBigLabel(faceRt, tokens, "AFFRONTER UN MAÎTRE", topMargin: 6, bottomMargin: 6);
            lbl.fontSize = 20;
            lbl.color = tokens.navyContour;

            var view = btn.GetComponent<AffronterMaitreButtonView>();
            view.Group = group;
            view.Background = face;
            view.Label = lbl;
            view.Root = rt;
            view.Modal = modal;
        }

        // ====================================================================================
        //  HELPERS
        // ====================================================================================

        private static Image BuildHalo(RectTransform parent, Sprite sprite, Color color)
        {
            var glow = new GameObject("ReadyHalo", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(parent, false);
            glow.transform.SetAsFirstSibling();
            var rt = (RectTransform)glow.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-14, -14); rt.offsetMax = new Vector2(14, 14);
            var img = glow.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = new Color(color.r, color.g, color.b, 0f);
            img.raycastTarget = false;
            return img;
        }

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
            tmp.fontSize = 48;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.text = text;
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.32f;
            tmp.characterSpacing = 6f; // P9 : tracking +6 sur titres Lilita
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }
    }
}
