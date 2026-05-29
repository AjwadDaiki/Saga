using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte — bottom nav 5 tabs (Shop/Hero/Dojo/Artifacts/Legend) with Dojo active.
    /// Also owns the "Coming Soon" toast for inactive tabs, and the standalone Inventaire side button.
    /// </summary>
    public static class BottomNavBuilder
    {
        private static GameObject _comingSoon;

        public static void Build(BuilderContext ctx)
        {
            BuildBottomNav(ctx.Canvas);
        }

        /// <summary>Bottom nav — 5 tabs (Shop/Hero/Dojo/Artifacts/Legend). Dojo active (raised, bleu).</summary>
        private static void BuildBottomNav(Canvas canvas)
        {
            var tokens = DesignTokens.Get();
            var nav = new GameObject("BottomNav", typeof(RectTransform), typeof(Image));
            nav.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)nav.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 150); // ~7.8% of 1920 incl. safe area
            var bg = nav.GetComponent<Image>();
            bg.color = tokens.m3SurfaceContainerHighest;

            // Top border (charcoal line).
            var border = new GameObject("TopBorder", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(nav.transform, false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(1, 1); brt.pivot = new Vector2(0.5f, 1f);
            brt.sizeDelta = new Vector2(0, 4); brt.anchoredPosition = Vector2.zero;
            border.GetComponent<Image>().color = tokens.m3Outline;

            string[] labels = { "Shop", "Hero", "Dojo", "Artifacts", "Legend" };
            string[] glyphs = { "🛒", "🥋", "⚔", "✦", "🏆" };
            const int count = 5;
            var frac = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var active = i == 2; // Dojo
                var tab = new GameObject($"Tab_{labels[i]}", typeof(RectTransform), typeof(Image), typeof(Button));
                tab.transform.SetParent(nav.transform, false);
                var trt = (RectTransform)tab.transform;
                trt.anchorMin = new Vector2(i * frac, 0); trt.anchorMax = new Vector2((i + 1) * frac, 1);
                // Active tab raised + colored card; inactive transparent.
                trt.offsetMin = new Vector2(8, active ? 18 : 30);
                trt.offsetMax = new Vector2(-8, active ? -8 : -24);
                var timg = tab.GetComponent<Image>();
                if (active)
                {
                    timg.sprite = PuffySprite.RoundedFill(16); timg.type = Image.Type.Sliced;
                    timg.color = tokens.m3TertiaryContainer;
                }
                else timg.color = new Color(0, 0, 0, 0);

                var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lbl.transform.SetParent(tab.transform, false);
                var lrt = (RectTransform)lbl.transform;
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
                var tmp = lbl.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center; tmp.font = tokens.PrimaryFont;
                tmp.fontSize = tokens.fontSmall; tmp.fontStyle = FontStyles.Bold;
                tmp.color = active ? tokens.m3OnTertiaryContainer : tokens.m3Outline;
                tmp.text = $"{glyphs[i]}\n{labels[i]}";
                tmp.raycastTarget = false;

                if (!active)
                {
                    var captured = labels[i];
                    tab.GetComponent<Button>().onClick.AddListener(() => ShowComingSoon(canvas, captured));
                }
            }
        }

        private static void ShowComingSoon(Canvas canvas, string screen)
        {
            var tokens = DesignTokens.Get();
            if (_comingSoon == null)
            {
                _comingSoon = new GameObject("ComingSoonToast", typeof(RectTransform), typeof(Image));
                _comingSoon.transform.SetParent(canvas.transform, false);
                var rt = (RectTransform)_comingSoon.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(560, 120);
                var img = _comingSoon.GetComponent<Image>();
                img.sprite = PuffySprite.RoundedFill(24); img.type = Image.Type.Sliced; img.color = tokens.m3InverseSurface;
                var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lbl.transform.SetParent(_comingSoon.transform, false);
                var lrt = (RectTransform)lbl.transform; lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
                var tmp = lbl.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center; tmp.font = tokens.DisplayFont; tmp.fontSize = 28; tmp.color = Color.white;
                _comingSoon.AddComponent<ComingSoonToast>().Label = tmp;
            }
            _comingSoon.transform.SetAsLastSibling();
            _comingSoon.GetComponent<ComingSoonToast>().Show($"{screen}\nBientôt disponible");
        }

        /// <summary>
        /// Sprint 7 standalone Inventaire side button (left side-rail, below Souffle).
        /// Called by Bootstrap after the EquipmentInventoryModal exists.
        /// </summary>
        public static void BuildInventaireButton(Canvas canvas, EquipmentInventoryModal modal)
        {
            var tokens = DesignTokens.Get();
            var btn = new GameObject("InventaireButton",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Button));
            btn.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)btn.transform;
            // Sprint 7.5 refonte : left side-rail, below the Souffle button (clear of top bar pills).
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(16, -24);
            rt.sizeDelta = new Vector2(132, 72);

            var img = btn.GetComponent<Image>();
            img.color = tokens.surfaceMid;
            img.raycastTarget = true;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one; lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = tokens.textPrimary;
            lblTmp.font = tokens.PrimaryFont;
            lblTmp.fontSize = tokens.fontH3;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.text = "INVENTAIRE";
            lblTmp.raycastTarget = false;

            btn.GetComponent<Button>().onClick.AddListener(modal.Open);
            SagaButton.Wrap(btn, SagaButton.Variant.Standard);
        }
    }
}
