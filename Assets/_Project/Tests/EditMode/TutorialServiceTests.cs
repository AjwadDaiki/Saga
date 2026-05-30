using System.Collections.Generic;
using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Save;
using Saga.Tutorial;
using UnityEngine;

namespace Saga.Tests
{
    /// <summary>
    /// Sprint 8 Phase A — TutorialService POCO + SaveService v10 migration coverage.
    /// </summary>
    [TestFixture]
    public class TutorialServiceTests
    {
        private static TutorialStep MakeStep(string id,
            TutorialStep.TriggerCondition trigger,
            int threshold = 0,
            TutorialStep.AnchorTarget anchor = TutorialStep.AnchorTarget.CombatZone)
        {
            var step = ScriptableObject.CreateInstance<TutorialStep>();
            step.name = $"Step_{id}";
            step.id = id;
            step.trigger = trigger;
            step.triggerThreshold = threshold;
            step.anchor = anchor;
            step.messageFr = $"message-{id}";
            return step;
        }

        [Test]
        public void FirstBoot_StartsAtStep0_AndShowsCurrent()
        {
            var state = new GameState { tutorialDone = false, tutorialStepIndex = 0 };
            var steps = new List<TutorialStep>
            {
                MakeStep("a", TutorialStep.TriggerCondition.TapsReached, threshold: 3),
                MakeStep("b", TutorialStep.TriggerCondition.TapsReached, threshold: 3),
            };
            var svc = new TutorialService(state, save: null, testSteps: steps);

            TutorialStep shown = null;
            void Listener(TutorialStep s) => shown = s;
            GameEvents.OnTutorialStepShown += Listener;
            try
            {
                svc.Start();
                Assert.IsTrue(svc.IsActive, "Service should be active after Start on fresh state.");
                Assert.AreEqual(0, state.tutorialStepIndex, "Step index should start at 0.");
                Assert.AreEqual(steps[0], svc.CurrentStep, "CurrentStep should be the first step.");
                Assert.AreEqual(steps[0], shown, "OnTutorialStepShown should have fired for step 0.");
            }
            finally
            {
                GameEvents.OnTutorialStepShown -= Listener;
                foreach (var s in steps) Object.DestroyImmediate(s);
            }
        }

        [Test]
        public void Skip_MarksTutorialDone_AndRaisesFinished()
        {
            var state = new GameState { tutorialDone = false, tutorialStepIndex = 0 };
            var steps = new List<TutorialStep>
            {
                MakeStep("a", TutorialStep.TriggerCondition.TapsReached, threshold: 3),
            };
            var svc = new TutorialService(state, save: null, testSteps: steps);

            var finished = false;
            void Listener() => finished = true;
            GameEvents.OnTutorialFinished += Listener;
            try
            {
                svc.Start();
                svc.Skip();
                Assert.IsTrue(state.tutorialDone, "tutorialDone should be true after Skip.");
                Assert.IsFalse(svc.IsActive, "Service should no longer be active.");
                Assert.IsTrue(finished, "OnTutorialFinished should have fired.");
            }
            finally
            {
                GameEvents.OnTutorialFinished -= Listener;
                foreach (var s in steps) Object.DestroyImmediate(s);
            }
        }

        [Test]
        public void TapsReachedTrigger_AdvancesAfterThresholdTaps()
        {
            var state = new GameState { tutorialDone = false, tutorialStepIndex = 0 };
            var steps = new List<TutorialStep>
            {
                MakeStep("a", TutorialStep.TriggerCondition.TapsReached, threshold: 3),
                MakeStep("b", TutorialStep.TriggerCondition.TapsReached, threshold: 100),
            };
            var svc = new TutorialService(state, save: null, testSteps: steps);

            string completedId = null;
            void Listener(string id) => completedId = id;
            GameEvents.OnTutorialStepCompleted += Listener;
            try
            {
                svc.Start();
                // Fire 3 taps → first step's threshold reached → advance to step 2.
                for (var i = 0; i < 3; i++)
                {
                    GameEvents.RaiseTapResolved(new BigDouble(1), 1f, Vector2.zero);
                }
                Assert.AreEqual("a", completedId, "Step 'a' should have completed after 3 taps.");
                Assert.AreEqual(1, state.tutorialStepIndex, "Index should advance to 1.");
                Assert.AreEqual(steps[1], svc.CurrentStep, "Current step should be 'b' now.");
                Assert.IsFalse(state.tutorialDone, "tutorialDone should still be false (one step remaining).");
            }
            finally
            {
                GameEvents.OnTutorialStepCompleted -= Listener;
                svc.Skip(); // cleanup subscriptions
                foreach (var s in steps) Object.DestroyImmediate(s);
            }
        }

        [Test]
        public void SaveService_MigrationV9toV10_SetsTutorialDoneTrueForLegacy()
        {
            // Existing v9 save (legacy player) — tutorialDone defaults to false in GameState constructor
            // but with saveVersion=9, migration should force it to true (skip tutorial for veterans).
            var legacy = new GameState
            {
                saveVersion = 9,
                tutorialDone = false,
                tutorialStepIndex = 0,
            };
            var migrated = SaveService.Migrate(legacy);
            Assert.AreEqual(10, migrated.saveVersion, "saveVersion should be bumped to current v10.");
            Assert.IsTrue(migrated.tutorialDone, "Legacy v9 save should auto-skip tutorial (tutorialDone=true).");

            // Fresh save (saveVersion = current default = 10 via constructor) — migration is idempotent
            // and should NOT override tutorialDone if it's already at the right version.
            var fresh = new GameState(); // saveVersion = 10 default
            Assert.IsFalse(fresh.tutorialDone, "Fresh save default should have tutorialDone=false.");
            var freshMigrated = SaveService.Migrate(fresh);
            Assert.IsFalse(freshMigrated.tutorialDone, "Fresh save v10 should NOT be force-tutorial-done.");
        }
    }
}
