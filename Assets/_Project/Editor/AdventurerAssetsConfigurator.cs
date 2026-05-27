using System.Collections.Generic;
using System.IO;
using System.Linq;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// One-shot configurator for Sprint 3 character assets.
    /// Menu: <b>Saga > Sprint 3 > Configure Adventurer Assets</b>
    ///
    /// Idempotent. Applies twice = same result.
    ///
    /// Does three things:
    /// 1. Walks all PNGs under Art/Characters/Adventurer/ and Art/FX/ and forces
    ///    Filter Mode = Point, PPU = 32, Compression = None, sRGB on.
    /// 2. Builds the <c>AdventurerAnimationLibrary</c> ScriptableObject in
    ///    Assets/_Project/Resources/Animations/, populated with idle/attack/hurt
    ///    sorted by filename.
    /// 3. Ensures the Resources/Animations folder exists.
    /// </summary>
    public static class AdventurerAssetsConfigurator
    {
        private const string CharactersFolder = "Assets/_Project/Art/Characters/Adventurer";
        private const string FxFolder = "Assets/_Project/Art/FX";
        private const string AnimationsFolder = "Assets/_Project/Resources/Animations";
        private const string LibraryPath = "Assets/_Project/Resources/Animations/AdventurerAnimationLibrary.asset";

        private const int CharacterPpu = 32;
        private const int FxPpu = 32;

        [MenuItem("Saga/Sprint 3/Configure Adventurer Assets")]
        public static void Configure()
        {
            // Pull in any disk-level changes (folder renames, new PNGs) before scanning.
            AssetDatabase.Refresh();

            EnsureFolder(AnimationsFolder);
            ConfigureCharacterImports();
            ConfigureFxImports();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BuildAnimationLibrary();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureCharacterImports()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { CharactersFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                ApplyPixelImportSettings(path, CharacterPpu, SpriteImportMode.Single);
            }
        }

        private static void ConfigureFxImports()
        {
            if (!AssetDatabase.IsValidFolder(FxFolder)) return;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { FxFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                // For now treat all FX as Single sprites. Slicing the weapon-hit spritesheet
                // is a Sprint 4+ polish step (requires knowing exact cell dimensions).
                ApplyPixelImportSettings(path, FxPpu, SpriteImportMode.Single);
            }
        }

        private static void ApplyPixelImportSettings(string assetPath, int ppu, SpriteImportMode mode)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            var changed = false;
            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != mode) { importer.spriteImportMode = mode; changed = true; }
            if (importer.spritePixelsPerUnit != ppu) { importer.spritePixelsPerUnit = ppu; changed = true; }
            if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void BuildAnimationLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<SpriteAnimationLibrary>(LibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<SpriteAnimationLibrary>();
                AssetDatabase.CreateAsset(lib, LibraryPath);
            }

            var anims = new List<SpriteAnimationData>
            {
                BuildAnimation("idle",    $"{CharactersFolder}/Idle",    frameDuration: 0.15f, loop: true),
                // Sprint 3 weighted attack pool — see Gameplay/AttackSelector.cs for distribution per combo tier.
                BuildAnimation("attack1", $"{CharactersFolder}/Attack1", frameDuration: 0.06f, loop: false),
                BuildAnimation("attack2", $"{CharactersFolder}/Attack2", frameDuration: 0.06f, loop: false),
                BuildAnimation("attack3", $"{CharactersFolder}/Attack3", frameDuration: 0.06f, loop: false),
                BuildAnimation("hurt",    $"{CharactersFolder}/Hurt",    frameDuration: 0.08f, loop: false),
            };

            // EditorSetAnimations REPLACES the array entirely — previous entries (e.g. legacy "attack")
            // disappear cleanly. No orphaned references.
            var final = anims.Where(a => a != null && a.frames != null && a.frames.Length > 0).ToArray();
            lib.EditorSetAnimations(final);
            EditorUtility.SetDirty(lib);

            // Clear summary so Ajwad can confirm at a glance which anims landed in the library.
            if (final.Length == 0)
            {
                Debug.LogError($"[Saga] Sprint 3 library REBUILD FAILED — 0 anims registered. Check that {CharactersFolder}/{{Idle,Attack1,Attack2,Attack3,Hurt}} exist and contain PNG sprites.");
            }
            else
            {
                var summary = string.Join(", ", final.Select(a => $"{a.animationName}({a.frames.Length}f)"));
                Debug.Log($"[Saga] Sprint 3 library REBUILT — {final.Length} anims: {summary}. Asset: {LibraryPath}");
            }
        }

        private static SpriteAnimationData BuildAnimation(string animName, string folder, float frameDuration, bool loop)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"[Saga] Animation folder missing: {folder}");
                return null;
            }
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            var sprites = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p, System.StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(s => s != null)
                .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[Saga] No sprites found in {folder}");
                return null;
            }

            Debug.Log($"[Saga] Animation '{animName}': {sprites.Length} frames from {folder}");
            return new SpriteAnimationData
            {
                animationName = animName,
                frames = sprites,
                frameDuration = frameDuration,
                loop = loop
            };
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
