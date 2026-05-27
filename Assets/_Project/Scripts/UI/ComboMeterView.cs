using System.Globalization;
using Saga.Core;
using Saga.Gameplay;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Top-right combo display. Visible only when combo tier > 0.
    /// Shows "xN.N" — e.g. "x1.2", "x1.5", "x2.0".
    /// Listens to <see cref="GameEvents.OnComboChanged"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class ComboMeterView : MonoBehaviour
    {
        // Mirror of ComboSystem tier table for display. Keeping it local avoids leaking the table.
        private static readonly float[] _tierMultipliers = { 1.0f, 1.2f, 1.5f, 2.0f };

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _group;

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
            HandleComboChanged(0);
        }

        private void OnDisable()
        {
            GameEvents.OnComboChanged -= HandleComboChanged;
        }

        private void HandleComboChanged(int tier)
        {
            if (_label == null) return;
            var multiplier = tier >= 0 && tier < _tierMultipliers.Length
                ? _tierMultipliers[tier]
                : 1f;
            _label.text = "x" + multiplier.ToString("0.0", CultureInfo.InvariantCulture);

            // Fade in/out via CanvasGroup if available.
            if (_group != null)
            {
                _group.alpha = tier <= 0 ? 0f : Mathf.Lerp(0.6f, 1f, tier / 3f);
            }
        }
    }
}
