using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using Saga.Math;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// One upgrade card. Self-builds its UI hierarchy on <see cref="Init"/>.
    ///
    /// Sprint 7.5 polish — glass-morphism row layout for portrait :
    ///   [ Icon 80x80 ] [ Name (h3) + Description (body) ] [ Cost (h2 numbers) ] [ Lvl (small top-right) ]
    ///
    /// Visual states (all colors from <see cref="DesignTokens"/>):
    /// - Default     : bg_card with subtle white 10% border, button interactable based on affordability
    /// - Near-miss   : bg pulses warm tint when player has ≥80% of cost
    /// - Disabled    : button non-interactable + dim text when can't afford and not near-miss
    /// </summary>
    [DisallowMultipleComponent]
    public class UpgradeCardView : MonoBehaviour
    {
        private UpgradeData _data;
        private TextMeshProUGUI _nameLabel;
        private TextMeshProUGUI _effectLabel;
        private TextMeshProUGUI _levelLabel;
        private TextMeshProUGUI _costLabel;
        private Image _background;
        private Image _border;
        private Image _icon;
        private Button _button;
        private RectTransform _rect;

        public void Init(UpgradeData data)
        {
            _data = data;
            _rect = transform as RectTransform;
            BuildHierarchy();
            Refresh();
        }

        private void OnEnable()
        {
            GameEvents.OnForceChanged += Refresh;
            GameEvents.OnUpgradePurchased += HandleUpgradePurchased;
        }

        private void OnDisable()
        {
            GameEvents.OnForceChanged -= Refresh;
            GameEvents.OnUpgradePurchased -= HandleUpgradePurchased;
        }

        private void HandleUpgradePurchased(string upgradeId, int newLevel)
        {
            if (_data != null && upgradeId == _data.UpgradeId && _rect != null)
            {
                // Punch scale on successful buy. DOPunchScale is in DOTween core (ShortcutExtensions).
                _rect.DOKill();
                _rect.localScale = Vector3.one;
                _rect.DOPunchScale(Vector3.one * 0.08f, 0.28f, 6, 0.5f)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            Refresh();
        }

        private void OnBuyClicked()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null || _data == null) return;
            gm.Upgrades?.TryPurchase(gm.State, _data.UpgradeId);
        }

        private void Refresh()
        {
            if (_data == null || _nameLabel == null) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;

            var level = gm.Upgrades.GetLevel(gm.State, _data.UpgradeId);
            var cost = gm.Upgrades.GetCostForNextLevel(gm.State, _data.UpgradeId);
            var canAfford = gm.Upgrades.CanAfford(gm.State, _data.UpgradeId);

            _nameLabel.text = _data.DisplayName;
            _effectLabel.text = _data.DescriptionFr;
            _levelLabel.text = level > 0 ? $"Lvl {level}" : "—";
            _costLabel.text = NumberFormatter.Format(cost);

            var tokens = DesignTokens.Get();
            var nearMiss = !canAfford && IsNearMiss(gm.State.force, cost);
            _background.color = nearMiss
                ? new Color(tokens.accentAction.r * 0.4f, tokens.accentAction.g * 0.3f, tokens.accentAction.b * 0.2f, 0.7f)
                : new Color(tokens.bgCard.r, tokens.bgCard.g, tokens.bgCard.b, 0.65f);
            _costLabel.color = canAfford
                ? tokens.accentSuccess
                : (nearMiss ? tokens.accentPrimary : tokens.textDisabled);
            if (_icon != null) _icon.color = canAfford ? tokens.accentPrimary : tokens.textSecondary;

            if (_button != null) _button.interactable = canAfford;
        }

        private static bool IsNearMiss(BigDouble force, BigDouble cost)
        {
            if (cost.Sign() <= 0) return false;
            // pct = force / cost. Near-miss = [0.8, 1.0).
            var pct = (force / cost).ToDouble();
            return pct >= 0.8 && pct < 1.0;
        }

        // ----- Hierarchy build (runtime UI gen for Sprint 2 bootstrap) ---

        private void BuildHierarchy()
        {
            var tokens = DesignTokens.Get();

            // Background (glass : semi-transparent surface card).
            _background = gameObject.GetComponent<Image>();
            if (_background == null) _background = gameObject.AddComponent<Image>();
            _background.color = new Color(tokens.bgCard.r, tokens.bgCard.g, tokens.bgCard.b, 0.65f);
            _background.raycastTarget = true;

            // Subtle white-10% border on top (1px effective at this scale).
            var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(transform, false);
            var borderRt = (RectTransform)borderGo.transform;
            borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero; borderRt.offsetMax = Vector2.zero;
            _border = borderGo.GetComponent<Image>();
            _border.color = new Color(1f, 1f, 1f, 0.10f);
            _border.raycastTarget = false;
            // Sprint 11 polish: real 1px border via 9-sliced sprite. For now we use a thin filled rect overlay
            // and rely on the bg card behind to create the perceived border by inset. Set Image alpha low.
            _border.color = new Color(1, 1, 1, 0); // hidden until we have a 9-slice ; leave neutral

            // Button — entire card is clickable.
            _button = gameObject.GetComponent<Button>();
            if (_button == null) _button = gameObject.AddComponent<Button>();
            _button.targetGraphic = _background;
            _button.onClick.AddListener(OnBuyClicked);

            // Icon (left, 80px square, vertically centered).
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(transform, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(16, 0);
            iconRt.sizeDelta = new Vector2(80, 80);
            _icon = iconGo.GetComponent<Image>();
            _icon.sprite = CreateUpgradeIcon(_data?.UpgradeId);
            _icon.color = tokens.accentPrimary;
            _icon.raycastTarget = false;
            _icon.preserveAspect = true;

            // Name (top of info column).
            _nameLabel = CreateLabel("Name", new Vector2(0, 0.55f), new Vector2(0.7f, 1f),
                new Vector2(0, 0), new Vector2(0, 0),
                TextAlignmentOptions.MidlineLeft, tokens.fontH3, tokens.textPrimary, paddingX: 0);
            _nameLabel.fontStyle = FontStyles.Bold;
            // Manual offset since CreateLabel doesn't take padding on the icon side.
            var nameRt = (RectTransform)_nameLabel.transform;
            nameRt.offsetMin = new Vector2(108, 0); nameRt.offsetMax = new Vector2(-16, -4);

            // Description (below name).
            _effectLabel = CreateLabel("Effect", new Vector2(0, 0), new Vector2(0.7f, 0.55f),
                new Vector2(0, 0), new Vector2(0, 0),
                TextAlignmentOptions.TopLeft, tokens.fontBody, tokens.textSecondary, paddingX: 0);
            var effectRt = (RectTransform)_effectLabel.transform;
            effectRt.offsetMin = new Vector2(108, 8); effectRt.offsetMax = new Vector2(-16, 0);
            _effectLabel.textWrappingMode = TextWrappingModes.Normal;

            // Cost (right side, big numbers).
            _costLabel = CreateLabel("Cost", new Vector2(0.7f, 0), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, 0),
                TextAlignmentOptions.Center, tokens.fontH2, tokens.accentPrimary, paddingX: 0);
            _costLabel.fontStyle = FontStyles.Bold;
            _costLabel.font = tokens.NumbersFont;
            var costRt = (RectTransform)_costLabel.transform;
            costRt.offsetMin = new Vector2(0, 8); costRt.offsetMax = new Vector2(-16, -8);

            // Level chip (top-right corner).
            _levelLabel = CreateLabel("Level", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-16, -10), new Vector2(80, 24),
                TextAlignmentOptions.TopRight, tokens.fontSmall, tokens.textSecondary, paddingX: 0);
            _levelLabel.font = tokens.NumbersFont;
            _levelLabel.fontStyle = FontStyles.Bold;
        }

        /// <summary>
        /// Procedural icon for an upgrade — simple geometric glyph (human silhouette / crossed swords /
        /// breath spiral). Sprint 11 will swap these for real Aseprite icons.
        /// </summary>
        private static Sprite CreateUpgradeIcon(string upgradeId)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var clear = new Color32(0, 0, 0, 0);
            var solid = new Color32(255, 255, 255, 255);
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = clear;

            switch (upgradeId)
            {
                case "disciple":
                    // Human silhouette : head circle + torso/legs rect.
                    DrawCircle(pixels, size, 32, 50, 8, solid);
                    DrawRect(pixels, size, 22, 24, 42, 44, solid);
                    DrawRect(pixels, size, 24, 8, 30, 22, solid);
                    DrawRect(pixels, size, 34, 8, 40, 22, solid);
                    break;
                case "frappe":
                    // Crossed swords (X shape).
                    DrawLine(pixels, size, 12, 12, 52, 52, solid, thickness: 3);
                    DrawLine(pixels, size, 52, 12, 12, 52, solid, thickness: 3);
                    break;
                case "meditation":
                    // Concentric breath spiral.
                    DrawCircleOutline(pixels, size, 32, 32, 24, solid, thickness: 2);
                    DrawCircleOutline(pixels, size, 32, 32, 16, solid, thickness: 2);
                    DrawCircleOutline(pixels, size, 32, 32, 8, solid, thickness: 2);
                    break;
                default:
                    // Fallback : centered dot.
                    DrawCircle(pixels, size, 32, 32, 12, solid);
                    break;
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            s.name = $"UpgradeIcon_{upgradeId}";
            return s;
        }

        private static void DrawCircle(Color32[] px, int size, int cx, int cy, int r, Color32 c)
        {
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = x - cx; var dy = y - cy;
                    if (dx * dx + dy * dy <= r * r) px[y * size + x] = c;
                }
        }

        private static void DrawCircleOutline(Color32[] px, int size, int cx, int cy, int r, Color32 c, int thickness)
        {
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = x - cx; var dy = y - cy;
                    var d2 = dx * dx + dy * dy;
                    if (d2 <= r * r && d2 >= (r - thickness) * (r - thickness)) px[y * size + x] = c;
                }
        }

        private static void DrawRect(Color32[] px, int size, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (var y = y0; y <= y1; y++)
                for (var x = x0; x <= x1; x++)
                    if (x >= 0 && x < size && y >= 0 && y < size) px[y * size + x] = c;
        }

        private static void DrawLine(Color32[] px, int size, int x0, int y0, int x1, int y1, Color32 c, int thickness)
        {
            // Simple Bresenham + thickness via square footprint.
            var dx = Mathf.Abs(x1 - x0);
            var dy = Mathf.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx - dy;
            var x = x0; var y = y0;
            var t = thickness / 2;
            while (true)
            {
                for (var ix = -t; ix <= t; ix++)
                for (var iy = -t; iy <= t; iy++)
                {
                    var xx = x + ix; var yy = y + iy;
                    if (xx >= 0 && xx < size && yy >= 0 && yy < size) px[yy * size + xx] = c;
                }
                if (x == x1 && y == y1) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
        }

        private TextMeshProUGUI CreateLabel(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta, TextAlignmentOptions alignment,
            float fontSize, Color color, float paddingX = 0)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            // Pivot mirrors anchors for clean placement.
            rt.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            if (paddingX > 0)
            {
                rt.offsetMin = new Vector2(paddingX, rt.offsetMin.y);
                rt.offsetMax = new Vector2(-paddingX, rt.offsetMax.y);
            }

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = alignment;
            label.fontSize = fontSize;
            label.color = color;
            label.raycastTarget = false;
            label.text = "";
            return label;
        }
    }
}
