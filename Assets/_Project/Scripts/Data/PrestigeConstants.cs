namespace Saga.Data
{
    /// <summary>
    /// Tunables for the Prestige system (Sprint 6).
    /// Formula: <c>echos = max(MinReward, floor(log10(forceMax) × FormulaBase))</c>.
    /// </summary>
    public static class PrestigeConstants
    {
        /// <summary>Coefficient on log10(forceMax). Higher = more Échos per run.</summary>
        public const int EchosFormulaBase = 10;

        /// <summary>Minimum Échos guaranteed per prestige (so a short run still grants something).</summary>
        public const int EchosMinReward = 10;

        /// <summary>Capitaines defeated needed to unlock one Maître invocation slot.</summary>
        public const int CapitainesPerMaitreSlot = 3;

        /// <summary>Max characters allowed in the player's final citation.</summary>
        public const int PlayerCitationMaxLength = 80;
    }
}
