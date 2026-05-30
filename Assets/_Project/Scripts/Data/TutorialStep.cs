using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// Sprint 8 Phase A — onboarding tutorial step.
    /// Materialized as <c>Resources/Tutorial/Step_NN_xxx.asset</c> SOs.
    /// <see cref="Saga.Tutorial.TutorialService"/> reads them in order, subscribes to the
    /// step's <see cref="trigger"/>, and emits <c>GameEvents.OnTutorialStepShown</c> +
    /// <c>OnTutorialStepCompleted</c> for the overlay view to render / dismiss.
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Saga/Tutorial Step")]
    public class TutorialStep : ScriptableObject
    {
        /// <summary>UI element the highlight ring + speech bubble tail point at.</summary>
        public enum AnchorTarget
        {
            CombatZone,
            ForcePill,
            UpgradeStrike,
            StageChip,
            VagueButton,
            SettingsButton,
        }

        /// <summary>Event that completes the current step + advances to the next.</summary>
        public enum TriggerCondition
        {
            /// <summary>Step shown immediately at first boot (initial prompt).</summary>
            Boot,
            /// <summary>OnTapResolved cumulative count ≥ <see cref="triggerThreshold"/>.</summary>
            TapsReached,
            /// <summary>OnUpgradePurchased fired for any upgrade.</summary>
            UpgradePurchased,
            /// <summary>OnAdversaireDefeated or OnStadeChanged fires (player progressing).</summary>
            StageProgressed,
            /// <summary>OnElanChanged with current = max (Vague ready).</summary>
            ElanReachedMax,
            /// <summary>OnVagueTriggered fired (first time).</summary>
            VagueTriggered,
        }

        [Tooltip("Stable identifier — used as save key for completion + ordering.")]
        public string id;

        [Tooltip("UI element the highlight ring + speech bubble point at.")]
        public AnchorTarget anchor;

        [Tooltip("Event that completes the step.")]
        public TriggerCondition trigger;

        [Tooltip("Numeric threshold (used by TapsReached). Ignored otherwise.")]
        public int triggerThreshold;

        [TextArea(2, 4)]
        [Tooltip("Message FR (max 2 lines / ~60 chars).")]
        public string messageFr;
    }
}
