using DG.Tweening;
using Saga.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Full-screen "TU ES MORT" overlay shown on <see cref="GameEvents.OnPlayerDiedTemporary"/>.
    /// Listens to a pointer click anywhere on the overlay → applies the -10% Force penalty (via
    /// <see cref="Gameplay.CombatProcessor.ResolveDeathTemporary"/>) and fades back out.
    ///
    /// blocksRaycasts is toggled true while visible so the click is captured before reaching
    /// the tap zone underneath.
    /// </summary>
    [DisallowMultipleComponent]
    public class DeathOverlayView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _subtitle;
        [SerializeField] private TextMeshProUGUI _hint;
        [SerializeField] private float _fadeIn = 0.8f;
        [SerializeField] private float _fadeOut = 0.4f;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public TextMeshProUGUI Title { get => _title; set => _title = value; }
        public TextMeshProUGUI Subtitle { get => _subtitle; set => _subtitle = value; }
        public TextMeshProUGUI Hint { get => _hint; set => _hint = value; }

        private Sequence _running;
        private bool _isVisible;

        private void Awake()
        {
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.blocksRaycasts = false;
                _group.interactable = false;
            }
        }

        private void OnEnable() => GameEvents.OnPlayerDiedTemporary += HandlePlayerDiedTemporary;
        private void OnDisable() => GameEvents.OnPlayerDiedTemporary -= HandlePlayerDiedTemporary;

        private void HandlePlayerDiedTemporary()
        {
            if (_group == null) return;
            if (_title != null) _title.text = "TU ES MORT";
            if (_subtitle != null) _subtitle.text = "Tu n'as pas frappé assez vite. Le silence t'a rattrapé.";
            if (_hint != null) _hint.text = "Click pour reprendre";

            _running?.Kill();
            _group.blocksRaycasts = true;
            _group.interactable = true;
            _isVisible = true;
            _running = DOTween.Sequence()
                .Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, _fadeIn).SetEase(Ease.OutQuad));
            _running.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isVisible) return;
            _isVisible = false;

            // Apply penalty + transition back to Training via CombatProcessor.
            var gm = GameManager.Instance;
            gm?.Combat?.ResolveDeathTemporary(gm.State);

            _running?.Kill();
            _running = DOTween.Sequence()
                .Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, _fadeOut).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    if (_group == null) return;
                    _group.blocksRaycasts = false;
                    _group.interactable = false;
                });
            _running.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
