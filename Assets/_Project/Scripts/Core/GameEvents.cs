using System;
using BreakInfinity;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Static C# event channels for cross-system notification.
    /// Sprint 1 minimal — migrate to ScriptableObject event channels (per 07_ARCHITECTURE.md §3)
    /// when surface area grows in Sprint 2+ (multiple upgrades, esprits, milestones).
    ///
    /// Convention: handlers MUST unsubscribe in OnDisable/OnDestroy to avoid leaks
    /// across scene reloads (static events outlive scenes).
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised when GameState.force changes (tap, upgrade, offline gains, load).</summary>
        public static event Action OnForceChanged;

        /// <summary>Raised when the combo step changes (0..N). 0 = no combo, 1+ = active multiplier tier.</summary>
        public static event Action<int> OnComboChanged;

        /// <summary>
        /// Raised after a tap is resolved. Args: gain, multiplier applied (e.g. 1.5), world-space tap pos.
        /// UI uses this to spawn floating +X numbers and FX.
        /// </summary>
        public static event Action<BigDouble, float, Vector2> OnTapResolved;

        public static void RaiseForceChanged() => OnForceChanged?.Invoke();

        public static void RaiseComboChanged(int comboStep) => OnComboChanged?.Invoke(comboStep);

        public static void RaiseTapResolved(BigDouble gain, float multiplier, Vector2 worldPos)
            => OnTapResolved?.Invoke(gain, multiplier, worldPos);
    }
}
