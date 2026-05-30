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
    /// Sprint 7.5 refonte zone 6 — slim driver for the puffy upgrade cards built by
    /// <see cref="Builders.UpgradesBuilder"/>. Replaces the legacy <see cref="UpgradeCardView"/>
    /// which self-built its own hierarchy (glass-morphism era, pre-Vibrant Quest).
    ///
    /// Responsibilities :
    ///   - Bind labels (name, level, description, cost) to the upgrade SO
    ///   - React to GameEvents (Force, purchases) and refresh affordability
    ///   - Wire the cost-pill button click → <c>UpgradeService.TryPurchase</c>
    ///   - Punch-scale on successful buy + dim cost label when unaffordable
    /// </summary>
    [DisallowMultipleComponent]
    public class UpgradeCardDriver : MonoBehaviour
    {
        private UpgradeData _data;
        private Image _bodyImage;
        private Image _iconImage;
        private TextMeshProUGUI _nameLabel;
        private TextMeshProUGUI _effectLabel;
        private TextMeshProUGUI _levelLabel;
        private TextMeshProUGUI _costLabel;
        private Button _costButton;
        private RectTransform _rect;
        private CanvasGroup _costGroup;

        public void Bind(UpgradeData data, Image body, TextMeshProUGUI nameLbl,
            TextMeshProUGUI effectLbl, TextMeshProUGUI levelLbl, TextMeshProUGUI costLbl,
            Button costBtn, Image iconImg)
        {
            _data = data;
            _bodyImage = body;
            _nameLabel = nameLbl;
            _effectLabel = effectLbl;
            _levelLabel = levelLbl;
            _costLabel = costLbl;
            _costButton = costBtn;
            _iconImage = iconImg;
            _rect = transform as RectTransform;

            if (_costButton != null)
            {
                _costButton.onClick.RemoveAllListeners();
                _costButton.onClick.AddListener(OnBuyClicked);
            }

            Refresh();
        }

        private void OnEnable()
        {
            GameEvents.OnForceChanged += Refresh;
            GameEvents.OnUpgradePurchased += HandlePurchased;
        }

        private void OnDisable()
        {
            GameEvents.OnForceChanged -= Refresh;
            GameEvents.OnUpgradePurchased -= HandlePurchased;
        }

        private void HandlePurchased(string upgradeId, int newLevel)
        {
            if (_data != null && upgradeId == _data.UpgradeId && _rect != null)
            {
                _rect.DOKill();
                _rect.localScale = Vector3.one;
                _rect.DOPunchScale(Vector3.one * 0.05f, 0.24f, 6, 0.5f)
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
            if (_effectLabel != null) _effectLabel.text = _data.DescriptionFr;
            if (_levelLabel != null) _levelLabel.text = $"Lv.{level}";
            if (_costLabel != null) _costLabel.text = NumberFormatter.Format(cost);

            if (_costButton != null) _costButton.interactable = canAfford;

            // Dim the cost label when unaffordable.
            if (_costLabel != null)
            {
                var alpha = canAfford ? 1f : 0.55f;
                var c = _costLabel.color;
                _costLabel.color = new Color(c.r, c.g, c.b, alpha);
            }
        }

        private static bool IsNearMiss(BigDouble force, BigDouble cost)
        {
            if (cost.Sign() <= 0) return false;
            var pct = (force / cost).ToDouble();
            return pct >= 0.8 && pct < 1.0;
        }
    }
}
