using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10A — Hero_Samurai tap-reactive animations (custom assets Ajwad).
    ///
    /// Subscribe à <see cref="GameEvents.OnTapResolved"/> et joue l'attack sequence :
    /// - Weapon (Katana) : rotation -45°→+60°→0° ~0.18s + scale 2.5D oscillate
    /// - Body : rotation +5°→0° ~0.20s (tilt forward)
    /// - Head : rotation -3° + scale brief 1.05 retour ~0.15s
    ///
    /// Throttle 0.10s entre 2 attacks pour éviter spam si player tap rapide.
    /// Sous-éléments lookup via Transform.Find — null-safe (skip si manquant).
    /// </summary>
    [DisallowMultipleComponent]
    public class HeroAnimator : MonoBehaviour
    {
        private Transform _body;
        private Transform _head;
        private Transform _weapon;
        private float _lastAttackTime = -10f;

        private void Awake()
        {
            _body = transform.Find("Body");
            _head = transform.Find("Head");
            _weapon = transform.Find("Weapon") ?? transform.Find("Weapons") ?? transform.Find("Katana");
        }

        private void OnEnable() => GameEvents.OnTapResolved += HandleTap;
        private void OnDisable() => GameEvents.OnTapResolved -= HandleTap;

        private void HandleTap(BigDouble value, float multiplier, Vector2 screenPos)
        {
            if (Time.unscaledTime - _lastAttackTime < 0.10f) return;
            _lastAttackTime = Time.unscaledTime;
            Attack();
        }

        public void Attack()
        {
            // Weapon : swing diagonal -45° → +60° → 0° avec scale 2.5D.
            if (_weapon != null)
            {
                _weapon.DOKill();
                _weapon.localScale = Vector3.one;
                var weaponSeq = DOTween.Sequence();
                weaponSeq.Append(_weapon.DOLocalRotate(new Vector3(0, 0, -45f), 0.05f).SetEase(Ease.OutQuad));
                weaponSeq.Append(_weapon.DOLocalRotate(new Vector3(0, 0, 60f), 0.08f).SetEase(Ease.OutQuad));
                weaponSeq.Append(_weapon.DOLocalRotate(Vector3.zero, 0.05f).SetEase(Ease.OutBack));
                // Scale 2.5D oscillate parallèle (rapid squash sur axe X, stretch Y).
                weaponSeq.Join(_weapon.DOScale(new Vector3(0.75f, 1.05f, 1f), 0.06f).SetEase(Ease.OutQuad));
                weaponSeq.Insert(0.06f, _weapon.DOScale(Vector3.one, 0.10f).SetEase(Ease.OutBack));
                weaponSeq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }

            // Body : tilt forward +5° → 0°.
            if (_body != null)
            {
                _body.DOKill();
                var bodySeq = DOTween.Sequence();
                bodySeq.Append(_body.DOLocalRotate(new Vector3(0, 0, 5f), 0.08f).SetEase(Ease.OutQuad));
                bodySeq.Append(_body.DOLocalRotate(Vector3.zero, 0.12f).SetEase(Ease.OutBack));
                bodySeq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }

            // Head : tilt -3° + scale 1.05 brief retour.
            if (_head != null)
            {
                _head.DOKill();
                _head.localScale = Vector3.one;
                var headSeq = DOTween.Sequence();
                headSeq.Append(_head.DOLocalRotate(new Vector3(0, 0, -3f), 0.06f).SetEase(Ease.OutQuad));
                headSeq.Join(_head.DOScale(1.05f, 0.06f).SetEase(Ease.OutQuad));
                headSeq.Append(_head.DOLocalRotate(Vector3.zero, 0.09f).SetEase(Ease.OutBack));
                headSeq.Join(_head.DOScale(1f, 0.09f).SetEase(Ease.OutBack));
                headSeq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }
    }
}
