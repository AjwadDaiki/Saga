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

        public LayeredCharacterRenderer Renderer
        {
            get => _renderer;
            set => _renderer = value;
        }

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<LayeredCharacterRenderer>();
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
            transform.localScale = Vector3.one;
            _breathTween = transform.DOScale(_breathScale, _breathHalfPeriod)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
