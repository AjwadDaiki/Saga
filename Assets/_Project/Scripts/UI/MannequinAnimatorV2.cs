using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — Mannequin tap-reactive (custom assets Ajwad).
    ///
    /// API publique :
    ///   - PlayIdle()    : Top oscille -1°/+1° sur 2s, Base breathing scaleY 1.00↔1.02
    ///   - PlayHit()     : Top wobble damped + Base micro-shake + flash white
    ///   - PlayWobble()  : alias PlayHit pour API claire externe
    ///
    /// Auto-subscribe OnTapResolved → PlayHit() throttled 0.10s. Idle loop OnEnable,
    /// resume après tap après 0.4s.
    ///
    /// Note V2 : aucun Shadow custom GO trouvé dans Main.unity V2 — Ajwad les a retirés
    /// ou intégrés au sprite. Code reste défensif (skip si Shadow component présent).
    /// </summary>
    [DisallowMultipleComponent]
    public class MannequinAnimatorV2 : MonoBehaviour
    {
        private Transform _top, _base;
        private Image _topImage;
        private Color _topOriginalColor = Color.white;
        private Vector3 _topPos0, _topRot0Euler, _topScale0;
        private Vector3 _basePos0, _baseScale0;
        private float _lastHitTime = -10f;
        private Tween _idleTop, _idleBase;
        private Sequence _activeHit;

        private void Awake()
        {
            _top = SceneRegistry.FindChildTolerant(transform, "Top");
            _base = SceneRegistry.FindChildTolerant(transform, "Base");

            if (_top != null)
            {
                _topPos0 = _top.localPosition;
                _topRot0Euler = _top.localEulerAngles;
                _topScale0 = _top.localScale;
                _topImage = _top.GetComponent<Image>();
                if (_topImage != null) _topOriginalColor = _topImage.color;
            }
            if (_base != null)
            {
                _basePos0 = _base.localPosition;
                _baseScale0 = _base.localScale;
            }

            Debug.Log($"[MannequinV2] Parts — Top:{_top!=null} Base:{_base!=null} TopImage:{_topImage!=null}");
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTap;
            PlayIdle();
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTap;
            KillAllTweens();
        }

        private void HandleTap(BigDouble value, float multiplier, Vector2 screenPos)
        {
            if (Time.unscaledTime - _lastHitTime < 0.10f) return;
            _lastHitTime = Time.unscaledTime;
            PlayHit();
        }

        public void PlayIdle()
        {
            KillIdleTweens();
            if (_top != null)
            {
                _idleTop = _top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, 1f), 1.0f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            if (_base != null)
            {
                var peakScale = new Vector3(_baseScale0.x, _baseScale0.y * 1.02f, _baseScale0.z);
                _idleBase = _base.DOScale(peakScale, 1.2f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        public void PlayHit()
        {
            KillIdleTweens();
            KillActiveHit();

            var seq = DOTween.Sequence();
            if (_top != null)
            {
                seq.Append(_top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, -15f), 0.05f).SetEase(Ease.OutQuad));
                seq.Append(_top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, 12f), 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, -8f), 0.07f).SetEase(Ease.OutQuad));
                seq.Append(_top.DOLocalRotate(_topRot0Euler, 0.20f).SetEase(Ease.OutElastic));
            }
            if (_base != null)
            {
                seq.Insert(0f, _base.DOLocalMoveX(_basePos0.x + 1.5f, 0.05f));
                seq.Insert(0.05f, _base.DOLocalMoveX(_basePos0.x - 1f, 0.05f));
                seq.Insert(0.10f, _base.DOLocalMoveX(_basePos0.x, 0.05f));
            }
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
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeHit = seq;
            ScheduleIdleResume(0.4f);
        }

        public void PlayWobble() => PlayHit();

        /// <summary>FEATURE 7 — coup massif (combo tier ≥ 2) : Top rotate ±25° + scale squash 0.8/1.15
        /// puis pop back. Base micro-shake amplifié. Override le PlayHit normal pour cette frame.</summary>
        public void PlayMassiveHit()
        {
            KillIdleTweens();
            KillActiveHit();

            var seq = DOTween.Sequence();
            if (_top != null)
            {
                seq.Append(_top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, -25f), 0.06f).SetEase(Ease.OutQuad));
                seq.Join(_top.DOScale(new Vector3(_topScale0.x * 1.15f, _topScale0.y * 0.80f, _topScale0.z), 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, 22f), 0.10f).SetEase(Ease.OutQuad));
                seq.Join(_top.DOScale(new Vector3(_topScale0.x * 0.85f, _topScale0.y * 1.18f, _topScale0.z), 0.10f).SetEase(Ease.OutQuad));
                seq.Append(_top.DOLocalRotate(_topRot0Euler + new Vector3(0, 0, -12f), 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_top.DOLocalRotate(_topRot0Euler, 0.30f).SetEase(Ease.OutElastic));
                seq.Join(_top.DOScale(_topScale0, 0.30f).SetEase(Ease.OutElastic));
            }
            if (_base != null)
            {
                seq.Insert(0f, _base.DOLocalMoveX(_basePos0.x + 4f, 0.06f));
                seq.Insert(0.06f, _base.DOLocalMoveX(_basePos0.x - 3f, 0.06f));
                seq.Insert(0.12f, _base.DOLocalMoveX(_basePos0.x + 1.5f, 0.06f));
                seq.Insert(0.18f, _base.DOLocalMoveX(_basePos0.x, 0.10f));
                seq.Insert(0f, _base.DOScale(new Vector3(_baseScale0.x * 1.10f, _baseScale0.y * 0.85f, _baseScale0.z), 0.08f));
                seq.Insert(0.20f, _base.DOScale(_baseScale0, 0.20f).SetEase(Ease.OutElastic));
            }
            if (_topImage != null)
            {
                _topImage.color = Color.white;
                var img = _topImage;
                var orig = _topOriginalColor;
                DOTween.Sequence()
                    .AppendInterval(0.10f)
                    .AppendCallback(() => { if (img != null) img.color = orig; })
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeHit = seq;
            ScheduleIdleResume(0.7f);
        }

        /// <summary>Read accessor pour Shadow sync (Polish 6 ShadowSyncBridge).</summary>
        public Transform TopTransform => _top;
        public Vector3 TopOriginalRotEuler => _topRot0Euler;

        private void ScheduleIdleResume(float delay)
        {
            DOTween.Sequence().AppendInterval(delay).AppendCallback(PlayIdle)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void KillIdleTweens()
        {
            _idleTop?.Kill(); _idleBase?.Kill();
            if (_top != null) { _top.localPosition = _topPos0; _top.localEulerAngles = _topRot0Euler; _top.localScale = _topScale0; }
            if (_base != null) { _base.localPosition = _basePos0; _base.localScale = _baseScale0; }
        }

        private void KillActiveHit()
        {
            _activeHit?.Kill();
            _activeHit = null;
        }

        private void KillAllTweens()
        {
            KillIdleTweens();
            KillActiveHit();
        }
    }
}
