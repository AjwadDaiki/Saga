using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 3 mannequin: simple sprite that shake-recoils on each tap.
    /// Future stages: swap sprite per stade (poteau brut → palus → cible cérémoniale).
    /// Visual placeholder for now (procedural quad sprite tinted brown).
    /// </summary>
    [DisallowMultipleComponent]
    public class MannequinView : MonoBehaviour
    {
        [SerializeField] private float _shakeStrength = 0.08f;
        [SerializeField] private float _shakeDuration = 0.18f;

        private void OnEnable() => GameEvents.OnTapResolved += HandleTapResolved;
        private void OnDisable() => GameEvents.OnTapResolved -= HandleTapResolved;

        private void HandleTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            // DOShakePosition is a core Transform shortcut in DOTween.dll, no UI module required.
            transform.DOKill(complete: true); // snap any prior shake back
            transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 14, randomness: 90f, snapping: false, fadeOut: true);
        }
    }
}
