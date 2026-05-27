# 05 - VISUAL STYLE (DIRECTION A LOCKED)

## Direction verrouillée: "Lame & Encre"

**Esthétique**: sumi-e moderne (encre japonaise sur cocoon noir), prestigieux, dark, mythique. Référence visuelle: croisement entre Sekiro, Ghost of Tsushima, FromSoft item descriptions, et UI moderne type Linear/Arc.

**Accents C aux moments épiques**: aux moments clés (prestige, unlock voie, boss kill, palier mythique), on bascule temporairement vers le mode "Voie Brutale" (haut contraste, typo qui claque, rose hot accent). Ces accents marquent les pivots narratifs sans changer l'ambiance globale.

## Palette de couleurs

### Couleurs de base (UI globale)

```
--bg-deep:        #0d0d0d   /* fond principal */
--bg-card:        #161616   /* cards, surfaces */
--bg-card-hover:  #1f1f1f   /* hover state */
--border-subtle:  #2a2a2a   /* bordures fines */
--border-strong:  #3a3a3a   /* bordures emphasis */

--text-primary:   #fafafa   /* texte principal */
--text-secondary: #888888   /* labels, secondaire */
--text-tertiary:  #555555   /* hints, désactivé */

--accent-primary: #FAC775   /* ambre, currency principale, highlights */
--accent-warm:    #993C1D   /* coral, secondaire, Force icon */
--accent-cool:    #3C3489   /* indigo, Technique icon */
--accent-earth:   #0F6E56   /* teal foncé, Renom icon */
--accent-soul:    #712B13   /* brun rouge profond, Échos icon */

--state-success: #639922
--state-warn:    #BA7517
--state-danger:  #993C1D
```

### Couleurs par voie (override accent-primary)

Quand une voie est choisie, son accent remplace l'ambre par défaut:

- **Samurai**: `#FAC775` (or pâle, ambre, c'est la voie par défaut)
- **Gaulois**: `#A77B40` (bronze ocre, peinture de guerre)
- **Saladin**: `#D9A547` (or chaud désert)
- **Spartiate**: `#B85C32` (bronze brûlé)
- **Viking**: `#7DA7C9` (bleu acier glacial)
- **Mongol**: `#94A89C` (gris steppe)
- **Aztèque**: `#5CB89A` (jade)
- **Wuxia**: `#9FBFA8` (vert pâle)

Couleur récupérable via `VoieData.AccentColor` sur la voie active.

## Typographie

**Police principale**: **Inter** ou **Space Grotesk** (open source, gratuit, mobile-friendly). Pour iOS native: SF Pro fallback.

**Police accent** (pour les chiffres énormes et les titres de palier): **JetBrains Mono** ou **Space Mono** (chiffres tabulaires obligatoires pour idle).

**Tailles**:
- Display (chiffres principaux): 32-48px, weight 500
- H1 (palier, voie): 24px weight 500
- H2 (sections): 18px weight 500
- Body: 14-15px weight 400
- Label: 11-12px weight 500, letter-spacing 1.5px (uppercase sentence-case)
- Hint: 10-11px weight 400 (#555)

**Règle**: sentence case partout, jamais ALL CAPS sauf labels courts (≤10 chars) avec letter-spacing.

## Style d'illustration

### Personnage

> **Direction tranchée 2026-05-27** : pivot vers **pixel art chibi simple** (style rvros Animated Adventurer). Voir DESIGN_DECISIONS_LOG.md même date.

- **Pixel art chibi**, proportions ~1:2 (tête grosse, corps petit), style "Pixel Adventurer" simple et lisible
- Culturellement **neutre par défaut**. Customisation par voie via overlays / palette swap (Sprint 4+).
- Le perso est lisible en miniature à 64px haut.
- Animations frame-by-frame depuis spritesheets : idle (4 frames), attack (5-6 frames), hurt (3 frames). Minimum viable pour Sprint 3 = idle + attack.
- **Tech : Unity 2D Animation built-in** (Spine 2D écarté pour MVP — coût licence + workflow plus complexe non justifié).
- Import settings : Filter Point, PPU 32 (à ajuster en play), Compression None.
- Les armes restent signature par voie (visible sur les sprites override / overlays, pas sur le perso neutre).

### Mannequin / training dummy

- Bois sculpté, base lourde
- Évolue visuellement avec le tier du dojo (Stade 1: poteau brut, Stade 6: cible cérémoniale)
- Animations: shake-recoil sur tap, particules de bois sur crit

### Dojo / background

- **Sprint 3+** : background dark mais pas noir total. Couleur de base `#1a1a1a` avec accents chaleureux ambre (pas le `#0d0d0d` ultra-dark de l'UI globale qui reste valide pour cards/modals).
- Couches parallax simples (3 plans max pour les perfs mobile)
- Ambient particles (poussière qui flotte, pétales selon voie)
- Lighting 2D dynamique (URP 2D Lights) — une lumière chaude sur le perso, plus froide en background
- Évolue par tier (voir 04_PROGRESSION.md)
- **Pixel Perfect Camera** activée dans la scene Main pour préserver le rendu crispé des sprites pixel art.

### FX et particules

- **Tap basique**: small puff de poussière + ring d'impact (1 sec, fade out)
- **Crit**: shake + flash blanc + particules ambrées + ring large + son spécial
- **Currency tick**: petite particule qui flotte du mannequin vers le compteur
- **Milestone**: full-screen ink wash effect (1.5s) + drum hit
- **Prestige**: cinématique 3-5s avec ink dispersal et reformation

## Règles UI mobile

### Layout
- Format portrait obligatoire (16:9 ou 19.5:9)
- Safe areas iOS respectées (notch, home indicator)
- Bottom dock à 88px minimum du bas
- Tap targets ≥ 48px (Apple/Google guidelines)

### Densité
- Pas plus de 5 informations actives visibles à un moment T sur le screen principal
- Sub-menus en bottom sheets (pas en push transitions, ça casse l'immersion)
- Modals = full overlay dark avec ink wash, jamais des popups bordés

### Animations UI
- Tous les boutons: `scale(0.96)` au tap (DOTween 0.15s, easing OutCubic)
- Number changes: ticker animation, jamais teleport (DOTween numbers, 0.3-0.5s)
- Transitions de screen: ink wash horizontal (0.4s)
- Apparition d'élément: scale 0→1 + alpha 0→1 (0.2s, easing OutBack)

## Sound design (résumé visuel)

Voir 02_GAME_DESIGN.md pour les principes audio détaillés. Le visuel et l'audio sont **synchrones** sur tous les events:
- Visual milestone = drum hit
- Crit = cymbal + flash
- Achievement = trois notes + reverb visuel
- Prestige = silence soudain → drone qui monte → drum massif

## Accents "Voie Brutale" (mode événementiel)

Aux moments suivants, l'écran bascule temporairement en mode haut-contraste:

- **Choix de voie au prestige**: fond blanc cassé, typo énorme, choix en cards noires
- **Boss kill**: titre du boss en typo énorme, screen flash + ink wash
- **Voie cachée débloquée**: full takeover, glitch effect court, révélation typographique
- **Milestone "Mythe"**: l'écran se reconstruit en sumi-e blanc sur noir, le nom du joueur apparaît en typo énorme

Ces moments ne durent JAMAIS plus de 3-5 secondes. Ils marquent, puis retournent à l'ambiance Lame & Encre.

## Assets à produire (liste prioritaire MVP)

### Personnage
- [ ] Silhouette de base (Stade 1 Mendiant)
- [ ] Skin Stade 2 Apprenti (avec katana de base si Samurai)
- [ ] Skin Stade 3 Guerrier Samurai
- [ ] Skin Stade 4 Maître Samurai
- [ ] Animations: idle, tap react, walk, prestige cinematic
- [ ] Variation Wuxia (Stade 2-4)
- [ ] Variation Spartiate (Stade 2-4)
- [ ] Variation Viking (Stade 2-4)

### Mannequins
- [ ] Mannequin Stade 1 (poteau brut)
- [ ] Mannequin Stade 2-3 (entraînement bois)
- [ ] Mannequin Stade 4 (palus avec marques)

### Dojos
- [ ] Background Stade 1-2 (clairière/cabane)
- [ ] Background Stade 3-4 (dojo bois → pierre)
- [ ] Variations par voie (Samurai = japonais, Spartiate = grec, etc.)

### UI elements
- [ ] Icons currency (Force/Technique/Renom/Échos) en SVG kanji style
- [ ] Icons esprits (15-20 esprits, vector style)
- [ ] Bouton tap zone
- [ ] Cards d'upgrade
- [ ] Bottom dock 4 icônes
- [ ] Progress bars custom

### FX
- [ ] Particle: tap dust
- [ ] Particle: crit burst
- [ ] Particle: currency floaty (ambre)
- [ ] Shader: ink wash transition
- [ ] Shader: dissolve sur perso (pour prestige cinematic)

### Audio
- [ ] Tap sound (basique)
- [ ] Crit sound
- [ ] Milestone drum
- [ ] Achievement chime
- [ ] Prestige cinematic music (10s composition)
- [ ] Ambient loop par voie (Samurai, Wuxia, Spartiate, Viking pour MVP)

## Note sur l'art temporaire

Pour le proto MVP, **on peut utiliser des silhouettes simples SVG-converted-to-prefab** et des placeholders. L'art final viendra en parallèle du dev. Le coordinateur peut fournir des mockups SVG sur demande. Ajwad peut aussi dessiner si besoin.

**Style placeholder valide**: silhouettes flat noir/blanc, ratio correct, animations Spine basiques. Tant que la lisibilité est bonne, le placeholder peut rester jusqu'au polish.
