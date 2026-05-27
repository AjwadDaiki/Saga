# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 5 closed (validé in-play par Ajwad, mergé `dev` + `main`, tagué `v0.5.0`). **Sprint 6 en attente du brief enrichi** (Boss Majeurs / Maîtres légendaires + Prestige + Souffle — gros sprint narratif).

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `dev` (en standby), pas de feat branch active.

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur, combo, juice, save | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation | ✅ closed |
| 3 | `v0.3.0` | Pixel adventurer + 3 attack variations + stade transition + NumberJuice | ✅ closed |
| 4 | `v0.4.0` | Combat Active System : Adversaire + chrono + mort temporaire + 5 adversaires | ✅ closed |
| 5 | `v0.5.0` | Boss Mineurs (8 Capitaines) + Élan + Vague AOE | ✅ closed |

## Sprint 5 — accomplissements clés (validés en play)

- **Élan jauge** se remplit au tap, synergy combo ×2 confirmée (combo > tier 0)
- **Bouton VAGUE** apparait à 100% Élan avec scale punch animation
- **Vague Training** : flash écran + slash énorme + camera shake + buff +500% Force 5s — compteur Force explose visiblement
- **Adversaires** Sprint 4 toujours OK, damage "-X" rouge depuis position adversaire confirmé
- **8 Capitaines** (un par voie) : Hattori du Mont, Bjorn aux Tresses, Lin du Bambou, Pythagoras Lame Brève, Berke le Cavalier, Nour ad-Din, Cuauhtémoc le Jeune, Brennus du Cor
- **Cinématique intro Capitaine** : vignette + lettrage nom + intro citation, 2.5s total
- **HP phase tracking** : couleur HP bar change vert → jaune → orange → rouge enrage à 25%
- **Enrage** : tint rouge progressif + shake permanent léger
- **Victoire Capitaine** : slow-mo + flash + death citation + "VICTOIRE" doré, 3s total
- **Layout normalisé** : map verticale stable en portrait/landscape
- **91 tests EditMode** verts

## Métriques de projet

- **Sprints closed** : 5/11
- **Branches** : main + dev synced à `944edd6` (tag v0.5.0)
- **Tags** : `v0.1.0`, `v0.2.0`, `v0.3.0`, `v0.4.0`, `v0.5.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~4000
- **Tests EditMode** : 91 cases (NumberFormatter 13, ComboSystem 11, UpgradeData 5, StatsCalculator 8, UpgradeService 10, StadeManager 10, AdversaireData 3, AdversaireSpawner 5, CombatProcessor 8, ElanService 6, CapitaineData 3, CapitaineSpawner 5, VagueResolver 4)
- **ScriptableObjects** : 17 (3 upgrades + 1 anim library + 5 adversaires + 8 capitaines)
- **Combat phases implémentées** : 8/8 Sprint 5 — Maîtres ajoutent 3 phases Sprint 6 (MaitreIncoming, MaitreActive, MaitreVictory_Defeat=Prestige)
- **Voies tintées** : 9 (None + 8 cultures)

## ⏸️ Sprint 6 — en attente du brief enrichi

Coordinateur prépare le brief. **Gros sprint narrativement** (Maîtres légendaires + Prestige + Souffle).

### 3 décisions design déjà tranchées (DESIGN_DECISIONS_LOG.md)

1. **Maîtres visuels** = rvros tinted + aura épaisse (Sprint 6 placeholder). Sprite dédié Sprint 11 polish.
2. **Souffle cooldown** = **permanent**, ne reset pas au prestige. Mécaniques actives "apprises" persistent.
3. **Citation finale joueur** = prompt au début du run (max 80 chars) + modifiable à la mort. Si skip au début, prompt à la mort fallback.

### Sprint 6 base technique prête

Foundations Sprint 5 réutilisables :
- `CombatProcessor` 8 phases — ajouter 3 phases Maître sans refactor du squelette
- `CombatPhase` enum extensible
- `OnPhaseChanged` event pour les transitions Maître
- `CapitaineData` SO pattern → `MaitreData` SO (avec champs supplémentaires : citation finale, arène thématique, drum hit, aura color)
- `CapitaineSpawner` queue pattern → `MaitreSpawner` (gated par totalCapitainesDefeated par voie + bouton "Affronter le Maître" volontaire)
- `CapitaineWorldView` + aura sprite → `MaitreWorldView` (aura plus épaisse)
- `CapitaineIntroView` → `MaitreIntroView` (cinématique épique 2-3s, lettrage MASSIF letter-by-letter)
- `CapitaineDeathView` → `MaitreDeathView` (cinématique 5-8s avec Voyage Intérieur)
- `SaveService.Migrate` system en place (v6 actuel, prêt pour v7)

### Sprint 6 nouveaux composants attendus

- `Data/MaitreData.cs` SO (HP énormes 10-20× adversaire, chrono 2-5min, arène, citation finale)
- `Data/MaitreInvocationGate.cs` (logic pour débloquer le "Affronter le Maître")
- `Gameplay/MaitreSpawner.cs` (volontaire, pas auto-spawn)
- `Gameplay/PrestigeService.cs` (calcul Échos, reset Force/upgrades/etc, persist Échos/Reliques)
- `Gameplay/SouffleService.cs` (meditation 5s + buff 30s + cooldown 2min, permanent reset Sprint 6)
- `UI/AffronterMaitreButtonView.cs` (apparait quand X Capitaines battus)
- `UI/PrestigeCinematicView.cs` (5-8s cinematic complète : mort + voyage intérieur + citation joueur + renaissance)
- `UI/CitationInputModal.cs` (prompt max 80 chars, début de run + modifiable à mort)
- Editor utility : `Saga > Sprint 6 > Generate Maitre Assets` (8 SOs)

## Polish items deferred (toujours optionnels)

1. Refactor `MainSceneBootstrap` → scene-authored UI (8 sprints d'affilée, candidate Sprint 7-8)
2. Fix `IsPointerOverGameObject` warnings InputSystem
3. Import fonts Inter + JetBrains Mono + Cinzel/Cormorant (lore) via Font Asset Creator
4. Slice `10_weaponhit_spritesheet.png` pour slash FX frame-by-frame
5. Sprite dédié Capitaines + Maîtres (Sprint 11 polish)

## Prochaine session (Sprint 6)

**Quand brief arrive** : Maîtres + Prestige + Souffle.

**Estimation** : ~6-8 jours d'effort (gros sprint).

**Pré-requis** : brief enrichi du coordinateur avec spécifications détaillées (HP balance, citation des 8 Maîtres, formule Échos, mécanique invocation, etc.).

## Notes session

- **5 sprints clos consécutifs**, workflow merge → tag → cleanup bien rodé.
- 91 tests EditMode total. Coverage solide sur tous les services POCO + data layer.
- `GameEvents.cs` commence à être gros (~150 lignes, ~17 events). Sprint 7+ candidate pour split par domaine (CombatEvents, EconomyEvents, ProgressionEvents).
- Pattern Editor utility (`Saga > Sprint N > Generate Y Assets`) maintenant à 4 utilities (Sprint 2, 3, 4, 5). Sprint 6 ajoutera la 5e (Maîtres).
- Bridge MCP tools non retesté Sprint 5. Filesystem-only 5 sprints d'affilée. Continue.
- Architecture event-driven scale bien : la state machine combat 8 phases reste lisible. L'ajout de 3 phases Maître Sprint 6 sera additif, pas refactor.
- Quand Sprint 6 ajoutera le Prestige, la `SaveService.Migrate` chain v6→v7 devra gérer le reset partial. Pattern déjà éprouvé sur 5 migrations.
