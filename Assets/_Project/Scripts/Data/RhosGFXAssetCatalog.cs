using System;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// Sprint 7.6 — Single source of sprite references for the RhosGFX Cartoony UI Pack.
    /// Loaded from <c>Resources/UI/RhosGFXCatalog.asset</c> on first access.
    ///
    /// Materialize the asset via menu <b>Saga > Sprint 7.6 > Generate RhosGFX Catalog</b>
    /// (see <c>RhosGFXCatalogGenerator</c> in Saga.Editor). The generator picks the sprites
    /// from <c>Assets/_Project/Art/External/cartoony-ui-pack-full/</c> following the
    /// mapping documented in <c>docs/RHOSGFX_MAPPING.md</c>.
    ///
    /// Builders consume this catalog through <see cref="Get"/>. Each <see cref="ButtonStateSet"/>
    /// carries the 4 RhosGFX states (standard/hover/focus/pressed) for runtime sprite swap on press.
    ///
    /// Color tinting (palette DA) is applied at runtime via <c>Image.color</c> on top of the
    /// RhosGFX neutral sprite — see <see cref="DesignTokens"/> for palette source of truth.
    /// </summary>
    [CreateAssetMenu(fileName = "RhosGFXCatalog", menuName = "Saga/RhosGFX Asset Catalog")]
    public class RhosGFXAssetCatalog : ScriptableObject
    {
        private static RhosGFXAssetCatalog _cached;

        /// <summary>Singleton accessor — loads from Resources once, blank fallback if asset missing.</summary>
        public static RhosGFXAssetCatalog Get()
        {
            if (_cached != null) return _cached;
            _cached = Resources.Load<RhosGFXAssetCatalog>("UI/RhosGFXCatalog");
            if (_cached == null)
            {
                Debug.LogWarning("[RhosGFX] Catalog missing — run 'Saga > Sprint 7.6 > Generate RhosGFX Catalog' menu once. Falling back to blank catalog (builders will use legacy PuffySprite procedural).");
                _cached = CreateInstance<RhosGFXAssetCatalog>();
            }
            return _cached;
        }

        /// <summary>True when the catalog asset was actually loaded (i.e. builders should use RhosGFX path).</summary>
        public bool IsMaterialized => round3DYellow25.standard != null;

        [Serializable]
        public struct ButtonStateSet
        {
            public Sprite standard;
            public Sprite hover;
            public Sprite focus;
            public Sprite pressed;
        }

        // ----- Buttons 3D Round (currency pills, nav active, settings) -----
        [Header("Buttons 3D Round")]
        [Tooltip("Force pill — tint runtime DA Or #FFD84D (jauneReward).")]
        public ButtonStateSet round3DYellow25;
        [Tooltip("Échos pill — tint runtime DA Lavande #9A7BFF (lavandeUI).")]
        public ButtonStateSet round3DPurple25;
        [Tooltip("Settings round button.")]
        public ButtonStateSet round3DGrey1;
        [Tooltip("Card icon container, tab Dojo actif lift +12.")]
        public ButtonStateSet round3DYellow1;

        // ----- Buttons 3D Square (skills, modal CTA) -----
        [Header("Buttons 3D Square")]
        [Tooltip("VAGUE skill — tint runtime DA Cyan #65C8FF (skyBlue).")]
        public ButtonStateSet square3DBlue25;
        [Tooltip("SOUFFLE skill — tint runtime DA Mint #69E6A3 (mintPositif).")]
        public ButtonStateSet square3DForestGreen25;
        [Tooltip("Modal CTA generic (Affronter Maître).")]
        public ButtonStateSet square3DGreen25;

        // ----- Buttons Flat Square (cost pill) -----
        [Header("Buttons Flat Square")]
        [Tooltip("Cost pill — tint runtime DA Or.")]
        public ButtonStateSet squareFlatYellow25;

        // ----- Buttons Flat Round (nav inactive) -----
        [Header("Buttons Flat Round")]
        [Tooltip("Bottom nav tab inactive — alpha 0.4-0.5 + tint Brown.")]
        public ButtonStateSet roundFlatGrey1;

        // ----- Containers -----
        [Header("Containers (9-slice)")]
        [Tooltip("Stage chip background — tint Or DA.")]
        public Sprite container3DYellow;
        [Tooltip("BottomNav + Upgrades panel background — tint Brown dojo.")]
        public Sprite containerFlatBrown;
        [Tooltip("Modal background — tint Brown.")]
        public Sprite container3DBrown;

        // ----- Bars -----
        [Header("Bars (Regular for boss HP, Thin for Élan)")]
        [Tooltip("Boss progress container — tint Rouge DA #C8392A (accentDanger).")]
        public Sprite barRegularRedContainer;
        [Tooltip("Boss progress fill (RadialFill horizontal).")]
        public Sprite barRegularRedFill;
        [Tooltip("Élan jauge interne VAGUE (overlay) — tint Blanc.")]
        public Sprite barThinWhiteFill;

        // ----- Frames -----
        [Header("Frames (decorative)")]
        [Tooltip("Cards Upgrades — tint Brown dojo.")]
        public Sprite frameBasicBrown;
        [Tooltip("Toast Coming Soon — tint Grey.")]
        public Sprite frameBasicGrey;
        [Tooltip("Inventaire slot, modal border — tint Brown.")]
        public Sprite frameInsetBrown;
        [Tooltip("Citation Maître scroll — décoratif.")]
        public Sprite frameOrnateBrown;
        [Tooltip("Achievement / Maître unlock.")]
        public Sprite frameNailedBrown;
        [Tooltip("Combat combo overlay — décoratif.")]
        public Sprite framePointedYellow;

        // ----- Icons 64px (HUD/buttons) -----
        [Header("Icons 64px")]
        public Sprite iconCoinGold;       // Force counter
        public Sprite iconGem;            // Échos counter (re-tint Lavande)
        public Sprite iconGearOutline;    // Settings
        public Sprite iconSkull;          // Stage boss markers
        public Sprite iconHome;           // BottomNav Dojo
        public Sprite iconMap;            // BottomNav Voies
        public Sprite iconBackpack;       // BottomNav Inventaire
        public Sprite iconChest;          // BottomNav Reliques / Maître unlock
        public Sprite iconTrophy;         // BottomNav Hall des Légendes
        public Sprite iconSword;          // Card Strike
        public Sprite iconBell;           // Card Focus (méditation zen)
        public Sprite iconFriends;        // Card Power (disciples groupe)
        public Sprite iconHeartOutline;   // Fallback Life / Focus alt
        public Sprite iconClock;          // Toast Coming Soon
        public Sprite iconXButton;        // Modal close
        public Sprite iconScroll;         // Citation Maître / lore
        public Sprite iconPotion;         // VAGUE icon alt
    }
}
