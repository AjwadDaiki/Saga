using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.6 zone 6 — Bottom nav 5 tabs (Shop/Hero/Dojo/Artifacts/Legend) with Dojo active.
    /// RhosGFX flat round buttons for inactive tabs + RhosGFX 3D round Yellow for active Dojo
    /// (lift +12 px scale 1.05). Icons from RhosGFX catalog. Owns the Coming Soon toast.
    /// </summary>
    public static class BottomNavBuilder
    {
        private static GameObject _comingSoon;
        private static readonly string[] Labels = { "Shop", "Hero", "Dojo", "Artifacts", "Legend" };

        public static void Build(BuilderContext ctx)
        {
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            BuildBottomNav(parent);
        }

        private static void BuildBottomNav(Transform parent)
        {
            var tokens = DesignTokens.Get();
            var catalog = RhosGFXAssetCatalog.Get();

            var nav = new GameObject("BottomNav", typeof(RectTransform), typeof(Image));
            nav.transform.SetParent(parent, false);
            var rt = (RectTransform)nav.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(-32, 192);

            var bg = nav.GetComponent<Image>();
            bg.sprite = catalog.containerFlatBrown;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;
            bg.color = new Color(tokens.boisDojoDark.r, tokens.boisDojoDark.g, tokens.boisDojoDark.b, 0.92f);
            bg.raycastTarget = false;

            const int count = 5;
            var frac = 1f / count;
            for (var i = 0; i < count; i++)
            {
                BuildTab(rt, tokens, catalog, parent, Labels[i], i, frac);
            }
        }

        private static void BuildTab(RectTransform navRt, DesignTokens tokens, RhosGFXAssetCatalog catalog,
            Transform toastParent, string label, int index, float frac)
        {
            var active = index == 2; // Dojo center
            var tab = new GameObject($"Tab_{label}",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(PressBounceView));
            tab.transform.SetParent(navRt, false);
            var trt = (RectTransform)tab.transform;
            trt.anchorMin = new Vector2(index * frac, 0); trt.anchorMax = new Vector2((index + 1) * frac, 1);
            trt.offsetMin = new Vector2(8, active ? 28 : 16);
            trt.offsetMax = new Vector2(-8, active ? -4 : -16);
            if (active) trt.localScale = new Vector3(1.05f, 1.05f, 1f);

            var timg = tab.GetComponent<Image>();
            timg.type = Image.Type.Simple;
            timg.preserveAspect = false;
            if (active)
            {
                timg.sprite = catalog.round3DYellow1.standard;
                timg.color = tokens.jauneReward;
            }
            else
            {
                timg.sprite = catalog.roundFlatGrey1.standard;
                // Brown tint + reduced alpha for "visible but desaturated" inactive tab (Eatventure pattern).
                timg.color = new Color(tokens.boisDojoLight.r, tokens.boisDojoLight.g, tokens.boisDojoLight.b, 0.55f);
            }
            timg.raycastTarget = true;

            // Active tab pulsing halo behind (subtle "you are here" cue).
            if (active)
            {
                var glow = BuildHalo(trt, catalog.round3DYellow1.standard, tokens.jauneReward);
                var ready = tab.AddComponent<ReadyGlowView>();
                ready.Glow = glow;
                ready.SetReady(true);
            }

            // Icon (RhosGFX catalog).
            BuildTabIcon(trt, tokens, catalog, label, active);

            // Label below.
            var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(tab.transform, false);
            var lrt = (RectTransform)lbl.transform;
            lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 0); lrt.pivot = new Vector2(0.5f, 0f);
            lrt.sizeDelta = new Vector2(0, 30); lrt.anchoredPosition = new Vector2(0, 10);
            var tmp = lbl.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = tokens.PrimaryFont;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = active ? tokens.navyContour : tokens.cremeText;
            tmp.text = label;
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = active ? 0.15f : 0.20f;
            tmp.raycastTarget = false;

            if (!active)
            {
                var captured = label;
                tab.GetComponent<Button>().onClick.AddListener(() => ShowComingSoon(toastParent, captured));
            }
        }

        private static void BuildTabIcon(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog,
            string label, bool active)
        {
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var rt = (RectTransform)icon.transform;
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -10);
            rt.sizeDelta = new Vector2(active ? 80f : 68f, active ? 80f : 68f);

            var img = icon.GetComponent<Image>();
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.sprite = ResolveTabSprite(catalog, label, useOutline: !active);
            img.color = active ? tokens.navyContour : tokens.cremeText;
        }

        private static Sprite ResolveTabSprite(RhosGFXAssetCatalog catalog, string label, bool useOutline)
        {
            switch (label)
            {
                case "Shop": return catalog.iconBackpack;
                case "Hero": return useOutline && catalog.iconFriendsOutline != null ? catalog.iconFriendsOutline : catalog.iconFriends;
                case "Dojo": return useOutline && catalog.iconHomeOutline != null ? catalog.iconHomeOutline : catalog.iconHome;
                case "Artifacts": return catalog.iconChest;
                case "Legend": return catalog.iconTrophy;
                default: return catalog.iconCoinGold;
            }
        }

        private static Image BuildHalo(RectTransform parent, Sprite sprite, Color color)
        {
            var glow = new GameObject("Halo", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(parent, false);
            glow.transform.SetAsFirstSibling();
            var rt = (RectTransform)glow.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-10, -10); rt.offsetMax = new Vector2(10, 10);
            var img = glow.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = new Color(color.r, color.g, color.b, 0f);
            img.raycastTarget = false;
            return img;
        }

        // ====================================================================================
        //  COMING SOON TOAST (Sprint 7.6 M5 refactor — RhosGFX frame Grey + clock icon + slide-in).
        // ====================================================================================

        private static void ShowComingSoon(Transform parent, string screen)
        {
            var tokens = DesignTokens.Get();
            var catalog = RhosGFXAssetCatalog.Get();
            if (_comingSoon == null)
            {
                _comingSoon = ComingSoonToastBuilder.Build(parent, tokens, catalog);
            }
            _comingSoon.transform.SetAsLastSibling();
            _comingSoon.GetComponent<ComingSoonToast>().Show($"{screen}\nBientôt disponible");
        }
    }
}
