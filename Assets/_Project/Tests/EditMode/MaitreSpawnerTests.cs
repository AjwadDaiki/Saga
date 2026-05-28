using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class MaitreSpawnerTests
    {
        private GameState _state;
        private ContentDatabase _content;
        private MaitreSpawner _spawner;
        private MaitreData _yoshitsune;
        private MaitreData _ragnar;

        [SetUp]
        public void Setup()
        {
            _yoshitsune = MaitreData.CreateForTests("yoshitsune", "Yoshitsune", Voie.Samurai, 8000, 4000, 180f);
            _ragnar = MaitreData.CreateForTests("ragnar", "Ragnar Lodbrok", Voie.Viking, 9000, 4500, 180f);
            _content = new ContentDatabase(
                System.Array.Empty<UpgradeData>(),
                System.Array.Empty<AdversaireData>(),
                System.Array.Empty<CapitaineData>(),
                new[] { _yoshitsune, _ragnar });
            _state = new GameState();
            _spawner = new MaitreSpawner(_content);
        }

        [Test]
        public void Capitaines_Per_Slot_Constant_Is_Three()
        {
            Assert.AreEqual(3, PrestigeConstants.CapitainesPerMaitreSlot);
        }

        [Test]
        public void Content_Database_Exposes_Maitre_Pool()
        {
            Assert.AreEqual(2, _content.AllMaitres.Count);
            Assert.IsNotNull(_content.GetMaitre("yoshitsune"));
            Assert.IsNotNull(_content.GetMaitre("ragnar"));
            Assert.IsNull(_content.GetMaitre("nonexistent"));
        }

        [Test]
        public void TryInvokeMaitre_Fails_When_No_Slots()
        {
            _state.maitreInvocationSlots = 0;
            _state.currentPhase = CombatPhase.Training;
            var ok = _spawner.TryInvokeMaitre(_state, "yoshitsune");
            Assert.IsFalse(ok);
            Assert.AreEqual(0, _state.maitreInvocationSlots);
        }

        [Test]
        public void TryInvokeMaitre_Fails_When_Not_In_Training_Phase()
        {
            _state.maitreInvocationSlots = 2;
            _state.currentPhase = CombatPhase.AdversaireActive;
            var ok = _spawner.TryInvokeMaitre(_state, "yoshitsune");
            Assert.IsFalse(ok);
            Assert.AreEqual(2, _state.maitreInvocationSlots, "Slot should not be consumed on failure.");
        }

        [Test]
        public void TryInvokeMaitre_Fails_On_Unknown_Id()
        {
            _state.maitreInvocationSlots = 1;
            _state.currentPhase = CombatPhase.Training;
            var ok = _spawner.TryInvokeMaitre(_state, "unknown_maitre");
            Assert.IsFalse(ok);
            Assert.AreEqual(1, _state.maitreInvocationSlots);
        }

        [Test]
        public void TryInvokeMaitre_Decrements_Slot_On_Success()
        {
            _state.maitreInvocationSlots = 2;
            _state.currentPhase = CombatPhase.Training;
            // GameManager.Instance is null in tests; spawner's null-safe ?. chain handles the
            // Combat.StartMaitreIncoming call gracefully. We assert on the observable state change.
            var ok = _spawner.TryInvokeMaitre(_state, "yoshitsune");
            Assert.IsTrue(ok);
            Assert.AreEqual(1, _state.maitreInvocationSlots);
        }
    }
}
