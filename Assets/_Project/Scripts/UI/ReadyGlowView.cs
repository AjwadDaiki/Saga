using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.6 polish micro-anim — perpetual halo pulse for "ready" states
    /// (VAGUE 100% Élan, Dojo tab active, Affronter Maître unlocked). Drives an Image's
    /// alpha + scale in a soft Sin wave to suggest "tap me" urgency without being aggressive.
    ///
    /// <see cref="SetReady"/> activates the loop. <see cref="SetReady(false)"/> stops + resets alpha 0.
    /// </summary>
    [DisallowMultipleComponent]
    public class ReadyGlowView : MonoBehaviour
    {
        [SerializeField] private Image _glow;
        [SerializeField] private float _minAlpha = 0.20f;
        [SerializeField] private float _maxAlpha = 0.55f;
        [SerializeField] private float _minScale = 1.00f;
        [SerializeField] private float _maxScale = 1.08f;
        [SerializeField] private float _period = 1.0f;

        public Image Glow { get => _glow; set => _glow = value; }

        private Tween _alphaTween;
        private Tween _scaleTween;
        private bool _ready;

        public void SetReady(bool ready)
        {
            if (_glow == null) return;
            if (ready == _ready) return;
            _ready = ready;
            _alphaTween?.Kill();
            _scaleTween?.Kill();
            if (!ready)
            {
                var c = _glow.color;
                _glow.color = new Color(c.r, c.g, c.b, 0f);
                _glow.rectTransform.localScale = Vector3.one;
                return;
            }
            // Start at min alpha + min scale.
            var col = _glow.color;
            _glow.color = new Color(col.r, col.g, col.b, _minAlpha);
            _glow.rectTransform.localScale = Vector3.one * _minScale;

            _alphaTween = UIFadeUtil.Fade(_glow, _maxAlpha, _period * 0.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _scaleTween = _glow.rectTransform.DOScale(_maxScale, _period * 0.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void OnDisable()
        {
            _alphaTween?.Kill();
            _scaleTween?.Kill();
            _ready = false;
        }
    }
}
