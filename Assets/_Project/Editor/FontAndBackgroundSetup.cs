using System.IO;
using System.Linq;
using Saga.Data;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 7.5 refonte — automates the two manual asset steps that normally need the TMP Font
    /// Asset Creator GUI + manual import:
    ///   1. Generates 3 TMP_FontAssets (Plus Jakarta Sans / Lilita One / JetBrains Mono) from the
    ///      .ttf files under Art/Fonts, with dynamic atlas (glyphs rendered on demand).
    ///   2. Copies the Vibrant Quest background into Resources/Backgrounds so it can be loaded at runtime.
    ///   3. Assigns the fonts to the DesignTokens asset.
    ///
    /// Menu: <b>Saga > Design > Generate Font Assets + Background</b>
    /// Idempotent — re-running overwrites the generated assets.
    /// </summary>
    public static class FontAndBackgroundSetup
    {
        private const string FontOutFolder = "Assets/_Project/Resources/Fonts";
        private const string BgOutFolder = "Assets/_Project/Resources/Backgrounds";
        private const string TokensPath = "Assets/_Project/Resources/DesignTokens/SagaDesignTokens.asset";

        [MenuItem("Saga/Design/Generate Font Assets + Background")]
        public static void Setup()
        {
            AssetDatabase.Refresh();
            EnsureFolder("Assets/_Project/Resources");
            EnsureFolder(FontOutFolder);
            EnsureFolder(BgOutFolder);

            var primary = BuildFontAsset("PlusJakartaSans-Bold", "Saga_PlusJakartaSans");
            var display = BuildFontAsset("LilitaOne-Regular", "Saga_LilitaOne");
            var numbers = BuildFontAsset("JetBrainsMono-Bold", "Saga_JetBrainsMono");

            CopyBackground();
            AssignToTokens(primary, display, numbers);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Saga] Font assets + background generated. DesignTokens font slots assigned.");
        }

        private static TMP_FontAsset BuildFontAsset(string ttfNameContains, string outName)
        {
            // Find the .ttf by EXACT filename (the Art/Fonts tree has nested download folders +
            // sibling variants like *-BoldItalic that a Contains() match would wrongly grab).
            var wanted = ttfNameContains + ".ttf";
            var guids = AssetDatabase.FindAssets("t:Font");
            string ttfPath = null;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileName(p) == wanted)
                {
                    ttfPath = p;
                    break;
                }
            }
            if (ttfPath == null)
            {
                Debug.LogError($"[Saga] TTF '{wanted}' introuvable sous Art/Fonts. Font asset non généré.");
                return null;
            }

            var srcFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (srcFont == null)
            {
                Debug.LogError($"[Saga] Impossible de charger la Font à {ttfPath}.");
                return null;
            }

            var outPath = $"{FontOutFolder}/{outName} SDF.asset";

            // Dynamic atlas (1024²) — glyphs rasterized on demand, no pre-bake needed.
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                srcFont, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (fontAsset == null)
            {
                Debug.LogError($"[Saga] CreateFontAsset a échoué pour {ttfNameContains}.");
                return null;
            }
            fontAsset.name = outName;

            // Remove any prior asset then write fresh, embedding material + atlas as sub-assets.
            AssetDatabase.DeleteAsset(outPath);
            AssetDatabase.CreateAsset(fontAsset, outPath);
            if (fontAsset.material != null)
            {
                fontAsset.material.name = outName + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }
            if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
            {
                fontAsset.atlasTextures[0].name = outName + " Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            }
            EditorUtility.SetDirty(fontAsset);
            Debug.Log($"[Saga] Font asset '{outName}' généré depuis {ttfPath}.");
            return fontAsset;
        }

        private static void CopyBackground()
        {
            // Prefer PNG, fall back to JPG.
            var src = "Assets/_Project/Art/Backgrounds/Vibrant Quest Background.png";
            if (!File.Exists(src)) src = "Assets/_Project/Art/Backgrounds/Vibrant Quest Background.jpg";
            if (!File.Exists(src))
            {
                Debug.LogWarning("[Saga] Vibrant Quest Background introuvable sous Art/Backgrounds — fond illustré non copié (fallback gradient utilisé).");
                return;
            }
            var ext = Path.GetExtension(src);
            var dst = $"{BgOutFolder}/dojo_bg{ext}";
            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(src, dst))
            {
                // Ensure it imports as a Sprite so Image can use it.
                var importer = AssetImporter.GetAtPath(dst) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                Debug.Log($"[Saga] Background copié vers {dst} (Sprite).");
            }
        }

        private static void AssignToTokens(TMP_FontAsset primary, TMP_FontAsset display, TMP_FontAsset numbers)
        {
            var tokens = AssetDatabase.LoadAssetAtPath<DesignTokens>(TokensPath);
            if (tokens == null)
            {
                Debug.LogWarning("[Saga] DesignTokens asset introuvable — lance 'Saga > Design > Generate Design Tokens' d'abord, puis re-run.");
                return;
            }
            var so = new SerializedObject(tokens);
            if (primary != null) so.FindProperty("fontPrimary").objectReferenceValue = primary;
            if (display != null) so.FindProperty("fontDisplay").objectReferenceValue = display;
            if (numbers != null) so.FindProperty("fontNumbers").objectReferenceValue = numbers;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tokens);
            Debug.Log("[Saga] DesignTokens : fontPrimary/fontDisplay/fontNumbers assignés.");
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
