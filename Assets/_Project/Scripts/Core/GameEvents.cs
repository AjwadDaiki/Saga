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

        // ----- Sprint 5: Élan + Vague AOE -------------------------------

        /// <summary>Per-tick Élan gauge update. Args: current value (0..max), max (typically 100).</summary>
        public static event Action<float, float> OnElanChanged;

        /// <summary>Fired the moment Élan reaches the cap (transition from below 100 → 100). Drives the Vague button reveal.</summary>
        public static event Action OnElanFull;

        /// <summary>Fired when the player clicks the Vague button. Args: combat phase when triggered.</summary>
        public static event Action<CombatPhase> OnVagueTriggered;

        /// <summary>Resolution of the Vague: in Training = the multiplier applied, in Combat = the damage dealt.</summary>
        public static event Action<BigDouble> OnVagueResolved;

        /// <summary>Toggles the Vague Training buff. True = buff active (×5 Force), false = buff ended.</summary>
        public static event Action<bool> OnVagueBuffActive;

        // ----- Sprint 5: Capitaines (Boss Mineurs) ----------------------

        /// <summary>Fired when a Capitaine engagement starts (CapitaineIncoming phase).</summary>
        public static event Action<CapitaineData> OnCapitaineSpawned;

        /// <summary>Fired when the Capitaine's HP crosses a phase threshold. Args: previous phase index, new phase index (0..3 typically).</summary>
        public static event Action<int, int> OnCapitainePhaseChanged;

        /// <summary>Fired once when the Capitaine enters the enrage phase (~25% HP).</summary>
        public static event Action OnCapitaineEnraged;

        /// <summary>Fired when the Capitaine dies. Args: the CapitaineData, the Force reward granted.</summary>
        public static event Action<CapitaineData, BigDouble> OnCapitaineDefeated;

        // ----- Sprint 6: Souffle (meditation buff) ----------------------

        /// <summary>Fired when the player enters the 5s meditation window (taps blocked).</summary>
        public static event Action OnSouffleStarted;

        /// <summary>Fired at the end of the meditation window (right before buff starts).</summary>
        public static event Action OnSouffleEnded;

        /// <summary>Fired at the start of the post-meditation buff. Args: buff duration seconds.</summary>
        public static event Action<float> OnSouffleBuffStarted;

        /// <summary>Fired when the buff expires.</summary>
        public static event Action OnSouffleBuffEnded;

        /// <summary>Per-tick cooldown update. Args: seconds remaining, total cooldown.</summary>
        public static event Action<float, float> OnSouffleCooldownUpdated;

        // ----- Sprint 6: Maîtres (Boss Majeurs) -------------------------

        /// <summary>Fired when the player unlocks a new Maître invocation slot (every X Capitaines defeated).</summary>
        public static event Action OnMaitreUnlocked;

        /// <summary>Fired when the player invokes a Maître (CapitaineIncoming/Active flow begins).</summary>
        public static event Action<MaitreData> OnMaitreSpawned;

        /// <summary>Fired when the Maître's HP crosses a phase threshold. Args: previous phase, new phase.</summary>
        public static event Action<int, int> OnMaitrePhaseChanged;

        /// <summary>Fired when the player defeats the Maître. Args: MaitreData, Force reward.</summary>
        public static event Action<MaitreData, BigDouble> OnMaitreDefeated;

        // ----- Sprint 6: Prestige ---------------------------------------

        /// <summary>Fired when prestige starts (player died vs a Maître). Args: defeating Maître, Échos earned.</summary>
        public static event Action<MaitreData, BigDouble> OnPrestigeTriggered;

        /// <summary>Fired when the prestige cinematic completes and gameplay resumes at stade 1.</summary>
        public static event Action OnPrestigeCompleted;

        // ----- Sprint 7: Equipment + Voies + Sprite Layers --------------

        /// <summary>
        /// Fired when an equipment slot changes. Args: slot, new SpriteLayerSet (null if cleared),
        /// previous SpriteLayerSet (null if slot was empty). UI + character renderer subscribe.
        /// </summary>
        public static event Action<EquipmentSlot, SpriteLayerSet, SpriteLayerSet> OnEquipmentChanged;

        /// <summary>Fired when a SpriteLayerSet is added to the inventory (drop, prestige, debug). Args: the added layer set.</summary>
        public static event Action<SpriteLayerSet> OnItemAddedToInventory;

        /// <summary>Fired when the player selects a Voie. Args: previous, new.</summary>
        public static event Action<Voie, Voie> OnVoieSelected;

        /// <summary>Fired when the player masters a new Voie (Sprint 7+ progression). Args: voie.</summary>
        public static event Action<Voie> OnVoieMastered;

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

        public static void RaiseElanChanged(float current, float max)
            => OnElanChanged?.Invoke(current, max);

        public static void RaiseElanFull()
            => OnElanFull?.Invoke();

        public static void RaiseVagueTriggered(CombatPhase phase)
            => OnVagueTriggered?.Invoke(phase);

        public static void RaiseVagueResolved(BigDouble damageOrBuffApplied)
            => OnVagueResolved?.Invoke(damageOrBuffApplied);

        public static void RaiseVagueBuffActive(bool active)
            => OnVagueBuffActive?.Invoke(active);

        public static void RaiseCapitaineSpawned(CapitaineData data)
            => OnCapitaineSpawned?.Invoke(data);

        public static void RaiseCapitainePhaseChanged(int previous, int next)
            => OnCapitainePhaseChanged?.Invoke(previous, next);

        public static void RaiseCapitaineEnraged()
            => OnCapitaineEnraged?.Invoke();

        public static void RaiseCapitaineDefeated(CapitaineData data, BigDouble reward)
            => OnCapitaineDefeated?.Invoke(data, reward);

        public static void RaiseSouffleStarted() => OnSouffleStarted?.Invoke();
        public static void RaiseSouffleEnded() => OnSouffleEnded?.Invoke();
        public static void RaiseSouffleBuffStarted(float duration) => OnSouffleBuffStarted?.Invoke(duration);
        public static void RaiseSouffleBuffEnded() => OnSouffleBuffEnded?.Invoke();
        public static void RaiseSouffleCooldownUpdated(float remaining, float total)
            => OnSouffleCooldownUpdated?.Invoke(remaining, total);

        public static void RaiseMaitreUnlocked() => OnMaitreUnlocked?.Invoke();
        public static void RaiseMaitreSpawned(MaitreData data) => OnMaitreSpawned?.Invoke(data);
        public static void RaiseMaitrePhaseChanged(int prev, int next) => OnMaitrePhaseChanged?.Invoke(prev, next);
        public static void RaiseMaitreDefeated(MaitreData data, BigDouble reward)
            => OnMaitreDefeated?.Invoke(data, reward);

        public static void RaisePrestigeTriggered(MaitreData defeatedBy, BigDouble echos)
            => OnPrestigeTriggered?.Invoke(defeatedBy, echos);
        public static void RaisePrestigeCompleted() => OnPrestigeCompleted?.Invoke();

        public static void RaiseEquipmentChanged(EquipmentSlot slot, SpriteLayerSet next, SpriteLayerSet previous)
            => OnEquipmentChanged?.Invoke(slot, next, previous);

        public static void RaiseItemAddedToInventory(SpriteLayerSet layerSet)
            => OnItemAddedToInventory?.Invoke(layerSet);

        public static void RaiseVoieSelected(Voie previous, Voie next)
            => OnVoieSelected?.Invoke(previous, next);

        public static void RaiseVoieMastered(Voie voie)
            => OnVoieMastered?.Invoke(voie);
    }
}
