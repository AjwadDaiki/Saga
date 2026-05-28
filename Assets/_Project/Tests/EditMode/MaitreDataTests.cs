using NUnit.Framework;
using Saga.Data;
using UnityEngine;

namespace Saga.Tests
{
    [TestFixture]
    public class MaitreDataTests
    {
        [Test]
        public void Properties_Round_Trip_From_Factory()
        {
            var m = MaitreData.CreateForTests("yoshitsune", "Yoshitsune", Voie.Samurai,
                hp: 8000, rewardForce: 4000, chronoSeconds: 180f,
                introCitation: "L'art du sabre dépasse la chair.",
                victoryCitation: "Mon élève...",
                defeatCitation: "Le saut du tigre demande de la grâce.",
                reliqueName: "Tantō de Yoshitsune",
                reliqueBonus: 1500);

            Assert.AreEqual("yoshitsune", m.Id);
            Assert.AreEqual("Yoshitsune", m.DisplayName);
            Assert.AreEqual(Voie.Samurai, m.Voie);
            Assert.AreEqual(8000.0, m.Hp.ToDouble(), 1e-9);
            Assert.AreEqual(4000.0, m.RewardForce.ToDouble(), 1e-9);
            Assert.AreEqual(180f, m.ChronoSeconds);
            Assert.AreEqual("L'art du sabre dépasse la chair.", m.IntroCitation);
            Assert.AreEqual("Mon élève...", m.VictoryCitation);
            Assert.AreEqual("Le saut du tigre demande de la grâce.", m.DefeatCitation);
            Assert.AreEqual("Tantō de Yoshitsune", m.ReliqueUniqueName);
            Assert.AreEqual(1500.0, m.ReliqueStatsBonus.ToDouble(), 1e-9);
        }

        [Test]
        public void Phase_Thresholds_Default_To_75_50_25()
        {
            var m = MaitreData.CreateForTests("x", "X", Voie.None, 10000, 5000, 180f);
            Assert.AreEqual(3, m.PhaseThresholds.Length);
            Assert.AreEqual(0.75f, m.PhaseThresholds[0]);
            Assert.AreEqual(0.5f, m.PhaseThresholds[1]);
            Assert.AreEqual(0.25f, m.PhaseThresholds[2]);
        }

        [Test]
        public void ComputePhase_Maps_Hp_Ratio_To_Phase_Index()
        {
            var m = MaitreData.CreateForTests("x", "X", Voie.None, 10000, 5000, 180f);
            Assert.AreEqual(0, m.ComputePhase(1.0));      // full HP -> phase 0
            Assert.AreEqual(0, m.ComputePhase(0.76));     // above 75% -> phase 0
            Assert.AreEqual(1, m.ComputePhase(0.74));     // below 75% -> phase 1
            Assert.AreEqual(2, m.ComputePhase(0.49));     // below 50% -> phase 2
            Assert.AreEqual(3, m.ComputePhase(0.24));     // below 25% -> phase 3 (enrage)
            Assert.AreEqual(3, m.ComputePhase(0.0));      // dead -> phase 3
        }

        [Test]
        public void Phase_Colors_Default_Array_Has_Four_Slots()
        {
            var m = MaitreData.CreateForTests("x", "X", Voie.None, 10000, 5000, 180f);
            Assert.IsNotNull(m.PhaseColors);
            Assert.AreEqual(4, m.PhaseColors.Length);
        }
    }
}
