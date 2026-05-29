using Saga.Core;
using Saga.Data;
using Saga.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 refonte zone 2 — top bar : Force pill (or) + Échos pill (violet) + Settings round.
    /// Tous les éléments sont puffy 3D : pill charcoal + floor offset darker + icône colorée
    /// débordante construite en stack 2-Image (cercle outer + inner darker) pour matcher le mockup
    /// sans dépendre de glyphes Unicode que les fonts SDF Latin n'ont pas (力/◆/⚙).
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
        private const float GapPills = 16f;

        public static void Build(BuilderContext ctx)
        {
            // Sprint 7.5 Polish Phase 3 — parent under safe-area-clamped UIRoot so currency pills
            // and settings stay clear of the iPhone notch.
            var parent = ctx.UIRoot != null ? (Transform)ctx.UIRoot : ctx.Canvas.transform;
            BuildForcePill(parent, ctx.Tokens);
            BuildEchosPill(parent, ctx.Tokens);
            BuildSettingsButton(parent, ctx.Tokens);
        }

        // ====================================================================================
        //  FORCE PILL — top-left, gold coin icon, ForceCounterView drives the value label.
        // ====================================================================================

        private static void BuildForcePill(Transform parent, DesignTokens tokens)
        {
            var pill = BuildPillRoot(parent, "ForcePill",
                anchor: new Vector2(0, 1),
                pos: new Vector2(SidePad, TopY),
                size: PillSizeForce);
            pill.AddComponent<ForceCounterView>();

            // Phase 4 §1 — icône or 48 × 48 débordant à gauche.
            var icon = BuildIconCircle(pill.transform, tokens.accentPrimary, DesignTokens.Darken(tokens.accentPrimary, 0.28f), diameter: 48);
            var iconRt = (RectTransform)icon.transform;
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(8, 0);

            // Value label : JetBrains Mono Bold, white, big.
            var label = BuildValueLabel(pill.transform, tokens, leftMargin: 60);
            var view = pill.GetComponent<ForceCounterView>();
            view.Compact = true;
            view.Label = label;
        }

        // ====================================================================================
        //  ÉCHOS PILL — left of settings (alongside Force), violet gem icon.
        // ====================================================================================

        private static void BuildEchosPill(Transform parent, DesignTokens tokens)
        {
            var pill = BuildPillRoot(parent, "EchosPill",
                anchor: new Vector2(0, 1),
                pos: new Vector2(SidePad + PillSizeForce.x + GapPills, TopY),
                size: PillSizeEchos);

            // Phase 4 §1 — icône violet 48 × 48 débordant à gauche, inner rotated diamond.
            var iconViolet = new Color(0.73f, 0.36f, 1.00f, 1f);   // #bb5cff
            var iconDeep = new Color(0.37f, 0.11f, 0.71f, 1f);     // #5f1db4
            var icon = BuildIconCircle(pill.transform, iconViolet, iconDeep, diameter: 48);
            var iconRt = (RectTransform)icon.transform;
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(8, 0);

            // Replace the inner circle with a rotated darker square to evoke a diamond/gem facet.
            var inner = icon.transform.Find("Inner");
            if (inner != null)
            {
                var innerRt = (RectTransform)inner.transform;
                innerRt.localEulerAngles = new Vector3(0, 0, 45f);
                var innerImg = inner.GetComponent<Image>();
                innerImg.sprite = PuffySprite.RoundedFill(3); // tight corners → diamond reads as a gem cut
            }

            // Value label.
            var label = BuildValueLabel(pill.transform, tokens, leftMargin: 60);
            var gm = GameManager.Instance;
            label.text = gm?.State != null ? Saga.Math.NumberFormatter.Format(gm.State.totalEchos) : "0";
        }

        // ====================================================================================
        //  SETTINGS — round puffy button top-right, charcoal "≡" icon (gear glyph fallback).
        // ====================================================================================

        private static void BuildSettingsButton(Transform parent, DesignTokens tokens)
        {
            var btn = SagaButton.Create(parent, "SettingsButton", SagaButton.Variant.Standard,
                DesignTokens.Darken(tokens.panelClair, 0.15f), "", radius: (int)(SettingsSize.x / 2f), floorPx: 5);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-SidePad, TopY);
            rt.sizeDelta = SettingsSize;

            // Phase 4 §1 — gear glyph 3 dashes scaled pour 80 × 80 (vs 60 avant) : 32 × 5 dashes, 12 px gap.
            var face = btn.transform.Find("Face");
            if (face == null) face = btn.transform; // Wrap variant has no Face child
            for (var i = 0; i < 3; i++)
            {
                var dash = new GameObject($"Dash{i}", typeof(RectTransform), typeof(Image));
                dash.transform.SetParent(face, false);
                var drt = (RectTransform)dash.transform;
                drt.anchorMin = new Vector2(0.5f, 0.5f); drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.pivot = new Vector2(0.5f, 0.5f);
                drt.anchoredPosition = new Vector2(0, (i - 1) * 12f);
                drt.sizeDelta = new Vector2(32, 5);
                var dimg = dash.GetComponent<Image>();
                dimg.sprite = PuffySprite.RoundedFill(2);
                dimg.type = Image.Type.Sliced;
                dimg.color = tokens.navyContour;
                dimg.raycastTarget = false;
            }
        }

        // ====================================================================================
        //  HELPERS — shared pill construction blocks.
        // ====================================================================================

        /// <summary>Charcoal pill body + floor (puffy depth). Returns the root pill GameObject.</summary>
        private static GameObject BuildPillRoot(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var tokens = DesignTokens.Get();
            var pill = new GameObject(name, typeof(RectTransform), typeof(Image));
            pill.transform.SetParent(parent, false);
            var rt = (RectTransform)pill.transform;
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(anchor.x, anchor.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var radius = Mathf.RoundToInt(size.y / 2f);
            var bg = pill.GetComponent<Image>();
            bg.sprite = PuffySprite.RoundedFill(radius);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.086f, 0.114f, 0.122f, 0.92f); // charcoal 92%

            // Floor (peeks 6px below).
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(pill.transform, false);
            floor.transform.SetAsFirstSibling();
            var frt = (RectTransform)floor.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(0, -6); frt.offsetMax = Vector2.zero;
            var fi = floor.GetComponent<Image>();
            fi.sprite = PuffySprite.RoundedFill(radius);
            fi.type = Image.Type.Sliced;
            fi.color = tokens.panelSombre2;
            fi.raycastTarget = false;

            // Top gloss bar (subtle white sheen).
            var gloss = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            gloss.transform.SetParent(pill.transform, false);
            var glRt = (RectTransform)gloss.transform;
            glRt.anchorMin = new Vector2(0.05f, 0.55f); glRt.anchorMax = new Vector2(0.95f, 0.92f);
            glRt.offsetMin = Vector2.zero; glRt.offsetMax = Vector2.zero;
            var glImg = gloss.GetComponent<Image>();
            glImg.sprite = PuffySprite.RoundedFill(radius / 2);
            glImg.type = Image.Type.Sliced;
            glImg.color = new Color(1f, 1f, 1f, 0.12f);
            glImg.raycastTarget = false;

            return pill;
        }

        /// <summary>Round colored icon : outer bright disc + inner darker disc (depth). Caller positions it.</summary>
        private static GameObject BuildIconCircle(Transform parent, Color outer, Color inner, int diameter)
        {
            var radius = diameter / 2;
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var rt = (RectTransform)icon.transform;
            rt.sizeDelta = new Vector2(diameter, diameter);

            var outerImg = icon.GetComponent<Image>();
            outerImg.sprite = PuffySprite.RoundedFill(radius);
            outerImg.type = Image.Type.Sliced;
            outerImg.color = outer;
            outerImg.raycastTarget = false;

            // Charcoal outline around the icon (matches saga_target_spec.md "contour noir épais").
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(icon.transform, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, 2);
            olImg.type = Image.Type.Sliced;
            olImg.color = DesignTokens.Get().navyContour;
            olImg.raycastTarget = false;

            // Inner darker disc (the "depth" giving the coin/gem its 3D look).
            var innerDiameter = (int)(diameter * 0.5f);
            var innerR = innerDiameter / 2;
            var innerGo = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            innerGo.transform.SetParent(icon.transform, false);
            var innerRt = (RectTransform)innerGo.transform;
            innerRt.anchorMin = new Vector2(0.5f, 0.5f); innerRt.anchorMax = new Vector2(0.5f, 0.5f);
            innerRt.pivot = new Vector2(0.5f, 0.5f);
            innerRt.anchoredPosition = Vector2.zero;
            innerRt.sizeDelta = new Vector2(innerDiameter, innerDiameter);
            var innerImg = innerGo.GetComponent<Image>();
            innerImg.sprite = PuffySprite.RoundedFill(innerR);
            innerImg.type = Image.Type.Sliced;
            innerImg.color = inner;
            innerImg.raycastTarget = false;

            // Tiny white sparkle top-left of the icon (gloss spéculaire).
            var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(icon.transform, false);
            var srt = (RectTransform)shine.transform;
            srt.anchorMin = new Vector2(0.18f, 0.62f); srt.anchorMax = new Vector2(0.42f, 0.86f);
            srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
            var sImg = shine.GetComponent<Image>();
            sImg.sprite = PuffySprite.RoundedFill(8);
            sImg.type = Image.Type.Sliced;
            sImg.color = new Color(1f, 1f, 1f, 0.5f);
            sImg.raycastTarget = false;

            return icon;
        }

        /// <summary>Big white value label in JetBrains Mono Bold, charcoal outline.</summary>
        private static TextMeshProUGUI BuildValueLabel(Transform parent, DesignTokens tokens, int leftMargin)
        {
            var val = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            val.transform.SetParent(parent, false);
            var rt = (RectTransform)val.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(leftMargin, 0);
            rt.offsetMax = new Vector2(-14, 0);

            var tmp = val.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.font = tokens.NumbersFont;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.text = "0";
            tmp.raycastTarget = false;
            // Charcoal outline like the puffy text recipe (saga_target_spec.md).
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.2f;
            return tmp;
        }
    }
}
