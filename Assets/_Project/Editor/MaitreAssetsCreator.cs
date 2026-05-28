using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for the 8 Sprint 6 Maître ScriptableObjects (one per voie, all from real mythology).
    /// Idempotent — re-running updates values in place.
    ///
    /// Menu: <b>Saga > Sprint 6 > Generate Maitre Assets</b>
    /// </summary>
    public static class MaitreAssetsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string MaitresFolder = "Assets/_Project/Resources/Maitres";

        [MenuItem("Saga/Sprint 6/Generate Maitre Assets")]
        public static void GenerateMaitreAssets()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(MaitresFolder);

            // Per-voie aura tints (phase 0=full, 1=light wound, 2=heavy, 3=enrage rouge intense).
            CreateOrUpdate("Yoshitsune_l_Inatteignable", "yoshitsune_l_inatteignable", "Yoshitsune l'Inatteignable",
                Voie.Samurai, 8000, 4000, 180f,
                intro: "Je suis Yoshitsune. Le vent m'a appris à danser sur les lames.",
                victory: "Le vent ne se laisse pas saisir. Tu as compris.",
                defeat: "Le vent ne se laisse pas saisir. Repose-toi.",
                reliqueName: "Lame de Yoshitsune",
                reliqueDesc: "Tranchante comme la brise de l'aube.",
                reliqueBonus: 500,
                arenaColor: new Color(0.98f, 0.78f, 0.46f, 1f),
                phaseTints: SamuraiPhaseColors());

            CreateOrUpdate("Ragnar_aux_Yeux_d_Acier", "ragnar_aux_yeux_d_acier", "Ragnar aux Yeux d'Acier",
                Voie.Viking, 9500, 4500, 180f,
                intro: "Les corbeaux m'ont annoncé. Te voilà donc.",
                victory: "Tu auras ta place au Valhalla.",
                defeat: "Les corbeaux n'oublient pas. Ils chanteront pour toi.",
                reliqueName: "Hache de Ragnar",
                reliqueDesc: "Forgée pour les longues nuits du Nord.",
                reliqueBonus: 600,
                arenaColor: new Color(0.49f, 0.65f, 0.79f, 1f),
                phaseTints: VikingPhaseColors());

            CreateOrUpdate("Sun_le_Voyageur_Celeste", "sun_le_voyageur_celeste", "Sun le Voyageur Céleste",
                Voie.Wuxia, 7500, 3800, 180f,
                intro: "Le ciel est vaste. Marchons un peu.",
                victory: "Le ciel est vaste. Tu as bien marché.",
                defeat: "Tu apprends. Reviens quand tu seras prêt.",
                reliqueName: "Bâton du Voyageur",
                reliqueDesc: "Il s'étire au-delà des nuages.",
                reliqueBonus: 550,
                arenaColor: new Color(0.62f, 0.75f, 0.66f, 1f),
                phaseTints: WuxiaPhaseColors());

            CreateOrUpdate("Leonidas_du_Marbre_Brise", "leonidas_du_marbre_brise", "Léonidas du Marbre Brisé",
                Voie.Spartiate, 10500, 5000, 180f,
                intro: "Reviens avec ton bouclier, ou dessus.",
                victory: "Tu mérites la table d'honneur.",
                defeat: "Le marbre se souvient de toi.",
                reliqueName: "Lance de Léonidas",
                reliqueDesc: "Brisée 300 fois, jamais cédée.",
                reliqueBonus: 700,
                arenaColor: new Color(0.79f, 0.47f, 0.29f, 1f),
                phaseTints: SpartiatePhaseColors());

            CreateOrUpdate("Subutai_le_Vent_du_Levant", "subutai_le_vent_du_levant", "Subutaï le Vent du Levant",
                Voie.Mongol, 8500, 4200, 180f,
                intro: "Les steppes connaissent ton nom.",
                victory: "Chevauche loin, et reviens.",
                defeat: "Les steppes se souviennent. Toujours.",
                reliqueName: "Arc de Subutaï",
                reliqueDesc: "Tire plus loin que l'horizon.",
                reliqueBonus: 580,
                arenaColor: new Color(0.72f, 0.61f, 0.42f, 1f),
                phaseTints: MongolPhaseColors());

            CreateOrUpdate("Salah_ad-Din_du_Desert", "salah_ad_din_du_desert", "Salah ad-Din du Désert",
                Voie.Saladin, 9000, 4400, 180f,
                intro: "La miséricorde est plus tranchante que la lame.",
                victory: "Tu as appris la lumière. Va.",
                defeat: "La lumière brûle aussi. Reviens éclairé.",
                reliqueName: "Cimeterre de Saladin",
                reliqueDesc: "Forgée par les sables du désert.",
                reliqueBonus: 620,
                arenaColor: new Color(0.91f, 0.72f, 0.36f, 1f),
                phaseTints: SaladinPhaseColors());

            CreateOrUpdate("Ahuitzotl_le_Coeur_Brulant", "ahuitzotl_le_coeur_brulant", "Ahuitzotl le Cœur Brûlant",
                Voie.Aztec, 11000, 5200, 180f,
                intro: "Le soleil exige son tribut.",
                victory: "Le soleil t'a accepté.",
                defeat: "Le soleil se nourrit. Tu nourriras.",
                reliqueName: "Macuahuitl d'Ahuitzotl",
                reliqueDesc: "Tranchant comme le silex sacré.",
                reliqueBonus: 720,
                arenaColor: new Color(0.75f, 0.23f, 0.17f, 1f),
                phaseTints: AztecPhaseColors());

            CreateOrUpdate("Vercingetorix_le_Dernier", "vercingetorix_le_dernier", "Vercingétorix le Dernier",
                Voie.Gaulois, 9500, 4600, 180f,
                intro: "Nous sommes peu. Nous tombons debout.",
                victory: "Tu nous as honorés. Reviens vers le couchant.",
                defeat: "Vae victis. Mais nous sommes debout.",
                reliqueName: "Épée de Vercingétorix",
                reliqueDesc: "Levée une dernière fois, à jamais.",
                reliqueBonus: 650,
                arenaColor: new Color(0.36f, 0.54f, 0.31f, 1f),
                phaseTints: GauloisPhaseColors());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Sprint 6 Maître assets generated/updated in {MaitresFolder} — 8 SOs ready.");
        }

        // Per-voie tint scales (0=normal → 3=enrage rouge).
        private static Color[] SamuraiPhaseColors() => new[]
        {
            new Color(0.98f, 0.78f, 0.46f, 1f),
            new Color(0.99f, 0.65f, 0.30f, 1f),
            new Color(1.00f, 0.45f, 0.20f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };
        private static Color[] VikingPhaseColors() => new[]
        {
            new Color(0.49f, 0.65f, 0.79f, 1f),
            new Color(0.40f, 0.55f, 0.78f, 1f),
            new Color(0.35f, 0.45f, 0.70f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };
        private static Color[] WuxiaPhaseColors() => new[]
        {
            new Color(0.62f, 0.75f, 0.66f, 1f),
            new Color(0.55f, 0.70f, 0.60f, 1f),
            new Color(0.50f, 0.65f, 0.55f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };
        private static Color[] SpartiatePhaseColors() => new[]
        {
            new Color(0.79f, 0.47f, 0.29f, 1f),
            new Color(0.85f, 0.40f, 0.25f, 1f),
            new Color(0.90f, 0.35f, 0.20f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };
        private static Color[] MongolPhaseColors() => new[]
        {
            new Color(0.72f, 0.61f, 0.42f, 1f),
            new Color(0.65f, 0.55f, 0.38f, 1f),
            new Color(0.58f, 0.48f, 0.32f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };
        private static Color[] SaladinPhaseColors() => new[]
        {
            new Color(0.91f, 0.72f, 0.36f, 1f),
            new Color(0.93f, 0.62f, 0.28f, 1f),
            new Color(0.96f, 0.52f, 0.20f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };
        private static Color[] AztecPhaseColors() => new[]
        {
            new Color(0.75f, 0.23f, 0.17f, 1f),
            new Color(0.80f, 0.20f, 0.14f, 1f),
            new Color(0.85f, 0.15f, 0.10f, 1f),
            new Color(0.95f, 0.10f, 0.06f, 1f),
        };
        private static Color[] GauloisPhaseColors() => new[]
        {
            new Color(0.36f, 0.54f, 0.31f, 1f),
            new Color(0.32f, 0.48f, 0.27f, 1f),
            new Color(0.28f, 0.42f, 0.23f, 1f),
            new Color(0.95f, 0.18f, 0.13f, 1f),
        };

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
            double hp, double rewardForce, float chrono,
            string intro, string victory, string defeat,
            string reliqueName, string reliqueDesc, double reliqueBonus,
            Color arenaColor, Color[] phaseTints)
        {
            var path = $"{MaitresFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<MaitreData>(path);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<MaitreData>();
                AssetDatabase.CreateAsset(asset, path);
                created = true;
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_voie").enumValueIndex = (int)voie;
            so.FindProperty("_hp").doubleValue = hp;
            so.FindProperty("_rewardForce").doubleValue = rewardForce;
            so.FindProperty("_chronoSeconds").floatValue = chrono;
            so.FindProperty("_introCitation").stringValue = intro;
            so.FindProperty("_victoryCitation").stringValue = victory;
            so.FindProperty("_defeatCitation").stringValue = defeat;
            so.FindProperty("_reliqueUniqueName").stringValue = reliqueName;
            so.FindProperty("_reliqueUniqueDescription").stringValue = reliqueDesc;
            so.FindProperty("_reliqueStatsBonus").doubleValue = reliqueBonus;
            so.FindProperty("_arenaBackgroundColor").colorValue = arenaColor;
            so.FindProperty("_drumHitPitch").floatValue = 0.9f;

            var thresholds = so.FindProperty("_phaseThresholds");
            thresholds.arraySize = 3;
            thresholds.GetArrayElementAtIndex(0).floatValue = 0.75f;
            thresholds.GetArrayElementAtIndex(1).floatValue = 0.5f;
            thresholds.GetArrayElementAtIndex(2).floatValue = 0.25f;

            var colors = so.FindProperty("_phaseColors");
            colors.arraySize = phaseTints.Length;
            for (var i = 0; i < phaseTints.Length; i++) colors.GetArrayElementAtIndex(i).colorValue = phaseTints[i];

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} {fileName} (voie={voie}, hp={hp}, reward={rewardForce}F, chrono={chrono}s)");
        }
    }
}
