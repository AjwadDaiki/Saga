using NUnit.Framework;
using Saga.Data;
using Saga.Gameplay;
using UnityEngine;

namespace Saga.Tests
{
    /// <summary>
    /// Sprint 7 LayeredCharacterRenderer coverage — looping policy + slot swap.
    /// </summary>
    [TestFixture]
    public class LayeredCharacterRendererTests
    {
        [Test]
        public void IsLooping_Returns_True_For_Idle()
        {
            Assert.IsTrue(LayeredCharacterRenderer.IsLooping("idle"));
        }

        [Test]
        public void IsLooping_Returns_True_For_Meditation()
        {
            Assert.IsTrue(LayeredCharacterRenderer.IsLooping("meditation"));
        }

        [Test]
        public void IsLooping_Returns_False_For_Attack_And_Hurt_And_Die()
        {
            Assert.IsFalse(LayeredCharacterRenderer.IsLooping("attack1"));
            Assert.IsFalse(LayeredCharacterRenderer.IsLooping("attack2"));
            Assert.IsFalse(LayeredCharacterRenderer.IsLooping("attack3"));
            Assert.IsFalse(LayeredCharacterRenderer.IsLooping("hurt"));
            Assert.IsFalse(LayeredCharacterRenderer.IsLooping("die"));
        }

        [Test]
        public void SetLayer_Updates_The_Right_Slot()
        {
            var go = new GameObject("rendererTest", typeof(LayeredCharacterRenderer));
            try
            {
                var renderer = go.GetComponent<LayeredCharacterRenderer>();
                var armor = SpriteLayerSet.CreateForTests("armor_x", EquipmentSlot.Armor, Voie.None,
                    Rarity.Commun, statsBonusForce: 5);
                renderer.SetLayer(EquipmentSlot.Armor, armor);
                Assert.AreSame(armor, renderer.ArmorLayer);
                Assert.IsNull(renderer.BodyLayer);
                Assert.IsNull(renderer.WeaponLayer);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
