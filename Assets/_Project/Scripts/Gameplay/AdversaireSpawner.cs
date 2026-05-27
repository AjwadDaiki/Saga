using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Tracks the "Prochain Adversaire" bar fill from per-tap progress (Training phase only).
    /// When the threshold for the player's current stade is reached, picks a random adversaire
    /// from <see cref="ContentDatabase"/> and hands it to <see cref="CombatProcessor"/>.
    ///
    /// Threshold per GAME_DESIGN_v2 §Équilibrage: 50/100/200/400/800/1600 taps for stades 1..6.
    /// </summary>
    public sealed class AdversaireSpawner
    {
        private readonly ContentDatabase _content;
        private readonly CombatProcessor _combat;

        public AdversaireSpawner(ContentDatabase content, CombatProcessor combat)
        {
            _content = content;
            _combat = combat;
        }

        /// <summary>Spawn threshold for the player's current stade (1..6). Doubles per stade from 50.</summary>
        public static int GetThresholdForStade(int stade)
        {
            if (stade <= 1) return 50;
            if (stade >= 6) return 1600;
            // 50 * 2^(stade-1) -> 50, 100, 200, 400, 800
            return 50 * (1 << (stade - 1));
        }

        /// <summary>
        /// Call from <see cref="GameEvents.OnTapResolved"/> handler (registered by GameManager).
        /// Increments the per-tap progress; if threshold is crossed and we're in Training,
        /// fires a spawn.
        /// </summary>
        public void OnTap(GameState state)
        {
            if (state == null) return;
            if (state.currentPhase != CombatPhase.Training) return;

            state.tapsTowardsNextAdversaire++;
            var threshold = GetThresholdForStade(state.currentStade);
            GameEvents.RaiseAdversaireProgressUpdated(state.tapsTowardsNextAdversaire, threshold);

            if (state.tapsTowardsNextAdversaire < threshold) return;

            TrySpawn(state);
        }

        private void TrySpawn(GameState state)
        {
            var pool = _content?.AllAdversaires;
            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning("[AdversaireSpawner] No adversaires loaded — run \"Saga > Sprint 4 > Generate Adversaire Assets\".");
                state.tapsTowardsNextAdversaire = 0;
                return;
            }

            var picked = pool[Random.Range(0, pool.Count)];
            state.tapsTowardsNextAdversaire = 0;
            GameEvents.RaiseAdversaireProgressUpdated(0, GetThresholdForStade(state.currentStade));
            _combat?.StartIncoming(state, picked);
        }
    }
}
