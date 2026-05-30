using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 8 Phase A — One-shot generator for the 6 tutorial step ScriptableObjects.
    /// Idempotent — re-running resets to the canonical content below.
    ///
    /// Menu: <b>Saga > Sprint 8 > Generate Tutorial Steps</b>
    /// </summary>
    public static class TutorialStepsCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string TutorialFolder = "Assets/_Project/Resources/Tutorial";

        [MenuItem("Saga/Sprint 8/Generate Tutorial Steps")]
        public static void Generate()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(TutorialFolder);

            CreateOrUpdate("Step_01_tap_to_gain", s =>
            {
                s.id = "tap_to_gain";
                s.anchor = TutorialStep.AnchorTarget.CombatZone;
                s.trigger = TutorialStep.TriggerCondition.TapsReached;
                s.triggerThreshold = 5; // first 5 taps acknowledge the gesture
                s.messageFr = "Tape sur l'écran\npour gagner de la Force";
            });

            CreateOrUpdate("Step_02_force_intro", s =>
            {
                s.id = "force_intro";
                s.anchor = TutorialStep.AnchorTarget.ForcePill;
                s.trigger = TutorialStep.TriggerCondition.TapsReached;
                s.triggerThreshold = 15;
                s.messageFr = "Voici ta Force.\nPlus tu en as, plus tu progresses.";
            });

            CreateOrUpdate("Step_03_buy_strike", s =>
            {
                s.id = "buy_strike";
                s.anchor = TutorialStep.AnchorTarget.UpgradeStrike;
                s.trigger = TutorialStep.TriggerCondition.UpgradePurchased;
                s.messageFr = "Achète Strike\npour taper plus fort.";
            });

            CreateOrUpdate("Step_04_stages_intro", s =>
            {
                s.id = "stages_intro";
                s.anchor = TutorialStep.AnchorTarget.StageChip;
                s.trigger = TutorialStep.TriggerCondition.StageProgressed;
                s.messageFr = "Bats les adversaires\npour avancer les Stades.";
            });

            CreateOrUpdate("Step_05_vague_ready", s =>
            {
                s.id = "vague_ready";
                s.anchor = TutorialStep.AnchorTarget.VagueButton;
                s.trigger = TutorialStep.TriggerCondition.ElanReachedMax;
                s.messageFr = "Vague est prête !\nDéclenche-la.";
            });

            CreateOrUpdate("Step_06_settings_bravo", s =>
            {
                s.id = "settings_bravo";
                s.anchor = TutorialStep.AnchorTarget.SettingsButton;
                s.trigger = TutorialStep.TriggerCondition.VagueTriggered;
                s.messageFr = "Bravo ! Ouvre ici les paramètres.\nBonne aventure.";
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Saga] Generated 6 tutorial steps in {TutorialFolder}/. TutorialService.Load() picks them up on next boot.");
        }

        private static void CreateOrUpdate(string fileName, System.Action<TutorialStep> setup)
        {
            var path = $"{TutorialFolder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<TutorialStep>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TutorialStep>();
                AssetDatabase.CreateAsset(asset, path);
            }
            setup(asset);
            EditorUtility.SetDirty(asset);
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
