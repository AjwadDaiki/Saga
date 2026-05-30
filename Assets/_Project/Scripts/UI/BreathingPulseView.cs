using DG.Tweening;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.6 polish micro-anim — subtle idle breathing pulse on UI elements
    /// (cards, skill buttons, nav active tab). Scale Sin wave 1.0 ↔ <see cref="_peak"/>
    /// over <see cref="_period"/> seconds, perpetual, paused when GameObject disabled.
    ///
    /// Aware of premium feel "breathing" without dominating the screen. Trigger animations
    /// on user action (bounce, glow) cohabitent — ce view ne lutte pas avec les tweens ad-hoc
    /// car il pilote uniquement <c>localScale</c> sur sa durée propre.
    /// </summary>
    [DisallowMultipleComponent]
    public class BreathingPulseView : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _peak = 1.025f;
        [SerializeField] private float _period = 2.5f;

        public RectTransform Target { get => _target; set => _target = value; }
        public float Peak { get => _peak; set => _peak = value; }
        public float Period { get => _period; set => _period = value; }

        private Tween _tween;

        private void OnEnable()
        {
            if (_target == null) _target = transform as RectTransform;
            _tween?.Kill();
            _target.localScale = Vector3.one;
            _tween = _target.DOScale(_peak, _period * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void OnDisable()
        {
            _tween?.Kill();
            if (_target != null) _target.localScale = Vector3.one;
        }
    }
}
