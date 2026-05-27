namespace Saga.Data
{
    /// <summary>
    /// Combat lifecycle phases per GAME_DESIGN_v2 §⚔️ Le Système de Combat.
    /// Sprint 4 implements Training, AdversaireIncoming, AdversaireActive, AdversaireVictory,
    /// PlayerDeathTemporary. Boss phases land in Sprint 5-6.
    /// </summary>
    public enum CombatPhase
    {
        /// <summary>Default state. Player taps the mannequin, generates Force, fills the Prochain Adversaire bar.</summary>
        Training = 0,
        /// <summary>Brief cinematic ~1s: mannequin fades out, adversaire fades in with name reveal. No damage yet.</summary>
        AdversaireIncoming = 1,
        /// <summary>Active combat. Chrono ticks down, taps damage the adversaire. Win=HP≤0, lose=chrono≤0.</summary>
        AdversaireActive = 2,
        /// <summary>~2s celebration: adversaire dies, reward applied, then auto-return to Training.</summary>
        AdversaireVictory = 3,
        /// <summary>Mort temporaire: black overlay + "TU ES MORT" + click-to-resume. -10% Force on resume.</summary>
        PlayerDeathTemporary = 4,

        // Sprint 5 — Capitaines (Boss Mineurs) every 10 adversaires defeated
        /// <summary>~1.5s cinematic: vignette + name letter-by-letter + citation. Adversaire-style fade-in.</summary>
        CapitaineIncoming = 5,
        /// <summary>Active combat with a Capitaine. HP phase thresholds drive bar color, enrage at 25%.</summary>
        CapitaineActive = 6,
        /// <summary>~3s cinematic: slow motion + flash + death citation + "VICTOIRE" + loot drop.</summary>
        CapitaineVictory = 7
    }
}
