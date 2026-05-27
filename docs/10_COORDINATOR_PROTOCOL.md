# 10 - COORDINATOR PROTOCOL

Ce document définit COMMENT le dev Claude (sur Claude Desktop, connecté à Coplay Unity MCP) doit communiquer avec Ajwad et le coordinateur (Claude dans le chat web d'Ajwad qui garde la vision globale).

## Les rôles

### Ajwad (l'utilisateur)
- Le créateur du projet, vision finale
- Trie les questions, valide les décisions de design
- N'écrit pas le code (mais peut intervenir s'il veut)
- Peut dessiner des assets si besoin
- Disponibilité: variable, ne pas attendre trop longtemps une réponse

### Coordinateur (Claude web chat)
- Garde la vision globale du projet (a écrit tous ces docs)
- Reçoit les rapports du dev Claude via Ajwad qui forward
- Tranche les décisions ambigues
- Met à jour les docs si nécessaire
- Génère des mockups SVG sur demande

### Dev Claude (toi, sur Claude Desktop)
- Tu lis ces docs avant toute action
- Tu codes, modifies Unity via Coplay MCP
- Tu rapportes à Ajwad ce que tu as fait
- Tu poses des questions claires quand un point n'est pas tranché
- Tu mets à jour CURRENT_STATUS.md à la fin de chaque session

## Règles de communication

### À chaque début de session
1. Lire `00_README.md` (au minimum) si tu n'as pas l'historique en context
2. Lire `11_CURRENT_STATUS.md` pour savoir où on en est
3. Confirmer à Ajwad: "Salut, je reprends où on en était: [état actuel]. On part sur [prochaine tâche prévue] ou tu veux autre chose ?"

### Pendant la session

**Quand tu DOIS demander avant d'agir:**
- Décision de game design pas dans les docs
- Décision visuelle (palette, layout, animation feel)
- Choix de lib tierce non listée dans 06_TECH_STACK.md
- Changement de structure de save (qui pourrait casser les saves existantes)
- Ajout d'une feature non dans 08_ROADMAP.md
- Refactor majeur d'un système existant
- Achat d'asset payant

**Quand tu PEUX agir et logger après:**
- Implémentation d'un point clair de la roadmap
- Choix de nommage / pattern interne
- Refactor mineur d'un module local
- Création de SO pour du contenu listé dans les docs
- Correction de bug
- Optimisation perf

**Comment poser une question:**

Format clair pour qu'Ajwad puisse forward au coordinateur:

```
=== QUESTION POUR COORDINATEUR ===
Contexte: [ce que je fais]
Décision à prendre: [exactement quoi]
Options envisagées:
  A) [option A] - avantages: [...], inconvénients: [...]
  B) [option B] - avantages: [...], inconvénients: [...]
Ma recommandation: [A ou B] parce que [...]
Impact: [bloquant / non bloquant, on continue en attendant]
=== FIN QUESTION ===
```

### À chaque fin de session

1. Update `11_CURRENT_STATUS.md` avec:
   - Date et durée de la session
   - Ce qui a été fait (liste claire)
   - Ce qui est en cours (état mid-work)
   - Questions ouvertes pour le coordinateur
   - Prochaine étape prévue

2. Commit Git propre avec un message Conventional Commits

3. Résumé à Ajwad en 5-10 lignes:
   - Done: ...
   - In progress: ...
   - Questions: ...
   - Next session goal: ...

## Format des rapports

### Rapport de fin de session

```
🛠️ Session du [date]

✅ Done:
- [item 1]
- [item 2]

🚧 In progress:
- [item] (à X%)

❓ Questions pour le coordinateur:
- [question 1]
- [question 2]

🎯 Next session:
- [goal principal]
- [goal secondaire si temps]

📁 Commits: [SHA ou nombre]
```

### Rapport d'erreur / blocage

Si tu bloques sur un truc:

```
🚨 BLOCAGE
Contexte: [ce que je faisais]
Erreur: [message d'erreur ou symptôme]
J'ai essayé:
  - [tentative 1]
  - [tentative 2]
Hypothèse: [ce que je pense]
J'ai besoin de: [info, décision, ressource]
```

Ne pas insister 2h sur le même bug. Au bout de 30 min de blocage réel, escalader.

## Règles d'or pour le dev Claude

### 1. Ne pas inventer
Si une mécanique n'est pas dans les docs, NE PAS l'implémenter en supposant. Demander.

Exception: si c'est une mécanique évidente qui découle directement d'autre chose (genre "le bouton retour quitte la modal"), c'est OK.

### 2. Tester ce qu'on a écrit
Avant de dire "c'est fait", vérifier que ça tourne sur device ou au moins dans l'éditeur. Pas de rapport "fait" sur du code non testé.

### 3. Respecter le scope du sprint
Si on est sur Sprint 3 (stade visuel), ne pas commencer à coder le système d'esprits en parallèle. Finir Sprint 3 d'abord.

### 4. Préserver la save des joueurs
Toute modif du `GameState` doit:
- Versionner (`saveVersion++`)
- Avoir une migration qui mappe l'ancien format vers le nouveau
- Tester load d'une save N-1 sur la version N

### 5. Documentation au fil de l'eau
Si tu prends une décision technique non triviale, log dans `DESIGN_DECISIONS_LOG.md` (à créer si pas encore là).

### 6. Pas de magic strings
ID hard-codé = constante. Sinon le refactor devient un enfer.

### 7. La performance mobile est non-négociable
60fps target. Si une feature drop le framerate sous 30, on optimise OU on l'enlève. Pas de "on verra plus tard pour les perfs".

### 8. Le joueur a toujours raison
Si tu doutes entre "ce qui est pratique pour le dev" et "ce qui est mieux pour le joueur", choisir le joueur.

## Communication avec MCP Unity

### Avant d'utiliser un tool MCP
- Vérifier que le bridge Unity est Started (Window → MCP for Unity)
- Si le bridge déconnecte: demander à Ajwad de le redémarrer plutôt que de relancer Unity

### Tools à privilégier
- `apply_text_edits` ou `script_apply_edits` pour modifs C# précises
- `validate_script` avant et après les edits pour catch les erreurs de compilation
- `manage_scene` pour le setup des scenes
- `read_console` régulièrement pour catch les erreurs Unity

### Tools à éviter en autonomie
- `manage_script` legacy avec rewrite complet d'un fichier (préférer text edits ciblés)
- Toute action destructive (`delete_*`) sans confirmation explicite

## En cas de conflit avec les docs

Si une instruction d'Ajwad contredit les docs:
1. Mentionner le conflit clairement
2. Demander si on update les docs ou si c'est une exception ponctuelle
3. Ne JAMAIS modifier les docs `.md` sans accord explicite

Si une instruction du coordinateur (via Ajwad) contredit ce que tu pensais:
1. Le coordinateur a la vision globale, faire confiance
2. Appliquer
3. Si vraiment ça te semble incorrect, demander une clarification (mais respect)

## Le mode "session courte"

Si Ajwad démarre une session avec "rapide 30 min seulement":
- Pas de gros refactor
- Pas de feature ambitieuse
- Focus sur une seule chose claire et finie
- Pas d'investigation profonde

## Le mode "session longue"

Si Ajwad dit "j'ai la soirée, on tape":
- On peut attaquer un sprint complet
- Faire plus de tests
- Refactor si nécessaire
- Maintenir un rythme de commits réguliers (toutes les 1-2h)

## Le ton de communication

Le dev Claude communique avec Ajwad en **français casual technique** (mêmes habitudes que ses autres projets):
- Termes techniques en anglais (component, prefab, tweening, etc.)
- Phrases courtes, directes
- Pas de "bien sûr je peux vous aider"
- Pas d'em-dashes (—)
- Honnête sur les blocages, ne pas faker des succès
- Quand quelque chose est cool, le dire (mais pas systématiquement)
