# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 3 — Stade visuel & character animé (code 100%, validation Editor pending).

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `feat/sprint-3-stade-visuel` (depuis dev).

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins (BreakInfinity, DOTween) | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur ticker, combo 4 tiers, "+X" floating, dust particles, save/reload | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation. UI cards bottom, near-miss glow. | ✅ closed |
| 3 | (pending) | Stade visuel : character animé (Adventurer pixel), mannequin, slash FX, transition cinématique Stade 1→2, NumberJuice | 🟡 code 100%, validation pending |

## Sprint 3 — ce qui est fait ✅

### Direction artistique tranchée + docs
- DESIGN_DECISIONS_LOG.md : entrée "art direction pivot vers Simple Chibi - Pixel Adventurer" + décisions tech (Unity 2D Animation built-in, Pixel Perfect Camera, Filter Point, PPU 32)
- 05_VISUAL_STYLE.md : section Personnage réécrite (chibi pixel proportions 1:2, neutre par défaut). Section Dojo amendée (bg `#1a1a1a` non-noir + Pixel Perfect Camera note)
- 06_TECH_STACK décision Spine vs Unity 2D Animation : tranchée = Unity 2D Animation built-in

### Asset import (Phase B)
- `Assets/_Project/Art/Characters/Adventurer/{Idle,Attack,Hurt}/` — 12 PNGs individuels (4+5+3)
- `Assets/_Project/Art/Characters/Adventurer/_Reference_Spritesheet.png` — original adventurer-Sheet.png pour référence
- `Assets/_Project/Art/FX/Slashes/10_weaponhit_spritesheet.png` — pour Sprint 4+ polish (slicing pas implémenté Sprint 3)
- `Assets/_Project/Art/FX/HitEffect01/` — 3 PNGs simples pour potential crit Sprint 4+
- Templates Eris Esra mis en réserve (downloads/, pas copiés dans le projet pour l'instant)

### Editor utility (Phase C)
- `Editor/AdventurerAssetsConfigurator.cs` : menu `Saga > Sprint 3 > Configure Adventurer Assets`
  - Force Filter Point, PPU 32, Compression None sur tous les PNGs sous Art/Characters et Art/FX
  - Construit `Assets/_Project/Resources/Animations/AdventurerAnimationLibrary.asset` avec idle/attack/hurt sorted by filename
  - Idempotent

### Data (Phase D)
- `Data/SpriteAnimationLibrary.cs` (SO + nested SpriteAnimationData) — collection de frame-by-frame anims avec frameDuration + loop
- `Data/Stade.cs` enum 1..6 (Mendiant → Mythe) — no Divin tier (design verrouillé)
- `Data/GameState.cs` : ajout `currentStade` (default 1), saveVersion bump 2 → 3
- `Save/SaveService.cs` : migration v2 → v3 (init currentStade si nécessaire). Idempotent.

### Services + events (Phase D)
- `Gameplay/StadeManager.cs` (POCO) : table de thresholds Force, méthode statique `ComputeStade(force)`, `Tick(state, dt)` qui raise OnStadeChanged sur transition
- `Core/GameEvents.cs` : nouvel event `OnStadeChanged(int previous, int next)`
- `Core/GameManager.cs` : instancie StadeManager dans Awake
- `Core/GameTicker.cs` : hook `Stades.Tick()` après `Disciples.Tick()`

### Visual runtime (Phase D + E)
- `Gameplay/SpriteAnimator.cs` (MonoBehaviour) : driver frame-by-frame, supporte loop + queueNext (e.g. play "attack" → return to "idle")
- `Gameplay/CharacterView.cs` : écoute OnTapResolved → joue "attack" queueNext "idle". Écoute OnStadeChanged (placeholder pour swap library Sprint 4+).
- `Gameplay/MannequinView.cs` : écoute OnTapResolved → `DOShakePosition` (core DOTween Transform shortcut)
- `Gameplay/SlashFxSpawner.cs` : écoute OnTapResolved → spawn sprite procédural blanc 8×2px entre character et mannequin, scale+fade+rotate via DOTween core. Couleur ambre `#FAC775`.

### Stade transition cinematic (Phase F)
- `UI/StadeTransitionView.cs` : full-screen Image overlay + 2 TMP labels (title "Nouveau stade : Apprenti", subtitle "Stade 2"). Séquence DOTween 0.4s fade-in → 1.2s hold → 0.4s fade-out. Peak alpha 0.75. Ignore les régressions de stade.

### NumberJuice (Phase G)
- `UI/ForceCounterView.cs` : fontSize adaptive (84/96/108/124) + couleur par magnitude :
  - < 100 : gris `#888`
  - < 10k : blanc `#fafafa`
  - < 10M : ambre `#FAC775`
  - 10M+ : coral `#993C1D`

### Scene wiring (Phase H)
- `MainSceneBootstrap.cs` updaté :
  - `EnsureMainCamera()` orthographic, size 4, clear color `#1a1a1a`
  - `WorldRoot` parent pour Character + Mannequin + SlashFxSpawner (world-space sprites)
  - Character position (-1.8, -0.6, 0), Mannequin (1.8, -0.6, 0)
  - Mannequin sprite procédural (16×40 wooden post)
  - Background UI Image retirée (Camera clear color suffit pour fond uni)
  - `BuildStadeTransitionOverlay` ajouté au Canvas
  - `BuildUpgradePanel` ordre préservé

### Tests EditMode (10 nouveaux)
- `Tests/EditMode/StadeManagerTests.cs` : 10 cases (Mendiant à Mythe thresholds, beyond-Mythe cap, Tick raise event only on transition)
- **Total** : 47 (Sprint 1-2) + 10 (Sprint 3) = **57 tests**

## Sprint 3 — Ajwad-pending (3 clics)

1. **Refocus Unity** → compile + génération .meta des nouveaux scripts (~10 fichiers)
2. **Menu `Saga > Sprint 3 > Configure Adventurer Assets`** → applique import settings + crée la `AdventurerAnimationLibrary.asset` dans `Resources/Animations/`
3. **Test Runner** > EditMode > Run All → 57 tests devraient passer
4. **Play mode** Main → vérifier :
   - Camera background `#1a1a1a` (vu)
   - Character à gauche en idle anim (4 frames boucle)
   - Mannequin à droite (poteau de bois procédural)
   - 3 cards upgrades en bas
   - Tap → character joue attack anim → return idle, slash FX ambre, mannequin shake
   - Force counter scale up + couleur change selon magnitude
   - À 1k Force → cinématique "Nouveau stade : Apprenti" overlay 2s
   - Save/quit/reload → état restauré (force + currentStade + upgradeLevels)

## Critère de succès Sprint 3 (cf 08_ROADMAP)

> "tu joues, le perso est à l'écran et idle-breathe ; tu tapes, perso fait attack anim, slash FX apparaît, mannequin shake ; tu atteins 1k Force, mini-cinématique Stade 2 joue"

✅ Code prêt. Validation par Ajwad après les 3 clics ci-dessus.

## Polish items deferred (toujours optionnels)

1. **Refactor `MainSceneBootstrap` → scene-authored UI** : bloqué sur MCP tools (manage_scene flaky)
2. **Fix 43 warnings `IsPointerOverGameObject`** : mérite play test post-fix pour valider UI filter
3. **Import fonts Inter + JetBrains Mono via Font Asset Creator** : requires Editor UI

## Métriques de projet

- **Sprints closed** : 2/11 ; **Sprint en cours validation** : 3
- **Branches** : main + dev à `0de172c` + dev a 2 commits devant (status + content SOs). feat/sprint-3-stade-visuel en cours.
- **Tags** : `v0.1.0`, `v0.2.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~2400 (Sprint 3: +500)
- **Tests EditMode** : 57 cases
- **ScriptableObjects** : 3 upgrades + 1 SpriteAnimationLibrary (à générer via menu)
- **Voies implémentées** : 0/8 (Sprint 4+)

## Questions ouvertes pour le coordinateur

Aucune nouvelle Sprint 3. La décision art direction a été prise en début de session.

**Décisions à valider Sprint 4** :
- Variations per-voie : on commence par swap palette (cheap) ou par swap library (coûteux mais signature) ?
- Stade 3+ visuels : nouveau sprite set par stade, ou même Adventurer avec props/équipement overlay ?

## Prochaine session (Sprint 4)

**Objectif** : première voie complète (Samurai). Cf 08_ROADMAP.md Sprint 4.

- `Data/VoieData.cs` SO + `IVoiePassive` interface
- `Gameplay/SamuraiPassive.cs` (chaque 10e tap × 50)
- Voie Samurai par défaut au démarrage (pas de sélection encore)
- Palette ambre appliquée (default Samurai)
- Audio placeholder Samurai (koto/taiko)
- Lore fragments Samurai (5-10, à débloquer aux milestones)
- Stade 3 et 4 visuels (Guerrier + Maître) Samurai

**Pré-requis avant Sprint 4** : tu valides Sprint 3 (3 clics ci-dessus).

## Notes libres

- Bridge MCP Coplay tools toujours down (resource OK, tools timeout). 3 sprints d'affilée en filesystem-only sans drame.
- Pattern Editor utility (`Saga > Sprint N > Generate/Configure Y`) confirmé top : 2 utilities Sprint 2 + 1 Sprint 3, propre et reproductible.
- L'asset rvros adventurer est solide : 12 frames Sprint 3 (idle/attack/hurt) suffisent largement, et il y a ~60 autres frames dispos dans `Adventurer/Individual Sprites/` pour run, jump, fall, die, etc. — utile pour Sprint 8+ duels actifs.
- Le slash FX procédural (sprite blanc 8×2 stretched + rotated + faded ambre) est volontairement minimaliste — quand on aura le temps Sprint 4-5 polish, on swap pour le frame-by-frame du `10_weaponhit_spritesheet.png` (qu'il faudra slicer via Editor Sprite Editor).
- ForceCounterView fontSize bump rends le compteur très lisible à magnitude. À jouer avec en play test pour vérifier que le ticker animation reste fluide quand la fontSize change pendant l'anim.
- StadeTransitionView pour Sprint 3 c'est juste fade + texte. Sprint 7+ (ink wash transition cinematic per docs) on remplace par quelque chose de plus stylé. Pour l'instant, ça communique clairement le palier.
