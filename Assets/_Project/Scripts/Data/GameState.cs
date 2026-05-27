using System;
using BreakInfinity;

namespace Saga.Data
{
    /// <summary>
    /// Single source of truth for the game session.
    /// Persisted via SaveService. Bump <see cref="saveVersion"/> on schema change
    /// and provide a migration in SaveService.Load().
    /// </summary>
    [Serializable]
    public class GameState
    {
        /// <summary>Schema version. Bump on breaking change.</summary>
        public int saveVersion = 1;

        // -- Currencies (BigDouble — protect against 1e15+ ceiling) -------
        public BigDouble force;
        public BigDouble technique;
        public BigDouble renom;
        public BigDouble echos; // persistent across prestige

        // -- Lifetime stats ------------------------------------------------
        public int totalTaps;
        public DateTime gameStarted = DateTime.UtcNow;
        public DateTime lastSession = DateTime.UtcNow;

        // Sprint 1+ : aggregates expanded as systems come online
        // (disciples, voies, esprits, regions, prestige level, lore, etc.)
    }
}
