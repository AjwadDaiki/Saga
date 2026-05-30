using System.IO;
using Saga.Data;
using UnityEditor;
using UnityEngine;

namespace Saga.EditorTools
{
    /// <summary>
    /// Sprint 7.6 — One-shot generator for the RhosGFX Asset Catalog.
    ///
    /// Scans <c>Assets/_Project/Art/External/cartoony-ui-pack-full/</c>, forces all referenced PNGs
    /// to <c>TextureImporterType.Sprite</c>, and materializes the asset at
    /// <c>Assets/_Project/Resources/UI/RhosGFXCatalog.asset</c>.
    ///
    /// Idempotent — re-running rebuilds the references. Safe to re-run after pack updates.
    ///
    /// Menu: <b>Saga > Sprint 7.6 > Generate RhosGFX Catalog</b>
    /// </summary>
    public static class RhosGFXCatalogGenerator
    {
        private const string PackRoot = "Assets/_Project/Art/External/cartoony-ui-pack-full";
        private const string Buttons3D = PackRoot + "/Buttons/3D";
        private const string ButtonsFlat = PackRoot + "/Buttons/Flat";
        private const string Containers = PackRoot + "/Containers";
        private const string Bars = PackRoot + "/Bars";
        private const string Frames = PackRoot + "/Frames";
        private const string Icons = PackRoot + "/[THANK YOU!] Icons";

        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const string UIFolder = "Assets/_Project/Resources/UI";
        private const string AssetPath = "Assets/_Project/Resources/UI/RhosGFXCatalog.asset";

        [MenuItem("Saga/Sprint 7.6/Generate RhosGFX Catalog")]
        public static void Generate()
        {
            AssetDatabase.Refresh();
            EnsureFolder(ResourcesFolder);
            EnsureFolder(UIFolder);

            var catalog = AssetDatabase.LoadAssetAtPath<RhosGFXAssetCatalog>(AssetPath);
            var created = false;
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<RhosGFXAssetCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
                created = true;
            }

            // Naming convention discovered in pack:
            //   Buttons / Bars / Containers / Frames-Basic : require -{shade} suffix where
            //     shade ∈ {regular, dark, darker, light, lighter}. Baseline = "-regular".
            //   Frames Inset/Ornate/Nailed/Pointed : bare baseline (no shade) + optional shades.
            //   Brown color splits into "darkbrown" / "lightbrown" — we pick darkbrown for dojo depth.
            //   Thin Bars : no shade suffix at all.

            // ----- Buttons 3D Round (color-regular baseline) -----
            catalog.round3DYellow25 = LoadButtonSet($"{Buttons3D}/Round/3. Yellow", "button-round-3d-2.5-yellow-regular");
            catalog.round3DPurple25 = LoadButtonSet($"{Buttons3D}/Round/7. Purple", "button-round-3d-2.5-purple-regular");
            catalog.round3DGrey1 = LoadButtonSet($"{Buttons3D}/Round/1. Grey", "button-round-3d-1-grey-regular");
            catalog.round3DYellow1 = LoadButtonSet($"{Buttons3D}/Round/3. Yellow", "button-round-3d-1-yellow-regular");

            // ----- Buttons 3D Square -----
            catalog.square3DBlue25 = LoadButtonSet($"{Buttons3D}/Square/6. Blue", "button-square-3d-2.5-blue-regular");
            catalog.square3DForestGreen25 = LoadButtonSet($"{Buttons3D}/Square/5. Forest Green", "button-square-3d-2.5-forestgreen-regular");
            catalog.square3DGreen25 = LoadButtonSet($"{Buttons3D}/Square/4. Green", "button-square-3d-2.5-green-regular");

            // ----- Buttons Flat -----
            catalog.squareFlatYellow25 = LoadButtonSet($"{ButtonsFlat}/Square/3. Yellow", "button-square-flat-2.5-yellow-regular");
            catalog.roundFlatGrey1 = LoadButtonSet($"{ButtonsFlat}/Round/1. Grey", "button-round-flat-1-grey-regular");

            // ----- Containers (color-regular) -----
            catalog.container3DYellow = LoadSprite($"{Containers}/3D/3. Yellow/container-3d-yellow-regular.png");
            catalog.containerFlatBrown = LoadSprite($"{Containers}/Flat/9. Brown/container-flat-darkbrown-regular.png");
            catalog.container3DBrown = LoadSprite($"{Containers}/3D/9. Brown/container-3d-darkbrown-regular.png");

            // ----- Bars Regular (container + fill, color-regular). Thin = bare. -----
            catalog.barRegularRedContainer = LoadSprite($"{Bars}/Regular/2. Red/progress-container-regular-red-regular.png");
            catalog.barRegularRedFill = LoadSprite($"{Bars}/Regular/2. Red/progress-bar-regular-red-regular.png");
            catalog.barThinWhiteFill = LoadSprite($"{Bars}/Thin/0. White/progress-bar-thin-white.png");

            // ----- Frames -----
            // Basic Brown : darkbrown-regular (deeper dojo wood).
            catalog.frameBasicBrown = LoadSprite($"{Frames}/Basic/9. Brown/frame-basic-darkbrown-regular.png");
            catalog.frameBasicGrey = LoadSprite($"{Frames}/Basic/1. Grey/frame-basic-grey-regular.png");
            // Inset/Ornate/Nailed/Pointed : bare baseline (no shade).
            catalog.frameInsetBrown = LoadSprite($"{Frames}/Inset/9. Brown/frame-inset-darkbrown.png");
            catalog.frameOrnateBrown = LoadSprite($"{Frames}/Ornate/9. Brown/frame-ornate-darkbrown.png");
            catalog.frameNailedBrown = LoadSprite($"{Frames}/Nailed/9. Brown/frame-nailed-darkbrown.png");
            catalog.framePointedYellow = LoadSprite($"{Frames}/Pointed/3. Yellow/frame-pointed-yellow.png");

            // ----- Icons 64px -----
            catalog.iconCoinGold = LoadSprite($"{Icons}/Coin 2/Coin 2 Gold 64.png");
            catalog.iconGem = LoadSprite($"{Icons}/Gem/Gem 64.png");
            catalog.iconGearOutline = LoadSprite($"{Icons}/Settings 2/Gear 2 Outline 64.png");
            catalog.iconSkull = LoadSprite($"{Icons}/Skull/Skull 64.png");
            catalog.iconHome = LoadSprite($"{Icons}/Home 2/Home 2 Blue 64.png");
            catalog.iconMap = LoadSprite($"{Icons}/Map/Map 64.png");
            catalog.iconBackpack = LoadSprite($"{Icons}/Backpack/Backpack 64.png");
            catalog.iconChest = LoadSprite($"{Icons}/Chest 2/Chest Medium 2 64.png");
            catalog.iconTrophy = LoadSprite($"{Icons}/Trophy/Trophy 64.png");
            catalog.iconSword = LoadSprite($"{Icons}/Sword/Sword 2 Blue 64.png");
            catalog.iconBell = LoadSprite($"{Icons}/Bell/Bell 64.png");
            catalog.iconFriends = LoadSprite($"{Icons}/Friends 2/Friends 2 64.png");
            catalog.iconHeartOutline = LoadSprite($"{Icons}/Heart/Heart Outline 64.png");
            catalog.iconClock = LoadSprite($"{Icons}/Clock/Clock 64.png");
            catalog.iconXButton = LoadSprite($"{Icons}/X Button/X Button 64.png");
            catalog.iconScroll = LoadSprite($"{Icons}/Scroll/Scroll 64.png");
            catalog.iconPotion = LoadSprite($"{Icons}/Potion 1/Potion 1 64.png");

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Saga] {(created ? "Created" : "Updated")} RhosGFX catalog at {AssetPath}. Builders Sprint 7.6 consume RhosGFXAssetCatalog.Get() — Phase D ready.");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = catalog;
        }

        private static RhosGFXAssetCatalog.ButtonStateSet LoadButtonSet(string folder, string baseName)
        {
            return new RhosGFXAssetCatalog.ButtonStateSet
            {
                standard = LoadSprite($"{folder}/{baseName}_standard.png"),
                hover = LoadSprite($"{folder}/{baseName}_hover.png"),
                focus = LoadSprite($"{folder}/{baseName}_focus.png"),
                pressed = LoadSprite($"{folder}/{baseName}_pressed.png"),
            };
        }

        /// <summary>
        /// Loads a sprite, forcing PNG → TextureImporterType.Sprite if needed (RhosGFX ships as
        /// generic Texture2D by default — Sprite mode required for Image.sprite consumption).
        /// </summary>
        private static Sprite LoadSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.SaveAndReimport();
            }
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                Debug.LogWarning($"[RhosGFX] Missing sprite at {path}");
            }
            return s;
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
