using System;
using BreakInfinity;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Static C# event channels for cross-system notification.
    /// Sprint 1 minimal — migrate to ScriptableObject event channels (per 07_ARCHITECTURE.md §3)
    /// when surface area grows in Sprint 3+ (multiple voies, esprits, milestones).
    ///
    /// Convention: handlers MUST unsubscribe in OnDisable/OnDestroy to avoid leaks
    /// across scene reloads (static events outlive scenes).
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised when GameState.force changes (tap, upgrade purchase, passive tick, load).</summary>
        public static event Action OnForceChanged;

        /// <summary>
        /// Raised when the combo tier changes. Args: (tier 0..3, base multiplier for that tier).
        /// Multiplier here is the BASE combo curve value — apply <see cref="Saga.Gameplay.StatsCalculator.GetComboMultiplierBonus"/>
        /// at the consumer side if you need the final post-upgrade combo multiplier.
        /// </summary>
        public static event Action<int, float> OnComboChanged;

        /// <summary>
        /// Raised after a tap is resolved. Args: gain, total multiplier applied (base × bonus),
        /// screen-space tap pos. UI uses this to spawn floating +X numbers and FX.
        /// </summary>
        public static event Action<BigDouble, float, Vector2> OnTapResolved;

        /// <summary>Raised after an upgrade purchase succeeds. Args: upgradeId, new level.</summary>
        public static event Action<string, int> OnUpgradePurchased;

        public static void RaiseForceChanged() => OnForceChanged?.Invoke();

        public static void RaiseComboChanged(int tier, float baseMultiplier)
            => OnComboChanged?.Invoke(tier, baseMultiplier);

        public static void RaiseTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
            => OnTapResolved?.Invoke(gain, multiplier, screenPos);

        public static void RaiseUpgradePurchased(string upgradeId, int newLevel)
            => OnUpgradePurchased?.Invoke(upgradeId, newLevel);
    }
}
