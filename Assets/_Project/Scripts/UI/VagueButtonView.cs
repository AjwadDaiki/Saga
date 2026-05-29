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
        [SerializeField] private bool _alwaysVisible;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public Image Background { get => _background; set => _background = value; }
        public TextMeshProUGUI Label { get => _label; set => _label = value; }
        public RectTransform Root { get => _root; set => _root = value; }

        /// <summary>
        /// Sprint 7.5 refonte zone 5 : when true, the button stays fully visible at all times
        /// (puffy skill slot in the bottom row). The view only gates click trigger on _isVisible
        /// (Élan ≥ max) instead of alpha. When false (legacy), the button popups/hides on Élan full.
        /// </summary>
        public bool AlwaysVisible { get => _alwaysVisible; set => _alwaysVisible = value; }

        private Tween _glow;
        private Tween _scalePulse;
        private bool _isVisible;

        private void Awake()
        {
            if (_root == null) _root = transform as RectTransform;
            if (!_alwaysVisible) HideInstant();
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
            if (_alwaysVisible)
            {
                // In refonte mode the slot is always painted; we only flip the "is ready to fire" flag.
                _isVisible = current >= ElanConstants.ElanMax;
                if (_group != null) _group.interactable = _isVisible;
                return;
            }
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
            if (_alwaysVisible) return; // in refonte mode, no scale punch or glow yoyo — gauge does the telegraphy

            _root.DOKill();
            _root.localScale = Vector3.zero;
            _root.DOScale(1f, 0.35f).SetEase(Ease.OutBack)
                .OnComplete(StartReadyPulse)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            // Sprint 7.5 A5: stronger glow — wider alpha range (up to 1.0) so the CTA clearly reads "READY".
            _glow?.Kill();
            if (_background != null)
            {
                _glow = DOTween.To(
                    () => _background.color.a,
                    a => { if (_background == null) return; var c = _background.color; c.a = a; _background.color = c; },
                    1.0f,
                    0.7f
                ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                 .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        private void StartReadyPulse()
        {
            if (_root == null) return;
            _scalePulse?.Kill();
            _root.localScale = Vector3.one;
            // Subtle 1.0 ↔ 1.06 breathing so the button feels alive while waiting for the tap.
            _scalePulse = _root.DOScale(1.06f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HideWithScale()
        {
            if (_alwaysVisible) return;
            if (_root == null || _group == null || !_isVisible) return;
            _isVisible = false;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _glow?.Kill();
            _scalePulse?.Kill();

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
