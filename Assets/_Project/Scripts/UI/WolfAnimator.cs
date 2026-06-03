using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — Enemy_Wolf hit-reactive (ennemi en phase Combat).
    ///
    /// API publique :
    ///   - PlayIdle()         : body float Y ±5px sur 1.5s
    ///   - PlayHit()          : body recoil X -8px + head tilt -10° + flash white
    ///   - PlayBossEntrance() : stub Sprint 10B+ (zoom dramatique)
    ///
    /// Auto-subscribe OnTapResolved : tap déclenche PlayHit UNIQUEMENT si combat actif
    /// (CombatPhase != Training) — sinon Mannequin reçoit le hit.
    /// </summary>
    [DisallowMultipleComponent]
    public class WolfAnimator : MonoBehaviour
    {
        private Transform _body, _head;
        private Image _bodyImage;
        private Color _bodyOriginalColor = Color.white;
        private Vector3 _bodyPos0, _headRot0;
        private float _lastHitTime = -10f;
        private Tween _idleTween;
        private Sequence _activeHit;

        private void Awake()
        {
            _body = SceneRegistry.FindChildTolerant(transform, "Body") ?? transform;
            _head = SceneRegistry.FindChildTolerant(transform, "Head");

            _bodyPos0 = _body.localPosition;
            if (_head != null) _headRot0 = _head.localEulerAngles;
            _bodyImage = _body.GetComponent<Image>();
            if (_bodyImage != null) _bodyOriginalColor = _bodyImage.color;

            Debug.Log($"[Wolf] Parts — Body:{_body!=null} Head:{_head!=null} BodyImage:{_bodyImage!=null}");
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
            // Wolf reçoit hit uniquement en phase Combat. Sinon Mannequin handle.
            var phase = GameManager.Instance?.State?.currentPhase ?? CombatPhase.Training;
            var isCombat = phase == CombatPhase.AdversaireActive
                        || phase == CombatPhase.CapitaineActive
                        || phase == CombatPhase.MaitreActive;
            if (!isCombat) return;
            if (Time.unscaledTime - _lastHitTime < 0.10f) return;
            _lastHitTime = Time.unscaledTime;
            PlayHit();
        }

        public void PlayIdle()
        {
            KillIdleTween();
            if (_body == null) return;
            _idleTween = _body.DOLocalMoveY(_bodyPos0.y + 5f, 0.75f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void PlayHit()
        {
            KillIdleTween();
            KillActiveHit();
            var seq = DOTween.Sequence();
            if (_body != null)
            {
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x - 8f, 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x + 2f, 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x, 0.08f).SetEase(Ease.OutBack));
            }
            if (_head != null)
            {
                seq.Insert(0f, _head.DOLocalRotate(_headRot0 + new Vector3(0, 0, -10f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.15f, _head.DOLocalRotate(_headRot0, 0.12f).SetEase(Ease.OutBack));
            }
            if (_bodyImage != null)
            {
                _bodyImage.color = Color.white;
                var img = _bodyImage;
                var orig = _bodyOriginalColor;
                DOTween.Sequence().AppendInterval(0.06f).AppendCallback(() => { if (img != null) img.color = orig; })
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeHit = seq;
            ScheduleIdleResume(0.3f);
        }

        public void PlayBossEntrance()
        {
            // TODO Sprint 10B+ : zoom dramatique + écran sombre + apparition wolf boss.
            Debug.Log("[Wolf] PlayBossEntrance stub — Sprint 10B+ implémentera.");
        }

        private void ScheduleIdleResume(float delay)
        {
            DOTween.Sequence().AppendInterval(delay).AppendCallback(PlayIdle)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void KillIdleTween()
        {
            _idleTween?.Kill();
            if (_body != null) _body.localPosition = _bodyPos0;
            if (_head != null) _head.localEulerAngles = _headRot0;
        }

        private void KillActiveHit()
        {
            _activeHit?.Kill();
            _activeHit = null;
        }

        private void KillAllTweens()
        {
            KillIdleTween();
            KillActiveHit();
        }
    }
}
