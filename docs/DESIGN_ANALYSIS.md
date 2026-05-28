# DESIGN ANALYSIS — Refonte UI SAGA (Tactile & Bold / Puffy 3D)

> Analyse des 3 références fournies par Ajwad avant la refonte UI Sprint 7.5+.
> Auteur : dev Claude (Opus 4.7). Date : 2026-05-28.
> **Ce document ne contient AUCUN code de prod.** C'est la spec de référence pour la refonte.

## Sources analysées

| # | Source | Localisation | Type | Rôle |
|---|--------|--------------|------|------|
| 1 | **Claude Design** | `idle/` (app.jsx, design-canvas.jsx, screens/shared.jsx 344L, combat.jsx 596L, equipement.jsx 567L) + 3 uploads PNG | JSX + SVG | STRUCTURE + sprites SVG (chibis + 18 icônes) |
| 2 | **Google Stitch** | `stitch_saga_echoes_of_legends/` (4 écrans : dojo, hall, boutique, équipement — code.html + screen.png chacun) | HTML/Tailwind + PNG | MOOD + 4 écrans complets |
| 3 | **Vibrant Quest** | `stitch_saga_echoes_of_legends/vibrant_quest/DESIGN.md` (171L) + le dojo code.html (254L) | Spec Material 3 + écran Tailwind | DESIGN SYSTEM COMPLET "Tactile & Bold" |

Les 3 convergent vers le **même langage visuel** : *puffy 3D / sticker / squishy*. C'est une chance — pas de conflit fondamental, juste des nuances de palette et de polices à trancher.

---

## A. LE STYLE "TACTILE & BOLD / PUFFY 3D" (le cœur)

C'est LA signature. 5 ingrédients, tous reproductibles en Unity uGUI sans shader.

### A1. Structural offsets (3D bottom borders) — PAS d'ombres molles

La profondeur vient d'un **"plancher" solide** sous chaque élément, jamais d'une ombre gaussienne.

- **Boutons** : `border-b-[6px]` (6px) d'une teinte 20% plus sombre que le fill.
- **Cards/list items** : `border-b-[5px]` (5px).
- **Petits éléments (icon containers, chips)** : `shadow-[0_3px_0_...]` ou `border-b` 3-4px.
- **Nav active** : `shadow-[0_4px_0_rgba(0,74,120,1)]`.

Tailwind exact (dojo code.html) :
```
border-[3px] border-on-surface border-b-[6px]      // bouton START
border-[3px] border-outline border-b-[5px]         // upgrade card
shadow-[0_4px_0_rgba(109,123,105,1)]               // currency pill
shadow-[0_3px_0_rgba(22,29,31,1)]                  // icon container
shadow-[0_6px_0_rgba(22,29,31,1)]                  // avatar (plus profond)
```

Claude Design (shared.jsx) confirme avec `boxShadow` empilés :
```
boxShadow: `0 5px 0 ${shadow}, 0 8px 0 ${T.ink}`   // PuffyButton : double plancher (couleur + outline)
boxShadow: `0 4px 0 ${T.ink}`                      // Card
```

> **Le double-plancher du PuffyButton est la version la plus riche** : un premier offset coloré (teinte sombre du fill), puis un second offset à la couleur d'outline (ink). Donne une vraie tranche 3D. À adopter pour les boutons primaires.

### A2. Heavy outlines (charcoal, PAS noir pur)

- Épaisseur : **3px** sur containers/boutons, **2-2.5px** sur petits éléments, **1.5px** sur détails.
- Couleur : **`#161d1f`** (Vibrant Quest `on-surface`) ou **`#1F1B2E`** (Claude Design `T.ink`). Les deux sont des charcoals quasi-noirs avec un soupçon de teinte (vert-bleuté vs violet). **Jamais `#000000`.**
- Appliqué partout : pills, cards, boutons, icon containers, barres de progression.

### A3. Internal shines (gloss spéculaire)

Ovale/bande blanche semi-transparente en **haut-gauche**, légèrement tournée, pour l'effet "jouet brillant".

Dojo code.html :
```html
<div class="absolute top-1 left-2 w-12 h-4 bg-white/20 rounded-full rotate-[-15deg]"></div>
```
Vibrant Quest CSS :
```css
.shine-highlight { top:4px; left:8px; width:30%; height:25%; background:rgba(255,255,255,0.4); border-radius:9999px; transform:rotate(-10deg); }
```
Claude Design PuffyButton :
```js
// gloss : linear-gradient(180deg, rgba(255,255,255,0.55), rgba(255,255,255,0)), top 4 left 8 right 8 height 30%
```

> Deux variantes : **ovale tourné** (-10° à -15°, top-left) pour les boutons, ou **bande gradient horizontale** (top, pleine largeur) pour barres de progression et fills. On utilisera les deux.

### A4. Pressed state (le clic physique)

L'élément descend de la hauteur de son plancher, et le plancher disparaît → sensation de "appui".

```css
.btn-press:active   { transform: translateY(4px); border-bottom-width: 0 !important; margin-bottom: 4px; }
.squishy-btn:active { transform: translateY(4px); border-bottom-width: 0px !important; margin-bottom: 6px; }
```
Tailwind inline : `active:border-b-[3px] active:translate-y-[3px]` (réduit le plancher au lieu de le supprimer — variante plus douce).
Nav active : `active:translate-y-[4px] active:shadow-none`.

> **Le `margin-bottom` compensatoire est crucial** : il évite le layout shift quand le plancher disparaît. En Unity, on simule via DOTween : `translateY` du RectTransform + réduction du "floor" enfant. Durée brève (`duration-75` = 75ms).

### A5. Rounded (rayons)

| Élément | Vibrant Quest | Claude Design | Verdict SAGA |
|---------|---------------|---------------|--------------|
| Petits (chips/mini-btn) | 16px (`rounded-lg`=1rem) | 10-14px | **12-14px** |
| Boutons standard | 16px | 16-18px | **16px** |
| Cards/list items | — | 14-18px | **16px** |
| Panels/modals | 24px (`rounded-xl`=1.5rem) | 18-22px | **22-24px** |
| Pills/barres | full (9999px) | full | **full** |
| Icon containers | full (cercle) | 12px (rounded-square) | **cercle pour currency, rounded-square 12px pour upgrades** |

> Note : le `tailwind.config` du dojo redéfinit `rounded` plus petit (lg=0.5rem) que le DESIGN.md (lg=1rem) — incohérence interne de Stitch. On suit le **DESIGN.md** (les valeurs en rem du frontmatter) qui est la spec canonique.

---

## B. SYSTÈME DE COULEURS (fusion des 3 sources)

### B1. Palette Material 3 (Vibrant Quest — la base structurelle)

```
PRIMARY (vert)    #006e20   container #2ccb4c   on #ffffff   → succès / achat / enhance / progress
SECONDARY (rouge) #b71422   container #db3237   on #ffffff   → START / alertes / close / boss
TERTIARY (bleu)   #00629e   container #67b6ff   on #ffffff   → navigation / info / utilitaires
ERROR             #ba1a1a                                     → danger critique
OUTLINE           #6d7b69 (variant #bccbb6)                   → bordures secondaires / planchers neutres
ON-SURFACE (ink)  #161d1f                                     → outline principal + texte
BACKGROUND        #f4fafd   (clair)                            → fond surface clair
SURFACE-CONTAINER #e8eff1 / #dde4e6 (highest)                  → panels clairs, nav
INVERSE-SURFACE   #2b3234                                      → pills sombres, stage chip
```

### B2. Tokens Claude Design (shared.jsx — la palette "jeu")

```
ink #1F1B2E   inkSoft #3A3354   cream #FFF6E0   paper #FFFFFF
Samurai : red #C42B2B  redDeep #8B1A1A  redSunset #FF6A3D  gold #FFC03A  goldDeep #C28800
Skills  : echo #9B5CFF (echoDeep #6B2DD9)  combo #FF6A3D  vague #2BC6FF (deep #0F8FBF)  souffle #3DD68C (deep #1E9C5A)
Raretés : commun #A8A8B5  rare #4F8BFF  epic #B763FF  legend #FFB319  sacre #FF3D7A
BG dusk : bgTop #FF8F66 → bgMid #E94A3F → bgBot #6B1E3A   (coucher de soleil samouraï)
HP      : hp #33C75A   hpBoss #E03A4A
```

### B3. Logique couleur par FONCTION (la règle d'or — convergente entre les 2)

| Fonction | Couleur | Source |
|----------|---------|--------|
| Succès / achat / upgrade / "Enhance" | **vert** | M3 primary `#006e20` / container `#2ccb4c` |
| Action / START / danger / close / boss | **rouge** | M3 secondary `#b71422` / Claude red `#C42B2B` |
| Navigation / info / sélection active | **bleu** | M3 tertiary `#00629e` / container `#67b6ff` |
| Monnaie (Or/Force) | **jaune/or** | `#FFC03A` (Claude gold) / yellow-400 (Stitch) |
| Échos (prestige currency) | **violet** | `#9B5CFF` (Claude echo) |
| Combo | **orange** | `#FF6A3D` |
| Vague (AOE) | **cyan** | `#2BC6FF` |
| Souffle (méditation) | **vert menthe** | `#3DD68C` |

> **Ces fonctions matchent quasi 1:1 nos GameEvents existants.** Le mapping est direct : `OnVagueTriggered`→cyan, `OnSouffleStarted`→menthe, currency Force→or, Échos→violet. C'est la grande force de cette direction : elle parle déjà notre langage de domaine.

### B4. TRANCHE DE FOND (la question ouverte) — mon avis

Tension réelle dans les sources :
- **Stitch Hall** = fond sombre `radial-gradient(#67b6ff → #004673)` bleu mystique. **Stitch Boutique/Équipement** = fond clair `#f4fafd`. **Stitch Dojo** = fond illustré coucher de soleil violet→rose.
- **Claude Design Combat** = fond illustré dusk samouraï (`#FF8F66→#E94A3F→#6B1E3A`) avec torii + montagnes + sakura. **Claude Équipement** = fond sombre dégradé violet (`#2A2440→#4A1F3A`).
- Nos screenshots Sprint 7.5 actuels = fond noir dojo + plancher bois (notre direction "sombre japonaise" initiale).

**Ma recommandation : fond illustré par CONTEXTE, style puffy partout.**

| Écran | Fond proposé | Raison |
|-------|--------------|--------|
| **Dojo (main game)** | Illustré dusk samouraï subtil (réutilise notre gradient + torii/montagnes silhouette Claude) avec overlay sombre 30-40% en haut/bas pour lisibilité UI | C'est l'écran qu'on regarde 90% du temps — un fond vivant donne l'âme, l'overlay garde le contraste UI |
| **Modals (Inventaire, Affronter Maître, Citation)** | Fond sombre `bgOverlay` rgba(0,0,0,0.85) | Focus sur le contenu, pas de bruit |
| **Hall des Légendes (Prestige)** | Fond sombre mystique bleu/violet radial | Moment solennel — le Stitch Hall le fait déjà parfaitement |
| **Boutique / Hero (Sprint 8+)** | Fond clair surface `#f4fafd` OU sombre selon vibe finale | À trancher quand on les construit |

Le **style puffy reste identique sur les deux types de fond** — c'est ça qui unifie. Un bouton vert puffy marche aussi bien sur fond clair que sombre grâce à son outline charcoal 3px.

> Je m'écarte légèrement du "tout sombre" initial d'Ajwad : garder le **Dojo illustré** (pas plat noir) parce que tous les idle premium (les 3 uploads : Mysterious Hell, Lucky Guy, idle restaurant) ont un fond illustré coloré, jamais noir plat. Le noir plat lit "proto". Mais je garde le sombre pour les moments narratifs (Hall/Prestige) où il sert le ton.

---

## C. TYPOGRAPHIE

### C1. Ce que disent les sources

| Source | UI | Display/Titres | Chiffres |
|--------|-----|----------------|----------|
| Vibrant Quest (DESIGN.md) | **Plus Jakarta Sans** 400-800 | Plus Jakarta Sans ExtraBold 800 + outline 4px + drop shadow 2px | (mono non spécifié) |
| Claude Design | Nunito (body) | **Lilita One** (UI display) + **Bagel Fat One** (combo géant) | Lilita One |
| Notre Sprint 7.5 | Inter | Cinzel (lore) | JetBrains Mono |

### C2. Technique titres (DESIGN.md, à reproduire en TMP)

1. **Outline** : 4px stroke charcoal sur tout texte display (titres + labels boutons). → TMP `outlineWidth` + `outlineColor`.
2. **Drop shadow** : 2px vertical charcoal. → TMP underlay `_UnderlayOffsetY`.
3. **Contraste** : titres blancs `#FFFFFF` sur fonds colorés.
4. CSS text-outline 4 directions :
   ```css
   text-shadow: -1px -1px 0 #161d1f, 1px -1px 0 #161d1f, -1px 1px 0 #161d1f, 1px 1px 0 #161d1f;
   ```
   → en TMP, l'outline natif fait ça proprement.

### C3. Ma recommandation de stack finale

| Usage | Police | Poids | Raison |
|-------|--------|-------|--------|
| **UI générale** (labels, desc, boutons) | **Plus Jakarta Sans** | 600/700/800 | C'est le choix de la spec canonique (DESIGN.md). Géométrique, friendly, lisible en petit. Remplace notre Inter — même famille de feeling, mais c'est la ref. |
| **Display / gros titres** (TU ES MORT, VICTOIRE, titres modals, START) | **Lilita One** (ou Baloo 2 / Fredoka comme alternatives chunky rondes) | 400 | Police ronde "puffy" qui matche le style sticker. Cinzel (notre choix lore) est trop sérif-élégant pour le ton high-energy — **on garde Cinzel UNIQUEMENT pour les citations de Maîtres** (moment lore solennel), Lilita One pour tout le reste display. |
| **Chiffres** (Force, HP, Élan%, costs, levels, dégâts) | **JetBrains Mono** | 700 | On garde notre choix — le mono donne le feeling "data" satisfaisant. Confirmé par le besoin idle. |
| **Lore** (citations Maîtres uniquement) | **Cinzel** | 400/700 | Conservé, niche lore. |

**Stack finale = Plus Jakarta Sans (UI) + Lilita One (display) + JetBrains Mono (chiffres) + Cinzel (lore citations).** 4 polices, chacune un rôle net.

> Argument clé : on avait Inter/JetBrains/Cinzel. On **remplace Inter par Plus Jakarta Sans** (la spec le demande, c'est mieux pour le ton jeu), on **ajoute Lilita One** pour les gros titres ronds (Cinzel seul rendait les titres trop austères pour un jeu "micro-célébration"), on **garde JetBrains Mono + Cinzel** dans leurs niches. DesignTokens a déjà 3 slots `fontPrimary/fontNumbers/fontLore` — il faut **ajouter un 4e slot `fontDisplay`**.

---

## D. LAYOUT & COMPOSANTS (depuis le Dojo code.html — écran complet)

Structure verticale du Dojo Stitch (de haut en bas), avec valeurs exactes :

### D1. Top bar — currency pills + settings
```
Conteneur : flex justify-between, p-md (20px), mt-4
Pill currency : bg-on-surface/80 (charcoal 80%), rounded-full, pr-4 py-1,
                border-[3px] border-outline, shadow-[0_4px_0_rgba(109,123,105,1)]
Icône débordante : absolute -left-4, w-8 h-8, bg-yellow-400 (or) / bg-tertiary-container (bleu gems),
                   rounded-full, border-[3px] border-on-surface  → l'icône CHEVAUCHE le bord gauche du pill
Valeur : ml-6 text-on-primary font-body-bold text-outline
Settings : bg-surface-container-highest, p-2, rounded-full, border-[3px], shadow-[0_4px_0], btn-press
```
> Claude Design fait pareil (TopHud combat.jsx) : icône ronde colorée à gauche qui déborde (`marginLeft:-2`), valeur Lilita One cream, + sous-texte combo `×12 COMBO` en or sous la Force. **À reprendre : le combo affiché DANS le pill Force.**

### D2. Stage info — pill + progress bar + boss marker
```
Pill "Stage : 14" : bg-inverse-surface/90, px-6 py-2, rounded-full, border-[3px], shadow-[0_4px_0],
                    icône skull rouge + texte headline-lg text-outline tracking-wider
Progress bar : w-2/3, bg-on-surface (fond charcoal), h-4, rounded-full, border-[3px], shadow-[0_4px_0]
  Fill : bg-primary-container (vert), rounded-full, border-r-[2px] border-on-surface
    Gloss : absolute top-0 h-1/2 bg-white/30 rounded-t-full  ← bande brillante sur moitié haute
  Boss marker : absolute -right-3 -top-2, bg-error rounded-full w-6 h-6 border-[2px], skull icon
```
> Claude StageRibbon ajoute des **milestones numérotés** (1/4/7/10) le long de la barre, le 10 = boss rouge. Plus riche. **À reprendre pour nos paliers adversaires.**

### D3. Battle area
```
flex-grow (pousse les contrôles en bas)
Character : absolute bottom-10, animate-bounce, w-24 h-24, rounded-full,
            border-4 border-on-surface, shadow-[0_6px_0_rgba(22,29,31,1)]
```
> Le perso est dans un **cercle bordé** (avatar style) dans le Stitch dojo. MAIS dans le Claude Combat + les 3 uploads, le perso est **pleine silhouette sur la scène** (pas en médaillon). **Recommandation : pleine silhouette** (notre LayeredCharacterRenderer), le médaillon c'est pour les écrans Hero/menu. `animate-bounce` = notre breathing DOTween (déjà fait).

### D4. Controls panel
```
Conteneur : bg-surface-container/95, p-sm, rounded-t-xl, border-[3px] border-b-0, mt-auto, pb-8
START button : w-full bg-secondary-container (rouge), py-4, rounded-xl,
               border-[3px] border-on-surface border-b-[6px],
               active:border-b-[3px] active:translate-y-[3px], overflow-hidden
  Shine : absolute top-1 left-2 w-12 h-4 bg-white/20 rounded-full rotate-[-15deg]
  Label : font-headline-lg text-outline uppercase tracking-wider + icône skull
Upgrade grid : grid-cols-2 gap-sm   ← 2 COLONNES (Stitch) vs notre 3 cards verticales
  Card : bg-surface, border-[3px] border-outline border-b-[5px], rounded-lg, p-2,
         active:border-b-[3px] active:translate-y-[2px], flex-col items-center gap-2
    Icon container : bg-{tertiary|primary}-container, w-12 h-12, rounded-full,
                     border-[2px], shadow-[0_3px_0]
    Nom + Lv : font-body-bold + sous-texte text-xs
    Cost pill : bg-primary-container (vert), px-3 py-1, rounded-full, border-[2px],
                icône coin or + valeur
```
> Claude Combat fait les upgrades en **ROWS horizontales** (icône gauche / nom+desc centre / cost pill droite) — c'est ce qu'on a déjà fait en Sprint 7.5 ! Stitch les fait en **grid 2-cols**. Les deux marchent. **Recommandation : garder nos rows** (meilleure lisibilité desc en portrait) mais appliquer le style puffy (border-b plancher + cost pill vert).

### D5. Bottom nav — 5 onglets
```
nav : fixed bottom-0, h-20, bg-surface-container-highest, border-t-4 border-outline,
      flex justify-around items-end, px-sm pb-safe
Onglet inactif : flex-col, text-on-surface-variant, opacity-80, icône 2xl + label-caps
Onglet ACTIF (Dojo) : bg-tertiary-container (bleu), text-on-tertiary-container, rounded-lg,
                      border-2, shadow-[0_4px_0_rgba(0,74,120,1)],
                      active:translate-y-[4px] active:shadow-none, relative -top-2  ← SURÉLEVÉ
                      icône 3xl (plus grosse) FILL=1
5 onglets : Shop (shopping_cart) / Hero (sports_martial_arts) / Dojo (swords) / Artifacts (auto_awesome) / Legend (emoji_events)
```
> Claude NavTab : identique (actif = gold surélevé `margin -4px 0`, border + plancher). Les 4 écrans Stitch ont TOUS la même bottom nav 5-onglets avec l'onglet courant surélevé+coloré. **C'est le squelette de navigation cible.**

---

## E. LES 4 ÉCRANS STITCH (mood depuis les PNG)

### E1. Dojo (`saga_le_dojo_v2`) — NOTRE écran principal
- Fond coucher de soleil violet→rose, nuages. Top : 2 currency pills (Or 106.5k, Gems 65) + settings rond.
- Stage chip rouge "Stage 14" + barre verte avec marker boss à droite.
- Perso en médaillon jaune circulaire au centre, qui bounce.
- Panel bas blanc-gris : gros bouton HARD START rouge + 2 cards (Strike Lv42 / Focus Lv15) avec cost vert.
- Bottom nav 5 onglets, Dojo actif (bleu surélevé).

### E2. Hall des Légendes (`saga_hall`) — NOTRE Prestige
- Fond bleu mystique radial. Titre "HALL DES LÉGENDES" blanc outline + sparkles.
- Chip central "12,450 Echos Total" (icône history_edu).
- **Liste de cards de boss vaincus** : chaque card = icône colorée (rouge/vert/bleu) + nom (Sir Galahad/Hattori Hanzo/Merlin/Unknown Hero) + origine (Camelot/Edo/Avalon) + récompense Échos `+4,200` en chip bleu. Chaque card a son `border-b-[6px]` plancher.
- **Gros bouton RENAISSANCE rouge** en bas (= notre `CompletePrestige`).
- Bottom nav, Legend actif.
> Ça mappe EXACTEMENT notre système : DeathRecords → cards, totalEchos → chip, Renaissance → CompletePrestige. Hattori Hanzo/Edo = nos Maîtres. **Le Stitch Hall est un blueprint direct de notre écran Prestige Sprint 6.**

### E3. Boutique (`saga_boutique`) — Sprint 8+
- Fond clair. Top : settings + logo SAGA rouge + gems pill.
- 3 onglets : Free (bleu) / Packs (rouge actif) / Artifacts (vert).
- Starter Pack card : header rouge + visuel + lignes "500 Gems / 10,000 Coins" + bouton prix vert `$4.99`.
- Daily Deals : timer "Refreshes in 04:12:00" + grid 2-cols (Iron Pickaxe, Health Potion -20%, Dungeon Key, Fire Spell SOLD OUT grisé).
- Bottom nav, Shop actif.

### E4. Hero/Équipement (`saga_quipement`) — Sprint 8+
- Fond sombre. Top : Level 42 chip + settings.
- Grand portrait perso (médaillon carré) avec armure encadrée.
- Nom "SOLID SHOTGUNNER". 3 slots équipement (arme/armure/relique) en rounded-square.
- Stats : ATK 1195 (+792 en vert) / ATK SPD 0.9s / CRITRATE 15%.
- Mini-stats étoilées (1★ 89% / 3★ 10% / 5★ 1%) + gros bouton ENHANCE! vert + coût.
- Bottom nav, Hero actif.
> Claude `equipement.jsx` est BEAUCOUP plus riche (culture tabs Samurai/Viking/Wuxia, 3 EquippedSlots CORPS/ARMURE/ARME avec raretés+étoiles+niveau, grille inventaire 6-cols avec InvCell rareté/level/count/NEW/equipped, panneau stats, bouton FUSION + ÉQUIPER LA SÉLECTION). **C'est le blueprint de notre future UI équipement complète** (on a déjà le squelette EquipmentInventoryModal Sprint 7).

---

## F. LES SVG DU CLAUDE DESIGN (inventaire complet)

Tous en `viewBox="0 0 32 32"` (icônes) ou `0 0 120 160` (chibis), `strokeWidth` 1.5-3, fills = tokens `T.*`, `strokeLinejoin="round"`. Technique : paths géométriques simples + outline ink épais (le même langage que le reste).

### F1. 18 icônes (`shared.jsx` objet `I`)
| Icône | Glyphe | Usage SAGA |
|-------|--------|------------|
| **Coin** | cercle or + 力 (kanji Force) | currency Force |
| **Gem** | losange violet facetté | Échos |
| **Plus** | croix arrondie | bouton add |
| **Gear** | engrenage 12 dents + centre | settings |
| **Skull** | crâne | stage/boss/Hall |
| **Scroll** | parchemin + lignes | quêtes |
| **Trophy** | coupe + anses | héros/classement |
| **Chest** | coffre + serrure | cadeaux/loot |
| **Fist** | poing fermé + doigts | upgrade Frappe |
| **Lotus** | fleur 3 pétales + base | upgrade Méditation |
| **People** | 2 têtes + corps | upgrade Disciple |
| **Sword** | katana + garde or | combat/arme |
| **Shop** | échoppe + auvent | boutique |
| **Lightning** | éclair | Vague (AOE) |
| **Zen** | cercle + vague + 2 points | Souffle |
| **Clock** | horloge + aiguilles | chrono |
| **Back** | chevron gauche | retour |
| **Star** | étoile 5 branches or | rareté/notation |

> 18 icônes couvrent **tout notre jeu actuel**. Coin=Force, Gem=Échos, Fist/Lotus/People = nos 3 upgrades exacts, Lightning=Vague, Zen=Souffle, Skull=Maîtres. Mapping 1:1.

### F2. 2 chibis (`viewBox 0 0 120 160`)
- **ChibiSamurai** : armure rouge (`redDeep` corps + `red` plaques), ceinture or, topknot + bandeau rouge headband avec disque cream, katana en fourreau dans le dos (ligne ink + manche bois + pommeau or), yeux déterminés, mains beige. ~30 paths. `facing` flip via `scaleX(-1)`.
- **ChibiEnemy** (Bandit Rōnin) : corps gris `#4A4258`, hem en lambeaux (zigzag), capuche violet sombre, yeux rouges colère, balafre, naginata brisée. ~25 paths.

> Technique : silhouette en gros paths fill + outline ink 3px, détails (yeux/ceinture/arme) en petits paths. **Reproductible en SVG→Sprite Unity** ou directement en mesh UI. C'est le sujet du PROMPT 2 (génération SVG). Ces chibis remplaceraient/complèteraient nos sprites rvros — à décider (rvros = pixel art, ces chibis = vectoriel puffy ; mélanger les deux styles serait incohérent).

---

## G. PLAN DE REFONTE pour SAGA (ordonné par impact)

### Sprint 7.5 (immédiat — sur la branche actuelle ou 7.6)

| # | Changement | Effort | Impact |
|---|-----------|--------|--------|
| **G1** | **Étendre DesignTokens** : repalette Material 3 (primary vert/secondary rouge/tertiary bleu), ajouter slot `fontDisplay` (Lilita One), confirmer mapping fonction→couleur | S | ⭐⭐⭐⭐⭐ |
| **G2** | **Style puffy 3D sur SagaButton** : remplacer le glow/flat actuel par bottom-border plancher (Image enfant offset Y + teinte sombre) + outline 3px charcoal + gloss ovale top-left + pressed translateY. C'est LE changement signature. | M | ⭐⭐⭐⭐⭐ |
| **G3** | **Currency pills top bar** : pill charcoal arrondi + icône ronde colorée débordante à gauche (Coin=Force or, Gem=Échos violet) + valeur JetBrains. Combo affiché dans le pill Force. | M | ⭐⭐⭐⭐ |
| **G4** | **Cards upgrades en style puffy** : garder nos rows, ajouter plancher border-b 5px + cost pill vert (achetable) + icône dans container rounded-square coloré | S | ⭐⭐⭐⭐ |
| **G5** | **Barres puffy** (Élan, Adv progress, HP) : fond charcoal + outline 3px + fill gradient + gloss bande haute + milestones markers | S | ⭐⭐⭐ |
| **G6** | **Scaffold bottom nav 5 onglets** : Shop / Hero / **Dojo (actif)** / Artifacts / Legend. Les 4 autres ouvrent un placeholder "Bientôt" pour l'instant. Onglet actif surélevé+bleu. Pose le squelette de navigation. | M | ⭐⭐⭐⭐ |
| **G7** | **Stage chip + progress** : pill "Palier N" charcoal + barre verte gloss + marker boss rouge | S | ⭐⭐⭐ |
| **G8** | **18 icônes SVG → Sprites Unity** (PROMPT 2) : remplacer nos icônes procédurales (DrawCircle/Line) par les vraies icônes Claude Design | M | ⭐⭐⭐ |

### Sprint 8+ (écrans supplémentaires — scaffold nav maintenant, contenu plus tard)

| # | Écran | Mappe à | Source blueprint |
|---|-------|---------|------------------|
| G9 | **Hall des Légendes** (Legend tab) | notre Prestige Sprint 6 (DeathRecords + totalEchos + Renaissance) | Stitch Hall — quasi prêt à porter |
| G10 | **Hero/Équipement** (Hero tab) | notre EquipmentInventoryModal Sprint 7 étendu (culture tabs + 3 slots + grille inventaire + stats + fusion) | Claude equipement.jsx (riche) + Stitch équipement |
| G11 | **Boutique** (Shop tab) | nouveau (monétisation) | Stitch boutique |
| G12 | **Artifacts** (Artifacts tab) | nos Reliques Sprint 6/7 | à designer |
| G13 | **Chibis vectoriels** (option) | remplacer/compléter rvros | Claude ChibiSamurai/Enemy — ⚠️ décision style pixel vs vectoriel |

### Ce qui NE change PAS (déjà bon en Sprint 7.5)
- Architecture POCO services + GameEvents (le mapping couleur/fonction colle déjà dessus).
- AudioService / HapticService procéduraux.
- LayeredCharacterRenderer (perso pleine silhouette = bon choix vs médaillon).
- Layout portrait bands (à ajuster pour la bottom nav 5-onglets qui mange ~80px en bas).

---

## ❓ QUESTIONS POUR AJWAD (à trancher avant de coder)

1. **Fond Dojo : sombre plat (actuel) ou illustré dusk samouraï subtil ?**
   Mon avis : illustré (gradient dusk + torii/montagnes silhouette + overlay sombre pour lisibilité). Le noir plat lit "proto", tous les idle premium ont un fond illustré. Sombre réservé aux modals + Hall/Prestige. → **valides-tu illustré pour le Dojo ?**

2. **Stack de polices : Plus Jakarta Sans (UI) + Lilita One (display) + JetBrains Mono (chiffres) + Cinzel (lore citations) ?**
   Ça remplace Inter par Plus Jakarta Sans, ajoute Lilita One pour les gros titres ronds, garde JetBrains + Cinzel. 4 polices, 4 rôles. → **OK, ou tu préfères garder Inter et zapper Lilita One ?**

3. **Densité visuelle : "calme premium" ou "loud festif" ?**
   Les 3 uploads (Mysterious Hell, Lucky Guy) sont TRÈS chargés (badges partout, side-buttons, popups). Le Stitch dojo est plus épuré. SAGA a un ton "mythique-humain" plus posé. → **niveau de densité visé ? Je penche épuré-premium (Stitch dojo) plutôt que loud (uploads).**

4. **Bottom nav : scaffold les 5 onglets maintenant (Dojo actif + 4 placeholders "Bientôt") ou juste le Dojo pour l'instant ?**
   Mon avis : scaffold les 5 maintenant (la nav structure tout le reste, et les placeholders sont triviaux). → **on pose les 5 onglets dès 7.6 ?**

5. **Chibis : on reste sur le pixel art rvros, ou on bascule vers les chibis vectoriels puffy (ChibiSamurai) pour matcher le style sticker ?**
   ⚠️ Mélanger pixel art + vectoriel puffy serait visuellement incohérent. Le style "Tactile & Bold" appelle du **vectoriel**. Mais on a déjà investi dans rvros + le LayeredCharacterRenderer (Body/Armor/Weapon). → **gros call de direction artistique : pixel (garder rvros) OU vectoriel puffy (refonte perso) ?** C'est la question la plus structurante.

6. **Material 3 palette (vert/rouge/bleu saturés) vs notre palette sombre ambre/charcoal actuelle ?**
   Le style puffy demande des couleurs saturées high-contrast. Notre DesignTokens 7.5 est sombre/sobre (ambre + charcoal). → **on adopte la palette M3 vive, ou on garde notre ambre dominant et on emprunte juste la STRUCTURE puffy ?** (On peut faire un hybride : structure puffy + palette ambre/sombre SAGA.)

> **Une fois ces 6 points tranchés, PROMPT 2 = génération des SVG** (18 icônes + 2 chibis si on part vectoriel), puis refonte UI ordonnée G1→G8.
