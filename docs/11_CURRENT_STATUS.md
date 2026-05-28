# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 6 implémenté côté code, **en attente validation in-play par Ajwad** avant squash merge `feat/sprint-6-maitres-prestige-souffle` → `dev` → `main` + tag `v0.6.0`.

**Dernière session**: 2026-05-28, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `feat/sprint-6-maitres-prestige-souffle` (pushable, code-complete, tests à exécuter).

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur, combo, juice, save | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation | ✅ closed |
| 3 | `v0.3.0` | Pixel adventurer + 3 attack variations + stade transition + NumberJuice | ✅ closed |
| 4 | `v0.4.0` | Combat Active System : Adversaire + chrono + mort temporaire + 5 adversaires | ✅ closed |
| 5 | `v0.5.0` | Boss Mineurs (8 Capitaines) + Élan + Vague AOE | ✅ closed |
| 6 | `v0.6.0` (pending) | Maîtres légendaires + Prestige + Souffle | 🟡 implem done, attend validation Ajwad |

## Sprint 6 — accomplissements clés (à valider en play)

### Système Maîtres légendaires
- **8 Maîtres SO** (un par voie, mythologies réelles zéro copyright) : Yoshitsune (Samurai), Ragnar Lodbrok (Viking), Sun (Shaolin), Léonidas (Spartiate), Subutaï (Mongol), Salah ad-Din (Sarrasin), Ahuitzotl (Aztèque), Vercingétorix (Gaulois)
- **HP énormes** 7500-11000, chrono 180s, intro/victory/defeat citations + Relique unique
- **`MaitreSpawner`** cadence /3 Capitaines → +1 slot d'invocation. Invocation volontaire (pas auto-spawn)
- **3 nouvelles phases combat** : `MaitreIncoming`, `MaitreActive`, `MaitreVictory`
- **Cinématique intro Maître** : arena tint + vignette + nom MASSIF + citation 2.5s
- **Chrono ×2 en enrage** (phase 3, HP < 25%) — punition narrative
- **`MaitreWorldView`** : sprite procédural 120×180 + aura radiale épaisse (192px, scale 1.5×) + tint par phase
- **Mort vs Maître** → trigger Prestige automatique

### Système Prestige
- **`PrestigeService`** : formule `echos = max(10, floor(log10(forceMax) × 10))`
- **Reset matrix** PERSIST/RESET stricte (voir DESIGN_DECISIONS_LOG.md)
  - PERSIST : `totalEchos`, `relicsOwned`, `titlesUnlocked`, `achievementsUnlocked`, `prestigeCount`, `deathRecords`, `playerCitation`, `lastSouffleTime`
  - RESET : `force`, `upgradeLevels`, `currentStade`, counters runs, slots, états combat
- **Cinématique 4-phase** (~17s) : "TU ES MORT" → Voyage Intérieur → Citation finale → Renaissance
- **Hall des Légendes** : `DeathRecord` ajouté par mort (voie/nom/citation/forceMax/UTC)
- **Titres "Vaincu par X"** unlock auto + dédup

### Souffle (méditation)
- **`SouffleService`** state machine 3 états : Idle → Meditating (5s, taps bloqués) → Buffing (30s, ×1.5 Force) → Idle
- **Cooldown 120s** démarre au début de méditation (PAS fin de buff)
- **Cooldown PERSISTE au prestige** (compétence apprise)
- Bouton top-left avec compte à rebours

### UI
- **`SouffleButtonView`** : bouton top-left, états visuels Idle/Meditating/Buffing/Cooldown
- **`AffronterMaitreButtonView`** : visible quand slot ≥ 1, scale punch animation sur unlock
- **`AffronterMaitreModal`** : liste les 8 Maîtres avec portrait procédural + citation + bouton "Affronter"
- **`CitationInputModal`** : TMP_InputField max 80 chars, Confirm enabled when non-empty, Skip optionnel
- **`PrestigeCinematicView`** : coroutine 4 phases avec citation prompt-or-edit pattern
- **`MaitreIntroView`** : cinematic 2.5s arena tint + nom MASSIF + subtitle + citation
- **`CombatHudView`** : étendu pour écouter `OnMaitreSpawned` + `OnMaitrePhaseChanged`

### Save migration
- **v6 → v7** : Souffle + Maître fields (lastSouffleTime, souffleBuffActiveUntil, currentMaitreId, currentMaitrePhase, maitreInvocationSlots)
- **v7 → v8** : Prestige fields (totalEchos, currentRunEchosEarned, currentRunForceMax, playerCitation, relicsOwned/Conserved, titlesUnlocked, achievementsUnlocked, prestigeCount, lastPrestigeAt, deathRecords, playerCitationLockedForRun)
- Chaîne idempotente + defensive boot resets

### Tests
- **23 nouveaux tests EditMode** : MaitreData (4), MaitreSpawner (6), PrestigeService (8), SouffleService (7)
- **Total EditMode** : 91 + 23 = **114 tests** (à confirmer en Test Runner)

## Métriques de projet

- **Sprints closed** : 5/11 (Sprint 6 en attente validation)
- **Branches** : `feat/sprint-6-maitres-prestige-souffle` active sur `dev`
- **Tags actuels** : `v0.1.0` → `v0.5.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~5500 (≈ +1500 Sprint 6)
- **Tests EditMode** : 114 cases (NumberFormatter 13, ComboSystem 11, UpgradeData 5, StatsCalculator 8, UpgradeService 10, StadeManager 10, AdversaireData 3, AdversaireSpawner 5, CombatProcessor 8, ElanService 6, CapitaineData 3, CapitaineSpawner 5, VagueResolver 4, MaitreData 4, MaitreSpawner 6, PrestigeService 8, SouffleService 7)
- **ScriptableObjects** : 25 (3 upgrades + 1 anim library + 5 adversaires + 8 capitaines + 8 maîtres)
- **Combat phases implémentées** : 11/11 (Training + Adv×3 + PlayerDeath + Cap×3 + Maître×3)
- **Voies tintées** : 9 (None + 8 cultures)
- **GameEvents channels** : ~30 (split Sprint 7+ candidate)

## ✅ Sprint 6 — checklist Ajwad pour validation

1. **Refocus Unity** (recompile automatique) — vérifier console clean
2. **Generate Maître SOs** : `Saga > Sprint 6 > Generate Maitre Assets` (crée les 8 SOs dans `Resources/Maitres/`)
3. **Test Runner > EditMode > Run All** : 114 tests verts attendus
4. **Play smoke test** :
   - Boot → tap normal, Force monte, Élan se remplit (régression Sprint 5 OK)
   - Bouton Souffle top-left clickable → 5s méditation (taps bloqués) → 30s buff ×1.5
   - Battre 3 Capitaines → bouton "Affronter le Maître" apparait (scale punch)
   - Click → modal liste 8 Maîtres → "Affronter" sur un (ex: Yoshitsune)
   - Cinematic intro 2.5s : arena tint + nom MASSIF + citation
   - Combat actif : phases HP tint progressif vert→jaune→orange→rouge
   - Enrage phase 3 (HP < 25%) : shake permanent + chrono accélère
   - **Scénario A** : battre le Maître → "VICTOIRE" + relique ajoutée à `relicsOwned`
   - **Scénario B** : laisser chrono tomber à 0 → cinématique Prestige 17s complète (TU ES MORT → Voyage Intérieur stats/Échos → Citation prompt (si vide) ou affichage → Renaissance fade white → reprise Training avec totalEchos ↑, prestigeCount = 1, deathRecords[0] présent)
5. **Reset state test** : après Prestige, vérifier `force = 0`, upgrades vides, stade = 1, mais titres/relics/totalEchos PERSIST

## Polish items deferred (toujours optionnels)

1. Refactor `MainSceneBootstrap` → scene-authored UI (8 sprints d'affilée, candidate Sprint 7-8)
2. Import fonts Inter + JetBrains Mono + Cinzel/Cormorant (lore) via Font Asset Creator
3. Slice `10_weaponhit_spritesheet.png` pour slash FX frame-by-frame
4. Sprite dédié Capitaines + Maîtres (Sprint 11 polish)
5. Drum hit audio à Phase 1 de la cinématique Prestige (`MaitreData.DrumHitPitch` champ déjà prêt)
6. Hall des Légendes UI (afficher `deathRecords` quelque part — Sprint 7+ candidate)
7. UI sélection Reliques à conserver au prestige (Sprint 7+ MVP toutes persistent)
8. Sprint 7+ split `GameEvents.cs` par domaine (CombatEvents, EconomyEvents, ProgressionEvents)

## Prochaine session

**Si Sprint 6 validé in-play** :
- Squash merge `feat/sprint-6-maitres-prestige-souffle` → `dev` → `main`
- Tag `v0.6.0`
- Cleanup branch
- Standby pour brief Sprint 7

**Si bugs détectés en play** :
- Fix sur la même branche (additional commits)
- Re-validation puis squash

## Notes session

- **6e sprint enchaîné**, workflow merge → tag → cleanup hyperbien rodé.
- 23 nouveaux tests = +25%. Coverage solide sur les 3 services Sprint 6 + MaitreData.
- `GameEvents.cs` maintenant ~30 events sur ~280 lignes. Split par domaine devient prioritaire Sprint 7.
- `MainSceneBootstrap.cs` dépasse les 700 lignes (re-mesurer). Refactor scene-authored UI = candidat fort Sprint 7-8.
- Pattern Editor utility maintenant à 5 utilities (Sprint 2/3/4/5/6).
- 2 nouvelles migrations save (v6→v7→v8). Chain à 8 migrations cumulées, toujours idempotente.
- L'ajout de 3 phases Maître + cinématique Prestige sans refactor du CombatProcessor squelette confirme la solidité du pattern état-machine event-driven.
- Bridge MCP Unity : déconnecté depuis Sprint 4. Filesystem-only continue à scaler.
- Sprint 6 est **LE** sprint le plus dense narrativement. Code-side est complet ; le vrai test sera la valeur émotionnelle de la cinématique Prestige en play.
