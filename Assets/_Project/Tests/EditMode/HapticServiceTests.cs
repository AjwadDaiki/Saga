using NUnit.Framework;
using Saga.Audio;

namespace Saga.Tests
{
    /// <summary>
    /// Sprint 7.5 haptic service — minimal contract test. Real vibration only fires on device,
    /// so we just verify the enable/disable gate + that all 3 strengths are reachable without throwing.
    /// </summary>
    [TestFixture]
    public class HapticServiceTests
    {
        [Test]
        public void Vibrate_All_Strengths_Without_Exception()
        {
            var svc = new HapticService();
            Assert.DoesNotThrow(() => svc.Light());
            Assert.DoesNotThrow(() => svc.Medium());
            Assert.DoesNotThrow(() => svc.Heavy());
        }

        [Test]
        public void Enabled_Toggles_Vibration_Path()
        {
            var svc = new HapticService { Enabled = false };
            Assert.IsFalse(svc.Enabled);
            // No throw expected; the gate just no-ops.
            svc.Heavy();
            svc.Enabled = true;
            Assert.IsTrue(svc.Enabled);
        }
    }
}
