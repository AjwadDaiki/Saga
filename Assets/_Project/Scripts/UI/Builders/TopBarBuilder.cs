using Saga.Core;
using Saga.Data;
using Saga.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.6 RhosGFX zone 1 — top bar : Force pill (or) + Échos pill (violet) + Settings rond.
    /// Boutons 3D Round RhosGFX (Cartoony UI Pack) tinted runtime palette DA. Pas de PuffySprite procédural.
    ///
    /// Force pill : button-round-3d-2.5-yellow-regular + Coin 2 Gold icon natif.
    /// Échos pill : button-round-3d-2.5-purple-regular tint Lavande + Gem Outline tint Lavande.
    /// Settings  : button-round-3d-1-grey-regular + Gear 2 Outline tint encre.
    /// </summary>
    public static class TopBarBuilder
    {
        // ---- Layout @ 1080×1920 reference (Phase 4 wireframe SAGA §1) -------------------
        // Top HUD band 154 px haut, 32 px lat inset, top = safeArea.top + 16.
        // Force pill 200×80, Échos pill 200×80, Settings 80×80 rond.
        private static readonly Vector2 PillSizeForce = new Vector2(200, 80);
        private static readonly Vector2 PillSizeEchos = new Vector2(200, 80);
        private static readonly Vector2 SettingsSize = new Vector2(80, 80);
        private const float TopY = -16f;     // 16 px sous le bord top du SafeAreaContainer
        private const float SidePad = 32f;
        private const float GapPills = 28f;  // Sprint 7.6 M2-fix P6 : 16→28 (pills moins collés)

        // M2-fix P2/P3/P7 — sprite RhosGFX a une ombre baked-in en bas.
        // Settings (round 80×80) a une ombre proportionnellement plus grosse que les pills ovales
        // → lift plus marqué (8px vs 4px) confirmé par retour visuel Ajwad.
        private const float ShadowCompPill = 4f;
        private const float ShadowCompSettings = 8f;

        public static void Build(BuilderContext ctx)
        {
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            var catalog = RhosGFXAssetCatalog.Get();
            BuildForcePill(parent, ctx.Tokens, catalog);
            BuildEchosPill(parent, ctx.Tokens, catalog);
            BuildSettingsButton(parent, ctx.Tokens, catalog);
        }

        // ====================================================================================
        //  FORCE PILL — top-left, Coin 2 Gold icon, ForceCounterView drives the value label.
        // ====================================================================================

        private static void BuildForcePill(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            // Tint Or DA #FFD84D (jauneReward) sur sprite Yellow regular.
            var pill = BuildRhosPill(parent, "ForcePill",
                anchor: new Vector2(0, 1),
                pos: new Vector2(SidePad, TopY),
                size: PillSizeForce,
                bgSprite: catalog.round3DYellow25.standard,
                bgTint: tokens.jauneReward);

            var view = pill.AddComponent<ForceCounterView>();

            // E3 — Glow halo derrière le pill (alpha 0 idle, pulse Or sur big change).
            var glow = BuildGlowHalo(pill.transform, catalog.round3DYellow25.standard, tokens.jauneReward);
            view.Glow = glow;

            // M2-fix P1 : icon 56→48 + offsetX 32→38 (P15 : légèrement plus à droite, demande Ajwad).
            // M2-fix P2/P3 : offsetY = ShadowCompPill (lift 4px) compense l'ombre baked-in.
            var iconGo = BuildIconImage(pill.transform, "ForceIcon", catalog.iconCoinGold, Color.white, diameter: 48,
                anchor: new Vector2(0, 0.5f), offsetX: 38, offsetY: ShadowCompPill);
            // E4 — Shimmer permanent (subtle 4-6s flash sur le coin doré).
            AddShimmerOverlay(iconGo.transform);

            // Value label : encre sur fond or, font 32 + outline 0.35 (P5 : moins fin).
            // leftMargin suit l'icon shifté à droite (P15) : 66 → 72.
            var label = BuildValueLabel(pill.transform, tokens, leftMargin: 72, color: tokens.navyContour,
                liftY: ShadowCompPill);
            view.Compact = true;
            view.Label = label;
        }

        // ====================================================================================
        //  ÉCHOS PILL — left of settings, Gem Outline tinted Lavande.
        // ====================================================================================

        private static void BuildEchosPill(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var pill = BuildRhosPill(parent, "EchosPill",
                anchor: new Vector2(0, 1),
                pos: new Vector2(SidePad + PillSizeForce.x + GapPills, TopY),
                size: PillSizeEchos,
                bgSprite: catalog.round3DPurple25.standard,
                bgTint: tokens.lavandeUI);

            // M2-fix P1/P2/P3 + P15 : icon 48 + offsetX 38 dedans + lift 4px shadow comp.
            BuildIconImage(pill.transform, "EchosIcon",
                catalog.iconGemOutline != null ? catalog.iconGemOutline : catalog.iconGem,
                tokens.cremeText, diameter: 48,
                anchor: new Vector2(0, 0.5f), offsetX: 38, offsetY: ShadowCompPill);

            // Value label : blanc sur fond lavande, font 32 + outline plus épais (P5).
            // leftMargin légèrement augmenté pour suivre l'icon (38+24=62→72 environ).
            var label = BuildValueLabel(pill.transform, tokens, leftMargin: 72, color: Color.white,
                liftY: ShadowCompPill);
            var gm = GameManager.Instance;
            label.text = gm?.State != null ? Saga.Math.NumberFormatter.Format(gm.State.totalEchos) : "0";
        }

        // ====================================================================================
        //  SETTINGS — round puffy button top-right, Gear 2 Outline tinted encre.
        // ====================================================================================

        private static void BuildSettingsButton(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var btn = SagaButton.Create(parent, "SettingsButton", catalog.round3DGrey1,
                SagaButton.Variant.Standard, Color.white, label: "",
                labelSize: 22, labelColor: null, labelFont: null);
            // M2-fix P13 : sprite round-3d-1 a ratio ~5:6 (round + ombre baked-in) ; preserveAspect=true
            // force le sprite à garder son ratio dans le RectTransform 80×80 carré → bouton reste rond.
            if (btn.Fill != null) btn.Fill.preserveAspect = true;
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-SidePad, TopY);
            rt.sizeDelta = SettingsSize;

            // M2-fix P2 : gear icon lifted ShadowCompSettings pour rester centré sur la face.
            // M2-fix P1 : diameter 48→44 (icon dans bouton 80×80 avec marge confortable).
            BuildIconImage(btn.transform, "GearIcon", catalog.iconGearOutline, tokens.navyContour,
                diameter: 44, anchor: new Vector2(0.5f, 0.5f), offsetX: 0, offsetY: ShadowCompSettings);
        }

        // ====================================================================================
        //  HELPERS RhosGFX
        // ====================================================================================

        /// <summary>
        /// Pill RhosGFX = 1 Image (sprite 3D Round déjà puffy avec contour/gloss bakés).
        /// Pas de Floor procédural — le sprite contient déjà le relief.
        /// </summary>
        private static GameObject BuildRhosPill(Transform parent, string name, Vector2 anchor, Vector2 pos,
            Vector2 size, Sprite bgSprite, Color bgTint)
        {
            var pill = new GameObject(name, typeof(RectTransform), typeof(Image));
            pill.transform.SetParent(parent, false);
            var rt = (RectTransform)pill.transform;
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(anchor.x, anchor.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var bg = pill.GetComponent<Image>();
            bg.sprite = bgSprite;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;
            bg.color = bgTint;
            bg.raycastTarget = false;

            return pill;
        }

        /// <summary>
        /// E3 — Glow halo : Image clone du sprite pill, alpha 0 idle, anim par ForceCounterView.Bounce.
        /// Stretched 12px outside les bords du pill pour déborder légèrement → halo.
        /// </summary>
        private static Image BuildGlowHalo(Transform parent, Sprite sprite, Color color)
        {
            var glow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(parent, false);
            glow.transform.SetAsFirstSibling(); // sous le BG du pill
            var rt = (RectTransform)glow.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-12, -12); rt.offsetMax = new Vector2(12, 12);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var img = glow.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = new Color(color.r, color.g, color.b, 0f);
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// E4 — Shimmer overlay : élipse blanche douce posée sur l'icône, alpha 0 idle, flash
        /// toutes les 4-6s via ShimmerLoopView (random delay = pas robotique).
        /// </summary>
        private static void AddShimmerOverlay(Transform iconTransform)
        {
            var shineGo = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shineGo.transform.SetParent(iconTransform, false);
            var rt = (RectTransform)shineGo.transform;
            rt.anchorMin = new Vector2(0.1f, 0.55f); rt.anchorMax = new Vector2(0.55f, 0.92f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = shineGo.GetComponent<Image>();
            img.sprite = PuffySprite.Gloss();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = false;
            var loop = shineGo.AddComponent<ShimmerLoopView>();
            loop.Shine = img;
        }

        /// <summary>Single Image with RhosGFX icon sprite. Caller positions via anchor + offsetX/Y.</summary>
        private static GameObject BuildIconImage(Transform parent, string name, Sprite sprite, Color tint,
            int diameter, Vector2 anchor, float offsetX, float offsetY = 0f)
        {
            var icon = new GameObject(name, typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var rt = (RectTransform)icon.transform;
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(offsetX, offsetY);
            rt.sizeDelta = new Vector2(diameter, diameter);

            var img = icon.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.color = tint;
            img.raycastTarget = false;
            return icon;
        }

        /// <summary>
        /// JetBrains Mono Bold value label with charcoal outline (puffy text recipe).
        /// M2-fix P5 : fontSize 26→32, outlineWidth 0.2→0.35 pour matcher l'épaisseur cartoony RhosGFX.
        /// liftY décale tout le label vers le haut pour compenser l'ombre baked-in du sprite container.
        /// </summary>
        private static TextMeshProUGUI BuildValueLabel(Transform parent, DesignTokens tokens, int leftMargin,
            Color color, float liftY = 0f)
        {
            var val = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            val.transform.SetParent(parent, false);
            var rt = (RectTransform)val.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(leftMargin, liftY);
            rt.offsetMax = new Vector2(-14, liftY);

            var tmp = val.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.font = tokens.NumbersFont;
            // M2-fix P9 : auto-size pour que "12.3K" / "1.5M" ne déborde jamais verticalement.
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 22;
            tmp.fontSizeMax = 32;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.text = "0";
            tmp.raycastTarget = false;
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.35f;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }
    }
}
