using BreakInfinity;
using Saga.Core;
using Saga.Data;

namespace Saga.Gameplay
{
    /// <summary>
    /// State machine for the combat lifecycle (Sprint 4 — adversaire only).
    /// Ticks every 10Hz from <see cref="GameTicker"/>, drives phase transitions:
    ///
    ///   Training ──(spawn)──&gt; AdversaireIncoming ──(1s)──&gt; AdversaireActive
    ///                                                       │
    ///                                            ┌──────────┴──────────┐
    ///                                            │                     │
    ///                                       HP ≤ 0:               chrono ≤ 0:
    ///                                  AdversaireVictory     PlayerDeathTemporary
    ///                                            │                     │
    ///                                          (2s)              (click resume)
    ///                                            │                     │
    ///                                          Training &lt;──────────────┘
    ///
    /// PlayerDeathTemporary transitions back to Training only when the UI calls
    /// <see cref="ResolveDeathTemporary"/> (i.e. the player clicked the overlay).
    /// </summary>
    public sealed class CombatProcessor
    {
        public const float IncomingCinematicSeconds = 1.0f;
        public const float VictoryCelebrationSeconds = 2.0f;
        public const float DeathForcePenalty = 0.10f; // -10% Force totale per GAME_DESIGN_v2

        private readonly ContentDatabase _content;
        private float _phaseTimer;
        private float _activeChronoTotal; // remembers the total chrono for ratio display

        public CombatProcessor(ContentDatabase content)
        {
            _content = content;
        }

        public float ActiveChronoTotal => _activeChronoTotal;

        /// <summary>Begin the incoming-cinematic phase, configuring the state for the next combat.</summary>
        public void StartIncoming(GameState state, AdversaireData data)
        {
            if (state == null || data == null) return;

            state.currentAdversaireId = data.Id;
            state.currentAdversaireHp = data.Hp;
            state.chronoRemaining = data.ChronoSeconds;
            _activeChronoTotal = data.ChronoSeconds;
            _phaseTimer = IncomingCinematicSeconds;

            TransitionTo(state, CombatPhase.AdversaireIncoming);
            GameEvents.RaiseAdversaireSpawned(data);
        }

        /// <summary>
        /// Called by UI when the player clicks-to-resume on the death overlay.
        /// Applies the -10% Force penalty and returns to Training.
        /// </summary>
        public void ResolveDeathTemporary(GameState state)
        {
            if (state == null) return;
            // -10% Force totale.
            state.force = state.force * (1.0 - DeathForcePenalty);
            ClearCombatState(state);
            TransitionTo(state, CombatPhase.Training);
            GameEvents.RaiseForceChanged();
        }

        /// <summary>10Hz tick from GameTicker.</summary>
        public void Tick(GameState state, float dt)
        {
            if (state == null) return;

            switch (state.currentPhase)
            {
                case CombatPhase.AdversaireIncoming:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                    {
                        TransitionTo(state, CombatPhase.AdversaireActive);
                    }
                    break;

                case CombatPhase.AdversaireActive:
                    state.chronoRemaining -= dt;
                    GameEvents.RaiseChronoUpdated(state.chronoRemaining, _activeChronoTotal);

                    if (state.currentAdversaireHp.Sign() <= 0)
                    {
                        OnAdversaireDefeated(state);
                    }
                    else if (state.chronoRemaining <= 0f)
                    {
                        OnPlayerDeathFromChrono(state);
                    }
                    break;

                case CombatPhase.AdversaireVictory:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                    {
                        ClearCombatState(state);
                        TransitionTo(state, CombatPhase.Training);
                    }
                    break;

                case CombatPhase.PlayerDeathTemporary:
                    // Waits for ResolveDeathTemporary() — driven by the UI overlay click.
                    break;

                case CombatPhase.Training:
                default:
                    // No-op; AdversaireSpawner handles the Training → Incoming transition.
                    break;
            }
        }

        private void OnAdversaireDefeated(GameState state)
        {
            var data = _content?.GetAdversaire(state.currentAdversaireId);
            var reward = data != null ? data.RewardForce : new BigDouble(0);

            state.force += reward;
            state.totalAdversairesDefeated++;
            GameEvents.RaiseForceChanged();

            if (data != null) GameEvents.RaiseAdversaireDefeated(data, reward);

            _phaseTimer = VictoryCelebrationSeconds;
            TransitionTo(state, CombatPhase.AdversaireVictory);
        }

        private void OnPlayerDeathFromChrono(GameState state)
        {
            state.chronoRemaining = 0f;
            TransitionTo(state, CombatPhase.PlayerDeathTemporary);
            GameEvents.RaisePlayerDiedTemporary();
        }

        private static void ClearCombatState(GameState state)
        {
            state.currentAdversaireId = null;
            state.currentAdversaireHp = default;
            state.chronoRemaining = 0f;
        }

        private static void TransitionTo(GameState state, CombatPhase next)
        {
            var prev = state.currentPhase;
            if (prev == next) return;
            state.currentPhase = next;
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaisePhaseChanged(prev, next);
        }
    }
}
