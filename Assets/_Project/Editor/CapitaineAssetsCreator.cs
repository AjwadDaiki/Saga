using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for the 8 Sprint 5 Capitaine ScriptableObjects (one per voie).
    /// Idempotent — re-running updates values in place.
    ///
    /// Menu: <b>Saga > Sprint 5 > Generate Capitaine Assets</b>
    /// </summary>
    public static class CapitaineAssetsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string CapitainesFolder = "Assets/_Project/Resources/Capitaines";

        // Per-voie aura tints (phase 0=normal, 1=light wound, 2=heavy, 3=enrage rouge).
        private static readonly Color[] TintsSamurai   = { new Color(0.98f, 0.78f, 0.46f, 1f), new Color(0.99f, 0.65f, 0.30f, 1f), new Color(1.00f, 0.45f, 0.20f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsViking    = { new Color(0.49f, 0.65f, 0.79f, 1f), new Color(0.40f, 0.55f, 0.78f, 1f), new Color(0.35f, 0.45f, 0.70f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsWuxia     = { new Color(0.62f, 0.75f, 0.66f, 1f), new Color(0.55f, 0.70f, 0.60f, 1f), new Color(0.50f, 0.65f, 0.55f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsSpartiate = { new Color(0.79f, 0.47f, 0.29f, 1f), new Color(0.85f, 0.40f, 0.25f, 1f), new Color(0.90f, 0.35f, 0.20f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsMongol    = { new Color(0.58f, 0.66f, 0.61f, 1f), new Color(0.50f, 0.60f, 0.55f, 1f), new Color(0.45f, 0.55f, 0.50f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsSaladin   = { new Color(0.85f, 0.65f, 0.28f, 1f), new Color(0.88f, 0.60f, 0.25f, 1f), new Color(0.92f, 0.55f, 0.20f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsAztec     = { new Color(0.36f, 0.72f, 0.60f, 1f), new Color(0.45f, 0.65f, 0.50f, 1f), new Color(0.55f, 0.55f, 0.40f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };
        private static readonly Color[] TintsGaulois   = { new Color(0.65f, 0.48f, 0.25f, 1f), new Color(0.60f, 0.42f, 0.20f, 1f), new Color(0.55f, 0.36f, 0.18f, 1f), new Color(0.95f, 0.20f, 0.15f, 1f) };

        [MenuItem("Saga/Sprint 5/Generate Capitaine Assets")]
        public static void GenerateCapitaineAssets()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(CapitainesFolder);

            CreateOrUpdate("Hattori_du_Mont",       "hattori_du_mont",       "Hattori du Mont",       Voie.Samurai,   500, 250, 60f, TintsSamurai,
                intro: "Le sommet n'attend personne.",        death: "Le sommet… reconnaît.");
            CreateOrUpdate("Bjorn_aux_Tresses",     "bjorn_aux_tresses",     "Bjorn aux Tresses",     Voie.Viking,    600, 300, 70f, TintsViking,
                intro: "Les nœuds gardent les promesses.",    death: "Mes nœuds… tiennent à toi.");
            CreateOrUpdate("Lin_du_Bambou",         "lin_du_bambou",         "Lin du Bambou",         Voie.Wuxia,     450, 220, 65f, TintsWuxia,
                intro: "Plier ne signifie pas céder.",         death: "L'âme du bambou… te suit.");
            CreateOrUpdate("Pythagoras_Lame_Breve", "pythagoras_lame_breve", "Pythagoras Lame Brève", Voie.Spartiate, 700, 320, 75f, TintsSpartiate,
                intro: "Le glaive court trouve la gorge.",     death: "Le bouclier… te revient.");
            CreateOrUpdate("Berke_le_Cavalier",     "berke_le_cavalier",     "Berke le Cavalier",     Voie.Mongol,    550, 280, 60f, TintsMongol,
                intro: "La steppe t'a vu.",                    death: "La steppe… t'accompagne.");
            CreateOrUpdate("Nour_ad-Din",           "nour_ad_din",           "Nour ad-Din",           Voie.Saladin,   580, 300, 70f, TintsSaladin,
                intro: "La lumière brûle aussi.",              death: "La lumière… est tienne.");
            CreateOrUpdate("Cuauhtemoc_le_Jeune",   "cuauhtemoc_le_jeune",   "Cuauhtémoc le Jeune",   Voie.Aztec,     650, 330, 65f, TintsAztec,
                intro: "Le cœur bat encore.",                  death: "Mon cœur… bat pour toi.");
            CreateOrUpdate("Brennus_du_Cor",        "brennus_du_cor",        "Brennus du Cor",        Voie.Gaulois,   750, 350, 80f, TintsGaulois,
                intro: "Vae victis.",                          death: "Vincit qui se vincit.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Sprint 5 Capitaine assets generated/updated in {CapitainesFolder} — 8 SOs ready.");
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
            double hp, double reward, float chrono, Color[] phaseTints,
            string intro, string death)
        {
            var path = $"{CapitainesFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<CapitaineData>(path);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CapitaineData>();
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_voie").enumValueIndex = (int)voie;
            so.FindProperty("_hp").doubleValue = hp;
            so.FindProperty("_rewardForce").doubleValue = reward;
            so.FindProperty("_guaranteedLoot").boolValue = true;
            so.FindProperty("_chronoSeconds").floatValue = chrono;
            so.FindProperty("_introCitation").stringValue = intro;
            so.FindProperty("_deathCitation").stringValue = death;

            // Phase thresholds 75/50/25 (3 thresholds → 4 phases including enrage).
            var thresholds = so.FindProperty("_phaseThresholds");
            thresholds.arraySize = 3;
            thresholds.GetArrayElementAtIndex(0).floatValue = 0.75f;
            thresholds.GetArrayElementAtIndex(1).floatValue = 0.5f;
            thresholds.GetArrayElementAtIndex(2).floatValue = 0.25f;

            // Phase colors (one per phase index 0..3).
            var colors = so.FindProperty("_phaseColors");
            colors.arraySize = phaseTints.Length;
            for (var i = 0; i < phaseTints.Length; i++) colors.GetArrayElementAtIndex(i).colorValue = phaseTints[i];

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} {fileName} (voie={voie}, hp={hp}, reward={reward}F, chrono={chrono}s)");
        }
    }
}
