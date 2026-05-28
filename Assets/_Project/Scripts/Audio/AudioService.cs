using System.Collections.Generic;
using UnityEngine;

namespace Saga.Audio
{
    /// <summary>
    /// Sprint 7.5 audio service. Pre-generates all SFX clips once on construction (cheap —
    /// total &lt;1MB) and plays them via a single <see cref="AudioSource"/> attached to the
    /// GameManager GO. No external .wav files; clips are produced by
    /// <see cref="ProceduralSoundGenerator"/>.
    ///
    /// All Sprint 11+ work needs to do is replace the clip generation with
    /// <c>Resources.Load&lt;AudioClip&gt;(name)</c> when real audio assets land.
    /// </summary>
    public sealed class AudioService
    {
        public enum Sfx
        {
            TapBasic,
            TapCombo,
            UpgradeBuy,
            VagueTrigger,
            AdversaireArrive,
            CapitaineArrive,
            MaitreArrive,
            Death,
            PrestigePhase
        }

        private readonly Dictionary<Sfx, AudioClip> _bank = new Dictionary<Sfx, AudioClip>();
        private readonly AudioSource _source;
        private float _masterVolume = 0.7f;

        public float MasterVolume
        {
            get => _masterVolume;
            set { _masterVolume = Mathf.Clamp01(value); if (_source != null) _source.volume = _masterVolume; }
        }

        public AudioService(AudioSource source)
        {
            _source = source;
            if (_source != null) _source.volume = _masterVolume;
            Rebuild();
        }

        /// <summary>Re-synthesize the SFX bank. Call after changing the generator seed/params.</summary>
        public void Rebuild()
        {
            _bank.Clear();
            _bank[Sfx.TapBasic] = ProceduralSoundGenerator.TapBasic();
            _bank[Sfx.TapCombo] = ProceduralSoundGenerator.TapCombo();
            _bank[Sfx.UpgradeBuy] = ProceduralSoundGenerator.UpgradeBuy();
            _bank[Sfx.VagueTrigger] = ProceduralSoundGenerator.VagueTrigger();
            _bank[Sfx.AdversaireArrive] = ProceduralSoundGenerator.AdversaireArrive();
            _bank[Sfx.CapitaineArrive] = ProceduralSoundGenerator.CapitaineArrive();
            _bank[Sfx.MaitreArrive] = ProceduralSoundGenerator.MaitreArrive();
            _bank[Sfx.Death] = ProceduralSoundGenerator.Death();
            _bank[Sfx.PrestigePhase] = ProceduralSoundGenerator.PrestigePhase();
        }

        /// <summary>Play a one-shot SFX. No-op if the bank or audio source is unavailable.</summary>
        public void Play(Sfx sfx, float volumeScale = 1f)
        {
            if (_source == null) return;
            if (!_bank.TryGetValue(sfx, out var clip) || clip == null) return;
            _source.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        /// <summary>Returns the clip baked for a given SFX id — useful for tests / preview tools.</summary>
        public AudioClip GetClip(Sfx sfx) => _bank.TryGetValue(sfx, out var c) ? c : null;
    }
}
