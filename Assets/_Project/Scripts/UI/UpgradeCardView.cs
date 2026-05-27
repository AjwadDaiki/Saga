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
    /// One upgrade card. Self-builds its UI hierarchy on <see cref="Init"/> for the runtime-bootstrap
    /// scene (Sprint 2). Will be replaced by scene-authored prefab when MCP stabilizes (Sprint 3+).
    ///
    /// Visual states:
    /// - Default     : bg-card #161616, button interactable based on affordability
    /// - Near-miss   : bg pulses ambre when player has ≥80% of cost (NM UX per 02_GAME_DESIGN §3)
    /// - Disabled    : button non-interactable + dim text when can't afford and not near-miss
    /// </summary>
    [DisallowMultipleComponent]
    public class UpgradeCardView : MonoBehaviour
    {
        // Palette per 05_VISUAL_STYLE.md
        private static readonly Color BgCard = new Color(0.086f, 0.086f, 0.086f, 1f);     // #161616
        private static readonly Color BgNearMiss = new Color(0.18f, 0.14f, 0.08f, 1f);    // warm ambre tint
        private static readonly Color BorderSubtle = new Color(0.165f, 0.165f, 0.165f, 1f); // #2a2a2a
        private static readonly Color TextPrimary = new Color(0.98f, 0.98f, 0.98f, 1f);   // #fafafa
        private static readonly Color TextSecondary = new Color(0.53f, 0.53f, 0.53f, 1f); // #888
        private static readonly Color TextTertiary = new Color(0.33f, 0.33f, 0.33f, 1f);  // #555
        private static readonly Color AccentPrimary = new Color(0.98f, 0.78f, 0.46f, 1f); // #FAC775
        private static readonly Color CostAffordable = AccentPrimary;
        private static readonly Color CostNearMiss = new Color(0.99f, 0.85f, 0.55f, 1f);

        private UpgradeData _data;
        private TextMeshProUGUI _nameLabel;
        private TextMeshProUGUI _effectLabel;
        private TextMeshProUGUI _levelLabel;
        private TextMeshProUGUI _costLabel;
        private Image _background;
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

            var nearMiss = !canAfford && IsNearMiss(gm.State.force, cost);
            _background.color = nearMiss ? BgNearMiss : BgCard;
            _costLabel.color = canAfford ? CostAffordable : (nearMiss ? CostNearMiss : TextSecondary);

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
            // Background
            _background = gameObject.GetComponent<Image>();
            if (_background == null) _background = gameObject.AddComponent<Image>();
            _background.color = BgCard;
            _background.raycastTarget = true;

            // Button — entire card is clickable
            _button = gameObject.GetComponent<Button>();
            if (_button == null) _button = gameObject.AddComponent<Button>();
            _button.targetGraphic = _background;
            _button.onClick.AddListener(OnBuyClicked);

            // Name label (top-left)
            _nameLabel = CreateLabel("Name", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(16, -16), new Vector2(180, 36),
                TextAlignmentOptions.TopLeft, 28, TextPrimary);

            // Level (top-right, secondary)
            _levelLabel = CreateLabel("Level", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-16, -16), new Vector2(100, 30),
                TextAlignmentOptions.TopRight, 22, TextSecondary);

            // Effect description (middle)
            _effectLabel = CreateLabel("Effect", new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(0, 0), new Vector2(-32, 60),
                TextAlignmentOptions.Center, 18, TextTertiary, paddingX: 16);
            _effectLabel.textWrappingMode = TextWrappingModes.Normal;

            // Cost (bottom-center, accented)
            _costLabel = CreateLabel("Cost", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 20), new Vector2(220, 40),
                TextAlignmentOptions.Center, 30, CostAffordable);
            _costLabel.fontStyle = FontStyles.Bold;
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
