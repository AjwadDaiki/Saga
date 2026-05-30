using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Owns equipment mutations of <see cref="GameState"/>. The single source of truth for
    /// "what is the character wearing right now" + "what's in the inventory".
    ///
    /// Sprint 7 MVP: 3 slots (Body / Armor / Weapon). Sprint 8+ extends (Helmet, etc.).
    /// </summary>
    public sealed class EquipmentService
    {
        private readonly ContentDatabase _content;

        public EquipmentService(ContentDatabase content)
        {
            _content = content;
        }

        /// <summary>Add a SpriteLayerSet id to the inventory if not already present. Raises event on add.</summary>
        public void AddToInventory(GameState state, string layerSetId)
        {
            if (state == null || string.IsNullOrEmpty(layerSetId)) return;
            if (state.inventoryLayerSetIds == null)
                state.inventoryLayerSetIds = new System.Collections.Generic.List<string>();
            if (state.inventoryLayerSetIds.Contains(layerSetId)) return;
            state.inventoryLayerSetIds.Add(layerSetId);
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseItemAddedToInventory(_content?.GetSpriteLayerSet(layerSetId));
        }

        /// <summary>Equip an owned layer set to its native slot. Refuses wrong-slot assignments.</summary>
        public bool Equip(GameState state, string layerSetId)
        {
            if (state == null || string.IsNullOrEmpty(layerSetId)) return false;
            var layer = _content?.GetSpriteLayerSet(layerSetId);
            if (layer == null) return false;
            if (state.inventoryLayerSetIds == null || !state.inventoryLayerSetIds.Contains(layerSetId))
            {
                Debug.LogWarning($"[EquipmentService] Cannot equip '{layerSetId}': not owned.");
                return false;
            }

            var slot = layer.SlotType;
            var previousId = GetEquippedId(state, slot);
            var previous = !string.IsNullOrEmpty(previousId) ? _content.GetSpriteLayerSet(previousId) : null;
            SetEquippedId(state, slot, layerSetId);
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseEquipmentChanged(slot, layer, previous);
            return true;
        }

        /// <summary>Clear a slot. Refuses on <see cref="EquipmentSlot.Body"/> — body is always set.</summary>
        public bool Unequip(GameState state, EquipmentSlot slot)
        {
            if (state == null) return false;
            if (slot == EquipmentSlot.Body)
            {
                Debug.LogWarning("[EquipmentService] Cannot unequip Body (always set).");
                return false;
            }
            var previousId = GetEquippedId(state, slot);
            if (string.IsNullOrEmpty(previousId)) return false;
            var previous = _content?.GetSpriteLayerSet(previousId);
            SetEquippedId(state, slot, null);
            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseEquipmentChanged(slot, null, previous);
            return true;
        }

        /// <summary>Look up the currently-equipped SpriteLayerSet for a slot (null if empty).</summary>
        public SpriteLayerSet GetEquipped(GameState state, EquipmentSlot slot)
        {
            if (state == null || _content == null) return null;
            var id = GetEquippedId(state, slot);
            return string.IsNullOrEmpty(id) ? null : _content.GetSpriteLayerSet(id);
        }

        /// <summary>Sum the Force bonuses across all equipped slots. Reads from ContentDatabase via state IDs.</summary>
        public BigDouble GetTotalStatsBonus(GameState state)
        {
            if (state == null || _content == null) return new BigDouble(0);
            var total = new BigDouble(0);
            foreach (var slot in new[] { EquipmentSlot.Body, EquipmentSlot.Armor, EquipmentSlot.Weapon })
            {
                var layer = GetEquipped(state, slot);
                if (layer == null) continue;
                total += layer.StatsBonusForce;
            }
            return total;
        }

        private static string GetEquippedId(GameState state, EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Body: return state.equippedBodyId;
                case EquipmentSlot.Armor: return state.equippedArmorId;
                case EquipmentSlot.Weapon: return state.equippedWeaponId;
                default: return null;
            }
        }

        private static void SetEquippedId(GameState state, EquipmentSlot slot, string id)
        {
            switch (slot)
            {
                case EquipmentSlot.Body: state.equippedBodyId = id; break;
                case EquipmentSlot.Armor: state.equippedArmorId = id; break;
                case EquipmentSlot.Weapon: state.equippedWeaponId = id; break;
            }
        }
    }
}
