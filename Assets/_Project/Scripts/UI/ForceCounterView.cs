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
    /// Top-bar / top-center Force counter. Subscribes to <see cref="GameEvents.OnForceChanged"/>.
    ///
    /// Sprint 7.6 polish micro-anim (compact pill mode) :
    ///   - E2 number rolling : DOTween.To drives <c>_displayed</c> from previous → new target with
    ///     adaptive duration (small delta = 0.2s snap, big jump = up to 1.0s satisfying roll).
    ///   - E1 squash & stretch bounce : on significant change (≥ 5% relative OR ≥ 100 absolute delta),
    ///     throttled to once / 0.4s, gain plays 1.0→1.08→0.95→1.0 OutBack, loss plays 1.0→0.92→1.0.
    ///
    /// Big counter mode (Compact = false) keeps the legacy <see cref="PunchTick"/> + exponential
    /// smoothing — same UX it had before Sprint 7.6.
    /// </summary>
    [DisallowMultipleComponent]
    public class ForceCounterView : MonoBehaviour
    {
        [Tooltip("Big-counter ticker time-constant (seconds). Compact pill uses DOTween rolling instead.")]
        [SerializeField] private float _smoothing = 0.12f;

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private bool _compact;
        [SerializeField] private UnityEngine.UI.Image _glow; // E3 — halo derrière le pill, pulse on change.

        private BigDouble _displayed;
        private BigDouble _target;
        private Tween _tickTween;
        private Tween _rollTween;
        private RectTransform _rect;
        private float _lastBounceTime = -10f;
        private double _rollFrom;
        private double _rollTo;

        public TextMeshProUGUI Label
        {
            get => _label;
            set => _label = value;
        }

        public bool Compact { get => _compact; set => _compact = value; }
        public UnityEngine.UI.Image Glow { get => _glow; set => _glow = value; }

        private void OnEnable()
        {
            _rect = transform as RectTransform;
            GameEvents.OnForceChanged += HandleForceChanged;
            // Snap on first show.
            var gm = GameManager.Instance;
            _target = gm?.State != null ? gm.State.force : new BigDouble(0);
            _displayed = _target;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnForceChanged -= HandleForceChanged;
            _rollTween?.Kill();
            _tickTween?.Kill();
        }

        private void Update()
        {
            // Compact mode = DOTween-driven, skip exponential smoothing here (would fight the tween).
            if (_compact) return;
            // Big counter mode (non-Compact) uses exponential smoothing — legacy UX preserved.
            var alpha = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.001f, _smoothing));
            _displayed += (_target - _displayed) * alpha;
            Refresh();
        }

        private void HandleForceChanged()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;
            var prevTarget = _target;
            _target = gm.State.force;

            if (_compact)
            {
                // ----- E2 : DOTween-driven counter rolling -----
                _rollFrom = _displayed.ToDouble();
                _rollTo = _target.ToDouble();
                var deltaAbs = System.Math.Abs(_rollTo - _rollFrom);
                // Adaptive duration : small delta = 0.2s snap, big delta = up to 1.0s satisfaction.
                var duration = deltaAbs > 1.0
                    ? Mathf.Clamp(Mathf.Log10((float)System.Math.Max(1.0, deltaAbs)) * 0.18f + 0.25f, 0.25f, 1.0f)
                    : 0.2f;
                _rollTween?.Kill();
                var startVal = _rollFrom;
                _rollTween = DOTween.To(() => startVal, x =>
                {
                    startVal = x;
                    _displayed = new BigDouble(x);
                    Refresh();
                }, _rollTo, duration)
                    .SetEase(Ease.OutCubic)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

                // ----- E1 : squash & stretch on significant change (throttled) -----
                if (Time.unscaledTime - _lastBounceTime > 0.4f)
                {
                    var prevDouble = prevTarget.ToDouble();
                    var relChange = deltaAbs / System.Math.Max(1.0, System.Math.Abs(prevDouble));
                    var significant = relChange > 0.05 || deltaAbs > 100.0;
                    if (significant)
                    {
                        Bounce(_rollTo >= _rollFrom ? +1 : -1);
                        _lastBounceTime = Time.unscaledTime;
                    }
                }
            }
            else
            {
                PunchTick();
            }
        }

        // ----- Animations -----

        private void PunchTick()
        {
            if (_rect == null) return;
            _tickTween?.Kill();
            _rect.localScale = Vector3.one;
            _tickTween = _rect.DOPunchScale(Vector3.one * 0.04f, 0.15f, 4, 0.5f)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        /// <summary>E1 — squash & stretch bounce. +1 = gain (overshoot up), -1 = loss (compression).</summary>
        private void Bounce(int direction)
        {
            if (_rect == null) return;
            _tickTween?.Kill();
            _rect.localScale = Vector3.one;
            var seq = DOTween.Sequence();
            if (direction >= 0)
            {
                // Gain : 1.0 → 1.08 → 0.95 → 1.0 cartoony OutBack.
                seq.Append(_rect.DOScale(1.08f, 0.10f).SetEase(Ease.OutQuad));
                seq.Append(_rect.DOScale(0.95f, 0.10f).SetEase(Ease.InOutQuad));
                seq.Append(_rect.DOScale(1.0f, 0.10f).SetEase(Ease.OutBack));
            }
            else
            {
                // Loss : 1.0 → 0.92 → 1.0 (compression).
                seq.Append(_rect.DOScale(0.92f, 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_rect.DOScale(1.0f, 0.12f).SetEase(Ease.OutBack));
            }
            _tickTween = seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            // E3 — glow halo pulse alpha 0 → 0.55 → 0 + scale 1.0 → 1.18 sur ~0.25s.
            if (_glow != null)
            {
                var glowRt = _glow.rectTransform;
                _glow.DOKill();
                glowRt.DOKill();
                var baseColor = _glow.color;
                _glow.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
                glowRt.localScale = Vector3.one;
                var gseq = DOTween.Sequence();
                gseq.Join(_glow.DOFade(0.55f, 0.10f).SetEase(Ease.OutQuad));
                gseq.Join(glowRt.DOScale(1.18f, 0.10f).SetEase(Ease.OutQuad));
                gseq.Append(_glow.DOFade(0f, 0.15f).SetEase(Ease.InQuad));
                gseq.Join(glowRt.DOScale(1.0f, 0.15f).SetEase(Ease.InQuad));
                gseq.SetLink(_glow.gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        // ----- Display -----

        private void Refresh()
        {
            if (_label == null) return;
            // M2-fix P9 : pill mode = 1 decimal (compact "12.3K" vs verbose "12.34K").
            _label.text = _compact ? NumberFormatter.Format(_displayed, 1) : NumberFormatter.Format(_displayed);
            ApplyMagnitudeStyling(_displayed);
        }

        private void ApplyMagnitudeStyling(BigDouble value)
        {
            var compare = value;
            if (value.Sign() < 0) compare = -value;

            var tokens = DesignTokens.Get();

            if (_compact)
            {
                // Pill mode : couleur figée par builder, taille gérée par autoSizing TMP.
                if (tokens.NumbersFont != null && _label.font != tokens.NumbersFont)
                    _label.font = tokens.NumbersFont;
                return;
            }

            _label.color = tokens.ForceTierColor(compare);
            _label.fontSize = tokens.ForceFontSize(compare);

            var huge = compare >= new BigDouble(10_000_000);
            _label.fontStyle = huge ? FontStyles.Bold : FontStyles.Normal;
            _label.outlineColor = new Color(tokens.accentPrimary.r, tokens.accentPrimary.g, tokens.accentPrimary.b, huge ? 0.65f : 0f);
            _label.outlineWidth = huge ? 0.18f : 0f;

            if (tokens.NumbersFont != null && _label.font != tokens.NumbersFont)
                _label.font = tokens.NumbersFont;
        }
    }
}
