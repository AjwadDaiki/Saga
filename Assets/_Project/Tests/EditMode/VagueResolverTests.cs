using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class VagueResolverTests
    {
        private GameState _state;
        private ContentDatabase _content;
        private VagueResolver _resolver;
        private int _buffActiveEventCount;
        private bool _lastBuffActive;
        private int _resolvedEventCount;

        [SetUp]
        public void Setup()
        {
            _state = new GameState { currentPhase = CombatPhase.Training, currentStade = 1 };
            _content = new ContentDatabase(System.Array.Empty<UpgradeData>(),
                System.Array.Empty<AdversaireData>(),
                System.Array.Empty<CapitaineData>());
            _resolver = new VagueResolver(_content);

            _buffActiveEventCount = 0;
            _lastBuffActive = false;
            _resolvedEventCount = 0;

            GameEvents.OnVagueBuffActive += OnBuffActive;
            GameEvents.OnVagueResolved += OnResolved;
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.OnVagueBuffActive -= OnBuffActive;
            GameEvents.OnVagueResolved -= OnResolved;
        }

        private void OnBuffActive(bool active) { _buffActiveEventCount++; _lastBuffActive = active; }
        private void OnResolved(BigDouble _) { _resolvedEventCount++; }

        [Test]
        public void TriggerVague_In_Training_Activates_Buff()
        {
            _resolver.TriggerVague(_state);
            Assert.IsTrue(_resolver.IsTrainingBuffActive);
            Assert.IsTrue(_lastBuffActive);
            Assert.AreEqual(1, _buffActiveEventCount);
            Assert.AreEqual(1, _resolvedEventCount);
        }

        [Test]
        public void Buff_Expires_After_Duration()
        {
            _resolver.TriggerVague(_state);
            // Tick past the duration in small steps.
            for (var i = 0; i < 60; i++) _resolver.Tick(_state, 0.1f); // 6s total > 5s duration
            Assert.IsFalse(_resolver.IsTrainingBuffActive);
            Assert.IsFalse(_lastBuffActive);
            // Expect 2 buff-state events: true on trigger, false on expiry.
            Assert.AreEqual(2, _buffActiveEventCount);
        }

        [Test]
        public void TriggerVague_In_Combat_Deals_Damage_To_Adversaire()
        {
            var adv = AdversaireData.CreateForTests("a1", "A1", Voie.None, 100000, 0, 0f, 30f);
            var content = new ContentDatabase(System.Array.Empty<UpgradeData>(), new[] { adv });
            var resolver = new VagueResolver(content);

            _state.currentPhase = CombatPhase.AdversaireActive;
            _state.currentAdversaireId = "a1";
            _state.currentAdversaireHp = new BigDouble(100000);
            _state.currentStade = 1;

            resolver.TriggerVague(_state);

            // Base 3000 × stade 1 = 3000 damage.
            Assert.AreEqual(100000.0 - ElanConstants.VagueCombatDamageBase, _state.currentAdversaireHp.ToDouble(), 1e-5);
            Assert.IsFalse(resolver.IsTrainingBuffActive, "Combat Vague should not activate the Training buff");
        }

        [Test]
        public void Combat_Damage_Scales_With_Stade()
        {
            var adv = AdversaireData.CreateForTests("a1", "A1", Voie.None, 100000, 0, 0f, 30f);
            var content = new ContentDatabase(System.Array.Empty<UpgradeData>(), new[] { adv });
            var resolver = new VagueResolver(content);

            _state.currentPhase = CombatPhase.AdversaireActive;
            _state.currentAdversaireId = "a1";
            _state.currentAdversaireHp = new BigDouble(100000);
            _state.currentStade = 3;

            resolver.TriggerVague(_state);

            // 3000 × stade 3 = 9000 damage.
            Assert.AreEqual(100000.0 - 3 * ElanConstants.VagueCombatDamageBase, _state.currentAdversaireHp.ToDouble(), 1e-5);
        }
    }
}
