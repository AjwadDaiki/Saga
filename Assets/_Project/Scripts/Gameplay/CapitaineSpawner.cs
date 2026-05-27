using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Spawns a Capitaine every 10 adversaires defeated. Listens to
    /// <see cref="GameEvents.OnAdversaireDefeated"/>; when the cadence matches, queues
    /// a pending Capitaine. <see cref="CombatProcessor"/> consumes the queue when
    /// transitioning out of <see cref="CombatPhase.AdversaireVictory"/> (preempts the
    /// Training fallback).
    /// </summary>
    public sealed class CapitaineSpawner
    {
        public const int CapitaineEvery = 10;

        private readonly ContentDatabase _content;
        private CapitaineData _pending;

        public CapitaineSpawner(ContentDatabase content)
        {
            _content = content;
            GameEvents.OnAdversaireDefeated += HandleAdversaireDefeated;
        }

        /// <summary>True if a Capitaine is queued (set after every 10th adversaire kill).</summary>
        public bool HasPending => _pending != null;

        /// <summary>Returns and clears the pending Capitaine. Used by CombatProcessor.</summary>
        public CapitaineData ConsumePending()
        {
            var p = _pending;
            _pending = null;
            return p;
        }

        private void HandleAdversaireDefeated(AdversaireData adversaire, BreakInfinity.BigDouble reward)
        {
            var gm = GameManager.Instance;
            if (gm?.State == null) return;
            if (gm.State.totalAdversairesDefeated % CapitaineEvery != 0) return;

            var pool = _content?.AllCapitaines;
            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning("[CapitaineSpawner] No capitaines loaded — run \"Saga > Sprint 5 > Generate Capitaine Assets\".");
                return;
            }

            _pending = pool[Random.Range(0, pool.Count)];
        }
    }
}
