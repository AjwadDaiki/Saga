using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class CombatProcessorTests
    {
        private GameState _state;
        private ContentDatabase _content;
        private CombatProcessor _combat;
        private AdversaireData _adversaire;

        // Event spies
        private int _phaseChangeCount;
        private CombatPhase _lastPrev;
        private CombatPhase _lastNext;
        private int _defeatedCount;
        private int _deathCount;

        [SetUp]
        public void Setup()
        {
            _adversaire = AdversaireData.CreateForTests("test_dummy", "Test Dummy", Voie.None,
                hp: 100, rewardForce: 30, lootChance: 0f, chronoSeconds: 10f);
            _state = new GameState { currentPhase = CombatPhase.Training };
            _content = new ContentDatabase(System.Array.Empty<UpgradeData>(), new[] { _adversaire });
            _combat = new CombatProcessor(_content);

            _phaseChangeCount = 0;
            _defeatedCount = 0;
            _deathCount = 0;
            GameEvents.OnPhaseChanged += OnPhase;
            GameEvents.OnAdversaireDefeated += OnDefeated;
            GameEvents.OnPlayerDiedTemporary += OnDied;
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.OnPhaseChanged -= OnPhase;
            GameEvents.OnAdversaireDefeated -= OnDefeated;
            GameEvents.OnPlayerDiedTemporary -= OnDied;
        }

        private void OnPhase(CombatPhase prev, CombatPhase next) { _phaseChangeCount++; _lastPrev = prev; _lastNext = next; }
        private void OnDefeated(AdversaireData _, BigDouble __) { _defeatedCount++; }
        private void OnDied() { _deathCount++; }

        [Test]
        public void StartIncoming_Sets_State_And_Raises_PhaseChanged()
        {
            _combat.StartIncoming(_state, _adversaire);

            Assert.AreEqual(CombatPhase.AdversaireIncoming, _state.currentPhase);
            Assert.AreEqual(_adversaire.Id, _state.currentAdversaireId);
            Assert.AreEqual(100.0, _state.currentAdversaireHp.ToDouble(), 1e-9);
            Assert.AreEqual(10f, _state.chronoRemaining);
            Assert.AreEqual(1, _phaseChangeCount);
            Assert.AreEqual(CombatPhase.Training, _lastPrev);
            Assert.AreEqual(CombatPhase.AdversaireIncoming, _lastNext);
        }

        [Test]
        public void Incoming_Cinematic_Transitions_To_Active_After_1s()
        {
            _combat.StartIncoming(_state, _adversaire);
            _combat.Tick(_state, 0.5f);
            Assert.AreEqual(CombatPhase.AdversaireIncoming, _state.currentPhase);
            _combat.Tick(_state, 0.6f);
            Assert.AreEqual(CombatPhase.AdversaireActive, _state.currentPhase);
        }

        [Test]
        public void Active_Phase_Decrements_Chrono()
        {
            _combat.StartIncoming(_state, _adversaire);
            _combat.Tick(_state, 1.0f); // skip incoming
            var before = _state.chronoRemaining;
            _combat.Tick(_state, 1.0f);
            Assert.Less(_state.chronoRemaining, before);
        }

        [Test]
        public void Zero_Hp_Triggers_Victory_Phase_And_Reward_Applied()
        {
            _combat.StartIncoming(_state, _adversaire);
            _combat.Tick(_state, 1.0f); // → Active

            _state.currentAdversaireHp = new BigDouble(0);
            _combat.Tick(_state, 0.1f);

            Assert.AreEqual(CombatPhase.AdversaireVictory, _state.currentPhase);
            Assert.AreEqual(1, _defeatedCount);
            Assert.AreEqual(30.0, _state.force.ToDouble(), 1e-9);
            Assert.AreEqual(1, _state.totalAdversairesDefeated);
        }

        [Test]
        public void Chrono_Expiry_Triggers_PlayerDeathTemporary()
        {
            _combat.StartIncoming(_state, _adversaire);
            _combat.Tick(_state, 1.0f); // → Active

            // Force chrono near 0 and tick past
            _state.chronoRemaining = 0.05f;
            _combat.Tick(_state, 0.1f);

            Assert.AreEqual(CombatPhase.PlayerDeathTemporary, _state.currentPhase);
            Assert.AreEqual(1, _deathCount);
            Assert.AreEqual(0, _defeatedCount);
        }

        [Test]
        public void Victory_Phase_Returns_To_Training_After_2s()
        {
            _combat.StartIncoming(_state, _adversaire);
            _combat.Tick(_state, 1.0f); // → Active
            _state.currentAdversaireHp = new BigDouble(0);
            _combat.Tick(_state, 0.1f); // → Victory

            _combat.Tick(_state, 1.0f);
            Assert.AreEqual(CombatPhase.AdversaireVictory, _state.currentPhase);

            _combat.Tick(_state, 1.5f);
            Assert.AreEqual(CombatPhase.Training, _state.currentPhase);
            Assert.IsNull(_state.currentAdversaireId);
        }

        [Test]
        public void ResolveDeathTemporary_Applies_10pct_Force_Penalty()
        {
            _state.force = new BigDouble(1000);
            _state.currentPhase = CombatPhase.PlayerDeathTemporary;

            _combat.ResolveDeathTemporary(_state);

            Assert.AreEqual(900.0, _state.force.ToDouble(), 1e-6);
            Assert.AreEqual(CombatPhase.Training, _state.currentPhase);
            Assert.IsNull(_state.currentAdversaireId);
            Assert.AreEqual(0f, _state.chronoRemaining);
        }

        [Test]
        public void Tick_In_PlayerDeathTemporary_Is_No_Op_Until_Resolve()
        {
            _state.currentPhase = CombatPhase.PlayerDeathTemporary;
            _state.force = new BigDouble(500);
            // Spam ticks — should NOT auto-resume.
            for (var i = 0; i < 100; i++) _combat.Tick(_state, 1.0f);
            Assert.AreEqual(CombatPhase.PlayerDeathTemporary, _state.currentPhase);
            Assert.AreEqual(500.0, _state.force.ToDouble(), 1e-9, "Force shouldn't change in PlayerDeathTemporary tick");
        }
    }
}
