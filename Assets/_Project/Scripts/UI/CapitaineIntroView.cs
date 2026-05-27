using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Capitaine arrival cinematic. Triggered by <see cref="GameEvents.OnCapitaineSpawned"/>.
    /// Sprint 5 basic: vignette darken + name + citation + fade out. Sprint 11 polish adds
    /// letter-by-letter reveal + drum hit + arène fade-in.
    /// </summary>
    [DisallowMultipleComponent]
    public class CapitaineIntroView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _vignette;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _subtitleLabel;
        [SerializeField] private TextMeshProUGUI _citationLabel;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image Vignette { get => _vignette; set => _vignette = value; }
        public TextMeshProUGUI NameLabel { get => _nameLabel; set => _nameLabel = value; }
        public TextMeshProUGUI SubtitleLabel { get => _subtitleLabel; set => _subtitleLabel = value; }
        public TextMeshProUGUI CitationLabel { get => _citationLabel; set => _citationLabel = value; }

        private Sequence _running;

        private void Awake()
        {
            if (_group != null) _group.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnCapitaineSpawned += HandleSpawned;
        private void OnDisable() => GameEvents.OnCapitaineSpawned -= HandleSpawned;

        private void HandleSpawned(CapitaineData data)
        {
            if (_group == null || data == null) return;
            if (_nameLabel != null) _nameLabel.text = data.DisplayName.ToUpperInvariant();
            if (_subtitleLabel != null) _subtitleLabel.text = $"Capitaine {data.Voie}";
            if (_citationLabel != null) _citationLabel.text = $"« {data.IntroCitation} »";

            _running?.Kill();
            _group.alpha = 0f;

            _running = DOTween.Sequence();
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, 0.5f).SetEase(Ease.OutQuad));
            _running.AppendInterval(1.5f);
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, 0.5f).SetEase(Ease.InQuad));
        }
    }
}
