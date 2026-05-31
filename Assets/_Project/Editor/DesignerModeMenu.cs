using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 8 — Designer-First workflow toggle (Ajwad place ses assets custom directement dans
    /// Main.unity sans que MainSceneBootstrap procédural les écrase).
    ///
    /// Quand ON : MainSceneBootstrap.Awake retourne tôt → la scene authored manuellement
    /// est préservée au Play. Quand OFF : bootstrap procédural normal (RhosGFX builders, etc).
    ///
    /// État stocké dans EditorPrefs (per-Editor, pas commité). Build shippé ignore le flag.
    ///
    /// Menu : <b>Saga > Designer Mode > Enabled</b> (checkmark visible quand ON).
    /// </summary>
    public static class DesignerModeMenu
    {
        private const string PrefKey = "Saga.DesignerMode";
        private const string MenuPath = "Saga/Designer Mode/Enabled";

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            var current = EditorPrefs.GetBool(PrefKey, false);
            var next = !current;
            EditorPrefs.SetBool(PrefKey, next);
            Debug.Log($"[Saga] Designer Mode: {(next ? "ON" : "OFF")} " +
                      (next
                          ? "→ MainSceneBootstrap.Awake() will SKIP procedural scene generation. " +
                            "Authored Main.unity content preserved at Play."
                          : "→ MainSceneBootstrap.Awake() will run normally (procedural build)."));
        }

        [MenuItem(MenuPath, validate = true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, false));
            return true;
        }

        [MenuItem("Saga/Designer Mode/Status (Print to Console)")]
        private static void PrintStatus()
        {
            var state = EditorPrefs.GetBool(PrefKey, false);
            Debug.Log($"[Saga] Designer Mode is currently: {(state ? "ON (procedural bootstrap SKIPPED)" : "OFF (procedural bootstrap ENABLED)")}");
        }
    }
}
