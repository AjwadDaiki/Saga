using DG.Tweening;
using Saga.Core;
using Saga.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// "PROCHAIN ADVERSAIRE" bar fixed at the top of the screen.
    /// Fills as the player taps the mannequin (Training phase). Subtle pulse near completion.
    /// Hides itself while in combat phases — irrelevant then.
    /// </summary>
    [DisallowMultipleComponent]
    public class AdversaireProgressBarView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private RectTransform _pulseTarget;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image FillImage { get => _fillImage; set => _fillImage = value; }
        public TextMeshProUGUI Label { get => _label; set => _label = value; }
        public RectTransform PulseTarget { get => _pulseTarget; set => _pulseTarget = value; }

        private Tween _pulse;

        private void OnEnable()
        {
            GameEvents.OnAdversaireProgressUpdated += HandleProgress;
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
            Refresh(0, AdversaireSpawner.GetThresholdForStade(1));
        }

        private void OnDisable()
        {
            GameEvents.OnAdversaireProgressUpdated -= HandleProgress;
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
            _pulse?.Kill();
        }

        private void HandleProgress(int currentTaps, int threshold)
        {
            Refresh(currentTaps, threshold);
        }

        private void HandlePhaseChanged(Saga.Data.CombatPhase prev, Saga.Data.CombatPhase next)
        {
            // Visible only during Training. Combat phases get the CombatHud.
            if (_group == null) return;
            var target = next == Saga.Data.CombatPhase.Training ? 1f : 0f;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, target, 0.25f).SetEase(Ease.OutQuad);
        }

        private void Refresh(int currentTaps, int threshold)
        {
            if (_fillImage != null)
            {
                var ratio = threshold > 0 ? Mathf.Clamp01(currentTaps / (float)threshold) : 0f;
                _fillImage.fillAmount = ratio;
                UpdatePulse(ratio);
            }
            if (_label != null)
            {
                _label.text = $"Prochain adversaire — {currentTaps}/{threshold}";
            }
        }

        private void UpdatePulse(float ratio)
        {
            if (_pulseTarget == null) return;
            if (ratio >= 0.9f && (_pulse == null || !_pulse.IsActive() || !_pulse.IsPlaying()))
            {
                _pulse?.Kill();
                _pulseTarget.localScale = Vector3.one;
                _pulse = _pulseTarget.DOScale(1.04f, 0.5f)
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
