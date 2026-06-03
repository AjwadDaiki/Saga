using DG.Tweening;
using Saga.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — Card click juicy animation (Cards Upgrades premium feel).
    /// Animations VISUAL ONLY (Sprint 9 Phase 3 étape 1b ajoutera le wiring buy real).
    ///
    /// Theme colors auto-resolved par le name :
    ///   - Card_Strike → Coral (#FF6B6B coralAction)
    ///   - Card_Focus  → Sky   (#65C8FF skyBlue)
    ///   - Card_Power  → Or    (#FFD84D jauneReward)
    ///
    /// Sequence on click :
    ///   1. Press down (0.08s) : parent scale 0.95 + Y -3, BG 0.92, Icon 0.90, Label 0.92
    ///   2. Release bounce (0.15s OutBack) : retour 1.0 + Y 0, Icon pulse 1.15→1.0, BG flash
    ///      theme color alpha 0.4→0 sur 0.3s, Label bounce
    ///   3. Particles burst : 8 dots theme color depuis center Icon explose vers extérieur 0.4s
    ///   4. Hover idle loop (sans click) : BG shimmer brightness ±5% sur 3s, Icon micro float
    ///      ±1px Y sur 2s (déphasé entre 3 cards via Awake random offset)
    ///
    /// Disabled state (à wirer Sprint 9 1b) : shake X ±5px en 0.2s + no other anim.
    /// </summary>
    [DisallowMultipleComponent]
    public class CardAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Theme { Strike, Focus, Power }

        private Theme _theme;
        private Transform _root;
        private Transform _bg, _icon, _label;
        private Image _bgImage;
        private Vector3 _rootPos0, _rootScale0;
        private Vector3 _bgScale0, _iconScale0, _labelScale0;
        private Color _bgOriginalColor = Color.white;

        private Tween _idleShimmer, _idleIconFloat;
        private Sequence _activeClick;
        private float _hoverPhaseOffset;
        private bool _pointerDown;

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

            _theme = ResolveTheme(name);
            _hoverPhaseOffset = Random.Range(0f, 1f); // déphasage idle inter-cards
            Debug.Log($"[CardAnimator] {name} → theme {_theme}, parts BG:{_bg!=null} Icon:{_icon!=null} Label:{_label!=null}");
        }

        private void OnEnable()
        {
            // Idle loop démarre après un délai aléatoire pour ne pas tous synchroniser.
            DOTween.Sequence().AppendInterval(_hoverPhaseOffset).AppendCallback(StartIdleLoop)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void OnDisable()
        {
            KillIdleTweens();
            KillActiveClick();
        }

        public void OnPointerDown(PointerEventData _)
        {
            _pointerDown = true;
            KillActiveClick();
            KillIdleTweens();
            var seq = DOTween.Sequence();
            seq.Append(_root.DOScale(_rootScale0 * 0.95f, 0.08f).SetEase(Ease.OutQuad));
            seq.Join(_root.DOLocalMoveY(_rootPos0.y - 3f, 0.08f).SetEase(Ease.OutQuad));
            if (_bg != null) seq.Join(_bg.DOScale(_bgScale0 * 0.92f, 0.08f).SetEase(Ease.OutQuad));
            if (_icon != null) seq.Join(_icon.DOScale(_iconScale0 * 0.90f, 0.08f).SetEase(Ease.OutQuad));
            if (_label != null) seq.Join(_label.DOScale(_labelScale0 * 0.92f, 0.08f).SetEase(Ease.OutQuad));
            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activeClick = seq;
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (!_pointerDown) return;
            _pointerDown = false;
            KillActiveClick();

            var tokens = DesignTokens.Get();
            var themeColor = ResolveThemeColor(_theme, tokens);

            var seq = DOTween.Sequence();
            // Retour scale + Y avec OutBack + Icon pulse 1.15→1.0.
            seq.Append(_root.DOScale(_rootScale0, 0.15f).SetEase(Ease.OutBack));
            seq.Join(_root.DOLocalMoveY(_rootPos0.y, 0.15f).SetEase(Ease.OutBack));
            if (_bg != null) seq.Join(_bg.DOScale(_bgScale0, 0.15f).SetEase(Ease.OutBack));
            if (_icon != null)
            {
                seq.Join(_icon.DOScale(_iconScale0 * 1.15f, 0.08f).SetEase(Ease.OutQuad));
                seq.Insert(0.08f, _icon.DOScale(_iconScale0, 0.10f).SetEase(Ease.OutBack));
            }
            if (_label != null)
            {
                seq.Join(_label.DOScale(_labelScale0 * 1.08f, 0.10f).SetEase(Ease.OutQuad));
                seq.Insert(0.10f, _label.DOScale(_labelScale0, 0.10f).SetEase(Ease.OutBack));
            }

            // BG flash theme color alpha 0.4 → 0 sur 0.3s (via tint Image.color).
            if (_bgImage != null)
            {
                var img = _bgImage;
                var orig = _bgOriginalColor;
                img.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.7f);
                DOTween.Sequence().AppendInterval(0.05f)
                    .Append(DOTween.To(() => img.color, c => img.color = c, orig, 0.30f).SetEase(Ease.OutQuad))
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }

            seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            seq.OnComplete(StartIdleLoop);
            _activeClick = seq;

            // Particles burst depuis center Icon.
            SpawnParticles(themeColor);
        }

        private void StartIdleLoop()
        {
            KillIdleTweens();
            if (_icon != null)
            {
                _idleIconFloat = _icon.DOLocalMoveY(_icon.localPosition.y + 1f, 1.0f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            // Optionnel : BG shimmer brightness — skip V1 pour économie (rendu pas critique).
        }

        private void SpawnParticles(Color color)
        {
            var center = _icon != null ? _icon : (Transform)transform;
            const int count = 8;
            for (var i = 0; i < count; i++)
            {
                var angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var dotGo = new GameObject("Particle", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                dotGo.transform.SetParent(center, false);
                var rt = (RectTransform)dotGo.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(8, 8);
                var img = dotGo.GetComponent<Image>();
                img.color = new Color(color.r, color.g, color.b, 0.9f);
                img.raycastTarget = false;
                var cg = dotGo.GetComponent<CanvasGroup>();
                cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false;

                var target = dir * Random.Range(50f, 80f);
                Object.Destroy(dotGo, 0.45f);
                DOTween.Sequence()
                    .Append(rt.DOLocalMove(new Vector3(target.x, target.y, 0f), 0.40f).SetEase(Ease.OutCubic))
                    .Join(DOTween.To(() => cg.alpha, a => { if (cg != null) cg.alpha = a; }, 0f, 0.40f).SetEase(Ease.InQuad))
                    .Join(rt.DOScale(0.4f, 0.40f).SetEase(Ease.InQuad))
                    .SetLink(dotGo, LinkBehaviour.KillOnDestroy);
            }
        }

        private void KillIdleTweens()
        {
            _idleShimmer?.Kill(); _idleIconFloat?.Kill();
        }

        private void KillActiveClick()
        {
            _activeClick?.Kill();
            _activeClick = null;
        }

        // ===== Helpers =====

        private static Theme ResolveTheme(string goName)
        {
            var n = goName.Trim().ToLowerInvariant();
            if (n.Contains("focus")) return Theme.Focus;
            if (n.Contains("power")) return Theme.Power;
            return Theme.Strike;
        }

        private static Color ResolveThemeColor(Theme theme, DesignTokens tokens)
        {
            switch (theme)
            {
                case Theme.Focus: return tokens.skyBlue;
                case Theme.Power: return tokens.jauneReward;
                default: return tokens.coralAction;
            }
        }
    }
}
