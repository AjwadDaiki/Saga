using NUnit.Framework;
using Saga.Data;
using UnityEngine;

namespace Saga.Tests
{
    [TestFixture]
    public class CapitaineDataTests
    {
        [Test]
        public void Properties_Round_Trip_From_Factory()
        {
            var c = CapitaineData.CreateForTests("hattori_du_mont", "Hattori du Mont", Voie.Samurai,
                hp: 500, rewardForce: 250, chronoSeconds: 60f,
                introCitation: "Le sommet n'attend personne.",
                deathCitation: "Le sommet… reconnaît.");

            Assert.AreEqual("hattori_du_mont", c.Id);
            Assert.AreEqual("Hattori du Mont", c.DisplayName);
            Assert.AreEqual(Voie.Samurai, c.Voie);
            Assert.AreEqual(500.0, c.Hp.ToDouble(), 1e-9);
            Assert.AreEqual(250.0, c.RewardForce.ToDouble(), 1e-9);
            Assert.AreEqual(60f, c.ChronoSeconds);
            Assert.AreEqual("Le sommet n'attend personne.", c.IntroCitation);
            Assert.IsTrue(c.GuaranteedLoot);
        }

        [Test]
        public void Phase_Thresholds_Default_To_75_50_25()
        {
            var c = CapitaineData.CreateForTests("x", "X", Voie.None, 100, 50, 30);
            Assert.AreEqual(3, c.PhaseThresholds.Length);
            Assert.AreEqual(0.75f, c.PhaseThresholds[0]);
            Assert.AreEqual(0.5f, c.PhaseThresholds[1]);
            Assert.AreEqual(0.25f, c.PhaseThresholds[2]);
        }

        [Test]
        public void ComputePhase_Maps_Hp_Ratio_To_Phase_Index()
        {
            var c = CapitaineData.CreateForTests("x", "X", Voie.None, 100, 50, 30);
            Assert.AreEqual(0, c.ComputePhase(1.0));      // full HP -> phase 0
            Assert.AreEqual(0, c.ComputePhase(0.76));     // above 75% -> phase 0
            Assert.AreEqual(1, c.ComputePhase(0.74));     // below 75% -> phase 1
            Assert.AreEqual(2, c.ComputePhase(0.49));     // below 50% -> phase 2
            Assert.AreEqual(3, c.ComputePhase(0.24));     // below 25% -> phase 3 (enrage)
            Assert.AreEqual(3, c.ComputePhase(0.0));      // dead -> phase 3
        }
    }
}
