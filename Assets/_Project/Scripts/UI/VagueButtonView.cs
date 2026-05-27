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
    /// "VAGUE" button — appears with a punch-scale when <see cref="GameEvents.OnElanFull"/> fires,
    /// hides again when Élan drops below 100% (after trigger or decay).
    /// Click triggers <see cref="Gameplay.VagueResolver.TriggerVague"/> via <see cref="GameManager.Vague"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class VagueButtonView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private RectTransform _root;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image Background { get => _background; set => _background = value; }
        public TextMeshProUGUI Label { get => _label; set => _label = value; }
        public RectTransform Root { get => _root; set => _root = value; }

        private Tween _glow;
        private bool _isVisible;

        private void Awake()
        {
            if (_root == null) _root = transform as RectTransform;
            HideInstant();
        }

        private void OnEnable()
        {
            GameEvents.OnElanFull += HandleElanFull;
            GameEvents.OnElanChanged += HandleElanChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnElanFull -= HandleElanFull;
            GameEvents.OnElanChanged -= HandleElanChanged;
            _glow?.Kill();
        }

        private void HandleElanFull()
        {
            ShowWithPunch();
        }

        private void HandleElanChanged(float current, float max)
        {
            if (_isVisible && current < ElanConstants.ElanMax)
            {
                HideWithScale();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isVisible) return;
            var gm = GameManager.Instance;
            gm?.Vague?.TriggerVague(gm.State);
            gm?.Elan?.ResetForVague(gm.State);
            // Hide will be handled by OnElanChanged when ResetForVague fires (current = 0 < max).
        }

        private void ShowWithPunch()
        {
            if (_root == null || _group == null) return;
            _isVisible = true;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            _group.alpha = 1f;

            _root.DOKill();
            _root.localScale = Vector3.zero;
            _root.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            // Permanent glow pulse on the background while visible.
            _glow?.Kill();
            if (_background != null)
            {
                var baseColor = _background.color;
                _glow = DOTween.To(
                    () => _background.color.a,
                    a => { if (_background == null) return; var c = _background.color; c.a = a; _background.color = c; },
                    Mathf.Lerp(baseColor.a, 0.6f, 0.7f),
                    0.6f
                ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                 .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        private void HideWithScale()
        {
            if (_root == null || _group == null || !_isVisible) return;
            _isVisible = false;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _glow?.Kill();

            _root.DOKill();
            _root.DOScale(0f, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() => { if (_group != null) _group.alpha = 0f; })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HideInstant()
        {
            _isVisible = false;
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.blocksRaycasts = false;
                _group.interactable = false;
            }
            if (_root != null) _root.localScale = Vector3.zero;
        }
    }
}
