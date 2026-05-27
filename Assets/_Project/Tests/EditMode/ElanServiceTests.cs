using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class ElanServiceTests
    {
        private GameState _state;
        private ElanService _elan;
        private int _fullEventCount;
        private float _lastEmittedCurrent;

        [SetUp]
        public void Setup()
        {
            _state = new GameState { currentElan = 0f };
            _elan = new ElanService();
            _fullEventCount = 0;
            _lastEmittedCurrent = 0f;
            GameEvents.OnElanFull += OnFull;
            GameEvents.OnElanChanged += OnChanged;
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.OnElanFull -= OnFull;
            GameEvents.OnElanChanged -= OnChanged;
        }

        private void OnFull() { _fullEventCount++; }
        private void OnChanged(float current, float max) { _lastEmittedCurrent = current; }

        [Test]
        public void Tap_Without_Combo_Adds_Base_Per_Tap()
        {
            _elan.RegisterTap(_state, comboTier: 0);
            Assert.AreEqual(ElanConstants.ElanPerTap, _state.currentElan, 1e-5);
            Assert.AreEqual(_state.currentElan, _lastEmittedCurrent, 1e-5);
        }

        [Test]
        public void Tap_With_Combo_Applies_Bonus_Multiplier()
        {
            _elan.RegisterTap(_state, comboTier: 2);
            Assert.AreEqual(ElanConstants.ElanPerTap * ElanConstants.ElanComboBonusMultiplier, _state.currentElan, 1e-5);
        }

        [Test]
        public void Decay_After_Idle_Grace_Drops_Elan()
        {
            _state.currentElan = 50f;
            // Burn through the grace window so decay actually kicks in next tick.
            _elan.Tick(_state, ElanConstants.ElanIdleGraceSeconds + 0.1f);
            // Next tick: 1 second of decay.
            _elan.Tick(_state, 1.0f);
            Assert.Less(_state.currentElan, 50f);
            Assert.AreEqual(50f - ElanConstants.ElanDecayPerSec, _state.currentElan, 0.1f);
        }

        [Test]
        public void Tap_Caps_At_Max()
        {
            _state.currentElan = ElanConstants.ElanMax - 1f;
            _elan.RegisterTap(_state, comboTier: 3); // gain would be 10 but cap at 100
            Assert.AreEqual(ElanConstants.ElanMax, _state.currentElan);
        }

        [Test]
        public void OnElanFull_Fires_Exactly_Once_Per_Cycle()
        {
            // Tap until full (20 taps at 5 each = 100).
            for (var i = 0; i < 25; i++) _elan.RegisterTap(_state, comboTier: 0);
            Assert.AreEqual(1, _fullEventCount, "OnElanFull should only fire once when crossing the cap");
            Assert.AreEqual(ElanConstants.ElanMax, _state.currentElan);
        }

        [Test]
        public void ResetForVague_Drops_To_Zero_And_Allows_Refull()
        {
            for (var i = 0; i < 20; i++) _elan.RegisterTap(_state, comboTier: 0);
            Assert.AreEqual(1, _fullEventCount);

            _elan.ResetForVague(_state);
            Assert.AreEqual(0f, _state.currentElan);

            // Now tap to full again — OnElanFull should fire a SECOND time.
            for (var i = 0; i < 20; i++) _elan.RegisterTap(_state, comboTier: 0);
            Assert.AreEqual(2, _fullEventCount);
        }
    }
}
