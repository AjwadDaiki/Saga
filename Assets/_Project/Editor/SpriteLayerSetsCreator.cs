using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for Sprint 7 SpriteLayerSet content.
    /// MVP scope (per Sprint 7 Decision D1): 1 body + 3 armures + 5 armes = 9 SOs.
    /// Idempotent — re-running updates IDs and stats in place; sprite arrays are auto-populated
    /// from the rvros Adventurer body slices (Sprint 8+ will introduce per-set art).
    ///
    /// Menu: <b>Saga > Sprint 7 > Generate Sprite Layer Sets</b>
    /// </summary>
    public static class SpriteLayerSetsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string LayerSetsFolder = "Assets/_Project/Resources/SpriteLayerSets";
        private const string AdventurerArtFolder = "Assets/_Project/Art/Characters/Adventurer";

        [MenuItem("Saga/Sprint 7/Generate Sprite Layer Sets")]
        public static void GenerateLayerSets()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(LayerSetsFolder);

            // -- Body (1) -- always equipped, never droppable. Idle/attack/hurt/die wired to rvros frames.
            CreateOrUpdate(EquipmentConstants.DefaultBodyId, "Corps Neutre", EquipmentSlot.Body,
                Voie.None, Rarity.Commun,
                statsBonusForce: 0, statsBonusCrit: 0f,
                description: "Le corps neutre. Toujours présent.",
                useBodyFrames: true);

            // -- Armures (3) -- Sprint 7 MVP: Commun / Affûté / Légendaire.
            CreateOrUpdate("armor_tunique_chanvre", "Tunique de Chanvre", EquipmentSlot.Armor,
                Voie.None, Rarity.Commun,
                statsBonusForce: 5, statsBonusCrit: 0f,
                description: "Une tunique simple. Elle tient.",
                useBodyFrames: false);

            CreateOrUpdate("armor_haubert_acier", "Haubert d'Acier", EquipmentSlot.Armor,
                Voie.None, Rarity.Affute,
                statsBonusForce: 25, statsBonusCrit: 0.02f,
                description: "Le métal protège, et le métal donne.",
                useBodyFrames: false);

            CreateOrUpdate("armor_kimono_yamato", "Kimono Yamato", EquipmentSlot.Armor,
                Voie.Samurai, Rarity.Legendaire,
                statsBonusForce: 120, statsBonusCrit: 0.05f,
                description: "Tissé par les femmes de Yamato sous la pleine lune.",
                useBodyFrames: false);

            // -- Armes (5) -- 1 par voie de démarrage + 1 commune + 1 affûtée (placeholders Sprint 7).
            CreateOrUpdate("weapon_baton_bois", "Bâton de Bois", EquipmentSlot.Weapon,
                Voie.None, Rarity.Commun,
                statsBonusForce: 3, statsBonusCrit: 0f,
                description: "Le premier outil. Le plus humble.",
                useBodyFrames: false);

            CreateOrUpdate("weapon_epee_courte", "Épée Courte", EquipmentSlot.Weapon,
                Voie.None, Rarity.Affute,
                statsBonusForce: 18, statsBonusCrit: 0.02f,
                description: "Le tranchant suffit.",
                useBodyFrames: false);

            CreateOrUpdate("weapon_katana_samurai", "Katana Samouraï", EquipmentSlot.Weapon,
                Voie.Samurai, Rarity.Legendaire,
                statsBonusForce: 80, statsBonusCrit: 0.06f,
                description: "Forgée pli après pli, mille fois.",
                useBodyFrames: false);

            CreateOrUpdate("weapon_hache_viking", "Hache Viking", EquipmentSlot.Weapon,
                Voie.Viking, Rarity.Legendaire,
                statsBonusForce: 90, statsBonusCrit: 0.04f,
                description: "Le bois retient le sang des batailles.",
                useBodyFrames: false);

            CreateOrUpdate("weapon_jian_wuxia", "Jian du Voyageur", EquipmentSlot.Weapon,
                Voie.Wuxia, Rarity.Legendaire,
                statsBonusForce: 75, statsBonusCrit: 0.08f,
                description: "L'épée droite des arts céleste.",
                useBodyFrames: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Sprint 7 SpriteLayerSets generated/updated in {LayerSetsFolder} — 9 SOs ready (1 body + 3 armors + 5 weapons).");
        }

        private static void CreateOrUpdate(string id, string displayName, EquipmentSlot slot,
            Voie voie, Rarity rarity, double statsBonusForce, float statsBonusCrit,
            string description, bool useBodyFrames)
        {
            var path = $"{LayerSetsFolder}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SpriteLayerSet>(path);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SpriteLayerSet>();
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_slotType").enumValueIndex = (int)slot;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_voie").enumValueIndex = (int)voie;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_statsBonusForce").doubleValue = statsBonusForce;
            so.FindProperty("_statsBonusCrit").floatValue = statsBonusCrit;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_frameDuration").floatValue = EquipmentConstants.DefaultFrameDuration;

            if (useBodyFrames)
            {
                // Body slot: wire to the rvros adventurer slices so the character is visible at boot.
                AssignSprites(so, "_spriteIdle", LoadFrames("Idle", "adventurer-idle"));
                AssignSprites(so, "_spriteAttack1", LoadFrames("Attack1", "adventurer-attack1"));
                AssignSprites(so, "_spriteAttack2", LoadFrames("Attack2", "adventurer-attack2"));
                AssignSprites(so, "_spriteAttack3", LoadFrames("Attack3", "adventurer-attack3"));
                AssignSprites(so, "_spriteHurt", LoadFrames("Hurt", "adventurer-hurt"));
            }
            // Sprint 7: armor/weapon slots ship with empty Sprite[] (renderer falls back to null gracefully).
            // Sprint 8+ will wire per-set art when the layered overlay pipeline is ready.

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} {id} (slot={slot}, voie={voie}, rarity={rarity}, +F={statsBonusForce}, +crit={statsBonusCrit:P0})");
        }

        private static Sprite[] LoadFrames(string subFolder, string prefix)
        {
            var dir = $"{AdventurerArtFolder}/{subFolder}";
            if (!AssetDatabase.IsValidFolder(dir)) return System.Array.Empty<Sprite>();
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { dir });
            var list = new System.Collections.Generic.List<Sprite>();
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null && s.name.StartsWith(prefix)) list.Add(s);
            }
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        private static void AssignSprites(SerializedObject so, string propertyName, Sprite[] sprites)
        {
            var prop = so.FindProperty(propertyName);
            prop.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
