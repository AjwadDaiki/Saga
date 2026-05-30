using NUnit.Framework;
using Saga.Audio;
using UnityEngine;

namespace Saga.Tests
{
    /// <summary>Sprint 7.5 audio coverage — procedural SFX bank generation + non-null clips.</summary>
    [TestFixture]
    public class AudioServiceTests
    {
        [Test]
        public void Service_Bakes_All_Sfx_Variants_With_Nonzero_Length()
        {
            var go = new GameObject("audioTest");
            try
            {
                var src = go.AddComponent<AudioSource>();
                var svc = new AudioService(src);

                foreach (AudioService.Sfx sfx in System.Enum.GetValues(typeof(AudioService.Sfx)))
                {
                    var clip = svc.GetClip(sfx);
                    Assert.IsNotNull(clip, $"SFX {sfx} should have a baked clip");
                    Assert.Greater(clip.samples, 0, $"SFX {sfx} clip should have samples");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Master_Volume_Clamps_To_Unit_Range()
        {
            var go = new GameObject("audioVolTest");
            try
            {
                var src = go.AddComponent<AudioSource>();
                var svc = new AudioService(src) { MasterVolume = 3f };
                Assert.AreEqual(1f, svc.MasterVolume, 1e-5);
                svc.MasterVolume = -1f;
                Assert.AreEqual(0f, svc.MasterVolume, 1e-5);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Play_Does_Not_Throw_With_Unknown_Source()
        {
            var svc = new AudioService(null);
            Assert.DoesNotThrow(() => svc.Play(AudioService.Sfx.TapBasic));
        }
    }
}
