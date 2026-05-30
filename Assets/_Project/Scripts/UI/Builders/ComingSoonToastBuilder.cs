using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.6 M5 — builds the Coming Soon toast visual with RhosGFX Frame Basic Grey + Clock icon.
    /// Returns the GameObject hosting the <see cref="ComingSoonToast"/> component (already
    /// wired with the label). Caller controls position / sibling order / lifecycle.
    /// </summary>
    public static class ComingSoonToastBuilder
    {
        public static GameObject Build(Transform parent, DesignTokens tokens, RhosGFXAssetCatalog catalog)
        {
            var go = new GameObject("ComingSoonToast",
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(ComingSoonToast));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620, 140);

            // Background — RhosGFX Frame Basic Grey, tint Neutre DA panelSombre alpha 92%.
            var bg = go.GetComponent<Image>();
            bg.sprite = catalog.frameBasicGrey;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;
            bg.color = new Color(tokens.panelSombre.r, tokens.panelSombre.g, tokens.panelSombre.b, 0.92f);
            bg.raycastTarget = false;

            // Clock icon left.
            var iconGo = new GameObject("Clock", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0, 0.5f); iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(60, 0);
            iconRt.sizeDelta = new Vector2(72, 72);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = catalog.iconClock;
            iconImg.type = Image.Type.Simple;
            iconImg.preserveAspect = true;
            iconImg.color = tokens.cremeText;
            iconImg.raycastTarget = false;

            // Label.
            var lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(go.transform, false);
            var lblRt = (RectTransform)lbl.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(120, 8); lblRt.offsetMax = new Vector2(-24, -8);
            var tmp = lbl.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = tokens.cremeText;
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.24f;
            tmp.text = "Bientôt disponible";
            tmp.raycastTarget = false;

            go.GetComponent<ComingSoonToast>().Label = tmp;
            return go;
        }
    }
}
