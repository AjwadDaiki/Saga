using Saga.Data;
using Saga.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte zone 3 — stage chip + boss progress bar.
    ///
    /// Mockup (saga_target_spec.md §2) :
    ///   - Pill "Stade N" charcoal anthracite + crâne rouge débordant à gauche
    ///   - Barre progression verte glossy avec crâne rouge marker à droite
    ///
    /// Le bar reuse <see cref="AdversaireProgressBarView"/> (Sprint 4 component que SaveService déjà
    /// alimente via GameEvents) — on le re-skin puffy 3D au lieu de l'ambre flat précédent.
    /// </summary>
    public static class StageBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            var tokens = ctx.Tokens;
            if (ctx.Canvas == null || tokens == null) return;
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;

            // Phase 4 wireframe SAGA §2 — Stage band 96 px haut (5 %), juste sous Top HUD (154 px).
            // 32 px lat inset (sizeDelta.x = -64). Horizontal layout : chip 240 + gap 16 + bar stretch + gap 16 + skull 48.
            var band = new GameObject("StageBand", typeof(RectTransform));
            band.transform.SetParent(parent, false);
            var bandRt = (RectTransform)band.transform;
            bandRt.anchorMin = new Vector2(0, 1); bandRt.anchorMax = new Vector2(1, 1);
            bandRt.pivot = new Vector2(0.5f, 1f);
            bandRt.anchoredPosition = new Vector2(0, -154);
            bandRt.sizeDelta = new Vector2(-64, 96);

            BuildStageChip(bandRt, tokens);
            BuildBossProgressBar(bandRt, tokens);
            BuildBossSkullRight(bandRt, tokens);
        }

        // ====================================================================================
        //  STAGE CHIP — pill charcoal + red skull icon + "Stade N" label.
        // ====================================================================================

        private static void BuildStageChip(Transform parent, DesignTokens tokens)
        {
            var chip = new GameObject("StageChip", typeof(RectTransform), typeof(Image), typeof(StagePillView));
            chip.transform.SetParent(parent, false);
            var rt = (RectTransform)chip.transform;
            // Phase 4 §2 — chip anchored top-left de la band, centré vertical (pivot.y=0.5).
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(240, 60);

            var radius = Mathf.RoundToInt(rt.sizeDelta.y / 2f);
            var bg = chip.GetComponent<Image>();
            bg.sprite = PuffySprite.RoundedFill(radius);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.11f, 0.13f, 0.14f, 0.95f);     // anthracite per spec
            bg.raycastTarget = false;

            // Floor (puffy depth).
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(chip.transform, false);
            floor.transform.SetAsFirstSibling();
            var frt = (RectTransform)floor.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(0, -5); frt.offsetMax = Vector2.zero;
            var fi = floor.GetComponent<Image>();
            fi.sprite = PuffySprite.RoundedFill(radius);
            fi.type = Image.Type.Sliced;
            fi.color = tokens.panelSombre2;
            fi.raycastTarget = false;

            // Charcoal outline.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(chip.transform, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, 3);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Phase 4 §2 — Skull 48 × 48 débordant à gauche du chip.
            var skull = BuildSkullIcon(chip.transform, tokens, diameter: 48);
            var skullRt = (RectTransform)skull.transform;
            skullRt.anchorMin = new Vector2(0, 0.5f); skullRt.anchorMax = new Vector2(0, 0.5f);
            skullRt.pivot = new Vector2(0.5f, 0.5f);
            skullRt.anchoredPosition = new Vector2(8, 0);

            // Phase 4 §2 — "Stade N" Lilita One bold 36 px à droite du skull.
            var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(chip.transform, false);
            var lblRt = (RectTransform)lbl.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(56, 0); lblRt.offsetMax = new Vector2(-20, 0);
            var lblTmp = lbl.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.font = tokens.DisplayFont;
            lblTmp.fontSize = 36;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = Color.white;
            lblTmp.text = "Stade 1";
            lblTmp.outlineColor = tokens.navyContour;
            lblTmp.outlineWidth = 0.22f;
            lblTmp.raycastTarget = false;
            lblTmp.textWrappingMode = TextWrappingModes.NoWrap;

            var view = chip.GetComponent<StagePillView>();
            view.Label = lblTmp;
        }

        // ====================================================================================
        //  BOSS PROGRESS BAR — green glossy fill + red skull marker at right.
        // ====================================================================================

        private static void BuildBossProgressBar(Transform parent, DesignTokens tokens)
        {
            var root = new GameObject("StageBossBar",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AdversaireProgressBarView));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            // Phase 4 §2 — bar stretch H entre chip et skull droite. 24 px haut, centré vertical.
            // offsetMin.x = 256 = 240 (chip) + 16 (gap). offsetMax.x = -64 = -(48 skull + 16 gap).
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = new Vector2(256, -12); rt.offsetMax = new Vector2(-64, 12);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.interactable = false; group.blocksRaycasts = false;

            var radius = Mathf.RoundToInt(rt.sizeDelta.y / 2f);

            // Track (charcoal, puffy).
            var bg = new GameObject("Track", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(rt, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = PuffySprite.RoundedFill(radius);
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.10f, 0.12f, 0.13f, 0.95f);
            bgImg.raycastTarget = false;

            // Track outline.
            var olGo = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            olGo.transform.SetParent(rt, false);
            var olRt = (RectTransform)olGo.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = olGo.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, 3);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Green glossy fill.
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rt, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -4);
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = PuffySprite.RoundedFill(radius - 4);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.color = tokens.mintPositif;                    // #69E6A3 vert vif DA §5.1
            fillImg.raycastTarget = false;

            // Top gloss bar inside fill (subtle white sheen across upper half).
            var gloss = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            gloss.transform.SetParent(fill.transform, false);
            var glRt = (RectTransform)gloss.transform;
            glRt.anchorMin = new Vector2(0.02f, 0.55f); glRt.anchorMax = new Vector2(0.98f, 0.92f);
            glRt.offsetMin = Vector2.zero; glRt.offsetMax = Vector2.zero;
            var glImg = gloss.GetComponent<Image>();
            glImg.sprite = PuffySprite.RoundedFill(radius - 6);
            glImg.type = Image.Type.Sliced;
            glImg.color = new Color(1f, 1f, 1f, 0.22f);
            glImg.raycastTarget = false;

            // Phase 4 §2 — skull boss déplacé hors du bar (élément séparé via BuildBossSkullRight).

            // Hidden label (the view requires a non-null label — we use a tiny invisible one).
            var hiddenLbl = new GameObject("HiddenLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            hiddenLbl.transform.SetParent(rt, false);
            var hLblRt = (RectTransform)hiddenLbl.transform;
            hLblRt.anchorMin = Vector2.zero; hLblRt.anchorMax = Vector2.zero;
            hLblRt.sizeDelta = new Vector2(0, 0);
            var hLblTmp = hiddenLbl.GetComponent<TextMeshProUGUI>();
            hLblTmp.fontSize = 1;
            hLblTmp.color = new Color(0, 0, 0, 0);
            hLblTmp.raycastTarget = false;

            var view = root.GetComponent<AdversaireProgressBarView>();
            view.Group = group;
            view.FillImage = fillImg;
            view.Label = hLblTmp;
            view.PulseTarget = rt;
        }

        // ====================================================================================
        //  BOSS SKULL RIGHT — 48 × 48 marker at right edge of stage band (Phase 4 §2).
        // ====================================================================================

        private static void BuildBossSkullRight(Transform parent, DesignTokens tokens)
        {
            var skull = BuildSkullIcon(parent, tokens, diameter: 48);
            var rt = (RectTransform)skull.transform;
            rt.anchorMin = new Vector2(1f, 0.5f); rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        // ====================================================================================
        //  SKULL ICON — small red circle + charcoal outline + 2 dark eye dots.
        //  Procedural so we don't depend on Unicode 💀 / ☠ glyphs missing from Latin SDF atlases.
        // ====================================================================================

        private static GameObject BuildSkullIcon(Transform parent, DesignTokens tokens, int diameter)
        {
            var radius = diameter / 2;
            var icon = new GameObject("Skull", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var rt = (RectTransform)icon.transform;
            rt.sizeDelta = new Vector2(diameter, diameter);

            var img = icon.GetComponent<Image>();
            img.sprite = PuffySprite.RoundedFill(radius);
            img.type = Image.Type.Sliced;
            img.color = tokens.accentDanger;            // red puffy boss color
            img.raycastTarget = false;

            // Charcoal outline.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(icon.transform, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // 2 eye dots.
            for (var x = 0; x < 2; x++)
            {
                var eye = new GameObject($"Eye{x}", typeof(RectTransform), typeof(Image));
                eye.transform.SetParent(icon.transform, false);
                var ert = (RectTransform)eye.transform;
                ert.anchorMin = new Vector2(0.5f, 0.55f);
                ert.anchorMax = new Vector2(0.5f, 0.55f);
                ert.pivot = new Vector2(0.5f, 0.5f);
                ert.anchoredPosition = new Vector2(x == 0 ? -diameter * 0.18f : diameter * 0.18f, 0);
                ert.sizeDelta = new Vector2(diameter * 0.18f, diameter * 0.22f);
                var eimg = eye.GetComponent<Image>();
                eimg.sprite = PuffySprite.RoundedFill(3);
                eimg.type = Image.Type.Sliced;
                eimg.color = tokens.navyContour;
                eimg.raycastTarget = false;
            }

            return icon;
        }
    }
}
