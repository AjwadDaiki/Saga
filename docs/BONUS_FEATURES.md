# SAGA - Bonus Features (Roadmap étendue)

> Ce document recense toutes les features additionnelles discutées en plus du scope MVP de base.
> Elles sont **ordonnées par priorité d'implémentation et impact joueur**.
> Date de création : 2026-05-27.

---

## 🟢 PRIORITÉ HAUTE (à intégrer pendant le MVP)

Ces features amplifient massivement l'addictivité et l'identité du jeu. Elles s'intègrent naturellement au scope MVP de base.

### 1. Page Statistiques détaillée

**Sprint cible** : 9 (Hub features)
**Effort dev** : ~1 jour
**Backend** : Aucun (données déjà sauvées localement)

Une page profil qui affiche toutes les stats du joueur de manière satisfaisante :

```
🎯 Combat
  - Total taps de ta vie : 1,247,832
  - Frappe la plus puissante : 12.7M
  - Combo max atteint : x2.0 (487 fois)
  - Crits déclenchés : 8,931

⚔️ Builds explorés
  - Voies maîtrisées : 5/8
  - Hybrides découvertes : 3/28
  - Esprits débloqués : 14/20
  - Régions conquises : 7/15

💎 Loot
  - Objets Légendaires : 23
  - Objets Mythiques : 4
  - Objets Sacrés : 0

⏱ Temps
  - Première session : il y a 47 jours
  - Temps de jeu : 89h 12m
  - Session la plus longue : 4h 23m
  - Plus long combo de jours : 31 jours

🌟 Prestiges
  - Total prestiges : 12
  - Échos accumulés : 7,420
  - Première mort mythique : il y a 32 jours
```

**Pourquoi prioritaire** : énorme ROI (1 jour de dev pour beaucoup de dopamine joueur). Les gens ADORENT voir leurs stats. Zéro coût backend.

---

### 2. Achievements + Hall des Légendes

**Sprint cible** : 9
**Effort dev** : 2-3 jours
**Backend** : Aucun

Système de 100+ achievements à débloquer, chacun donnant une rune visible dans le Hall des Légendes.

**Exemples d'achievements** :
- **Premier sang** : 1er prestige effectué
- **Le Patient** : 7 jours de jeu consécutifs
- **Le Maniaque** : 10 000 taps en une session
- **Le Maître des Voies** : 4 voies maîtrisées
- **Tout en silence** : Voie du Néant débloquée
- **L'Œil du Cyclone** : combo x2.0 maintenu 60 secondes
- **Le Cartographe** : 100% des régions conquises
- **L'Écho Mille-Feuilles** : 100 prestiges réalisés

**Pourquoi prioritaire** : trophées = collectionnite, le joueur revient pour compléter sa galerie.

---

### 3. Daily Ronin

**Sprint cible** : 9 (déjà prévu dans la roadmap de base)
**Effort dev** : 1-2 jours
**Backend** : Aucun (calculé sur date système locale)

Une quête courte par jour, skippable sans punition, payoff cool.

**Exemples** :
- "Atteins 100K Force en 10 min"
- "Bat le boss du Mont du Pic Brisé"
- "Tap 500 fois aujourd'hui"
- "Découvre 3 fragments de lore"

**Récompense** : currency boost + chance loot rare + 1 fragment lore exclusif.

**Important** : pas de streak qui se reset. Le joueur ne perd RIEN s'il skip un jour. Juste un bonus s'il participe.

---

### 4. Citation de prestige

**Sprint cible** : 5 (prestige system)
**Effort dev** : ~0.5 jour
**Backend** : Aucun

Quand le joueur meurt en prestige, il peut écrire **sa propre citation** (max 80 caractères) qui sera gravée dans son hall personnel.

Au bout de 20 prestiges, il a 20 citations qui racontent son voyage.

**Exemple** :
> "J'ai cherché le silence. J'ai trouvé le tranchant."

Format Elden Ring/FromSoft. Très narratif, très personnel, énorme valeur émotionnelle.

---

### 5. Titre personnalisé

**Sprint cible** : 6 (esprits) ou 9 (hub)
**Effort dev** : ~0.5 jour
**Backend** : Aucun

À partir d'un certain niveau (ex: 5 prestiges), le joueur peut **créer un titre custom** affiché sur sa carte de combattant.

**Exemples** :
- "Ajwad le Patient"
- "Daiki le Maître des Vents"
- "Le Ronin Sans Étoile"

Petit truc d'identité personnelle, gros impact narratif.

---

### 6. Carte de combattant partageable

**Sprint cible** : 9
**Effort dev** : 1-2 jours
**Backend** : Aucun (image générée localement)

Le joueur tape "Partager mon build", le jeu génère un PNG stylisé qu'il peut sauvegarder/partager sur Discord/Insta.

```
╔═══════════════════════════════╗
║   AJWAD - Maître du Vide      ║
║   Voie : Samurai + Wuxia      ║
║   Stade : Mythe (lvl 6)       ║
║                               ║
║   ⚔️ Katana du 11e Pleur      ║
║   🛡️ Robe du Cerisier        ║
║                               ║
║   👻 Corbeau · Phénix · Loup  ║
║                               ║
║   ⚡ Force Max : 3.7aa        ║
║   🏆 47 Prestiges             ║
║   ⏱ 142h de jeu               ║
║                               ║
║   "Le silence vient avant     ║
║   la lame."                   ║
╚═══════════════════════════════╝
```

**Pourquoi** : viral marketing gratuit, les joueurs flex leurs builds sans backend nécessaire.

---

## 🟡 PRIORITÉ MOYENNE (à intégrer si backend dispo, post-MVP early)

Ces features demandent un backend léger sur ton VPS mais apportent énormément.

### 7. Templates communautaires + armes user-generated

**Sprint cible** : EN COURS dès Sprint 2 (templates générés)
**Effort dev** : ~3-4 jours pour l'Importer Tool + UI
**Backend** : VPS (stockage PNG + métadonnées)

**Status actuel** : Templates générés (voir dossier `saga-templates/`).

**Workflow** :
1. Communauté dessine sur les templates (256x384 corps, 256x256 armes)
2. Soumet PNG + nom + description + culture + rareté suggérée
3. Toi tu valides et définis les stats finales dans Unity
4. L'arme rentre dans le jeu avec crédit `@pseudo` cliquable

**Pourquoi haute valeur** : contenu illimité, gratuit, engagement communautaire, identité forte.

---

### 8. Save Cloud (synchro entre devices)

**Sprint cible** : 11+ (post-MVP)
**Effort dev** : 2-3 jours
**Backend** : VPS + DB

Endpoint simple :
- `POST /save?user_id=X` (upload save chiffrée)
- `GET /save?user_id=X` (download dernière save)

Le joueur peut jouer sur son téléphone, fermer l'app, ouvrir sur tablette, retrouver sa save.

**Sécurité** : save chiffrée côté client avec une key dérivée du device, pas de risque RGPD.

---

### 9. Partage de build via lien

**Sprint cible** : 11+
**Effort dev** : 2 jours
**Backend** : VPS (DB) ou même juste URL-encoded

Le joueur génère un lien `saga.app/build/abc123` qui contient sa config complète. Ses potes l'ouvrent dans LEUR jeu et voient son build en mode lecture seule.

**Variante sans backend** : tout encoder dans l'URL (build = string base64 encodée).

**Avantage** : pousse à essayer les builds des potes = nouvelle session = rétention.

---

### 10. Lend Aura (prêt de perso async)

**Sprint cible** : 11+
**Effort dev** : 3-5 jours
**Backend** : VPS (DB pour les aides actives)

**Mécanique validée** :
- Joueur A active "Aide un ami" → ses stats sont uploadées
- Pendant 4-24h, une silhouette translucide de A apparait dans le dojo de B
- Cette silhouette frappe à un rythme calculé sur les stats de A
- B gagne **+30% Force/sec** pendant la durée
- A ne perd rien, c'est gratuit pour lui
- Cooldown : max 1 aide envoyée par jour

**Concept narratif** : "ton maître mythique vient t'aider de l'au-delà"

**Pas exploitable** : plafonné, async, pas de leaderboard.

---

## 🟠 PRIORITÉ BASSE (long terme post-launch)

Ces features sont du gravy à ajouter si SAGA décolle et qu'il y a une communauté active.

### 11. Mur des Légendes Mortes (communauté async)

**Sprint cible** : post-launch (mois 2-3)
**Effort dev** : 3-4 jours
**Backend** : VPS (feed de citations)

Une page in-app où toutes les morts de tous les prestiges de tous les joueurs défilent en background.

**Exemple** :
> "Maître Hiroshi est mort à l'âge de 47 prestiges, voie Wuxia.
> Dernière citation : 'Le silence m'a appris à parler.'"

Beau, mélancolique, communauté sans compétition. Inspiré des Memorials de FromSoftware.

---

### 12. Défis hebdomadaires

**Sprint cible** : post-launch
**Effort dev** : 2-3 jours
**Backend** : VPS (config des défis hebdo)

Un défi par semaine, plus dur que le Daily Ronin, payoff énorme.

**Exemples** :
- "Termine un run sans jamais cliquer activement"
- "Bat 5 boss en 24h"
- "Atteins le Stade Mythe en moins de 7 jours"

Rotation automatique, optionnel, gros buzz dans la communauté.

---

### 13. Reliques de l'Empire (objet quotidien)

**Sprint cible** : post-launch
**Effort dev** : 1-2 jours
**Backend** : VPS (config quotidienne)

Chaque jour réel, **un objet aléatoire** dans le monde devient "Relique de l'Empire". Si tu le drop ce jour-là, il a un bonus spécial.

Pousse les joueurs à revenir quotidiennement sans être anxiogène.

---

### 14. Communauté de Voies (pages async)

**Sprint cible** : post-launch
**Effort dev** : 4-5 jours
**Backend** : VPS

Chaque voie a une page in-app qui affiche :
- Combien de joueurs maîtrisent cette voie
- Les top builds (titres anonymisés)
- Les citations populaires de prestige
- Les "secrets" découverts par la communauté

Async, cache local, pas de pression compétitive.

---

### 15. Cartes de visite collectibles

**Sprint cible** : post-launch
**Effort dev** : 3-4 jours
**Backend** : Optionnel

Carte de visite custom (genre business card) avec ton perso favori, ta meilleure citation, ta stat préférée.

Partageable, collectionnable. Tes potes peuvent collectionner les cartes de leurs amis.

---

### 16. Pilgrimage mode (voyage long)

**Sprint cible** : post-launch
**Effort dev** : 3-4 jours
**Backend** : Aucun

Un mode optionnel où tu entreprends un voyage entre régions qui prend 4-12h réelles. Récompenses uniques (artefacts, lore exclusif).

Idéal le vendredi soir : tu lances, tu reviens le samedi matin.

---

## ❌ FEATURES REJETÉES (avec raisons)

### Gacha
**Raison** : prédateur, contre les valeurs du jeu, illegal dans plusieurs pays, ta cible déteste ça.
**Alternative validée** : système de loot avec raretés visibles, pity timer, AUCUN achat possible.

### Leaderboards globaux
**Raison** : coûts serveur, anti-cheat impossible, anxiété joueur, casse le ton contemplatif.
**Alternative validée** : Hall des Légendes individuel + Mur des Légendes communautaire async.

### Système de guildes / clans
**Raison** : trop ambitieux pour solo dev, modération lourde, pas dans le scope.

### PvP
**Raison** : hors scope total, contre la philosophie idle/chill.

### NFT / Blockchain
**Raison** : crypto-scam, gaspillage énergétique, public anti, mort marketing.

### Prêt de perso en temps réel (sockets)
**Raison** : complexité énorme pour 0 valeur de gameplay (c'est un idle).
**Alternative validée** : Lend Aura async.

### Refs pop culture directes (Goku, sabre laser, Excalibur design Disney, etc.)
**Raison** : copyright infringement, retrait stores, lettres d'avocats.
**Alternative validée** :
- Noms d'items = mythologie réelle (Sun Wukong, Honjō Masamune, Macuahuitl, Mjölnir, etc.)
- Easter eggs subtils dans les descriptions narratives uniquement
- Création originale qui s'inspire sans copier

---

## 💰 MONÉTISATION ÉTHIQUE (rappel)

**4 piliers** :

1. **Skins cosmétiques** : 1-3€ pièce, 10€ bundle. Variations des stades 5-6 par voie.
2. **Patron Pack** : 5-10€ one-time. Unlock all skins + 1.5x offline gain permanent + statut Patron visible.
3. **Pub optionnelle** : rewarded ads pour 2x gain pendant 1h. Jamais imposée, toujours skippable.
4. **Tip Jar** : 1€/5€/10€/20€ choix libre. Pour les fans qui veulent juste donner.

**Aucun energy timer. Aucun speed-up payant. Aucun loot box. Aucun gacha.**

**Modèle de référence** : Melvor Idle, A Dark Room, Universal Paperclips.

**Revenue estimé MVP** : ~3000€/an pour 10k downloads. ~30k€/an pour 100k downloads (réaliste si viralisation).

---

## 🛠️ STACK BACKEND PROPOSÉE (quand on en aura besoin)

**Hébergement** : VPS personnel d'Ajwad (déjà disponible)

**Stack** :
- **Backend** : Python FastAPI (Ajwad maîtrise Python, FastAPI moderne et rapide)
- **DB** : PostgreSQL (relations) ou SQLite (si simple, sur disque VPS)
- **Storage assets** : direct sur disque VPS, optionnellement MinIO pour S3-compatibility
- **Auth** : JWT simple, pas besoin d'OAuth lourd
- **Rate limiting** : nginx + fail2ban

**Coûts mensuels estimés** : 0€ (VPS déjà payé), uniquement le temps de maintenance (~2h/mois).

---

## ✅ DÉCISIONS VALIDÉES (signature: Ajwad, 2026-05-27)

| # | Feature | Statut |
|---|---|---|
| 1 | Page Statistiques détaillée | ✅ Validé |
| 2 | Achievements + Hall des Légendes | ✅ Validé |
| 3 | Daily Ronin | ✅ Validé (déjà roadmap) |
| 4 | Citation de prestige | ✅ Validé |
| 5 | Titre personnalisé | ✅ Validé |
| 6 | Carte de combattant partageable | ✅ Validé |
| 7 | Templates communautaires | ✅ Validé (en cours) |
| 8 | Save Cloud | ✅ Validé (post-MVP) |
| 9 | Partage de build via lien | ✅ Validé (post-MVP) |
| 10 | Lend Aura async | ✅ Validé (post-MVP) |
| 11 | Mur des Légendes Mortes | 🟡 Validé (long terme) |
| 12 | Défis hebdomadaires | 🟡 Validé (long terme) |
| 13 | Reliques de l'Empire quotidiennes | 🟡 Validé (long terme) |
| 14 | Communauté de Voies | 🟡 Validé (long terme) |
| 15 | Cartes de visite collectibles | 🟡 Validé (long terme) |
| 16 | Pilgrimage mode | 🟡 Validé (long terme) |
| 17 | Stats armes définies par Ajwad à la validation | ✅ Validé |
| 18 | Outil recommandé communauté : Piskel | ✅ Validé |
| 19 | Refs pop : mythologie réelle + easter eggs subtils desc | ✅ Validé (vérifier au cas par cas avec coordinateur) |
| 20 | Témoignages disciples | ❌ Rejeté |

---

## 📅 PROCHAINES ÉTAPES

1. **Sprint 2 en cours** (validation par Ajwad imminente)
2. **Sprint 3** : Stade visuel + perso (décision Spine 2D vs Unity 2D Animation à prendre)
3. **Sprint 4-8** : suivre la roadmap MVP de base (08_ROADMAP.md)
4. **Sprint 9** : intégrer features prioritaires (stats, achievements, daily, carte partageable, titre, citation prestige)
5. **Sprint 10** : polish MVP, prêt pour testeurs externes
6. **Sprint 11+** : intégrer features priorité moyenne (save cloud, partage build, lend aura)
7. **Post-launch** : features long terme selon traction communautaire

---

🗡️ **SAGA Project** — HiddenLab — Working title v1.0
