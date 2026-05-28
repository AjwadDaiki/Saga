using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot generator for the Sprint 7.5 DesignTokens ScriptableObject.
    /// Idempotent — re-running resets to the spec values from the Sprint 7.5 brief.
    /// To re-skin without losing the asset, edit it directly in the Inspector.
    ///
    /// Menu: <b>Saga > Design > Generate Design Tokens</b>
    /// </summary>
    public static class DesignTokensCreator
    {
        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string TokensFolder = "Assets/_Project/Resources/DesignTokens";
        private const string AssetPath = "Assets/_Project/Resources/DesignTokens/SagaDesignTokens.asset";

        [MenuItem("Saga/Design/Generate Design Tokens")]
        public static void GenerateTokens()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(TokensFolder);

            var asset = AssetDatabase.LoadAssetAtPath<DesignTokens>(AssetPath);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DesignTokens>();
                AssetDatabase.CreateAsset(asset, AssetPath);
                created = true;
            }

            // The SO field initializers already match the brief — calling Reset reapplies them.
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} DesignTokens at {AssetPath}. Drop the 3 Google fonts in the font slots: Inter / JetBrains Mono / Cinzel.");
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
