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
    /// Sprint 1 uses TMP default font (LiberationSans). Replace with JetBrains Mono SDF
    /// at polish for tabular digit width — see DESIGN_DECISIONS_LOG.md 2026-05-27.
    /// </summary>
    [DisallowMultipleComponent]
    public class ForceCounterView : MonoBehaviour
    {
        [Tooltip("Ticker time-constant (seconds). Lower = snappier, higher = smoother. Default 0.12s feels punchy.")]
        [SerializeField] private float _smoothing = 0.12f;

        [SerializeField] private TextMeshProUGUI _label;

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
        }
    }
}
