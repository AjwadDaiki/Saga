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
            importer.filterMode = FilterMode.Bilinear;        // P12 : Bilinear pour UI 2D
            importer.mipmapEnabled = false;                   // P12 : pas de mipmaps en UI overlay
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            // Sprint 7.7 P2/P3 — 9-slice borders heuristiques pour stretching propre sans squish.
            // Sprites RhosGFX sont 64×64 ou 128×128 ou 160×64 natifs ; rendus sur RT pills/cards
            // beaucoup plus grands → Simple stretch déforme. Image.Type.Sliced avec borders
            // préserve les corners ronds et tile/stretch uniquement le centre plat.
            //
            // Borders heuristiques par catégorie d'asset (chemin) — testées contre l'inventaire :
            //   Buttons round 3D 1 (64×64)        : corner 16 (radius rond visible ~16 px)
            //   Buttons round 3D 2.5 (160×64)     : corner 24 horizontal, 16 vertical
            //   Containers / Frames (128×128)     : corner 20 (deeper rounded)
            //   Bars regular container (129×20)   : corner 8 (capsule mince)
            //   Bars fill (61×12 ou 61×8)         : corner 4 (très fin)
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteBorder = BorderForAsset(assetPath);
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
        }

        /// <summary>
        /// Heuristic 9-slice border (left, bottom, right, top) pour chaque famille de sprite RhosGFX.
        /// </summary>
        private static Vector4 BorderForAsset(string path)
        {
            // Buttons : corner radius dans le sprite est visible — borders qui préservent les arcs.
            if (path.Contains("/Buttons/3D/Round/") || path.Contains("/Buttons/Flat/Round/"))
            {
                // Round 2.5 = 160×64 native (corner ~24 px). Round 1 = 64×64 native (corner ~16 px).
                if (path.Contains("3d-2.5") || path.Contains("flat-2.5"))
                    return new Vector4(24, 16, 24, 16);
                return new Vector4(16, 16, 16, 16);
            }
            if (path.Contains("/Buttons/3D/Square/") || path.Contains("/Buttons/Flat/Square/"))
            {
                if (path.Contains("3d-2.5") || path.Contains("flat-2.5"))
                    return new Vector4(20, 14, 20, 14);
                return new Vector4(14, 14, 14, 14);
            }
            // Containers + Frames : 128×128 native, corners arrondis visibles ~20 px.
            if (path.Contains("/Containers/") || path.Contains("/Frames/"))
            {
                return new Vector4(20, 20, 20, 20);
            }
            // Bars Regular container : ~129×20 native, capsule horizontal — borders 8 caps.
            if (path.Contains("/Bars/Regular/") && path.Contains("container"))
            {
                return new Vector4(8, 8, 8, 8);
            }
            // Bars fill (Regular ou Thin) : très fins, borders petits.
            if (path.Contains("/Bars/"))
            {
                return new Vector4(4, 4, 4, 4);
            }
            // Icons + default : pas de borders (rendu Simple).
            return Vector4.zero;
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
