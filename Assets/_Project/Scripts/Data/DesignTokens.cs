using TMPro;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// Sprint 7.5 design system — single source of truth for colors, spacing, typography,
    /// radii. Loaded from Resources/DesignTokens/SagaDesignTokens.asset on first access.
    ///
    /// Convention: never hardcode a color or px value in UI code — read it from this SO.
    /// Lets us re-skin the entire game by editing one asset (or by generating per-region
    /// theme variants Sprint 8+ for the carte monde).
    ///
    /// Fonts are <c>TMP_FontAsset</c> slots that fall back to <c>TMP_Settings.defaultFontAsset</c>
    /// when null (Ajwad fills them in from Window > TextMeshPro > Font Asset Creator with the
    /// 3 Google Fonts: Inter / JetBrains Mono / Cinzel).
    /// </summary>
    [CreateAssetMenu(fileName = "SagaDesignTokens", menuName = "Saga/Design Tokens")]
    public class DesignTokens : ScriptableObject
    {
        private static DesignTokens _cached;

        /// <summary>Singleton accessor — loads from Resources once, caches forever.</summary>
        public static DesignTokens Get()
        {
            if (_cached != null) return _cached;
            _cached = Resources.Load<DesignTokens>("DesignTokens/SagaDesignTokens");
            if (_cached == null)
            {
                Debug.LogWarning("[DesignTokens] Resources/DesignTokens/SagaDesignTokens missing — using runtime defaults. Run 'Saga > Design > Generate Design Tokens' to materialize the asset.");
                _cached = CreateRuntimeFallback();
            }
            return _cached;
        }

        // ----- Backgrounds -----
        [Header("Backgrounds")]
        public Color bgDeep = new Color(0.039f, 0.039f, 0.039f, 1f);     // #0a0a0a
        public Color bgMain = new Color(0.075f, 0.075f, 0.075f, 1f);     // #131313
        public Color bgPanel = new Color(0.110f, 0.110f, 0.110f, 1f);    // #1c1c1c
        public Color bgCard = new Color(0.137f, 0.137f, 0.137f, 1f);     // #232323
        public Color bgOverlay = new Color(0f, 0f, 0f, 0.85f);

        // ----- Surfaces -----
        [Header("Surfaces")]
        public Color surfaceLow = new Color(0.165f, 0.165f, 0.165f, 1f); // #2a2a2a
        public Color surfaceMid = new Color(0.227f, 0.227f, 0.227f, 1f); // #3a3a3a
        public Color surfaceHigh = new Color(0.290f, 0.290f, 0.290f, 1f); // #4a4a4a

        // ----- Text -----
        [Header("Text")]
        public Color textPrimary = new Color(0.945f, 0.937f, 0.910f, 1f);  // #F1EFE8
        public Color textSecondary = new Color(0.722f, 0.690f, 0.627f, 1f); // #B8B0A0
        public Color textDisabled = new Color(0.416f, 0.384f, 0.376f, 1f); // #6a6260
        public Color textLore = new Color(0.831f, 0.773f, 0.627f, 1f);     // #D4C5A0

        // ----- Accents -----
        [Header("Accents")]
        public Color accentPrimary = new Color(0.980f, 0.780f, 0.459f, 1f); // #FAC775
        public Color accentAction = new Color(0.910f, 0.588f, 0.259f, 1f);  // #E89642
        public Color accentDanger = new Color(0.784f, 0.224f, 0.165f, 1f);  // #C8392A
        public Color accentSuccess = new Color(0.478f, 0.671f, 0.361f, 1f); // #7AAB5C

        // ----- DA Palette (DIRECTION_ARTISTIQUE.md §5) — palette officielle Sprint 7.5 Polish.
        // Source de vérité unique : DIRECTION_ARTISTIQUE.md (§4.2 contours, §5.1 principale,
        // §5.2 neutres, §5.3 décor samouraï, §7.2 héros, §10.3 raretés). Remplace l'ancienne
        // palette Material 3 "Vibrant Quest" (supprimée Sprint 7.5 Phase 2 Polish).
        [Header("DA Palette — section 5.1 principale")]
        public Color coralAction = new Color(1.000f, 0.420f, 0.420f, 1f);     // #FF6B6B
        public Color roseSakura = new Color(1.000f, 0.620f, 0.784f, 1f);      // #FF9EC8
        public Color mintPositif = new Color(0.412f, 0.902f, 0.639f, 1f);     // #69E6A3
        public Color vertBouton = new Color(0.490f, 0.890f, 0.310f, 1f);      // #7DE34F
        public Color skyBlue = new Color(0.396f, 0.784f, 1.000f, 1f);         // #65C8FF
        public Color jauneReward = new Color(1.000f, 0.847f, 0.302f, 1f);     // #FFD84D
        public Color orangeChaud = new Color(1.000f, 0.624f, 0.239f, 1f);     // #FF9F3D
        public Color lavandeUI = new Color(0.604f, 0.482f, 1.000f, 1f);       // #9A7BFF
        public Color navyContour = new Color(0.145f, 0.157f, 0.239f, 1f);     // #25283D
        public Color cremeText = new Color(1.000f, 0.957f, 0.839f, 1f);       // #FFF4D6

        [Header("DA Palette — section 5.2 neutres")]
        public Color panelSombre = new Color(0.227f, 0.263f, 0.345f, 1f);     // #3A4358
        public Color panelSombre2 = new Color(0.184f, 0.216f, 0.290f, 1f);    // #2F374A
        public Color panelClair = new Color(0.976f, 0.902f, 0.784f, 1f);      // #F9E6C8
        public Color blancChaud = new Color(1.000f, 0.976f, 0.918f, 1f);      // #FFF9EA
        public Color grisTexteSecondaire = new Color(0.725f, 0.757f, 0.831f, 1f); // #B9C1D4
        public Color ombreDouce = new Color(0.110f, 0.125f, 0.188f, 1f);      // #1C2030

        [Header("DA Palette — section 5.3 décor samouraï (couleurs principales)")]
        public Color boisDojoMid = new Color(0.725f, 0.435f, 0.227f, 1f);     // #B96F3A
        public Color boisDojoDark = new Color(0.561f, 0.310f, 0.180f, 1f);    // #8F4F2E
        public Color boisDojoLight = new Color(0.886f, 0.631f, 0.361f, 1f);   // #E2A15C
        public Color toitJaponaisMid = new Color(0.204f, 0.361f, 0.612f, 1f); // #345C9C
        public Color toitJaponaisDark = new Color(0.141f, 0.239f, 0.451f, 1f);// #243D73
        public Color sakuraMid = new Color(1.000f, 0.694f, 0.820f, 1f);       // #FFB1D1
        public Color sakuraDark = new Color(1.000f, 0.471f, 0.682f, 1f);      // #FF78AE
        public Color montagneMid = new Color(0.612f, 0.769f, 0.847f, 1f);     // #9CC4D8
        public Color montagneDark = new Color(0.431f, 0.588f, 0.702f, 1f);    // #6E96B3

        [Header("DA Palette — section 7.2 héros samouraï")]
        public Color heroTenue = new Color(0.184f, 0.243f, 0.471f, 1f);       // #2F3E78
        public Color heroTenueOmbre = new Color(0.114f, 0.153f, 0.310f, 1f);  // #1D274F
        public Color heroCeinture = new Color(0.910f, 0.290f, 0.290f, 1f);    // #E84A4A

        [Header("DA Palette — section 10.3 raretés")]
        public Color rarityCommunDA = new Color(0.725f, 0.757f, 0.831f, 1f);  // #B9C1D4
        public Color rarityRareDA = new Color(0.396f, 0.784f, 1.000f, 1f);    // #65C8FF
        public Color rarityEpiqueDA = new Color(0.604f, 0.482f, 1.000f, 1f);  // #9A7BFF
        public Color rarityLegendaireDA = new Color(1.000f, 0.847f, 0.302f, 1f); // #FFD84D
        public Color rarityMythiqueDA = new Color(1.000f, 0.420f, 0.420f, 1f);   // #FF6B6B

        // ----- Voies -----
        [Header("Voies")]
        public Color voieSamurai = new Color(0.980f, 0.780f, 0.459f, 1f);   // #FAC775
        public Color voieViking = new Color(0.490f, 0.655f, 0.788f, 1f);    // #7DA7C9
        public Color voieWuxia = new Color(0.624f, 0.749f, 0.659f, 1f);     // #9FBFA8
        public Color voieSpartiate = new Color(0.788f, 0.475f, 0.290f, 1f); // #C9794A
        public Color voieMongol = new Color(0.722f, 0.612f, 0.416f, 1f);    // #B89C6A
        public Color voieSaladin = new Color(0.910f, 0.722f, 0.361f, 1f);   // #E8B85C
        public Color voieAztec = new Color(0.753f, 0.231f, 0.169f, 1f);     // #C03B2B
        public Color voieGaulois = new Color(0.361f, 0.541f, 0.310f, 1f);   // #5C8A4F

        // ----- Raretés -----
        [Header("Rarities")]
        public Color rarityCommun = new Color(0.722f, 0.690f, 0.627f, 1f);
        public Color rarityAffute = new Color(0.490f, 0.655f, 0.788f, 1f);
        public Color rarityLegendaire = new Color(0.980f, 0.780f, 0.459f, 1f);
        public Color rarityMythique = new Color(0.753f, 0.231f, 0.169f, 1f);
        public Color raritySacre = new Color(0.600f, 0.235f, 0.114f, 1f);   // #993C1D

        // ----- Spacing (px @ 1080×1920) -----
        [Header("Spacing")]
        public int spacingXs = 4;
        public int spacingSm = 8;
        public int spacingMd = 16;
        public int spacingLg = 24;
        public int spacingXl = 32;
        public int spacingXxl = 48;

        // ----- Radii (Vibrant Quest puffy : sm/buttons 16, panels 24) -----
        [Header("Border Radii")]
        public int radiusSmall = 12;
        public int radiusMedium = 16;
        public int radiusLarge = 24;

        // ----- Puffy 3D (Vibrant Quest depth recipe) -----
        [Header("Puffy 3D")]
        [Tooltip("Charcoal outline thickness around puffy elements (px).")]
        public int puffyOutline = 3;
        [Tooltip("3D bottom-border 'floor' height for buttons (px).")]
        public int puffyFloorButton = 6;
        [Tooltip("3D bottom-border 'floor' height for cards/list items (px).")]
        public int puffyFloorCard = 5;

        // ----- Typography scale (font sizes @ 1080×1920 ref) -----
        // Renamed fontDisplay → fontSizeDisplay to avoid collision with the TMP_FontAsset fontDisplay slot.
        [Header("Typography Scale")]
        public int fontSizeDisplay = 48;
        public int fontH1 = 32;
        public int fontH2 = 24;
        public int fontH3 = 20;
        public int fontBody = 16;
        public int fontCaption = 14;
        public int fontSmall = 12;

        // ----- Font assets (nullable — fall back to TMP default) -----
        [Header("Font Assets (Saga > Design > Generate Font Assets)")]
        [Tooltip("Plus Jakarta Sans — primary UI font (labels, descriptions, body). Google Fonts.")]
        public TMP_FontAsset fontPrimary;
        [Tooltip("Lilita One — display font (big titles, button labels, cartoon punch). Google Fonts.")]
        public TMP_FontAsset fontDisplay;
        [Tooltip("JetBrains Mono — numbers (Force, HP, Élan%, levels, costs). Google Fonts.")]
        public TMP_FontAsset fontNumbers;
        [Tooltip("Cinzel — lore (citations Maîtres). Optional — dropped from the core stack Sprint 7.5.")]
        public TMP_FontAsset fontLore;

        /// <summary>Returns the configured primary font or TMP default if unassigned.</summary>
        public TMP_FontAsset PrimaryFont => fontPrimary != null ? fontPrimary : TMP_Settings.defaultFontAsset;
        public TMP_FontAsset DisplayFont => fontDisplay != null ? fontDisplay : (fontPrimary != null ? fontPrimary : TMP_Settings.defaultFontAsset);
        public TMP_FontAsset NumbersFont => fontNumbers != null ? fontNumbers : TMP_Settings.defaultFontAsset;
        public TMP_FontAsset LoreFont => fontLore != null ? fontLore : TMP_Settings.defaultFontAsset;

        // ----- Voie / Rarity lookup helpers -----

        public Color VoieColor(Voie v)
        {
            switch (v)
            {
                case Voie.Samurai: return voieSamurai;
                case Voie.Viking: return voieViking;
                case Voie.Wuxia: return voieWuxia;
                case Voie.Spartiate: return voieSpartiate;
                case Voie.Mongol: return voieMongol;
                case Voie.Saladin: return voieSaladin;
                case Voie.Aztec: return voieAztec;
                case Voie.Gaulois: return voieGaulois;
                default: return textSecondary;
            }
        }

        /// <summary>
        /// Darker shade of a color for the puffy 3D bottom-border ("floor"). Default -20% luminance
        /// with a touch more saturation, matching the Vibrant Quest spec ("20% darker, increased sat").
        /// </summary>
        public static Color Darken(Color c, float percent = 0.20f)
        {
            var f = 1f - Mathf.Clamp01(percent);
            return new Color(c.r * f, c.g * f, c.b * f, c.a);
        }

        public Color RarityColor(Rarity r)
        {
            switch (r)
            {
                case Rarity.Commun: return rarityCommun;
                case Rarity.Affute: return rarityAffute;
                case Rarity.Legendaire: return rarityLegendaire;
                case Rarity.Mythique: return rarityMythique;
                case Rarity.Sacre: return raritySacre;
                default: return textSecondary;
            }
        }

        /// <summary>
        /// Tier color for the Force counter — readable telegraph of magnitude reached.
        /// 0..1K text_secondary, 1K..100K text_primary, 100K..10M accent, 10M+ accent (caller adds glow).
        /// </summary>
        public Color ForceTierColor(BreakInfinity.BigDouble force)
        {
            if (force < new BreakInfinity.BigDouble(1000)) return textSecondary;
            if (force < new BreakInfinity.BigDouble(100_000)) return textPrimary;
            if (force < new BreakInfinity.BigDouble(10_000_000)) return accentPrimary;
            return accentPrimary; // 10M+ : caller adds glow on top.
        }

        /// <summary>
        /// Adaptive font size for the Force counter based on magnitude (so big numbers don't truncate).
        /// </summary>
        public int ForceFontSize(BreakInfinity.BigDouble force)
        {
            if (force < new BreakInfinity.BigDouble(1_000_000)) return 96;
            if (force < new BreakInfinity.BigDouble(1_000_000_000)) return 84;
            if (force < new BreakInfinity.BigDouble(1_000_000_000_000L)) return 72;
            return 64;
        }

        private static DesignTokens CreateRuntimeFallback()
        {
            // Identical defaults to the field initializers — Resources missing should still be playable.
            return CreateInstance<DesignTokens>();
        }
    }
}
