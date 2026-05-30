using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.6 polish micro-anim — tactile press feedback for any clickable UI element
    /// that isn't a <see cref="SagaButton"/> (cards, custom buttons, nav tabs).
    /// Down : scale 0.95 in 0.08s (OutQuad). Up : scale 1.0 with overshoot 1.05 in 0.18s (OutBack).
    /// Doesn't conflict with <see cref="BreathingPulseView"/> because it tweens the same
    /// transform — DOTween latest tween on same target wins, breathing resumes on next yoyo.
    /// </summary>
    [DisallowMultipleComponent]
    public class PressBounceView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _pressScale = 0.95f;
        [SerializeField] private float _releaseOvershoot = 1.05f;
        [SerializeField] private float _downDuration = 0.08f;
        [SerializeField] private float _upDuration = 0.18f;

        public RectTransform Target { get => _target; set => _target = value; }

        private void Awake()
        {
            if (_target == null) _target = transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData _)
        {
            if (_target == null) return;
            _target.DOKill();
            _target.DOScale(_pressScale, _downDuration).SetEase(Ease.OutQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (_target == null) return;
            _target.DOKill();
            var seq = DOTween.Sequence();
            seq.Append(_target.DOScale(_releaseOvershoot, _upDuration * 0.6f).SetEase(Ease.OutQuad));
            seq.Append(_target.DOScale(1f, _upDuration * 0.4f).SetEase(Ease.OutBack));
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
