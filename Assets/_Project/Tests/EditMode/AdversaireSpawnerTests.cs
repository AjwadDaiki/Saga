using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class AdversaireSpawnerTests
    {
        private GameState _state;
        private ContentDatabase _content;
        private CombatProcessor _combat;
        private AdversaireSpawner _spawner;
        private int _spawnEventCount;
        private AdversaireData _lastSpawnedFromEvent;

        [SetUp]
        public void Setup()
        {
            var pool = new[]
            {
                AdversaireData.CreateForTests("a1", "A1", Voie.None,  100, 30, 0f, 30f),
                AdversaireData.CreateForTests("a2", "A2", Voie.Viking, 80, 25, 0f, 25f),
            };
            _state = new GameState { currentStade = 1, currentPhase = CombatPhase.Training };
            _content = new ContentDatabase(System.Array.Empty<UpgradeData>(), pool);
            _combat = new CombatProcessor(_content);
            _spawner = new AdversaireSpawner(_content, _combat);

            _spawnEventCount = 0;
            _lastSpawnedFromEvent = null;
            GameEvents.OnAdversaireSpawned += OnSpawn;
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.OnAdversaireSpawned -= OnSpawn;
        }

        private void OnSpawn(AdversaireData data)
        {
            _spawnEventCount++;
            _lastSpawnedFromEvent = data;
        }

        [Test]
        public void Threshold_Doubles_Per_Stade_From_50()
        {
            Assert.AreEqual(50,   AdversaireSpawner.GetThresholdForStade(1));
            Assert.AreEqual(100,  AdversaireSpawner.GetThresholdForStade(2));
            Assert.AreEqual(200,  AdversaireSpawner.GetThresholdForStade(3));
            Assert.AreEqual(400,  AdversaireSpawner.GetThresholdForStade(4));
            Assert.AreEqual(800,  AdversaireSpawner.GetThresholdForStade(5));
            Assert.AreEqual(1600, AdversaireSpawner.GetThresholdForStade(6));
        }

        [Test]
        public void Below_Threshold_Does_Not_Spawn()
        {
            for (var i = 0; i < 49; i++) _spawner.OnTap(_state);
            Assert.AreEqual(0, _spawnEventCount);
            Assert.AreEqual(49, _state.tapsTowardsNextAdversaire);
            Assert.AreEqual(CombatPhase.Training, _state.currentPhase);
        }

        [Test]
        public void Hitting_Threshold_Spawns_And_Resets_Counter()
        {
            for (var i = 0; i < 50; i++) _spawner.OnTap(_state);
            Assert.AreEqual(1, _spawnEventCount);
            Assert.AreEqual(0, _state.tapsTowardsNextAdversaire);
            Assert.AreEqual(CombatPhase.AdversaireIncoming, _state.currentPhase);
            Assert.IsNotNull(_lastSpawnedFromEvent);
        }

        [Test]
        public void Out_Of_Training_No_Spawn_Even_If_Threshold_Crossed()
        {
            _state.currentPhase = CombatPhase.AdversaireActive;
            for (var i = 0; i < 100; i++) _spawner.OnTap(_state);
            Assert.AreEqual(0, _spawnEventCount);
            // Counter shouldn't have been bumped either.
            Assert.AreEqual(0, _state.tapsTowardsNextAdversaire);
        }

        [Test]
        public void Empty_Pool_Resets_Counter_And_Skips_Spawn()
        {
            var emptyContent = new ContentDatabase(System.Array.Empty<UpgradeData>(), System.Array.Empty<AdversaireData>());
            var emptySpawner = new AdversaireSpawner(emptyContent, new CombatProcessor(emptyContent));
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("No adversaires loaded"));
            for (var i = 0; i < 50; i++) emptySpawner.OnTap(_state);
            Assert.AreEqual(0, _state.tapsTowardsNextAdversaire);
            Assert.AreEqual(CombatPhase.Training, _state.currentPhase);
        }
    }
}
