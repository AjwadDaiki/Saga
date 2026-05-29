using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte — top bar currency pills (Échos pill + settings round button).
    /// Force pill is built separately by Bootstrap (it owns ForceCounterView).
    /// </summary>
    public static class TopBarBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            BuildTopBarPills(ctx.Canvas);
        }

        /// <summary>Échos currency pill (Force is the main ForceCounter pill, built separately).</summary>
        private static void BuildTopBarPills(Canvas canvas)
        {
            var tokens = DesignTokens.Get();

            // Échos pill (top-right area, left of settings). Violet icon débordante + valeur.
            var echos = BuildCurrencyPill(canvas, "EchosPill", new Vector2(1f, 1f), new Vector2(-96, -40),
                iconColor: tokens.accentPrimary /*overridden below*/, glyph: "◆");
            // Override icon color to violet (Échos).
            var echosIcon = echos.transform.Find("Icon")?.GetComponent<Image>();
            if (echosIcon != null) echosIcon.color = new Color(0.69f, 0.48f, 1f, 1f); // violet
            var echosLabel = echos.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
            var gm = GameManager.Instance;
            if (echosLabel != null && gm?.State != null)
                echosLabel.text = Saga.Math.NumberFormatter.Format(gm.State.totalEchos);

            // Settings round puffy button (top-right corner).
            var settings = SagaButton.Create(canvas.transform, "SettingsButton", SagaButton.Variant.Standard,
                tokens.m3SurfaceContainerHighest, "⚙", radius: 22, floorPx: 5, labelSize: 26,
                labelColor: tokens.m3OnSurface);
            var srt = (RectTransform)settings.transform;
            srt.anchorMin = new Vector2(1f, 1f); srt.anchorMax = new Vector2(1f, 1f); srt.pivot = new Vector2(1f, 1f);
            srt.anchoredPosition = new Vector2(-24, -36);
            srt.sizeDelta = new Vector2(64, 64);
        }

        /// <summary>
        /// Build a puffy currency pill : charcoal rounded pill + a colored round icon overflowing the
        /// left edge + value label in JetBrains Mono. Returns the pill GO (Icon child + Value label child).
        /// </summary>
        private static GameObject BuildCurrencyPill(Canvas canvas, string name, Vector2 anchor, Vector2 pos,
            Color iconColor, string glyph)
        {
            var tokens = DesignTokens.Get();
            var pill = new GameObject(name, typeof(RectTransform), typeof(Image));
            pill.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)pill.transform;
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(anchor.x, anchor.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(140, 40);
            var bg = pill.GetComponent<Image>();
            bg.sprite = PuffySprite.RoundedFill(20);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.086f, 0.114f, 0.122f, 0.85f); // charcoal 85%

            // Floor (puffy depth).
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(pill.transform, false);
            floor.transform.SetAsFirstSibling();
            var fr = (RectTransform)floor.transform;
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = new Vector2(0, -4); fr.offsetMax = Vector2.zero;
            var fi = floor.GetComponent<Image>();
            fi.sprite = PuffySprite.RoundedFill(20); fi.type = Image.Type.Sliced; fi.color = tokens.m3Outline; fi.raycastTarget = false;

            // Icon round, overflowing left. Image + glyph on SEPARATE GameObjects (two Graphics on one
            // GO share a CanvasRenderer → TMP init NRE).
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(pill.transform, false);
            var irt = (RectTransform)icon.transform;
            irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(0, 0.5f); irt.pivot = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = new Vector2(2, 0); irt.sizeDelta = new Vector2(38, 38);
            var iimg = icon.GetComponent<Image>();
            iimg.sprite = PuffySprite.RoundedFill(19); iimg.type = Image.Type.Sliced; iimg.color = iconColor;
            var iglyphGo = new GameObject("Glyph", typeof(RectTransform), typeof(TextMeshProUGUI));
            iglyphGo.transform.SetParent(icon.transform, false);
            var iglyphRt = (RectTransform)iglyphGo.transform;
            iglyphRt.anchorMin = Vector2.zero; iglyphRt.anchorMax = Vector2.one; iglyphRt.offsetMin = Vector2.zero; iglyphRt.offsetMax = Vector2.zero;
            var iglyph = iglyphGo.GetComponent<TextMeshProUGUI>();
            iglyph.alignment = TextAlignmentOptions.Center; iglyph.font = tokens.DisplayFont; iglyph.fontSize = 18;
            iglyph.color = tokens.m3OnSurface; iglyph.text = glyph; iglyph.raycastTarget = false;

            // Value label.
            var val = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            val.transform.SetParent(pill.transform, false);
            var vrt = (RectTransform)val.transform;
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one; vrt.offsetMin = new Vector2(42, 0); vrt.offsetMax = new Vector2(-12, 0);
            var vtmp = val.GetComponent<TextMeshProUGUI>();
            vtmp.alignment = TextAlignmentOptions.Left; vtmp.font = tokens.NumbersFont; vtmp.fontStyle = FontStyles.Bold;
            vtmp.fontSize = 18; vtmp.color = Color.white; vtmp.text = "0"; vtmp.raycastTarget = false;

            return pill;
        }
    }
}
