using BreakInfinity;
using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    /// <summary>
    /// Sprint 7 EquipmentService coverage — inventory + equip/unequip + stat aggregation.
    /// Uses CreateForTests factories so we don't touch Resources/.
    /// </summary>
    [TestFixture]
    public class EquipmentServiceTests
    {
        private SpriteLayerSet _bodyDefault;
        private SpriteLayerSet _armorCommun;
        private SpriteLayerSet _armorRare;
        private SpriteLayerSet _weaponKatana;

        [SetUp]
        public void SetUp()
        {
            _bodyDefault = SpriteLayerSet.CreateForTests(EquipmentConstants.DefaultBodyId,
                EquipmentSlot.Body, Voie.None, Rarity.Commun, statsBonusForce: 0);
            _armorCommun = SpriteLayerSet.CreateForTests("armor_tunique_chanvre",
                EquipmentSlot.Armor, Voie.None, Rarity.Commun, statsBonusForce: 5);
            _armorRare = SpriteLayerSet.CreateForTests("armor_haubert_acier",
                EquipmentSlot.Armor, Voie.None, Rarity.Affute, statsBonusForce: 25);
            _weaponKatana = SpriteLayerSet.CreateForTests("weapon_katana_samurai",
                EquipmentSlot.Weapon, Voie.Samurai, Rarity.Legendaire, statsBonusForce: 80);
        }

        private (GameState state, ContentDatabase content, EquipmentService svc) Build()
        {
            var state = new GameState
            {
                equippedBodyId = _bodyDefault.Id
            };
            state.inventoryLayerSetIds.Add(_bodyDefault.Id);
            var content = new ContentDatabase(null, null, null, null,
                new[] { _bodyDefault, _armorCommun, _armorRare, _weaponKatana });
            return (state, content, new EquipmentService(content));
        }

        [Test]
        public void AddToInventory_Adds_Once_And_Is_Idempotent()
        {
            var (state, _, svc) = Build();
            svc.AddToInventory(state, _armorCommun.Id);
            svc.AddToInventory(state, _armorCommun.Id);
            Assert.AreEqual(2, state.inventoryLayerSetIds.Count); // body + armor
            Assert.IsTrue(state.inventoryLayerSetIds.Contains(_armorCommun.Id));
        }

        [Test]
        public void Equip_Refuses_When_Item_Not_Owned()
        {
            var (state, _, svc) = Build();
            // Katana not in inventory yet.
            var ok = svc.Equip(state, _weaponKatana.Id);
            Assert.IsFalse(ok);
            Assert.IsTrue(string.IsNullOrEmpty(state.equippedWeaponId));
        }

        [Test]
        public void Equip_Sets_Correct_Slot_And_Replaces_Previous()
        {
            var (state, _, svc) = Build();
            svc.AddToInventory(state, _armorCommun.Id);
            svc.AddToInventory(state, _armorRare.Id);
            Assert.IsTrue(svc.Equip(state, _armorCommun.Id));
            Assert.AreEqual(_armorCommun.Id, state.equippedArmorId);
            Assert.IsTrue(svc.Equip(state, _armorRare.Id));
            Assert.AreEqual(_armorRare.Id, state.equippedArmorId);
        }

        [Test]
        public void Unequip_Refuses_On_Body_Slot()
        {
            var (state, _, svc) = Build();
            Assert.IsFalse(svc.Unequip(state, EquipmentSlot.Body));
            Assert.AreEqual(_bodyDefault.Id, state.equippedBodyId);
        }

        [Test]
        public void Unequip_Clears_Armor_And_Weapon()
        {
            var (state, _, svc) = Build();
            svc.AddToInventory(state, _armorRare.Id);
            svc.AddToInventory(state, _weaponKatana.Id);
            Assert.IsTrue(svc.Equip(state, _armorRare.Id));
            Assert.IsTrue(svc.Equip(state, _weaponKatana.Id));
            Assert.IsTrue(svc.Unequip(state, EquipmentSlot.Armor));
            Assert.IsTrue(string.IsNullOrEmpty(state.equippedArmorId));
            Assert.AreEqual(_weaponKatana.Id, state.equippedWeaponId);
        }

        [Test]
        public void GetTotalStatsBonus_Sums_All_Equipped_Force_Across_Slots()
        {
            var (state, _, svc) = Build();
            svc.AddToInventory(state, _armorRare.Id);   // +25
            svc.AddToInventory(state, _weaponKatana.Id); // +80
            svc.Equip(state, _armorRare.Id);
            svc.Equip(state, _weaponKatana.Id);
            // Body=0 + Armor=25 + Weapon=80 = 105
            var total = svc.GetTotalStatsBonus(state);
            Assert.AreEqual(105.0, total.ToDouble(), 1e-9);
        }
    }
}
