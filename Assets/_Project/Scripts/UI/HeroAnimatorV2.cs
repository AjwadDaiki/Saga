using System.Collections.Generic;
using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — Hero_Samurai tap-reactive animations sur 7 parties.
    ///
    /// Convention V2 Ajwad (BUG 1 fix) : Weapon (katana) tenu par <b>Hand_Left</b>. Les attack
    /// variants swing avec _handLeft, _handRight fait le balance/anticipation.
    ///
    /// Idle robust (BUG 2/4 fix) : guard <c>_attacking</c> + OnComplete restart explicite.
    /// Plus de race conditions sur les rapid taps. Idle ne tombe jamais en silence.
    ///
    /// Hands "floating boules" (BUG 3 fix) : orbit organique X/Y + rotation Z, périodes
    /// désynchronisées entre Left/Right pour effet chibi magique.
    ///
    /// API : PlayIdle / PlayAttack(int) / PlayHit / PlayVague / PlaySouffle / PlayDeath.
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
        private bool _attacking;
        private readonly List<Tween> _idleTweens = new List<Tween>();
        private Sequence _activeAttack;

        private void Awake()
        {
            _body = SceneRegistry.FindChildTolerant(transform, "Body");
            _head = SceneRegistry.FindChildTolerant(transform, "Head");
            _handLeft = SceneRegistry.FindChildTolerant(transform, "Hand_Left");
            _handRight = SceneRegistry.FindChildTolerant(transform, "Hand_Right");
            _legLeft = SceneRegistry.FindChildTolerant(transform, "Leg_Left");
            _legRight = SceneRegistry.FindChildTolerant(transform, "Leg_Right");

            // BUG 1 fix : Weapon priorité direct child sous Hero, fallback Hand_Left (pas Hand_Right).
            _weapon = SceneRegistry.FindChildTolerant(transform, "Weapon");
            if (_weapon == null && _handLeft != null)
            {
                _weapon = SceneRegistry.FindChildTolerant(_handLeft, "Weapon");
            }

            // Capture originals.
            if (_body != null) { _bodyPos0 = _body.localPosition; _bodyRot0 = _body.localRotation; }
            if (_head != null) { _headPos0 = _head.localPosition; _headRot0 = _head.localRotation; }
            if (_handLeft != null) { _handLeftPos0 = _handLeft.localPosition; _handLeftRot0 = _handLeft.localRotation; }
            if (_handRight != null) { _handRightPos0 = _handRight.localPosition; _handRightRot0 = _handRight.localRotation; }
            if (_weapon != null) { _weaponPos0 = _weapon.localPosition; _weaponRot0 = _weapon.localRotation; _weaponScale0 = _weapon.localScale; }

            Debug.Log($"[HeroV2] Parts — Body:{_body!=null} Head:{_head!=null} HandL:{_handLeft!=null} HandR:{_handRight!=null} LegL:{_legLeft!=null} LegR:{_legRight!=null} Weapon:{_weapon!=null} (held by Hand_Left convention)");
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

        /// <summary>
        /// Loop idle robuste : body float + head bob + hands organique orbit + rotation.
        /// No-op si attack actif (idle reprend automatiquement via OnComplete attack).
        /// </summary>
        public void PlayIdle()
        {
            if (_attacking) return;
            KillIdleTweens();

            // Body float Y ±5px sur 0.8s yoyo infini.
            if (_body != null)
            {
                _idleTweens.Add(_body.DOLocalMoveY(_bodyPos0.y + 5f, 0.80f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy));
            }

            // Head bobbing rotation Z ±2° + slight Y oscillation antiphase avec body.
            if (_head != null)
            {
                _idleTweens.Add(_head.DOLocalRotate(new Vector3(0, 0, 2f), 0.65f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy));
            }

            // BUG 3 fix — Hands "petites boules flottantes" : orbit organique X/Y + rotation.
            // Hand_Left (qui tient katana) : amplitude légère, period 1.0s.
            if (_handLeft != null)
            {
                _idleTweens.Add(_handLeft.DOLocalMove(_handLeftPos0 + new Vector3(4f, 3f, 0f), 1.00f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy));
                _idleTweens.Add(_handLeft.DOLocalRotate(new Vector3(0, 0, 5f), 1.20f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy));
            }
            // Hand_Right (off-hand) : amplitude légèrement différente + period décalé pour async.
            if (_handRight != null)
            {
                _idleTweens.Add(_handRight.DOLocalMove(_handRightPos0 + new Vector3(-5f, -3f, 0f), 0.85f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy));
                _idleTweens.Add(_handRight.DOLocalRotate(new Vector3(0, 0, -5f), 1.10f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy));
            }
        }

        public void PlayAttack(int variant)
        {
            _attacking = true;
            KillIdleTweens();
            KillActiveAttack();

            var seq = DOTween.Sequence();
            switch (variant % 4)
            {
                case 0: BuildSwingHorizontal(seq); break;
                case 1: BuildStabForward(seq); break;
                case 2: BuildDiagonalSlash(seq); break;
                default: BuildDoubleSwing(seq); break;
            }
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            // BUG 2/4 fix — OnComplete relance idle propre. Plus de ScheduleIdleResume time-check race.
            seq.OnComplete(() =>
            {
                _attacking = false;
                PlayIdle();
            });
            _activeAttack = seq;
        }

        public void PlayHit()
        {
            _attacking = true;
            KillIdleTweens();
            var seq = DOTween.Sequence();
            if (_body != null)
            {
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x + 3f, 0.05f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x - 3f, 0.05f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x, 0.05f).SetEase(Ease.OutQuad));
            }
            if (_head != null)
            {
                seq.Join(_head.DOLocalRotate(new Vector3(0, 0, -10f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.10f, _head.DOLocalRotate(_headRot0.eulerAngles, 0.15f).SetEase(Ease.OutBack));
            }
            seq.OnComplete(() => { _attacking = false; PlayIdle(); });
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeAttack = seq;
        }

        public void PlayVague()
        {
            _attacking = true;
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
            seq.OnComplete(() => { _attacking = false; PlayIdle(); });
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeAttack = seq;
        }

        public void PlaySouffle()
        {
            _attacking = true;
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
            seq.OnComplete(() => { _attacking = false; PlayIdle(); });
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeAttack = seq;
        }

        public void PlayDeath()
        {
            Debug.Log("[HeroV2] PlayDeath stub — Sprint 10B+ implémentera.");
        }

        // ===== 4 attack variants — Weapon swung by Hand_Left (BUG 1 fix) =====

        private void BuildSwingHorizontal(Sequence seq)
        {
            if (_body != null)
            {
                seq.Join(_body.DOLocalRotate(new Vector3(0, 0, 8f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.20f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.15f).SetEase(Ease.OutBack));
            }
            // Hand_Left swing wide arc (holds weapon).
            if (_handLeft != null)
            {
                seq.Insert(0f, _handLeft.DOLocalRotate(new Vector3(0, 0, -60f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.08f, _handLeft.DOLocalRotate(new Vector3(0, 0, 70f), 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.20f, _handLeft.DOLocalRotate(_handLeftRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            // Hand_Right balance / anticipation (off-hand pulls back).
            if (_handRight != null)
            {
                seq.Insert(0f, _handRight.DOLocalMoveX(_handRightPos0.x + 12f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.18f, _handRight.DOLocalMoveX(_handRightPos0.x, 0.12f).SetEase(Ease.OutBack));
            }
            if (_weapon != null)
            {
                seq.Insert(0f, _weapon.DOScale(_weaponScale0 * 1.15f, 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.18f, _weapon.DOScale(_weaponScale0, 0.10f).SetEase(Ease.OutBack));
            }
        }

        private void BuildStabForward(Sequence seq)
        {
            if (_body != null)
            {
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x + 15f, 0.10f).SetEase(Ease.OutQuad));
                seq.Append(_body.DOLocalMoveX(_bodyPos0.x, 0.18f).SetEase(Ease.OutBack));
            }
            if (_handLeft != null)
            {
                seq.Insert(0f, _handLeft.DOLocalMoveX(_handLeftPos0.x - 25f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0f, _handLeft.DOLocalRotate(new Vector3(0, 0, 20f), 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.10f, _handLeft.DOLocalMoveX(_handLeftPos0.x, 0.15f).SetEase(Ease.OutBack));
                seq.Insert(0.10f, _handLeft.DOLocalRotate(_handLeftRot0.eulerAngles, 0.15f).SetEase(Ease.OutBack));
            }
            if (_head != null)
            {
                seq.Insert(0f, _head.DOLocalRotate(new Vector3(0, 0, -8f), 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.20f, _head.DOLocalRotate(_headRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
        }

        private void BuildDiagonalSlash(Sequence seq)
        {
            if (_handLeft != null)
            {
                seq.Append(_handLeft.DOLocalRotate(new Vector3(0, 0, 30f), 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_handLeft.DOLocalRotate(new Vector3(0, 0, -90f), 0.18f).SetEase(Ease.OutQuad));
                seq.Append(_handLeft.DOLocalRotate(_handLeftRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            if (_body != null)
            {
                seq.Insert(0f, _body.DOLocalRotate(new Vector3(0, 0, 5f), 0.10f));
                seq.Insert(0f, _body.DOLocalMoveY(_bodyPos0.y - 8f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.25f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.12f).SetEase(Ease.OutBack));
                seq.Insert(0.25f, _body.DOLocalMoveY(_bodyPos0.y, 0.12f).SetEase(Ease.OutBack));
            }
            if (_handRight != null)
            {
                seq.Insert(0f, _handRight.DOLocalMoveX(_handRightPos0.x + 8f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.25f, _handRight.DOLocalMoveX(_handRightPos0.x, 0.12f).SetEase(Ease.OutBack));
            }
        }

        private void BuildDoubleSwing(Sequence seq)
        {
            if (_handLeft != null)
            {
                seq.Append(_handLeft.DOLocalRotate(new Vector3(0, 0, -45f), 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_handLeft.DOLocalRotate(new Vector3(0, 0, 30f), 0.08f).SetEase(Ease.OutQuad));
                seq.AppendInterval(0.05f);
                seq.Append(_handLeft.DOLocalRotate(new Vector3(0, 0, -30f), 0.06f).SetEase(Ease.OutQuad));
                seq.Append(_handLeft.DOLocalRotate(new Vector3(0, 0, 60f), 0.08f).SetEase(Ease.OutQuad));
                seq.Append(_handLeft.DOLocalRotate(_handLeftRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
            if (_body != null)
            {
                seq.Insert(0f, _body.DOLocalRotate(new Vector3(0, 0, 4f), 0.06f));
                seq.Insert(0.20f, _body.DOLocalRotate(new Vector3(0, 0, -2f), 0.08f));
                seq.Insert(0.40f, _body.DOLocalRotate(_bodyRot0.eulerAngles, 0.10f).SetEase(Ease.OutBack));
            }
        }

        // ===== Helpers =====

        /// <summary>Read accessor pour Shadow sync (Polish 6 ShadowSyncBridge).</summary>
        public Transform BodyTransform => _body;
        public Vector3 BodyOriginalPos => _bodyPos0;

        private void KillIdleTweens()
        {
            foreach (var t in _idleTweens) t?.Kill();
            _idleTweens.Clear();
            // Reset parts to original.
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
            _attacking = false;
        }
    }
}
