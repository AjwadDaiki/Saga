namespace Saga.Data
{
    /// <summary>
    /// Tunables for the Souffle (meditation) mechanic (Sprint 6).
    /// Per DESIGN_DECISIONS_LOG: cooldown is PERSISTENT across prestige —
    /// active skills don't unlearn themselves.
    /// </summary>
    public static class SouffleConstants
    {
        /// <summary>Seconds spent in the meditation phase (no tap input accepted).</summary>
        public const float MeditationDurationSeconds = 5f;

        /// <summary>Seconds the post-meditation buff lasts.</summary>
        public const float BuffDurationSeconds = 30f;

        /// <summary>Multiplier applied to Force/tap during the buff window.</summary>
        public const float BuffForceMultiplier = 1.5f;

        /// <summary>Seconds between two Souffle activations (cooldown).</summary>
        public const float CooldownSeconds = 120f;

        /// <summary>HP regen per second during combat while buff is active (Sprint 6 stores the rate; full HP system Sprint 7+).</summary>
        public const float HpRegenPerSecondDuringCombat = 0.05f;
    }
}
