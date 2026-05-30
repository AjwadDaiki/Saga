using System.Globalization;
using Saga.Core;
using Saga.Gameplay;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Top-right combo display. Visible only when combo tier > 0.
    /// Shows "xN.N" — the FINAL multiplier including Méditation bonus, not just the base tier.
    /// </summary>
    [DisallowMultipleComponent]
    public class ComboMeterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _group;

        private int _lastTier;
        private float _lastBaseMult = 1f;

        public TextMeshProUGUI Label
        {
            get => _label;
            set => _label = value;
        }

        public CanvasGroup Group
        {
            get => _group;
            set => _group = value;
        }

        private void OnEnable()
        {
            GameEvents.OnComboChanged += HandleComboChanged;
            GameEvents.OnUpgradePurchased += HandleUpgradePurchased;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnComboChanged -= HandleComboChanged;
            GameEvents.OnUpgradePurchased -= HandleUpgradePurchased;
        }

        private void HandleComboChanged(int tier, float baseMultiplier)
        {
            _lastTier = tier;
            _lastBaseMult = baseMultiplier;
            Refresh();
        }

        private void HandleUpgradePurchased(string upgradeId, int newLevel)
        {
            // Méditation purchase tweaks bonus → refresh display even without tier change.
            Refresh();
        }

        private void Refresh()
        {
            if (_label == null) return;

            var gm = GameManager.Instance;
            var bonus = gm != null
                ? StatsCalculator.GetComboMultiplierBonus(gm.State, gm.Content)
                : 1f;
            var finalMult = _lastBaseMult * bonus;

            // Sprint 7.5 zone 4 — signature "×N COMBO" tilted (mockup) instead of plain "x1.0".
            _label.text = "×" + finalMult.ToString("0.0", CultureInfo.InvariantCulture) + " COMBO";

            if (_group != null)
            {
                _group.alpha = _lastTier <= 0 ? 0f : Mathf.Lerp(0.6f, 1f, _lastTier / 3f);
            }
        }
    }
}
