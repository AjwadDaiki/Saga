# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 4 — Combat Active System (code 100%, validation Editor pending).

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `feat/sprint-4-combat-active` (depuis dev).

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur ticker, combo 4 tiers, "+X" floating, dust, save/reload | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation. Cards bottom, near-miss glow. | ✅ closed |
| 3 | `v0.3.0` | Pixel adventurer + 3 attack variations weighted + stade transition + NumberJuice | ✅ closed |
| 4 | (pending) | Combat Active System : Adversaire, chrono, mort temporaire | 🟡 code 100%, validation pending |

## Sprint 4 — ce qui est fait ✅

### GAME_DESIGN_v2 absorbed
- `docs/GAME_DESIGN_v2.md` lu intégralement. Architecture combat à 3 phases (Training / Adversaire / Boss). Sprint 4 implémente Phase 2 (Adversaire). Bosses Sprint 5-6.

### Data layer (Phase A)
- `Data/CombatPhase.cs` enum (Training, AdversaireIncoming, AdversaireActive, AdversaireVictory, PlayerDeathTemporary)
- `Data/Voie.cs` enum (None, Samurai, Viking, Wuxia, Spartiate, Mongol, Saladin, Aztec, Gaulois) — 9 valeurs
- `Data/AdversaireData.cs` SO (id, displayName, voie, hp BigDouble, rewardForce BigDouble, lootChance, spriteIdleName, chronoSeconds). `CreateForTests` factory sous `#if UNITY_INCLUDE_TESTS`.
- `Data/GameState.cs` : ajout `currentPhase`, `currentAdversaireId`, `currentAdversaireHp`, `chronoRemaining`, `tapsTowardsNextAdversaire`, `totalAdversairesDefeated`. saveVersion 3 → 4.
- `Save/SaveService.cs.Migrate` v3 → v4 : init combat fields. Defensive: toujours reset à Training au boot pour éviter le mid-combat resurrection.

### Events (Phase B) — 7 nouveaux channels dans `GameEvents.cs`
- `OnPhaseChanged(CombatPhase prev, CombatPhase next)`
- `OnAdversaireSpawned(AdversaireData)`
- `OnAdversaireDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)`
- `OnAdversaireDefeated(AdversaireData, BigDouble reward)`
- `OnPlayerDiedTemporary()`
- `OnChronoUpdated(float remaining, float total)`
- `OnAdversaireProgressUpdated(int currentTaps, int threshold)`

### Services (Phase C)
- `Gameplay/AdversaireSpawner.cs` (POCO) : table thresholds par stade (50/100/200/400/800/1600), incrémente sur tap (Training only), pick random au seuil, délègue à CombatProcessor.StartIncoming
- `Gameplay/CombatProcessor.cs` (POCO) : state machine 5 phases avec `StartIncoming`, `Tick`, `ResolveDeathTemporary`. Penalty -10% Force à la mort temporaire. Reward auto-appliqué à la victoire.
- `Gameplay/DamageDealer.cs` (POCO) : subscriber `OnTapResolved` static-lifetime. Applique damage à `currentAdversaireHp` uniquement en AdversaireActive. Raise `OnAdversaireDamaged`.
- `Core/ContentDatabase.cs` étendu : 2e dictionnaire `adversairesById`, charge depuis `Resources/Adversaires`, DI seam pour tests (`new ContentDatabase(upgrades, adversaires)`).
- `Core/GameManager.cs` : instancie Combat + Adversaires + Damage en Awake. Hook `OnTapResolved` → `Adversaires.OnTap(state)`.
- `Core/GameTicker.cs` : appelle `Combat.Tick(state, dt)` après `Stades.Tick`.
- `Gameplay/TapHandler.cs` : phase-gating — Training ajoute Force, AdversaireActive laisse DamageDealer faire son boulot, autres phases ignorent le tap.

### Editor utility (Phase D)
- `Editor/AdversaireAssetsCreator.cs` + menu `Saga > Sprint 4 > Generate Adversaire Assets`
- Crée 5 SOs idempotent dans `Resources/Adversaires/` :
  - Ronin Errant (None, 100 HP, 30F reward, 15% loot, 30s)
  - Spadassin Nordique (Viking, 150, 50, 20%, 35s)
  - Initié Wuxia (Wuxia, 200, 75, 25%, 40s)
  - Hoplite Lâche (Spartiate, 250, 100, 30%, 45s)
  - Pèlerin du Nord (None, 80, 40, 20%, 25s)

### UI (Phase E) — 5 nouvelles views
- `UI/AdversaireProgressBarView.cs` : top-center, fill ambre + label "Prochain adversaire — N/M". Pulse subtil ≥ 90%. Hide en combat.
- `UI/CombatHudView.cs` : nom + HP bar rouge + chrono (rouge sous 5s). Fade in/out par phase.
- `UI/AdversaireSpawnView.cs` : full-screen centered banner — fade-in name 0.25s, hold 1s, fade-out 0.3s sur OnAdversaireSpawned.
- `UI/DeathOverlayView.cs` : full-screen Image noir alpha 0.95 + "TU ES MORT" + sous-titre quote + hint "Click pour reprendre". `IPointerClickHandler` capture le click → `CombatProcessor.ResolveDeathTemporary`.
- `Gameplay/AdversaireWorldView.cs` : sprite procédural 60×100 tinté par voie. Idle scale loop, shake on damage, fade-out on defeated, translucent on death temporary.

### Scene wiring (Phase E suite)
- `MainSceneBootstrap.cs` étendu :
  - `BuildAdversaire` (world space) à la position du mannequin, visibility phase-driven
  - `BuildAdversaireProgressBar` (top center, sous le compteur)
  - `BuildCombatHud` (top center, sous progress bar, hidden by default)
  - `BuildAdversaireSpawnView` (banner overlay)
  - `BuildDeathOverlay` (full-screen, last sibling pour render on top)
- `MannequinView.cs` phase-aware : fade-out à l'entrée de combat, fade-in retour Training. Shake-on-tap filtré Training uniquement.

### Tests EditMode (Phase F) — 16 nouveaux cases (total 73)
- `AdversaireDataTests` (3) : factory round-trip, enum distinct, MVP set construction
- `AdversaireSpawnerTests` (5) : thresholds par stade, below/at threshold, non-Training no-spawn, empty pool gracieux
- `CombatProcessorTests` (8) : StartIncoming, Incoming → Active après 1s, chrono decrement, HP 0 → Victory + reward, chrono expire → Death, Victory → Training après 2s, ResolveDeathTemporary -10% Force, Tick no-op en Death sans Resolve

## Sprint 4 — décisions design loguées (DESIGN_DECISIONS_LOG.md)

1. **CombatProcessor state machine POCO** ticked à 10Hz, pattern reproductible
2. **Taps en combat = damage, pas Force** — gating dans TapHandler
3. **Boot policy reset à Training** — évite mid-combat resurrection corrompue
4. **Sprites procéduraux par voie** — Sprint 5+ remplace avec art

## Sprint 4 — Ajwad-pending (3 clics)

1. **Refocus Unity** → compile (~15 nouveaux fichiers)
2. **Menu `Saga > Sprint 4 > Generate Adversaire Assets`** → crée les 5 SOs
3. **Test Runner > EditMode > Run All** → 73 tests devraient passer
4. **Play mode** Main scene → checklist :
   - Barre "Prochain adversaire" en haut, se remplit avec les taps
   - À 50 taps (stade 1) : mannequin fade-out, adversaire fade-in, banner nom centré
   - Chrono démarre (30s typique), HP bar adversaire visible
   - Tap = damage, HP descend, mannequin reste caché
   - Si tu tues à temps : reward Force ajoutée, mannequin réapparait après 2s
   - Si chrono expire : écran noir + "TU ES MORT" + click pour reprendre, Force -10%
   - Save/quit/reload → boot en Training, pas de combat zombie

## Critère de succès Sprint 4 (cf brief)

> "Tu joues, tu tapes le mannequin → Barre se remplit → adversaire arrive → chrono démarre → tu tapes ses HP → victoire (reward) ou chrono expire (mort temporaire) → retour mannequin"

✅ Code prêt. Validation par Ajwad après les 3 clics.

## Métriques de projet

- **Sprints closed** : 3/11 (Sprint 4 en validation)
- **Branches** : main + dev à `v0.3.0`, feat/sprint-4-combat-active en cours
- **Tags** : v0.1.0, v0.2.0, v0.3.0
- **Lignes de code C# runtime (hors lib tierce)** : ~3000 (Sprint 4: +600)
- **Tests EditMode** : 73 cases
- **ScriptableObjects** : 3 upgrades + 1 SpriteAnimationLibrary + 5 adversaires (9 total)
- **Voies implémentées** : 0/8 mécaniquement (Sprint 7+), tintées dans AdversaireWorldView dès Sprint 4

## Polish items deferred (toujours optionnels)

1. Refactor `MainSceneBootstrap` → scene-authored UI
2. Fix `IsPointerOverGameObject` warnings InputSystem Unity 6
3. Import fonts Inter + JetBrains Mono via Font Asset Creator
4. Slice du `10_weaponhit_spritesheet.png` pour slash FX frame-by-frame
5. "+X" floating vs "-X" floating selon phase (Sprint 4 amer mais lisible)

## Questions ouvertes pour le coordinateur

Aucune côté dev Sprint 4. Toutes les décisions tranchées par défauts raisonnables loguées dans DESIGN_DECISIONS_LOG.

**Pré-décisions Sprint 5** (Boss Mineurs / Capitaines + Élan) :
- 8 Capitaines (un par voie) : sprite per capitaine ou même rvros adventurer tinté par voie ?
- Élan jauge UI : remplacement du compteur de combo actuel ou en plus ?
- Mécanique Vague (cinematic AOE) : Sprint 5 visuel basique ou polish Sprint 11 ?

## Prochaine session (Sprint 5)

Cf GAME_DESIGN_v2 §Sprint 5 :
- Mécanique Élan (jauge + bouton Vague + animation)
- Boss Mineurs (Capitaines) tous les 10 adversaires
- 8 Capitaines avec variations HP/phases
- Cinématique d'arrivée Capitaine
- Drops runes/reliques (basique)

**Pré-requis avant Sprint 5** : tu valides Sprint 4 (3 clics ci-dessus).

## Notes session

- Bridge MCP tools toujours flaky (resource OK, tools "no instance"). 4 sprints d'affilée en filesystem-only.
- Pattern Editor utility (`Saga > Sprint N > Generate Y Assets`) maintenant à 3 utilities (Sprint 2, 3, 4). Workflow rodé pour générer du contenu SO sans dépendre de MCP.
- Le combat phase state machine est très clean — 5 phases, 6 transitions, tout couvert par 8 tests. Sprint 5 Capitaines ajoutera des phases mais le pattern Tick + TransitionTo se réutilise sans refactor.
- Architecture event-driven solide : `OnTapResolved` est la seule entrée user → AdversaireSpawner (compteur), DamageDealer (damage), CharacterView (anim), TapFxSpawner (FX), MannequinView (shake), CombatProcessor (indirect via Tick). 6 consumers, 0 coupling.
- 73 tests EditMode, tous green-ready (la suite tests EditMode est devenue notre safety net principal vu que les visuals doivent encore être validés en play).
- Combat lifecycle: ~30-45s par fight. Avec 5 adversaires variés et chrono variable, le rythme devrait être bon. À itérer post-play test.
