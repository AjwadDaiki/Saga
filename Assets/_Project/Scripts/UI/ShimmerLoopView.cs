using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.6 E4 — subtle ambient shimmer overlay (idle currency icon polish).
    /// Drives a target Image's alpha through a fade-in / fade-out flash at random intervals
    /// (4–6 s by default) to make the icon "breathe" without dominating the screen.
    ///
    /// Sits on the same GameObject as the target Image and references it via SerializeField.
    /// Stops cleanly via coroutine yield when the host disables.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShimmerLoopView : MonoBehaviour
    {
        [SerializeField] private Image _shine;
        [SerializeField] private float _minDelay = 4f;
        [SerializeField] private float _maxDelay = 6f;
        [SerializeField] private float _peakAlpha = 0.5f;
        [SerializeField] private float _fadeIn = 0.25f;
        [SerializeField] private float _hold = 0.10f;
        [SerializeField] private float _fadeOut = 0.30f;

        public Image Shine { get => _shine; set => _shine = value; }

        private void OnEnable()
        {
            if (_shine == null) return;
            _shine.color = new Color(_shine.color.r, _shine.color.g, _shine.color.b, 0f);
            StartCoroutine(Loop());
        }

        private IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_minDelay, _maxDelay));
                if (_shine == null) yield break;
                _shine.DOKill();
                UIFadeUtil.Fade(_shine, _peakAlpha, _fadeIn).SetEase(Ease.OutQuad)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
                yield return new WaitForSeconds(_fadeIn + _hold);
                if (_shine == null) yield break;
                UIFadeUtil.Fade(_shine, 0f, _fadeOut).SetEase(Ease.InQuad)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
                yield return new WaitForSeconds(_fadeOut);
            }
        }
    }
}
