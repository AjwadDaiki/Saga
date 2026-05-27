using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for the 3 Sprint 2 upgrade ScriptableObjects.
    /// Idempotent: re-running updates values in place rather than duplicating assets.
    ///
    /// Menu: <b>Saga > Sprint 2 > Generate Upgrade Assets</b>
    ///
    /// Values per 08_ROADMAP.md Sprint 2:
    /// - Frappe       : +1 Force/tap base, cost 10, ×1.15/level
    /// - Disciple     : +1 Force/sec passive, cost 50, ×1.20/level
    /// - Méditation   : +5% combo cap bonus, cost 200, ×1.50/level
    /// </summary>
    public static class UpgradeAssetsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string UpgradesFolder = "Assets/_Project/Resources/Upgrades";

        [MenuItem("Saga/Sprint 2/Generate Upgrade Assets")]
        public static void GenerateUpgradeAssets()
        {
            EnsureFolder(ResourcesFolder);
            EnsureFolder(UpgradesFolder);

            CreateOrUpdate(
                fileName: "Upgrade_Frappe",
                id: "frappe",
                displayName: "Frappe",
                description: "+1 Force par tap, multiplié par le combo. Le fondement de tout maître.",
                costBase: 10,
                costMultiplier: 1.15f,
                effectType: UpgradeEffectType.ForcePerTap,
                effectValue: 1f);

            CreateOrUpdate(
                fileName: "Upgrade_Disciple",
                id: "disciple",
                displayName: "Disciple",
                description: "+1 Force par seconde, en continu. Tes premiers suiveurs s'entraînent même quand tu dors.",
                costBase: 50,
                costMultiplier: 1.20f,
                effectType: UpgradeEffectType.ForcePerSecond,
                effectValue: 1f);

            CreateOrUpdate(
                fileName: "Upgrade_Meditation",
                id: "meditation",
                displayName: "Méditation",
                description: "+5% au multiplicateur de combo. La maîtrise du souffle amplifie chaque frappe.",
                costBase: 200,
                costMultiplier: 1.50f,
                effectType: UpgradeEffectType.ComboMultiplierBonus,
                effectValue: 0.05f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Sprint 2 upgrade assets generated/updated in {UpgradesFolder} — 3 SOs ready.");
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

        private static void CreateOrUpdate(string fileName, string id, string displayName, string description,
            double costBase, float costMultiplier, UpgradeEffectType effectType, float effectValue)
        {
            var path = $"{UpgradesFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UpgradeData>();
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_upgradeId").stringValue = id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_descriptionFr").stringValue = description;
            so.FindProperty("_costBase").doubleValue = costBase;
            so.FindProperty("_costMultiplier").floatValue = costMultiplier;
            so.FindProperty("_effectType").enumValueIndex = (int)effectType;
            so.FindProperty("_effectValue").floatValue = effectValue;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} {fileName} (id={id}, cost={costBase}×{costMultiplier}^level, effect={effectType}×{effectValue})");
        }
    }
}
