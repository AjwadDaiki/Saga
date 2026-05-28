using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Cinematic d'arrivée d'un Maître (Sprint 6). Triggered by <see cref="GameEvents.OnMaitreSpawned"/>.
    /// 2.5s total: vignette épaisse + arena tint + lettrage MASSIF + citation.
    /// </summary>
    [DisallowMultipleComponent]
    public class MaitreIntroView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _vignette;
        [SerializeField] private Image _arenaTint;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _subtitleLabel;
        [SerializeField] private TextMeshProUGUI _citationLabel;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image Vignette { get => _vignette; set => _vignette = value; }
        public Image ArenaTint { get => _arenaTint; set => _arenaTint = value; }
        public TextMeshProUGUI NameLabel { get => _nameLabel; set => _nameLabel = value; }
        public TextMeshProUGUI SubtitleLabel { get => _subtitleLabel; set => _subtitleLabel = value; }
        public TextMeshProUGUI CitationLabel { get => _citationLabel; set => _citationLabel = value; }

        private Sequence _running;

        private void Awake()
        {
            if (_group != null) _group.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnMaitreSpawned += HandleSpawned;
        private void OnDisable() => GameEvents.OnMaitreSpawned -= HandleSpawned;

        private void HandleSpawned(MaitreData data)
        {
            if (_group == null || data == null) return;
            if (_nameLabel != null) _nameLabel.text = data.DisplayName.ToUpperInvariant();
            if (_subtitleLabel != null) _subtitleLabel.text = $"Maître {data.Voie}";
            if (_citationLabel != null) _citationLabel.text = $"« {data.IntroCitation} »";

            // Apply arena tint (the bigger backdrop wash).
            if (_arenaTint != null)
            {
                var c = data.ArenaBackgroundColor; c.a = 0.4f;
                _arenaTint.color = c;
            }

            _running?.Kill();
            _group.alpha = 0f;

            _running = DOTween.Sequence();
            // Fade in vignette + content
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, 0.5f).SetEase(Ease.OutQuad));
            // Hold
            _running.AppendInterval(1.6f);
            // Fade out
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, 0.4f).SetEase(Ease.InQuad));
            _running.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
