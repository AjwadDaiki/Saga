using NUnit.Framework;
using Saga.Data;
using UnityEngine;

namespace Saga.Tests
{
    /// <summary>
    /// Sprint 7 SpriteLayerSet coverage — frame lookup contract + fallback semantics.
    /// </summary>
    [TestFixture]
    public class SpriteLayerSetTests
    {
        private static Sprite MakeSprite(string name)
        {
            var tex = new Texture2D(2, 2);
            var s = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            s.name = name;
            return s;
        }

        [Test]
        public void GetFrames_Returns_Idle_Array_For_Idle()
        {
            var idle = new[] { MakeSprite("idle0"), MakeSprite("idle1") };
            var layer = SpriteLayerSet.CreateForTests("body_x", EquipmentSlot.Body, Voie.None, Rarity.Commun,
                statsBonusForce: 0, idle: idle);
            var frames = layer.GetFrames("idle");
            Assert.IsNotNull(frames);
            Assert.AreEqual(2, frames.Length);
            Assert.AreSame(idle[0], frames[0]);
        }

        [Test]
        public void GetFrames_Returns_Null_For_Unknown_Animation()
        {
            var layer = SpriteLayerSet.CreateForTests("body_x", EquipmentSlot.Body, Voie.None, Rarity.Commun, 0);
            Assert.IsNull(layer.GetFrames("unknown_anim"));
        }

        [Test]
        public void StatsBonusForce_Reflects_Configured_Value()
        {
            var layer = SpriteLayerSet.CreateForTests("weapon_x", EquipmentSlot.Weapon, Voie.Samurai,
                Rarity.Legendaire, statsBonusForce: 80, statsBonusCrit: 0.06f);
            Assert.AreEqual(80.0, layer.StatsBonusForce.ToDouble(), 1e-9);
            Assert.AreEqual(0.06f, layer.StatsBonusCrit, 1e-6);
            Assert.AreEqual(EquipmentSlot.Weapon, layer.SlotType);
            Assert.AreEqual(Voie.Samurai, layer.Voie);
        }
    }
}
