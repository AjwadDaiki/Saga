# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 5 — Boss Mineurs + Élan + Vague AOE (code 100%, validation Editor pending).

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `feat/sprint-5-boss-mineurs-elan` (depuis dev).

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur, combo, juice, save | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation | ✅ closed |
| 3 | `v0.3.0` | Pixel adventurer + 3 attack variations + stade transition + NumberJuice | ✅ closed |
| 4 | `v0.4.0` | Combat Active System : Adversaire + chrono + mort temporaire | ✅ closed |
| 5 | (pending) | Boss Mineurs (Capitaines, 8) + Élan + Vague AOE | 🟡 code 100%, validation pending |

## Sprint 5 — ce qui est fait ✅

### Log mineure Sprint 4 fix (Phase A)
- `TapFxSpawner` : phase-aware, en combat (AdversaireActive / CapitaineActive) spawn "-X" rouge depuis position de l'adversaire (converted via Camera.main.WorldToScreenPoint), jitter horizontal + offset vertical pour éviter la stack. En Training, "+X" ambre à la position du tap comme avant. Dust reste au tap point dans les 2 cas.

### Data layer (Phase B)
- `Data/CombatPhase.cs` enum étendu : ajout `CapitaineIncoming`, `CapitaineActive`, `CapitaineVictory` (3 nouvelles phases, total 8)
- `Data/CapitaineData.cs` SO + `CreateForTests` factory + `ComputePhase(hpRatio)` helper
- `Data/ElanConstants.cs` static (ElanPerTap=5, ComboBonus=2×, Decay=4/s, IdleGrace=3s, Max=100, VagueTrainingMul=5×, VagueDuration=5s, VagueCombatBase=3000)
- `Data/GameState.cs` ajout : `currentElan`, `lastVagueTime`, `totalCapitainesDefeated`, `currentCapitaineId`, `currentCapitainePhase`. saveVersion 4 → 6 (deux bumps : v4→v5 Élan, v5→v6 Capitaines)
- `Save/SaveService.Migrate` étendu — 2 migrations chained, defensive boot reset au Training

### Events (Phase C) — 9 nouveaux channels dans `GameEvents.cs`
- `OnElanChanged(float, float)`, `OnElanFull()`
- `OnVagueTriggered(CombatPhase)`, `OnVagueResolved(BigDouble)`, `OnVagueBuffActive(bool)`
- `OnCapitaineSpawned(CapitaineData)`, `OnCapitainePhaseChanged(int, int)`, `OnCapitaineEnraged()`, `OnCapitaineDefeated(CapitaineData, BigDouble)`

### Services (Phase C + D)
- `Gameplay/ElanService.cs` (POCO) — gain par tap avec synergie combo, decay après grace, RegisterTap testable directement, ResetForVague helper
- `Gameplay/VagueResolver.cs` (POCO) — TriggerVague phase-aware. Training: buff +500% Force × 5s. Combat: 3000 × stade damage. Tick gère le buff timer.
- `Gameplay/CapitaineSpawner.cs` (POCO) — listens OnAdversaireDefeated, queue un capitaine tous les 10, exposé via `HasPending` + `ConsumePending`. Découplé de CombatProcessor via "queue-and-consume" pattern.
- `Gameplay/CombatProcessor.cs` étendu — 3 nouvelles phases dans le state machine (CapitaineIncoming/Active/Victory), HP phase tracking via `CapitaineData.ComputePhase`, enrage event au dernier seuil, ConsumePending de CapitaineSpawner après AdversaireVictory
- `Gameplay/DamageDealer.cs` étendu — handle CapitaineActive (lookup maxHp via Content.GetCapitaine)
- `Core/ContentDatabase.cs` étendu — 3e dictionnaire pour capitaines (`Resources/Capitaines/`), DI seam 3-arg constructor pour tests
- `Core/GameManager.cs` — instancie 3 nouveaux services (Elan, Vague, Capitaines), attach CapitaineSpawner à Combat
- `Core/GameTicker.cs` — hooks `Elan.Tick` + `Vague.Tick` après `Combat.Tick`
- `Gameplay/TapHandler.cs` — accepte CapitaineActive (phase gating), applique le Vague Training buff (×5 Force quand `IsTrainingBuffActive`)

### Editor utility (Phase E)
- `Editor/CapitaineAssetsCreator.cs` + menu `Saga > Sprint 5 > Generate Capitaine Assets`
- Crée 8 SOs idempotent dans `Resources/Capitaines/` :
  - Hattori du Mont (Samurai, 500 HP, 250 reward, 60s)
  - Bjorn aux Tresses (Viking, 600, 300, 70s)
  - Lin du Bambou (Wuxia, 450, 220, 65s)
  - Pythagoras Lame Brève (Spartiate, 700, 320, 75s)
  - Berke le Cavalier (Mongol, 550, 280, 60s)
  - Nour ad-Din (Saladin, 580, 300, 70s)
  - Cuauhtémoc le Jeune (Aztec, 650, 330, 65s)
  - Brennus du Cor (Gaulois, 750, 350, 80s)
- Chacun : intro/death citations + thresholds 75/50/25 + 4 phase colors (vert/jaune/orange/rouge enrage)

### UI (Phase F) — 6 nouvelles views + CombatHud update
- `UI/ElanBarView.cs` — barre horizontale ambre, label "ÉLAN N%", pulse ≥90%
- `UI/VagueButtonView.cs` — bouton caché par défaut, scale punch on OnElanFull, click → `TriggerVague` + `ResetForVague`, hide on OnElanChanged (current<max), glow alpha pulse
- `Gameplay/VagueVisualController.cs` — flash écran blanc (0.4s 3-stage) + camera shake (0.3s, strength 0.25, vibrato 18) on OnVagueTriggered
- `UI/CapitaineIntroView.cs` — full-screen vignette + name + voie subtitle + intro citation, 2.5s total (fade-in/hold/fade-out)
- `UI/CapitaineDeathView.cs` — flash blanc 0.5s + death citation + "VICTOIRE" doré 2.2s
- `Gameplay/CapitaineWorldView.cs` — sprite 90×150 + aura radial 128px tinté par phase color, idle scale + aura pulse, shake on damage, enrage = permanent gentle shake, fade-out on defeated
- `UI/CombatHudView.cs` étendu — visible pendant Capitaine phases aussi, HP bar color par phase (vert→jaune→orange→rouge enrage), name + chrono

### Scene wiring (Phase F suite)
- `MainSceneBootstrap.cs` étendu :
  - `BuildCapitaine` world (aura child + body child, both SpriteRenderer, anchored à MannequinPosition)
  - `BuildElanBarAndVagueButton` — row container avec barre 760px + bouton 220px à droite, anchored bottom centered au-dessus du upgrade panel
  - `BuildVagueFlashOverlay` — full-screen Image pour le flash blanc
  - `BuildCapitaineIntroOverlay` — vignette 0.6 alpha + name + subtitle + citation
  - `BuildCapitaineDeathOverlay` — flash + citation + "VICTOIRE" doré
  - `TapFxSpawner.EnemyAnchor` wired à AdversaireTransform

### Tests EditMode (Phase G) — 18 nouveaux cases (total 91)
- `ElanServiceTests` (6) — base/combo bonus, decay, cap, OnElanFull once per cycle, ResetForVague allows refull
- `CapitaineDataTests` (3) — factory round-trip, default thresholds 75/50/25, ComputePhase
- `CapitaineSpawnerTests` (5) — threshold constant, initial state, ConsumePending contract, content database exposes pool, empty pool safe
- `VagueResolverTests` (4) — Training buff activate, buff expires after duration, combat damage to adversaire, damage scales with stade

## Sprint 5 — décisions design loguées (DESIGN_DECISIONS_LOG.md)

1. **Élan synergy ×2 quand combo > tier 0** (encourage maintenir combo)
2. **CapitaineSpawner queue + CombatProcessor consume pattern** (découplage clean, pattern reproductible Maîtres Sprint 6)
3. **Capitaine HP partagé avec adversaire (currentAdversaireHp)** — schema simplification, à rebaptiser `currentEnemyHp` Sprint 7+

## Sprint 5 — Ajwad-pending (3 clics)

1. **Refocus Unity** → compile (~12 nouveaux fichiers + 8 modifs)
2. **Menu `Saga > Sprint 5 > Generate Capitaine Assets`** → crée les 8 SOs dans `Resources/Capitaines/`
3. **Test Runner > EditMode > Run All** → 91 tests devraient passer
4. **Play mode** :
   - Tap → barre Élan se remplit en bas
   - Combo monte → Élan accélère (×2)
   - Élan à 100% → bouton VAGUE apparait (scale punch)
   - Click VAGUE → flash blanc + camera shake + buff +500% Force pendant 5 sec
   - Tu continues tap, adversaires arrivent normalement
   - Au 10e adversaire battu : Capitaine arrive avec cinématique (vignette + nom + citation)
   - Combat capitaine : HP bar stylée, couleur change par phase (vert→jaune→orange→rouge à 25% enrage)
   - Victoire capitaine : flash + death citation + "VICTOIRE" doré + retour Training

## Critère de succès Sprint 5

> "Tu joues, tu tapes → barre Élan se remplit / combo monte → Élan accélère / Élan 100% → bouton VAGUE / click → flash + buff / continue normalement / 10e adversaire → Capitaine cinematic / combat phasé / victoire stylée"

✅ Code prêt. Validation par Ajwad après les 3 clics.

## Métriques de projet

- **Sprints closed** : 4/11 (Sprint 5 en validation)
- **Branches** : main + dev à v0.4.0, feat/sprint-5-boss-mineurs-elan en cours
- **Tags** : v0.1.0..v0.4.0
- **Lignes de code C# runtime (hors lib tierce)** : ~4000 (Sprint 5: +1000)
- **Tests EditMode** : 91 cases
- **ScriptableObjects** : 17 (3 upgrades + 1 SpriteAnimationLibrary + 5 adversaires + 8 capitaines)
- **Combat phases** : 8/8 Sprint 5 (Training, AdversaireIncoming/Active/Victory, PlayerDeathTemporary, CapitaineIncoming/Active/Victory). Maîtres Sprint 6.
- **Voies tintées** : 9 (None + 8 cultures, dans AdversaireWorldView + CapitaineWorldView phase colors)

## Polish items deferred (encore optionnels)

1. Refactor `MainSceneBootstrap` → scene-authored UI (8 sprints d'affilée, on devrait s'y mettre Sprint 7-8)
2. Fix `IsPointerOverGameObject` warnings InputSystem
3. Import fonts Inter + JetBrains Mono via Font Asset Creator
4. Slice du `10_weaponhit_spritesheet.png` pour slash FX frame-by-frame
5. Sprite dédié Capitaines (Sprint 11 polish au-delà du procédural)

## Questions ouvertes pour le coordinateur

Aucune côté Sprint 5. Toutes les décisions tranchées.

**Anticipation Sprint 6** (Boss Majeurs + Prestige + Souffle) :
- Sprite Maîtres : dédié dès Sprint 6 ou même rvros tinté épais aura ?
- Mécanique Souffle : reset cooldown au prestige ou cooldown permanent ?
- Citation finale du joueur : prompt obligatoire au début du run ou à la mort ?

## Prochaine session (Sprint 6)

Maîtres légendaires (8) + Prestige + Souffle. **Gros sprint narrativement**. Cf GAME_DESIGN_v2 §Sprint 6.

**Pré-requis Sprint 6** : tu valides Sprint 5 (3 clics).

## Notes session

- 5 sprints clos consécutifs ou en validation, baseline filesystem-only solide.
- Sprint 5 = +1000 lignes. La plus grosse session côté logique pure (state machine extension + 3 nouveaux services POCO + 6 views UI). Architecture pattern POCO + tick + events tient solidement.
- `CombatProcessor` state machine maintenant 8 phases, géré par un switch propre. Pattern Tick + TransitionTo + queue-consume reste lisible.
- 91 tests EditMode, tous green-ready. Coverage solide sur les nouveaux services (ElanService 6, VagueResolver 4, CapitaineSpawner 5).
- Le "-X" damage en combat est élégant : phase check + WorldToScreenPoint + jitter. Polish léger à faire si les chiffres se chevauchent en spam tap, mais MVP fonctionnel.
