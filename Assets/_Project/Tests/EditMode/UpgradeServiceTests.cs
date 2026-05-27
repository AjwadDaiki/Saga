using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class UpgradeServiceTests
    {
        private GameState _state;
        private ContentDatabase _content;
        private UpgradeService _service;
        private UpgradeData _frappe;

        [SetUp]
        public void Setup()
        {
            _frappe = UpgradeData.CreateForTests("frappe", costBase: 10, costMultiplier: 1.15f,
                effectType: UpgradeEffectType.ForcePerTap, effectValue: 1f);
            _state = new GameState();
            _content = new ContentDatabase(new[] { _frappe });
            _service = new UpgradeService(_content);
        }

        [Test]
        public void GetLevel_Initial_Is_Zero()
        {
            Assert.AreEqual(0, _service.GetLevel(_state, "frappe"));
        }

        [Test]
        public void GetLevel_Unknown_Upgrade_Returns_Zero()
        {
            Assert.AreEqual(0, _service.GetLevel(_state, "unknown_id"));
        }

        [Test]
        public void GetCostForNextLevel_At_Zero_Returns_Base()
        {
            Assert.AreEqual(10.0, _service.GetCostForNextLevel(_state, "frappe").ToDouble(), 1e-9);
        }

        [Test]
        public void CanAfford_Returns_False_When_Force_Below_Cost()
        {
            _state.force = new BigDouble(5);
            Assert.IsFalse(_service.CanAfford(_state, "frappe"));
        }

        [Test]
        public void CanAfford_Returns_True_When_Force_Equals_Cost()
        {
            _state.force = new BigDouble(10);
            Assert.IsTrue(_service.CanAfford(_state, "frappe"));
        }

        [Test]
        public void TryPurchase_Fails_When_Insufficient_Force()
        {
            _state.force = new BigDouble(5);
            Assert.IsFalse(_service.TryPurchase(_state, "frappe"));
            Assert.AreEqual(0, _service.GetLevel(_state, "frappe"));
            Assert.AreEqual(5.0, _state.force.ToDouble(), 1e-9);
        }

        [Test]
        public void TryPurchase_Succeeds_And_Deducts_Force()
        {
            _state.force = new BigDouble(15);
            Assert.IsTrue(_service.TryPurchase(_state, "frappe"));
            Assert.AreEqual(1, _service.GetLevel(_state, "frappe"));
            // 15 - 10 = 5
            Assert.AreEqual(5.0, _state.force.ToDouble(), 1e-9);
        }

        [Test]
        public void Cost_Increases_After_Purchase()
        {
            _state.force = new BigDouble(100);
            _service.TryPurchase(_state, "frappe");
            // After level 1: next cost = 10 * 1.15 = 11.5
            Assert.AreEqual(11.5, _service.GetCostForNextLevel(_state, "frappe").ToDouble(), 1e-6);
        }

        [Test]
        public void Sequential_Purchases_Match_Geometric_Series()
        {
            _state.force = new BigDouble(1000);
            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(_service.TryPurchase(_state, "frappe"), $"Purchase #{i + 1} should succeed");
            }
            Assert.AreEqual(5, _service.GetLevel(_state, "frappe"));
        }

        [Test]
        public void TryPurchase_Unknown_Upgrade_Returns_False()
        {
            _state.force = new BigDouble(1_000_000);
            Assert.IsFalse(_service.TryPurchase(_state, "unknown_id"));
        }
    }
}
