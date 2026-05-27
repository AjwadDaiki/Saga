# 04 - PROGRESSION

## Les currencies (4 layers)

### 力 Force (FOR)
- Currency principale, gagnée par tap + disciples
- Sert à: upgrades de base (frappe, disciple, équipement bas tier)
- Reset au prestige

### 技 Technique (TEC)
- Currency intermédiaire, gagnée par combos parfaits, duels actifs, certains achievements
- Sert à: débloquer voies, apprendre techniques, upgrade Esprits
- Reset au prestige

### 名 Renom (REN)
- Currency d'influence, gagnée en conquérant régions, battant boss, recrutant lieutenants
- Sert à: recruter disciples spéciaux, débloquer expéditions longues, certains skins
- Reset au prestige

### 魂 Échos (ECH)
- Meta-currency, gagnée UNIQUEMENT au prestige
- **Persistante entre runs**
- Sert à: bonus permanents, débloquer points de départ alternatifs, voies cachées, esprits exclusifs

## Tiers visuels (6 stades)

Le joueur passe par 6 stades visuels distincts au cours d'un run. À chaque stade, **le perso change physiquement, le dojo change, et une mini-cinématique d'1-2s joue**.

### Stade 1 — Mendiant
- Vêtements en chiffons, mains nues
- Dojo: clairière avec mannequin solo
- Disciples: 0
- Tap basique, pas d'arme
- Seuil: début du jeu

### Stade 2 — Apprenti
- Vêtements simples, première arme (selon voie)
- Dojo: cabane, 2-3 disciples
- Cinématique: le perso reçoit son arme d'un maître anonyme
- Seuil: ~1k Force

### Stade 3 — Guerrier
- Équipement complet de sa culture
- Dojo en bois, 10 disciples
- Cinématique: première victoire en duel
- Seuil: ~100k Force

### Stade 4 — Maître
- Armure complète, premier "stand" (esprit visible)
- Dojo de pierre, structure culturelle
- Cinématique: le maître devient maître (transmission de savoir aux disciples)
- Seuil: ~10M Force ou prestige 1 atteint

### Stade 5 — Légende
- Aura visible permanente, 2 esprits actifs
- Académie légendaire, 50+ disciples
- Cinématique: le nom du joueur est gravé dans la pierre
- Seuil: ~1B Force ou prestige 2 atteint

### Stade 6 — Mythe
- Forme finale (humain mais légendaire), 3 esprits
- Sanctuaire/royaume, généraux nommés, armée
- Cinématique: le joueur devient une histoire qu'on raconte
- Seuil: prestige 3 atteint OU 1aa Force (post-aa notation)

**Important**: pas de stade 7 "Divin". On reste dans le mythique-humain. Voir 01_VISION.md.

## Le prestige (système central)

### Prestige 1 - "Première Mort"
- Déclenché manuellement par le joueur OU forcé à un palier
- Cinématique: duel mythique, le perso meurt avec dignité
- Le joueur choisit sa **Voie Majeure** (parmi les 8)
- Récompense: X Échos basé sur la Force max atteinte
- Reset: Force, Technique, Renom, disciples, équipement (sauf reliques permanentes)
- Garde: skins, achievements, Échos, fragments de lore, voie maîtrisée

### Prestige 2 - "Renaissance Hybride"
- Disponible après le 1er prestige + conquête de 3 régions
- Le joueur choisit une **Voie Secondaire** qui se combine avec la majeure
- Cinématique: deux maîtres anciens transmettent leur savoir
- Récompense: Échos x10 par rapport au prestige 1
- Effet: unlock builds hybrides, nouvelles régions de la carte

### Prestige 3 - "L'Empire Reconstitué"
- Disponible après 5 régions conquises et 2e prestige fait
- Cinématique: vision de l'Empire des Mille Voies
- Le joueur peut maintenant accéder à des voies cachées
- Récompense: Échos x100, déblocage du Stade 6 (Mythe)
- Effet: unlock end-game (challenges, voies cachées, no-tap runs)

### Au-delà du prestige 3
- Le joueur peut chercher les **voies cachées** (5+) via conditions secrètes
- Chaque voie cachée = nouvelle catégorie de run, défi unique
- Pas d'urgence, c'est du contentement long terme

## Les esprits compagnons (15-20 dans le jeu fini)

Liste indicative pour le dev. Chaque esprit a:
- Un nom propre
- Un visuel distinct
- Un passif unique
- Une animation d'idle (orbite, flotte, ondule)
- Une condition de déblocage

### Premiers esprits (MVP)

1. **Corbeau Ancestral** - auto-tap toutes les 2s
2. **Forgeron Divin** - upgrades 10% moins chères
3. **Esprit de la Montagne** - réduit les malus de Fureur (utile pour Gaulois)
4. **Danseur du Vent** - combo multiplier augmenté
5. **Loup Spectral** - bonus actif quand build Berserker
6. **Phénix** - une fois par run, revive après mort (utile pour voies risquées)
7. **Dragon Endormi** - passive minime, mais débloque conditions de voies cachées
8. **Ombre du Passé** - copie 10% des gains dans une currency offline

### Esprits post-MVP

9. **Tigre de Pierre** - défense passive
10. **Serpent Lunaire** - gains x1.5 pendant la nuit IRL du joueur (utilise heure système)
11. **Renard Trickster** - 5% chance de doubler le coût mais aussi le bonus d'un upgrade
12. **Carpe Koi** - gains Renom +50%
13. **Cheval du Vent** - expéditions 2x plus rapides
14. **Aigle des Cimes** - vision révèle des secrets de la carte
15. **Cristal Vivant** - convertit lentement Force en Technique
16. **Murmure des Morts** - lit un fragment de lore par tap silencieux (no-action sur 30s)

Tu débloques tout au long du jeu, équipes 3 max, swap selon le build.

## La carte du monde

Débloquée après le 1er prestige. 10-15 régions au total.

### Régions de base (post-prestige 1)
- **Île du Soleil Levant** (samurai) - boss: Le Dernier Ronin
- **Forêts de Brocéliande** (gaulois) - boss: Le Chêne Rouge
- **Désert d'Hégire** (saladin) - boss: Le Sultan Voilé
- **Cité aux Trois Portes** (spartiate) - boss: Léonidas Errant
- **Fjords du Givre** (viking) - boss: Bjorn Sans-Tête
- **Steppes du Khan** (mongol) - boss: Subötaï l'Aveugle
- **Pyramides du Sang** (aztèque) - boss: Le Prêtre Solaire
- **Monastère du Pic Blanc** (wuxia) - boss: Le Moine Oublié

### Régions mythiques (post-prestige 2)
- **Mont du Pic Brisé** - région unique, débloque accès aux régions mythiques
- **Cité des Échos** - capitale légendaire de l'Empire des Mille Voies, end-game

### Loot et boss
Chaque boss drop un objet légendaire/mythique signature de sa culture. Le joueur peut farmer en refaisant la région après l'avoir conquise (cooldown 24h IRL).

## Équipement (5 slots, 5 raretés)

### Slots
1. **Arme principale**
2. **Arme secondaire / Talisman**
3. **Armure** (corps)
4. **Casque / Couvre-tête**
5. **3 slots Esprit** (équipement séparé visuellement)

### Raretés
- Commun (gris)
- Affûté (blanc)
- Légendaire (ambre)
- Mythique (coral)
- Divin **renommé**: **Sacré** (violet) - pour rester dans le ton mythique-humain

### Set bonuses
Porter plusieurs pièces de la même culture = bonus de set incrémental:
- 2 pièces: +10% stat principale de la voie
- 3 pièces: passive secondaire ajoutée
- 4 pièces: passive ultime débloquée

Ça pousse à thématiser ses builds et à farmer pour des sets.

### Crafting (post-MVP)
- Fondre 3 légendaires de même type = 1 mythique aléatoire
- Forge avec mini rythm-game pour qualité (timing pour stats)
- Reroll d'affixes avec currency dédiée

## Daily Ronin

Quête quotidienne courte. Reset à minuit IRL du joueur (basé sur fuseau système).

- Difficulté modulée selon progression du joueur
- Payoff: 1 currency boost + 1 fragment de lore + chance loot rare
- **Skippable sans punition**. Pas de streak qui se reset.
