using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Applies the Vague AOE effect when triggered by <see cref="ElanService"/>:
    /// - In Training: starts a 5s ×5 Force buff (consumed by <see cref="TapHandler"/>).
    /// - In AdversaireActive/CapitaineActive: deals a flat burst of damage scaled by stade.
    ///
    /// Ticked at 10Hz via <see cref="GameTicker"/> to manage the buff timer.
    /// </summary>
    public sealed class VagueResolver
    {
        private readonly ContentDatabase _content;
        private float _buffTimer; // seconds remaining; 0 = no buff active
        private bool _buffWasActive;

        public VagueResolver(ContentDatabase content)
        {
            _content = content;
        }

        /// <summary>True while the Training Force buff is active. TapHandler reads this each tap.</summary>
        public bool IsTrainingBuffActive => _buffTimer > 0f;

        /// <summary>Trigger the Vague based on the current phase. Called by VagueButtonView/UI.</summary>
        public void TriggerVague(GameState state)
        {
            if (state == null) return;

            var phase = state.currentPhase;
            GameEvents.RaiseVagueTriggered(phase);

            if (phase == CombatPhase.AdversaireActive || phase == CombatPhase.CapitaineActive)
            {
                ApplyCombatDamage(state);
            }
            else
            {
                StartTrainingBuff();
            }
        }

        public void Tick(GameState state, float dt)
        {
            if (_buffTimer <= 0f)
            {
                if (_buffWasActive)
                {
                    _buffWasActive = false;
                    GameEvents.RaiseVagueBuffActive(false);
                }
                return;
            }

            _buffTimer -= dt;
            if (_buffTimer <= 0f)
            {
                _buffTimer = 0f;
                _buffWasActive = false;
                GameEvents.RaiseVagueBuffActive(false);
            }
        }

        private void StartTrainingBuff()
        {
            _buffTimer = ElanConstants.VagueTrainingDurationSeconds;
            _buffWasActive = true;
            GameEvents.RaiseVagueBuffActive(true);
            GameEvents.RaiseVagueResolved(new BigDouble(ElanConstants.VagueTrainingForceMultiplier));
        }

        private void ApplyCombatDamage(GameState state)
        {
            // Damage scales linearly with stade for now — Sprint 7+ may revisit balance.
            var stadeMul = Mathf.Max(1, state.currentStade);
            var damage = new BigDouble(ElanConstants.VagueCombatDamageBase * stadeMul);

            BigDouble maxHp = default;
            if (state.currentPhase == CombatPhase.AdversaireActive)
            {
                var adv = _content?.GetAdversaire(state.currentAdversaireId);
                if (adv != null) maxHp = adv.Hp;
            }
            else if (state.currentPhase == CombatPhase.CapitaineActive)
            {
                var cap = _content?.GetCapitaine(state.currentCapitaineId);
                if (cap != null) maxHp = cap.Hp;
            }

            state.currentAdversaireHp -= damage;
            if (state.currentAdversaireHp.Sign() < 0) state.currentAdversaireHp = new BigDouble(0);
            GameManager.Instance?.Save?.MarkDirty();

            GameEvents.RaiseAdversaireDamaged(damage, state.currentAdversaireHp, maxHp);
            GameEvents.RaiseVagueResolved(damage);
        }
    }
}
