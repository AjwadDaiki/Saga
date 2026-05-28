using System;
using System.Collections.Generic;
using BreakInfinity;
using Saga.Core;
using Saga.Data;

namespace Saga.Gameplay
{
    /// <summary>
    /// Unlocks and invokes Maîtres légendaires (Sprint 6).
    ///
    /// Unlock cadence: every <see cref="PrestigeConstants.CapitainesPerMaitreSlot"/> (3) Capitaines
    /// defeated grants one invocation slot. Slots are stored on the GameState and persist within a run.
    /// At prestige, slots reset (alongside totalCapitainesDefeated).
    ///
    /// Invocation: the player picks ONE of the 8 Maîtres via UI when slots ≥ 1.
    /// <see cref="CombatProcessor.StartMaitreIncoming"/> takes over from there.
    /// </summary>
    public sealed class MaitreSpawner
    {
        private readonly ContentDatabase _content;

        public MaitreSpawner(ContentDatabase content)
        {
            _content = content;
            GameEvents.OnCapitaineDefeated += HandleCapitaineDefeated;
        }

        /// <summary>Returns all 8 Maîtres when at least one invocation slot is free, otherwise empty.</summary>
        public IReadOnlyList<MaitreData> GetAvailableMaitres()
        {
            var gm = GameManager.Instance;
            if (gm?.State == null) return Array.Empty<MaitreData>();
            if (gm.State.maitreInvocationSlots <= 0) return Array.Empty<MaitreData>();
            return _content?.AllMaitres ?? (IReadOnlyList<MaitreData>)Array.Empty<MaitreData>();
        }

        /// <summary>Spend one slot and start the Maître Incoming cinematic. False if ineligible.</summary>
        public bool TryInvokeMaitre(GameState state, string maitreId)
        {
            if (state == null) return false;
            if (state.maitreInvocationSlots <= 0) return false;
            if (state.currentPhase != CombatPhase.Training) return false;
            var data = _content?.GetMaitre(maitreId);
            if (data == null) return false;

            state.maitreInvocationSlots--;
            GameManager.Instance?.Save?.MarkDirty();
            GameManager.Instance?.Combat?.StartMaitreIncoming(state, data);
            return true;
        }

        private void HandleCapitaineDefeated(CapitaineData _, BigDouble __)
        {
            var gm = GameManager.Instance;
            if (gm?.State == null) return;
            var killCount = gm.State.totalCapitainesDefeated;
            if (killCount <= 0) return;
            if (killCount % PrestigeConstants.CapitainesPerMaitreSlot != 0) return;

            gm.State.maitreInvocationSlots++;
            gm.Save?.MarkDirty();
            GameEvents.RaiseMaitreUnlocked();
        }
    }
}
