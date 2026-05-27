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
        public int saveVersion = 6;

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

        // -- Combat active system (Sprint 4) ------------------------------
        /// <summary>Current lifecycle phase per <see cref="CombatPhase"/>. Default Training.</summary>
        public CombatPhase currentPhase = CombatPhase.Training;
        /// <summary>Id of the currently engaged adversaire (null if none).</summary>
        public string currentAdversaireId;
        /// <summary>Live HP of the current adversaire during AdversaireActive.</summary>
        public BigDouble currentAdversaireHp;
        /// <summary>Seconds left on the combat chrono (counts down during AdversaireActive).</summary>
        public float chronoRemaining;
        /// <summary>Taps accumulated since last spawn (or last reset) toward the next adversaire.</summary>
        public int tapsTowardsNextAdversaire;
        /// <summary>Lifetime count of adversaires defeated (drives Capitaine/Maître progression Sprint 5+).</summary>
        public int totalAdversairesDefeated;

        // -- Élan / Vague AOE (Sprint 5) ----------------------------------
        /// <summary>Élan gauge value, 0..100. Fills on taps, decays after idle grace.</summary>
        public float currentElan;
        /// <summary>UTC seconds since boot of the last Vague trigger (reserved for future cooldown).</summary>
        public float lastVagueTime;

        // -- Capitaines (Sprint 5) ----------------------------------------
        /// <summary>Lifetime count of Capitaines defeated (gates Maître access in Sprint 6).</summary>
        public int totalCapitainesDefeated;
        /// <summary>Id of the currently engaged Capitaine (null outside CapitaineIncoming/Active/Victory).</summary>
        public string currentCapitaineId;
        /// <summary>Current Capitaine HP phase: 0 = full, 1 = &lt;75%, 2 = &lt;50%, 3 = &lt;25% (enrage).</summary>
        public int currentCapitainePhase;

        // Sprint 6+ : aggregates expanded as systems come online
        // (Maîtres, voies, esprits, regions, prestige level, lore, etc.)
    }
}
