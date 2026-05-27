using DG.Tweening;
using Saga.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 3 stade transition cinematic. Full-screen dark overlay with a centered title
    /// "Nouveau stade: &lt;Name&gt;" that fades in, holds, fades out. Listens
    /// <see cref="GameEvents.OnStadeChanged"/>.
    ///
    /// 2s total: 0.4s fade-in → 1.2s hold → 0.4s fade-out.
    /// </summary>
    [DisallowMultipleComponent]
    public class StadeTransitionView : MonoBehaviour
    {
        private static readonly string[] _stadeNames =
        {
            null, "Mendiant", "Apprenti", "Guerrier", "Maître", "Légende", "Mythe"
        };

        [SerializeField] private CanvasGroup _overlay;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _subtitle;

        [SerializeField] private float _fadeInDuration = 0.4f;
        [SerializeField] private float _holdDuration = 1.2f;
        [SerializeField] private float _fadeOutDuration = 0.4f;
        [SerializeField] private float _peakAlpha = 0.75f;

        public CanvasGroup Overlay { get => _overlay; set => _overlay = value; }
        public Image Background { get => _background; set => _background = value; }
        public TextMeshProUGUI Title { get => _title; set => _title = value; }
        public TextMeshProUGUI Subtitle { get => _subtitle; set => _subtitle = value; }

        private Sequence _running;

        private void Awake()
        {
            if (_overlay != null) _overlay.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnStadeChanged += HandleStadeChanged;
        private void OnDisable() => GameEvents.OnStadeChanged -= HandleStadeChanged;

        private void HandleStadeChanged(int previous, int next)
        {
            if (next <= previous) return; // No regression cinematic.
            Play(next);
        }

        public void Play(int newStade)
        {
            if (_overlay == null || _title == null) return;

            _title.text = $"Nouveau stade : {GetStadeName(newStade)}";
            if (_subtitle != null) _subtitle.text = $"Stade {newStade}";

            _running?.Kill();
            _overlay.alpha = 0f;

            _running = DOTween.Sequence();
            _running.Append(DOTween.To(() => _overlay.alpha, a => _overlay.alpha = a, _peakAlpha, _fadeInDuration).SetEase(Ease.OutQuad));
            _running.AppendInterval(_holdDuration);
            _running.Append(DOTween.To(() => _overlay.alpha, a => _overlay.alpha = a, 0f, _fadeOutDuration).SetEase(Ease.InQuad));
        }

        private static string GetStadeName(int stade)
        {
            if (stade >= 1 && stade < _stadeNames.Length) return _stadeNames[stade];
            return "Inconnu";
        }
    }
}
