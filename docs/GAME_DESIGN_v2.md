# SAGA - GAME DESIGN v2

> **Document fondateur** révisé après Sprint 3 closed.
> Intègre le système Boss/Adversaire/Cycle, la mort narrative, les mécaniques actives, et la nouvelle roadmap Sprint 4-12.
> Date: 2026-05-27
> Status: VALIDÉ par Ajwad

---

## 🎯 PHILOSOPHIE DU JEU

**SAGA** est un idle/incremental mobile avec une couche de combat actif qui transforme un grind passif en **expérience narrative mythique**.

Le joueur incarne un combattant qui s'entraîne, affronte des adversaires, défie des Boss légendaires, et finit par mourir face à un Maître ultime pour renaître plus fort. Chaque mort raconte une histoire. Chaque prestige est une légende personnelle.

**Inspirations** : Sekiro (la mort comme moteur narratif), Vampire Survivors (visuel simple addictif), Hades (cycles de mort/renaissance), Loop Hero (idle avec couche tactique).

---

## ⚔️ LE SYSTÈME DE COMBAT (LA PIÈCE CENTRALE)

### Vue d'ensemble

Le combat dans SAGA se déroule en **3 phases alternées** :

```
┌──────────────────────────────────────────────────────────┐
│                                                          │
│   PHASE 1            PHASE 2            PHASE 3          │
│   ENTRAÎNEMENT       ADVERSAIRE         BOSS             │
│   ━━━━━━━━━━━        ━━━━━━━━━          ━━━━             │
│                                                          │
│   Tu tapes le        Un combattant      Tous les 10      │
│   mannequin pour     arrive, chrono     adversaires, un  │
│   grind Force.       démarre. Tu dois   Boss arrive.     │
│                      le tuer avant la                    │
│   Une barre          fin du temps.      Combat plus dur, │
│   "PROCHAIN                              cinématique,    │
│   ADVERSAIRE"        Victoire → loot    récompense       │
│   se remplit en      Défaite → mort     unique.          │
│   haut.              temporaire                          │
│                                          Mort face Boss  │
│   90% du temps.      ~30-60 sec.        légendaire =     │
│                                          PRESTIGE        │
└──────────────────────────────────────────────────────────┘
```

### Phase 1 - Entraînement (état par défaut, 90% du temps)

**Mécanique** :
- Tu tapes le mannequin de bois au centre de l'écran
- Chaque tap génère de la Force (selon tes upgrades)
- Une **barre de progression "PROCHAIN ADVERSAIRE"** se remplit en haut de l'écran
- La barre se remplit en fonction du nombre de taps ou du temps écoulé (à ajuster selon balance)
- Tu accumules des Disciples, des Upgrades, du Loot passif

**Visuels** :
- Perso à gauche, mannequin à droite
- Slash FX au tap, mannequin shake
- Compteur Force au centre, qui grossit selon magnitude
- Combo en haut à droite (×1.0 → ×2.0)
- Cards d'upgrades en bas
- **Barre "Prochain Adversaire" en haut**, qui se remplit visuellement

**Tempo** : 30-90 secondes selon les paliers (plus rapide au début, plus lent aux stades avancés)

### Phase 2 - Adversaire (combat actif chronométré)

**Déclenchement** : la barre "Prochain Adversaire" est pleine.

**Mécanique** :
- Le mannequin disparaît en fade-out
- Un **adversaire fade-in** à sa place avec son nom affiché 1 seconde
- **Chrono démarre** (30-60 sec selon le niveau de l'adversaire)
- L'adversaire a une **barre de HP** à éliminer
- Tu dois infliger les dégâts avant la fin du chrono
- Pendant ce temps, tu peux toujours utiliser Élan et Souffle

**Issues** :
- **Victoire** :
  - Animation de mort de l'adversaire (animation "hurt" du pack rvros)
  - Drop de récompenses (Force bonus + chance loot)
  - Retour automatique au mannequin (Phase 1) après 2 sec
  - Barre "Prochain Adversaire" se remet à zéro
- **Défaite** (chrono expiré ou HP insuffisants) :
  - Cinématique de mort de ton perso (animation "die")
  - Écran fade to black
  - Affichage centré "TU ES MORT" en rouge sombre
  - Citation : "Tu n'as pas frappé assez vite. Le silence t'a rattrapé."
  - **Tombe affichée** au centre (visuel simple : croix en pierre + nom du perso)
  - **Chrono se met en pause**
  - **Click n'importe où** pour relancer
  - Retour devant le mannequin (Phase 1)
  - Perte mineure (genre -10% Force totale, pour donner du poids sans frustrer)

**Variations d'adversaires** (à designer Sprint 4-5) :
- Adversaires lambda (5-10 types visuels) : Ronin Errant, Spadassin Nordique, Initié Wuxia, Hoplite Lâché, etc.
- HP et chrono adaptés au stade du joueur

### Phase 3 - Boss (tous les 10 adversaires battus)

#### Boss Mineurs (Capitaines)

**Déclenchement** : tous les 10 adversaires vaincus.

**Mécanique** :
- Cinématique d'arrivée plus marquée (1-2 sec) : écran s'assombrit légèrement, lettrage du nom
- HP plus élevés (3-5x un adversaire normal)
- Chrono plus généreux (60-90 sec)
- Boss a des **phases** (75% / 50% / 25% HP)
- Récompense : drop garanti d'une **Rune ou Relique**

**Liste des Boss Mineurs** (8 capitaines, un par voie, à designer Sprint 5) :
- Capitaine Samurai : "Hattori du Mont"
- Capitaine Viking : "Bjorn aux Tresses"
- Capitaine Wuxia : "Lin du Bambou"
- Capitaine Spartiate : "Pythagoras Lame Brève"
- Capitaine Mongol : "Berke le Cavalier"
- Capitaine Saladin : "Nour ad-Din"
- Capitaine Aztèque : "Cuauhtémoc le Jeune"
- Capitaine Gaulois : "Brennus du Cor"

**Issue de défaite** : comme un adversaire normal (mort temporaire, retour mannequin).

#### Boss Majeurs (Maîtres légendaires)

**Déclenchement** : après avoir vaincu un certain nombre de Capitaines d'une voie (X = à équilibrer, probablement 3-5).

**Le joueur INVOQUE volontairement** le Boss majeur via un bouton "Affronter le Maître" qui apparait dans le menu. **C'est une décision narrative** : tu sais que ça peut te tuer définitivement.

**Mécanique** :
- Cinématique d'arrivée ÉPIQUE (2-3 sec)
- HP énormes (10-20x un adversaire normal)
- Chrono très long (2-5 min)
- 4 phases (100% / 75% / 50% / 25% HP)
- À 25% HP : **enrage** (camera shake, screen tint rouge, dégâts boss x2)
- Récompense : **Relique du Maître** (équipement légendaire spécifique) + **Stade visuel suivant débloqué**

#### Les 8 Boss Majeurs (Maîtres légendaires)

Tous inspirés de **mythologie réelle** (zéro copyright) :

| Voie | Boss légendaire | Inspiration | Citation finale |
|---|---|---|---|
| **Samurai** | **Yoshitsune l'Inatteignable** | héros samurai 1159-1189 | "Le vent ne se laisse pas saisir." |
| **Viking** | **Ragnar aux Yeux d'Acier** | Ragnar Lothbrok | "Les corbeaux n'oublient pas." |
| **Wuxia** | **Sun le Voyageur Céleste** | Sun Wukong | "Le ciel est vaste. Tu as bien marché." |
| **Spartiate** | **Léonidas du Marbre Brisé** | Roi de Sparte | "Reviens avec ton bouclier, ou dessus." |
| **Mongol** | **Subutaï le Vent du Levant** | Général de Genghis | "Les steppes se souviennent." |
| **Saladin** | **Salah ad-Din du Désert** | Sultan ayyubide | "La miséricorde est plus tranchante que la lame." |
| **Aztèque** | **Ahuitzotl le Coeur Brûlant** | Empereur aztèque | "Le soleil se nourrit." |
| **Gaulois** | **Vercingétorix le Dernier** | Roi gaulois | "Nous sommes tombés. Souviens-toi de nous." |

Chaque boss a :
- Son **arène thématique** (background unique)
- Sa **musique dédiée** (track boss)
- Sa **couleur aura** (selon sa voie)
- Sa **cinématique d'apparition** unique
- Sa **citation de mort** quand tu le bats
- Sa **citation de victoire** quand il te tue (utilisée dans la cinématique de PRESTIGE)
- Son **drop unique** (Relique légendaire avec stats top tier)

---

## ⚰️ LA MORT ET LE PRESTIGE

### Deux types de mort

**Mort Temporaire** (face à un Adversaire normal ou un Capitaine) :
- Cinématique courte (1-2 sec)
- Écran noir + tombe + "TU ES MORT"
- Click pour relancer
- Retour devant le mannequin
- Perte mineure (-10% Force totale)
- **Pas de reset**

**Mort Définitive** (face à un Maître légendaire) :
- C'est ça qui déclenche le **PRESTIGE**
- Le joueur a CHOISI d'affronter le Maître, donc il accepte ce risque
- Cinématique LONGUE et narrative (5-8 sec)
- Citation du Maître affichée
- Affichage : "MAÎTRE [Pseudo joueur] EST MORT FACE À [Nom du Boss]"
- Citation finale du joueur affichée (qu'il avait écrite avant)
- "Tu es devenu une légende. Tes Échos vivent encore."
- Calcul des Échos gagnés (selon progression du run)
- **RESET** complet sauf : Échos accumulés, Reliques conservées (1 par run), Titres débloqués, Achievements

### Le Prestige en détail

**Déclencheur** : Mort face à un Boss Majeur (un des 8 Maîtres légendaires).

**Phase 1 - Cinématique de mort (5-8 sec)** :
1. Slow motion sur le coup fatal du Boss
2. Ton perso bascule (animation "die" du pack rvros)
3. Écran fade to black
4. Apparition lettrage : "**TU ES MORT**" en rouge profond
5. Sous-titre : "Face à [Nom du Maître]"
6. Citation du Maître affichée (sa quote spécifique)
7. Drum hit grave

**Phase 2 - Voyage Intérieur (3-5 sec)** :
1. Écran reste noir
2. Petites lumières apparaissent (les Échos)
3. Voix off textuelle : "Tu as combattu pendant [X] jours. Atteint le Stade [Y]. Vaincu [Z] Maîtres."
4. Affichage : "**[X] Échos accumulés**"

**Phase 3 - Citation finale du joueur (5-8 sec)** :
1. Apparition : "Ton héritage :"
2. Citation finale du joueur (qu'il avait écrite en début de run)
3. Si pas de citation écrite : prompt "Écris ta dernière phrase..." (max 80 caractères)
4. La citation est enregistrée dans le Hall des Légendes du joueur

**Phase 4 - Renaissance (3 sec)** :
1. Fade to white
2. Apparition du perso au stade 1 (Mendiant)
3. Affichage : "**Tu renais. Que ta prochaine légende soit plus longue.**"
4. Retour au gameplay (Phase 1 Entraînement)

**Récompenses du Prestige** :
- **Échos accumulés** (currency permanente)
- **Reliques conservées** (le joueur peut garder 1 item, le reste retourne en Échos)
- **Hall des Légendes** : la mort est gravée avec citation
- **Titre débloqué** "Vaincu par [Boss]" pour ton prochain run

---

## 🎮 MÉCANIQUES ACTIVES (LE SKILL DU JOUEUR)

### Mécanique 1 : ÉLAN (la jauge de tap)

**UI** : Petite jauge sous le compteur Force, en permanence visible.

**Mécanique** :
- Chaque tap remplit la jauge de +X%
- La jauge **diminue naturellement** si tu ne tap pas (2-3 sec sans tap = perte progressive)
- À **100% d'Élan** : la jauge clignote, un bouton "VAGUE" apparait
- Click sur Vague → ton perso fait une **animation spéciale** (DOTween anim cool + slash FX énorme)
- Effet de Vague :
  - En Phase 1 (mannequin) : **+500% Force pendant 5 secondes**
  - En Phase 2/3 (combat) : **dégâts massifs au boss/adversaire**
- L'Élan retombe à 0% après une Vague

**Pourquoi c'est génial** :
- Récompense le joueur actif (vs idle pur)
- Spectacle visuel régulier
- Mini-objectif court-terme (remplir la jauge)
- Bouton "Vague" donne une décision tactique (quand l'utiliser ?)

### Mécanique 2 : SOUFFLE (le mode meditation)

**UI** : Bouton "Souffle" (icône de respiration / fumée) à côté de la jauge Élan.

**Mécanique** :
- Click sur Souffle → ton perso passe en **mode méditation pendant 5 sec**
- Pendant ces 5 sec :
  - Impossible de tap (le perso médite, animation idle-2 du pack rvros)
  - Particules de fumée/encens autour du perso
  - Petit halo doré
- Après les 5 sec :
  - **+50% gain Force** pendant 30 secondes
  - Pendant un combat : **+20% HP regen pendant 30 sec**
- **Cooldown de 2 minutes** entre deux Souffles

**Pourquoi c'est génial** :
- Choix tactique : souffler avant un boss ? Maintenant ?
- Rompt la monotonie du tap
- Mécanique calme/zen dans un jeu d'action
- Crée un "moment méditation" satisfaisant

---

## 🎨 IDENTITÉ VISUELLE DE COMBAT

### Cinématique d'arrivée d'un Adversaire normal

```
[Phase 1 - mannequin frappé]
    ↓ (barre prochaine adversaire pleine)
[Mannequin fade-out 0.3s]
[Adversaire fade-in 0.3s]
[Affichage nom 1s] : "Ronin Errant" (police pixel ambre)
[Chrono démarre]
```

### Cinématique d'arrivée d'un Capitaine (Boss Mineur)

```
[Écran s'assombrit légèrement 0.5s (vignette)]
[Drum hit medium]
[Mannequin fade-out 0.3s]
[Boss fade-in avec aura colorée 0.5s]
[Affichage lettrage moyen] : "Hattori du Mont" (police pixel, couleur voie)
[Affichage 1.5s puis fade]
[Chrono démarre]
```

### Cinématique d'arrivée d'un Maître (Boss Majeur)

```
[Écran s'assombrit COMPLÈTEMENT 1s (vignette épaisse)]
[Drum hit GRAVE + sound effect mystique]
[Background fade-out → arène thématique fade-in 1s]
[Boss apparait avec aura ÉPAISSE 1s]
[Lettrage MASSIF letter-by-letter 1.5s]:
  "YOSHITSUNE L'INATTEIGNABLE"
[Sous-titre 0.5s] : "Maître Samurai"
[Lettrage fade 0.5s]
[Combat commence avec barre HP STYLÉE]
```

### Cinématique de victoire (boss tué)

```
[Slow motion sur le coup final 0.5s à 30% speed]
[Boss bascule, animation "die" du pack rvros 0.5s]
[Écran flash blanc 0.2s]
[Citation du boss s'affiche centre] : "Le vent ne se laisse pas saisir."
[Loot tombe vers ton inventaire 1s]
[Affichage "VICTOIRE" en doré 1s]
[Background re-fade au normal]
[Retour mannequin]
```

### Cinématique de mort (vs Maître = Prestige)

```
[Slow motion sur le coup fatal du Boss 0.5s à 20% speed]
[Ton perso bascule 0.5s]
[Camera shake léger]
[Écran fade to black 1s]
[Affichage "TU ES MORT" en rouge profond, MASSIF, 2s]
[Sous-titre "Face à Yoshitsune l'Inatteignable" 1s]
[Citation du Maître affichée 2s]
[Voyage Intérieur 4s] : stats du run affichées
[Calcul Échos 1s] : "[X] Échos gagnés"
[Citation du joueur (prompt si absente) 3s]
[Fade to white 1s]
[Renaissance Stade 1 1s]
[Retour gameplay]
```

### Pendant un combat (différences visuelles)

| Élément | Phase 1 Entraînement | Phase 2 Adversaire | Phase 3 Capitaine | Phase 3 Maître |
|---|---|---|---|---|
| Background | Dojo basique | Dojo basique | Dojo + tint léger | **Arène thématique** |
| Vignette | Aucune | Aucune | Subtile | **Épaisse** |
| Camera | Statique | Statique | Statique | **Zoom léger + shake** |
| Musique | Ambient calme | Ambient + drum | Ambient + drums | **Track boss dédié** |
| HP cible | Aucun (mannequin) | Barre simple | Barre stylée | **Barre avec nom + phases** |
| Particules | Légères | Aura ennemi | Aura colorée | **Aura ÉPAISSE + particules** |
| UI | Standard | Chrono + HP | Chrono + HP | **Chrono massif + HP stylée** |

---

## 📊 ÉQUILIBRAGE (à raffiner durant Sprint 4-6)

### Barre "Prochain Adversaire"

- Stade 1 (Mendiant) : 50 taps pour remplir
- Stade 2 (Apprenti) : 100 taps
- Stade 3 (Guerrier) : 200 taps
- Stade 4 (Maître) : 400 taps
- Stade 5 (Légende) : 800 taps
- Stade 6 (Mythe) : 1600 taps

### HP Adversaires

- Adversaire lambda : 100 HP × multiplicateur stade
- Capitaine : 500 HP × multiplicateur stade
- Maître : 5000 HP × multiplicateur stade

### Chrono

- Adversaire : 30 sec
- Capitaine : 60 sec
- Maître : 180 sec (3 min)

### Perte à la mort

- Mort temporaire (Adversaire/Capitaine) : -10% Force totale
- Mort définitive (Maître) : RESET + Échos calculés selon progression

### Échos gagnés au Prestige

- Formule : `floor(log10(Force max atteinte) × 10)`
- Exemple : Force max 1M = log10(1M) × 10 = 60 Échos
- À ajuster selon balance interne

---

## 🗺️ NOUVELLE ROADMAP SPRINT 4-12

### Sprint 4 - Combat Active System (gros sprint)
**Goal** : Phase Adversaire + chrono + mort temporaire

- Système Adversaire avec spawn, HP, chrono
- Barre de progression "Prochain Adversaire" en haut
- 5 types d'adversaires lambda (visuels simples)
- Animation de mort/victoire adversaire
- Mort temporaire avec écran noir + tombe + click-to-resume
- Système de récompenses (Force bonus + chance loot)

**Effort** : 4-6 jours

### Sprint 5 - Boss Mineurs + Élan
**Goal** : Première mécanique active + premiers boss

- Mécanique Élan (jauge + bouton Vague + animation)
- Boss Mineurs (Capitaines) tous les 10 adversaires
- 8 Capitaines (variations visuelles, HP, phases simples)
- Cinématique d'arrivée Capitaine
- Drops de runes/reliques

**Effort** : 4-5 jours

### Sprint 6 - Boss Majeurs + Prestige + Souffle
**Goal** : Le coeur de la boucle, mort narrative

- Mécanique Souffle (méditation, cooldown)
- 8 Boss Majeurs (Maîtres légendaires)
- Cinématique d'apparition épique par boss
- Bouton "Affronter le Maître" volontaire
- Mort face à un Maître = PRESTIGE
- Système prestige (Échos, citation finale, reset)
- Cinématique de Prestige complète (mort + voyage + renaissance)
- Hall des Légendes (citations accumulées)

**Effort** : 6-8 jours (gros sprint narrativement)

### Sprint 7 - Voies + Identité culturelle
**Goal** : Le joueur sent une appartenance à une voie

- Système Voie (Samurai d'abord, puis Viking, Wuxia, etc.)
- VoieData ScriptableObject avec affinity
- Palette dynamique par stade (gris → ambre → coral)
- Audio ambient par voie (placeholder)
- Skin perso évolue selon voie maîtrisée

**Effort** : 5-6 jours

### Sprint 8 - Esprits Compagnons
**Goal** : Profondeur du build

- 8 esprits débloquables
- Slots d'équipement (3 esprits max)
- Effets visuels par esprit
- UI dédiée gestion esprits
- Esprits affectent stats + visuels

**Effort** : 5-6 jours

### Sprint 9 - Carte du monde + Régions
**Goal** : Exploration au-delà du dojo

- Carte navigable (style parchemin Loop Hero)
- 10-15 régions thématiques
- Conquêtes de régions (mini-quêtes)
- Loot exclusif par région
- Voyage avec animation "run" du pack rvros

**Effort** : 5-7 jours

### Sprint 10 - Hub Features + Daily
**Goal** : Rétention et social

- Page stats détaillée (combat, builds, loot, temps, prestiges)
- Achievements + Hall des Légendes complet
- Daily Ronin (1 quête courte par jour)
- Carte de combattant partageable (PNG généré)
- Titre custom + Citation prestige
- Settings page propre

**Effort** : 5-6 jours

### Sprint 11 - POLISH FINAL (le gros polish)
**Goal** : Le jeu passe de "fonctionnel" à "sublime"

- **Fonts custom** : Inter (UI) + JetBrains Mono (chiffres) + Cinzel ou Cormorant (lore)
- **Sound design complet** : tap sounds, combo, achat, palier, boss music
- **Musique** du beatmaker pote intégrée (3-5 tracks)
- **Background dojo** propre (parallax, ambient particles, plancher détaillé)
- **UI vivante** : chaque élément a son micro-animation
- **Palette dynamique** affinée par stade
- **Transitions cinématiques** entre tous les écrans
- **Vibrations haptiques** sur mobile
- **60 FPS** garanti partout
- **Optimisation** mobile (taille app, perfs)
- **Mode portrait** uniquement, format mobile fini

**Effort** : 5-7 jours

### Sprint 12 - Beta testing + ajustements
**Goal** : Préparation lancement

- Beta privée à 10-20 testeurs
- Système de feedback in-app (rapport bug rapide)
- Intégration des retours
- Balance des stats (Force, HP, chronos)
- Préparation stores (App Store + Play Store)
- Trailer marketing
- Page de présentation site (HiddenLab)

**Effort** : 4-5 jours

### TOTAL post-Sprint 3 : ~50-70 jours de dev

Soit environ **2 à 3 mois** au rythme actuel. **Lancement officiel envisageable août-septembre 2026**.

---

## 🎯 IDÉES PRIORISÉES (les 30 idées + validations bonus)

### TOP PRIORITÉ (intégrées dans roadmap)

| # | Idée | Sprint cible |
|---|---|---|
| 1 | **Système Boss/Adversaire/Cycle** | Sprint 4-6 |
| 2 | **Mort narrative (Maître = Prestige)** | Sprint 6 |
| 3 | **Mécanique Élan + Vague AOE** | Sprint 5 |
| 4 | **Mécanique Souffle + meditation** | Sprint 6 |
| 5 | **Cinématiques d'arrivée/mort/victoire** | Sprint 5-6 |
| 6 | **Musique adaptive par palier** | Sprint 7-11 |
| 7 | **UI vivante (tout respire)** | Sprint 11 |
| 8 | **Palette dynamique selon palier** | Sprint 7 |
| 9 | **Page stats détaillée** | Sprint 10 |
| 10 | **Achievements + Hall des Légendes** | Sprint 10 |
| 11 | **Daily Ronin** | Sprint 10 |
| 12 | **Carte de combattant partageable** | Sprint 10 |
| 13 | **Titre custom + Citation prestige** | Sprint 6, 10 |

### MOYENNE PRIORITÉ (Sprint 11+)

| # | Idée | Sprint cible |
|---|---|---|
| 14 | Vibrations haptiques | Sprint 11 |
| 15 | Mode Une main (UX mobile) | Sprint 11 |
| 16 | Sound design ultra-précis | Sprint 11 |
| 17 | Le Voyage Intérieur (cinématique prestige) | Sprint 6 (basique), 11 (poli) |
| 18 | Conseil des Maîtres (commentaires post-mort) | Sprint 11 |
| 19 | Les Reliques Personnelles (conserver 1 item) | Sprint 6 |
| 20 | Transitions cinématiques entre menus | Sprint 11 |

### POST-MVP (post-launch, mois 2-6)

| # | Idée | Quand |
|---|---|---|
| 21 | **Lend Aura async** (prêt de perso) | Mois 2-3 |
| 22 | **Save Cloud** (sync devices) | Mois 2 |
| 23 | **Templates communauté + armes user-generated** | Mois 3-4 |
| 24 | Mur des Légendes Mortes (communauté async) | Mois 4-6 |
| 25 | Défis hebdomadaires | Mois 3 |
| 26 | Reliques quotidiennes | Mois 3 |
| 27 | Communauté de Voies (pages async) | Mois 4-6 |
| 28 | Cartes de visite collectibles | Mois 6+ |
| 29 | Pilgrimage mode (4-12h offline) | Mois 4 |
| 30 | Échos de gloire hebdomadaires | Mois 3 |

### REJETÉ

- ❌ Combos de tap complexes (long press, double tap) - alourdit le tap simple
- ❌ Tension rhythm-game - trop complexe pour idle game
- ❌ Témoignages disciples - rejeté par Ajwad
- ❌ Gacha - contre les valeurs du jeu
- ❌ Leaderboards globaux - coûts + cheaters + anxiété
- ❌ PvP / Guildes - hors scope
- ❌ NFT - lol non
- ❌ Refs pop culture protégées - copyright

---

## 🎁 BONUS FEATURES (re-confirmées)

### Système 3 tiers pour armes communautaires

- **Tier Easter Egg / Custom Hero** : drop ultra rare (1/10000), stats 1/1/1, descriptions libres ("Sabre trop stylé par ma petite soeur")
- **Tier Communauté Standard** : drop normal, stats équilibrées, descriptions style mythique
- **Tier Officiel** : tes propres créations canon

### Outil dessin recommandé

- **Piskel** (gratuit, navigateur) pour les débutants
- **Aseprite** (20€) ou **Krita** (gratuit) pour les avancés

### Stack backend (Sprint 11+ post-MVP)

- **Hébergement** : VPS Ajwad
- **Backend** : Python FastAPI
- **DB** : PostgreSQL ou SQLite
- **Storage assets** : direct disk VPS
- **Auth** : JWT simple

---

## 💰 MONÉTISATION ÉTHIQUE (rappel inchangé)

**4 piliers** :

1. **Skins cosmétiques** (1-3€ piece, 10€ bundle)
2. **Patron Pack** (5-10€ one-time : all skins + 1.5x offline gain + Patron status)
3. **Rewarded ads** (2x pour 1h, jamais imposée, skippable)
4. **Tip Jar** (1/5/10/20€ libre choix)

**Aucun energy timer. Aucun speed-up payant. Aucun loot box. Aucun gacha.**

**Revenue estimé MVP** : ~3000€/an pour 10k downloads. ~30k€/an pour 100k downloads (réaliste si viralisation).

---

## 📅 PROCHAINES ÉTAPES IMMÉDIATES

1. **Validation finale de ce doc par Ajwad** (toi)
2. **Brief Sprint 4 enrichi pour le dev Claude** (à rédiger après validation)
3. **Démarrage Sprint 4** : Combat Active System (Adversaire + chrono + mort temporaire)
4. **Le dev Claude lance** la branche `feat/sprint-4-combat-active`
5. **Première session de combat actif testable** prévue dans 4-6 jours

---

## 🏛️ NOTE FINALE

Ce document est **vivant**. Il sera mis à jour à chaque sprint pour refléter les ajustements de balance, les retours de testeurs, et les décisions design qui émergent en cours de route.

L'identité de SAGA est **maintenant claire** :
- Un idle game qui devient une **expérience narrative mythique**
- Le combat est **actif sans être stressant** (chrono généreux, mort pas frustrante)
- La **mort est un moment de gloire**, pas une punition
- Le **prestige raconte une histoire** personnelle au joueur
- Les **Maîtres légendaires** sont des objectifs durables (1 mois+ pour battre les 8)
- L'**identité culturelle** se révèle progressivement (voies, reliques, lore)

**SAGA n'est pas juste un idle game. C'est un poème jouable sur la mort, la persévérance, et la légende.**

🗡️ **SAGA Project — HiddenLab — Game Design v2.0 — 2026-05-27**
