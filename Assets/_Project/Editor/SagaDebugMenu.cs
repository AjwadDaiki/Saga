using System.Linq;
using Saga.Core;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Cheats / dev shortcuts. Only meaningful in Play Mode (the runtime services exist).
    /// All entries are gated so they no-op gracefully outside Play Mode rather than fail.
    /// </summary>
    public static class SagaDebugMenu
    {
        [MenuItem("Saga/Debug/Give All Sprite Layer Sets To Inventory", false, 100)]
        public static void GiveAllSpriteLayerSets()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SagaDebug] Enter Play Mode first — runtime services aren't initialized at edit time.");
                return;
            }

            var gm = GameManager.Instance;
            if (gm == null || gm.Content == null || gm.Equipment == null || gm.State == null)
            {
                Debug.LogError("[SagaDebug] GameManager / Content / Equipment / State unavailable.");
                return;
            }

            var all = gm.Content.AllSpriteLayerSets;
            if (all == null || all.Count == 0)
            {
                Debug.LogWarning("[SagaDebug] ContentDatabase has 0 SpriteLayerSets — run 'Saga > Sprint 7 > Generate Sprite Layer Sets' first.");
                return;
            }

            var added = 0;
            foreach (var layer in all)
            {
                if (layer == null || string.IsNullOrEmpty(layer.Id)) continue;
                if (gm.State.inventoryLayerSetIds != null && gm.State.inventoryLayerSetIds.Contains(layer.Id)) continue;
                gm.Equipment.AddToInventory(gm.State, layer.Id);
                added++;
            }

            Debug.Log($"[SagaDebug] Granted {added} SpriteLayerSets to inventory (total now {gm.State.inventoryLayerSetIds.Count}).");
        }

        [MenuItem("Saga/Debug/Print Inventory + Equipment Snapshot", false, 110)]
        public static void PrintSnapshot()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SagaDebug] Enter Play Mode first.");
                return;
            }
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;
            var inv = gm.State.inventoryLayerSetIds ?? new System.Collections.Generic.List<string>();
            Debug.Log($"[SagaDebug] Inventory ({inv.Count}): {string.Join(", ", inv)}");
            Debug.Log($"[SagaDebug] Equipped: Body='{gm.State.equippedBodyId}' Armor='{gm.State.equippedArmorId}' Weapon='{gm.State.equippedWeaponId}'");
            Debug.Log($"[SagaDebug] Voie selected='{gm.State.voieSelectedId}' mastered=[{string.Join(",", gm.State.voiesMastered ?? Enumerable.Empty<string>().ToList())}]");
        }

        [MenuItem("Saga/Debug/Reset Save File", false, 200)]
        public static void ResetSave()
        {
            if (EditorUtility.DisplayDialog("Reset Save?",
                "This will delete the save file. The game must be restarted (exit + re-enter Play Mode).\n\nContinue?",
                "Delete", "Cancel"))
            {
                var path = System.IO.Path.Combine(Application.persistentDataPath, Saga.Save.SaveService.SaveFileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    Debug.Log($"[SagaDebug] Deleted save file at {path}");
                }
                else
                {
                    Debug.Log("[SagaDebug] No save file to delete.");
                }
            }
        }
    }
}
