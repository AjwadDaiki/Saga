using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 7 character view — drives the modular <see cref="LayeredCharacterRenderer"/>
    /// from gameplay events. Plays attack on tap (weighted variant), meditation while Souffle
    /// is active, hurt/die on damage taken (Sprint 8+ when player HP exists).
    ///
    /// Also runs the constant breathing DOScale on the root transform so the character feels
    /// alive even between events.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private LayeredCharacterRenderer _renderer;
        [SerializeField] private string _idleAnim = "idle";
        [SerializeField] private string _meditationAnim = "meditation";

        [Header("Breathing (constant idle scale)")]
        [SerializeField] private float _breathScale = 1.03f;
        [SerializeField] private float _breathHalfPeriod = 1.6f;

        private int _currentComboTier;
        private Tween _breathTween;
        private Vector3 _baseScale = Vector3.one;

        public LayeredCharacterRenderer Renderer
        {
            get => _renderer;
            set => _renderer = value;
        }

        /// <summary>
        /// Sprint 7.5 fix: set the resting scale the breathing tween animates around. Must be called
        /// AFTER the GameObject is built (Awake/OnEnable run during construction, before bootstrap can
        /// set the transform). Restarts the breathing tween so the new base takes effect immediately.
        /// </summary>
        public void SetBaseScale(Vector3 baseScale)
        {
            _baseScale = baseScale == Vector3.zero ? Vector3.one : baseScale;
            if (isActiveAndEnabled) StartBreathing();
            else transform.localScale = _baseScale;
        }

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<LayeredCharacterRenderer>();
            // Sprint 7.5 fix: capture the bootstrap-applied scale (e.g. 2x) so the breathing
            // tween animates AROUND it instead of stomping it back to 1.
            _baseScale = transform.localScale;
            if (_baseScale == Vector3.zero) _baseScale = Vector3.one;
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTapResolved;
            GameEvents.OnComboChanged += HandleComboChanged;
            GameEvents.OnStadeChanged += HandleStadeChanged;
            GameEvents.OnSouffleStarted += HandleSouffleStarted;
            GameEvents.OnSouffleEnded += HandleSouffleEnded;
            GameEvents.OnEquipmentChanged += HandleEquipmentChanged;
            StartBreathing();
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTapResolved;
            GameEvents.OnComboChanged -= HandleComboChanged;
            GameEvents.OnStadeChanged -= HandleStadeChanged;
            GameEvents.OnSouffleStarted -= HandleSouffleStarted;
            GameEvents.OnSouffleEnded -= HandleSouffleEnded;
            GameEvents.OnEquipmentChanged -= HandleEquipmentChanged;
            _breathTween?.Kill();
        }

        private void HandleComboChanged(int tier, float baseMultiplier)
        {
            _currentComboTier = tier;
        }

        private void HandleTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            if (_renderer == null) return;
            // Skip attack anim during meditation — Souffle visually claims the character.
            var gm = GameManager.Instance;
            if (gm?.Souffle != null && gm.Souffle.IsMeditating) return;

            var attackName = AttackSelector.Select(_currentComboTier);
            _renderer.PlayAnimation(attackName, queueNext: _idleAnim);
        }

        private void HandleStadeChanged(int previous, int next)
        {
            // Sprint 7: visual stade upgrade comes from EquipmentService swapping layers, not
            // from CharacterView directly. Stade transitions are still observable here for FX hooks.
        }

        private void HandleSouffleStarted()
        {
            _renderer?.PlayAnimation(_meditationAnim);
        }

        private void HandleSouffleEnded()
        {
            _renderer?.PlayAnimation(_idleAnim);
        }

        private void HandleEquipmentChanged(EquipmentSlot slot, SpriteLayerSet next, SpriteLayerSet previous)
        {
            if (_renderer == null) return;
            _renderer.SetLayer(slot, next);
        }

        private void StartBreathing()
        {
            _breathTween?.Kill();
            transform.localScale = _baseScale;
            // Breathe between base and base × breathScale so a 2x character still breathes subtly.
            _breathTween = transform.DOScale(_baseScale * _breathScale, _breathHalfPeriod)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
