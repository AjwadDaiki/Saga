using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for the 5 Sprint 4 adversaire ScriptableObjects.
    /// Idempotent — re-running updates values in place rather than duplicating assets.
    ///
    /// Menu: <b>Saga > Sprint 4 > Generate Adversaire Assets</b>
    ///
    /// Values per Sprint 4 brief:
    /// - Ronin Errant        : None, 100 HP, 30 Force, 15% loot, 30s
    /// - Spadassin Nordique  : Viking, 150 HP, 50 Force, 20% loot, 35s
    /// - Initié Wuxia        : Wuxia, 200 HP, 75 Force, 25% loot, 40s
    /// - Hoplite Lâche       : Spartiate, 250 HP, 100 Force, 30% loot, 45s
    /// - Pèlerin du Nord     : None, 80 HP (fast), 40 Force, 20% loot, 25s
    /// </summary>
    public static class AdversaireAssetsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string AdversairesFolder = "Assets/_Project/Resources/Adversaires";

        [MenuItem("Saga/Sprint 4/Generate Adversaire Assets")]
        public static void GenerateAdversaireAssets()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(AdversairesFolder);

            CreateOrUpdate("Ronin_Errant",       "ronin_errant",       "Ronin Errant",      Voie.None,      hp: 100, reward: 30,  loot: 0.15f, chrono: 30f);
            CreateOrUpdate("Spadassin_Nordique", "spadassin_nordique", "Spadassin Nordique", Voie.Viking,    hp: 150, reward: 50,  loot: 0.20f, chrono: 35f);
            CreateOrUpdate("Initie_Wuxia",       "initie_wuxia",       "Initié Wuxia",      Voie.Wuxia,     hp: 200, reward: 75,  loot: 0.25f, chrono: 40f);
            CreateOrUpdate("Hoplite_Lache",      "hoplite_lache",      "Hoplite Lâche",     Voie.Spartiate, hp: 250, reward: 100, loot: 0.30f, chrono: 45f);
            CreateOrUpdate("Pelerin_du_Nord",    "pelerin_du_nord",    "Pèlerin du Nord",   Voie.None,      hp: 80,  reward: 40,  loot: 0.20f, chrono: 25f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Sprint 4 adversaire assets generated/updated in {AdversairesFolder} — 5 SOs ready.");
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

        private static void CreateOrUpdate(string fileName, string id, string displayName, Voie voie,
            double hp, double reward, float loot, float chrono)
        {
            var path = $"{AdversairesFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<AdversaireData>(path);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AdversaireData>();
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_voie").enumValueIndex = (int)voie;
            so.FindProperty("_hp").doubleValue = hp;
            so.FindProperty("_rewardForce").doubleValue = reward;
            so.FindProperty("_lootChance").floatValue = loot;
            so.FindProperty("_chronoSeconds").floatValue = chrono;
            so.FindProperty("_spriteIdleName").stringValue = string.Empty; // Sprint 4 uses procedural sprites
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} {fileName} (id={id}, voie={voie}, hp={hp}, reward={reward}F, loot={loot:P0}, chrono={chrono}s)");
        }
    }
}
