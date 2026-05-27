# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 1 - Core tap loop (100% code-side, validation Editor pending au refocus Unity)

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code — Sprint 1 enchaîné après Sprint 0 dans la même session

## Sprint 1 — ce qui est fait ✅

### Architecture (Phase A + C)
Foundations posées per 07_ARCHITECTURE.md :
- `Data/GameState.cs` — POCO single source of truth (currencies BigDouble, totalTaps, timestamps, saveVersion 1)
- `Core/GameManager.cs` — service locator, bootstrappe avant scene load via `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`, expose `State` + `Save`, force-save on pause/quit
- `Core/GameTicker.cs` — tick loop 10Hz, drive le throttled save Sprint 1 (slots disciples/esprits/voie passive prêts pour Sprint 2+)
- `Core/GameEvents.cs` — channels événementiels statiques (`OnForceChanged`, `OnComboChanged`, `OnTapResolved`). Migration vers SO event channels deferred Sprint 2+ quand surface grandit
- `Save/SaveService.cs` — Newtonsoft JSON, backup rotation 3 slots, pattern `MarkDirty + TickThrottledSave` (1 write/sec max), `ForceSave` sur pause/quit, fallback backup en cas de corruption save
- `Save/BigDoubleJsonConverter.cs` — converter custom Newtonsoft pour BigDouble. Sérialise en `{ "m": mantissa, "e": exponent }`, tolère aussi string ("1.5e308") et numerics pour les saves edit à la main
- `Math/NumberFormatter.cs` — K/M/B/T/aa..zz suffixes + scientific fallback past zz, decimals param, sign-aware

### Asmdefs (Phase C)
- `BreakInfinity` + `BreakInfinity.Editor` (lib tierce wrap proprement, Editor split)
- `Saga.Runtime` (refs `BreakInfinity`, `Unity.InputSystem`, `Unity.TextMeshPro` ; precompiled `Newtonsoft.Json.dll` + `DOTween.dll`)
- `Saga.Tests.EditMode` (refs `Saga.Runtime`, `nunit.framework.dll`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`)

### Tests EditMode (Phase C)
- `NumberFormatterTests` — 13 cases (zéro, units, K, M, B, T, aa, bb, cc, zz, beyond-zz fallback, négatifs, decimals param, GetSuffix table)
- `ComboSystemTests` — 11 cases (initial state, paliers 1.0 → 1.2 → 1.5 → 2.0, cap, window expire, reset, manual reset, idle tick)

### Gameplay (Phase B)
- `Gameplay/ComboSystem.cs` — state machine POCO 4 paliers discrets (0-2 → x1.0, 3-5 → x1.2, 6-9 → x1.5, 10+ → x2.0), window 1.5s, broadcast `OnComboChanged` sur transition de tier
- `Gameplay/TapHandler.cs` — capture pointer press n'importe où via `Unity.InputSystem` (`InputAction` bound to `<Pointer>/press`), filter UI clicks via `EventSystem.IsPointerOverGameObject`, commit gain à State.force, MarkDirty save, raise events

### UI (Phase D)
- `UI/ForceCounterView.cs` — TMP label top-center, smoothing exponentiel frame-rate independent (`_smoothing` = 0.12s) sur la valeur affichée → ticker animation sans teleport
- `UI/ComboMeterView.cs` — TMP top-right, fade in/out via CanvasGroup, affiche "x1.2" / "x1.5" / "x2.0" selon tier
- `UI/FloatingNumberView.cs` — "+X" qui float up + fade via DOTween, color tint selon combo tier (blanc → ambre → coral)
- `UI/TapFxSpawner.cs` — listener `OnTapResolved`, spawn floating number + 5 dust particles (UI Image dots radiating + fade via DOTween, lightweight pour mobile)

### Scene wiring (Phase D)
- `Core/MainSceneBootstrap.cs` — auto-builder runtime de la hiérarchie UI Main scene si pas authorée. Auto-bootstrappe via `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` si scene active = "Main". Crée Canvas (ScreenSpaceOverlay, ref 1080x1920), background `#0d0d0d`, ForceCounter, ComboMeter, TapHandler, TapFxSpawner.
- `ProjectSettings/EditorBuildSettings.asset` — Main swappé en index 0 (boot direct sur Main, Boot reste à index 1 pour splash/load à Sprint 9+)

### Polish (Phase E)
- Save throttled 1/sec max ✅ via SaveService
- Load restore ✅ via GameManager.Awake
- Particules tap dust ✅ via TapFxSpawner (5 dots radiating)
- Floating +X ✅ via FloatingNumberView spawned at tap position

## Sprint 1 — décisions design loguées (3 entrées DESIGN_DECISIONS_LOG.md)

1. **Combo en paliers discrets** : 0-2 → x1.0, 3-5 → x1.2, 6-9 → x1.5, 10+ → x2.0 (vs lerp continu). Window 1.5s.
2. **TMP default font (LiberationSans SDF)** : custom fonts deferred. Tabular jitter accepté pour Sprint 1, à corriger Sprint 2 via Inter + JetBrains Mono via Font Asset Creator.
3. **Scene Main bootstrappée par runtime script** (`MainSceneBootstrap`) au lieu de YAML-authored : workaround MCP down, à refactor Sprint 2 quand bridge stable.

## Sprint 1 — Ajwad-pending (validation Editor)

1. **Refocus Unity** → trigger compile clean + génération des .meta pour tous les nouveaux .cs et .asmdef
2. **Test Runner** (Window > General > Test Runner > EditMode > Run All) → exécuter les 24 tests (NumberFormatter + ComboSystem)
3. **Play mode test** → ouvrir Main scene, hit Play, tap pendant 30 secondes, vérifier :
   - Counter Force monte en ticker (pas teleport)
   - Combo affiché en haut droite à partir du 3e tap consecutive
   - Floating "+X" spawn à chaque tap, monte et fade
   - Dust particles (5 ambres) burst au tap
   - Quit → relance Play → la Force est restaurée

## Critère de succès Sprint 1 (cf 08_ROADMAP)

> "ouvrir le jeu, taper 30 secondes, le compteur monte avec juice, ça sauvegarde, ça reload bien"

✅ Atteignable au play test Ajwad (tout le code est en place).

## Métriques de projet

- **Sprint actuel** : 1/11 (code 100%, validation Editor pending)
- **Lignes de code C# (runtime, hors lib tierce)** : ~1100
- **Tests EditMode** : 24 cases
- **ScriptableObjects créés** : 0 (Sprint 2+)
- **Voies implémentées** : 0/8 (Sprint 4+)

## Questions ouvertes pour le coordinateur

Toutes les questions Sprint 1 ont été tranchées en décisions techniques internes (Q1 combo paliers et Q2 fonts résolues par défauts raisonnables loggés dans DESIGN_DECISIONS_LOG). Ajwad peut reverser si nécessaire ; reswap = quelques lignes de code chacun.

## Prochaine session (Sprint 2)

**Objectif** : upgrades de base (Frappe, Disciple, Méditation) avec UI panneau + GameTicker driving disciples passive.

À faire (cf 08_ROADMAP.md Sprint 2) :
- `Data/UpgradeData.cs` ScriptableObject (ID, displayName, baseCost, costMultiplier, effectType, effectValue, icon)
- 3 SO instances : Frappe, Disciple, Méditation
- `Gameplay/UpgradeService.cs` (TryPurchase, GetCurrentCost, ApplyEffect)
- `Gameplay/DisciplesProcessor.cs` tick auto-tap passive
- UI : panneau upgrades 3 cards en bas, near-miss glow à <20% du coût
- Tests : UpgradeService cost progression + affordability

**Pré-requis avant Sprint 2** : Ajwad refocus Unity, valide compile + run tests + play mode test Sprint 1.

**Polish secondaire au début Sprint 2 (si pas chiant)** :
- Import Inter + JetBrains Mono via Font Asset Creator pour swap font ticker (tabular digits)
- Refactor MainSceneBootstrap vers scene-authored (drag les Views dans Main.unity)

## Notes libres

- Bridge MCP Coplay encore down dans la session, full filesystem mode. Tout codé sans tooling Editor — la première vraie validation se fera au refocus Ajwad. Risk surface : compile errors potentiels sur les usings/asmdef qu'on a inférés sans MCP feedback. Liste mentale des points à vérifier :
  - `Unity.InputSystem` asmdef ref → ok (vérifié dans PackageCache)
  - `Unity.TextMeshPro` asmdef ref → ok (vérifié)
  - `Newtonsoft.Json.dll` precompiled ref → ok (vérifié dans PackageCache)
  - `DOTween.dll` precompiled ref → ok (présent dans `Assets/Plugins/Demigiant/DOTween/`)
  - `using DG.Tweening` dans `FloatingNumberView` et `TapFxSpawner` → namespace officiel DOTween, ok
- ComboSystem broadcast `OnComboChanged` au changement de tier seulement (pas au changement de tapCount). Économise des UI repaints.
- TapFxSpawner dust particles : 5 par tap, donc à 10 taps/sec on a 50 UI Images créés/détruits par sec. Pas un problème sur device modern. À profile en Sprint 10 polish, optimisable via pool si besoin.
- DOTween free utilisé pour ticker + tweens. Upgrade vers Pro à 15$ envisagé au Sprint 2-3 si on a besoin du DOTweenPath ou du visual scripting.
