using BreakInfinity;
using NUnit.Framework;
using Saga.Data;
using UnityEngine;

namespace Saga.Tests
{
    /// <summary>Sprint 7.5 DesignTokens coverage — runtime fallback + lookup helpers.</summary>
    [TestFixture]
    public class DesignTokensTests
    {
        [Test]
        public void VoieColor_Maps_Each_Voie_To_A_Distinct_Color()
        {
            var t = ScriptableObject.CreateInstance<DesignTokens>();
            Assert.AreEqual(t.voieSamurai, t.VoieColor(Voie.Samurai));
            Assert.AreEqual(t.voieViking, t.VoieColor(Voie.Viking));
            Assert.AreEqual(t.voieWuxia, t.VoieColor(Voie.Wuxia));
            Assert.AreEqual(t.voieAztec, t.VoieColor(Voie.Aztec));
            // None falls back to textSecondary (graceful default).
            Assert.AreEqual(t.textSecondary, t.VoieColor(Voie.None));
        }

        [Test]
        public void RarityColor_Returns_Expected_Tier_Color()
        {
            var t = ScriptableObject.CreateInstance<DesignTokens>();
            Assert.AreEqual(t.rarityCommun, t.RarityColor(Rarity.Commun));
            Assert.AreEqual(t.raritySacre, t.RarityColor(Rarity.Sacre));
        }

        [Test]
        public void ForceFontSize_Shrinks_For_Bigger_Magnitudes()
        {
            var t = ScriptableObject.CreateInstance<DesignTokens>();
            Assert.AreEqual(96, t.ForceFontSize(new BigDouble(500)));
            Assert.AreEqual(84, t.ForceFontSize(new BigDouble(5_000_000)));
            Assert.AreEqual(72, t.ForceFontSize(new BigDouble(5_000_000_000L)));
            Assert.AreEqual(64, t.ForceFontSize(new BigDouble(5_000_000_000_000L)));
        }

        [Test]
        public void ForceTierColor_Promotes_Through_Tiers()
        {
            var t = ScriptableObject.CreateInstance<DesignTokens>();
            Assert.AreEqual(t.textSecondary, t.ForceTierColor(new BigDouble(50)));
            Assert.AreEqual(t.textPrimary, t.ForceTierColor(new BigDouble(5_000)));
            Assert.AreEqual(t.accentPrimary, t.ForceTierColor(new BigDouble(500_000)));
            Assert.AreEqual(t.accentPrimary, t.ForceTierColor(new BigDouble(50_000_000)));
        }
    }
}
