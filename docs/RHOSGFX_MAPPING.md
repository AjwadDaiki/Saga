# Mapping SAGA → RhosGFX (Sprint 7.6)

> Proposition d'attribution sprite RhosGFX pour chaque élément UI SAGA actuel (Phase 4 LAYOUT validée 2026-05-30).
> **Statut** : proposition en attente validation coordinateur (Ajwad) → STOP avant intégration.
> **Palette DA SAGA** : Or `#F5C842`, Violet `#8B5CF6`, Cyan `#22D3EE`, Vert menthe `#4ADE80`, Rouge `#EF4444`, Brun `#3A2A1E`, Sable `#D6BBA0`, Encre `#1F1B16`.

Légende : ✏️ = tint runtime nécessaire (DA SAGA ≠ couleur RhosGFX exacte).

---

## 1. Top HUD (zone 1 — 154 px)

| Élément SAGA           | Sprite RhosGFX                                       | Couleur | Taille recommandée   | Notes                                      |
|------------------------|------------------------------------------------------|---------|----------------------|--------------------------------------------|
| **Force pill (or)**    | `Buttons/3D/Round/3. Yellow/button-round-3d-2.5-yellow_standard.png` | Yellow (or DA) ✏️ | 200×80 (9-slice horiz) | Tint sur DA Or `#F5C842` si jaune trop pop |
| Force icon             | `Icons/Coin 2/Coin 2 Gold 64.png`                    | Gold    | 48×48                | Centré, leftMargin label 56→60             |
| Force label `1.2K`     | TMP Lilita One bold blanc + outline                  | —       | 32 sp                | Inchangé (texte > sprite)                  |
| **Échos pill (violet)**| `Buttons/3D/Round/7. Purple/button-round-3d-2.5-purple_standard.png` | Purple ✏️ | 200×80 (9-slice horiz) | Tint sur DA Violet `#8B5CF6`             |
| Échos icon             | `Icons/Gem/Gem 64.png` (re-tinted violet ✏️)         | Violet  | 48×48                | Re-tint via Image.color                    |
| **Settings round btn** | `Buttons/3D/Round/1. Grey/button-round-3d-1-grey_standard.png` | Grey    | 80×80                | Compact 3D rond                            |
| Settings icon          | `Icons/Settings 2/Gear 2 Outline 64.png`             | Blanc   | 48×48                | Outline blanc sur grey button              |

> **Pourquoi 3D Round 2.5 pour pills** : la taille `2.5` est plus large que `1` → meilleur fit pour pills horizontales 200×80. `_standard` au repos, switch sur `_pressed` au TouchDown.

---

## 2. Stage Band (zone 2 — 96 px)

| Élément SAGA           | Sprite RhosGFX                                  | Couleur          | Taille                | Notes                                       |
|------------------------|-------------------------------------------------|------------------|-----------------------|---------------------------------------------|
| **Stage chip**         | `Containers/3D/3. Yellow/container-3d-yellow.png` | Yellow (or) ✏️ | 240×60 (9-slice)      | Or DA. Sub-frame chip pour skull + label    |
| Stage chip skull       | `Icons/Skull/Skull 64.png` (re-tint blanc/encre) | Blanc/Encre ✏️ | 48×48                 | Position left chip                          |
| Stage label `Stade 12` | TMP Lilita One bold 36 sp                       | Encre `#1F1B16`  | —                     | Texte sombre sur fond or                    |
| **Boss progress bar**  | `Bars/Regular/2. Red/progress-container-regular-red.png` (container) + `Bars/Regular/2. Red/progress-bar-regular-red.png` (fill) | Red | 760×24 (9-slice horiz) | Container fixe + fill animé par boss HP %  |
| Boss skull (droite)    | `Icons/Skull/Skull 64.png`                      | Rouge ✏️         | 48×48                 | Tint danger Rouge `#EF4444`                |

> **Astuce TIPS.txt** : si on veut **effet 3D feuilleté** sur la bar (premium look), utiliser `Bars/Regular` sur `Bars/Wide` container, fill aligné top → à explorer Phase 7.6-E.

---

## 3. Combat (zone 3 — 960 px)

| Élément SAGA           | Sprite RhosGFX                       | Notes                            |
|------------------------|--------------------------------------|----------------------------------|
| Background dojo        | (Phase 3 procédural conservé)        | Pas de remplacement RhosGFX      |
| Perso chibi (1.5×)     | (sprite SAGA existant)               | RhosGFX = UI only, pas character |
| Mannequin (0.4×)       | (sprite SAGA existant)               | Idem                             |
| Combo overlay puffy    | `Frames/Pointed/3. Yellow/frame-pointed-yellow.png` ✏️ | Optionnel : encadrer combo number multiplier |
| Damage numbers         | TMP DOTween (existant)               | Inchangé                          |

> **Phase 4-E candidat** : ajuster encore perso scale (1.5 → 1.3 ?) si Ajwad le souhaite après screenshot comparatif.

---

## 4. Upgrades Panel (zone 4 — 288 px)

| Élément SAGA           | Sprite RhosGFX                                       | Couleur                | Taille                | Notes                                       |
|------------------------|------------------------------------------------------|------------------------|-----------------------|---------------------------------------------|
| Panel background       | `Containers/Flat/9. Brown/container-flat-brown.png` ✏️ | Brun dojo `#3A2A1E`   | 1032×288 (9-slice)    | Flat car derrière cards = arrière-plan      |
| **Card frame**         | `Frames/Basic/9. Brown/frame-basic-brown.png` ✏️    | Brun + accent voie ✏️ | 333×248 (9-slice)     | 3 cards horizontales (Strike/Focus/Power)   |
| Card icon container    | `Buttons/3D/Round/3. Yellow/button-round-3d-1-yellow_standard.png` ✏️ | Or DA ✏️ | 96×96                | Cercle 3D doré, glyph dessus                |
| Card icon glyph        | `Icons/Sword/Sword 64.png` (Strike)<br>`Icons/Gem/Gem 64.png` (Focus)<br>`Icons/Trophy/Trophy 64.png` (Power) | Encre `#1F1B16` ✏️ | 64×64 | Tint sombre sur or button |
| Card name label        | TMP Lilita One bold 28 sp                            | Blanc + outline encre  | —                     |                                             |
| Card level             | TMP Plus Jakarta Sans 20 sp                          | Sable `#D6BBA0`        | —                     |                                             |
| Card effect            | TMP Plus Jakarta Sans 22 sp                          | Sable                  | —                     |                                             |
| **Cost pill**          | `Buttons/Flat/Square/3. Yellow/button-square-flat-2.5-yellow_standard.png` ✏️ | Or DA ✏️ | (cardWidth-16)×48 (9-slice) | Flat Square car CTA secondaire |
| Cost icon              | `Icons/Coin 2/Coin 2 Gold 64.png`                    | Gold                   | 32×32                 |                                             |

---

## 5. Skills Row (zone 5 — 230 px)

| Élément SAGA           | Sprite RhosGFX                                       | Couleur            | Taille                | Notes                                            |
|------------------------|------------------------------------------------------|--------------------|-----------------------|--------------------------------------------------|
| **VAGUE button**       | `Buttons/3D/Square/6. Blue/button-square-3d-2.5-blue_standard.png` ✏️ | Cyan DA `#22D3EE` ✏️ | 680×180 (9-slice)     | Square 2.5 large pour CTA principal              |
| VAGUE icon             | `Icons/Potion 1/Potion 1 64.png` ou custom wave SVG  | Cyan/Blanc ✏️      | 64×64                 | Optionnel — peut rester texte seul               |
| VAGUE label `VAGUE`    | TMP Lilita One 48 sp bold + outline                  | Blanc + outline    | —                     | Centré sur button                                |
| VAGUE Élan jauge       | `Bars/Thin/0. White/progress-bar-thin-white.png` (fill) + container White | Blanc fill, contour transparent | 600×16 (9-slice horiz) | **Overlay interne** au button (préserver feature) — anchored bottom, marge 16 |
| **SOUFFLE button**     | `Buttons/3D/Square/5. Forest Green/button-square-3d-2.5-forest-green_standard.png` ✏️ | Vert menthe DA `#4ADE80` ✏️ | 280×180 (9-slice)     | Square 2.5 compact (asymétrie validée Phase 4)    |
| SOUFFLE icon           | `Icons/Heart/Heart Outline 64.png` ou custom         | Blanc ✏️           | 64×64                 | Optionnel                                        |
| SOUFFLE label          | TMP Lilita One 32 sp bold + outline                  | Blanc + outline    | —                     |                                                  |

> **Feature critique préservée** : VAGUE Élan jauge interne **doit rester fonctionnelle** (fill % piloté par GameEvents.OnElanChanged). Wire le RhosGFX bar dans le button comme overlay.

---

## 6. Bottom Nav (zone 6 — 192 px)

| Élément SAGA           | Sprite RhosGFX                                       | Couleur            | Taille                | Notes                                            |
|------------------------|------------------------------------------------------|--------------------|-----------------------|--------------------------------------------------|
| **Nav background**     | `Containers/Flat/9. Brown/container-flat-brown.png` ✏️ | Brun dojo opacité 0.6 ✏️ | 1048×192 (9-slice horiz) | Inset 16 latéral. Brun + alpha pour ne pas voler l'œil |
| **Tab inactive**       | `Buttons/Flat/Round/1. Grey/button-round-flat-1-grey_standard.png` ✏️ | Brun foncé / transparent ✏️ | 200×156 (proche carré rond) | Round flat = discret                            |
| **Tab actif (Dojo)**   | `Buttons/3D/Round/3. Yellow/button-round-3d-1-yellow_standard.png` ✏️ | Or DA ✏️           | 200×156 (lift +12 + scale 1.05) | 3D Round = saillance, état "tu es ici"          |
| Tab icon (Dojo)        | `Icons/Home 2/Home 2 64.png` (re-tint encre)         | Encre `#1F1B16`    | 80×80                 | Tu changes Home 2 Blue par tint                  |
| Tab icon (Voies)       | `Icons/Map/Map 64.png`                               | Sable ✏️           | 80×80                 |                                                  |
| Tab icon (Inventaire)  | `Icons/Backpack/Backpack 64.png`                     | Sable ✏️           | 80×80                 |                                                  |
| Tab icon (Reliques)    | `Icons/Chest 2/Chest 2 64.png`                       | Sable ✏️           | 80×80                 |                                                  |
| Tab icon (Hall)        | `Icons/Trophy/Trophy 64.png`                         | Sable ✏️           | 80×80                 |                                                  |
| Tab label              | TMP Plus Jakarta Sans 24 sp medium                   | Encre sur actif, sable sur inactif | —     |                                                  |

---

## 7. Autres (Modals, Toasts, HUD secondaire)

| Élément SAGA              | Sprite RhosGFX                                       | Notes                                            |
|---------------------------|------------------------------------------------------|--------------------------------------------------|
| **Toast Coming Soon**     | `Frames/Basic/1. Grey/frame-basic-grey.png` ✏️       | 9-slice. Texte centré + Clock icon left          |
| Toast icon                | `Icons/Clock/Clock 64.png`                           | Sable                                            |
| **Modal background**      | `Containers/3D/9. Brown/container-3d-brown.png` ✏️   | 9-slice, alpha 0.95                              |
| Modal frame border        | `Frames/Inset/9. Brown/frame-inset-brown.png` ✏️     | Encapsule modal content                          |
| **Modal CTA button**      | `Buttons/3D/Square/4. Green/button-square-3d-2.5-green_standard.png` ✏️ | Validation, "Affronter Maître"   |
| **Modal close X**         | `Icons/X Button/X Button 64.png`                     | Top-right modal                                  |
| **Damage number bg**      | (laisser TMP outline existant)                       | RhosGFX overkill ici                             |
| **Citation Maître scroll**| `Frames/Ornate/9. Brown/frame-ornate-brown.png` ✏️   | Décoratif pour intro cinematic                   |
| **Hall des Légendes entry**| `Frames/Nailed/9. Brown/frame-nailed-brown.png` ✏️  | Death record avec clous décoratifs               |

---

## 8. Catalogue SO (Phase 7.6-D)

Proposition `RhosGFXAssetCatalog.cs` ScriptableObject avec sections :

```csharp
[CreateAssetMenu(menuName = "Saga/RhosGFX Asset Catalog")]
public class RhosGFXAssetCatalog : ScriptableObject
{
    [Header("Buttons 3D Round")]
    public ButtonStateSet roundYellow3D_25;   // 4 states
    public ButtonStateSet roundPurple3D_25;
    public ButtonStateSet roundGrey3D_1;
    public ButtonStateSet roundYellow3D_1;

    [Header("Buttons 3D Square")]
    public ButtonStateSet squareBlue3D_25;
    public ButtonStateSet squareForestGreen3D_25;
    public ButtonStateSet squareGreen3D_25;

    [Header("Buttons Flat Square")]
    public ButtonStateSet squareYellowFlat_25;

    [Header("Buttons Flat Round")]
    public ButtonStateSet roundGreyFlat_1;

    [Header("Containers")]
    public Sprite container3DYellow;
    public Sprite containerFlatBrown;
    public Sprite container3DBrown;

    [Header("Bars Regular")]
    public Sprite barRegularRedContainer;
    public Sprite barRegularRedFill;
    public Sprite barThinWhiteFill;

    [Header("Frames")]
    public Sprite frameBasicBrown;
    public Sprite frameBasicGrey;
    public Sprite frameInsetBrown;
    public Sprite frameOrnateBrown;
    public Sprite frameNailedBrown;
    public Sprite framePointedYellow;

    [Header("Icons (64px)")]
    public Sprite iconCoinGold;
    public Sprite iconGem;
    public Sprite iconGearOutline;
    public Sprite iconSkull;
    public Sprite iconHome;
    public Sprite iconMap;
    public Sprite iconBackpack;
    public Sprite iconChest;
    public Sprite iconTrophy;
    public Sprite iconSword;
    public Sprite iconHeartOutline;
    public Sprite iconClock;
    public Sprite iconXButton;
    public Sprite iconScroll;
    public Sprite iconPotion;
}

[System.Serializable]
public struct ButtonStateSet
{
    public Sprite standard;
    public Sprite hover;
    public Sprite focus;
    public Sprite pressed;
}
```

Stockage : `Assets/_Project/Resources/UI/RhosGFXCatalog.asset`. Chargé une seule fois au boot.

---

## 9. Contraintes préservées

- ✅ Gameplay (services, GameEvents, modulaire) **inchangé**
- ✅ SaveService versionning **inchangé**
- ✅ Élan jauge interne VAGUE → wire RhosGFX thin bar comme overlay (feature préservée)
- ✅ DesignTokens palette → continue à piloter les **tints**, RhosGFX fournit les **formes**
- ✅ 138 tests EditMode doivent rester verts

---

## 10. STOP — Validation coordinateur requise

Avant de passer en Phase 7.6-D (intégration) :

**Questions ouvertes pour Ajwad / Coordinateur** :

1. **Direction "3D Round" pour currency pills** vs Flat Square : OK ?
2. **Tint runtime systématique** des couleurs pour matcher DA SAGA (Or `#F5C842` ≠ Yellow RhosGFX, Cyan `#22D3EE` ≠ Blue RhosGFX) : accepter overhead Image.color = DA ?
3. **VAGUE Élan jauge interne** comme overlay Thin Bar : OK ? Ou autre approche ?
4. **Cards Upgrades** : Frame Basic Brown + Container Flat fond ou Frame seul sans container ?
5. **Bottom Nav** : tabs inactifs = Round Flat Brown vs vraiment transparent. Lequel ?
6. **Tab actif Dojo** : Or 3D Round avec lift +12 px conservé Phase 4 — OK avec sprite jaune ?
7. **Toast Coming Soon** : Frame Basic Grey assez discret ou trop neutre ?
8. **Cards icons glyphs** : Sword/Gem/Trophy pour Strike/Focus/Power → OK ou autre mapping ?

→ Une fois validé, Phase 7.6-D peut commencer (RhosGFXAssetCatalog + Builders modif).
