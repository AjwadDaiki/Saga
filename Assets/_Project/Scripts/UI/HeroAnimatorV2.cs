using System.Collections.Generic;
using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — Hero_Samurai tap-reactive animations sur 7 parties (Body / Head /
    /// Hand_Left / Hand_Right / Leg_Left / Leg_Right / Weapon).
    ///
    /// API publique :
    ///   - PlayIdle()              : loop floating + arms balance + weapon swing subtle
    ///   - PlayAttack(int variant) : 0=Swing horizontal, 1=Stab, 2=Diagonal slash, 3=Double swing
    ///   - PlayHit()               : flash red + body shake + head jerk
    ///   - PlayVague()             : big slash 360° + scale + impact (Sprint 10 Phase 6)
    ///   - PlaySouffle()           : saut Y +30 + tilt back zen (Sprint 10 Phase 6)
    ///   - PlayDeath()             : stub V2 (Sprint 10B+ implémentera)
    ///
    /// Auto-subscribe OnTapResolved → random attack variant. Throttle 0.10s.
    /// Idle loop démarre OnEnable, kill au tap, reprend après cooldown 0.4s.
    ///
    /// Weapon hierarchy défensif : cherche d'abord direct child "Weapon", fallback
    /// "Hand_Right/Weapon" si nested. Tolérance naming via SceneRegistry.FindChildTolerant.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeroAnimatorV2 : MonoBehaviour
    {
        // 7 parts (null-safe — graceful skip si manquant).
        private Transform _body, _head, _handLeft, _handRight, _legLeft, _legRight, _weapon;

        // Captured originals pour reset après animation.
        private Vector3 _bodyPos0, _headPos0, _handLeftPos0, _handRightPos0, _weaponPos0;
        private Quaternion _bodyRot0, _headRot0, _handLeftRot0, _handRightRot0, _weaponRot0;
        private Vector3 _weaponScale0;

        private float _lastAttackTime = -10f;
        private float _idleResumeTime;
        private Tween _idleTween1, _idleTween2, _idleTween3, _idleTween4;
        private Sequence _activeAttack;

        private void Awake()
        {
            _body = SceneRegistry.FindChildTolerant(transform, "Body");
            _head = SceneRegistry.FindChildTolerant(transform, "Head");
            _handLeft = SceneRegistry.FindChildTolerant(transform, "Hand_Left");
            _handRight = SceneRegistry.FindChildTolerant(transform, "Hand_Right");
            _legLeft = SceneRegistry.FindChildTolerant(transform, "Leg_Left");
            _legRight = SceneRegistry.FindChildTolerant(transform, "Leg_Right");

            // Weapon : priorité direct child, fallback nested sous Hand_Right.
            _weapon = SceneRegistry.FindChildTolerant(transform, "Weapon");
            if (_weapon == null && _handRight != null)
            {
                _weapon = SceneRegistry.FindChildTolerant(_handRight, "Weapon");
            }

            // Capture originals.
            if (_body != null) { _bodyPos0 = _body.localPosition; _bodyRot0 = _body.localRotation; }
            if (_head != null) { _headPos0 = _head.localPosition; _headRot0 = _head.localRotation; }
            if (_handLeft != null) { _handLeftPos0 = _handLeft.localPosition; _handLeftRot0 = _handLeft.localRotation; }
            if (_handRight != null) { _handRightPos0 = _handRight.localPosition; _handRightRot0 = _handRight.localRotation; }
            if (_weapon != null) { _weaponPos0 = _weapon.localPosition; _weaponRot0 = _weapon.localRotation; _weaponScale0 = _weapon.localScale; }

            Debug.Log($"[HeroV2] Parts detected — Body:{_body!=null} Head:{_head!=null} HandL:{_handLeft!=null} HandR:{_handRight!=null} LegL:{_legLeft!=null} LegR:{_legRight!=null} Weapon:{_weapon!=null}");
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
            if (Time.unscaledTime - _lastAttackTime < 0.10f) return;
            _lastAttackTime = Time.unscaledTime;
            PlayAttack(Random.Range(0, 4));
        }

        // ===== Public API =====

        /// <summary>Boucle idle : body float Y, head bobbing, hands balance opposite, weapon subtle swing.</summary>
        public void PlayIdle()
        {
            KillIdleTweens();

            if (_body != null)
            {
                _idleTween1 = _body.DOLocalMoveY(_bodyPos0.y + 5f, 0.75f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            if (_head != null)
            {
                _idleTween2 = _head.DOLocalRotate(new Vector3(0, 0, 2f), 0.60f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            if (_handLeft != null && _handRight != null)
            {
                // Balance opposition : main gauche monte tandis que main droite descend.
                _idleTween3 = _handLeft.DOLocalMoveY(_handLeftPos0.y + 3f, 1.0f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
                _idleTween4 = _handRight.DOLocalMoveY(_handRightPos0.y - 3f, 1.0f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            // Weapon léger swing au repos (couplé visuellement avec hand_right idle).
            // Pas de tween dédié pour rester économe — il suit hand_right via parenting si nested.
        }

        /// <summary>Joue une variante d'attack random ou indexée 0-3.</summary>
        public void PlayAttack(int variant)
        {
            KillIdleTweens();
            KillActiveAttack();

            switch (variant % 4)
            {
                case 0: AttackSwingHorizontal(); break;
                case 1: AttackStabForward(); break;
                case 2: AttackDiagonalSlash(); break;
                default: AttackDoubleSwing(); break;
            }
        }

        /// <summary>Hit reaction — body shake + head jerk + flash red (futur via Image color).</summary>
        public void PlayHit()
        {
            KillIdleTweens();
            if (_body != null)
            {
                var seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x + 3f, 0.05f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x - 3f, 0.05f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x, 0.05f).SetEase(Ease.OutQuad));
            }
            if (_head != null)
            {
                _head.DOLocalRotate(new Vector3(0, 0, -10f), 0.08f).SetEase(Ease.OutQuad)
                    .OnComplete(() => _head.DOLocalRotate(_headRot0.eulerAngles, 0.15f).SetEase(Ease.OutBack))
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            ScheduleIdleResume(0.4f);
        }

        /// <summary>VAGUE skill — big slash + scale impact (durée ~1s).</summary>
        public void PlayVague()
        {
            KillIdleTweens();
            KillActiveAttack();
            var seq = DOTween.Sequence();
            if (_weapon != null)
            {
                seq.Append(_weapon.DOScale(_weaponScale0 * 1.5f, 0.15f).SetEase(Ease.OutBack));
                seq.Join(_weapon.DOLocalRotate(new Vector3(0, 0, 360f), 0.4f, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic));
                seq.Append(_weapon.DOScale(_weaponScale0, 0.2f).SetEase(Ease.OutQuad));
            }
            if (_body != null)
            {
                seq.Insert(0f, _body.DOLocalRotate(new Vector3(0, 0, 10f), 0.2f).SetEase(Ease.OutQuad));
                seq.Insert(0.4f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.3f).SetEase(Ease.OutBack));
            }
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeAttack = seq;
            ScheduleIdleResume(0.9f);
        }

        /// <summary>SOUFFLE skill — petit saut Y +30 + tilt back zen (durée ~1s).</summary>
        public void PlaySouffle()
        {
            KillIdleTweens();
            KillActiveAttack();
            var seq = DOTween.Sequence();
            if (_body != null)
            {
                seq.Append(_body.DOLocalMoveY(_bodyPos0.y + 30f, 0.3f).SetEase(Ease.OutQuad));
                seq.Join(_body.DOLocalRotate(new Vector3(0, 0, -5f), 0.3f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveY(_bodyPos0.y, 0.4f).SetEase(Ease.InQuad));
                seq.Join(_body.DOLocalRotate(_bodyRot0.eulerAngles, 0.3f).SetEase(Ease.OutBack));
            }
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeAttack = seq;
            ScheduleIdleResume(0.8f);
        }

        /// <summary>Stub Sprint 10B+ — fall + fade.</summary>
        public void PlayDeath()
        {
            // TODO Sprint 10B : full death animation (collapse + ground impact + fade).
            Debug.Log("[HeroV2] PlayDeath stub — Sprint 10B+ implémentera l'animation complète.");
        }

        // ===== 4 attack variants implementation =====

        private void AttackSwingHorizontal()
        {
            var seq = DOTween.Sequence();
            if (_body != null)
            {
                seq.Join(_body.DOLocalRotate(new Vector3(0, 0, 8f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.20f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.15f).SetEase(Ease.OutBack));
            }
            if (_handRight != null)
            {
                seq.Insert(0f, _handRight.DOLocalRotate(new Vector3(0, 0, -60f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.08f, _handRight.DOLocalRotate(new Vector3(0, 0, 70f), 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.20f, _handRight.DOLocalRotate(_handRightRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            if (_handLeft != null)
            {
                seq.Insert(0f, _handLeft.DOLocalMoveX(_handLeftPos0.x - 12f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.18f, _handLeft.DOLocalMoveX(_handLeftPos0.x, 0.12f).SetEase(Ease.OutBack));
            }
            if (_weapon != null)
            {
                seq.Insert(0f, _weapon.DOScale(_weaponScale0 * 1.15f, 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.18f, _weapon.DOScale(_weaponScale0, 0.10f).SetEase(Ease.OutBack));
            }
            FinalizeAttack(seq, 0.4f);
        }

        private void AttackStabForward()
        {
            var seq = DOTween.Sequence();
            if (_body != null)
            {
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x + 15f, 0.10f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x, 0.18f).SetEase(Ease.OutBack));
            }
            if (_handRight != null)
            {
                seq.Insert(0f, _handRight.DOLocalMoveX(_handRightPos0.x + 25f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0f, _handRight.DOLocalRotate(new Vector3(0, 0, -20f), 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.10f, _handRight.DOLocalMoveX(_handRightPos0.x, 0.15f).SetEase(Ease.OutBack));
                seq.Insert(0.10f, _handRight.DOLocalRotate(_handRightRot0.eulerAngles, 0.15f).SetEase(Ease.OutBack));
            }
            if (_head != null)
            {
                seq.Insert(0f, _head.DOLocalRotate(new Vector3(0, 0, -8f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.20f, _head.DOLocalRotate(_headRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            FinalizeAttack(seq, 0.4f);
        }

        private void AttackDiagonalSlash()
        {
            var seq = DOTween.Sequence();
            if (_handRight != null)
            {
                seq.Append(_handRight.DOLocalRotate(new Vector3(0, 0, -30f), 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_handRight.DOLocalRotate(new Vector3(0, 0, 90f), 0.18f).SetEase(Ease.OutQuad));
                seq.Append(_handRight.DOLocalRotate(_handRightRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            if (_body != null)
            {
                seq.Insert(0f, _body.DOLocalRotate(new Vector3(0, 0, 5f), 0.10f));
                seq.Insert(0f, _body.DOLocalMoveY(_bodyPos0.y - 8f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.25f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.12f).SetEase(Ease.OutBack));
                seq.Insert(0.25f, _body.DOLocalMoveY(_bodyPos0.y, 0.12f).SetEase(Ease.OutBack));
            }
            if (_handLeft != null)
            {
                seq.Insert(0f, _handLeft.DOLocalMoveX(_handLeftPos0.x - 8f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.25f, _handLeft.DOLocalMoveX(_handLeftPos0.x, 0.12f).SetEase(Ease.OutBack));
            }
            FinalizeAttack(seq, 0.45f);
        }

        private void AttackDoubleSwing()
        {
            var seq = DOTween.Sequence();
            if (_handRight != null)
            {
                seq.Append(_handRight.DOLocalRotate(new Vector3(0, 0, -45f), 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_handRight.DOLocalRotate(new Vector3(0, 0, 30f), 0.08f).SetEase(Ease.OutQuad));
                seq.AppendInterval(0.05f);
                seq.Append(_handRight.DOLocalRotate(new Vector3(0, 0, -30f), 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_handRight.DOLocalRotate(new Vector3(0, 0, 60f), 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_handRight.DOLocalRotate(_handRightRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            if (_body != null)
            {
                seq.Insert(0f, _body.DOLocalRotate(new Vector3(0, 0, 4f), 0.06f));
                seq.Insert(0.20f, _body.DOLocalRotate(new Vector3(0, 0, -2f), 0.08f));
                seq.Insert(0.40f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            FinalizeAttack(seq, 0.55f);
        }

        // ===== Helpers =====

        private void FinalizeAttack(Sequence seq, float resumeIdleAfter)
        {
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeAttack = seq;
            ScheduleIdleResume(resumeIdleAfter);
        }

        private void ScheduleIdleResume(float delay)
        {
            _idleResumeTime = Time.unscaledTime + delay;
            DOTween.Sequence().AppendInterval(delay).AppendCallback(() =>
            {
                if (Time.unscaledTime >= _idleResumeTime - 0.01f) PlayIdle();
            }).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void KillIdleTweens()
        {
            _idleTween1?.Kill(); _idleTween2?.Kill(); _idleTween3?.Kill(); _idleTween4?.Kill();
            // Reset parts to original (autres attacks repartent d'une base propre).
            if (_body != null) { _body.localPosition = _bodyPos0; _body.localRotation = _bodyRot0; }
            if (_head != null) { _head.localPosition = _headPos0; _head.localRotation = _headRot0; }
            if (_handLeft != null) { _handLeft.localPosition = _handLeftPos0; _handLeft.localRotation = _handLeftRot0; }
            if (_handRight != null) { _handRight.localPosition = _handRightPos0; _handRight.localRotation = _handRightRot0; }
            if (_weapon != null) { _weapon.localPosition = _weaponPos0; _weapon.localRotation = _weaponRot0; _weapon.localScale = _weaponScale0; }
        }

        private void KillActiveAttack()
        {
            _activeAttack?.Kill();
            _activeAttack = null;
        }

        private void KillAllTweens()
        {
            KillIdleTweens();
            KillActiveAttack();
        }
    }
}
