using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 3 mannequin: simple sprite that shake-recoils on each tap during Training.
    /// Sprint 4: phase-aware — hides when combat starts, fades back in on return to Training.
    /// Future stages: swap sprite per stade (poteau brut → palus → cible cérémoniale).
    /// </summary>
    [DisallowMultipleComponent]
    public class MannequinView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private float _shakeStrength = 0.08f;
        [SerializeField] private float _shakeDuration = 0.18f;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTapResolved;
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTapResolved;
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandleTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            // Only react to taps during Training — adversaire has its own shake during combat.
            var gm = GameManager.Instance;
            if (gm?.State != null && gm.State.currentPhase != CombatPhase.Training) return;

            transform.DOKill(complete: true);
            transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 14, randomness: 90f, snapping: false, fadeOut: true);
        }

        private void HandlePhaseChanged(CombatPhase prev, CombatPhase next)
        {
            if (_renderer == null) return;
            var target = next == CombatPhase.Training ? 1f : 0f;
            DOTween.To(() => _renderer.color.a, a =>
            {
                if (_renderer == null) return;
                var c = _renderer.color; c.a = a; _renderer.color = c;
            }, target, 0.3f).SetEase(Ease.OutQuad).SetTarget(transform);
        }
    }
}
