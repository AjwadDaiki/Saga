using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for Sprint 7's 8 Maître relique SpriteLayerSets — one weapon-slot
    /// SO per Maître. Created alongside (not inside) the regular weapon catalog so they
    /// can drop from prestige with stats already tuned to mid-late game (statsBonusForce
    /// matches each MaitreData._reliqueStatsBonus).
    ///
    /// Also wires <c>_reliqueSpriteLayerSetId</c> on each MaitreData so
    /// CombatProcessor (Phase H) can grant the relique on Maître defeat.
    ///
    /// Idempotent. Run AFTER <see cref="MaitreAssetsCreator"/> and
    /// <see cref="SpriteLayerSetsCreator"/>.
    ///
    /// Menu: <b>Saga > Sprint 7 > Generate Maitre Relique Layer Sets</b>
    /// </summary>
    public static class MaitreReliqueLayerSetsCreator
    {
        private const string LayerSetsFolder = "Assets/_Project/Resources/SpriteLayerSets";
        private const string MaitresFolder = "Assets/_Project/Resources/Maitres";

        [MenuItem("Saga/Sprint 7/Generate Maitre Relique Layer Sets")]
        public static void GenerateReliqueLayerSets()
        {
            AssetDatabase.Refresh();
            if (!AssetDatabase.IsValidFolder(MaitresFolder))
            {
                Debug.LogError($"[Saga] {MaitresFolder} missing — run Generate Maitre Assets first.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(LayerSetsFolder))
            {
                Debug.LogError($"[Saga] {LayerSetsFolder} missing — run Generate Sprite Layer Sets first.");
                return;
            }

            // Maître file name → (relique layer set id, display name, voie, force bonus, crit bonus, description)
            CreateReliqueLayerSet("Yoshitsune_l_Inatteignable",
                "relique_lame_yoshitsune", "Lame de Yoshitsune", Voie.Samurai,
                500, 0.08f, "Tranchante comme la brise de l'aube.");

            CreateReliqueLayerSet("Ragnar_aux_Yeux_d_Acier",
                "relique_hache_ragnar", "Hache de Ragnar", Voie.Viking,
                600, 0.05f, "Forgée pour les longues nuits du Nord.");

            CreateReliqueLayerSet("Sun_le_Voyageur_Celeste",
                "relique_baton_sun", "Bâton du Voyageur", Voie.Wuxia,
                550, 0.10f, "Il s'étire au-delà des nuages.");

            CreateReliqueLayerSet("Leonidas_du_Marbre_Brise",
                "relique_lance_leonidas", "Lance de Léonidas", Voie.Spartiate,
                700, 0.06f, "Brisée 300 fois, jamais cédée.");

            CreateReliqueLayerSet("Subutai_le_Vent_du_Levant",
                "relique_arc_subutai", "Arc de Subutaï", Voie.Mongol,
                580, 0.07f, "Tire plus loin que l'horizon.");

            CreateReliqueLayerSet("Salah_ad-Din_du_Desert",
                "relique_cimeterre_saladin", "Cimeterre de Saladin", Voie.Saladin,
                620, 0.07f, "Forgée par les sables du désert.");

            CreateReliqueLayerSet("Ahuitzotl_le_Coeur_Brulant",
                "relique_macuahuitl_ahuitzotl", "Macuahuitl d'Ahuitzotl", Voie.Aztec,
                720, 0.05f, "Tranchant comme le silex sacré.");

            CreateReliqueLayerSet("Vercingetorix_le_Dernier",
                "relique_epee_vercingetorix", "Épée de Vercingétorix", Voie.Gaulois,
                650, 0.06f, "Levée une dernière fois, à jamais.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Saga] Sprint 7 Maître relique layer sets generated + wired into MaitreData (8 SOs).");
        }

        private static void CreateReliqueLayerSet(string maitreFileName,
            string layerSetId, string displayName, Voie voie, double force, float crit, string description)
        {
            // 1) Create / update the SpriteLayerSet.
            var layerPath = $"{LayerSetsFolder}/{layerSetId}.asset";
            var layer = AssetDatabase.LoadAssetAtPath<SpriteLayerSet>(layerPath);
            var createdLayer = false;
            if (layer == null)
            {
                layer = ScriptableObject.CreateInstance<SpriteLayerSet>();
                AssetDatabase.CreateAsset(layer, layerPath);
                createdLayer = true;
            }

            var sl = new SerializedObject(layer);
            sl.FindProperty("_id").stringValue = layerSetId;
            sl.FindProperty("_slotType").enumValueIndex = (int)EquipmentSlot.Weapon;
            sl.FindProperty("_displayName").stringValue = displayName;
            sl.FindProperty("_voie").enumValueIndex = (int)voie;
            sl.FindProperty("_rarity").enumValueIndex = (int)Rarity.Mythique;
            sl.FindProperty("_statsBonusForce").doubleValue = force;
            sl.FindProperty("_statsBonusCrit").floatValue = crit;
            sl.FindProperty("_description").stringValue = description;
            sl.FindProperty("_frameDuration").floatValue = EquipmentConstants.DefaultFrameDuration;
            sl.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(layer);

            // 2) Wire the id onto the matching MaitreData.
            var maitrePath = $"{MaitresFolder}/{maitreFileName}.asset";
            var maitre = AssetDatabase.LoadAssetAtPath<MaitreData>(maitrePath);
            if (maitre == null)
            {
                Debug.LogWarning($"[Saga] Maitre asset missing at {maitrePath} — relique created but not wired.");
                return;
            }
            var sm = new SerializedObject(maitre);
            sm.FindProperty("_reliqueSpriteLayerSetId").stringValue = layerSetId;
            sm.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(maitre);

            Debug.Log($"[Saga] {(createdLayer ? "Created" : "Updated")} {layerSetId} ({voie}, +F={force}, +crit={crit:P0}) → wired into {maitreFileName}");
        }
    }
}
