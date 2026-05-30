using System;
using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 6 — Prestige system. Triggered when the player dies vs a Maître.
    ///
    /// Reset matrix (per DESIGN_DECISIONS_LOG):
    ///   RESET: force, upgradeLevels, currentStade, totalAdversairesDefeated, totalCapitainesDefeated,
    ///          maitreInvocationSlots, currentRunEchosEarned, currentRunForceMax, currentElan,
    ///          combat state (phase / chrono / enemy refs)
    ///   PERSIST: totalEchos, relicsOwned, titlesUnlocked, achievementsUnlocked, deathRecords,
    ///            prestigeCount, lastPrestigeAt, playerCitation, lastSouffleTime (Souffle cooldown
    ///            is a learned skill — persists)
    ///   PERSIST (Sprint 7): inventoryLayerSetIds, equippedBodyId, equippedArmorId, equippedWeaponId,
    ///            voieSelectedId, voiesMastered. Gear + voie progression are long-term ladders, not
    ///            run-scoped. Reliques dropped by Maîtres remain owned forever.
    ///
    /// Formula: <c>echos = max(EchosMinReward, floor(log10(forceMax) × EchosFormulaBase))</c>.
    /// </summary>
    public sealed class PrestigeService
    {
        /// <summary>
        /// Preview the Échos the player would gain right now. Safe to call from UI on Update.
        /// </summary>
        public BigDouble CalculateEchosForCurrentRun(GameState state)
        {
            if (state == null) return new BigDouble(0);
            var forceMax = state.currentRunForceMax;
            if (forceMax.Sign() <= 0) return new BigDouble(PrestigeConstants.EchosMinReward);

            var log = BigDouble.Log10(forceMax);
            var raw = Mathf.FloorToInt((float)(log * PrestigeConstants.EchosFormulaBase));
            return new BigDouble(Mathf.Max(PrestigeConstants.EchosMinReward, raw));
        }

        /// <summary>
        /// Trigger the prestige flow (called by CombatProcessor when chrono expires vs a Maître).
        /// Computes Échos, appends to Hall des Légendes, resets RUN fields, transitions to a
        /// "PlayerDeathTemporary"-equivalent phase that the cinematic view consumes.
        /// </summary>
        public void TriggerPrestige(GameState state, MaitreData defeatedBy)
        {
            if (state == null) return;

            var echos = CalculateEchosForCurrentRun(state);

            // 1. Append Hall des Légendes record.
            state.deathRecords.Add(new DeathRecord
            {
                maitreVoie = defeatedBy?.Voie.ToString() ?? "Inconnu",
                maitreName = defeatedBy?.DisplayName ?? "Maître Inconnu",
                playerCitation = state.playerCitation ?? string.Empty,
                prestigeNumber = state.prestigeCount + 1,
                totalForceMaxAtDeath = state.currentRunForceMax.ToString(),
                deathUtc = DateTime.UtcNow
            });

            // 2. Unlock the "Vaincu par X" title if first time.
            if (defeatedBy != null)
            {
                var title = $"Vaincu par {defeatedBy.DisplayName}";
                if (!state.titlesUnlocked.Contains(title)) state.titlesUnlocked.Add(title);
            }

            // 3. Bank Échos + prestige counter (PERSIST).
            state.totalEchos += echos;
            state.prestigeCount++;
            state.lastPrestigeAt = DateTime.UtcNow;

            // 4. Phase transition. The PrestigeCinematicView listens to OnPrestigeTriggered and
            //    drives the visual flow; we move state to PlayerDeathTemporary as a "pause" until
            //    the cinematic finishes (and CompletePrestige resets fully).
            var prevPhase = state.currentPhase;
            state.currentPhase = CombatPhase.PlayerDeathTemporary;
            GameEvents.RaisePhaseChanged(prevPhase, state.currentPhase);

            GameEvents.RaisePrestigeTriggered(defeatedBy, echos);

            // Force-save right away so the prestige is durable even if the cinematic crashes.
            GameManager.Instance?.Save?.ForceSave(state);
        }

        /// <summary>Called by PrestigeCinematicView after Phase 4 (Renaissance) completes.</summary>
        public void CompletePrestige(GameState state)
        {
            if (state == null) return;

            // RESET fields per the matrix.
            state.force = new BigDouble(0);
            state.upgradeLevels.Clear();
            state.currentStade = 1;
            state.totalAdversairesDefeated = 0;
            state.totalCapitainesDefeated = 0;
            state.maitreInvocationSlots = 0;
            state.currentRunEchosEarned = new BigDouble(0);
            state.currentRunForceMax = new BigDouble(0);
            state.playerCitationLockedForRun = false;
            state.chronoRemaining = 0f;
            state.tapsTowardsNextAdversaire = 0;
            state.currentAdversaireId = null;
            state.currentCapitaineId = null;
            state.currentMaitreId = null;
            state.currentAdversaireHp = default;
            state.currentCapitainePhase = 0;
            state.currentMaitrePhase = 0;
            state.currentElan = 0f;
            // Sprint 7 fields intentionally NOT reset here: inventoryLayerSetIds, equippedBodyId,
            // equippedArmorId, equippedWeaponId, voieSelectedId, voiesMastered. See class doc.

            // Phase back to Training; gameplay loop resumes.
            var prevPhase = state.currentPhase;
            state.currentPhase = CombatPhase.Training;
            GameManager.Instance?.Save?.ForceSave(state);

            GameEvents.RaisePhaseChanged(prevPhase, state.currentPhase);
            GameEvents.RaiseForceChanged();
            GameEvents.RaisePrestigeCompleted();
        }
    }
}
