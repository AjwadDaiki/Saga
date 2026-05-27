using System;
using BreakInfinity;
using Saga.Data;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Static C# event channels for cross-system notification.
    /// Sprint 1-3 minimal — migration to ScriptableObject event channels deferred
    /// per 07_ARCHITECTURE.md §3 (will revisit Sprint 7+ when systems grow further).
    ///
    /// Convention: handlers MUST unsubscribe in OnDisable/OnDestroy to avoid leaks
    /// across scene reloads (static events outlive scenes).
    /// </summary>
    public static class GameEvents
    {
        // ----- Sprint 1: tap loop ---------------------------------------

        /// <summary>Raised when GameState.force changes (tap, upgrade purchase, passive tick, load, adversaire reward, mort temporaire penalty).</summary>
        public static event Action OnForceChanged;

        /// <summary>
        /// Raised when the combo tier changes. Args: (tier 0..3, base multiplier for that tier).
        /// Multiplier here is the BASE combo curve value — apply <see cref="Saga.Gameplay.StatsCalculator.GetComboMultiplierBonus"/>
        /// at the consumer side if you need the final post-upgrade combo multiplier.
        /// </summary>
        public static event Action<int, float> OnComboChanged;

        /// <summary>
        /// Raised after a tap is resolved. Args: gain/damage value, total multiplier (base × bonus),
        /// screen-space tap pos. UI uses this to spawn floating numbers and FX. In AdversaireActive
        /// the "gain" represents the damage applied, not Force added (Force only grows in Training).
        /// </summary>
        public static event Action<BigDouble, float, Vector2> OnTapResolved;

        // ----- Sprint 2: upgrades ---------------------------------------

        /// <summary>Raised after an upgrade purchase succeeds. Args: upgradeId, new level.</summary>
        public static event Action<string, int> OnUpgradePurchased;

        // ----- Sprint 3: stade visuel -----------------------------------

        /// <summary>Raised when the character visual tier crosses a threshold. Args: previousStade, newStade.</summary>
        public static event Action<int, int> OnStadeChanged;

        // ----- Sprint 4: combat active system ---------------------------

        /// <summary>Combat lifecycle transition. Args: previous phase, next phase.</summary>
        public static event Action<CombatPhase, CombatPhase> OnPhaseChanged;

        /// <summary>Fired when the spawner picks an adversaire and combat begins (AdversaireIncoming).</summary>
        public static event Action<AdversaireData> OnAdversaireSpawned;

        /// <summary>Fired per damage tick. Args: damage dealt this tap, adversaire HP after, adversaire max HP.</summary>
        public static event Action<BigDouble, BigDouble, BigDouble> OnAdversaireDamaged;

        /// <summary>Fired when the adversaire dies. Args: the AdversaireData, the Force reward granted.</summary>
        public static event Action<AdversaireData, BigDouble> OnAdversaireDefeated;

        /// <summary>Fired when the chrono runs out before victory. UI handles the black overlay + click-to-resume.</summary>
        public static event Action OnPlayerDiedTemporary;

        /// <summary>Per-tick chrono update during AdversaireActive. Args: seconds remaining, original total seconds.</summary>
        public static event Action<float, float> OnChronoUpdated;

        /// <summary>Per-tap progress toward next adversaire spawn (during Training only). Args: current taps, threshold.</summary>
        public static event Action<int, int> OnAdversaireProgressUpdated;

        // ----- Raisers --------------------------------------------------

        public static void RaiseForceChanged() => OnForceChanged?.Invoke();

        public static void RaiseComboChanged(int tier, float baseMultiplier)
            => OnComboChanged?.Invoke(tier, baseMultiplier);

        public static void RaiseTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
            => OnTapResolved?.Invoke(gain, multiplier, screenPos);

        public static void RaiseUpgradePurchased(string upgradeId, int newLevel)
            => OnUpgradePurchased?.Invoke(upgradeId, newLevel);

        public static void RaiseStadeChanged(int previousStade, int newStade)
            => OnStadeChanged?.Invoke(previousStade, newStade);

        public static void RaisePhaseChanged(CombatPhase previous, CombatPhase next)
            => OnPhaseChanged?.Invoke(previous, next);

        public static void RaiseAdversaireSpawned(AdversaireData data)
            => OnAdversaireSpawned?.Invoke(data);

        public static void RaiseAdversaireDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)
            => OnAdversaireDamaged?.Invoke(damage, currentHp, maxHp);

        public static void RaiseAdversaireDefeated(AdversaireData data, BigDouble reward)
            => OnAdversaireDefeated?.Invoke(data, reward);

        public static void RaisePlayerDiedTemporary()
            => OnPlayerDiedTemporary?.Invoke();

        public static void RaiseChronoUpdated(float remaining, float total)
            => OnChronoUpdated?.Invoke(remaining, total);

        public static void RaiseAdversaireProgressUpdated(int currentTaps, int threshold)
            => OnAdversaireProgressUpdated?.Invoke(currentTaps, threshold);
    }
}
