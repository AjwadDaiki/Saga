using UnityEngine;

namespace Saga.Audio
{
    /// <summary>
    /// Sprint 7.5 haptic feedback. Wraps <see cref="Handheld.Vibrate"/> on Android with a
    /// duration intent (Light / Medium / Heavy). iOS UIImpactFeedbackGenerator integration is
    /// stubbed — Sprint 8+ when we add Unity iOS plugin we'll route via AndroidJavaObject / iOS bridge.
    ///
    /// In the Unity editor and on desktop, this is a no-op. Test on a real device.
    /// Settings toggle: <see cref="Enabled"/> = false disables all haptics globally.
    /// </summary>
    public sealed class HapticService
    {
        public enum Strength { Light, Medium, Heavy }

        public bool Enabled { get; set; } = true;

        public void Light() => Vibrate(Strength.Light);
        public void Medium() => Vibrate(Strength.Medium);
        public void Heavy() => Vibrate(Strength.Heavy);

        public void Vibrate(Strength strength)
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Handheld.Vibrate has no duration parameter on Android — it always pulses ~250ms.
            // For variable duration we'd need AndroidJavaObject(Vibrator).vibrate(ms) which we
            // wire Sprint 8+ alongside the iOS bridge. For now the strength is just a hint.
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS UIImpactFeedbackGenerator placeholder — Sprint 8+ swap for a native plugin call.
            Handheld.Vibrate();
#else
            // Editor / desktop / WebGL: log only.
            // Debug.Log($"[Haptic] {strength}");  // disabled to keep the console clean during play
#endif
        }
    }
}
