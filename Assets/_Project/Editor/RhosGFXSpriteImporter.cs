using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 7.6 M2-fix P12 — Auto-imports RhosGFX Cartoony UI Pack PNGs as crisp UI Sprites.
    ///
    /// IMPORTANT (changement M2-fix-5) : <b>Bilinear + NO mipmaps</b> pour UI 2D au pixel près.
    /// L'itération précédente utilisait Trilinear + MipMaps (préconisé pour textures 3D qui
    /// réduisent à distance) — résultat : sprites flous au scale natif HUD. Pour UI ScreenSpaceOverlay
    /// avec CanvasScaler ScaleWithScreenSize, on veut un sampling 1:1 sans LOD.
    ///
    /// FullRect + Uncompressed garantit la netteté + alpha propre.
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
            // Note : SpriteMeshType est sur TextureImporterSettings (pas TextureImporter direct).
            // Pour SpriteImportMode.Single, le default est FullRect — pas besoin de set explicite.
            importer.filterMode = FilterMode.Bilinear;        // P12 : Bilinear pour UI 2D (vs Trilinear flou)
            importer.mipmapEnabled = false;                   // P12 : pas de mipmaps en UI overlay
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
            Debug.Log($"[Saga RhosGFX] Reimported {n} sprites with Bilinear + no MipMaps + Uncompressed + maxSize 1024 (UI 2D crisp).");
        }
    }
}
