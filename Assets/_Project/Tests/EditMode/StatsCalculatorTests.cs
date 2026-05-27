using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class StatsCalculatorTests
    {
        private static (GameState state, ContentDatabase content) BuildFixture(params (int level, UpgradeData data)[] entries)
        {
            var state = new GameState();
            foreach (var (level, data) in entries)
            {
                if (level > 0) state.upgradeLevels[data.UpgradeId] = level;
            }
            var content = new ContentDatabase(System.Linq.Enumerable.Select(entries, e => e.data));
            return (state, content);
        }

        [Test]
        public void ForcePerTap_With_No_Upgrades_Returns_One()
        {
            var (state, content) = BuildFixture();
            Assert.AreEqual(1.0, StatsCalculator.GetForcePerTap(state, content).ToDouble(), 1e-9);
        }

        [Test]
        public void ForcePerTap_Adds_Frappe_Level_Linearly()
        {
            var frappe = UpgradeData.CreateForTests("frappe", 10, 1.15f, UpgradeEffectType.ForcePerTap, 1f);
            var (state, content) = BuildFixture((5, frappe));
            // base 1 + 1 × 5 = 6
            Assert.AreEqual(6.0, StatsCalculator.GetForcePerTap(state, content).ToDouble(), 1e-9);
        }

        [Test]
        public void ForcePerTap_Ignores_Non_ForcePerTap_Upgrades()
        {
            var frappe = UpgradeData.CreateForTests("frappe", 10, 1.15f, UpgradeEffectType.ForcePerTap, 1f);
            var disciple = UpgradeData.CreateForTests("disciple", 50, 1.20f, UpgradeEffectType.ForcePerSecond, 1f);
            var (state, content) = BuildFixture((3, frappe), (10, disciple));
            // base 1 + 1×3 (frappe) + 0 (disciple doesn't add to tap) = 4
            Assert.AreEqual(4.0, StatsCalculator.GetForcePerTap(state, content).ToDouble(), 1e-9);
        }

        [Test]
        public void ForcePerSecond_Sums_All_Passive_Upgrades()
        {
            var disciple = UpgradeData.CreateForTests("disciple", 50, 1.20f, UpgradeEffectType.ForcePerSecond, 1f);
            var (state, content) = BuildFixture((10, disciple));
            Assert.AreEqual(10.0, StatsCalculator.GetForcePerSecond(state, content).ToDouble(), 1e-9);
        }

        [Test]
        public void ForcePerSecond_With_No_Disciple_Is_Zero()
        {
            var frappe = UpgradeData.CreateForTests("frappe", 10, 1.15f, UpgradeEffectType.ForcePerTap, 1f);
            var (state, content) = BuildFixture((5, frappe));
            Assert.AreEqual(0.0, StatsCalculator.GetForcePerSecond(state, content).ToDouble(), 1e-9);
        }

        [Test]
        public void ComboMultiplierBonus_With_No_Meditation_Is_One()
        {
            var (state, content) = BuildFixture();
            Assert.AreEqual(1f, StatsCalculator.GetComboMultiplierBonus(state, content));
        }

        [Test]
        public void ComboMultiplierBonus_Scales_With_Meditation_Level()
        {
            var meditation = UpgradeData.CreateForTests("meditation", 200, 1.50f, UpgradeEffectType.ComboMultiplierBonus, 0.05f);
            var (state, content) = BuildFixture((4, meditation));
            // 1 + 4 × 0.05 = 1.20
            Assert.AreEqual(1.20f, StatsCalculator.GetComboMultiplierBonus(state, content), 1e-5f);
        }

        [Test]
        public void Multiple_Upgrades_Combine_Correctly()
        {
            var frappe = UpgradeData.CreateForTests("frappe", 10, 1.15f, UpgradeEffectType.ForcePerTap, 1f);
            var disciple = UpgradeData.CreateForTests("disciple", 50, 1.20f, UpgradeEffectType.ForcePerSecond, 1f);
            var meditation = UpgradeData.CreateForTests("meditation", 200, 1.50f, UpgradeEffectType.ComboMultiplierBonus, 0.05f);
            var (state, content) = BuildFixture((10, frappe), (5, disciple), (2, meditation));

            Assert.AreEqual(11.0, StatsCalculator.GetForcePerTap(state, content).ToDouble(), 1e-9);
            Assert.AreEqual(5.0, StatsCalculator.GetForcePerSecond(state, content).ToDouble(), 1e-9);
            Assert.AreEqual(1.10f, StatsCalculator.GetComboMultiplierBonus(state, content), 1e-5f);
        }
    }
}
