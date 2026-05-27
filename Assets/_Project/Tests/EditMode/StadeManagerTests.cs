using BreakInfinity;
using NUnit.Framework;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class StadeManagerTests
    {
        [Test]
        public void Zero_Force_Maps_To_Mendiant()
        {
            Assert.AreEqual((int)Stade.Mendiant, StadeManager.ComputeStade(new BigDouble(0)));
        }

        [Test]
        public void Below_Apprenti_Threshold_Stays_Mendiant()
        {
            Assert.AreEqual((int)Stade.Mendiant, StadeManager.ComputeStade(new BigDouble(999)));
        }

        [Test]
        public void At_Apprenti_Threshold_Promotes()
        {
            Assert.AreEqual((int)Stade.Apprenti, StadeManager.ComputeStade(new BigDouble(1_000)));
        }

        [Test]
        public void Mid_Apprenti_Range()
        {
            Assert.AreEqual((int)Stade.Apprenti, StadeManager.ComputeStade(new BigDouble(50_000)));
        }

        [Test]
        public void At_Guerrier_Threshold_Promotes()
        {
            Assert.AreEqual((int)Stade.Guerrier, StadeManager.ComputeStade(new BigDouble(100_000)));
        }

        [Test]
        public void At_Maitre_Threshold_Promotes()
        {
            Assert.AreEqual((int)Stade.Maitre, StadeManager.ComputeStade(new BigDouble(10_000_000)));
        }

        [Test]
        public void At_Legende_Threshold_Promotes()
        {
            Assert.AreEqual((int)Stade.Legende, StadeManager.ComputeStade(new BigDouble(1_000_000_000)));
        }

        [Test]
        public void At_Mythe_Threshold_Promotes()
        {
            // 1e15 = 1aa in our notation = Mythe seuil (per 04_PROGRESSION.md).
            Assert.AreEqual((int)Stade.Mythe, StadeManager.ComputeStade(BigDouble.Pow(10, 15)));
        }

        [Test]
        public void Beyond_Mythe_Stays_Mythe()
        {
            // No higher tier — Mythe is the cap (no Divin per design decision).
            Assert.AreEqual((int)Stade.Mythe, StadeManager.ComputeStade(BigDouble.Pow(10, 30)));
        }

        [Test]
        public void Tick_Raises_Event_Only_On_Transition()
        {
            var manager = new StadeManager();
            var state = new GameState { force = new BigDouble(0), currentStade = (int)Stade.Mendiant };

            // No-op tick: same stade, no transition.
            var events = 0;
            void OnChange(int prev, int next) => events++;
            Saga.Core.GameEvents.OnStadeChanged += OnChange;

            try
            {
                manager.Tick(state, 0.1f);
                Assert.AreEqual(0, events, "No event expected when stade doesn't change");

                // Cross threshold.
                state.force = new BigDouble(1_500);
                manager.Tick(state, 0.1f);
                Assert.AreEqual(1, events, "Exactly one event expected on transition");
                Assert.AreEqual((int)Stade.Apprenti, state.currentStade);

                // Tick again — same stade, no further events.
                manager.Tick(state, 0.1f);
                Assert.AreEqual(1, events);
            }
            finally
            {
                Saga.Core.GameEvents.OnStadeChanged -= OnChange;
            }
        }
    }
}
