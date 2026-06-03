using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 9 Phase 1 — Designer-First scene introspection.
    ///
    /// Scanned au boot par MainSceneBootstrap : si Ajwad a authored sa layout dans Main.unity
    /// avec les noms conventionnels (Force, Echos, Settings, Stage_Chip, Card_Strike, ...),
    /// les builders Sprint 9+ WireFromAuthored(ctx) attache components à ces GO existants
    /// au lieu de re-créer une hiérarchie procédurale.
    ///
    /// Convention de naming (validée coordinateur Q1) :
    ///   - Top HUD   : Force, Echos, Settings
    ///   - Stage     : Stage_Chip, Boss_Bar, Boss_Skull
    ///   - Upgrades  : Card_Strike, Card_Focus, Card_Power
    ///   - Skills    : Vague, Souffle
    ///   - Bottom Nav: Tab_Shop, Tab_Hero, Tab_Dojo, Tab_Artifacts, Tab_Legend
    ///
    /// Sous-éléments attendus (conventions Q5) : Label (TMP), Icon (Image), Glow (Image)
    /// trouvés via <c>transform.Find</c>. Lookup gracieux : si manquant, builder log + auto-créer
    /// les sous-éléments runtime au besoin (V1 — Ajwad pose seulement les containers).
    ///
    /// Shadow components (O4 critique) : présence détectée + loggée. Builders Sprint 9 NE
    /// supprimeront PAS les Shadow customs d'Ajwad (skip tout procedural shadow sur GO authored).
    /// </summary>
    public class SceneRegistry
    {
        // ----- Top HUD -----
        public GameObject Force;
        public GameObject Echos;
        public GameObject Settings;

        // ----- Stage Bar -----
        public GameObject StageChip;
        public GameObject BossBar;
        public GameObject BossSkull;

        // ----- Upgrades Cards -----
        public GameObject CardStrike;
        public GameObject CardFocus;
        public GameObject CardPower;

        // ----- Skills -----
        public GameObject VagueButton;
        public GameObject SouffleButton;

        // ----- Bottom Nav -----
        public GameObject TabShop;
        public GameObject TabHero;
        public GameObject TabDojo;
        public GameObject TabArtifacts;
        public GameObject TabLegend;

        /// <summary>
        /// True dès qu'on trouve au moins la pill Force (sentinel — preuve qu'Ajwad a authored).
        /// MainSceneBootstrap utilise ce flag pour basculer en mode Designer-First.
        /// </summary>
        public bool IsDesignerLayoutDetected => Force != null;

        /// <summary>
        /// Scan la scène active pour les GameObjects nommés selon la convention. Les références
        /// non trouvées restent null (les builders concernés tomberont en fallback procédural).
        /// </summary>
        public void TryAutoPopulate()
        {
            // Sprint 10 Phase 1a — lookup tolérant : ignore espaces trailing + case-insensitive.
            // V2 assets Ajwad ont des typos ("Tab_shop" lowercase, "Tab_Legend " trailing space) →
            // GameObject.Find strict ratait ces GO. Tolérant trim+lower match les variations.
            var all = CollectAllSceneGameObjects();

            Force = FindTolerant(all, "Force");
            Echos = FindTolerant(all, "Echos");
            Settings = FindTolerant(all, "Settings");

            StageChip = FindTolerant(all, "Stage_Chip");
            BossBar = FindTolerant(all, "Boss_Bar");
            BossSkull = FindTolerant(all, "Boss_Skull");

            CardStrike = FindTolerant(all, "Card_Strike");
            CardFocus = FindTolerant(all, "Card_Focus");
            CardPower = FindTolerant(all, "Card_Power");

            VagueButton = FindTolerant(all, "Vague");
            SouffleButton = FindTolerant(all, "Souffle");

            TabShop = FindTolerant(all, "Tab_Shop");
            TabHero = FindTolerant(all, "Tab_Hero");
            TabDojo = FindTolerant(all, "Tab_Dojo");
            TabArtifacts = FindTolerant(all, "Tab_Artifacts");
            TabLegend = FindTolerant(all, "Tab_Legend");
        }

        /// <summary>Collect tous les GameObjects (actifs + inactifs) de la scène active.</summary>
        private static List<GameObject> CollectAllSceneGameObjects()
        {
            var list = new List<GameObject>();
            var active = SceneManager.GetActiveScene();
            var roots = active.GetRootGameObjects();
            foreach (var root in roots)
            {
                CollectRecursive(root.transform, list);
            }
            return list;
        }

        private static void CollectRecursive(Transform t, List<GameObject> list)
        {
            list.Add(t.gameObject);
            for (var i = 0; i < t.childCount; i++)
            {
                CollectRecursive(t.GetChild(i), list);
            }
        }

        /// <summary>
        /// Lookup tolérant : trim + case-insensitive. Si plusieurs GO matchent, return le premier
        /// (suit l'ordre de DFS scene roots — généralement le plus haut dans la hiérarchie).
        /// </summary>
        private static GameObject FindTolerant(List<GameObject> all, string target)
        {
            var trimmed = target.Trim();
            return all.FirstOrDefault(go => go != null
                && go.name.Trim().Equals(trimmed, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Helper public pour les builders qui font Transform.Find sur sub-elements
        /// (BG / Icon / Label) — tolérance même règle (trim + case-insensitive).
        /// </summary>
        public static Transform FindChildTolerant(Transform parent, string name)
        {
            if (parent == null) return null;
            var trimmed = name.Trim();
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name.Trim().Equals(trimmed, System.StringComparison.OrdinalIgnoreCase))
                    return child;
            }
            return null;
        }

        /// <summary>
        /// Log un récap des GO trouvés + les Shadow components customs d'Ajwad (O4 critique :
        /// preserve ses ombres → on les liste pour traçabilité, on n'y touche jamais).
        /// </summary>
        public void LogDetectedLayout()
        {
            var detected = new List<string>();
            var missing = new List<string>();
            CheckOne("Force", Force, detected, missing);
            CheckOne("Echos", Echos, detected, missing);
            CheckOne("Settings", Settings, detected, missing);
            CheckOne("Stage_Chip", StageChip, detected, missing);
            CheckOne("Boss_Bar", BossBar, detected, missing);
            CheckOne("Boss_Skull", BossSkull, detected, missing);
            CheckOne("Card_Strike", CardStrike, detected, missing);
            CheckOne("Card_Focus", CardFocus, detected, missing);
            CheckOne("Card_Power", CardPower, detected, missing);
            CheckOne("Vague", VagueButton, detected, missing);
            CheckOne("Souffle", SouffleButton, detected, missing);
            CheckOne("Tab_Shop", TabShop, detected, missing);
            CheckOne("Tab_Hero", TabHero, detected, missing);
            CheckOne("Tab_Dojo", TabDojo, detected, missing);
            CheckOne("Tab_Artifacts", TabArtifacts, detected, missing);
            CheckOne("Tab_Legend", TabLegend, detected, missing);

            if (!IsDesignerLayoutDetected)
            {
                Debug.Log("[SceneRegistry] No authored layout detected (Force GO missing). Falling back to procedural bootstrap.");
                return;
            }

            Debug.Log($"[SceneRegistry] Designer layout detected — {detected.Count} GO trouvés, {missing.Count} manquants.\n" +
                      $"  Found   : {string.Join(", ", detected)}\n" +
                      $"  Missing : {(missing.Count == 0 ? "(aucun)" : string.Join(", ", missing))}");

            LogCustomShadows();
        }

        private static void CheckOne(string name, GameObject go, List<string> detected, List<string> missing)
        {
            if (go != null) detected.Add(name); else missing.Add(name);
        }

        /// <summary>
        /// O4 critique — détecte les Shadow / Outline components customs ajoutés par Ajwad dans
        /// l'Editor sur les GO authored. Les liste pour traçabilité (les builders Sprint 9 NE
        /// touchent JAMAIS ces components, ils sont préservés tels quels).
        /// </summary>
        private void LogCustomShadows()
        {
            ReportShadowsOn("Force", Force);
            ReportShadowsOn("Echos", Echos);
            ReportShadowsOn("Settings", Settings);
            ReportShadowsOn("Stage_Chip", StageChip);
            ReportShadowsOn("Boss_Bar", BossBar);
            ReportShadowsOn("Boss_Skull", BossSkull);
            ReportShadowsOn("Card_Strike", CardStrike);
            ReportShadowsOn("Card_Focus", CardFocus);
            ReportShadowsOn("Card_Power", CardPower);
            ReportShadowsOn("Vague", VagueButton);
            ReportShadowsOn("Souffle", SouffleButton);
            ReportShadowsOn("Tab_Shop", TabShop);
            ReportShadowsOn("Tab_Hero", TabHero);
            ReportShadowsOn("Tab_Dojo", TabDojo);
            ReportShadowsOn("Tab_Artifacts", TabArtifacts);
            ReportShadowsOn("Tab_Legend", TabLegend);
        }

        private static void ReportShadowsOn(string name, GameObject go)
        {
            if (go == null) return;
            var shadows = go.GetComponentsInChildren<Shadow>(includeInactive: true);
            if (shadows == null || shadows.Length == 0) return;
            foreach (var s in shadows)
            {
                var path = s.gameObject == go ? "(root)" : s.gameObject.name;
                Debug.Log($"[SceneRegistry] {name} has custom Shadow on {path} (effectDistance: {s.effectDistance}, color alpha: {s.effectColor.a:F2}) — preserved as-is.");
            }
        }
    }
}
