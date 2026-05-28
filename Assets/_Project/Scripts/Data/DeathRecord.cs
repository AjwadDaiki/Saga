using System;

namespace Saga.Data
{
    /// <summary>
    /// One entry in the player's Hall des Légendes — appended on every prestige.
    /// Sprint 6: stored in GameState.deathRecords. Sprint 10: rendered in the Hall UI.
    /// </summary>
    [Serializable]
    public class DeathRecord
    {
        public string maitreVoie;            // e.g. "Samurai"
        public string maitreName;            // e.g. "Yoshitsune l'Inatteignable"
        public string playerCitation;        // the citation the player wrote / accepted at death
        public int    prestigeNumber;        // 1, 2, 3, ...
        public string totalForceMaxAtDeath;  // serialized BigDouble (string for save robustness)
        public DateTime deathUtc;
    }
}
