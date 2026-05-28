using UnityEngine;

namespace Saga.Audio
{
    /// <summary>
    /// Sprint 7.5 placeholder sound generator. Synthesizes <see cref="AudioClip"/>s at runtime
    /// from a tiny library of waveform + envelope primitives so we never ship a .wav file
    /// (Ajwad's beatmaker will produce real audio Sprint 11). All functions return mono clips
    /// at 44.1 kHz. The clips don't loop and don't allocate beyond their sample buffer.
    /// </summary>
    public static class ProceduralSoundGenerator
    {
        public const int SampleRate = 44_100;

        // --- Primitives -------------------------------------------------

        /// <summary>ASR envelope helper. Attack/release in seconds, S is the sustain plateau.</summary>
        public static float Envelope(float t, float totalDuration, float attack = 0.005f, float release = 0.04f, float sustainLevel = 1f)
        {
            if (t < 0f || t > totalDuration) return 0f;
            var sustainEnd = totalDuration - release;
            if (t < attack) return Mathf.Lerp(0f, sustainLevel, t / attack);
            if (t < sustainEnd) return sustainLevel;
            var rt = (t - sustainEnd) / release;
            return Mathf.Lerp(sustainLevel, 0f, rt);
        }

        /// <summary>Sine wave at frequency f Hz with optional pitch glide (semitones over the clip).</summary>
        public static AudioClip Sine(string name, float frequency, float duration, float volume = 0.5f, float pitchGlide = 0f, float attack = 0.005f, float release = 0.05f)
        {
            var samples = Mathf.Max(1, Mathf.CeilToInt(duration * SampleRate));
            var data = new float[samples];
            var phase = 0.0;
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)SampleRate;
                var f = frequency * Mathf.Pow(2f, (pitchGlide * (t / duration)) / 12f);
                phase += 2.0 * Mathf.PI * f / SampleRate;
                var env = Envelope(t, duration, attack, release);
                data[i] = Mathf.Sin((float)phase) * env * volume;
            }
            return BuildClip(name, data);
        }

        /// <summary>Filtered noise (single-pole low-pass) — usable for woosh, dust, drums.</summary>
        public static AudioClip Noise(string name, float duration, float cutoff01 = 0.3f, float volume = 0.4f, float attack = 0.005f, float release = 0.08f)
        {
            var samples = Mathf.Max(1, Mathf.CeilToInt(duration * SampleRate));
            var data = new float[samples];
            var rng = new System.Random(0xBADBEEF);
            var last = 0f;
            var a = Mathf.Clamp01(cutoff01);
            for (var i = 0; i < samples; i++)
            {
                var raw = (float)(rng.NextDouble() * 2.0 - 1.0);
                last += (raw - last) * a;
                var env = Envelope(i / (float)SampleRate, duration, attack, release);
                data[i] = last * env * volume;
            }
            return BuildClip(name, data);
        }

        /// <summary>Mix two clips into a new one. Used for layered effects (sine + noise = drum).</summary>
        public static AudioClip Mix(string name, AudioClip a, AudioClip b, float weightA = 0.5f, float weightB = 0.5f)
        {
            var len = Mathf.Max(a.samples, b.samples);
            var data = new float[len];
            var aData = new float[a.samples];
            var bData = new float[b.samples];
            a.GetData(aData, 0);
            b.GetData(bData, 0);
            for (var i = 0; i < len; i++)
            {
                var av = i < aData.Length ? aData[i] : 0f;
                var bv = i < bData.Length ? bData[i] : 0f;
                data[i] = av * weightA + bv * weightB;
            }
            return BuildClip(name, data);
        }

        private static AudioClip BuildClip(string name, float[] samples)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // --- Saga sound preset bank ------------------------------------

        public static AudioClip TapBasic() => Noise("sfx_tap", 0.08f, cutoff01: 0.45f, volume: 0.25f, release: 0.05f);

        public static AudioClip TapCombo() => Sine("sfx_combo", 880f, 0.18f, volume: 0.32f, pitchGlide: 5f, attack: 0.002f, release: 0.10f);

        public static AudioClip UpgradeBuy()
        {
            var ping = Sine("sfx_buy_ping", 1200f, 0.20f, volume: 0.30f, pitchGlide: 3f, release: 0.12f);
            var click = Noise("sfx_buy_click", 0.04f, cutoff01: 0.6f, volume: 0.18f);
            return Mix("sfx_upgrade_buy", ping, click, weightA: 0.7f, weightB: 0.3f);
        }

        public static AudioClip VagueTrigger()
        {
            var boom = Sine("sfx_vague_boom", 80f, 0.45f, volume: 0.6f, pitchGlide: -6f, attack: 0.005f, release: 0.20f);
            var crash = Noise("sfx_vague_crash", 0.35f, cutoff01: 0.25f, volume: 0.45f);
            return Mix("sfx_vague_trigger", boom, crash, weightA: 0.55f, weightB: 0.45f);
        }

        public static AudioClip AdversaireArrive()
        {
            var thud = Sine("sfx_adv_thud", 140f, 0.22f, volume: 0.45f, pitchGlide: -3f, release: 0.12f);
            var hit = Noise("sfx_adv_hit", 0.10f, cutoff01: 0.35f, volume: 0.30f);
            return Mix("sfx_adv_arrive", thud, hit, weightA: 0.6f, weightB: 0.4f);
        }

        public static AudioClip CapitaineArrive()
        {
            var thud = Sine("sfx_cap_thud", 100f, 0.40f, volume: 0.55f, pitchGlide: -4f, release: 0.18f);
            var noise = Noise("sfx_cap_noise", 0.20f, cutoff01: 0.20f, volume: 0.40f);
            return Mix("sfx_cap_arrive", thud, noise, weightA: 0.65f, weightB: 0.35f);
        }

        public static AudioClip MaitreArrive()
        {
            var deep = Sine("sfx_maitre_deep", 65f, 0.80f, volume: 0.55f, pitchGlide: -2f, attack: 0.01f, release: 0.30f);
            var sting = Sine("sfx_maitre_sting", 220f, 0.60f, volume: 0.30f, pitchGlide: 3f, attack: 0.04f, release: 0.20f);
            return Mix("sfx_maitre_arrive", deep, sting, weightA: 0.7f, weightB: 0.3f);
        }

        public static AudioClip Death() => Sine("sfx_death", 55f, 1.2f, volume: 0.50f, pitchGlide: -3f, attack: 0.02f, release: 0.60f);

        public static AudioClip PrestigePhase() => Sine("sfx_prestige_drone", 110f, 1.8f, volume: 0.32f, pitchGlide: 0f, attack: 0.20f, release: 0.80f);
    }
}
