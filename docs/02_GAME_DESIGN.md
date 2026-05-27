# 02 - GAME DESIGN

## Core loop

### Loop par seconde (gameplay actif)
- Tap sur la zone de combat (mannequin ou ennemi)
- Génère X Force (X dépend des upgrades)
- Build combo si tap rapide consécutif
- Trigger crit si chance roll (modifiée par voie/équipement)

### Loop par minute
- Accumuler Force via taps + disciples auto
- Acheter 1 ou 2 upgrades visibles (Frappe, Disciple, Maître, etc.)
- Voir un milestone visuel/sonore (palier, animation, son)

### Loop par session (10-30 min)
- Atteindre le prochain stade visuel OU
- Débloquer une nouvelle mécanique OU
- Conquérir une région sur la carte OU
- Compléter un duel actif

### Loop par jour
- Récupérer gains offline accumulés
- Daily Ronin (1 quête courte, payoff cool)
- 1 à 3 upgrades majeures
- Idéalement 1 unlock structurel (esprit, voie, région)

### Loop par semaine
- Premier prestige (sortie de la première voie)
- Reset partiel, garde Échos
- Choisit voie majeure différente OU réessaie la même avec un autre angle

### Loop par mois
- Deuxième prestige (voie hybride)
- Découverte de voies cachées
- Run "challenge" (no-tap, no-disciple, mono-currency)

## Les piliers d'addiction (à intégrer obligatoirement)

Cette liste est la **checklist non-négociable** pour le sentiment du jeu.

### 1. Numbers go up (avec juice)
- Chiffres qui scale en taille selon la magnitude
- Couleur qui change (gris → blanc → ambre → coral) selon les paliers
- Format auto: 1.2K, 1.2M, 1.2B, 1.2aa (notation alphabétique après le quadrillion)
- Animation de tick: le compteur ne saute pas, il scrolle de l'ancienne valeur à la nouvelle en 0.2s

### 2. Variable rewards
- Crits random sur tap (5% base, +1% par tier de Technique)
- Drops aléatoires sur boss et milestones (loot tier-based)
- Daily Ronin avec récompense surprise (pas connue à l'avance)

### 3. Near-miss design
- "Plus que 47 Force pour upgrade !" affiché en bas du bouton quand on est proche
- Progress bar du prochain stade toujours visible
- Couleur de l'UI qui devient plus chaude quand un milestone est imminent

### 4. Visual milestone shocks
- Tous les 5-10 min de jeu actif pendant les 2 premières heures: un changement visible (perso, dojo, carte)
- Tous les 30-60 min: une nouvelle mécanique débloquée
- Cinématique courte (1-2 sec SVG anim) à chaque passage de stade

### 5. Offline progression
- Cap à 8h max (pas 24h, on veut que le joueur revienne sans être puni)
- Au retour: pop-up "Maître, vos disciples ont continué l'entraînement. +1.2M Force récoltés."
- Bonus de 1.5x si le joueur revient dans la fenêtre des 4-8h post-départ (encourage les retours intermédiaires)

### 6. Daily login (sans punir le skip)
- Pas de "streak" qui se reset (toxique)
- Daily reward simple: 1 currency boost, 1 fragment de lore, 1 chance de drop rare
- Si le joueur skip un jour, il ne perd rien, juste le bonus de ce jour

### 7. Achievements visibles
- Hall des Légendes accessible depuis le menu
- Chaque succès débloque une rune affichée dans le hall
- Achievements visuels = trophées avec animations

### 8. Audio crescendo
- Musique qui monte en intensité selon les gains/sec
- Drum hit à chaque palier
- Son de tap qui change selon la voie maîtrisée
- Crit = son spécial (cymbal crash style mais subtil)

### 9. Number juice (rules)
- Tout chiffre qui apparait à l'écran doit avoir une animation de spawn (scale 0 → 1 + alpha 0 → 1 en 0.15s)
- Tout chiffre qui change doit ticker (pas teleport)
- Les +X de tap doivent flotter vers le compteur cible et disparaître dedans

### 10. Pity timer caché
- Si pas de loot légendaire en 30 boss, garantie au 31e
- Si pas de crit en 50 taps, prochain est forcé crit
- Le joueur ne le sait pas mais sent que c'est juste

### 11. Choix irréversibles dans le run
- Une fois Samurai choisi pour un run, pas de retour à Gaulois sauf prestige
- Crée la curiosité pour les autres voies = pousse au prestige
- Voir 03_CULTURES.md pour les détails

### 12. Lore drip
- Une ligne de texte cryptique débloquée par milestone
- Item descriptions style Elden Ring
- Phrases des disciples au tap-long sur eux
- 200+ fragments de lore au total dans le jeu fini

## Mécaniques détaillées

### Le tap

- Zone de tap: centre de l'écran, large (mannequin + perso visible).
- Tap = +Force (multiplié par stats et voie)
- Tap rapide = combo (multiplicateur croissant: x1.0 → x1.2 → x1.5 → x2.0 à 10 taps)
- Combo expire 1.5s après le dernier tap
- Crit visuel: shake + flash + son spécial

### Les disciples (passifs)

- Chaque disciple génère X Force/sec
- Upgrade-able individuellement OU en bulk
- Pré-affecté à un type de tâche (frappe, méditation, ravitaillement)
- À partir du tier 4: lieutenants nommés avec backstory et passive unique
- Tier 7: armée de 100+, généraux nommés, mini-events internes (rivalités, trahisons)

### Les esprits compagnons

- Tu en débloques 15-20 au total dans le jeu fini.
- Tu en équipes 3 simultanément.
- Chaque esprit a un passive distinct (auto-tap, cost reduction, combo multiplier, defense, etc.)
- Visuellement: orbite autour du perso, animation distincte par esprit
- Voir liste complète dans 04_PROGRESSION.md

### Les duels actifs (anti-passivité)

- Toutes les ~10 min de jeu actif OU sur déclencheur (boss, palier)
- Mini rythm-game: 30-60 secondes
- 3 inputs: tap (timed), hold, swipe directionnel
- Récompense: loot rare, technique unique, currency exclusive, fragment lore
- **Optionnel**: le joueur peut décliner sans perte (juste pas la récompense)
- Boss de région: duel obligatoire avec mécaniques spécifiques

### Les expéditions (offline-active hybride)

- Tu envoies 3-10 disciples conquérir une zone
- Durée: 1h à 8h temps réel
- Pendant ce temps: pas dispo dans ton dojo
- Au retour: loot, currency, fragments de lore
- Risque: petit % de perte (1 disciple peut ne pas revenir, narrativement justifié)

### La carte du monde

- Débloquée après le 1er prestige
- 10-15 régions au total
- Chaque région: passive bonus permanent + currency unique + boss culturel
- Conquérir une région = débloquer la voie correspondante
- Régions mythiques (post-2e prestige): Olympe, Asgard, Takamagahara, Aaru, etc. - mais on garde le ton "mythe humain" (ce sont des terres légendaires, pas des dimensions divines)

## Monétisation éthique

**Aucun paywall.** Tout débloquable en jouant.

- **Skins premium**: variations cosmétiques des stades 5-6. 1-3€ chacun. Bundles à 10€.
- **Patron pack**: 5-10€ achat unique = unlock all skins + 1.5x offline gain permanent (QoL, pas pay-to-win)
- **Pub optionnelle**: rewarded ad pour 2x gain pendant 1h, jamais imposée, skippable
- **Pas de gacha. Pas de loot box payantes. Jamais.**

Modèle de référence: Melvor Idle, A Dark Room, Universal Paperclips. Respect du joueur.

## Anti-patterns à éviter absolument

- **Energy timers** ("vous avez 5 énergies, attendez 1h ou payez") - banni
- **Speed-up payant** ("votre upgrade prend 4h, payez 0.99€ pour skip") - banni
- **Wave de pubs forcées entre actions** - banni
- **Fake scarcity** ("Plus que 23 minutes pour acheter ce pack!") - banni
- **Pop-up de daily reward immédiat à l'ouverture** - banni, on attend que le joueur ait fait UN tap d'abord
- **Notifications guilt-trip** ("Vos disciples vous attendent depuis 3 jours") - banni
- **Streak punishment** - banni

## Sound design (principes)

- Tap basique: percussion sourde, japon-flavored (mais subtle, pas folklo)
- Crit: cymbal courte + harmonique cristalline
- Achievement: trois notes ascendantes + reverb
- Milestone visuel: drum hit + son d'ambiance qui change subtilement
- Musique de fond: changement selon la voie maîtrisée (samurai = koto + taiko, viking = drums + horn, etc.)
- Settings: musique séparée du sound design dans les options (les gens mettent souvent que les FX)
