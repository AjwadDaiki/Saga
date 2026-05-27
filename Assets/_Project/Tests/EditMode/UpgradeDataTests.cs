using BreakInfinity;
using NUnit.Framework;
using Saga.Data;

namespace Saga.Tests
{
    [TestFixture]
    public class UpgradeDataTests
    {
        [Test]
        public void Level_Zero_Cost_Equals_Base()
        {
            var u = UpgradeData.CreateForTests("frappe", costBase: 10, costMultiplier: 1.15f,
                effectType: UpgradeEffectType.ForcePerTap, effectValue: 1f);
            Assert.AreEqual(10.0, u.GetCostForLevel(0).ToDouble(), 1e-9);
        }

        [Test]
        public void Cost_Scales_Geometrically_With_Level()
        {
            var u = UpgradeData.CreateForTests("frappe", costBase: 10, costMultiplier: 1.15f,
                effectType: UpgradeEffectType.ForcePerTap, effectValue: 1f);
            // cost(level) = base * mult^level
            Assert.AreEqual(10.0 * 1.15, u.GetCostForLevel(1).ToDouble(), 1e-6);
            Assert.AreEqual(10.0 * 1.15 * 1.15, u.GetCostForLevel(2).ToDouble(), 1e-6);
            Assert.AreEqual(10.0 * System.Math.Pow(1.15, 10), u.GetCostForLevel(10).ToDouble(), 1e-4);
        }

        [Test]
        public void Disciple_Cost_Profile_Matches_Roadmap()
        {
            var u = UpgradeData.CreateForTests("disciple", costBase: 50, costMultiplier: 1.20f,
                effectType: UpgradeEffectType.ForcePerSecond, effectValue: 1f);
            Assert.AreEqual(50.0, u.GetCostForLevel(0).ToDouble(), 1e-9);
            Assert.AreEqual(50.0 * 1.20, u.GetCostForLevel(1).ToDouble(), 1e-6);
            // Level 20: 50 * 1.20^20 ≈ 1916
            Assert.AreEqual(50.0 * System.Math.Pow(1.20, 20), u.GetCostForLevel(20).ToDouble(), 1e-2);
        }

        [Test]
        public void Meditation_Has_Steep_Cost_Curve()
        {
            var u = UpgradeData.CreateForTests("meditation", costBase: 200, costMultiplier: 1.50f,
                effectType: UpgradeEffectType.ComboMultiplierBonus, effectValue: 0.05f);
            // x1.50 per level → level 5 ≈ 200 * 7.59 = 1518
            Assert.AreEqual(200.0 * System.Math.Pow(1.50, 5), u.GetCostForLevel(5).ToDouble(), 1e-2);
        }

        [Test]
        public void Properties_Round_Trip()
        {
            var u = UpgradeData.CreateForTests("frappe", costBase: 10, costMultiplier: 1.15f,
                effectType: UpgradeEffectType.ForcePerTap, effectValue: 1f, displayName: "Frappe");
            Assert.AreEqual("frappe", u.UpgradeId);
            Assert.AreEqual("Frappe", u.DisplayName);
            Assert.AreEqual(UpgradeEffectType.ForcePerTap, u.EffectType);
            Assert.AreEqual(1.15f, u.CostMultiplier);
            Assert.AreEqual(1f, u.EffectValue);
        }
    }
}
