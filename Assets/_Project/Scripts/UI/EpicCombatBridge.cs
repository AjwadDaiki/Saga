using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 FEATURE 7 — orchestrates "epic" combat reactions on OnTapResolved :
    ///   - Canvas shake scaled by current combo tier (tier 0 = 3px/0.10s ... tier 3 = 12px/0.25s).
    ///   - Hero body lean Z proportional to tap X offset relative to canvas center.
    ///   - Mannequin massive hit on tier ≥ 2 taps (±25° + squash &amp; stretch).
    ///
    /// Attached to MainCanvas by MainSceneBootstrap. Configure() wires the canvas RectTransform,
    /// Hero animator, and Mannequin animator. Throttled 0.08s to avoid stacking shakes.
    /// </summary>
    [DisallowMultipleComponent]
    public class EpicCombatBridge : MonoBehaviour
    {
        private RectTransform _canvasRt;
        private HeroAnimatorV2 _hero;
        private MannequinAnimatorV2 _mannequin;

        private int _currentTier;
        private float _lastShakeTime = -10f;
        private Vector3 _canvasOrigPos;

        public void Configure(Canvas canvas, HeroAnimatorV2 hero, MannequinAnimatorV2 mannequin)
        {
            _canvasRt = canvas != null ? canvas.transform as RectTransform : null;
            if (_canvasRt != null) _canvasOrigPos = _canvasRt.localPosition;
            _hero = hero;
            _mannequin = mannequin;
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTapResolved;
            GameEvents.OnComboChanged += HandleComboChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTapResolved;
            GameEvents.OnComboChanged -= HandleComboChanged;
        }

        private void HandleComboChanged(int tier, float baseMultiplier) => _currentTier = tier;

        private void HandleTapResolved(BigDouble value, float multiplier, Vector2 screenPos)
        {
            if (Time.unscaledTime - _lastShakeTime < 0.08f) return;
            _lastShakeTime = Time.unscaledTime;

            ShakeCanvasForTier(_currentTier);
            DispatchBodyLean(screenPos);
            DispatchMannequinHit(_currentTier);
        }

        private void ShakeCanvasForTier(int tier)
        {
            if (_canvasRt == null) return;
            float strength, duration;
            switch (Mathf.Clamp(tier, 0, 3))
            {
                case 0: strength = 3f;  duration = 0.10f; break;
                case 1: strength = 5f;  duration = 0.15f; break;
                case 2: strength = 8f;  duration = 0.20f; break;
                default: strength = 12f; duration = 0.25f; break;
            }
            _canvasRt.DOKill();
            _canvasRt.localPosition = _canvasOrigPos;
            _canvasRt.DOShakePosition(duration, strength, vibrato: 14, randomness: 90f, snapping: false, fadeOut: true)
                .OnComplete(() => { if (_canvasRt != null) _canvasRt.localPosition = _canvasOrigPos; })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void DispatchBodyLean(Vector2 screenPos)
        {
            if (_hero == null) return;
            var screenWidth = Mathf.Max(1f, Screen.width);
            // Map screen X to [-1..+1] around center; lean Z ∈ [+8°..-8°] (right tap = right lean).
            var t = Mathf.Clamp((screenPos.x - screenWidth * 0.5f) / (screenWidth * 0.5f), -1f, 1f);
            _hero.PlayBodyLean(-t * 8f);
        }

        private void DispatchMannequinHit(int tier)
        {
            if (_mannequin == null) return;
            if (tier >= 2) _mannequin.PlayMassiveHit();
        }
    }
}
