using BreakInfinity;
using NUnit.Framework;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class PrestigeServiceTests
    {
        private GameState _state;
        private PrestigeService _service;
        private MaitreData _yoshitsune;

        [SetUp]
        public void Setup()
        {
            _state = new GameState();
            _service = new PrestigeService();
            _yoshitsune = MaitreData.CreateForTests("yoshitsune", "Yoshitsune", Voie.Samurai,
                hp: 8000, rewardForce: 4000, chronoSeconds: 180f);
        }

        [Test]
        public void CalculateEchos_Returns_Min_Reward_When_Force_Is_Zero()
        {
            _state.currentRunForceMax = new BigDouble(0);
            var echos = _service.CalculateEchosForCurrentRun(_state);
            Assert.AreEqual(PrestigeConstants.EchosMinReward, echos.ToDouble(), 1e-9);
        }

        [Test]
        public void CalculateEchos_Applies_Log10_Formula()
        {
            // log10(1e6) = 6, × 10 = 60 → 60 Échos.
            _state.currentRunForceMax = new BigDouble(1_000_000);
            var echos = _service.CalculateEchosForCurrentRun(_state);
            Assert.AreEqual(60.0, echos.ToDouble(), 1e-9);
        }

        [Test]
        public void CalculateEchos_Honors_Min_Reward_Floor()
        {
            // log10(10) = 1, × 10 = 10 → exactly min reward (10).
            _state.currentRunForceMax = new BigDouble(10);
            var echos = _service.CalculateEchosForCurrentRun(_state);
            Assert.GreaterOrEqual(echos.ToDouble(), PrestigeConstants.EchosMinReward);
        }

        [Test]
        public void TriggerPrestige_Appends_Death_Record()
        {
            _state.currentRunForceMax = new BigDouble(1_000);
            _state.playerCitation = "Je tombe pour mieux renaître.";
            Assert.AreEqual(0, _state.deathRecords.Count);

            _service.TriggerPrestige(_state, _yoshitsune);

            Assert.AreEqual(1, _state.deathRecords.Count);
            var rec = _state.deathRecords[0];
            Assert.AreEqual("Yoshitsune", rec.maitreName);
            Assert.AreEqual("Samurai", rec.maitreVoie);
            Assert.AreEqual("Je tombe pour mieux renaître.", rec.playerCitation);
            Assert.AreEqual(1, rec.prestigeNumber);
        }

        [Test]
        public void TriggerPrestige_Unlocks_Vaincu_Par_Title_Once()
        {
            _state.currentRunForceMax = new BigDouble(1_000);

            _service.TriggerPrestige(_state, _yoshitsune);
            Assert.IsTrue(_state.titlesUnlocked.Contains("Vaincu par Yoshitsune"));
            var firstCount = _state.titlesUnlocked.Count;

            // Second prestige to the same Maître should NOT re-add the title.
            _service.TriggerPrestige(_state, _yoshitsune);
            Assert.AreEqual(firstCount, _state.titlesUnlocked.Count, "Title should be deduplicated.");
        }

        [Test]
        public void TriggerPrestige_Increments_Prestige_Count_And_Banks_Echos()
        {
            _state.currentRunForceMax = new BigDouble(1_000_000); // -> 60 échos
            _state.totalEchos = new BigDouble(100);
            Assert.AreEqual(0, _state.prestigeCount);

            _service.TriggerPrestige(_state, _yoshitsune);

            Assert.AreEqual(1, _state.prestigeCount);
            Assert.AreEqual(160.0, _state.totalEchos.ToDouble(), 1e-9);
        }

        [Test]
        public void CompletePrestige_Resets_Run_Fields()
        {
            _state.force = new BigDouble(99999);
            _state.upgradeLevels["frappe"] = 5;
            _state.currentStade = 7;
            _state.totalAdversairesDefeated = 42;
            _state.totalCapitainesDefeated = 9;
            _state.maitreInvocationSlots = 3;
            _state.currentRunEchosEarned = new BigDouble(500);
            _state.currentRunForceMax = new BigDouble(1_000_000);
            _state.currentElan = 75f;
            _state.playerCitationLockedForRun = true;
            _state.currentAdversaireId = "leftover";
            _state.currentCapitaineId = "leftover";
            _state.currentMaitreId = "leftover";
            _state.currentCapitainePhase = 2;
            _state.currentMaitrePhase = 2;

            _service.CompletePrestige(_state);

            Assert.AreEqual(0.0, _state.force.ToDouble(), 1e-9);
            Assert.AreEqual(0, _state.upgradeLevels.Count);
            Assert.AreEqual(1, _state.currentStade);
            Assert.AreEqual(0, _state.totalAdversairesDefeated);
            Assert.AreEqual(0, _state.totalCapitainesDefeated);
            Assert.AreEqual(0, _state.maitreInvocationSlots);
            Assert.AreEqual(0.0, _state.currentRunEchosEarned.ToDouble(), 1e-9);
            Assert.AreEqual(0.0, _state.currentRunForceMax.ToDouble(), 1e-9);
            Assert.AreEqual(0f, _state.currentElan);
            Assert.IsFalse(_state.playerCitationLockedForRun);
            Assert.IsNull(_state.currentAdversaireId);
            Assert.IsNull(_state.currentCapitaineId);
            Assert.IsNull(_state.currentMaitreId);
            Assert.AreEqual(0, _state.currentCapitainePhase);
            Assert.AreEqual(0, _state.currentMaitrePhase);
            Assert.AreEqual(CombatPhase.Training, _state.currentPhase);
        }

        [Test]
        public void CompletePrestige_Preserves_Persist_Fields()
        {
            // First trigger to populate persist fields with non-default values.
            _state.currentRunForceMax = new BigDouble(1_000_000);
            _state.playerCitation = "Mon héritage demeure.";
            _state.relicsOwned.Add("relic_yoshitsune");
            _state.titlesUnlocked.Add("Premier Sang");
            _state.achievementsUnlocked.Add("ach_first_capitaine");

            _service.TriggerPrestige(_state, _yoshitsune);
            var preservedTotalEchos = _state.totalEchos;
            var preservedPrestigeCount = _state.prestigeCount;
            var preservedDeathRecords = _state.deathRecords.Count;
            var preservedRelics = _state.relicsOwned.Count;
            var preservedTitles = _state.titlesUnlocked.Count;
            var preservedAch = _state.achievementsUnlocked.Count;
            var preservedCitation = _state.playerCitation;

            _service.CompletePrestige(_state);

            Assert.AreEqual(preservedTotalEchos.ToDouble(), _state.totalEchos.ToDouble(), 1e-9);
            Assert.AreEqual(preservedPrestigeCount, _state.prestigeCount);
            Assert.AreEqual(preservedDeathRecords, _state.deathRecords.Count);
            Assert.AreEqual(preservedRelics, _state.relicsOwned.Count);
            Assert.AreEqual(preservedTitles, _state.titlesUnlocked.Count);
            Assert.AreEqual(preservedAch, _state.achievementsUnlocked.Count);
            Assert.AreEqual(preservedCitation, _state.playerCitation);
        }
    }
}
