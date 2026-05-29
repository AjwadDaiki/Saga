using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte zone 6 — Upgrade cards puffy 3D (saga_target_spec.md §5).
    ///
    /// 3 cards empilées verticalement dans la band 8-32% du canvas (au-dessus bottom nav, sous skills) :
    ///   - Body cream <c>#FFF6E0</c> + floor 5px darker + outline charcoal 3px (radius 16)
    ///   - Row interne : icon container cercle puffy 56px (épée/lotus/people procédural) +
    ///     titre Lilita One + level JetBrains Mono + desc Plus Jakarta +
    ///     cost pill vert puffy avec coin or
    ///   - Mapping voie : Frappe→rouge (Strike), Méditation→bleu (Focus), Disciple→orange (Power)
    ///
    /// Le wiring affordability + click reste assuré par <see cref="UpgradeCardView"/> ; on lui passe
    /// les bonnes <c>Image</c>/<c>TextMeshProUGUI</c> à animer via réflexion-free public setters bientôt,
    /// mais pour Sprint 7.5 on reconstruit la hiérarchie ici et on attache la view en mode "decorative"
    /// pour qu'elle continue à wiring le click + refresh.
    /// </summary>
    public static class UpgradesBuilder
    {
        // ---- Color palette per upgrade voie (mockup spec §5) ------------------------------
        // Card 1 Strike  : red/orange icon background (Frappe)
        // Card 2 Focus   : blue icon background (Méditation)
        // Card 3 Power   : orange/red icon background (Disciple)
        private static readonly Color StrikeIconBg = new Color(0.859f, 0.196f, 0.216f, 1f);   // #db3237 rouge vif
        private static readonly Color FocusIconBg  = new Color(0.282f, 0.612f, 0.937f, 1f);   // #489CEF bleu doux
        private static readonly Color PowerIconBg  = new Color(0.961f, 0.522f, 0.224f, 1f);   // #F58539 orange chaud

        // Cream body (mockup) — #FFF6E0
        private static readonly Color CreamBody = new Color(1.000f, 0.965f, 0.878f, 1f);
        // Cost pill vert puffy (DA §5.1 vertBouton/mintPositif — hex hardcodés ici car couleurs custom)
        private static readonly Color CostFace = new Color(0.173f, 0.796f, 0.298f, 1f);       // #2ccb4c
        private static readonly Color CostFloor = new Color(0.000f, 0.431f, 0.125f, 1f);      // #006e20
        // Coin or pour le coin pill
        private static readonly Color CoinGold = new Color(0.980f, 0.780f, 0.259f, 1f);
        private static readonly Color CoinGoldDeep = new Color(0.745f, 0.541f, 0.094f, 1f);

        public static void Build(BuilderContext ctx)
        {
            Debug.Log("[UPGRADES] Build called, parent: " + (ctx.UIRoot != null ? "UIRoot" : "Canvas"));
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            BuildUpgradePanel(parent, ctx.Tokens);
        }

        private static void BuildUpgradePanel(Transform parent, DesignTokens tokens)
        {
            var gm = GameManager.Instance;
            if (gm?.Content == null) return;
            var upgrades = gm.Content.AllUpgrades;
            if (upgrades == null || upgrades.Count == 0)
            {
                Debug.LogWarning("[UpgradesBuilder] No upgrades in ContentDatabase — run menu \"Saga > Sprint 2 > Generate Upgrade Assets\" then reload Play.");
                return;
            }

            // Sprint 7.5 Polish Phase 3 — DA §3.2 : Panel upgrade 22-28% de l'écran. Bottom nav prend
            // ~12 %, donc upgrade panel commence à 12-13 % et termine à 34-35 %. Skills row (34-44 %)
            // suit juste au-dessus, scène commence à 44 %.
            var panel = new GameObject("UpgradePanel", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.04f, 0.12f);
            panelRt.anchorMax = new Vector2(0.96f, 0.34f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var count = upgrades.Count;
            var frac = 1f / count;
            const float spacing = 8f;   // px of inner-spacing between cards
            for (var i = 0; i < count; i++)
            {
                BuildCard(panelRt, tokens, upgrades[i], i, frac, spacing);
            }
        }

        private static void BuildCard(RectTransform panel, DesignTokens tokens,
            UpgradeData data, int index, float frac, float spacing)
        {
            // Each card stretches full width, stacks top→bottom.
            var card = new GameObject($"Card_{data.UpgradeId}",
                typeof(RectTransform), typeof(CanvasGroup), typeof(UpgradeCardView));
            card.transform.SetParent(panel, false);
            var rt = (RectTransform)card.transform;
            rt.anchorMin = new Vector2(0, 1f - (index + 1) * frac);
            rt.anchorMax = new Vector2(1, 1f - index * frac);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(0, spacing * 0.5f);
            rt.offsetMax = new Vector2(0, -spacing * 0.5f);

            // ---- Floor (puffy 3D depth — 5px darker, sits BEHIND the body) -----------------
            var floorGo = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floorGo.transform.SetParent(card.transform, false);
            var floorRt = (RectTransform)floorGo.transform;
            floorRt.anchorMin = Vector2.zero; floorRt.anchorMax = Vector2.one;
            floorRt.offsetMin = new Vector2(0, -tokens.puffyFloorCard);
            floorRt.offsetMax = Vector2.zero;
            var floorImg = floorGo.GetComponent<Image>();
            floorImg.sprite = PuffySprite.RoundedFill(tokens.radiusMedium);
            floorImg.type = Image.Type.Sliced;
            floorImg.color = DesignTokens.Darken(CreamBody, 0.32f); // brun/gris foncé per spec §5
            floorImg.raycastTarget = false;

            // ---- Body (cream face) ---------------------------------------------------------
            var bodyImg = card.AddComponent<Image>();
            bodyImg.sprite = PuffySprite.RoundedFill(tokens.radiusMedium);
            bodyImg.type = Image.Type.Sliced;
            bodyImg.color = CreamBody;
            bodyImg.raycastTarget = true;

            // ---- Outline (charcoal 3px) ----------------------------------------------------
            var outlineGo = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outlineGo.transform.SetParent(card.transform, false);
            var olRt = (RectTransform)outlineGo.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outlineGo.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(tokens.radiusMedium, tokens.puffyOutline);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // ---- Row : Icon (left) / Text (center) / Cost (right) -------------------------
            var iconBg = ResolveIconBg(data.UpgradeId);
            var iconRt = BuildIconCircle(card.transform, tokens, iconBg, data.UpgradeId);
            var nameLabel = BuildNameLabel(card.transform, tokens);
            var levelLabel = BuildLevelLabel(card.transform, tokens);
            var effectLabel = BuildEffectLabel(card.transform, tokens);
            var costLabel = BuildCostPill(card.transform, tokens, out var costButtonGo);

            // ---- Wire UpgradeCardView to keep affordability + click refresh logic ---------
            // The view drives the labels + colors. We expose internal refs via SetReferences.
            var view = card.GetComponent<UpgradeCardView>();
            // Reuse the public Init path — view will rebuild its OWN hierarchy by default. To avoid
            // that we set its private back-fields BEFORE Init by attaching a small bridge component.
            // Easiest: don't call Init() on the existing view (that rebuilds); use a slim driver.
            UnityEngine.Object.Destroy(view);
            var driver = card.AddComponent<UpgradeCardDriver>();
            driver.Bind(data, bodyImg, nameLabel, effectLabel, levelLabel, costLabel,
                costButtonGo.GetComponent<Button>(), iconRt.GetComponent<Image>());
        }

        // ====================================================================================
        //  ICON — 56px circle puffy with a procedural glyph identifying the upgrade voie.
        // ====================================================================================

        private static RectTransform BuildIconCircle(Transform parent, DesignTokens tokens,
            Color faceColor, string upgradeId)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(14, 0);
            rt.sizeDelta = new Vector2(56, 56);

            // Floor (puffy depth).
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(rt, false);
            floor.transform.SetAsFirstSibling();
            var frt = (RectTransform)floor.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(0, -4); frt.offsetMax = Vector2.zero;
            var fImg = floor.GetComponent<Image>();
            fImg.sprite = PuffySprite.RoundedFill(28);
            fImg.type = Image.Type.Sliced;
            fImg.color = DesignTokens.Darken(faceColor, 0.30f);
            fImg.raycastTarget = false;

            var bg = go.GetComponent<Image>();
            bg.sprite = PuffySprite.RoundedFill(28);
            bg.type = Image.Type.Sliced;
            bg.color = faceColor;
            bg.raycastTarget = false;

            // Outline.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(rt, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(28, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Glyph (white procedural inside).
            BuildGlyph(rt, upgradeId);

            return rt;
        }

        /// <summary>Builds a simple white glyph inside the icon circle (sword / lotus / people).</summary>
        private static void BuildGlyph(Transform parent, string upgradeId)
        {
            switch (upgradeId)
            {
                case "frappe":
                    // Sword : white vertical blade + horizontal guard (cross silhouette).
                    AddRect(parent, "Blade", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        Vector2.zero, new Vector2(7, 32), Color.white, radius: 3);
                    AddRect(parent, "Guard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0, 4), new Vector2(22, 5), Color.white, radius: 2);
                    AddRect(parent, "Hilt", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0, -12), new Vector2(5, 6), new Color(1f, 0.95f, 0.6f), radius: 2);
                    break;
                case "meditation":
                    // Lotus petals : 3 stacked rounded rects (centerline + 2 angled sides).
                    AddRect(parent, "Center", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        Vector2.zero, new Vector2(8, 26), Color.white, radius: 4);
                    AddAngledRect(parent, "Left", new Vector2(0.5f, 0.5f),
                        new Vector2(-6, 0), new Vector2(8, 22), Color.white, radius: 4, zEuler: 25f);
                    AddAngledRect(parent, "Right", new Vector2(0.5f, 0.5f),
                        new Vector2(6, 0), new Vector2(8, 22), Color.white, radius: 4, zEuler: -25f);
                    AddRect(parent, "Base", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0, -16), new Vector2(28, 4), Color.white, radius: 2);
                    break;
                case "disciple":
                    // People : 3 small heads + 3 stacked bodies (group silhouette).
                    AddRect(parent, "HeadL", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(-12, 10), new Vector2(8, 8), Color.white, radius: 4);
                    AddRect(parent, "HeadC", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0, 14), new Vector2(8, 8), Color.white, radius: 4);
                    AddRect(parent, "HeadR", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(12, 10), new Vector2(8, 8), Color.white, radius: 4);
                    AddRect(parent, "BodyL", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(-12, -6), new Vector2(10, 14), Color.white, radius: 3);
                    AddRect(parent, "BodyC", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0, -3), new Vector2(10, 18), Color.white, radius: 3);
                    AddRect(parent, "BodyR", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(12, -6), new Vector2(10, 14), Color.white, radius: 3);
                    break;
                default:
                    AddRect(parent, "Dot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        Vector2.zero, new Vector2(20, 20), Color.white, radius: 10);
                    break;
            }
        }

        private static void AddRect(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            Vector2 pos, Vector2 size, Color color, int radius)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = PuffySprite.RoundedFill(Mathf.Max(2, radius));
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
        }

        private static void AddAngledRect(Transform parent, string name, Vector2 anchor,
            Vector2 pos, Vector2 size, Color color, int radius, float zEuler)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchor; rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localEulerAngles = new Vector3(0, 0, zEuler);
            var img = go.GetComponent<Image>();
            img.sprite = PuffySprite.RoundedFill(Mathf.Max(2, radius));
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
        }

        private static Color ResolveIconBg(string upgradeId)
        {
            switch (upgradeId)
            {
                case "frappe": return StrikeIconBg;
                case "meditation": return FocusIconBg;
                case "disciple": return PowerIconBg;
                default: return StrikeIconBg;
            }
        }

        // ====================================================================================
        //  TEXT LABELS — title (Lilita One), level (JetBrains Mono), description (Plus Jakarta).
        // ====================================================================================

        private static TextMeshProUGUI BuildNameLabel(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0.66f, 1f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.offsetMin = new Vector2(84, 4);
            rt.offsetMax = new Vector2(0, -6);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 26;
            tmp.alignment = TextAlignmentOptions.BottomLeft;
            tmp.color = tokens.navyContour;
            tmp.text = "Strike";
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.12f;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        private static TextMeshProUGUI BuildLevelLabel(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0.66f, 1f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.offsetMin = new Vector2(84, 4);
            rt.offsetMax = new Vector2(0, -6);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.NumbersFont;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.BottomRight;
            tmp.color = new Color(tokens.navyContour.r, tokens.navyContour.g, tokens.navyContour.b, 0.65f);
            tmp.text = "Lv.0";
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        private static TextMeshProUGUI BuildEffectLabel(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("Effect", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0.66f, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.offsetMin = new Vector2(84, 6);
            rt.offsetMax = new Vector2(0, -2);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.PrimaryFont;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = new Color(tokens.navyContour.r, tokens.navyContour.g, tokens.navyContour.b, 0.72f);
            tmp.text = "";
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        // ====================================================================================
        //  COST PILL — green puffy button right-side with coin or + cost number.
        // ====================================================================================

        private static TextMeshProUGUI BuildCostPill(Transform parent, DesignTokens tokens,
            out GameObject buttonGo)
        {
            buttonGo = new GameObject("CostPill",
                typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var rt = (RectTransform)buttonGo.transform;
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-14, 0);
            rt.sizeDelta = new Vector2(126, 56);

            // Floor.
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(buttonGo.transform, false);
            floor.transform.SetAsFirstSibling();
            var frt = (RectTransform)floor.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(0, -5); frt.offsetMax = Vector2.zero;
            var fImg = floor.GetComponent<Image>();
            fImg.sprite = PuffySprite.RoundedFill(tokens.radiusMedium);
            fImg.type = Image.Type.Sliced;
            fImg.color = CostFloor;
            fImg.raycastTarget = false;

            // Face (green).
            var face = buttonGo.GetComponent<Image>();
            face.sprite = PuffySprite.RoundedFill(tokens.radiusMedium);
            face.type = Image.Type.Sliced;
            face.color = CostFace;
            face.raycastTarget = true;

            // Outline.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(buttonGo.transform, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(tokens.radiusMedium, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // Coin or (small circle on the left of the pill).
            var coin = new GameObject("Coin", typeof(RectTransform), typeof(Image));
            coin.transform.SetParent(buttonGo.transform, false);
            var crt = (RectTransform)coin.transform;
            crt.anchorMin = new Vector2(0, 0.5f); crt.anchorMax = new Vector2(0, 0.5f);
            crt.pivot = new Vector2(0, 0.5f);
            crt.anchoredPosition = new Vector2(8, 0);
            crt.sizeDelta = new Vector2(28, 28);
            var cImg = coin.GetComponent<Image>();
            cImg.sprite = PuffySprite.RoundedFill(14);
            cImg.type = Image.Type.Sliced;
            cImg.color = CoinGold;
            cImg.raycastTarget = false;

            // Coin outline charcoal.
            var coinOutline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            coinOutline.transform.SetParent(coin.transform, false);
            var coRt = (RectTransform)coinOutline.transform;
            coRt.anchorMin = Vector2.zero; coRt.anchorMax = Vector2.one;
            coRt.offsetMin = Vector2.zero; coRt.offsetMax = Vector2.zero;
            var coImg = coinOutline.GetComponent<Image>();
            coImg.sprite = PuffySprite.RoundedOutline(14, 2);
            coImg.type = Image.Type.Sliced;
            coImg.color = tokens.navyContour;
            coImg.raycastTarget = false;

            // Coin glyph (small darker stripe through the center).
            var coinStripe = new GameObject("Stripe", typeof(RectTransform), typeof(Image));
            coinStripe.transform.SetParent(coin.transform, false);
            var csRt = (RectTransform)coinStripe.transform;
            csRt.anchorMin = new Vector2(0.5f, 0.5f); csRt.anchorMax = new Vector2(0.5f, 0.5f);
            csRt.pivot = new Vector2(0.5f, 0.5f);
            csRt.anchoredPosition = Vector2.zero;
            csRt.sizeDelta = new Vector2(12, 4);
            var csImg = coinStripe.GetComponent<Image>();
            csImg.sprite = PuffySprite.RoundedFill(2);
            csImg.type = Image.Type.Sliced;
            csImg.color = CoinGoldDeep;
            csImg.raycastTarget = false;

            // Cost label (Lilita One white outlined charcoal).
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(buttonGo.transform, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(40, 0); lblRt.offsetMax = new Vector2(-6, 0);

            var tmp = lblGo.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = "1.2K";
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.24f;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            // Apply SagaButton.Wrap for puffy press tween (recolor coherent with skill buttons).
            SagaButton.Wrap(buttonGo, SagaButton.Variant.Standard,
                voieTint: null, radius: tokens.radiusMedium, floorPx: 5);

            return tmp;
        }
    }
}
