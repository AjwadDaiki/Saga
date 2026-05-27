using System;
using System.Collections.Generic;
using BreakInfinity;

namespace Saga.Data
{
    /// <summary>
    /// Single source of truth for the game session.
    /// Persisted via SaveService. Bump <see cref="saveVersion"/> on schema change
    /// and provide a migration in SaveService.Migrate().
    /// </summary>
    [Serializable]
    public class GameState
    {
        /// <summary>Schema version. Bump on breaking change. Migration handled by SaveService.</summary>
        public int saveVersion = 2;

        // -- Currencies (BigDouble — protect against 1e15+ ceiling) -------
        public BigDouble force;
        public BigDouble technique;
        public BigDouble renom;
        public BigDouble echos; // persistent across prestige

        // -- Lifetime stats ------------------------------------------------
        public int totalTaps;
        public DateTime gameStarted = DateTime.UtcNow;
        public DateTime lastSession = DateTime.UtcNow;

        // -- Upgrades (Sprint 2) ------------------------------------------
        /// <summary>Map from <c>UpgradeData.UpgradeId</c> to current level (0 = unpurchased).</summary>
        public Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

        // Sprint 3+ : aggregates expanded as systems come online
        // (voies, esprits, regions, prestige level, lore, etc.)
    }
}
