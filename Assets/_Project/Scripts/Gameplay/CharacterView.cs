using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 3 character view. Drives the SpriteAnimator state from gameplay events:
    /// - Tap resolved → roll a weighted attack (1/2/3 — see <see cref="AttackSelector"/>) based
    ///   on the current combo tier, then auto-return to "idle" on completion.
    /// - Combo changed → track the latest tier so the next tap selects from the right distribution.
    /// - Stade changed → swap library (later sprints) or re-bind sprites for the new tier.
    ///
    /// Sprint 3 fix #4: a subtle infinite breathing DOScale runs in parallel with the SpriteAnimator
    /// so the character never feels totally static. The 3% amplitude is imperceptible during a fast
    /// attack but reads as "alive" during idle.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator _animator;
        [SerializeField] private string _idleAnim = "idle";

        [Tooltip("Peak scale of the constant idle breathing — 1.03 = +3% (subtle).")]
        [SerializeField] private float _breathScale = 1.03f;

        [Tooltip("Seconds for a full breathe-in (one Yoyo half). Total cycle = 2× this.")]
        [SerializeField] private float _breathHalfPeriod = 1.5f;

        private int _currentComboTier;
        private Tween _breathTween;

        public SpriteAnimator Animator
        {
            get => _animator;
            set => _animator = value;
        }

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<SpriteAnimator>();
        }

        private void Start()
        {
            // Constant subtle breathing. Yoyo gives a clean back-and-forth without snap.
            _breathTween?.Kill();
            transform.localScale = Vector3.one;
            _breathTween = transform.DOScale(_breathScale, _breathHalfPeriod)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void OnDestroy()
        {
            _breathTween?.Kill();
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTapResolved;
            GameEvents.OnComboChanged += HandleComboChanged;
            GameEvents.OnStadeChanged += HandleStadeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTapResolved;
            GameEvents.OnComboChanged -= HandleComboChanged;
            GameEvents.OnStadeChanged -= HandleStadeChanged;
        }

        private void HandleComboChanged(int tier, float baseMultiplier)
        {
            _currentComboTier = tier;
        }

        private void HandleTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            if (_animator == null) return;
            var attackName = AttackSelector.Select(_currentComboTier);
            _animator.Play(attackName, queueNext: _idleAnim);
        }

        private void HandleStadeChanged(int previous, int next)
        {
            // Sprint 3: single library (Adventurer) used across all stades visually. When per-stade
            // sprite sets land (Sprint 4+), swap _animator.Library here based on the new stade.
        }
    }
}
