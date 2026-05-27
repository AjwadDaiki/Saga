using NUnit.Framework;
using Saga.Data;

namespace Saga.Tests
{
    [TestFixture]
    public class AdversaireDataTests
    {
        [Test]
        public void Properties_Round_Trip_From_Factory()
        {
            var a = AdversaireData.CreateForTests("ronin_errant", "Ronin Errant", Voie.None,
                hp: 100, rewardForce: 30, lootChance: 0.15f, chronoSeconds: 30f);

            Assert.AreEqual("ronin_errant", a.Id);
            Assert.AreEqual("Ronin Errant", a.DisplayName);
            Assert.AreEqual(Voie.None, a.Voie);
            Assert.AreEqual(100.0, a.Hp.ToDouble(), 1e-9);
            Assert.AreEqual(30.0, a.RewardForce.ToDouble(), 1e-9);
            Assert.AreEqual(0.15f, a.LootChance);
            Assert.AreEqual(30f, a.ChronoSeconds);
        }

        [Test]
        public void Voie_Enum_All_Values_Are_Distinct()
        {
            // Sanity: catches accidental enum value duplication after future edits.
            var values = System.Enum.GetValues(typeof(Voie));
            var seen = new System.Collections.Generic.HashSet<int>();
            foreach (int v in values)
            {
                Assert.IsTrue(seen.Add(v), $"Duplicate Voie value: {v}");
            }
            Assert.AreEqual(9, values.Length, "Expected 9 voies (None + 8 cultures).");
        }

        [Test]
        public void Different_Voie_And_Hp_Profile_Across_Mvp_Set()
        {
            // Smoke-check the 5 Sprint 4 adversaires can be constructed independently.
            var ronin    = AdversaireData.CreateForTests("ronin_errant",       "Ronin",     Voie.None,      100, 30,  0.15f, 30f);
            var viking   = AdversaireData.CreateForTests("spadassin_nordique", "Spadassin", Voie.Viking,    150, 50,  0.20f, 35f);
            var wuxia    = AdversaireData.CreateForTests("initie_wuxia",       "Initié",    Voie.Wuxia,     200, 75,  0.25f, 40f);
            var hoplite  = AdversaireData.CreateForTests("hoplite_lache",      "Hoplite",   Voie.Spartiate, 250, 100, 0.30f, 45f);
            var pelerin  = AdversaireData.CreateForTests("pelerin_du_nord",    "Pèlerin",   Voie.None,      80,  40,  0.20f, 25f);

            Assert.AreEqual(Voie.None, ronin.Voie);
            Assert.AreEqual(Voie.Viking, viking.Voie);
            Assert.AreEqual(Voie.Wuxia, wuxia.Voie);
            Assert.AreEqual(Voie.Spartiate, hoplite.Voie);
            // Pèlerin has the lowest HP — verify the curve isn't accidentally monotonic.
            Assert.Less(pelerin.Hp.ToDouble(), ronin.Hp.ToDouble());
        }
    }
}
