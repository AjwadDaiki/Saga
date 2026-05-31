using Saga.Core;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 9 Phase 4 — Reset tutorial progress flag pour replay fresh sans nuker la save.
    ///
    /// Menu : <b>Saga > Sprint 8 > Reset Tutorial Progress</b>
    ///
    /// Effet : set <c>state.tutorialDone = false</c> + <c>tutorialStepIndex = 0</c> puis save.
    /// Au prochain Play, TutorialOverlayBuilder relance le flux des 6 steps.
    /// Le reste de la save (Force, taps, upgrades, prestige, etc.) reste intact.
    ///
    /// Requires Play mode actif (GameManager.Instance disponible). Si appelé hors Play,
    /// affiche un dialogue d'erreur.
    /// </summary>
    public static class ResetTutorialMenu
    {
        [MenuItem("Saga/Sprint 8/Reset Tutorial Progress")]
        public static void ResetTutorial()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Reset Tutorial",
                    "Lance Play d'abord — ce reset nécessite GameManager.Instance actif.\n\n" +
                    "Alternative : supprime manuellement save.json dans Application.persistentDataPath " +
                    "(ouvre Window > Analysis > Profiler > File > Open Save Folder).",
                    "OK");
                return;
            }
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
            {
                EditorUtility.DisplayDialog("Reset Tutorial",
                    "GameManager.Instance ou State est null — bootstrap pas encore complet.",
                    "OK");
                return;
            }

            gm.State.tutorialDone = false;
            gm.State.tutorialStepIndex = 0;
            gm.Save?.ForceSave(gm.State);
            Debug.Log("[Saga] Tutorial reset — au prochain Play (ou re-entry scene Main), le flux des 6 steps redémarre.");
            EditorUtility.DisplayDialog("Reset Tutorial",
                "Tutorial flag reset. Stop Play → Re-Play pour rejouer le tutorial fresh.\n\n" +
                "Force / taps / upgrades / prestige restent intacts.",
                "OK");
        }
    }
}
