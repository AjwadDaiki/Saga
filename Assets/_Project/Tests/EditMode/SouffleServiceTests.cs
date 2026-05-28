using NUnit.Framework;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class SouffleServiceTests
    {
        private GameState _state;
        private SouffleService _service;

        [SetUp]
        public void Setup()
        {
            _state = new GameState();
            _service = new SouffleService();
        }

        [Test]
        public void Initial_State_Is_Idle_Not_Buffing_Not_On_Cooldown()
        {
            Assert.IsFalse(_service.IsMeditating);
            Assert.IsFalse(_service.IsBuffActive);
            Assert.IsFalse(_service.IsOnCooldown);
        }

        [Test]
        public void TryStartMeditation_Succeeds_From_Idle()
        {
            var ok = _service.TryStartMeditation(_state);
            Assert.IsTrue(ok);
            Assert.IsTrue(_service.IsMeditating);
            Assert.IsTrue(_service.IsOnCooldown, "Cooldown starts at meditation begin.");
        }

        [Test]
        public void TryStartMeditation_Fails_When_Already_Meditating()
        {
            _service.TryStartMeditation(_state);
            var second = _service.TryStartMeditation(_state);
            Assert.IsFalse(second);
        }

        [Test]
        public void Tick_Transitions_Meditation_To_Buff_After_Five_Seconds()
        {
            _service.TryStartMeditation(_state);
            _service.Tick(_state, SouffleConstants.MeditationDurationSeconds + 0.01f);
            Assert.IsFalse(_service.IsMeditating);
            Assert.IsTrue(_service.IsBuffActive);
        }

        [Test]
        public void Tick_Transitions_Buff_To_Idle_After_Thirty_Seconds()
        {
            _service.TryStartMeditation(_state);
            // Finish meditation.
            _service.Tick(_state, SouffleConstants.MeditationDurationSeconds + 0.01f);
            Assert.IsTrue(_service.IsBuffActive);

            // Drain buff.
            _service.Tick(_state, SouffleConstants.BuffDurationSeconds + 0.01f);
            Assert.IsFalse(_service.IsBuffActive);
            Assert.IsFalse(_service.IsMeditating);
        }

        [Test]
        public void Cooldown_Counts_Down_Each_Tick()
        {
            _service.TryStartMeditation(_state);
            var initial = _service.CooldownRemaining;
            Assert.AreEqual(SouffleConstants.CooldownSeconds, initial, 1e-3);

            _service.Tick(_state, 2f);
            Assert.AreEqual(initial - 2f, _service.CooldownRemaining, 1e-3);
        }

        [Test]
        public void TryStartMeditation_Fails_While_On_Cooldown()
        {
            // First meditation drives the full cycle (5s med + 30s buff) and leaves cooldown active.
            _service.TryStartMeditation(_state);
            _service.Tick(_state, SouffleConstants.MeditationDurationSeconds + 0.01f);  // -> Buffing
            _service.Tick(_state, SouffleConstants.BuffDurationSeconds + 0.01f);        // -> Idle
            Assert.IsFalse(_service.IsMeditating);
            Assert.IsFalse(_service.IsBuffActive);
            // Cooldown should still have ~85s left (120 - 35).
            Assert.IsTrue(_service.IsOnCooldown);

            var retry = _service.TryStartMeditation(_state);
            Assert.IsFalse(retry, "Cannot re-meditate while cooldown is non-zero.");
        }
    }
}
