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
    /// "AFFRONTER UN MAÎTRE" button (Sprint 6). Hidden until the player has at least 1
    /// invocation slot. Click opens the Maître selection modal.
    /// </summary>
    [DisallowMultipleComponent]
    public class AffronterMaitreButtonView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _background;
        [SerializeField] private AffronterMaitreModal _modal;
        [SerializeField] private TextMeshProUGUI _label;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public RectTransform Root { get => _root; set => _root = value; }
        public Image Background { get => _background; set => _background = value; }
        public AffronterMaitreModal Modal { get => _modal; set => _modal = value; }
        public TextMeshProUGUI Label { get => _label; set => _label = value; }

        private bool _isVisible;
        private Tween _glow;

        private void Awake()
        {
            if (_root == null) _root = transform as RectTransform;
            HideInstant();
        }

        private void OnEnable()
        {
            GameEvents.OnMaitreUnlocked += HandleUnlocked;
            GameEvents.OnMaitreSpawned += HandleSpawned;
            GameEvents.OnPrestigeCompleted += HandlePrestigeCompleted;
            RefreshFromState();
        }

        private void OnDisable()
        {
            GameEvents.OnMaitreUnlocked -= HandleUnlocked;
            GameEvents.OnMaitreSpawned -= HandleSpawned;
            GameEvents.OnPrestigeCompleted -= HandlePrestigeCompleted;
            _glow?.Kill();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isVisible) return;
            _modal?.Open();
        }

        private void HandleUnlocked() => RefreshFromState();
        private void HandleSpawned(MaitreData data) => RefreshFromState();
        private void HandlePrestigeCompleted() => RefreshFromState();

        private void RefreshFromState()
        {
            var gm = GameManager.Instance;
            var hasSlot = gm?.State != null && gm.State.maitreInvocationSlots > 0;
            if (hasSlot && !_isVisible) ShowWithPunch();
            else if (!hasSlot && _isVisible) HideWithScale();
        }

        private void ShowWithPunch()
        {
            if (_root == null || _group == null) return;
            _isVisible = true;
            _group.alpha = 1f;
            _group.interactable = true;
            _group.blocksRaycasts = true;

            _root.DOKill();
            _root.localScale = Vector3.zero;
            _root.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            // Permanent golden pulse on background while visible.
            _glow?.Kill();
            if (_background != null)
            {
                _glow = DOTween.To(
                    () => _background.color.a,
                    a => { if (_background == null) return; var c = _background.color; c.a = a; _background.color = c; },
                    0.4f, 0.7f
                ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                 .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        private void HideWithScale()
        {
            if (_root == null || _group == null || !_isVisible) return;
            _isVisible = false;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _glow?.Kill();

            _root.DOKill();
            _root.DOScale(0f, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() => { if (_group != null) _group.alpha = 0f; })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HideInstant()
        {
            _isVisible = false;
            if (_group != null) { _group.alpha = 0f; _group.interactable = false; _group.blocksRaycasts = false; }
            if (_root != null) _root.localScale = Vector3.zero;
        }
    }
}
