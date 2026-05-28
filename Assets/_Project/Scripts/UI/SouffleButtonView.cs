using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Top-left "Souffle" button (Sprint 6). Hidden while cooldown > 0, fades in when ready.
    /// While Meditating: shows a breathing icon. While buffing: shows buff timer. While cooldown:
    /// shows remaining seconds.
    /// </summary>
    [DisallowMultipleComponent]
    public class SouffleButtonView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private RectTransform _root;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image Background { get => _background; set => _background = value; }
        public TextMeshProUGUI Label { get => _label; set => _label = value; }
        public RectTransform Root { get => _root; set => _root = value; }

        private bool _isClickableNow = true;

        private void Awake()
        {
            if (_root == null) _root = transform as RectTransform;
        }

        private void OnEnable()
        {
            GameEvents.OnSouffleCooldownUpdated += HandleCooldown;
            GameEvents.OnSouffleStarted += HandleStarted;
            GameEvents.OnSouffleEnded += HandleEnded;
            GameEvents.OnSouffleBuffStarted += HandleBuffStarted;
            GameEvents.OnSouffleBuffEnded += HandleBuffEnded;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnSouffleCooldownUpdated -= HandleCooldown;
            GameEvents.OnSouffleStarted -= HandleStarted;
            GameEvents.OnSouffleEnded -= HandleEnded;
            GameEvents.OnSouffleBuffStarted -= HandleBuffStarted;
            GameEvents.OnSouffleBuffEnded -= HandleBuffEnded;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isClickableNow) return;
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.Souffle?.TryStartMeditation(gm.State);
        }

        private void HandleCooldown(float remaining, float total)
        {
            if (_label == null) return;
            if (remaining > 0f)
            {
                _isClickableNow = false;
                if (_group != null) { _group.alpha = 0.4f; _group.interactable = false; _group.blocksRaycasts = false; }
                _label.text = $"SOUFFLE\n{Mathf.CeilToInt(remaining)}s";
            }
            else
            {
                // Ready
                Refresh();
            }
        }

        private void HandleStarted()
        {
            _isClickableNow = false;
            if (_label != null) _label.text = "MÉDITATION";
            if (_group != null) { _group.alpha = 0.7f; _group.interactable = false; _group.blocksRaycasts = false; }
        }

        private void HandleEnded()
        {
            // Buff starts next; HandleBuffStarted will take over.
        }

        private void HandleBuffStarted(float duration)
        {
            _isClickableNow = false;
            if (_label != null) _label.text = $"+50%\n{Mathf.CeilToInt(duration)}s";
            if (_group != null) { _group.alpha = 0.9f; _group.interactable = false; _group.blocksRaycasts = false; }
        }

        private void HandleBuffEnded()
        {
            // Cooldown still running — show countdown via HandleCooldown.
        }

        private void Refresh()
        {
            if (_label == null) return;
            _isClickableNow = true;
            _label.text = "SOUFFLE";
            if (_group != null)
            {
                _group.alpha = 1f;
                _group.interactable = true;
                _group.blocksRaycasts = true;
            }
            if (_root != null)
            {
                _root.DOKill();
                _root.localScale = Vector3.one * 0.8f;
                _root.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }
    }
}
