using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10A — Mannequin tap-reactive hit reaction (custom assets Ajwad).
    ///
    /// Subscribe à <see cref="GameEvents.OnTapResolved"/> et joue le hit sequence :
    /// - Top : rotation -15°→+12°→-8°→0° OutElastic (wobble effet ressort)
    /// - Base : statique
    /// - Mannequin parent : position shake ±5px sur 0.10s
    /// - Top Image : flash white 0.05s
    ///
    /// Throttle 0.10s. Sous-éléments lookup via Transform.Find — null-safe.
    /// </summary>
    [DisallowMultipleComponent]
    public class MannequinAnimator : MonoBehaviour
    {
        private Transform _top;
        private Image _topImage;
        private Color _topOriginalColor;
        private Vector3 _originalLocalPos;
        private float _lastHitTime = -10f;

        private void Awake()
        {
            _top = transform.Find("Top");
            _originalLocalPos = transform.localPosition;
            if (_top != null)
            {
                _topImage = _top.GetComponent<Image>();
                if (_topImage != null) _topOriginalColor = _topImage.color;
            }
        }

        private void OnEnable() => GameEvents.OnTapResolved += HandleTap;
        private void OnDisable() => GameEvents.OnTapResolved -= HandleTap;

        private void HandleTap(BigDouble value, float multiplier, Vector2 screenPos)
        {
            if (Time.unscaledTime - _lastHitTime < 0.10f) return;
            _lastHitTime = Time.unscaledTime;
            Hit();
        }

        public void Hit()
        {
            // Top wobble OutElastic (-15° → +12° → -8° → 0°).
            if (_top != null)
            {
                _top.DOKill();
                var topSeq = DOTween.Sequence();
                topSeq.Append(_top.DOLocalRotate(new Vector3(0, 0, -15f), 0.05f).SetEase(Ease.OutQuad));
                topSeq.Append(_top.DOLocalRotate(new Vector3(0, 0, 12f), 0.08f).SetEase(Ease.OutQuad));
                topSeq.Append(_top.DOLocalRotate(new Vector3(0, 0, -8f), 0.07f).SetEase(Ease.OutQuad));
                topSeq.Append(_top.DOLocalRotate(Vector3.zero, 0.20f).SetEase(Ease.OutElastic));
                topSeq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }

            // Mannequin parent shake ±5px horizontal.
            transform.DOKill();
            var shakeSeq = DOTween.Sequence();
            shakeSeq.Append(transform.DOLocalMove(_originalLocalPos + new Vector3(5f, 0f, 0f), 0.04f).SetEase(Ease.OutQuad));
            shakeSeq.Append(transform.DOLocalMove(_originalLocalPos + new Vector3(-3f, 0f, 0f), 0.04f).SetEase(Ease.OutQuad));
            shakeSeq.Append(transform.DOLocalMove(_originalLocalPos, 0.04f).SetEase(Ease.OutQuad));
            shakeSeq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            // Top Image flash white 0.05s puis retour color originale.
            if (_topImage != null)
            {
                _topImage.color = Color.white;
                var img = _topImage;
                var orig = _topOriginalColor;
                DOTween.Sequence()
                    .AppendInterval(0.06f)
                    .AppendCallback(() => { if (img != null) img.color = orig; })
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }
    }
}
