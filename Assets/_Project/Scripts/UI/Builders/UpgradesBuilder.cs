using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.6 zone 4 — 3 Upgrade cards puffy 3D using RhosGFX Frames + tinted Icons.
    ///
    /// Mapping icons (validation coordinateur) :
    ///   - Strike (frappe)      → Sword Outline tinted Coral DA #FF6B6B
    ///   - Focus  (meditation)  → Bell  Outline tinted Sky   DA #65C8FF (cloche zen méditation)
    ///   - Power  (disciple)    → Friends Outline tinted Vert  DA #7DE34F (disciples)
    ///
    /// Layout : panel cream-on-brown 288 px haut, 3 cards 333×248 horizontales avec gap 16,
    /// chaque card : icon top 96×96 / nom / level / effect / cost pill plein largeur 48 px.
    /// </summary>
    public static class UpgradesBuilder
    {
        public static void Build(BuilderContext ctx)
        {
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
            var catalog = RhosGFXAssetCatalog.Get();

            var panel = new GameObject("UpgradePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0, 0); panelRt.anchorMax = new Vector2(1, 0);
            panelRt.pivot = new Vector2(0.5f, 0f);
            panelRt.anchoredPosition = new Vector2(0, 422);
            panelRt.sizeDelta = new Vector2(-48, 288);
            // Panel background = container flat brown, dojo wood feel.
            var panelImg = panel.GetComponent<Image>();
            panelImg.sprite = catalog.containerFlatBrown;
            panelImg.type = Image.Type.Simple;
            panelImg.preserveAspect = false;
            panelImg.color = new Color(tokens.boisDojoDark.r, tokens.boisDojoDark.g, tokens.boisDojoDark.b, 0.85f);
            panelImg.raycastTarget = false;

            var count = upgrades.Count;
            var frac = 1f / count;
            for (var i = 0; i < count; i++)
            {
                BuildCard(panelRt, tokens, catalog, upgrades[i], i, frac);
            }
        }

        private static void BuildCard(RectTransform panel, DesignTokens tokens, RhosGFXAssetCatalog catalog,
            UpgradeData data, int index, float frac)
        {
            var card = new GameObject($"Card_{data.UpgradeId}",
                typeof(RectTransform), typeof(CanvasGroup));
            card.transform.SetParent(panel, false);
            var rt = (RectTransform)card.transform;
            rt.anchorMin = new Vector2(index * frac, 0);
            rt.anchorMax = new Vector2((index + 1) * frac, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(8, 16);
            rt.offsetMax = new Vector2(-8, -16);

            // Background (cream interior — container flat used as inner panel, tinted DA cream/sable).
            var bodyImg = card.AddComponent<Image>();
            bodyImg.sprite = catalog.containerFlatBrown;
            bodyImg.type = Image.Type.Simple;
            bodyImg.preserveAspect = false;
            bodyImg.color = tokens.panelClair; // cream DA #F9E6C8
            bodyImg.raycastTarget = true;

            // Outer frame (decorative) — RhosGFX Basic Brown frame.
            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(card.transform, false);
            var frameRt = (RectTransform)frame.transform;
            frameRt.anchorMin = Vector2.zero; frameRt.anchorMax = Vector2.one;
            frameRt.offsetMin = Vector2.zero; frameRt.offsetMax = Vector2.zero;
            var frameImg = frame.GetComponent<Image>();
            frameImg.sprite = catalog.frameBasicBrown;
            frameImg.type = Image.Type.Simple;
            frameImg.preserveAspect = false;
            frameImg.color = Color.white;
            frameImg.raycastTarget = false;

            // Icon (96×96) — RhosGFX outline icon tinted DA voie color.
            var iconImg = BuildIconForUpgrade(card.transform, tokens, catalog, data.UpgradeId);

            // Text labels.
            var nameLabel = BuildNameLabel(card.transform, tokens);
            var levelLabel = BuildLevelLabel(card.transform, tokens);
            var effectLabel = BuildEffectLabel(card.transform, tokens);

            // Cost pill (full-width bottom).
            var costLabel = BuildCostPill(card.transform, tokens, catalog, out var costButtonGo);

            // BreathingPulseView — subtle idle pulse on card root.
            var pulse = card.AddComponent<BreathingPulseView>();
            pulse.Target = rt;
            pulse.Peak = 1.015f;
            pulse.Period = 3.0f;

            // Wire driver to keep affordability/click refresh logic.
            var driver = card.AddComponent<UpgradeCardDriver>();
            driver.Bind(data, bodyImg, nameLabel, effectLabel, levelLabel, costLabel,
                costButtonGo.GetComponent<Button>(), iconImg);
        }

        // ====================================================================================
        //  ICON — RhosGFX outline glyph tinted per voie.
        // ====================================================================================

        private static Image BuildIconForUpgrade(Transform parent, DesignTokens tokens,
            RhosGFXAssetCatalog catalog, string upgradeId)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -16);
            rt.sizeDelta = new Vector2(96, 96);

            var img = go.GetComponent<Image>();
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.raycastTarget = false;

            switch (upgradeId)
            {
                case "frappe":
                    img.sprite = catalog.iconSwordOutline != null ? catalog.iconSwordOutline : catalog.iconSword;
                    img.color = tokens.coralAction; // #FF6B6B
                    break;
                case "meditation":
                    img.sprite = catalog.iconBellOutline != null ? catalog.iconBellOutline : catalog.iconBell;
                    img.color = tokens.skyBlue; // #65C8FF
                    break;
                case "disciple":
                    img.sprite = catalog.iconFriendsOutline != null ? catalog.iconFriendsOutline : catalog.iconFriends;
                    img.color = tokens.vertBouton; // #7DE34F
                    break;
                default:
                    img.sprite = catalog.iconCoinGold;
                    img.color = Color.white;
                    break;
            }
            return img;
        }

        // ====================================================================================
        //  TEXT LABELS
        // ====================================================================================

        private static TextMeshProUGUI BuildNameLabel(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1f); rt.anchorMax = new Vector2(1, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -120);
            rt.sizeDelta = new Vector2(0, 36);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 30;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = tokens.navyContour;
            tmp.text = "Strike";
            tmp.outlineColor = tokens.cremeText;
            tmp.outlineWidth = 0.18f;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        private static TextMeshProUGUI BuildLevelLabel(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1f); rt.anchorMax = new Vector2(1, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -158);
            rt.sizeDelta = new Vector2(0, 24);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.NumbersFont;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(tokens.navyContour.r, tokens.navyContour.g, tokens.navyContour.b, 0.7f);
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
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 68);
            rt.sizeDelta = new Vector2(-20, 22);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.PrimaryFont;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(tokens.navyContour.r, tokens.navyContour.g, tokens.navyContour.b, 0.75f);
            tmp.text = "";
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        // ====================================================================================
        //  COST PILL — RhosGFX Square 3D Green button + Coin icon + cost text + PressBounce.
        // ====================================================================================

        private static TextMeshProUGUI BuildCostPill(Transform parent, DesignTokens tokens,
            RhosGFXAssetCatalog catalog, out GameObject buttonGo)
        {
            buttonGo = new GameObject("CostPill",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(PressBounceView));
            buttonGo.transform.SetParent(parent, false);
            var rt = (RectTransform)buttonGo.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 10);
            rt.sizeDelta = new Vector2(-20, 52);

            // Background sprite = RhosGFX square 3D green.
            var face = buttonGo.GetComponent<Image>();
            face.sprite = catalog.square3DGreen25.standard;
            face.type = Image.Type.Simple;
            face.preserveAspect = false;
            face.color = tokens.vertBouton; // #7DE34F vif DA
            face.raycastTarget = true;

            // Coin icon (left).
            var coin = new GameObject("Coin", typeof(RectTransform), typeof(Image));
            coin.transform.SetParent(buttonGo.transform, false);
            var crt = (RectTransform)coin.transform;
            crt.anchorMin = new Vector2(0, 0.5f); crt.anchorMax = new Vector2(0, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = new Vector2(28, 2);
            crt.sizeDelta = new Vector2(36, 36);
            var cImg = coin.GetComponent<Image>();
            cImg.sprite = catalog.iconCoinGold;
            cImg.type = Image.Type.Simple;
            cImg.preserveAspect = true;
            cImg.color = Color.white;
            cImg.raycastTarget = false;

            // Cost label.
            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(buttonGo.transform, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(56, 2); lblRt.offsetMax = new Vector2(-12, 2);

            var tmp = lblGo.GetComponent<TextMeshProUGUI>();
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = "1.2K";
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.28f;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }
    }
}
