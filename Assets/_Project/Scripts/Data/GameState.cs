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
        public int saveVersion = 3;

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

        // -- Tier visuel (Sprint 3) ---------------------------------------
        /// <summary>Current visual tier per <see cref="Stade"/>. Starts at 1 (Mendiant).</summary>
        public int currentStade = 1;

        // Sprint 4+ : aggregates expanded as systems come online
        // (voies, esprits, regions, prestige level, lore, etc.)
    }
}
