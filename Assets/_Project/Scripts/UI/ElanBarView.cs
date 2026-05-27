using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Horizontal Élan gauge, visible at all times above the upgrade cards.
    /// Pulses near 100%. Vague button (separate view) sits to the right.
    /// </summary>
    [DisallowMultipleComponent]
    public class ElanBarView : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private RectTransform _pulseTarget;

        public Image FillImage { get => _fillImage; set => _fillImage = value; }
        public TextMeshProUGUI Label { get => _label; set => _label = value; }
        public RectTransform PulseTarget { get => _pulseTarget; set => _pulseTarget = value; }

        private Tween _pulse;

        private void OnEnable()
        {
            GameEvents.OnElanChanged += HandleElanChanged;
            Refresh(0f, ElanConstants.ElanMax);
        }

        private void OnDisable()
        {
            GameEvents.OnElanChanged -= HandleElanChanged;
            _pulse?.Kill();
        }

        private void HandleElanChanged(float current, float max) => Refresh(current, max);

        private void Refresh(float current, float max)
        {
            if (_fillImage != null)
            {
                var ratio = max > 0 ? Mathf.Clamp01(current / max) : 0f;
                _fillImage.fillAmount = ratio;
                UpdatePulse(ratio);
            }
            if (_label != null)
            {
                _label.text = $"ÉLAN {Mathf.RoundToInt(current)}%";
            }
        }

        private void UpdatePulse(float ratio)
        {
            if (_pulseTarget == null) return;
            if (ratio >= 0.9f && (_pulse == null || !_pulse.IsActive() || !_pulse.IsPlaying()))
            {
                _pulse?.Kill();
                _pulseTarget.localScale = Vector3.one;
                _pulse = _pulseTarget.DOScale(1.03f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetTarget(_pulseTarget);
            }
            else if (ratio < 0.9f && _pulse != null && _pulse.IsActive())
            {
                _pulse.Kill();
                _pulse = null;
                _pulseTarget.localScale = Vector3.one;
            }
        }
    }
}
