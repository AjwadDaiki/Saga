using DG.Tweening;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.5 unified button. Wraps a <see cref="Button"/> + background <see cref="Image"/>
    /// and applies the design system : 3 variants (Standard / Primary / Special), 4 states
    /// (Normal / Hover / Pressed / Disabled), DOTween scale punch on press, optional glow halo.
    ///
    /// Drop on any GO that already has Image + Button + (optional) TextMeshProUGUI label.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image), typeof(Button))]
    public class SagaButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public enum Variant { Standard, Primary, Special }

        [SerializeField] private Variant _variant = Variant.Standard;
        [SerializeField] private Color _voieTint = Color.clear; // used only by Special variant
        [SerializeField] private Image _background;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _glowHalo; // optional ; auto-created for Primary variant

        public Variant ButtonVariant
        {
            get => _variant;
            set { _variant = value; ApplyStyle(); }
        }

        public Color VoieTint
        {
            get => _voieTint;
            set { _voieTint = value; ApplyStyle(); }
        }

        public TextMeshProUGUI Label => _label;
        public Button UnityButton => _button;

        private Tween _pulseTween;
        private RectTransform _rect;

        private void Awake()
        {
            if (_background == null) _background = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();
            if (_label == null) _label = GetComponentInChildren<TextMeshProUGUI>(true);
            _rect = (RectTransform)transform;
            ApplyStyle();
        }

        private void OnEnable() => RefreshInteractableState();

        public void ApplyStyle()
        {
            var t = DesignTokens.Get();
            if (_background == null) return;

            switch (_variant)
            {
                case Variant.Standard:
                    _background.color = t.surfaceMid;
                    if (_label != null) { _label.color = t.textPrimary; _label.fontStyle = FontStyles.SemiBold; }
                    SetGlow(t.accentPrimary, 0f);
                    StopPulse();
                    break;

                case Variant.Primary:
                    // Vertical-ish gradient simulated via a darker base + a brighter glow halo.
                    _background.color = t.accentAction;
                    if (_label != null) { _label.color = t.bgDeep; _label.fontStyle = FontStyles.Bold; }
                    SetGlow(t.accentPrimary, 0.40f);
                    StartPulseIfInteractable();
                    break;

                case Variant.Special:
                    _background.color = t.surfaceMid;
                    if (_label != null) { _label.color = t.textPrimary; _label.fontStyle = FontStyles.Bold; }
                    SetGlow(_voieTint.a > 0 ? _voieTint : t.accentPrimary, 0.30f);
                    StopPulse();
                    break;
            }
        }

        public void RefreshInteractableState()
        {
            var t = DesignTokens.Get();
            if (_button == null || _background == null) return;
            if (!_button.interactable)
            {
                _background.color = new Color(0.121f, 0.121f, 0.121f, 1f); // #1f1f1f
                if (_label != null) _label.color = t.textDisabled;
                SetGlow(Color.clear, 0f);
                StopPulse();
            }
            else
            {
                ApplyStyle();
            }
        }

        public void OnPointerDown(PointerEventData _)
        {
            if (_button == null || !_button.interactable) return;
            _rect.DOKill();
            _rect.DOScale(0.95f, 0.08f).SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            var t = DesignTokens.Get();
            if (_variant == Variant.Standard || _variant == Variant.Special)
                _background.color = t.surfaceLow;
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (_button == null || !_button.interactable) return;
            _rect.DOKill();
            _rect.DOScale(1f, 0.12f).SetEase(Ease.OutBack).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            ApplyStyle();
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (_button == null || !_button.interactable) return;
            if (_variant == Variant.Standard || _variant == Variant.Special)
                _background.color = DesignTokens.Get().surfaceHigh;
        }

        public void OnPointerExit(PointerEventData _)
        {
            if (_button == null || !_button.interactable) return;
            ApplyStyle();
        }

        private void SetGlow(Color color, float intensity)
        {
            if (_glowHalo == null) return;
            _glowHalo.color = new Color(color.r, color.g, color.b, intensity);
        }

        private void StartPulseIfInteractable()
        {
            if (_button != null && !_button.interactable) return;
            StopPulse();
            _rect = (RectTransform)transform;
            _pulseTween = _rect.DOScale(1.05f, 1.0f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void StopPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = null;
            if (_rect != null) _rect.localScale = Vector3.one;
        }

        /// <summary>
        /// Programmatic builder used by MainSceneBootstrap — attaches glow halo image as a sibling
        /// behind the button background.
        /// </summary>
        public static SagaButton Wrap(GameObject buttonGo, Variant variant, Color voieTint = default,
            bool createGlowHalo = true)
        {
            var sb = buttonGo.GetComponent<SagaButton>() ?? buttonGo.AddComponent<SagaButton>();
            if (createGlowHalo && variant != Variant.Standard)
            {
                var glow = new GameObject("GlowHalo", typeof(RectTransform), typeof(Image));
                glow.transform.SetParent(buttonGo.transform, false);
                var glowRt = (RectTransform)glow.transform;
                glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
                // Halo extends 16px beyond the button on each side.
                glowRt.offsetMin = new Vector2(-16, -16); glowRt.offsetMax = new Vector2(16, 16);
                var glowImg = glow.GetComponent<Image>();
                glowImg.sprite = null;
                glowImg.color = Color.clear;
                glowImg.raycastTarget = false;
                // Ensure halo renders BEHIND the button background.
                glow.transform.SetAsFirstSibling();
                sb._glowHalo = glowImg;
            }
            sb._variant = variant;
            sb._voieTint = voieTint;
            // Defer style apply to Awake (won't have refs in edit-time).
            if (Application.isPlaying) sb.ApplyStyle();
            return sb;
        }
    }
}
