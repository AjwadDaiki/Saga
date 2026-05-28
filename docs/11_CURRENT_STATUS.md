# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 6 closed (validé in-play par Ajwad, mergé `dev` + `main`, tagué `v0.6.0`). **Sprint 7 en attente du brief enrichi** (système modulaire des sprites + identité culturelle des voies). Roadmap allongée à 14 sprints, lancement officiel envisagé **octobre 2026**.

**Dernière session**: 2026-05-28, dev Claude (Opus 4.7) sur Claude Code.

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
| 6 | `v0.6.0` | Maîtres légendaires (8) + Prestige + Souffle | ✅ closed |

## Sprint 6 — accomplissements clés (validés en play)

> "Sprint 6 = LE moment magique du jeu." — Ajwad

- **Souffle** : meditation 5s + buff ×1.5 Force 30s + cooldown 120s permanent à travers le prestige
- **3 Capitaines battus** → bouton "Affronter Maître" apparait avec scale punch
- **Modal Maîtres** : liste des 8 légendaires avec portraits procéduraux + citations d'intro + bouton "Affronter"
- **Cinematic intro Yoshitsune** : vignette + lettrage MASSIF + citation, 2.5s
- **Combat Maître** : 180s chrono, phases HP color (vert/jaune/orange/rouge enrage), enrage chrono ×2
- **Scenario victoire** : drop Relique unique + cinematic "VICTOIRE" doré
- **Scenario défaite (THE moment)** : cinematic Prestige 17s complète
  1. **TU ES MORT** (5s) — slow-mo + fade noir + titre MASSIF + defeat citation Maître
  2. **Voyage Intérieur** (4s) — stats du run + Échos counter animé en doré
  3. **Citation finale** (5s) — prompt OR edit (max 80 chars)
  4. **Renaissance** (3s) — fade blanc + Stade 1 reborn + "Tu renais. Que ta prochaine légende soit plus longue."
- **Reset state matrix** validée : `force=0, upgrades=0, stade=1` mais `totalEchos / reliques / titres / citation / cooldown Souffle` PERSIST
- **Hall des Légendes** : DeathRecord appended à chaque mort vs Maître (voie, nom, citation, forceMax, UTC)
- **114 tests EditMode** verts

## Métriques de projet

- **Sprints closed** : 6/14
- **Branches** : main + dev synced à `9cee36e` (tag v0.6.0 à `e9974c9`)
- **Tags** : `v0.1.0` → `v0.6.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~5500
- **Tests EditMode** : 114 cases (NumberFormatter 13, ComboSystem 11, UpgradeData 5, StatsCalculator 8, UpgradeService 10, StadeManager 10, AdversaireData 3, AdversaireSpawner 5, CombatProcessor 8, ElanService 6, CapitaineData 3, CapitaineSpawner 5, VagueResolver 4, MaitreData 4, MaitreSpawner 5-6, PrestigeService 8, SouffleService 6-7)
- **ScriptableObjects** : 25 (3 upgrades + 1 anim library + 5 adversaires + 8 capitaines + 8 maîtres)
- **Combat phases implémentées** : 11/11 (Training + Adv×3 + PlayerDeath + Cap×3 + Maître×3)
- **GameEvents channels** : ~30 (Sprint 7+ candidate pour split par domaine)
- **Voies tintées** : 9 (None + 8 cultures)
- **DeathRecords** : ajoutés au Hall des Légendes (UI surface Sprint 10+)

## ⏸️ Sprint 7 — en attente du brief enrichi

### Décisions coordinateur déjà loguées (DESIGN_DECISIONS_LOG.md)

1. **Roadmap allongée à 14 sprints**. Lancement officiel envisagé **octobre 2026** (vs août-septembre prévu initialement)
2. **Système modulaire des sprites** validé pour Sprint 7 (longueur **6-7 jours** au lieu de 5)
   - 3 couches layered et interchangeables : **Corps + Armure + Arme** (sprites séparés)
   - Pattern : `SpriteLayerSet` SO, `LayeredCharacterRenderer` MonoBehaviour, `CharacterView` consomme un set au lieu d'un single library
3. **D6 — Système d'armes communautaires** (post-MVP, mois 3-4) : 3 tiers (Easter Egg / Communauté Standard / Officiel), outil Piskel/Aseprite, distribution VPS HiddenLab. Le système modulaire Sprint 7 doit supporter ce use case sans refactor ultérieur.

### Sprint 7 base technique prête à consommer

Foundations Sprint 6 réutilisables pour Sprint 7 :
- `CombatProcessor` 11 phases extensible (Stade-driven evolution n'ajoute pas de phases, juste re-skin)
- `Voie` enum déjà tinté partout (CombatHud HP color, MaitreData arenaBg, etc.)
- `SpriteAnimator` peut rester ou être étendu en `LayeredSpriteAnimator` (sync N renderers par frame index)
- `MainSceneBootstrap.BuildCharacter` refactor candidat : spawn 3 SpriteRenderer enfants au lieu d'1
- `AdventurerAnimationLibrary` (rvros 5 anims) devient le "Corps Stade 1 Mendiant" baseline
- `SaveService.Migrate` chain prête pour v8→v9 (équipement actif sur perso : armure ID + arme ID dans GameState)
- Pattern Editor utility `Saga > Sprint N > Generate Y Assets` (5 utilities déjà — Sprint 2/3/4/5/6) — Sprint 7 ajoutera la 6e (Generate Sprite Layer Sets)

### Sprint 7 nouveaux composants attendus (à confirmer au brief)

- `Data/SpriteLayerSet.cs` SO (3 layers : Body / Armor / Weapon)
- `Data/EquipmentSlot.cs` enum (Armor / Weapon) — Sprint 8+ étend (Helmet, Talisman, etc.)
- `Gameplay/LayeredCharacterRenderer.cs` MonoBehaviour
- `Gameplay/EquipmentService.cs` (gère armure équipée + arme équipée, mises à jour visuelles via OnEquipmentChanged event)
- `UI/EquipmentInventoryView.cs` (Sprint 7 MVP : panneau liste Reliques + bouton équiper/déséquiper)
- Editor utility : `Saga > Sprint 7 > Generate Sprite Layer Sets` (corps Mendiant + 1-2 armures + 1-2 armes MVP)
- Refactor : `CharacterView` consomme `SpriteLayerSet` au lieu de `SpriteAnimationLibrary`
- Refactor : `MaitreData.reliqueStatsBonus` consommé par `EquipmentService` quand équipé

## Polish items deferred (toujours optionnels)

1. Refactor `MainSceneBootstrap` → scene-authored UI (9 sprints d'affilée — candidate Sprint 8-9 si on a le temps)
2. Import fonts Inter + JetBrains Mono + Cinzel/Cormorant (lore) via Font Asset Creator
3. Slice `10_weaponhit_spritesheet.png` pour slash FX frame-by-frame
4. Sprite dédié Capitaines + Maîtres (Sprint 11 polish)
5. Hall des Légendes UI (Sprint 10 candidate)
6. UI sélection Reliques à conserver au prestige (Sprint 7+ MVP toutes persistent)
7. Drum hit audio à Phase 1 cinematic Prestige (`MaitreData.DrumHitPitch` déjà prêt)
8. Sprint 7+ split `GameEvents.cs` par domaine (CombatEvents, EconomyEvents, ProgressionEvents)

## Prochaine session (Sprint 7)

**Quand brief arrive** : système modulaire sprites (Corps + Armure + Arme layered) + identité culturelle des voies. Estimation **6-7 jours**.

**Pré-requis** : brief enrichi du coordinateur avec spécifications détaillées (combien de SpriteLayerSets MVP, quelle voie démarre, comment équiper visuellement, Reliques visuelles ou stats-only Sprint 7).

## Notes session

- **6 sprints clos consécutifs**, workflow merge → tag → cleanup hyperrodé. 6 tags propres : `v0.1.0` → `v0.6.0`.
- Sprint 6 = pic narratif du projet selon Ajwad ("LE moment magique"). La cinematic Prestige 4 phases a tenu sa promesse en play.
- 114 tests EditMode, tous green-ready. Coverage solide sur tous les services POCO + data layer + state machines.
- `GameEvents.cs` ~30 events maintenant — split par domaine devient pertinent Sprint 7+ pour clarté.
- L'état "code-staged-mais-pas-commité" au démarrage de cette session était inhabituel mais récupéré proprement après audit (5 fichiers critiques relus, match tight au brief). Pattern à éviter à l'avenir : commit immédiat après implementation, validation peut se faire sur un commit propre.
- Pattern Editor utility (`Saga > Sprint N > Generate Y Assets`) maintenant à 5 utilities (Sprint 2/3/4/5/6). Sprint 7 ajoutera la 6e pour les SpriteLayerSets.
- Bridge MCP Unity tools : disconnect signal en début de session (UnityMCP no longer available). Filesystem-only continue d'être la baseline opérationnelle — 7e sprint d'affilée sans drama.
- Roadmap allongée à 14 sprints / lancement octobre 2026. 8 sprints restants : Sprint 7 (sprites modulaires) → 8 (esprits) → 9 (carte monde) → 10 (hub features) → 11 (polish) → 12 (beta) → 13-14 (préparation lancement + ajustements).
