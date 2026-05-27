using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Capitaine death cinematic. Triggered by <see cref="GameEvents.OnCapitaineDefeated"/>.
    /// Sprint 5: flash blanc + citation de mort + "VICTOIRE" + fade.
    /// Sprint 11 polish adds slow motion + loot trail + drum hit.
    /// </summary>
    [DisallowMultipleComponent]
    public class CapitaineDeathView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _flash;
        [SerializeField] private TextMeshProUGUI _citationLabel;
        [SerializeField] private TextMeshProUGUI _victoryLabel;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image Flash { get => _flash; set => _flash = value; }
        public TextMeshProUGUI CitationLabel { get => _citationLabel; set => _citationLabel = value; }
        public TextMeshProUGUI VictoryLabel { get => _victoryLabel; set => _victoryLabel = value; }

        private Sequence _running;

        private void Awake()
        {
            if (_group != null) _group.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnCapitaineDefeated += HandleDefeated;
        private void OnDisable() => GameEvents.OnCapitaineDefeated -= HandleDefeated;

        private void HandleDefeated(CapitaineData data, BigDouble reward)
        {
            if (_group == null || data == null) return;
            if (_citationLabel != null) _citationLabel.text = $"« {data.DeathCitation} »";
            if (_victoryLabel != null) _victoryLabel.text = "VICTOIRE";

            _running?.Kill();
            _group.alpha = 0f;

            // Sequence: white flash → citation hold → victory label → fade out.
            _running = DOTween.Sequence();
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, 0.2f).SetEase(Ease.OutQuad));

            if (_flash != null)
            {
                var startColor = _flash.color; startColor.a = 0f; _flash.color = startColor;
                _running.Insert(0f, DOTween.To(() => _flash.color.a, a =>
                {
                    if (_flash == null) return; var c = _flash.color; c.a = a; _flash.color = c;
                }, 1f, 0.15f).SetEase(Ease.OutQuad));
                _running.Insert(0.15f, DOTween.To(() => _flash.color.a, a =>
                {
                    if (_flash == null) return; var c = _flash.color; c.a = a; _flash.color = c;
                }, 0f, 0.35f).SetEase(Ease.InQuad));
            }

            _running.AppendInterval(2.2f);
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, 0.4f).SetEase(Ease.InQuad));
            _running.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
