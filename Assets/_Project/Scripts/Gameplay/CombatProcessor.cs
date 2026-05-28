using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// State machine for the combat lifecycle (Sprint 4 — adversaire, Sprint 5 — capitaine).
    /// Ticks at 10Hz from <see cref="GameTicker"/>.
    ///
    ///   Training ──(spawn)──&gt; AdversaireIncoming ──(1s)──&gt; AdversaireActive
    ///                                              │
    ///                                   ┌──────────┴───────────┐
    ///                                   │                      │
    ///                              HP ≤ 0 :             chrono ≤ 0 :
    ///                       AdversaireVictory     PlayerDeathTemporary
    ///                                   │
    ///                  (every 10th defeat, CapitaineSpawner has pending)
    ///                                   │
    ///                       CapitaineIncoming ──(1.5s)──&gt; CapitaineActive
    ///                                              │
    ///                                   ┌──────────┴───────────┐
    ///                                   │                      │
    ///                              HP ≤ 0 :             chrono ≤ 0 :
    ///                        CapitaineVictory     PlayerDeathTemporary
    ///                                   │
    ///                                 (3s)
    ///                                   │
    ///                                Training
    ///
    /// HP phase tracking (Capitaine only): per-tick check against
    /// <see cref="CapitaineData.PhaseThresholds"/>; emits <see cref="GameEvents.OnCapitainePhaseChanged"/>
    /// on transition, <see cref="GameEvents.OnCapitaineEnraged"/> when reaching the last phase.
    /// </summary>
    public sealed class CombatProcessor
    {
        public const float IncomingCinematicSeconds = 1.0f;
        public const float CapitaineIncomingSeconds = 1.5f;
        public const float MaitreIncomingSeconds = 2.5f;
        public const float VictoryCelebrationSeconds = 2.0f;
        public const float CapitaineVictorySeconds = 3.0f;
        public const float MaitreVictorySeconds = 3.5f;
        public const float DeathForcePenalty = 0.10f;
        public const float MaitreEnrageChronoMultiplier = 2f; // chrono runs 2× faster at enrage

        private readonly ContentDatabase _content;
        private CapitaineSpawner _capitaineSpawner;
        private float _phaseTimer;
        private float _activeChronoTotal;

        public CombatProcessor(ContentDatabase content)
        {
            _content = content;
        }

        /// <summary>Late-injected by GameManager (handles the circular dependency between processor and spawner).</summary>
        public void AttachCapitaineSpawner(CapitaineSpawner spawner) => _capitaineSpawner = spawner;

        public float ActiveChronoTotal => _activeChronoTotal;

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

        public void StartCapitaineIncoming(GameState state, CapitaineData data)
        {
            if (state == null || data == null) return;

            state.currentCapitaineId = data.Id;
            state.currentAdversaireId = null; // adversaire slot is free during capitaine combat
            state.currentAdversaireHp = data.Hp; // reuse HP field for the engaged enemy
            state.chronoRemaining = data.ChronoSeconds;
            state.currentCapitainePhase = 0;
            _activeChronoTotal = data.ChronoSeconds;
            _phaseTimer = CapitaineIncomingSeconds;

            TransitionTo(state, CombatPhase.CapitaineIncoming);
            GameEvents.RaiseCapitaineSpawned(data);
        }

        public void StartMaitreIncoming(GameState state, MaitreData data)
        {
            if (state == null || data == null) return;

            state.currentMaitreId = data.Id;
            state.currentAdversaireId = null;
            state.currentCapitaineId = null;
            state.currentAdversaireHp = data.Hp; // shared HP field for the engaged enemy
            state.chronoRemaining = data.ChronoSeconds;
            state.currentMaitrePhase = 0;
            _activeChronoTotal = data.ChronoSeconds;
            _phaseTimer = MaitreIncomingSeconds;

            TransitionTo(state, CombatPhase.MaitreIncoming);
            GameEvents.RaiseMaitreSpawned(data);
        }

        public void ResolveDeathTemporary(GameState state)
        {
            if (state == null) return;
            state.force = state.force * (1.0 - DeathForcePenalty);
            ClearCombatState(state);
            TransitionTo(state, CombatPhase.Training);
            GameEvents.RaiseForceChanged();
        }

        public void Tick(GameState state, float dt)
        {
            if (state == null) return;

            switch (state.currentPhase)
            {
                case CombatPhase.AdversaireIncoming:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f) TransitionTo(state, CombatPhase.AdversaireActive);
                    break;

                case CombatPhase.AdversaireActive:
                    state.chronoRemaining -= dt;
                    GameEvents.RaiseChronoUpdated(state.chronoRemaining, _activeChronoTotal);
                    if (state.currentAdversaireHp.Sign() <= 0) OnAdversaireDefeated(state);
                    else if (state.chronoRemaining <= 0f) OnPlayerDeathFromChrono(state);
                    break;

                case CombatPhase.AdversaireVictory:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                    {
                        ClearCombatState(state);
                        // Capitaine queued by the spawner? Divert directly into CapitaineIncoming.
                        var pending = _capitaineSpawner?.ConsumePending();
                        if (pending != null) StartCapitaineIncoming(state, pending);
                        else TransitionTo(state, CombatPhase.Training);
                    }
                    break;

                case CombatPhase.CapitaineIncoming:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f) TransitionTo(state, CombatPhase.CapitaineActive);
                    break;

                case CombatPhase.CapitaineActive:
                    state.chronoRemaining -= dt;
                    GameEvents.RaiseChronoUpdated(state.chronoRemaining, _activeChronoTotal);
                    UpdateCapitainePhase(state);
                    if (state.currentAdversaireHp.Sign() <= 0) OnCapitaineDefeated(state);
                    else if (state.chronoRemaining <= 0f) OnPlayerDeathFromChrono(state);
                    break;

                case CombatPhase.CapitaineVictory:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                    {
                        ClearCombatState(state);
                        TransitionTo(state, CombatPhase.Training);
                    }
                    break;

                case CombatPhase.MaitreIncoming:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f) TransitionTo(state, CombatPhase.MaitreActive);
                    break;

                case CombatPhase.MaitreActive:
                    // Chrono accelerates at enrage (phase 3).
                    var chronoMultiplier = state.currentMaitrePhase >= 3 ? MaitreEnrageChronoMultiplier : 1f;
                    state.chronoRemaining -= dt * chronoMultiplier;
                    GameEvents.RaiseChronoUpdated(state.chronoRemaining, _activeChronoTotal);
                    UpdateMaitrePhase(state);
                    if (state.currentAdversaireHp.Sign() <= 0) OnMaitreDefeated(state);
                    else if (state.chronoRemaining <= 0f) OnPlayerDeathFromMaitre(state);
                    break;

                case CombatPhase.MaitreVictory:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                    {
                        ClearCombatState(state);
                        TransitionTo(state, CombatPhase.Training);
                    }
                    break;

                case CombatPhase.PlayerDeathTemporary:
                case CombatPhase.Training:
                default:
                    break;
            }
        }

        private void UpdateMaitrePhase(GameState state)
        {
            var maitre = _content?.GetMaitre(state.currentMaitreId);
            if (maitre == null) return;
            var maxHp = maitre.Hp.ToDouble();
            if (maxHp <= 0) return;
            var ratio = state.currentAdversaireHp.ToDouble() / maxHp;
            var newPhase = maitre.ComputePhase(ratio);
            if (newPhase == state.currentMaitrePhase) return;
            var prev = state.currentMaitrePhase;
            state.currentMaitrePhase = newPhase;
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseMaitrePhaseChanged(prev, newPhase);
            if (newPhase == maitre.PhaseThresholds.Length) GameEvents.RaiseCapitaineEnraged();
            // Note: re-use OnCapitaineEnraged event for now (Sprint 6 MVP). Could add OnMaitreEnraged if needed.
        }

        private void OnMaitreDefeated(GameState state)
        {
            var data = _content?.GetMaitre(state.currentMaitreId);
            var reward = data != null ? data.RewardForce : new BigDouble(0);

            state.force += reward;
            // Track the relic drop. Sprint 6 MVP: just record the Maître ID.
            if (data != null && !state.relicsOwned.Contains(data.Id)) state.relicsOwned.Add(data.Id);
            GameEvents.RaiseForceChanged();
            if (data != null) GameEvents.RaiseMaitreDefeated(data, reward);

            _phaseTimer = MaitreVictorySeconds;
            TransitionTo(state, CombatPhase.MaitreVictory);
        }

        private void OnPlayerDeathFromMaitre(GameState state)
        {
            state.chronoRemaining = 0f;
            var data = _content?.GetMaitre(state.currentMaitreId);
            // Hand off to PrestigeService — it transitions phase + raises OnPrestigeTriggered.
            GameManager.Instance?.Prestige?.TriggerPrestige(state, data);
        }

        private void UpdateCapitainePhase(GameState state)
        {
            var capitaine = _content?.GetCapitaine(state.currentCapitaineId);
            if (capitaine == null) return;

            var maxHp = capitaine.Hp.ToDouble();
            if (maxHp <= 0) return;

            var ratio = state.currentAdversaireHp.ToDouble() / maxHp;
            var newPhase = capitaine.ComputePhase(ratio);
            if (newPhase == state.currentCapitainePhase) return;

            var prev = state.currentCapitainePhase;
            state.currentCapitainePhase = newPhase;
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseCapitainePhaseChanged(prev, newPhase);

            // Enrage = last phase. Threshold count + 1 phases total (e.g. 4 phases for 3 thresholds).
            if (newPhase == capitaine.PhaseThresholds.Length) GameEvents.RaiseCapitaineEnraged();
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

        private void OnCapitaineDefeated(GameState state)
        {
            var data = _content?.GetCapitaine(state.currentCapitaineId);
            var reward = data != null ? data.RewardForce : new BigDouble(0);

            state.force += reward;
            state.totalCapitainesDefeated++;
            GameEvents.RaiseForceChanged();
            if (data != null) GameEvents.RaiseCapitaineDefeated(data, reward);

            _phaseTimer = CapitaineVictorySeconds;
            TransitionTo(state, CombatPhase.CapitaineVictory);
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
            state.currentCapitaineId = null;
            state.currentMaitreId = null;
            state.currentCapitainePhase = 0;
            state.currentMaitrePhase = 0;
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
