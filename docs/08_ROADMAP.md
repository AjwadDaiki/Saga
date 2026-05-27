# 08 - ROADMAP

Plan de développement par sprints. **Chaque sprint est indépendamment testable**. On ne passe au sprint N+1 que si N est jouable et propre.

## Sprint 0 - Setup (1-2 jours)

**Objectif**: avoir un projet Unity bootstrappé propre avec toutes les libs et la structure.

- [ ] Créer le projet Unity 6 LTS avec URP 2D
- [ ] Setup Git + .gitignore Unity + Git LFS
- [ ] Configurer la structure de dossiers (voir 06_TECH_STACK.md)
- [ ] Importer Packages: TextMeshPro, Addressables, Input System, Cinemachine, 2D Animation, Localization, Newtonsoft.Json
- [ ] Importer BreakInfinity.cs depuis GitHub
- [ ] Importer DOTween (free pour commencer)
- [ ] Setup target platforms (iOS/Android) dans Build Settings
- [ ] Créer scenes: Boot, Main, Map, Prestige (vides pour l'instant)
- [ ] Créer le GameManager skeleton avec singleton
- [ ] Créer le SaveService skeleton (load/save JSON vide)
- [ ] Setup Localization package avec FR/EN tables vides
- [ ] Premier build sur device Android et iOS pour vérifier que tout pipeline marche
- [ ] Commit initial sur `main`

**Critère de succès**: app démarre sur device, écran noir, log "GameManager OK", build moins de 80MB.

## Sprint 1 - Core tap loop (3-5 jours)

**Objectif**: le joueur peut tap sur l'écran, voir un nombre monter, c'est satisfaisant.

- [ ] Implémenter `GameState` avec champs Force (BigDouble)
- [ ] Implémenter `NumberFormatter` avec tests unitaires (K, M, B, T, aa, bb...)
- [ ] Créer scène Main avec:
  - Background flat dark (placeholder)
  - Zone de tap centrale (large)
  - Compteur Force en haut (TextMeshPro)
- [ ] Implémenter tap detection (Input System)
- [ ] Au tap: +1 Force, animation +1 floating, animation compteur ticker
- [ ] Particules simples sur tap (poussière)
- [ ] Save automatique à chaque tap (throttlé à 1/sec max pour perf)
- [ ] Audio: son de tap basique (placeholder ok)
- [ ] Système de combo: si tap dans 1.5s du dernier, multiplicateur x1.0 → x2.0 sur 10 taps consecutifs
- [ ] Affichage combo dans coin haut-droit

**Critère de succès**: tu peux ouvrir le jeu, taper 30 secondes, le compteur monte avec juice, ça sauvegarde, ça reload bien.

## Sprint 2 - Upgrades de base (3-5 jours)

**Objectif**: le joueur peut acheter des upgrades qui changent ses gains.

- [ ] Implémenter `UpgradeData` SO avec: ID, nom, cost base, cost multiplier, effect type, effect value
- [ ] Implémenter 3 upgrades initiales:
  - "Frappe" - +X Force/tap (cost: 10 base, x1.15 par level)
  - "Disciple" - +X Force/sec passive (cost: 50 base, x1.20)
  - "Méditation" - +5% combo multiplier (cost: 200 base, x1.50)
- [ ] UI panneau upgrades (3 cards en bas de l'écran)
- [ ] Affichage cost + effet + niveau actuel
- [ ] Bouton désactivé si pas assez de Force
- [ ] Animation d'achat (scale + sound)
- [ ] Implémenter GameTicker 10Hz pour les passives (disciples)
- [ ] Premier "near-miss" UX: bouton qui glow quand on est à <20% du coût

**Critère de succès**: tu peux farmer 30 minutes et acheter une dizaine d'upgrades, sentir une progression, l'auto-tap de disciples fonctionne.

## Sprint 3 - Stade visuel et milestone (4-6 jours)

**Objectif**: le joueur voit son perso changer visuellement aux paliers.

- [ ] Implémenter `StadeManager` qui surveille les seuils
- [ ] Setup rig perso Stade 1 (silhouette mendiant) + Stade 2 (apprenti avec katana)
- [ ] Animations idle + tap-react (Unity 2D Animation ou Spine selon décision)
- [ ] Background dojo Stade 1 + Stade 2
- [ ] Au passage de stade:
  - Mini-cinématique 1-2s (ink wash transition, swap des sprites, drum hit)
  - Pop-up "Nouveau Stade: Apprenti" avec animation
  - Save immédiat
- [ ] Mannequin évolue aussi (Stade 1 = poteau, Stade 2 = mannequin entraînement)
- [ ] Sound design: ambient loop différent par stade
- [ ] Implémenter le NumberJuice complet (taille de chiffre selon magnitude, couleur selon palier)

**Critère de succès**: tu joues, tu atteins le Stade 2, tu vois clairement le changement, ça donne envie d'aller au Stade 3.

## Sprint 4 - Première voie complète (Samurai) (5-7 jours)

**Objectif**: la voie Samurai entièrement implémentée comme template pour les autres.

- [ ] Implémenter `VoieData` SO avec tous les champs
- [ ] Implémenter `IVoiePassive` interface
- [ ] Implémenter `SamuraiPassive` (chaque 10e tap x50)
- [ ] Au démarrage, voie Samurai assignée par défaut (pas de choix encore)
- [ ] Palette Samurai appliquée (ambre comme accent)
- [ ] Audio Samurai (koto/taiko, placeholder)
- [ ] Lore fragments Samurai (5-10 fragments, à débloquer aux milestones)
- [ ] Stade 3 et 4 visuels Samurai (Guerrier + Maître)
- [ ] Test complet d'une session de 1-2h

**Critère de succès**: tu peux jouer 2h en Samurai, atteindre Stade 4, sentir l'identité de la voie.

## Sprint 5 - Système de prestige (4-6 jours)

**Objectif**: le joueur peut prestige et garder des Échos.

- [ ] Implémenter `PrestigeService` avec calcul Échos basé sur Force max atteinte
- [ ] UI bouton "Prestige" disponible à partir d'un seuil (ex: 1M Force)
- [ ] Cinématique de prestige (ink dispersal, fade to black, fade back)
- [ ] Reset des currencies Force/Technique/Renom
- [ ] Garde Échos, achievements, lore, voies maîtrisées
- [ ] Écran de choix de voie post-prestige (8 voies, mais 4 implémentées en MVP, 4 grisées "à venir")
- [ ] Implémenter les 3 autres voies du MVP: Wuxia, Spartiate, Viking
- [ ] Pour chacune: passive, palette, audio, lore basique

**Critère de succès**: tu peux faire ton premier prestige, choisir une autre voie, sentir une boucle complète.

## Sprint 6 - Esprits compagnons (4-5 jours)

**Objectif**: système d'esprits avec 8 esprits MVP.

- [ ] Implémenter `EspritData` SO + `IEspritPassive`
- [ ] Implémenter 8 esprits MVP (voir 04_PROGRESSION.md liste 1-8)
- [ ] UI inventaire esprits + slots équipés (3 slots)
- [ ] Visuel esprit qui orbite autour du perso quand équipé
- [ ] Conditions de déblocage (achievement-based)
- [ ] Sauvegarde de l'équipement

**Critère de succès**: tu débloques des esprits, tu en équipes 3, tu sens leur effet sur le gameplay.

## Sprint 7 - Carte du monde (5-7 jours)

**Objectif**: carte avec 4 régions navigables.

- [ ] Scene Map dédiée avec ink wash transition
- [ ] 4 régions sur la carte (Samurai, Wuxia, Spartiate, Viking)
- [ ] Points de la carte navigables
- [ ] UI région: stats, boss, ressource, lore preview
- [ ] Boss kill = duel actif (Sprint 8) - placeholder pour l'instant
- [ ] Au début, seule la région Samurai est dispo, les autres se débloquent par progression

**Critère de succès**: tu peux ouvrir la carte, voir tes régions, naviguer.

## Sprint 8 - Duels actifs (rythm-game) (6-8 jours)

**Objectif**: mini rythm-game pour les boss et les duels optionnels.

- [ ] Setup système de timeline d'attaques (signals à des timestamps)
- [ ] 3 types d'input: tap timed, hold, swipe directionnel
- [ ] Scoring system (Perfect, Good, OK, Miss)
- [ ] Récompense fin de duel selon score
- [ ] 1 boss complet (Le Dernier Ronin, Île du Soleil Levant)
- [ ] Musique et FX pendant le duel
- [ ] Duels optionnels qui apparaissent toutes les 10 min de gameplay actif

**Critère de succès**: tu peux faire un boss complet en rythm-game, sentir l'intensité.

## Sprint 9 - Hub des features secondaires (5-7 jours)

**Objectif**: lore, achievements, settings, hall des légendes.

- [ ] Hall des Légendes (achievements + runes visuelles)
- [ ] Codex de lore (tous les fragments lus)
- [ ] Settings (audio, langue, notifications, accessibilité)
- [ ] Daily Ronin (quête quotidienne)
- [ ] Offline progression complète (calcul au boot)
- [ ] Notifications mobiles (avec parcimonie)

**Critère de succès**: tout le contenu secondaire est accessible et fonctionne.

## Sprint 10 - Polish et juice (5-7 jours)

**Objectif**: le jeu est satisfaisant, juteux, fini.

- [ ] Audit complet de la juice (tap, transitions, milestones)
- [ ] Polish des animations (timing, easing)
- [ ] Optimisation des particules
- [ ] Tests sur devices low-end (target 30fps stable)
- [ ] Audit accessibilité (taille de tap targets, contraste, dyslexia-friendly font option)
- [ ] Localization complete FR + EN
- [ ] Build release-ready

**Critère de succès**: le jeu est en état d'être montré à des testeurs externes.

## Sprint 11+ - Post-MVP

Backlog des features post-MVP:
- 4 voies restantes (Saladin, Viking complet, Mongol, Aztèque)
- Système d'expéditions complet
- Crafting et reroll d'équipement
- Voies hybrides (post-prestige 2)
- Voies cachées
- Régions mythiques
- Esprits 9-16+
- Monétisation (skins premium, patron pack)
- Build et soumission stores

## Décisions à prendre tôt (à valider par le coordinateur)

- **Spine 2D ou Unity 2D Animation?** À trancher au Sprint 3 (skin perso). Default: Unity 2D Animation (gratuit, suffit pour MVP).
- **FMOD ou Unity Audio?** À trancher au Sprint 4 (audio voie). Default: Unity Audio (suffisant pour MVP).
- **Online sync de save?** À trancher en post-MVP. Default: pas pour MVP (offline-only).

## Estimation totale MVP

- Sprint 0 à 10 = environ **45-65 jours de dev solo full-time** pour un dev confirmé
- En part-time / soir et weekend: 3-5 mois réalistes
- Avec aide d'un Claude dev en coding: peut diviser par 2 sur la partie code, mais l'art et le polish prennent le même temps

## Note pour le dev Claude

- **Ne pas sauter de sprint**. Si Sprint 2 a un bug, on le corrige avant Sprint 3.
- **Ne pas implémenter de feature non listée**. Si une idée surgit, l'ajouter au backlog et demander au coordinateur via Ajwad.
- **Toujours updater CURRENT_STATUS.md** en fin de session avec ce qui a été fait, ce qui est en cours, les questions ouvertes.
- **Préférer une feature finie qu'à 60% à trois features à 20%**.
