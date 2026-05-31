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
            // Sprint 9 Phase 3 dual-mode dispatch.
            if (ctx.DesignerFirstActive && ctx.Registry?.StageChip != null)
            {
                WireFromAuthored(ctx);
                return;
            }
            BuildProcedural(ctx);
        }

        private static void BuildProcedural(BuilderContext ctx)
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
        //  SPRINT 9 Phase 3 — Designer-First wiring (attache views aux GO Ajwad authored).
        // ====================================================================================

        private static void WireFromAuthored(BuilderContext ctx)
        {
            var tokens = ctx.Tokens;
            var registry = ctx.Registry;
            if (registry.StageChip != null) WireStageChip(registry.StageChip, tokens);
            if (registry.BossBar != null) WireBossBar(registry.BossBar, tokens);
            // BossSkull : pulse léger sur changement combat phase (Sprint 10A si demandé).
            // Pour V1, on laisse statique (l'asset Ajwad est visible).
        }

        /// <summary>
        /// Stage_Chip : auto-create Label child (Lilita Bold) + StagePillView qui subscribe
        /// OnStadeChanged. Préserve Shadow customs + sprite background authored.
        /// </summary>
        private static void WireStageChip(GameObject go, DesignTokens tokens)
        {
            if (go.GetComponent<StagePillView>() != null) return;
            var label = EnsureLabel(go.transform, tokens, color: tokens.navyContour, fontSize: 36);
            var view = go.AddComponent<StagePillView>();
            view.Label = label;
            Debug.Log("[Stage] Wired Stage_Chip — live label active (Lilita 36sp).");
        }

        /// <summary>
        /// Boss_Bar : trouve ou auto-create Fill child (Image.Type.Filled horizontal) +
        /// AdversaireProgressBarView qui drive fillAmount via OnAdversaireDamaged.
        /// Le sprite background authored reste intact, le Fill enfant overlay anime.
        /// </summary>
        private static void WireBossBar(GameObject go, DesignTokens tokens)
        {
            if (go.GetComponent<AdversaireProgressBarView>() != null) return;

            // Ensure CanvasGroup (view shows/hides via group.alpha).
            var group = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            group.alpha = 1f; group.interactable = false; group.blocksRaycasts = false;

            // Trouve ou auto-create Fill child.
            var fillTransform = go.transform.Find("Fill");
            Image fillImg;
            if (fillTransform != null)
            {
                fillImg = fillTransform.GetComponent<Image>() ?? fillTransform.gameObject.AddComponent<Image>();
                Debug.Log("[Stage] Boss_Bar — Found existing Fill child, wiring Image.Filled.");
            }
            else
            {
                var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillGo.transform.SetParent(go.transform, false);
                var fillRt = (RectTransform)fillGo.transform;
                fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = new Vector2(8, 8); fillRt.offsetMax = new Vector2(-8, -8);
                fillImg = fillGo.GetComponent<Image>();
                Debug.Log("[Stage] Boss_Bar — Auto-created Fill child (inset 8px du sprite background).");
            }

            // Configure pour Image.Filled horizontal driven par view.
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.color = tokens.mintPositif; // vert progression positive
            fillImg.raycastTarget = false;

            // Hidden label requis par view (mais on n'affiche rien).
            var hiddenLbl = new GameObject("HiddenLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            hiddenLbl.transform.SetParent(go.transform, false);
            var hRt = (RectTransform)hiddenLbl.transform;
            hRt.sizeDelta = Vector2.zero;
            var hTmp = hiddenLbl.GetComponent<TextMeshProUGUI>();
            hTmp.fontSize = 1; hTmp.color = new Color(0, 0, 0, 0); hTmp.raycastTarget = false;

            var view = go.AddComponent<AdversaireProgressBarView>();
            view.Group = group;
            view.FillImage = fillImg;
            view.Label = hTmp;
            view.PulseTarget = go.transform as RectTransform;
            Debug.Log("[Stage] Wired Boss_Bar — fillAmount driven by AdversaireProgressBarView.");
        }

        /// <summary>Find or create "Label" TMP child. Lilita One Bold + outline cremeText 0.22 + tracking 6.</summary>
        private static TextMeshProUGUI EnsureLabel(Transform parent, DesignTokens tokens, Color color, int fontSize)
        {
            var existing = parent.Find("Label");
            if (existing != null)
            {
                var tmp = existing.GetComponent<TextMeshProUGUI>();
                if (tmp != null) return tmp;
                tmp = existing.gameObject.AddComponent<TextMeshProUGUI>();
                ConfigureLabel(tmp, tokens, color, fontSize);
                return tmp;
            }
            var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var newTmp = go.GetComponent<TextMeshProUGUI>();
            ConfigureLabel(newTmp, tokens, color, fontSize);
            return newTmp;
        }

        private static void ConfigureLabel(TextMeshProUGUI tmp, DesignTokens tokens, Color color, int fontSize)
        {
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = tokens.DisplayFont; // Lilita One SAGA
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.enableAutoSizing = false;
            tmp.outlineColor = tokens.cremeText;
            tmp.outlineWidth = 0.22f;
            tmp.characterSpacing = 6f;
            tmp.text = "Stade 1";
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
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
            rt.sizeDelta = new Vector2(260, 68);

            // P3 fix : sprite pill horizontal (button-round-3d-2.5-yellow 160×64 ratio 2.5:1) +
            // Image.Type.Sliced avec borders 9-slice du postprocessor → forme capsule nette,
            // corners ronds préservés, center stretch propre. Tint Or DA jauneReward (contraste fort).
            var bg = chip.GetComponent<Image>();
            bg.sprite = catalog.round3DYellow25.standard;
            bg.type = Image.Type.Sliced;
            bg.preserveAspect = false;
            bg.color = tokens.jauneReward;
            bg.raycastTarget = false;

            // Skull icon left (RhosGFX), tinted Coral.
            var skull = BuildSkullIcon(chip.transform, tokens, catalog, diameter: 40);
            var skullRt = (RectTransform)skull.transform;
            skullRt.anchorMin = new Vector2(0, 0.5f); skullRt.anchorMax = new Vector2(0, 0.5f);
            skullRt.pivot = new Vector2(0.5f, 0.5f);
            skullRt.anchoredPosition = new Vector2(28, 2);

            // "Stade N" label, Lilita Bold 30, encre sur Or (contraste optimal).
            var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(chip.transform, false);
            var lblRt = (RectTransform)lbl.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(56, 2); lblRt.offsetMax = new Vector2(-16, 2);
            var lblTmp = lbl.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.font = tokens.DisplayFont;
            lblTmp.fontSize = 30;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = tokens.navyContour;
            lblTmp.text = "Stade 1";
            lblTmp.outlineColor = tokens.cremeText;
            lblTmp.outlineWidth = 0.22f;
            lblTmp.characterSpacing = 6f;
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
            rt.offsetMin = new Vector2(284, -16); rt.offsetMax = new Vector2(-56, 16);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.interactable = false; group.blocksRaycasts = false;

            // Container (track) — Sliced 9-slice (borders 8,8,8,8 du postprocessor) → capsule
            // horizontale nette, corners ronds préservés à toute largeur.
            var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(rt, false);
            var trackRt = (RectTransform)track.transform;
            trackRt.anchorMin = Vector2.zero; trackRt.anchorMax = Vector2.one;
            trackRt.offsetMin = Vector2.zero; trackRt.offsetMax = Vector2.zero;
            var trackImg = track.GetComponent<Image>();
            trackImg.sprite = catalog.barRegularRedContainer;
            trackImg.type = Image.Type.Sliced;
            trackImg.preserveAspect = false;
            trackImg.color = Color.white; // sprite already red — keep native tint
            trackImg.raycastTarget = false;

            // Fill — Sliced (borders 4,4,4,4) Filled Horizontal driven par AdversaireProgressBarView.
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rt, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(6, 6); fillRt.offsetMax = new Vector2(-6, -6);
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = catalog.barRegularRedFill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
            fillImg.preserveAspect = false;
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
