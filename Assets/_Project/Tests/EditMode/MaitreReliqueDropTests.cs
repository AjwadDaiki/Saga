using NUnit.Framework;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;

namespace Saga.Tests
{
    /// <summary>
    /// Sprint 7 relique drop flow — exercises the EquipmentService.AddToInventory path that
    /// CombatProcessor.OnMaitreDefeated invokes when a Maître has a ReliqueSpriteLayerSetId.
    /// We can't drive a real GameManager from EditMode, so we test the building blocks: that
    /// the layer set ends up in the inventory, that the equippable id matches the MaitreData,
    /// and that re-entering the same Maître doesn't duplicate inventory entries.
    /// </summary>
    [TestFixture]
    public class MaitreReliqueDropTests
    {
        private const string ReliqueId = "relique_lame_yoshitsune";

        private static (GameState state, ContentDatabase content, EquipmentService svc, MaitreData maitre, SpriteLayerSet relique) Build()
        {
            var body = SpriteLayerSet.CreateForTests(EquipmentConstants.DefaultBodyId,
                EquipmentSlot.Body, Voie.None, Rarity.Commun, 0);
            var relique = SpriteLayerSet.CreateForTests(ReliqueId,
                EquipmentSlot.Weapon, Voie.Samurai, Rarity.Mythique, statsBonusForce: 500);
            var maitre = MaitreData.CreateForTests("yoshitsune_l_inatteignable", "Yoshitsune",
                Voie.Samurai, 8000, 4000, 180f, reliqueSpriteLayerSetId: ReliqueId);

            var state = new GameState { equippedBodyId = body.Id };
            state.inventoryLayerSetIds.Add(body.Id);
            var content = new ContentDatabase(null, null, null, new[] { maitre }, new[] { body, relique });
            return (state, content, new EquipmentService(content), maitre, relique);
        }

        [Test]
        public void MaitreData_Exposes_ReliqueSpriteLayerSetId()
        {
            var (_, _, _, maitre, _) = Build();
            Assert.AreEqual(ReliqueId, maitre.ReliqueSpriteLayerSetId);
        }

        [Test]
        public void Granting_Relique_Adds_It_To_Inventory_Once()
        {
            var (state, _, svc, maitre, _) = Build();
            svc.AddToInventory(state, maitre.ReliqueSpriteLayerSetId);
            svc.AddToInventory(state, maitre.ReliqueSpriteLayerSetId);
            Assert.IsTrue(state.inventoryLayerSetIds.Contains(ReliqueId));
            Assert.AreEqual(2, state.inventoryLayerSetIds.Count); // body + relique
        }

        [Test]
        public void Granted_Relique_Is_Equippable_And_Contributes_Stats_Bonus()
        {
            var (state, _, svc, maitre, relique) = Build();
            svc.AddToInventory(state, maitre.ReliqueSpriteLayerSetId);
            Assert.IsTrue(svc.Equip(state, relique.Id));
            Assert.AreEqual(relique.Id, state.equippedWeaponId);
            // Body=0 + Weapon=500 = 500
            Assert.AreEqual(500.0, svc.GetTotalStatsBonus(state).ToDouble(), 1e-9);
        }
    }
}
