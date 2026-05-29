using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Zone 5 — Élan bar + Vague button + Souffle side button + Affronter Maître button.
    /// SkillsBuilder.Build creates everything except the Affronter→Modal listener (the modal is
    /// built later by Bootstrap). Call <see cref="BindAffronterMaitre"/> once the modal exists.
    /// </summary>
    public static class SkillsBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            ctx.ElanRow = BuildElanRow(ctx.Canvas);
            BuildSouffleButton(ctx.Canvas);
            // Affronter Maître button is created here (placeholder modal=null) and bound later by
            // Bootstrap via BindAffronterMaitre once the modal exists.
            BuildAffronterMaitreButton(ctx.ElanRow, modal: null);
        }

        /// <summary>
        /// Wire the Affronter Maître button (built by <see cref="Build"/>) to the
        /// AffronterMaitreModal once the modal exists in the canvas.
        /// </summary>
        public static void BindAffronterMaitre(BuilderContext ctx, AffronterMaitreModal modal)
        {
            if (ctx.ElanRow == null || modal == null) return;
            var btn = ctx.ElanRow.Find("AffronterMaitreButton");
            if (btn == null) return;
            var view = btn.GetComponent<AffronterMaitreButtonView>();
            if (view != null) view.Modal = modal;
        }

        private static RectTransform BuildElanRow(Canvas canvas)
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

            // Glow halo behind the bar — picks up the accent color when fill > 70%.
            // Sprint 7.5 fix (BUG 3): tight + asymmetric padding so it never bleeds up into the
            // gameplay zone above the Élan row (extends sideways + down, barely up).
            var glowGo = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glowGo.transform.SetParent(barRt, false);
            var glowRt = (RectTransform)glowGo.transform;
            glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-8, -8); glowRt.offsetMax = new Vector2(8, 2);
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
            // Sprint 7.5 A5: warm accent_action CTA color (glow pulses its alpha up to 1.0 when ready).
            btnImg.color = new Color(tokens.accentAction.r, tokens.accentAction.g, tokens.accentAction.b, 0.7f);
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

        private static void BuildAffronterMaitreButton(RectTransform elanRow, AffronterMaitreModal modal)
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

        private static void BuildSouffleButton(Canvas canvas)
        {
            var tokens = DesignTokens.Get();
            var btn = new GameObject("SouffleButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(SouffleButtonView));
            btn.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)btn.transform;
            // Sprint 7.5 refonte : left side-rail mid-height (clear of the crowded top bar pills).
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(16, 64);
            rt.sizeDelta = new Vector2(132, 78);

            // Sprint 7.5 A5: "Special" look — surface_mid bg + a 4px voie-colored border frame.
            // SouffleButtonView uses IPointerClickHandler (not Button) so we can't SagaButton.Wrap it;
            // we replicate the Special visual manually. Souffle is voie-neutral → use accent_primary border.
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(rt, false);
            var borderRt = (RectTransform)border.transform;
            borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = new Vector2(-3, -3); borderRt.offsetMax = new Vector2(3, 3);
            var borderImg = border.GetComponent<Image>();
            borderImg.color = new Color(tokens.accentPrimary.r, tokens.accentPrimary.g, tokens.accentPrimary.b, 0.55f);
            borderImg.raycastTarget = false;
            border.transform.SetAsFirstSibling(); // behind the button fill

            var img = btn.GetComponent<Image>();
            img.color = tokens.surfaceMid;
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
    }
}
