using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
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
    /// Sprint 7.5 polish:
    ///   - Color + font size pulled from <see cref="DesignTokens"/> so the entire game re-themes
    ///     by editing one SO.
    ///   - Tick scale punch on every OnForceChanged event (visual tap feedback).
    ///   - 10M+ tier gets a soft glow via TMP outline.
    ///   - Optional small "FORCE" label above is set elsewhere (BuildForceCounter) using textSecondary.
    /// </summary>
    [DisallowMultipleComponent]
    public class ForceCounterView : MonoBehaviour
    {
        [Tooltip("Ticker time-constant (seconds). Lower = snappier, higher = smoother. Default 0.12s feels punchy.")]
        [SerializeField] private float _smoothing = 0.12f;

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private bool _compact; // pill mode : fixed font size, no magnitude scaling

        private BigDouble _displayed;
        private BigDouble _target;
        private Tween _tickTween;
        private RectTransform _rect;

        public TextMeshProUGUI Label
        {
            get => _label;
            set => _label = value;
        }

        /// <summary>Sprint 7.5: when true (top-bar pill), keep a fixed font size and skip the big
        /// magnitude scaling — only the color tier + value text update.</summary>
        public bool Compact { get => _compact; set => _compact = value; }

        private void OnEnable()
        {
            _rect = transform as RectTransform;
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

        private void PunchTick()
        {
            if (_rect == null) return;
            _tickTween?.Kill();
            _rect.localScale = Vector3.one;
            _tickTween = _rect.DOPunchScale(Vector3.one * 0.04f, 0.15f, 4, 0.5f)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
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
            PunchTick();
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

            var tokens = DesignTokens.Get();

            if (_compact)
            {
                // Pill mode : white value, fixed size (set at build), no magnitude scaling/glow.
                if (tokens.NumbersFont != null && _label.font != tokens.NumbersFont)
                    _label.font = tokens.NumbersFont;
                return;
            }

            _label.color = tokens.ForceTierColor(compare);
            _label.fontSize = tokens.ForceFontSize(compare);

            // 10M+ tier earns a soft accent glow via TMP outline.
            var huge = compare >= new BigDouble(10_000_000);
            _label.fontStyle = huge ? FontStyles.Bold : FontStyles.Normal;
            _label.outlineColor = new Color(tokens.accentPrimary.r, tokens.accentPrimary.g, tokens.accentPrimary.b, huge ? 0.65f : 0f);
            _label.outlineWidth = huge ? 0.18f : 0f;

            // Use the JetBrains Mono font for the numeric counter.
            if (tokens.NumbersFont != null && _label.font != tokens.NumbersFont)
                _label.font = tokens.NumbersFont;
        }
    }
}
