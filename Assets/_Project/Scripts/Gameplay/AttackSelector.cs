using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Weighted random selector for the 3 attack animations (attack1/2/3).
    /// Distribution depends on the combo tier — the higher the combo, the more often
    /// the flashier attacks (attack2, attack3) come out. Pure visual flavor, no stat
    /// impact (just reward feel).
    ///
    /// Tier 0 (×1.0) : attack1 80%, attack2 18%, attack3 2%
    /// Tier 1 (×1.2) : attack1 65%, attack2 30%, attack3 5%
    /// Tier 2 (×1.5) : attack1 45%, attack2 45%, attack3 10%
    /// Tier 3 (×2.0) : attack1 25%, attack2 55%, attack3 20%
    /// </summary>
    public static class AttackSelector
    {
        // [tier][0..2] = cumulative probabilities for (attack1, attack2, attack3).
        // Pre-cumulated for a single comparison per roll.
        private static readonly float[][] _cumulativeDistributions =
        {
            new[] { 0.80f, 0.98f, 1.00f }, // tier 0
            new[] { 0.65f, 0.95f, 1.00f }, // tier 1
            new[] { 0.45f, 0.90f, 1.00f }, // tier 2
            new[] { 0.25f, 0.80f, 1.00f }, // tier 3
        };

        private static readonly string[] _attackNames = { "attack1", "attack2", "attack3" };

        /// <summary>Roll an attack animation name for the given combo tier (0..3). Out-of-range tiers are clamped.</summary>
        public static string Select(int comboTier)
        {
            var tier = Mathf.Clamp(comboTier, 0, _cumulativeDistributions.Length - 1);
            var dist = _cumulativeDistributions[tier];
            var roll = Random.value; // 0..1, uniform
            for (var i = 0; i < dist.Length; i++)
            {
                if (roll < dist[i]) return _attackNames[i];
            }
            return _attackNames[_attackNames.Length - 1]; // fallback (shouldn't reach)
        }
    }
}
