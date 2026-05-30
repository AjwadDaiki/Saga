using System.Collections.Generic;
using System.Linq;
using Saga.Core;
using Saga.Data;
using Saga.Save;
using UnityEngine;

namespace Saga.Tutorial
{
    /// <summary>
    /// Sprint 8 Phase A — drives the first-session onboarding tutorial.
    ///
    /// Loads <see cref="TutorialStep"/> SOs from <c>Resources/Tutorial/</c> (sorted by asset
    /// filename so <c>Step_01_*</c> comes before <c>Step_02_*</c>), then for each step in turn
    /// subscribes to the relevant <see cref="GameEvents"/> trigger. When the trigger fires,
    /// raises <c>OnTutorialStepCompleted</c>, persists the step index, and advances to the next
    /// step (raising <c>OnTutorialStepShown</c>). When the last step is done, raises
    /// <c>OnTutorialFinished</c> + sets <c>GameState.tutorialDone = true</c>.
    ///
    /// Player can <see cref="Skip"/> at any time → marks done + raises Finished.
    ///
    /// Per Q1 coordinateur : tutorialDone PERSISTS cross-prestige (no re-prompt for veterans).
    /// Per Q5 : no timeout / re-prompt — player advances at own pace.
    /// </summary>
    public class TutorialService
    {
        private readonly GameState _state;
        private readonly SaveService _save;
        private readonly List<TutorialStep> _steps;
        private TutorialStep _current;
        private int _runtimeTapsAccumulator; // for TapsReached triggers (counted from boot)

        public TutorialService(GameState state, SaveService save, IList<TutorialStep> testSteps = null)
        {
            _state = state;
            _save = save;
            if (testSteps != null)
            {
                _steps = new List<TutorialStep>(testSteps);
            }
            else
            {
                _steps = Resources.LoadAll<TutorialStep>("Tutorial")
                    .OrderBy(s => s.name)
                    .ToList();
            }
        }

        /// <summary>True if the tutorial is currently active (not yet done + has remaining steps).</summary>
        public bool IsActive => !_state.tutorialDone && _state.tutorialStepIndex < _steps.Count;

        /// <summary>Total step count loaded from Resources.</summary>
        public int StepCount => _steps.Count;

        /// <summary>Currently displayed step (null if not active).</summary>
        public TutorialStep CurrentStep => _current;

        /// <summary>
        /// Starts driving the tutorial. Called by GameManager.Awake after services init.
        /// If <see cref="GameState.tutorialDone"/> is true (legacy save or skipped), no-op.
        /// </summary>
        public void Start()
        {
            if (_state.tutorialDone) return;
            if (_steps.Count == 0)
            {
                Debug.LogWarning("[Tutorial] No steps found in Resources/Tutorial/ — run menu 'Saga > Sprint 8 > Generate Tutorial Steps'. Auto-marking tutorial done to avoid blocking the player.");
                MarkDone();
                return;
            }
            // Clamp persisted index (in case content shrunk between sessions).
            if (_state.tutorialStepIndex < 0) _state.tutorialStepIndex = 0;
            if (_state.tutorialStepIndex >= _steps.Count)
            {
                MarkDone();
                return;
            }
            SubscribeGlobal();
            ShowCurrent();
        }

        /// <summary>Skip the tutorial entirely — marks done + raises Finished.</summary>
        public void Skip()
        {
            UnsubscribeAll();
            MarkDone();
        }

        // ----- Internal flow -----

        private void ShowCurrent()
        {
            _current = _steps[_state.tutorialStepIndex];
            GameEvents.RaiseTutorialStepShown(_current);
            // Subscribe to the specific trigger of this step.
            SubscribeTrigger(_current);
        }

        private void Advance()
        {
            UnsubscribeTrigger(_current);
            var completedId = _current.id;
            _state.tutorialStepIndex++;
            _save?.ForceSave(_state);
            GameEvents.RaiseTutorialStepCompleted(completedId);

            if (_state.tutorialStepIndex >= _steps.Count)
            {
                UnsubscribeGlobal();
                MarkDone();
                return;
            }
            ShowCurrent();
        }

        private void MarkDone()
        {
            _state.tutorialDone = true;
            _save?.ForceSave(_state);
            _current = null;
            GameEvents.RaiseTutorialFinished();
        }

        // ----- Triggers -----

        /// <summary>Subscribe to events that are needed for cumulative trigger conditions (TapsReached).</summary>
        private void SubscribeGlobal()
        {
            GameEvents.OnTapResolved += HandleAnyTap;
        }

        private void UnsubscribeGlobal()
        {
            GameEvents.OnTapResolved -= HandleAnyTap;
        }

        private void SubscribeTrigger(TutorialStep step)
        {
            switch (step.trigger)
            {
                case TutorialStep.TriggerCondition.Boot:
                    // Already shown by ShowCurrent — auto-advance on a 1s grace window? For simplicity
                    // Boot trigger means "show + wait for player tap on overlay dismiss" — handled by
                    // the overlay calling TryAdvanceForBoot when dismissed. For now, fall through.
                    break;
                case TutorialStep.TriggerCondition.TapsReached:
                    // counted by HandleAnyTap, advances when threshold met.
                    break;
                case TutorialStep.TriggerCondition.UpgradePurchased:
                    GameEvents.OnUpgradePurchased += HandleUpgradePurchased;
                    break;
                case TutorialStep.TriggerCondition.StageProgressed:
                    GameEvents.OnAdversaireDefeated += HandleAdversaireDefeated;
                    GameEvents.OnStadeChanged += HandleStadeChanged;
                    break;
                case TutorialStep.TriggerCondition.ElanReachedMax:
                    GameEvents.OnElanFull += HandleElanFull;
                    break;
                case TutorialStep.TriggerCondition.VagueTriggered:
                    GameEvents.OnVagueTriggered += HandleVagueTriggered;
                    break;
            }
        }

        private void UnsubscribeTrigger(TutorialStep step)
        {
            if (step == null) return;
            switch (step.trigger)
            {
                case TutorialStep.TriggerCondition.UpgradePurchased:
                    GameEvents.OnUpgradePurchased -= HandleUpgradePurchased;
                    break;
                case TutorialStep.TriggerCondition.StageProgressed:
                    GameEvents.OnAdversaireDefeated -= HandleAdversaireDefeated;
                    GameEvents.OnStadeChanged -= HandleStadeChanged;
                    break;
                case TutorialStep.TriggerCondition.ElanReachedMax:
                    GameEvents.OnElanFull -= HandleElanFull;
                    break;
                case TutorialStep.TriggerCondition.VagueTriggered:
                    GameEvents.OnVagueTriggered -= HandleVagueTriggered;
                    break;
            }
        }

        private void UnsubscribeAll()
        {
            if (_current != null) UnsubscribeTrigger(_current);
            UnsubscribeGlobal();
        }

        private void HandleAnyTap(BreakInfinity.BigDouble _, float __, Vector2 ___)
        {
            _runtimeTapsAccumulator++;
            if (_current != null
                && _current.trigger == TutorialStep.TriggerCondition.TapsReached
                && _runtimeTapsAccumulator >= _current.triggerThreshold)
            {
                Advance();
            }
        }

        private void HandleUpgradePurchased(string upgradeId, int newLevel) => Advance();
        private void HandleAdversaireDefeated(AdversaireData data, BreakInfinity.BigDouble reward) => Advance();
        private void HandleStadeChanged(int prev, int next) => Advance();
        private void HandleElanFull() => Advance();
        private void HandleVagueTriggered(CombatPhase phase) => Advance();
    }
}
