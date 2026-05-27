# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 3 closed (validé in-play par Ajwad, mergé `dev` + `main`, tagué `v0.3.0`). **Sprint 4 en attente du brief enrichi** (Boss/Adversaire/Cycle re-architecture en cours côté coordinateur).

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `dev` (en standby), pas de feat branch active.

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins (BreakInfinity, DOTween) | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur ticker, combo 4 tiers, "+X" floating, dust particles, save/reload | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe, Disciple, Méditation. UI cards bottom, near-miss glow. | ✅ closed |
| 3 | `v0.3.0` | Pixel adventurer character + 3 attack variations weighted by combo + stade transition + NumberJuice | ✅ closed |

## Sprint 3 — accomplissements clés (validés en play)

- **Pixel adventurer** rvros visible, idle anim + breathing DOScale infinite pour aliveness constante
- **3 variations d'attaque** weighted par combo tier (attack1 majoritaire low combo, attack3 émerge à haut combo) — observé en play avec combo x2.6
- **Slash FX** procédural ambre entre character et mannequin, lifecycle propre (Destroy + 50ms safety)
- **Mannequin** proportionné (~1.6× character) avec sprite procédural 40×80 PPU 32
- **NumberJuice** complet : fontSize + couleur compteur Force adaptifs par magnitude
- **Stade transition cinematic** 2s overlay au franchissement du seuil 1k Force
- **Editor utility** `Saga > Sprint 3 > Configure Adventurer Assets` (one-click idempotent SO + import config)

## Métriques de projet

- **Sprints closed** : 3/11
- **Branches** : main + dev synced à `b5c466a` (tag v0.3.0)
- **Tags** : `v0.1.0`, `v0.2.0`, `v0.3.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~2400
- **Tests EditMode** : 57 cases (NumberFormatter 13 + ComboSystem 11 + UpgradeData 5 + StatsCalculator 8 + UpgradeService 10 + StadeManager 10)
- **ScriptableObjects** : 3 upgrades + 1 SpriteAnimationLibrary (4 total)
- **Voies implémentées** : 0/8 (Sprint 4+)
- **Stades visuels actifs** : 1 (Adventurer, neutre, utilisé pour stade 1 → 6)

## ⏸️ Sprint 4 — en attente du brief enrichi

Le coordinateur + Ajwad re-architecturent le game design avec :
- **Système Boss / Adversaire / Cycle de combat** (au lieu du mannequin seul)
- **Mort narrative** (face aux boss → déclenche prestige)
- **Renforcement identité narrative**
- 30 idées d'amélioration UI/UX/feel à prioriser

→ Sprint 4 ne démarre pas tant que le brief enrichi n'est pas livré. Le dev Claude reste sur `dev` en standby.

## Sprint 4 — base technique prête à consommer

Quand le brief arrive, on aura déjà en place pour s'appuyer :
- Event system : `OnTapResolved`, `OnComboChanged`, `OnStadeChanged`, `OnUpgradePurchased` — facile d'ajouter `OnBossDamaged`, `OnPlayerDied`, etc.
- `GameTicker` 10Hz avec slots libres pour BossProcessor / CombatProcessor
- `SpriteAnimator` réutilisable pour les bosses (juste un library SO par boss)
- `StadeManager` avec transitions idempotentes — pourrait orchestrer les phases de combat
- `MainSceneBootstrap` peut spawn un boss à la place / en plus du mannequin
- `SaveService` migration system en place (v3 actuel, prêt pour v4 quand le schéma grandit)

## Polish items deferred (toujours optionnels)

1. **Refactor `MainSceneBootstrap` → scene-authored UI** : à reprendre maintenant que MCP serait UP côté Ajwad (bridge tools peut-être encore flaky côté dev Claude — à tester en début Sprint 4)
2. **Fix warnings `IsPointerOverGameObject`** : encore non traité — mérite play test post-fix
3. **Import fonts Inter + JetBrains Mono via Font Asset Creator** : encore non traité
4. **Slice du `10_weaponhit_spritesheet.png`** : pour slash FX frame-by-frame Sprint 4+ polish

## Questions ouvertes pour le coordinateur

Aucune côté dev. Toutes les questions ouvertes sont sur le brief Sprint 4 en cours d'élaboration (Boss/Adversaire/Cycle).

## Notes session

- **Bridge MCP** : Ajwad signale UP côté Editor (Session Active vert). Côté dev Claude le tool layer route encore via "No Unity Editor instances found" — possible bug routing de la beta. À retester début Sprint 4.
- **Pattern Editor utility** confirmé sur 3 sprints (1 utility Sprint 2, 1 Sprint 3). Sprint 4+ on en aura sûrement pour les VoieData / BossData SOs.
- **3 sprints de suite en filesystem-only** sans drame — workflow rodé, bridge MCP est un "nice-to-have" pas un blocker.
- 24 sprites individuels Adventurer importés, library SO de 5 anims. Reste ~50 frames non utilisés dans `downloads/Adventurer/Individual Sprites/` pour les sprints suivants (run, jump, fall, die, hurt, smrslt, crouch, slide, etc.) — voir le tableau dans le précédent rapport 🛠️.
- L'art direction "Simple Chibi Pixel Adventurer" tient la route — feel "alive" obtenu via combinaison frame anim + breathing DOScale + slash FX + mannequin shake. Pour Sprint 4 boss, le même rig adventurer peut être réutilisé (palette swap ou library override) ou on importera un asset boss dédié.
