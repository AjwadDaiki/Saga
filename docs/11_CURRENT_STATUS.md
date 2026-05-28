# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 7 + Sprint 7.5 implémentés (feat branch `feat/sprint-7.5-polish-mobile-pro` empilée sur `feat/sprint-7-voies-modulaire`). Sprint 7 = modulaire sprites + Voies + Equipment + Inventaire + Reliques. Sprint 7.5 = design system mobile pro (DesignTokens SO, background dojo, mannequin redesign, ForceCounter polish, Élan bar glow, cards glass morphism, SagaButton, AudioService procédural, HapticService). En attente validation in-play avant merge `dev` → tag `v0.7.5` (skip v0.7.0 separate, on tag direct la version polish).

**Dernière session**: 2026-05-28, dev Claude (Opus 4.7) sur Claude Code.

**Branche active**: `feat/sprint-7.5-polish-mobile-pro` (depuis `feat/sprint-7-voies-modulaire`).

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
| 7 | `v0.7.0` (skipped) | Sprites modulaires (Body/Armor/Weapon) + 8 Voies + EquipmentService + Inventaire + Reliques équipables | 🟡 implémenté, mergé dans 7.5 |
| 7.5 | `v0.7.5` (pending) | Design system mobile pro 2026 : DesignTokens SO + background dojo (gradient + particles + plancher) + mannequin redesign 3-sections + SagaButton unified + Cards glass morphism + ForceCounter polish + AudioService procédural + HapticService | 🟡 implémenté, en attente play validation |

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

- **Sprints closed** : 6/14 (Sprint 7 implémenté, en attente play validation + tag)
- **Branches** : main + dev synced à `9cee36e` (tag v0.6.0 à `e9974c9`) ; `feat/sprint-7-voies-modulaire` active
- **Tags** : `v0.1.0` → `v0.6.0` (Sprint 7 → `v0.7.0` à venir)
- **Lignes de code C# runtime (hors lib tierce)** : ~6300
- **Tests EditMode** : 130 cases (114 préexistants + 16 Sprint 7 : EquipmentService 6 + SpriteLayerSet 3 + LayeredCharacterRenderer 4 + MaitreReliqueDrop 3)
- **ScriptableObjects** : 42 (3 upgrades + 1 anim library + 5 adversaires + 8 capitaines + 8 maîtres + 9 sprite layer sets MVP + 8 reliques layer sets + 8 voies = 25 + 17 = 42)
- **Combat phases implémentées** : 11/11 (Training + Adv×3 + PlayerDeath + Cap×3 + Maître×3)
- **GameEvents channels** : ~30 (Sprint 7+ candidate pour split par domaine)
- **Voies tintées** : 9 (None + 8 cultures)
- **DeathRecords** : ajoutés au Hall des Légendes (UI surface Sprint 10+)

## Sprint 7 — accomplissements (en attente validation play)

> "C'est LE sprint qui rend SAGA scalable." — Coordinateur

- **`SpriteLayerSet` SO** : 1 corps + 3 armures + 5 armes MVP (8 weapons-slot reliques de Maître supplémentaires = 9+8=17 SOs au total). Champs : id, slot, voie, rarity, idle/attack1/2/3/hurt/meditation/die arrays, statsBonusForce (BigDouble), statsBonusCrit (float), iconSprite, frameDuration. Factory `CreateForTests`.
- **`VoieData` SO** : 8 voies (Samurai/Viking/Wuxia/Spartiate/Mongol/Saladin/Aztec/Gaulois). Couleurs main+accent, intro citation, description, bonus description (mécaniques portées par GameState, voir `voiesMastered`).
- **`LayeredCharacterRenderer`** : 3 SpriteRenderer enfants (Body z=0 / Armor z=1 / Weapon z=2). Une seule horloge (Body = autoritative). Looping : idle, meditation. One-shots : attack1/2/3, hurt, die. Fallback chain : requested anim → layer idle → null. Static helper `IsLooping(anim)`.
- **`CharacterView` refactor** : remplace `SpriteAnimator` legacy. Listen OnTapResolved/OnComboChanged/OnStadeChanged/OnSouffleStarted/Ended/OnEquipmentChanged. Pendant Souffle, joue meditation puis revient à idle. Breath DOScale avec SetLink.
- **`EquipmentService` POCO** : AddToInventory (idempotent) / Equip (refuse wrong-slot ou non-owned) / Unequip (refuse Body, qui est toujours équipé) / GetEquipped / GetTotalStatsBonus (somme Force de tous les slots). Raise OnEquipmentChanged + OnItemAddedToInventory.
- **`StatsCalculator.GetForcePerTap`** : intègre désormais `GetEquipmentForceBonus` (pure read GameState → ContentDatabase IDs).
- **`ContentDatabase`** : ajout `_layerSetsById` + `_voiesByEnum` + arrays + `AllSpriteLayerSets` + `AllVoies` + `GetSpriteLayerSet(id)` + `GetVoie(enum)`. Constructor 6-arg avec auto-load Resources/SpriteLayerSets et Resources/Voies.
- **`GameState` v9** : nouveaux champs `inventoryLayerSetIds` (List<string>), `equippedBodyId` (default `body_chibi_neutral`), `equippedArmorId`, `equippedWeaponId`, `voieSelectedId`, `voiesMastered` (List<string>). Migration v8→v9 garante body default toujours présent en inventaire.
- **`SaveService.Migrate`** : currentVersion = 9. Migration idempotente + defensive boot reset (citation, élan, souffle buff actif).
- **3 Editor utilities** : `Saga > Sprint 7 > Generate Sprite Layer Sets`, `Generate Voie Assets`, `Generate Maitre Relique Layer Sets` (wire les 8 reliques weapon-slot sur les MaitreData existants).
- **UI Inventaire** : modal procédural plein écran avec sections Body/Armor/Weapon, équipé en tête, click "Équiper". Bouton `INVENTAIRE` top-right (symétrique au `SOUFFLE` top-left). Subscribe OnEquipmentChanged + OnItemAddedToInventory pour rebuild live.
- **Reliques Maître drop** : CombatProcessor.OnMaitreDefeated grant `ReliqueSpriteLayerSetId` → inventaire via EquipmentService. Stats-only Sprint 7 (D3) ; visuelles Sprint 8+.
- **Prestige persistence matrix élargie (Sprint 7)** : inventoryLayerSetIds, equippedBodyId/Armor/Weapon, voieSelectedId, voiesMastered PERSIST. Gear et voie sont des échelles long-terme, pas run-scoped.
- **MainSceneBootstrap.BuildCharacter** refactor : crée Body+Armor+Weapon enfants + LayeredCharacterRenderer + CharacterView, layer assignment initial depuis GameState.
- **GameEvents Sprint 7** : OnEquipmentChanged(slot, next, previous), OnItemAddedToInventory(layer), OnVoieSelected(prev, next), OnVoieMastered(voie).
- **16 tests EditMode** ajoutés : EquipmentServiceTests (6), SpriteLayerSetTests (3), LayeredCharacterRendererTests (4), MaitreReliqueDropTests (3). Total : **130 cases**.

## ⏸️ Sprint 7 — brief consommé

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
