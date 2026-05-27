using BreakInfinity;
using DG.Tweening;
using Saga.Math;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// One-shot floating "+X" number that spawns at the tap world/screen position,
    /// floats upward toward the Force counter, fades out, then self-destroys.
    ///
    /// Per 05_VISUAL_STYLE §FX: tap dust + ring is separate (ParticleSystem); this is the number.
    /// Per 02_GAME_DESIGN §9: "Les +X de tap doivent flotter vers le compteur cible et disparaître dedans."
    /// </summary>
    [DisallowMultipleComponent]
    public class FloatingNumberView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private float _verticalRiseScreenPx = 120f;

        private RectTransform _rect;

        public TextMeshProUGUI Label
        {
            get => _label;
            set => _label = value;
        }

        public CanvasGroup Group
        {
            get => _group;
            set => _group = value;
        }

        private void Awake()
        {
            _rect = transform as RectTransform;
            if (_group == null) _group = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Set the number to display. Optionally tint by combo tier (0..3).
        /// </summary>
        public void Init(BigDouble gain, int comboTier)
        {
            if (_label != null)
            {
                _label.text = "+" + NumberFormatter.Format(gain);
                _label.color = TierColor(comboTier);
            }
        }

        /// <summary>
        /// Launch the float-up animation toward an optional target world position.
        /// If <paramref name="targetAnchored"/> is null, only floats upward.
        /// </summary>
        public void Play(Vector2? targetAnchored = null)
        {
            if (_rect == null) _rect = transform as RectTransform;
            if (_group == null) _group = GetComponent<CanvasGroup>();
            if (_rect == null) { Destroy(gameObject); return; }

            var start = _rect.anchoredPosition;
            var end = targetAnchored ?? new Vector2(start.x, start.y + _verticalRiseScreenPx);
            // Anchors centered (0.5, 0.5) so anchoredPosition maps 1:1 to localPosition.xy.
            // Tween via DOLocalMove (core DOTween shortcut in DOTween.dll) instead of DOAnchorPos
            // (which lives in the UI module — see DESIGN_DECISIONS_LOG.md 2026-05-27).
            var endLocal = new Vector3(end.x, end.y, _rect.localPosition.z);

            var seq = DOTween.Sequence();
            seq.Append(_rect.DOLocalMove(endLocal, _duration).SetEase(Ease.OutCubic));
            if (_group != null)
            {
                _group.alpha = 1f;
                // DOTween.To<float> is in DOTween core. CanvasGroup.alpha is a simple float setter
                // so we tween it directly without needing the UI-module DOFade extension.
                seq.Join(DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, _duration).SetEase(Ease.InQuad));
            }
            seq.OnComplete(() => Destroy(gameObject));
        }

        private static Color TierColor(int tier)
        {
            // Palette per 05_VISUAL_STYLE.md (accent-primary samurai default + crit warm).
            switch (tier)
            {
                case 0: return new Color(0.98f, 0.98f, 0.98f, 1f);     // #fafafa text-primary
                case 1: return new Color(0.98f, 0.78f, 0.46f, 1f);     // #FAC775 ambre
                case 2: return new Color(0.99f, 0.65f, 0.30f, 1f);     // mid-warm
                default: return new Color(0.99f, 0.45f, 0.20f, 1f);    // #993C1D-ish coral
            }
        }
    }
}
