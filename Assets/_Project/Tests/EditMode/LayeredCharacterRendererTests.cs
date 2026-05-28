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

        [Test]
        public void CreateRuntime_Body_Has_Frames_Available_For_Idle()
        {
            // Mirrors what MainSceneBootstrap.CreatePlaceholderBodyLayerSet does to keep the
            // player visible when the asset SpriteLayerSet hasn't been generated yet.
            var tex = new Texture2D(2, 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0f));
            sprite.name = "test_placeholder";
            var layer = SpriteLayerSet.CreateRuntime("body_runtime", EquipmentSlot.Body,
                idle: new[] { sprite });
            Assert.AreEqual("body_runtime", layer.Id);
            Assert.AreEqual(EquipmentSlot.Body, layer.SlotType);
            var idleFrames = layer.GetFrames("idle");
            Assert.IsNotNull(idleFrames);
            Assert.AreEqual(1, idleFrames.Length);
            Assert.AreSame(sprite, idleFrames[0]);
        }

        [Test]
        public void SetLayer_Body_With_Runtime_Placeholder_Survives_Boot()
        {
            // Reproduces BUG 1 fix : after BuildCharacter assigns a runtime placeholder body
            // SpriteLayerSet, the renderer reports that body layer as set (no null after Awake).
            var go = new GameObject("rendererBootTest", typeof(LayeredCharacterRenderer));
            try
            {
                var renderer = go.GetComponent<LayeredCharacterRenderer>();
                var tex = new Texture2D(2, 2);
                var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0f));
                var body = SpriteLayerSet.CreateRuntime("body_chibi_neutral", EquipmentSlot.Body,
                    idle: new[] { sprite });
                renderer.SetLayer(EquipmentSlot.Body, body);
                Assert.AreSame(body, renderer.BodyLayer);
                Assert.IsNotNull(renderer.BodyLayer.SpriteIdle);
                Assert.AreEqual(1, renderer.BodyLayer.SpriteIdle.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
