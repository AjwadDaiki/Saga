using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 7.6 M2-fix P8 — Auto-imports RhosGFX Cartoony UI Pack PNGs as crisp Sprite assets.
    /// Settings : Trilinear + MipMaps + Uncompressed + maxSize 1024 (sprites UI critiques zoom-friendly).
    /// alphaIsTransparency garantit un alpha propre (pas de halo noir au filtrage).
    ///
    /// Fires automatically on any (re)import under <c>cartoony-ui-pack-full/</c>. Use the menu
    /// <b>Saga > Sprint 7.6 > Reimport All RhosGFX Sprites</b> to force-apply on the existing pack.
    /// </summary>
    public class RhosGFXSpriteImporter : AssetPostprocessor
    {
        private const string PackRoot = "cartoony-ui-pack-full";

        private void OnPreprocessTexture()
        {
            if (assetPath.IndexOf(PackRoot, System.StringComparison.OrdinalIgnoreCase) < 0) return;
            if (assetPath.EndsWith(".svg", System.StringComparison.OrdinalIgnoreCase)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Trilinear;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        [MenuItem("Saga/Sprint 7.6/Reimport All RhosGFX Sprites")]
        public static void ReimportAll()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[]
            {
                "Assets/_Project/Art/External/cartoony-ui-pack-full"
            });
            var n = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    n++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            Debug.Log($"[Saga RhosGFX] Reimported {n} sprites with Trilinear + MipMaps + Uncompressed + maxSize 1024.");
        }
    }
}
