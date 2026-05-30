using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for the 8 Sprint 7 VoieData ScriptableObjects.
    /// Identity colors + intro citations drawn from 03_CULTURES.md.
    /// Idempotent — re-running updates values in place.
    ///
    /// Menu: <b>Saga > Sprint 7 > Generate Voie Assets</b>
    /// </summary>
    public static class VoieAssetsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string VoiesFolder = "Assets/_Project/Resources/Voies";

        [MenuItem("Saga/Sprint 7/Generate Voie Assets")]
        public static void GenerateVoieAssets()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(VoiesFolder);

            CreateOrUpdate("Voie_Samurai", Voie.Samurai, "Samouraï",
                main: new Color(0.98f, 0.78f, 0.46f, 1f),
                accent: new Color(0.95f, 0.18f, 0.13f, 1f),
                intro: "Le vent ne se laisse pas saisir, mais on peut apprendre à danser avec.",
                desc: "Voie de l'élégance et de l'honneur. Lames courbes, kimonos, méditation.",
                bonus: "Bonus Samouraï : +5% combo multiplier sur chaque palier.");

            CreateOrUpdate("Voie_Viking", Voie.Viking, "Viking",
                main: new Color(0.49f, 0.65f, 0.79f, 1f),
                accent: new Color(0.85f, 0.85f, 0.90f, 1f),
                intro: "Les corbeaux n'oublient pas. Ils chantent pour ceux qui tombent debout.",
                desc: "Voie de la fureur du Nord. Haches, peaux, runes, Valhalla.",
                bonus: "Bonus Viking : +10% Force de base.");

            CreateOrUpdate("Voie_Wuxia", Voie.Wuxia, "Wuxia",
                main: new Color(0.62f, 0.75f, 0.66f, 1f),
                accent: new Color(0.95f, 0.92f, 0.78f, 1f),
                intro: "Le ciel est vaste. Marche.",
                desc: "Voie céleste. Jian, vols, arts intérieurs, sagesse.",
                bonus: "Bonus Wuxia : +50% bonus de crit équipement.");

            CreateOrUpdate("Voie_Spartiate", Voie.Spartiate, "Spartiate",
                main: new Color(0.79f, 0.47f, 0.29f, 1f),
                accent: new Color(0.95f, 0.18f, 0.13f, 1f),
                intro: "Reviens avec ton bouclier, ou dessus.",
                desc: "Voie du marbre brisé. Lances, phalanges, sang.",
                bonus: "Bonus Spartiate : +15% Force vs Capitaines & Maîtres.");

            CreateOrUpdate("Voie_Mongol", Voie.Mongol, "Mongol",
                main: new Color(0.72f, 0.61f, 0.42f, 1f),
                accent: new Color(0.92f, 0.85f, 0.62f, 1f),
                intro: "Les steppes connaissent ton nom avant que tu n'arrives.",
                desc: "Voie des cavaliers du levant. Arcs, chevaux, horizons.",
                bonus: "Bonus Mongol : +20% Force/seconde passive.");

            CreateOrUpdate("Voie_Saladin", Voie.Saladin, "Saladin",
                main: new Color(0.91f, 0.72f, 0.36f, 1f),
                accent: new Color(0.45f, 0.30f, 0.20f, 1f),
                intro: "La miséricorde est plus tranchante que la lame.",
                desc: "Voie de la lumière du désert. Cimeterres, sagesse, lumière.",
                bonus: "Bonus Saladin : régénération de Souffle accélérée.");

            CreateOrUpdate("Voie_Aztec", Voie.Aztec, "Aztèque",
                main: new Color(0.75f, 0.23f, 0.17f, 1f),
                accent: new Color(0.98f, 0.85f, 0.30f, 1f),
                intro: "Le soleil exige son tribut.",
                desc: "Voie du cœur brûlant. Macuahuitl, soleil, sacrifice.",
                bonus: "Bonus Aztèque : +25% Échos au prestige.");

            CreateOrUpdate("Voie_Gaulois", Voie.Gaulois, "Gaulois",
                main: new Color(0.36f, 0.54f, 0.31f, 1f),
                accent: new Color(0.65f, 0.45f, 0.25f, 1f),
                intro: "Nous sommes peu. Nous tombons debout.",
                desc: "Voie du dernier rempart. Épées longues, forêts, druides.",
                bonus: "Bonus Gaulois : +30% Élan généré par tap.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Sprint 7 Voie assets generated/updated in {VoiesFolder} — 8 SOs ready.");
        }

        private static void CreateOrUpdate(string fileName, Voie voieEnum, string displayName,
            Color main, Color accent, string intro, string desc, string bonus)
        {
            var path = $"{VoiesFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<VoieData>(path);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<VoieData>();
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_voieEnum").enumValueIndex = (int)voieEnum;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_mainColor").colorValue = main;
            so.FindProperty("_accentColor").colorValue = accent;
            so.FindProperty("_introCitation").stringValue = intro;
            so.FindProperty("_description").stringValue = desc;
            so.FindProperty("_bonusDescription").stringValue = bonus;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} {fileName} ({voieEnum})");
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
