using Saga.Data;
using Saga.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.6 zone 2 — Stage band : chip "Stade N" + boss progress bar + boss skull marker.
    /// All RhosGFX sprites tinted DA palette. Pas de PuffySprite procédural.
    ///
    /// Layout :
    ///   [Chip 240×60 darkbrown w/ skull left + "Stade N" label]   [Bar stretched Red mint fill]   [Skull 48×48 right]
    /// </summary>
    public static class StageBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            var tokens = ctx.Tokens;
            if (ctx.Canvas == null || tokens == null) return;
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            var catalog = RhosGFXAssetCatalog.Get();

            // Phase 4 wireframe SAGA §2 — Stage band 96 px haut (5 %), juste sous Top HUD (154 px).
            // 32 px lat inset (sizeDelta.x = -64). Horizontal layout : chip 240 + bar stretch + skull 48.
            var band = new GameObject("StageBand", typeof(RectTransform));
            band.transform.SetParent(parent, false);
            var bandRt = (RectTransform)band.transform;
            bandRt.anchorMin = new Vector2(0, 1); bandRt.anchorMax = new Vector2(1, 1);
            bandRt.pivot = new Vector2(0.5f, 1f);
            bandRt.anchoredPosition = new Vector2(0, -154);
            bandRt.sizeDelta = new Vector2(-64, 96);

            BuildStageChip(bandRt, tokens, catalog);
            BuildBossProgressBar(bandRt, tokens, catalog);
            BuildBossSkullRight(bandRt, tokens, catalog);
        }

        // ====================================================================================
        //  STAGE CHIP — RhosGFX container 3D darkbrown tint + skull icon left + "Stade N" label.
        // ====================================================================================

        private static void BuildStageChip(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var chip = new GameObject("StageChip", typeof(RectTransform), typeof(Image), typeof(StagePillView));
            chip.transform.SetParent(parent, false);
            var rt = (RectTransform)chip.transform;
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(240, 60);

            var bg = chip.GetComponent<Image>();
            bg.sprite = catalog.container3DBrown != null ? catalog.container3DBrown : catalog.container3DYellow;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;
            bg.color = tokens.boisDojoDark;
            bg.raycastTarget = false;

            // Skull icon left (RhosGFX), tinted Rouge DA coral.
            var skull = BuildSkullIcon(chip.transform, tokens, catalog, diameter: 44);
            var skullRt = (RectTransform)skull.transform;
            skullRt.anchorMin = new Vector2(0, 0.5f); skullRt.anchorMax = new Vector2(0, 0.5f);
            skullRt.pivot = new Vector2(0.5f, 0.5f);
            skullRt.anchoredPosition = new Vector2(28, 4);

            // "Stade N" label, Lilita One bold 36, crème.
            var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(chip.transform, false);
            var lblRt = (RectTransform)lbl.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(58, 4); lblRt.offsetMax = new Vector2(-16, 4);
            var lblTmp = lbl.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.font = tokens.DisplayFont;
            lblTmp.fontSize = 32;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = tokens.cremeText;
            lblTmp.text = "Stade 1";
            lblTmp.outlineColor = tokens.navyContour;
            lblTmp.outlineWidth = 0.25f;
            lblTmp.raycastTarget = false;
            lblTmp.textWrappingMode = TextWrappingModes.NoWrap;

            var view = chip.GetComponent<StagePillView>();
            view.Label = lblTmp;
        }

        // ====================================================================================
        //  BOSS PROGRESS BAR — RhosGFX Regular Red container + fill (Image.Filled horizontal).
        // ====================================================================================

        private static void BuildBossProgressBar(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var root = new GameObject("StageBossBar",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AdversaireProgressBarView));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            // Bar stretches between chip (240 + 24 gap = 264) and skull right (48 + 16 gap = 64).
            rt.offsetMin = new Vector2(264, -18); rt.offsetMax = new Vector2(-64, 18);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.interactable = false; group.blocksRaycasts = false;

            // Container (track) — RhosGFX progress-container-regular-red-regular.
            var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(rt, false);
            var trackRt = (RectTransform)track.transform;
            trackRt.anchorMin = Vector2.zero; trackRt.anchorMax = Vector2.one;
            trackRt.offsetMin = Vector2.zero; trackRt.offsetMax = Vector2.zero;
            var trackImg = track.GetComponent<Image>();
            trackImg.sprite = catalog.barRegularRedContainer;
            trackImg.type = Image.Type.Simple;
            trackImg.preserveAspect = false;
            trackImg.color = Color.white; // sprite already red — keep native tint
            trackImg.raycastTarget = false;

            // Fill — RhosGFX progress-bar-regular-red-regular, Image.Filled horizontal driven by view.
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rt, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            // Inset 4px du container pour respirer (le sprite container a une bordure).
            fillRt.offsetMin = new Vector2(6, 6); fillRt.offsetMax = new Vector2(-6, -6);
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = catalog.barRegularRedFill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.preserveAspect = false;
            // Override la teinte rouge sprite par Mint DA (positive progress feel) — peut être ajusté.
            fillImg.color = tokens.mintPositif;
            fillImg.raycastTarget = false;

            // Hidden label (the view requires a non-null label — invisible tiny one).
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
        //  BOSS SKULL RIGHT — RhosGFX Skull icon at right edge of stage band.
        // ====================================================================================

        private static void BuildBossSkullRight(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var skull = BuildSkullIcon(parent, tokens, catalog, diameter: 56);
            var rt = (RectTransform)skull.transform;
            rt.anchorMin = new Vector2(1f, 0.5f); rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 4);
        }

        // ====================================================================================
        //  HELPER — RhosGFX Skull icon tinted Coral.
        // ====================================================================================

        private static GameObject BuildSkullIcon(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog, int diameter)
        {
            var icon = new GameObject("Skull", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var rt = (RectTransform)icon.transform;
            rt.sizeDelta = new Vector2(diameter, diameter);

            var img = icon.GetComponent<Image>();
            img.sprite = catalog.iconSkull;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.color = tokens.coralAction; // rouge cartoony DA #FF6B6B
            img.raycastTarget = false;
            return icon;
        }
    }
}
