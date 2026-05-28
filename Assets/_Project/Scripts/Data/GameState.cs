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
        public int saveVersion = 8;

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

        // -- Souffle (Sprint 6) -------------------------------------------
        /// <summary>UTC seconds-since-startup of the last Souffle trigger. Used to compute cooldown.</summary>
        public float lastSouffleTime;
        /// <summary>Game-time timestamp until which the Souffle buff is active (set to negative when no buff).</summary>
        public float souffleBuffActiveUntil = -1f;

        // -- Maîtres (Sprint 6) -------------------------------------------
        /// <summary>Id of the currently engaged Maître (null outside MaitreIncoming/Active/Victory).</summary>
        public string currentMaitreId;
        /// <summary>Current Maître HP phase: 0 = full, 1 = &lt;75%, 2 = &lt;50%, 3 = &lt;25% (enrage).</summary>
        public int currentMaitrePhase;
        /// <summary>Available Maître invocation slots. Increments per <c>CapitainesPerMaitreSlot</c> kills.</summary>
        public int maitreInvocationSlots;

        // -- Prestige (Sprint 6) ------------------------------------------
        /// <summary>Total Échos accumulated across runs. PERSISTS through prestige.</summary>
        public BigDouble totalEchos;
        /// <summary>Échos earned in the current run (resets at prestige). For preview before the player triggers prestige.</summary>
        public BigDouble currentRunEchosEarned;
        /// <summary>Highest Force value reached in the current run — feeds the Échos formula at prestige.</summary>
        public BigDouble currentRunForceMax;
        /// <summary>Player's chosen final citation (max 80 chars per PrestigeConstants).</summary>
        public string playerCitation = string.Empty;
        /// <summary>True once the citation was confirmed / edited at death for the current run.</summary>
        public bool playerCitationLockedForRun;
        /// <summary>IDs of unique Reliques owned (one per Maître defeated). PERSISTS through prestige.</summary>
        public List<string> relicsOwned = new List<string>();
        /// <summary>IDs of Reliques the player chose to conserve into the next run. Sprint 6 MVP: all relics persist; Sprint 7+ adds selection UI.</summary>
        public List<string> relicsConserved = new List<string>();
        /// <summary>Titles unlocked across all runs ("Vaincu par Yoshitsune", etc.). PERSISTS.</summary>
        public List<string> titlesUnlocked = new List<string>();
        /// <summary>Achievement IDs unlocked. PERSISTS (Sprint 10 will render them).</summary>
        public List<string> achievementsUnlocked = new List<string>();
        /// <summary>Number of prestiges completed. PERSISTS.</summary>
        public int prestigeCount;
        /// <summary>Timestamp of the most recent prestige.</summary>
        public DateTime lastPrestigeAt;
        /// <summary>Hall des Légendes — one record per death vs a Maître. PERSISTS.</summary>
        public List<DeathRecord> deathRecords = new List<DeathRecord>();

        // Sprint 7+ : voies, esprits, regions, lore fragments, etc.
    }
}
