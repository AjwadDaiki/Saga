using DG.Tweening;
using Saga.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — Tab click juicy animation (Bottom Nav premium feel).
    /// Animations VISUAL ONLY (Sprint 9 Phase 3 étape 1d ajoutera la navigation real).
    ///
    /// Sequence on click :
    ///   1. Press (0.08s) : parent scale 0.93 + Y -2, Icon 0.85, BG brightness up légère
    ///   2. Release pop (0.12s OutBack) : scale 1.05 → 1.0 + Y 0, Icon pulse 1.20→1.0 + Label bounce
    ///   3. Ripple effect : circle dots theme color expand depuis center 0.4s
    ///
    /// Active state (set via SetActive(true)) :
    ///   - Lift Y +5px persistent
    ///   - Icon pulse loop 1.0↔1.05 sur 2s
    /// Inactive state : standard idle subtle (parent micro float ±0.5px Y sur 2s).
    /// </summary>
    [DisallowMultipleComponent]
    public class TabAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool _isActiveTab; // Dojo par convention

        private Transform _root;
        private Transform _bg, _icon, _label;
        private Image _bgImage;
        private Color _bgOriginalColor = Color.white;
        private Vector3 _rootPos0, _rootScale0;
        private Vector3 _bgScale0, _iconScale0, _labelScale0;
        private Vector3 _activeLiftOffset = new Vector3(0f, 5f, 0f);

        private Tween _idleIconPulse, _idleParentFloat;
        private Sequence _activeClick;
        private float _phaseOffset;
        private bool _pointerDown;

        public bool IsActiveTab { get => _isActiveTab; set { _isActiveTab = value; ApplyState(); } }

        private void Awake()
        {
            _root = transform;
            _rootPos0 = _root.localPosition;
            _rootScale0 = _root.localScale;
            _bg = SceneRegistry.FindChildTolerant(transform, "BG");
            _icon = SceneRegistry.FindChildTolerant(transform, "Icon");
            _label = SceneRegistry.FindChildTolerant(transform, "Label");
            if (_bg != null)
            {
                _bgScale0 = _bg.localScale;
                _bgImage = _bg.GetComponent<Image>();
                if (_bgImage != null) _bgOriginalColor = _bgImage.color;
            }
            if (_icon != null) _iconScale0 = _icon.localScale;
            if (_label != null) _labelScale0 = _label.localScale;
            _phaseOffset = Random.Range(0f, 1f);
            Debug.Log($"[TabAnimator] {name} → active={_isActiveTab}, parts BG:{_bg!=null} Icon:{_icon!=null} Label:{_label!=null}");
        }

        private void OnEnable()
        {
            DOTween.Sequence().AppendInterval(_phaseOffset).AppendCallback(ApplyState)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void OnDisable()
        {
            KillIdle();
            KillActiveClick();
        }

        public void OnPointerDown(PointerEventData _)
        {
            _pointerDown = true;
            KillActiveClick();
            KillIdle();
            var seq = DOTween.Sequence();
            seq.Append(_root.DOScale(_rootScale0 * 0.93f, 0.08f).SetEase(Ease.OutQuad));
            seq.Join(_root.DOLocalMoveY(_rootPos0.y + (_isActiveTab ? _activeLiftOffset.y : 0f) - 2f, 0.08f).SetEase(Ease.OutQuad));
            if (_icon != null) seq.Join(_icon.DOScale(_iconScale0 * 0.85f, 0.08f).SetEase(Ease.OutQuad));
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeClick = seq;
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (!_pointerDown) return;
            _pointerDown = false;
            KillActiveClick();

            var tokens = DesignTokens.Get();
            var ripple = tokens.jauneReward;

            var seq = DOTween.Sequence();
            seq.Append(_root.DOScale(_rootScale0 * 1.05f, 0.07f).SetEase(Ease.OutQuad));
            seq.Append(_root.DOScale(_rootScale0, 0.05f).SetEase(Ease.OutBack));
            var liftY = _isActiveTab ? _rootPos0.y + _activeLiftOffset.y : _rootPos0.y;
            seq.Join(_root.DOLocalMoveY(liftY, 0.12f).SetEase(Ease.OutBack));
            if (_icon != null)
            {
                seq.Join(_icon.DOScale(_iconScale0 * 1.20f, 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.08f, _icon.DOScale(_iconScale0, 0.10f).SetEase(Ease.OutBack));
            }
            if (_label != null)
            {
                seq.Join(_label.DOScale(_labelScale0 * 1.08f, 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.08f, _label.DOScale(_labelScale0, 0.10f).SetEase(Ease.OutBack));
            }
            seq.OnComplete(ApplyState);
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeClick = seq;

            SpawnRipple(ripple);
        }

        private void ApplyState()
        {
            KillIdle();
            // Position : active lifted +5px persistent.
            var targetY = _isActiveTab ? _rootPos0.y + _activeLiftOffset.y : _rootPos0.y;
            _root.localPosition = new Vector3(_rootPos0.x, targetY, _rootPos0.z);

            if (_isActiveTab && _icon != null)
            {
                _idleIconPulse = _icon.DOScale(_iconScale0 * 1.05f, 1.0f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            else
            {
                _idleParentFloat = _root.DOLocalMoveY(targetY + 0.5f, 1.0f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        private void SpawnRipple(Color color)
        {
            var center = _icon != null ? _icon : (Transform)transform;
            var rippleGo = new GameObject("Ripple", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            rippleGo.transform.SetParent(center, false);
            var rt = (RectTransform)rippleGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(20, 20);
            rt.localScale = Vector3.one;
            var img = rippleGo.GetComponent<Image>();
            img.color = new Color(color.r, color.g, color.b, 0.5f);
            img.raycastTarget = false;
            var cg = rippleGo.GetComponent<CanvasGroup>();
            cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false;

            Object.Destroy(rippleGo, 0.45f);
            DOTween.Sequence()
                .Append(rt.DOScale(4f, 0.40f).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => cg.alpha, a => { if (cg != null) cg.alpha = a; }, 0f, 0.40f).SetEase(Ease.InQuad))
                .SetLink(rippleGo, LinkBehaviour.KillOnDestroy);
        }

        private void KillIdle()
        {
            _idleIconPulse?.Kill();
            _idleParentFloat?.Kill();
        }

        private void KillActiveClick()
        {
            _activeClick?.Kill();
            _activeClick = null;
        }
    }
}
