using BreakInfinity;
using Saga.Core;
using Saga.Data;

namespace Saga.Gameplay
{
    /// <summary>
    /// Watches <see cref="GameState.force"/> against the visual-tier thresholds
    /// (per 04_PROGRESSION.md) and bumps <see cref="GameState.currentStade"/>
    /// + raises <see cref="GameEvents.OnStadeChanged"/> when a threshold is crossed.
    ///
    /// Tick-driven (10Hz via GameTicker) — same cadence as the rest of the engine.
    /// Cheap: just a Force comparison vs a sorted threshold table.
    /// </summary>
    public sealed class StadeManager
    {
        // Force thresholds for each stade beyond Mendiant. Index = (Stade - 1).
        // Sprint 3 only renders Stade 2 visually; thresholds for 3+ are tracked but visuals
        // come in later sprints. Doc 04_PROGRESSION.md is the source of truth for tuning.
        private static readonly BigDouble[] _thresholds =
        {
            new BigDouble(0),         // Mendiant     — start
            new BigDouble(1_000),     // Apprenti     — ~1k
            new BigDouble(100_000),   // Guerrier     — ~100k
            new BigDouble(10_000_000),// Maître       — ~10M
            new BigDouble(1_000_000_000), // Légende  — ~1B
            BigDouble.Pow(10, 15)     // Mythe        — ~1aa
        };

        /// <summary>Stade given a Force value, ignoring prestige unlocks (Sprint 5+).</summary>
        public static int ComputeStade(BigDouble force)
        {
            var stade = 1;
            for (var i = _thresholds.Length - 1; i >= 0; i--)
            {
                if (force >= _thresholds[i])
                {
                    stade = i + 1;
                    break;
                }
            }
            return stade;
        }

        /// <summary>Drive from <see cref="GameTicker"/>. Raises events only on actual transitions.</summary>
        public void Tick(GameState state, float dt)
        {
            if (state == null) return;
            var newStade = ComputeStade(state.force);
            if (newStade == state.currentStade) return;

            var previous = state.currentStade;
            state.currentStade = newStade;
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseStadeChanged(previous, newStade);
        }
    }
}
