using NUnit.Framework;
using Saga.Gameplay;

namespace Saga.Tests
{
    [TestFixture]
    public class ComboSystemTests
    {
        [Test]
        public void Initial_State_Is_Tier_Zero()
        {
            var c = new ComboSystem();
            Assert.AreEqual(0, c.TapCount);
            Assert.AreEqual(0, c.CurrentTier);
            Assert.AreEqual(1f, c.Multiplier);
        }

        [Test]
        public void First_Two_Taps_Stay_In_Warmup_Tier()
        {
            var c = new ComboSystem();
            Assert.AreEqual(1f, c.RegisterTap());
            Assert.AreEqual(1f, c.RegisterTap());
            Assert.AreEqual(2, c.TapCount);
            Assert.AreEqual(0, c.CurrentTier);
        }

        [Test]
        public void Third_Tap_Enters_Tier_One_At_1_2x()
        {
            var c = new ComboSystem();
            c.RegisterTap();
            c.RegisterTap();
            var m = c.RegisterTap();
            Assert.AreEqual(1.2f, m);
            Assert.AreEqual(1, c.CurrentTier);
        }

        [Test]
        public void Sixth_Tap_Enters_Tier_Two_At_1_5x()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 5; i++) c.RegisterTap();
            var m = c.RegisterTap();
            Assert.AreEqual(1.5f, m);
            Assert.AreEqual(2, c.CurrentTier);
        }

        [Test]
        public void Tenth_Tap_Caps_At_Tier_Three_2x()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 9; i++) c.RegisterTap();
            var m = c.RegisterTap();
            Assert.AreEqual(2f, m);
            Assert.AreEqual(3, c.CurrentTier);
        }

        [Test]
        public void Past_Tenth_Tap_Multiplier_Stays_Capped()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 30; i++) c.RegisterTap();
            Assert.AreEqual(2f, c.Multiplier);
            Assert.AreEqual(3, c.CurrentTier);
            Assert.AreEqual(30, c.TapCount);
        }

        [Test]
        public void Tick_Within_Window_Does_Not_Reset()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 5; i++) c.RegisterTap();
            c.Tick(0.5f);
            c.Tick(0.5f);
            Assert.AreEqual(5, c.TapCount);
            Assert.AreEqual(1.2f, c.Multiplier);
        }

        [Test]
        public void Tick_Past_Window_Resets_To_Zero()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 5; i++) c.RegisterTap();
            c.Tick(ComboSystem.WindowSeconds + 0.01f);
            Assert.AreEqual(0, c.TapCount);
            Assert.AreEqual(0, c.CurrentTier);
            Assert.AreEqual(1f, c.Multiplier);
        }

        [Test]
        public void Tap_After_Expire_Starts_From_Zero()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 8; i++) c.RegisterTap();
            c.Tick(ComboSystem.WindowSeconds + 0.01f);
            var m = c.RegisterTap();
            Assert.AreEqual(1f, m);
            Assert.AreEqual(1, c.TapCount);
        }

        [Test]
        public void Manual_Reset_Clears_State()
        {
            var c = new ComboSystem();
            for (var i = 0; i < 10; i++) c.RegisterTap();
            c.Reset();
            Assert.AreEqual(0, c.TapCount);
            Assert.AreEqual(0, c.CurrentTier);
            Assert.AreEqual(1f, c.Multiplier);
        }

        [Test]
        public void Tick_While_Idle_Is_No_Op()
        {
            var c = new ComboSystem();
            Assert.DoesNotThrow(() => c.Tick(10f));
            Assert.AreEqual(0, c.TapCount);
        }
    }
}
