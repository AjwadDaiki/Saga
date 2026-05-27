using BreakInfinity;
using Saga.Core;
using Saga.Math;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Top-center Force counter. Subscribes to <see cref="GameEvents.OnForceChanged"/> and
    /// animates a "displayed" value toward the new target via exponential smoothing
    /// (per 02_GAME_DESIGN §1 — "ticker, jamais teleport").
    ///
    /// Sprint 3 number juice: font size + color scale with the magnitude
    /// (gris &lt; 100 → blanc &lt; 10k → ambre &lt; 10M → coral 10M+).
    /// </summary>
    [DisallowMultipleComponent]
    public class ForceCounterView : MonoBehaviour
    {
        [Tooltip("Ticker time-constant (seconds). Lower = snappier, higher = smoother. Default 0.12s feels punchy.")]
        [SerializeField] private float _smoothing = 0.12f;

        [SerializeField] private TextMeshProUGUI _label;

        // Tier palette per 05_VISUAL_STYLE.md (text-secondary / text-primary / accent-primary / accent-warm).
        private static readonly Color TierGris  = new Color(0.53f, 0.53f, 0.53f, 1f); // #888
        private static readonly Color TierBlanc = new Color(0.98f, 0.98f, 0.98f, 1f); // #fafafa
        private static readonly Color TierAmbre = new Color(0.98f, 0.78f, 0.46f, 1f); // #FAC775
        private static readonly Color TierCoral = new Color(0.99f, 0.45f, 0.20f, 1f); // ~#993C1D-ish

        // Font size by magnitude tier.
        private const float SizeSmall = 84f;   // < 100
        private const float SizeMid   = 96f;   // < 10k
        private const float SizeLarge = 108f;  // < 10M
        private const float SizeHuge  = 124f;  // 10M+

        private BigDouble _displayed;
        private BigDouble _target;

        public TextMeshProUGUI Label
        {
            get => _label;
            set => _label = value;
        }

        private void OnEnable()
        {
            GameEvents.OnForceChanged += HandleForceChanged;
            HandleForceChanged();
            // Snap on first show (no animation from 0).
            _displayed = _target;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnForceChanged -= HandleForceChanged;
        }

        private void Update()
        {
            // Exponential smoothing — frame-rate independent.
            var alpha = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.001f, _smoothing));
            _displayed += (_target - _displayed) * alpha;
            Refresh();
        }

        private void HandleForceChanged()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;
            _target = gm.State.force;
        }

        private void Refresh()
        {
            if (_label == null) return;
            _label.text = NumberFormatter.Format(_displayed);

            ApplyMagnitudeStyling(_displayed);
        }

        private void ApplyMagnitudeStyling(BigDouble value)
        {
            // Use absolute value for the styling decision (Force is always ≥ 0 in practice).
            var compare = value;
            if (value.Sign() < 0) compare = -value;

            Color color;
            float size;
            if      (compare < 100)        { color = TierGris;  size = SizeSmall; }
            else if (compare < 10_000)     { color = TierBlanc; size = SizeMid;   }
            else if (compare < 10_000_000) { color = TierAmbre; size = SizeLarge; }
            else                            { color = TierCoral; size = SizeHuge;  }

            _label.color = color;
            _label.fontSize = size;
        }
    }
}
