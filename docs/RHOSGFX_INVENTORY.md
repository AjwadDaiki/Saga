# RhosGFX Cartoony UI Pack — Inventaire

> Auteur : Rhos (rhosgfx.itch.io)
> Pack : Cartoony UI Pack — Full edition (toutes exports inclus)
> Localisation : `Assets/_Project/Art/External/cartoony-ui-pack-full/`
> Format : PNG (raster) + SVG (vector source). Texte cartoony chaleureux, contours noirs, ombres puffy.
> Conçu pour 9-slice resize (sprites tiled / preserve corners).

---

## 1. Palette couleurs disponible

Chaque catégorie majeure (Buttons / Bars / Containers / Frames / Widgets) propose **10 couleurs** :

| Index | Nom         | Usage SAGA candidat                |
|-------|-------------|------------------------------------|
| 0     | White       | Neutre, toast, modal               |
| 1     | Grey        | Désactivé, Settings, fond carte    |
| 2     | Red         | Boss bar, danger, dégâts           |
| 3     | Yellow      | Force (or), Stage chip jaune       |
| 4     | Green       | Souffle, validation, low-risk      |
| 5     | Forest Green| Souffle alt, nature                |
| 6     | Blue        | Vague, info                        |
| 7     | Purple      | Échos (violet), prestige           |
| 8     | Pink        | Cosmétique, événement              |
| 9     | Brown       | Bois/dojo, rustique                |

> Les SVG sources permettent re-tinting on-demand si une couleur exacte de la DA SAGA manque.

---

## 2. Buttons — `cartoony-ui-pack-full/Buttons/`

### Structure

```
Buttons/
├── 3D/          ← effet relief (recommandé Skills, Top HUD, Bottom Nav, Currency)
│   ├── Round/   ← pills, tabs, currency
│   └── Square/  ← cards, Settings, action buttons
└── Flat/        ← effet plat (recommandé Cost pill, sub-actions)
    ├── Round/
    └── Square/
```

Chaque dossier `{Style}/{Shape}/` contient les **10 couleurs** ci-dessus.

### Variantes par bouton

- **2 tailles** par bouton : `1` (compact) et `2.5` (large) — utile pour pills vs CTA majeur.
- **4 états** par bouton : `_standard`, `_hover`, `_focus`, `_pressed` — wire to SagaButton interaction states.

### Exemple naming

```
button-round-3d-2.5-white_standard.png
button-round-3d-2.5-white_hover.png
button-round-3d-2.5-white_focus.png
button-round-3d-2.5-white_pressed.png
button-round-3d-1-yellow_standard.png
button-square-flat-2.5-blue_standard.png
```

### Volume

2 styles × 2 shapes × 10 couleurs × 2 tailles × 4 états = **320 PNG** + 320 SVG.

---

## 3. Bars — `cartoony-ui-pack-full/Bars/`

### Sous-catégories

| Folder    | Contenu                                | Usage SAGA candidat        |
|-----------|----------------------------------------|----------------------------|
| Regular   | container + fill bar 10 couleurs       | Boss progress, Élan jauge  |
| Wide      | container + fill bar 10 couleurs       | Boss bar large, XP bar     |
| Thin      | container + fill bar (qq couleurs)     | Scrollbar, sub-bar         |
| Handles   | handles round / square                 | Slider (Settings volume)   |

### Structure par couleur

```
Bars/Regular/0. White/
├── progress-bar-regular-white.{png,svg}        ← fill (intérieur)
└── progress-container-regular-white.{png,svg}  ← contour (extérieur)
```

### Astuce DA (TIPS.txt)

- Vertical Thin bar = scrollbar
- Bar + Handle = slider Settings
- Regular bar sur Wide container (alignée top) = **effet 3D feuilleté** → potentiel pour Boss bar premium.

---

## 4. Containers — `cartoony-ui-pack-full/Containers/`

### Sous-catégories

- `3D/` — relief panneau (header, modal background)
- `Flat/` — fond uni discret (sub-section, list item)

### Structure

```
Containers/{Style}/{Color}/container-{style}-{color}.{png,svg}
```

1 sprite par couleur (conçu 9-slice — étirable).

---

## 5. Frames — `cartoony-ui-pack-full/Frames/`

### 7 styles décoratifs

| Style          | Look                                    | Usage SAGA candidat                  |
|----------------|-----------------------------------------|--------------------------------------|
| Basic          | contour simple, cartoony round          | Cards Upgrades, Modal pop-up         |
| Inset          | bord renfoncé                            | Inventaire slot, Stage chip          |
| Nailed         | clous décoratifs aux coins              | Achievement, Maître unlock           |
| Ornate         | volutes décoratives                     | Reliques rare, légendaires           |
| Pointed        | pointes/triangles                        | Voie sélection, combat               |
| Square Corners | contour anguleux                        | Tech/HUD, sub-modal                  |
| Thin           | contour fin                              | Hint, tooltip                        |

1 sprite par couleur, conçu 9-slice.

---

## 6. Widgets — `cartoony-ui-pack-full/Widgets/`

| Folder        | Contenu                                | Usage SAGA candidat               |
|---------------|----------------------------------------|-----------------------------------|
| Checkboxes    | check on/off, 10 couleurs              | Settings options, toggle anim     |
| Radio Buttons | radio on/off, 10 couleurs              | Voie sélection (1 parmi N)        |
| Toggles       | switch on/off, 10 couleurs             | Sound on/off, haptic on/off       |
| Handles       | round / square horizontal / vertical   | Slider Settings                   |
| Cursor        | curseurs souris                        | Web build / Editor preview only   |

---

## 7. Icons — `cartoony-ui-pack-full/[THANK YOU!] Icons/`

### 25 icônes catalogue

Backpack, Bell, Cash, Chest 2, Clock, Coin 2, Friends 2, Gem, Heart, Home 2, Key, Map, Mouse, Potion 1, Power Cell, Scroll, Settings 2 (Gear 2), Shopping Basket, Skull, Sound, Sword, Trophy, X Button.

### Variantes par icône

- **2 tailles PNG** : `64` (HUD/HD icon size) + `256` (preview / large display).
- **2 variantes** : standard + **Outline** (contour seul, tint au runtime).
- **SVG source** disponible pour re-tinting illimité.
- **Heart** a variante supplémentaire `Empty` (cœur vide pour life HUD).
- **Home 2** a variante `Blue` (déjà tintée).

### Naming

```
Coin 2 Gold 256.png
Coin 2 Gold Outline.svg
Skull 64.png
Gear 2 256.png
Heart Empty 64.png
```

### Icons SAGA candidats clairs

| Élément SAGA   | Icon RhosGFX              |
|----------------|---------------------------|
| Force (or)     | Coin 2 Gold               |
| Échos (violet) | Gem (re-tint purple)      |
| Settings       | Gear 2 (Settings 2)       |
| Boss skull     | Skull                     |
| Inventaire     | Backpack ou Chest 2       |
| Dojo (home)    | Home 2                    |
| Sound toggle   | Sound                     |
| Cards Strike   | Sword                     |
| Cards Focus    | Power Cell ou Gem         |
| Cards Power    | Trophy ou Power Cell      |
| Achievement    | Trophy                    |
| Citation/lore  | Scroll                    |
| Coming Soon    | Clock                     |
| Maître unlock  | Key ou Chest 2            |
| Vague potion?  | Potion 1                  |

---

## 8. Constat 9-slice

Conformément au README, **les sprites RhosGFX sont conçus pour être resize** :
- Buttons / Containers / Frames / Bars → 9-slice (preserve corners, tile center).
- Icons → 1:1 (pas de stretch, garder ratio carré 64/256).
- Unity import settings nécessaires : `Sprite Mode = Single`, `Mesh Type = Tight` ou `Full Rect`, configurer **Border** via Sprite Editor.

---

## 9. Volume total (estimation)

- Buttons : ~640 fichiers PNG+SVG (320 PNG + 320 SVG)
- Bars : ~80 (containers + bars + handles)
- Containers : ~40 (2 styles × 10 couleurs × 2 formats)
- Frames : ~140 (7 styles × 10 couleurs × 2 formats)
- Widgets : ~120
- Icons : ~150+ (25 × variantes × tailles × formats)

**Total > 1100 fichiers**, mais ne charger en Resources/Addressables que ce que SAGA utilise réellement (mapping cible Sprint 7.6 D).

---

## 10. Conclusion audit

Le pack couvre **100 % des besoins UI SAGA actuels** :
- ✅ Currency pills (Force, Échos) → Buttons 3D Round
- ✅ Action buttons (Vague, Souffle) → Buttons 3D Square
- ✅ Upgrades cards → Frames Basic/Inset + Container Flat
- ✅ Cost pill → Buttons Flat Square
- ✅ Boss progress bar → Bars Regular ou Wide
- ✅ Stage chip → Container 3D + Skull icon
- ✅ Bottom nav tabs → Buttons Round Flat (état actif = 3D)
- ✅ Settings button → Buttons Round 3D + Gear icon
- ✅ Toast Coming Soon → Frame Basic + Clock icon

Pas de gap. Prochaine étape : **docs/RHOSGFX_MAPPING.md** avec sprite exact + couleur + taille par élément SAGA.
