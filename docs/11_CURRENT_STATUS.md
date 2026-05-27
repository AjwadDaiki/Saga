# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 4 closed (validé in-play par Ajwad, mergé `dev` + `main`, tagué `v0.4.0`). **Sprint 5 en attente du brief** (Boss Mineurs + Élan, coordinateur prépare réponses aux 3 questions).

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `dev` (en standby), pas de feat branch active.

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur ticker, combo 4 tiers, "+X" floating, dust, save/reload | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation. Cards bottom, near-miss glow. | ✅ closed |
| 3 | `v0.3.0` | Pixel adventurer + 3 attack variations weighted + stade transition + NumberJuice | ✅ closed |
| 4 | `v0.4.0` | Combat Active System : Adversaire spawn + chrono + mort temporaire + 5 adversaires | ✅ closed |

## Sprint 4 — accomplissements clés (validés en play)

- **Barre "Prochain Adversaire"** se remplit par tap (threshold par stade : 50/100/200/400/800/1600)
- **5 adversaires** Sprint 4 implémentés (Ronin Errant, Spadassin Nordique, Initié Wuxia, Hoplite Lâche, Pèlerin du Nord)
- **Combat actif chronométré** : HP bar descend, chrono décompte, taps dealent damage (pas Force pendant combat)
- **Victoire** : reward Force ajoutée, animation fade-out, retour mannequin 2s
- **Mort temporaire** : écran noir "TU ES MORT" + quote + click-to-resume, -10% Force penalty
- **Phase state machine** propre (5 phases via `CombatPhase` enum + `CombatProcessor` POCO)
- **73 tests EditMode** verts (16 Sprint 4)
- **Editor utility** `Saga > Sprint 4 > Generate Adversaire Assets` one-click

## Métriques de projet

- **Sprints closed** : 4/11
- **Branches** : main + dev synced à `8ebfb2a` (tag v0.4.0)
- **Tags** : `v0.1.0`, `v0.2.0`, `v0.3.0`, `v0.4.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~3000
- **Tests EditMode** : 73 cases (NumberFormatter 13, ComboSystem 11, UpgradeData 5, StatsCalculator 8, UpgradeService 10, StadeManager 10, AdversaireData 3, AdversaireSpawner 5, CombatProcessor 8)
- **ScriptableObjects** : 9 (3 upgrades + 1 SpriteAnimationLibrary + 5 adversaires)
- **Phases combat implémentées** : 5/5 Sprint 4 (Training, AdversaireIncoming, AdversaireActive, AdversaireVictory, PlayerDeathTemporary). Bosses Sprint 5-6.

## ⏸️ Sprint 5 — en attente du brief enrichi

Coordinateur prépare le brief Sprint 5 (Boss Mineurs / Capitaines + mécanique Élan). 3 questions à trancher :
1. **Visuels Capitaines** : sprite dédié ou même adventurer rvros tinté par voie ?
2. **Élan jauge UI** : remplace l'affichage combo actuel ou en plus ?
3. **Mécanique Vague AOE** : visuel basique Sprint 5 ou polish Sprint 11 ?

Pendant ce temps, le dev Claude reste sur `dev` en standby.

## Sprint 5 — base technique prête à consommer

Foundations Sprint 4 prêtes pour Sprint 5 :
- `CombatProcessor` state machine extensible (2 phases bosses à ajouter sans refactor du squelette)
- `CombatPhase` enum extensible
- `OnPhaseChanged` event utilisable pour les transitions boss
- `DamageDealer` réutilisable (damage logic identique pour bosses)
- `AdversaireData` SO pattern réutilisable pour `CapitaineData` (mêmes champs + HP/phases plus complexes)
- `MainSceneBootstrap.BuildAdversaire` réutilisable pour le boss (même position, sprite différent)
- 50+ frames rvros restantes dans `downloads/` pour les boss (run, jump, fall, die particulièrement)
- 30 Échos + Reliques (Sprint 6) pourront utiliser le même `ContentDatabase` + Editor utility pattern

## Polish items deferred (toujours optionnels)

1. **Refactor `MainSceneBootstrap` → scene-authored UI** : encore non traité (MCP tools flaky)
2. **Fix `IsPointerOverGameObject` warnings** : encore non traité
3. **Import fonts Inter + JetBrains Mono** : encore non traité
4. **Slice `10_weaponhit_spritesheet.png`** pour slash FX frame-by-frame
5. **"+X" → "-X" floating en combat** : log mineure Sprint 4, à fix début Sprint 5

## Questions ouvertes pour le coordinateur

Aucune côté dev en standby. Toutes les questions Sprint 5 listées ci-dessus, en cours côté coordinateur.

## Prochaine session (Sprint 5)

**Quand brief arrive** : Boss Mineurs + Élan + Vague AOE. 4-5 jours d'effort estimé.

**Pré-requis Sprint 5** : tu valides ce sprint 4 (déjà fait ✅) + coordinateur livre brief avec décisions sur les 3 questions.

## Notes session

- 4 sprints clos consécutifs, workflow merge → tag → cleanup → status update bien rodé.
- Bridge MCP tools toujours flaky côté dev Claude (resource layer OK, tool routing échoue avec "no instance found"). 4 sprints d'affilée en filesystem-only validés sans drama — c'est la baseline opérationnelle.
- Sprint 4 fait passer SAGA d'un idle pur à un idle + combat actif chronométré. Première vraie couche "tension" introduite. Brief Sprint 5 ajoute Élan (jauge tap accumulator + Vague AOE button) — autre forme de tension active.
- 73 tests EditMode total — coverage solide sur data + services + state machines. UI views non testées (visual integration, validation play par Ajwad).
- 600 lignes Sprint 4 (3000 total). Bon ratio de productivité. Le state machine pattern (CombatProcessor) et le service POCO pattern (AdversaireSpawner, DamageDealer) sont devenus le standard architectural — Sprint 5 Capitaines suivra la même grammaire.
