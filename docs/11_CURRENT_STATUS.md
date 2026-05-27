# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 2 - Upgrades de base (code 100%, validation Editor pending)

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code

## Sprint 2 — ce qui est fait ✅

### Data layer (Phase A)
- `Data/UpgradeData.cs` ScriptableObject : ID, displayName, descriptionFr, costBase, costMultiplier, effectType, effectValue, icon. Méthode `GetCostForLevel(int)` = `base × multiplier^level`. OnValidate vérifie la cohérence éditeur. Factory `CreateForTests` sous `#if UNITY_INCLUDE_TESTS` pour les tests EditMode.
- `Data/UpgradeEffectType.cs` enum : `ForcePerTap`, `ForcePerSecond`, `ComboMultiplierBonus`
- `Data/GameState.cs` : ajout `upgradeLevels: Dictionary<string, int>`, `saveVersion` bumpé de 1 → 2
- `Save/SaveService.cs` : méthode `Migrate(GameState)` idempotente. v1 → v2 init upgradeLevels à dict vide.

### Services (Phase B)
- `Core/ContentDatabase.cs` : loads upgrades from `Resources/Upgrades/` (production) ou via DI (tests). API `GetUpgrade(id)`, `AllUpgrades`. Doublons d'ID warned.
- `Gameplay/UpgradeService.cs` : `GetLevel`, `GetCostForNextLevel`, `CanAfford`, `TryPurchase` (atomic: deduct + level++ + raise events + MarkDirty)
- `Gameplay/StatsCalculator.cs` : aggregator pur (`GetForcePerTap`, `GetForcePerSecond`, `GetComboMultiplierBonus`)
- `Gameplay/DisciplesProcessor.cs` : tick 10Hz, accrue `forcePerSec × dt` à state.force
- `Core/GameTicker.cs` : hook `Disciples.Tick()` avant le save throttle
- `Core/GameManager.cs` : instancie Content + Upgrades + Disciples au Awake. Log inclut le count d'upgrades.
- `Core/GameEvents.cs` : `OnComboChanged` signature étendue à `(int tier, float baseMultiplier)`, ajout `OnUpgradePurchased(string id, int newLevel)`
- `Gameplay/ComboSystem.cs` : update raise signature pour fournir baseMult
- `Gameplay/TapHandler.cs` : utilise `StatsCalculator.GetForcePerTap` + applique `× GetComboMultiplierBonus`. Field `BaseGainPerTap` retiré (obsolète).

### Editor utility (Phase C)
- `Assets/_Project/Editor/Saga.Editor.asmdef` (refs Saga.Runtime, Editor-only)
- `Assets/_Project/Editor/UpgradeAssetsCreator.cs` : menu `Saga > Sprint 2 > Generate Upgrade Assets`. Crée/update les 3 SOs idempotent :
  - Frappe : ForcePerTap, base 10, ×1.15, +1/level
  - Disciple : ForcePerSecond, base 50, ×1.20, +1/level
  - Méditation : ComboMultiplierBonus, base 200, ×1.50, +0.05/level

### UI (Phase D)
- `UI/UpgradeCardView.cs` : self-builds hierarchy (background, name, level, cost, effect, button). Listens `OnForceChanged` + `OnUpgradePurchased`. Near-miss glow ambre quand force ∈ [80%, 100%) du cost. Punch scale DOTween core sur purchase.
- `UI/ComboMeterView.cs` : refactor pour afficher la final multiplier (base × bonus). Listens aussi `OnUpgradePurchased` pour redraw quand Méditation up.
- `Core/MainSceneBootstrap.cs` : ajout `BuildUpgradePanel()` qui crée 3 cards horizontalement en bas (anchored bottom-stretch, 96px du bas, 220px de haut). Cards distribuées proportionnellement via anchors fractionnaires.

### Tests EditMode (Phase E)
- `Tests/EditMode/UpgradeDataTests.cs` : 5 cases (cost level 0, geometric scaling, Disciple profile, Méditation steep curve, properties round-trip)
- `Tests/EditMode/StatsCalculatorTests.cs` : 8 cases (no upgrades, Frappe linear, type filtering, Disciple sum, no-disciple zero, Méditation bonus, combined)
- `Tests/EditMode/UpgradeServiceTests.cs` : 10 cases (initial level, unknown id, base cost, affordability boundaries, purchase deduct, cost increase, sequential purchase, unknown upgrade fail)

**Total tests EditMode** : 24 (Sprint 1) + 23 (Sprint 2) = **47 cases**

## Sprint 2 — décisions design loguées (DESIGN_DECISIONS_LOG.md)

1. **Méditation = multiplicateur global** : `final = baseTier × (1 + level × 0.05)` (vs additif au cap, multiplicatif final-only, etc.). Toujours senti, compose proprement avec Frappe.
2. **ContentDatabase test seam** : injection optionnelle de upgrades via constructor pour DI tests. Production = Resources auto, tests = inline array.
3. **SO assets via Editor utility** : menu `Saga > Sprint 2 > Generate Upgrade Assets` plutôt que YAML manuel (filesystem-mode blocking au refresh des GUIDs scripts). Pattern reproductible Sprint 3+.

## Sprint 2 — Ajwad-pending (1 clic UI Unity)

1. **Refocus Unity** → compile + génération .meta de tous les nouveaux scripts/asmdefs
2. **Menu `Saga > Sprint 2 > Generate Upgrade Assets`** → crée les 3 SOs dans `Assets/_Project/Resources/Upgrades/`
3. **Test Runner** > EditMode > Run All → 47 tests devraient passer
4. **Play mode** Main scene → tap 30 min, acheter une dizaine d'upgrades, vérifier auto-tap Disciple (Force monte hors-tap), vérifier Méditation amplifie le combo
5. (Optional) Fix les 72 warnings IsPointerOverGameObject d'InputSystem — pas chiant si on a le temps, sinon Sprint 3

## Critère de succès Sprint 2 (cf 08_ROADMAP)

> "tu peux farmer 30 minutes et acheter une dizaine d'upgrades, sentir une progression, l'auto-tap de disciples fonctionne"

✅ Code prêt. Validation Editor + play test par Ajwad.

## Métriques de projet

- **Sprint actuel** : 2/11 (code 100%, validation Editor pending)
- **Lignes de code C# runtime (hors lib tierce)** : ~1900 (Sprint 1: 1100 + Sprint 2: 800)
- **Tests EditMode** : 47 cases
- **ScriptableObjects** : 3 prévus (Frappe, Disciple, Méditation), à générer via menu
- **Voies implémentées** : 0/8 (Sprint 4+)

## Questions ouvertes pour le coordinateur

Aucune. Q principal (formule Méditation multiplicative vs additive) tranchée par défaut raisonnable + log. Si Ajwad/coord rejette, refactor `StatsCalculator.GetComboMultiplierBonus` = 5 lignes.

## Prochaine session (Sprint 3)

**Objectif** : Stade visuel et milestone — le perso change visuellement au passage de palier (Mendiant → Apprenti → Guerrier).

À faire (cf 08_ROADMAP.md Sprint 3) :
- `Gameplay/StadeManager.cs` qui surveille les seuils
- Rig perso Stade 1 (silhouette mendiant) + Stade 2 (apprenti avec katana)
- Animations idle + tap-react (Unity 2D Animation ou Spine — DÉCISION À PRENDRE)
- Background dojo Stade 1 + Stade 2
- Mini-cinématique 1-2s au passage de stade (ink wash transition, swap sprites, drum hit)
- Sound design : ambient loop différent par stade
- NumberJuice complet (taille de chiffre selon magnitude, couleur selon palier)

**Pré-requis avant Sprint 3** : 
- Ajwad valide Sprint 2 (compile + menu generate + tests + play)
- **Décision Spine 2D vs Unity 2D Animation à trancher** (cf 06_TECH_STACK.md + DESIGN_DECISIONS_LOG)

**Polish secondaire en début Sprint 3** :
- Import Inter + JetBrains Mono via Font Asset Creator (deferred depuis Sprint 1)
- Fix les 72 warnings IsPointerOverGameObject (deferred depuis Sprint 1)
- Refactor MainSceneBootstrap vers scene-authored (deferred depuis Sprint 1)

## Notes libres

- Bridge MCP Coplay reste flaky : `mcpforunity://instances` voit Saga@97f3fed9b4b5fd40, mais tools (`read_console`, `manage_asset`, etc.) répondent "No Unity Editor instances found". Routing tool layer cassé. Filesystem-only continue de fonctionner.
- L'Editor utility `UpgradeAssetsCreator` est un pattern qu'on pourra reproduire pour générer les voies/esprits/régions des sprints futurs sans dépendre de MCP. Probably crée une suite "Saga > Sprint X > Generate Y" au fil du temps.
- Sprint 2 a 23 nouveaux tests EditMode. Coverage est plutôt bon : data layer (UpgradeData), service layer (UpgradeService), aggregator (StatsCalculator). Le UI (UpgradeCardView) n'a pas de tests — c'est OK, c'est du wiring + visual, pas de logique métier critique. Si Sprint 3+ amène plus de UI logique on ajoutera des PlayMode tests.
- DisciplesProcessor.Tick raise OnForceChanged à chaque tick (10Hz) quand Force/sec > 0. Donc 10 events/sec quand 1+ Disciple acheté. ForceCounterView gère bien (smoothing exp). Si ça devient trop bruyant en Sprint 9 (multi-source events) on debouncera.
- L'UpgradeService.TryPurchase appelle `GameManager.Instance?.Save?.MarkDirty()` directement — couplage tight au singleton. OK pour Sprint 2 minimal mais à refactor en injection Sprint 4+ quand on aura un proper IServiceContainer.
