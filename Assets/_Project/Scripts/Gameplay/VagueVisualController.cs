using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.Gameplay
{
    /// <summary>
    /// Visual feedback for a Vague trigger: full-screen flash + camera shake + giant slash.
    /// Sprint 5 basic — Sprint 11 polish adds shader fullscreen + particles + slow-mo.
    /// Listens to <see cref="GameEvents.OnVagueTriggered"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class VagueVisualController : MonoBehaviour
    {
        [SerializeField] private Image _flashImage;
        [SerializeField] private float _flashDuration = 0.4f;
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeStrength = 0.25f;

        public Image FlashImage { get => _flashImage; set => _flashImage = value; }

        private void OnEnable() => GameEvents.OnVagueTriggered += HandleVagueTriggered;
        private void OnDisable() => GameEvents.OnVagueTriggered -= HandleVagueTriggered;

        private void HandleVagueTriggered(CombatPhase phase)
        {
            DoFlash();
            ShakeCamera();
        }

        private void DoFlash()
        {
            if (_flashImage == null) return;
            var c = _flashImage.color;
            c.a = 0f;
            _flashImage.color = c;

            var seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => _flashImage.color.a, a =>
            {
                if (_flashImage == null) return;
                var col = _flashImage.color; col.a = a; _flashImage.color = col;
            }, 1f, _flashDuration * 0.3f).SetEase(Ease.OutQuad));
            seq.Append(DOTween.To(() => _flashImage.color.a, a =>
            {
                if (_flashImage == null) return;
                var col = _flashImage.color; col.a = a; _flashImage.color = col;
            }, 0f, _flashDuration * 0.7f).SetEase(Ease.InQuad));
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void ShakeCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            // Native DOShakePosition is on Transform (core DOTween shortcut).
            // SetLink to OUR gameObject (not the camera's) so the shake dies cleanly when this
            // controller is unloaded — the camera typically outlives any single scene reload.
            cam.transform.DOKill();
            var basePos = cam.transform.localPosition;
            cam.transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 18, randomness: 90f, snapping: false, fadeOut: true)
                .OnComplete(() => { if (cam != null) cam.transform.localPosition = basePos; })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
