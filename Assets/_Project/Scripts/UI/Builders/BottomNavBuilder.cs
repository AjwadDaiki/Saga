using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte zone 7 — bottom nav 5 tabs (Shop/Hero/Dojo/Artifacts/Legend) with Dojo active.
    /// Owns the "Coming Soon" toast for inactive tabs.
    ///
    /// Issue corrigée (Sprint 7.5 zone 7 décision Q8) :
    /// - Les emoji 🛒🥋⚔✦🏆 rendaient en carrés blancs (manquants des SDF Latin) : remplacés par
    ///   des icônes procédurales 2-Image (cercle coloré + glyphe contrasté).
    /// - Le bouton INVENTAIRE side-rail legacy a été supprimé du Bootstrap : l'inventaire migre dans
    ///   l'onglet Artifacts (placeholder Coming Soon Sprint 7.5, full UI Sprint 8+).
    /// </summary>
    public static class BottomNavBuilder
    {
        private static GameObject _comingSoon;

        // Icon palette per tab — colored cartoon glyphs against the dark nav.
        private static readonly Color ShopColor = new Color(0.961f, 0.522f, 0.224f, 1f);    // #F58539 orange (sac/shop)
        private static readonly Color HeroColor = new Color(0.282f, 0.612f, 0.937f, 1f);    // #489CEF bleu (héros)
        private static readonly Color DojoColor = new Color(0.859f, 0.196f, 0.216f, 1f);    // #db3237 rouge (épées)
        private static readonly Color ArtifactsColor = new Color(0.980f, 0.780f, 0.259f, 1f); // #FAC742 or (étoile/artefact)
        private static readonly Color LegendColor = new Color(0.239f, 0.839f, 0.549f, 1f);  // #3DD68C vert (coupe)

        public static void Build(BuilderContext ctx)
        {
            Debug.Log("[BOTTOMNAV] Build called, parent: " + (ctx.UIRoot != null ? "UIRoot" : "Canvas"));
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            BuildBottomNav(parent);
        }

        /// <summary>Bottom nav — 5 tabs (Shop/Hero/Dojo/Artifacts/Legend). Dojo active (raised, bleu).</summary>
        private static void BuildBottomNav(Transform parent)
        {
            var tokens = DesignTokens.Get();
            var nav = new GameObject("BottomNav", typeof(RectTransform), typeof(Image));
            nav.transform.SetParent(parent, false);
            var rt = (RectTransform)nav.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            // DIRECTION_ARTISTIQUE.md §3.2 — bottom nav 10-12 % de l'écran. À 1920 ref c'est ~220 px.
            // Le SafeAreaContainer parent gère déjà la home bar iPhone (~68 px) → 220 px utiles.
            rt.sizeDelta = new Vector2(0, 220);
            var bg = nav.GetComponent<Image>();
            bg.color = DesignTokens.Darken(tokens.panelClair, 0.15f);

            // Top border (charcoal line).
            var border = new GameObject("TopBorder", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(nav.transform, false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(1, 1); brt.pivot = new Vector2(0.5f, 1f);
            brt.sizeDelta = new Vector2(0, 4); brt.anchoredPosition = Vector2.zero;
            border.GetComponent<Image>().color = tokens.panelSombre2;

            string[] labels = { "Shop", "Hero", "Dojo", "Artifacts", "Legend" };
            Color[] iconColors = { ShopColor, HeroColor, DojoColor, ArtifactsColor, LegendColor };
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
                    timg.color = tokens.skyBlue;
                }
                else timg.color = new Color(0, 0, 0, 0);

                // Procedural icon (top half of the tab).
                BuildTabIcon(tab.transform, labels[i], iconColors[i], tokens);

                // Label (bottom strip).
                var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lbl.transform.SetParent(tab.transform, false);
                var lrt = (RectTransform)lbl.transform;
                lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 0.35f);
                lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
                var tmp = lbl.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center; tmp.font = tokens.PrimaryFont;
                tmp.fontSize = tokens.fontSmall; tmp.fontStyle = FontStyles.Bold;
                tmp.color = active ? tokens.navyContour : tokens.panelSombre2;
                tmp.text = labels[i];
                tmp.outlineColor = tokens.navyContour;
                tmp.outlineWidth = active ? 0f : 0.14f;
                tmp.raycastTarget = false;

                if (!active)
                {
                    var captured = labels[i];
                    tab.GetComponent<Button>().onClick.AddListener(() => ShowComingSoon(parent, captured));
                }
            }
        }

        // ====================================================================================
        //  Procedural tab icons — coloured circle + contrasting glyph (Shop/Hero/Dojo/Artifacts/Legend).
        //  Replaces the emoji glyphs 🛒🥋⚔✦🏆 that rendered as missing tofu in SDF Latin fonts.
        // ====================================================================================

        private static void BuildTabIcon(Transform parent, string label, Color iconColor, DesignTokens tokens)
        {
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var rt = (RectTransform)icon.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 16);
            rt.sizeDelta = new Vector2(36, 36);

            var bg = icon.GetComponent<Image>();
            bg.sprite = PuffySprite.RoundedFill(18);
            bg.type = Image.Type.Sliced;
            bg.color = iconColor;
            bg.raycastTarget = false;

            // Outline charcoal.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(rt, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(18, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = tokens.navyContour;
            olImg.raycastTarget = false;

            // White glyph centered inside.
            BuildGlyph(rt, label);
        }

        private static void BuildGlyph(Transform parent, string label)
        {
            switch (label)
            {
                case "Shop":
                    // Sac : rectangle inférieur + arc poignée approximé par 2 rects fins.
                    AddImg(parent, "Body", Vector2.zero, new Vector2(20, 14), Color.white, 4);
                    // Poignée gauche/droite (rectangles fins en haut formant un arc).
                    AddImg(parent, "HandleL", new Vector2(-6, 9), new Vector2(3, 10), Color.white, 1);
                    AddImg(parent, "HandleR", new Vector2(6, 9), new Vector2(3, 10), Color.white, 1);
                    AddImg(parent, "HandleTop", new Vector2(0, 12), new Vector2(12, 3), Color.white, 1);
                    break;
                case "Hero":
                    // Silhouette héros : tête (cercle) + corps (rect arrondi).
                    AddImg(parent, "Head", new Vector2(0, 7), new Vector2(10, 10), Color.white, 5);
                    AddImg(parent, "Body", new Vector2(0, -6), new Vector2(16, 12), Color.white, 4);
                    break;
                case "Dojo":
                    // Épées croisées : 2 rectangles longs diagonaux blancs.
                    AddRotatedImg(parent, "SwordL", new Vector2(-2, 0), new Vector2(4, 26), Color.white, 3, 35f);
                    AddRotatedImg(parent, "SwordR", new Vector2(2, 0), new Vector2(4, 26), Color.white, 3, -35f);
                    break;
                case "Artifacts":
                    // Étoile/sparkle : 4 rectangles fins en croix.
                    AddImg(parent, "Vert", Vector2.zero, new Vector2(4, 22), Color.white, 2);
                    AddImg(parent, "Horiz", Vector2.zero, new Vector2(22, 4), Color.white, 2);
                    AddRotatedImg(parent, "DiagL", Vector2.zero, new Vector2(3, 16), Color.white, 2, 45f);
                    AddRotatedImg(parent, "DiagR", Vector2.zero, new Vector2(3, 16), Color.white, 2, -45f);
                    break;
                case "Legend":
                    // Coupe : trapèze (rectangle large en haut) + tige + base.
                    AddImg(parent, "Cup", new Vector2(0, 6), new Vector2(20, 12), Color.white, 4);
                    AddImg(parent, "Stem", new Vector2(0, -4), new Vector2(4, 8), Color.white, 1);
                    AddImg(parent, "Base", new Vector2(0, -10), new Vector2(14, 4), Color.white, 2);
                    break;
                default:
                    AddImg(parent, "Dot", Vector2.zero, new Vector2(12, 12), Color.white, 6);
                    break;
            }
        }

        private static void AddImg(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int radius)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = PuffySprite.RoundedFill(Mathf.Max(2, radius));
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
        }

        private static void AddRotatedImg(Transform parent, string name, Vector2 pos, Vector2 size,
            Color color, int radius, float zEuler)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
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

        private static void ShowComingSoon(Transform parent, string screen)
        {
            var tokens = DesignTokens.Get();
            if (_comingSoon == null)
            {
                _comingSoon = new GameObject("ComingSoonToast", typeof(RectTransform), typeof(Image));
                _comingSoon.transform.SetParent(parent, false);
                var rt = (RectTransform)_comingSoon.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(560, 120);
                var img = _comingSoon.GetComponent<Image>();
                img.sprite = PuffySprite.RoundedFill(24); img.type = Image.Type.Sliced; img.color = tokens.panelSombre2;
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
    }
}
