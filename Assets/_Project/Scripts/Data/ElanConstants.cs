namespace Saga.Data
{
    /// <summary>
    /// Tunables for the Élan / Vague AOE mechanic (Sprint 5).
    /// Centralized here so balance tweaks don't ripple across the codebase.
    /// </summary>
    public static class ElanConstants
    {
        /// <summary>Base Elan gained per tap (Training or Combat).</summary>
        public const float ElanPerTap = 5f;

        /// <summary>Multiplier applied to ElanPerTap when combo tier > 0 (synergy).</summary>
        public const float ElanComboBonusMultiplier = 2f;

        /// <summary>Elan decay per second after <see cref="ElanIdleGraceSeconds"/> without tapping.</summary>
        public const float ElanDecayPerSec = 4f;

        /// <summary>Seconds the system waits after the last tap before decay kicks in.</summary>
        public const float ElanIdleGraceSeconds = 3f;

        /// <summary>Elan cap (also the "full" threshold for the Vague button).</summary>
        public const float ElanMax = 100f;

        // -- Vague AOE (Training) -------------------------------------------

        /// <summary>Multiplier applied to tap gain during the Vague buff window (Training only).</summary>
        public const float VagueTrainingForceMultiplier = 5.0f;

        /// <summary>Seconds the Vague Training buff lasts.</summary>
        public const float VagueTrainingDurationSeconds = 5f;

        // -- Vague AOE (Combat) ---------------------------------------------

        /// <summary>Base damage dealt by a Vague in combat. Scaled per stade in <see cref="Gameplay.VagueResolver"/>.</summary>
        public const float VagueCombatDamageBase = 3000f;
    }
}
