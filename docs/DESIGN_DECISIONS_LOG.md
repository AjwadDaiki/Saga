# DESIGN DECISIONS LOG

> Chaque décision de design (game design, visuel, ou architecture importante) prise en cours de dev est loggée ici.
> Format: date, contexte, décision, raison.
> Cela évite de relitiger les mêmes points et garde une traçabilité.

## 2026-05-27 — Direction visuelle verrouillée

**Décision**: Direction A "Lame & Encre" (sumi-e moderne dark) comme base, avec accents C "Voie Brutale" (high contrast) aux moments épiques (prestige, boss kill, unlock voie, milestone Mythe).

**Raison**: Direction A gagne sur la longueur (sessions du soir, OLED-friendly, ton prestigieux qui matche l'univers mythique-humain). Direction C ajoute le punch nécessaire aux pivots narratifs. Direction B (Parchemin) écartée pour éviter la confusion identitaire.

**Conséquence**: tous les assets et UI suivent la palette définie dans 05_VISUAL_STYLE.md.

---

## 2026-05-27 — Suppression du tier "Divin" / "Ascension céleste"

**Décision**: Le palier final visuel est **Mythe** (humain devenu légende), pas un tier divin. 6 stades au total (Mendiant → Mythe), pas 7.

**Raison**: Le ton du jeu est mythique-humain (cf 01_VISION.md). Une ascension divine cassait la narration "underdog devenu légende par la mémoire". L'arc final est "reconstituer la Voie Originelle" via la connaissance des cultures, pas devenir un dieu.

**Conséquence**: pas de currency "Essence Divine". On reste sur 4 currencies (Force, Technique, Renom, Échos). Régions mythiques (Olympe, etc.) restent dans le ton "terres légendaires", pas dimensions divines.

---

## 2026-05-27 — Stack technique de base

**Décision**: Unity 6 LTS + URP 2D + BreakInfinity.cs + DOTween (free puis Pro à 15$) + TextMeshPro + Addressables + 2D Animation + Cinemachine + Newtonsoft.Json + Coplay Unity MCP.

**Raison**: Setup éprouvé pour idle game mobile, libs gratuites ou peu chères, MCP déjà maitrisé par Ajwad.

**Conséquence**: voir 06_TECH_STACK.md pour la liste complète. Spine 2D et FMOD restent des décisions ouvertes à trancher en cours de dev.

---

## 2026-05-27 — Sprint 0: DOTween free imported manually via Asset Store

**Décision**: DOTween free n'est pas auto-installé via le script Sprint 0. Importé manuellement par Ajwad via Asset Store au focus Unity. Location effective: `Assets/Plugins/Demigiant/DOTween/` (DOTween crée son propre subfolder `Demigiant/` sous `Plugins/`).

**Raison**: DOTween free n'est pas dispo sur OpenUPM ni en UPM officiel. Télécharger un .zip du site demigiant.com sans validation = risque. L'install via Asset Store est triviale (Window → Package Manager → My Assets → DOTween → Import) et standard chez tous les devs Unity.

**Conséquence**: 
- `Assets/Plugins/DOTween/` placeholder supprimé (DOTween a créé son propre `Assets/Plugins/Demigiant/DOTween/`)
- 06_TECH_STACK.md ligne 129 mentionne `Plugins/DOTween/` — légèrement off, la realité est `Plugins/Demigiant/DOTween/`. Pas critique, à amender au prochain pass doc.
- `DOTween.dll` et `DOTweenEditor.dll` seront stockés via Git LFS (pattern `*.dll` dans `.gitattributes`).
- Sprint 0 DOTween: ✅ FAIT.

---

## 2026-05-27 — Sprint 6 pre-decisions (tranchées par coordinateur post-Sprint-5)

### Q1 — Maîtres visuels (Boss Majeurs)
**Décision**: rvros tinted avec **aura épaisse** pour Sprint 6 (placeholder cohérent avec la stratégie sprite Sprint 5 Capitaines).
- Réutilise le pattern `CapitaineWorldView` mais avec aura plus large + tint plus intense
- Sprite dédié par Maître → **Sprint 11 polish** uniquement
- 8 Maîtres légendaires : Yoshitsune, Ragnar, Sun, Léonidas, Subutaï, Salah ad-Din, Ahuitzotl, Vercingétorix

### Q2 — Souffle cooldown au prestige
**Décision**: **Permanent**. Les mécaniques de jeu (skills, cooldowns) ne reset pas au prestige.

| Prestige reset | Prestige persiste |
|---|---|
| Force | Échos accumulés |
| Levels d'upgrades | Reliques conservées (1 par run) |
| Voies non-maîtrisées | Titres |
| totalAdversairesDefeated | Achievements |
| totalCapitainesDefeated | **Cooldowns mécaniques (Souffle, Vague timing)** |
| Adversaires actifs / Capitaines / Maîtres | citations finales accumulées |

**Raison**: Un cooldown qui reset au prestige serait punitif et casserait le rythme du nouveau run. Les mécaniques actives sont "appris" — elles ne se désapprennent pas.

### Q3 — Citation finale du joueur (prompt timing)
**Décision**: **Prompt au début du run + modifiable à la mort**.
- 1er run : prompt "Écris ta première phrase…" (max 80 caractères) avant le tap loop
- Mort vs Maître : citation affichée dans la cinématique, **éditable avant validation**
- Si pas de citation écrite au début (skip) : prompt au moment de la mort comme fallback
- Citations cumulent dans `Hall des Légendes` du joueur

**Raison**: Donne le contrôle au joueur (peut planifier sa devise), tout en gardant la possibilité d'improviser à la mort. Le double-temps (début + mort) évite la frustration d'un prompt obligatoire en plein middle of nowhere.

**Conséquence**:
- `GameState.playerCitation` (string, nullable) à ajouter Sprint 6
- `Hall des Légendes` data structure (Sprint 10 surface), mais data tracking dès Sprint 6
- UI prompt component à créer (modal text input)

---

## 2026-05-27 — Sprint 5: Élan synergy with combo (×2 fill rate when combo active)

**Décision**: Élan gain par tap = `ElanPerTap (5) × (comboTier > 0 ? ElanComboBonusMultiplier (2) : 1)`.
Pas de combo = +5 Élan/tap (20 taps pour cap). Combo actif = +10 Élan/tap (10 taps pour cap).

**Raison**: Brief Sprint 5 demande "quand combo monte, l'Élan se remplit 2x plus vite" — synergie explicite. Le bonus se déclenche dès tier 1 (3+ taps consécutifs), pas réservé au tier max. Encourage à maintenir le combo activement.

**Conséquence**:
- `ElanService.RegisterTap(state, comboTier)` testable directement
- `ElanService.HandleTapResolved` tracker `_currentComboTier` via subscription à `OnComboChanged`
- Refactor Sprint 6 si on veut différencier par tier (e.g. tier 3 = ×3 fill rate)

---

## 2026-05-27 — Sprint 5: CapitaineSpawner queue + CombatProcessor consume pattern

**Décision**: Le `CapitaineSpawner` listens to `OnAdversaireDefeated`, vérifie `totalAdversairesDefeated % 10 == 0`, et stocke une `_pending CapitaineData`. Le `CombatProcessor` consulte `ConsumePending()` lors de la transition AdversaireVictory → Training et, si non-null, lance `StartCapitaineIncoming` au lieu de revenir à Training.

**Raison**: Évite un coupling direct CapitaineSpawner → CombatProcessor (les responsibilities restent claires). Permet aussi de différer le spawn de quelques frames (entre la défaite de l'adversaire à la fin de sa Victory celebration), ce qui matche l'UX souhaitée (le joueur voit la victoire de l'adversaire puis le Capitaine arrive).

**Conséquence**:
- `CombatProcessor.AttachCapitaineSpawner(spawner)` injection late par `GameManager` (évite circular dep)
- Pattern reproductible pour Maîtres Sprint 6 (`MaitreSpawner` listens to capitaines defeated count)

---

## 2026-05-27 — Sprint 5: Capitaine HP partagé avec adversaire (currentAdversaireHp)

**Décision**: Pour Sprint 5, l'HP de la `Capitaine` engagé est stocké dans `GameState.currentAdversaireHp` (le même field utilisé pour les adversaires). `currentCapitaineId` distingue le type. `currentAdversaireId` est mis à null quand un Capitaine est actif.

**Raison**: Évite une duplication de schema (currentAdversaireHp + currentCapitaineHp + un demain pour les Maîtres). Le field "HP de l'ennemi actif" est conceptuellement unique. Le naming `currentAdversaireHp` est legacy Sprint 4 — pourra être renommé `currentEnemyHp` Sprint 7+ lors d'un nettoyage de schema (avec migration v6→v7).

**Conséquence**:
- `DamageDealer` / `CombatProcessor` font un phase check pour savoir si l'ennemi actif est Adversaire ou Capitaine, puis lookup la maxHp via le bon Content getter
- `VagueResolver` même pattern

---

## 2026-05-27 — Sprint 4: Combat phase state machine (CombatProcessor POCO)

**Décision**: Le cycle de combat est modélisé comme une state machine POCO `CombatProcessor`, ticked par `GameTicker` à 10Hz. Les phases (`CombatPhase` enum) sont :
`Training` → `AdversaireIncoming` (1s cinematic) → `AdversaireActive` → soit `AdversaireVictory` (2s) → `Training`, soit `PlayerDeathTemporary` → (click pour reprendre) → `Training`.

Les transitions raise `OnPhaseChanged(prev, next)`. Les Views UI/World filtrent leur visibilité sur l'event.

**Raison**: Pattern reproductible (mêmes APIs que `StadeManager`). POCO = testable sans Unity. Le tick polling pour fin-de-combat (HP ≤ 0, chrono ≤ 0) garde `DamageDealer` simple et découple les responsabilities.

**Conséquence**:
- `CombatProcessor.Tick`, `StartIncoming`, `ResolveDeathTemporary` — surface minimale
- Tests EditMode 8 cases couvrent toutes les transitions
- Sprint 5 (Capitaines/Boss) ajoutera 2 phases (`BossIncoming`, `BossActive`, etc.) sans refactor du squelette

---

## 2026-05-27 — Sprint 4: les taps en combat dealent du damage, pas de la Force

**Décision**: Pendant `AdversaireActive`, chaque tap calcule `gain = forcePerTap × comboMult` mais l'applique au HP de l'adversaire au lieu de l'ajouter à `GameState.force`. La Force est gagnée uniquement à la victoire (`AdversaireData.RewardForce`).

Implementation: `TapHandler.OnTapPerformed` gating explicite sur `state.currentPhase`. `DamageDealer` (POCO subscriber to `OnTapResolved`) applique le damage uniquement en `AdversaireActive`.

**Raison**: Le brief Sprint 4 précise "Issues: Victoire → Drop de récompenses (Force bonus + chance loot)". Implicitly = pas de Force per-tap en combat. Préserve l'incentive narratif : la victoire est une récompense distincte, pas un gain progressif.

**Conséquence**:
- Les cards upgrades restent achetables pendant le combat (pas de raison de bloquer)
- `TapFxSpawner` "+X floating" garde la même UX visuelle mais représente damage en combat (peut-être à refiner Sprint 5 polish — `-X` depuis l'adversaire serait plus correct)
- `ComboSystem` continue de fonctionner cross-phase — le combo n'est pas reset en quittant Training

---

## 2026-05-27 — Sprint 4: boot policy = reset combat state à Training

**Décision**: `SaveService.Migrate` force `currentPhase = Training`, `currentAdversaireId = null`, `chronoRemaining = 0` à chaque load, indépendamment de la version save.

**Raison**: Évite de loader mid-combat avec un chrono stale qui se déclencherait à 0 dès le boot → `PlayerDeathTemporary` à l'ouverture du jeu = horrible UX. Plus simple : reset systématique. Sprint 5+ pourra persister le combat state intentionnellement si pertinent (e.g. quitter mid-boss pour ne pas perdre une victoire).

**Conséquence**:
- Quitter mid-combat = perdre le combat en cours (le joueur revient au mannequin)
- Pas vu comme une perte UX (les combats Sprint 4 durent <60s, perte mineure)
- Sprint 5 Capitaines: ajustement possible (chrono long, peut justifier persistance)

---

## 2026-05-27 — Sprint 4: placeholder sprites procéduraux pour adversaires

**Décision**: Pour Sprint 4 MVP, les adversaires sont représentés par un sprite procédural blanc 60×100 px généré au runtime, tinté par voie (gris/ambre/bleu/vert/bronze/etc. per `AdversaireWorldView.ColorForVoie`).

**Raison**: Brief explicite "Pour les sprites: utilise placeholder solide pour MVP". Sprint 5+ remplacera par de vrais sprites (rvros pack a déjà des frames hurt/die/idle réutilisables pour les ennemis, ou import dédié).

**Conséquence**:
- `AdversaireData.SpriteIdleName` field présent mais ignoré par Sprint 4 (utilisable Sprint 5+ pour pointer vers une Resources lookup)
- 1 texture statique partagée entre toutes les instances (perf-friendly)
- Lisibilité immédiate par couleur de voie (matches l'UI palette per-voie de `05_VISUAL_STYLE.md`)

---

## 2026-05-27 — Sprint 3: art direction pivot vers "Simple Chibi - Pixel Adventurer"

**Décision** (tranchée par coordinateur after Ajwad's review of initial anatomical templates):
- Style officiel personnage : **pixel art chibi simple**, type `rvros Animated Adventurer`
- Pas sumi-e dark (écarté), pas Cookie Run cute (écarté). Style efficace par sa simplicité.
- Proportions ~1:2 (tête grosse, corps petit mais pas exagéré)
- Culturellement neutre par défaut, customisable par voie via overlays (Sprint 4+)
- Background dark non-noir : `#1a1a1a` avec accents chaleureux
- UI menus : **on garde la direction A "Lame & Encre"** minimaliste. La palette ambre/coral/text reste valide pour toute l'UI (cards, compteur, popups). Seule la zone "character + background" change de style.

**Tech artistique** :
- Unity 2D Animation built-in (Spine 2D écarté pour MVP)
- Pixel Perfect Camera dans la scene Main
- Sprites : Filter Point (no filter), PPU 32 (pour les sprites adventurer ~50x37), Compression None
- Pixels Per Unit cohérent à tester en play, ajustable

**Raison**: 
- Style chibi pixel = vibe nostalgique + lisible mobile + production-friendly (peu d'animations nécessaires pour communiquer)
- rvros adventurer = base solide, 70+ frames d'animation prêtes à l'emploi
- Unity 2D Animation built-in = pas de coût licence Spine ($69), workflow plus simple pour MVP

**Conséquence**:
- 05_VISUAL_STYLE.md sera amendé (section character art uniquement, UI reste intacte)
- Sprint 3 implémente Stade 2 (Apprenti) avec rvros adventurer. Stade 1 Mendiant et 3+ pour plus tard.
- Templates eris esra gardés en stock pour Sprint 6+ (customisation par voie / communauté)
- Décision "Spine vs Unity 2D Animation" du 06_TECH_STACK : tranchée = Unity 2D Animation

---

## 2026-05-27 — Sprint 2: Méditation = multiplicative combo bonus (× base tier)

**Décision**: La formule finale du combo multiplier au tap est :
`final_mult = base_tier_multiplier × (1 + Σ meditationLevel × 0.05)`

Exemples :
- Tier 3 (x2.0), Méditation level 0 → final = 2.0
- Tier 3 (x2.0), Méditation level 4 → final = 2.0 × 1.20 = 2.40
- Tier 1 (x1.2), Méditation level 4 → final = 1.2 × 1.20 = 1.44

**Raison**: 08_ROADMAP Sprint 2 dit "+5% combo multiplier per level" sans préciser additif au cap ou multiplicatif global. Multiplicatif "lifts" toute la courbe combo, ce qui :
- Fait sentir Méditation toujours (pas juste au cap)
- Évite que Méditation paraisse useless en early game quand on n'atteint pas tier 3
- Compose proprement avec Frappe (additif) — Méditation = "skill multiplier", Frappe = "raw force"

Alternatives écartées :
- Additif au cap : "+0.05 per level au tier max" → invisible avant tier 3, feel weak
- Multiplicatif sur final tier seulement : trop conditionnel
- Linéaire interpolation : trop subtil pour communiquer le power-up

**Conséquence**: 
- `StatsCalculator.GetComboMultiplierBonus()` returns float multiplier (1.0 baseline)
- TapHandler computes `tierMult × bonus` at each tap
- ComboMeterView displays the final value (not base tier) so player sees the buff

---

## 2026-05-27 — Sprint 2: ContentDatabase test seam (DI optional injection)

**Décision**: `ContentDatabase` accepts an optional `IEnumerable<UpgradeData>` in its constructor. Null → loads from Resources/Upgrades. Non-null → uses injected. Production GameManager passes nothing → Resources. EditMode tests pass an inline array of `UpgradeData.CreateForTests` instances → no asset roundtrip needed.

**Raison**: Unit-testing UpgradeService and StatsCalculator otherwise requires creating real SO assets in EditMode test setup, then loading via Resources, then cleanup. Heavy and slow. Inline DI is cleaner and matches the 07_ARCHITECTURE.md service mindset.

**Conséquence**: 
- Production path unchanged (GameManager calls `new ContentDatabase()` → loads from Resources)
- Tests use `new ContentDatabase(new[] { stub1, stub2 })` directly
- `UpgradeData.CreateForTests(...)` static factory under `#if UNITY_INCLUDE_TESTS` provides in-memory SO instances. Stripped from production builds.

---

## 2026-05-27 — Sprint 2: SO assets generated via Editor utility (menu item)

**Décision**: Les 3 upgrades de Sprint 2 (Frappe, Disciple, Méditation) sont générés par un Editor utility `Saga.EditorTools.UpgradeAssetsCreator` accessible via menu `Saga > Sprint 2 > Generate Upgrade Assets`. Ajwad clique une fois → 3 `.asset` SOs créés/updates dans `Assets/_Project/Resources/Upgrades/`.

**Raison**: Authorer manuellement les YAML `.asset` requiert le GUID du script `UpgradeData.cs` (généré par Unity au .meta), chicken-and-egg en filesystem-mode. L'Editor utility utilise l'API Unity native (`AssetDatabase.CreateAsset`, `SerializedObject`) qui résout les GUIDs automatiquement. Idempotent (re-run = update in-place).

**Conséquence**:
- Sprint 2 ships avec code prêt, Ajwad fait 1 clic menu pour activer les upgrades
- Pattern reproductible Sprint 3+ pour les voies, esprits, régions
- `Assets/_Project/Editor/Saga.Editor.asmdef` créé pour héberger ce genre d'outils
- Si MCP `manage_asset` stabilise plus tard, l'utility devient redondante (mais le menu reste un dev tool utile)

---

## 2026-05-27 — Sprint 1 strategy pivot: use only DOTween core shortcuts (DOLocalMove + DOTween.To), drop UI-module extension dependency

**Décision** (coordinator pivot après 2 itérations infructueuses sur l'asmdef DOTween.Modules) :
- `RectTransform.DOAnchorPos(Vector2, float)` → `Transform.DOLocalMove(Vector3, float)` (core shortcut, dans DOTween.dll)
- `Image.DOFade(...)`, `TextMeshProUGUI.DOFade(...)`, `CanvasGroup.DOFade(...)` → `DOTween.To(() => x.alpha, a => x.alpha = a, end, duration)` (générique core)

Refactor concret :
- `FloatingNumberView` : ajout d'un field `_group: CanvasGroup`, Play() utilise `DOLocalMove` + `DOTween.To` sur l'alpha du CanvasGroup
- `TapFxSpawner.SpawnFloatingNumber` : wire `view.Group = go.GetComponent<CanvasGroup>()` (le GameObject crée déjà via `typeof(CanvasGroup)`)
- `TapFxSpawner.SpawnSingleDust` : ajout `CanvasGroup` au GameObject Dust, tween via DOLocalMove + DOTween.To
- Anchors centrés (0.5, 0.5) sur tous les éléments tweenés → `anchoredPosition` ≈ `localPosition.xy`, l'animation est visuellement identique

**Raison**: Note technique importante — `CanvasGroup.DOFade` est en réalité défini dans `DOTweenModuleUI.cs` (pas dans le core), donc ne fonctionnerait pas non plus dans notre setup actuel. Le vrai pattern stable est `DOTween.To<float>(getter, setter, end, duration)` qui est dans le DOTween.dll précompilé core, **indépendant des modules**. C'est aussi exactement ce que les extensions du module UI font sous le capot.

Avantages du pivot :
- Zero dépendance à `DOTween.Modules.asmdef` (donc plus de fragilité asmdef resolution)
- Pattern reproductible : tous les futurs tweens passent par core shortcuts ou DOTween.To
- Compatible avec n'importe quel `IEnumerable<float>`-like property (alpha, anything)

**Conséquence**:
- `DOTween.Modules.asmdef` reste sur disque mais inutilisée par Saga.Runtime (laissée par décision coordinator pour de futurs usages potentiels — ex: `SpriteRenderer.DOFade` au Sprint 3 sur le perso animé)
- 4 errors → 0. 4 warnings Unity 6 obsolete API (`FindObjectOfType<T>` → `FindFirstObjectByType<T>`) fixés en passant.
- Convention équipe : préférer `DOTween.To` direct + Transform core shortcuts pour toute animation Sprint 1+. Si on a vraiment besoin des extensions UI au Sprint 3+, on rouvre le dossier asmdef à ce moment.

---

## 2026-05-27 — Sprint 1 fix: DOTween Modules wrapped in dedicated asmdef

**Décision**: Création de `Assets/Plugins/Demigiant/DOTween/Modules/DOTween.Modules.asmdef` qui couvre tous les `DOTweenModule*.cs` (UI, Sprite, Audio, Physics, Physics2D, UnityVersion, Utils — UIToolkit et EPOOutline restent guards off via `#if`). Précompiled ref `DOTween.dll`. Saga.Runtime.asmdef ajoute `"DOTween.Modules"` à ses references.

**Raison**: Sans cet asmdef, les `*.cs` modules vivent dans Assembly-CSharp default. Saga.Runtime.asmdef avec `overrideReferences: true` ne peut pas voir Assembly-CSharp (impossibilité fondamentale d'Unity à référencer Assembly-CSharp depuis un asmdef). Conséquence: `RectTransform.DOAnchorPos` et `Image.DOFade` (qui vivent dans `DOTweenModuleUI.cs`) étaient invisibles → CS1061/CS1929 au compile. Le fix isole proprement les modules dans leur propre assembly référençable.

**Conséquence**: 
- DOTween UI extensions désormais accessibles depuis tout asmdef qui ref `DOTween.Modules`
- Pattern reproductible si on importe d'autres libs tierces source-based (Spine, Lottie, etc.)
- L'erreur cascade "Failed to resolve assembly Saga.Tests.EditMode" se résorbe automatiquement (les tests dépendaient de Saga.Runtime qui ne compilait pas)
- Cleanup secondaire : retiré `defineConstraints: ["UNITY_INCLUDE_TESTS"]` du test asmdef (optionnel, élimine une variable pour isoler)

---

## 2026-05-27 — Sprint 1: TMP default font (LiberationSans SDF), custom fonts deferred

**Décision**: Sprint 1 utilise TMP_Settings.defaultFontAsset (LiberationSans SDF, livré avec TextMeshPro). Pas d'import Inter + JetBrains Mono ce sprint.

**Raison**: Custom font asset creation requires TMP Font Asset Creator (Editor UI), pas faisable en filesystem direct. Pour Sprint 1 critère succès (compteur monte avec juice, save/reload OK), TMP default suffit. Tradeoff connu : LiberationSans n'est pas tabular-aware, le ticker animation des chiffres aura un léger jitter (digit width variable). Acceptable pour validation gameplay, pas pour polish final.

**Conséquence**:
- Sprint 1 ships avec font generic. Visual polish à finir Sprint 2 (ou en fin Sprint 1 si Ajwad veut).
- Quand fonts sont importées (Inter Variable + JetBrains Mono Variable depuis Google Fonts), drop dans `Assets/_Project/Art/Fonts/`, ouvrir Window > TextMeshPro > Font Asset Creator, générer SDF (sampling 8192 x 8192, character set ASCII + accented), résultat dans `Assets/_Project/Art/Fonts/Generated/`.
- Update `ForceCounterView._label.font` + `ComboMeterView._label.font` references via Inspector quand fonts dispo.

---

## 2026-05-27 — Sprint 1: scene Main bootstrappée par runtime script (pas YAML-authored)

**Décision**: Le contenu visuel de Main.unity (Canvas, ForceCounter TMP, ComboMeter TMP, background dark) est créé à runtime par `MainSceneBootstrap.Start()`. La scene Main.unity reste à son état de clone vide.

**Raison**: Bridge MCP down empêche l'utilisation de manage_scene pour authorer la scène. Éditer le YAML .unity manuellement pour ajouter une hiérarchie Canvas+TMP+RectTransform avec les fileIDs corrects = fragile et long. Bootstrap programmatique = même résultat fonctionnel, idempotent, et plus testable.

**Conséquence**: 
- `MainSceneBootstrap.cs` attaché au GameObject par défaut de Main.unity (à wire au refocus Unity, OR via auto-creation par GameManager si scene active = Main).
- Refactor vers scene-authored UI au Sprint 2 (idéalement quand MCP est back, ou quand Ajwad ouvre Unity et drag les Views dans la scene).
- Pattern Sprint 1 = défensif et autonome ; pattern Sprint 2+ = scene-driven proper.

---

## 2026-05-27 — Sprint 1: combo en paliers discrets

**Décision**: Système de combo en 4 tiers discrets:
- Tap count 0-2 → x1.0 (warmup, no bonus)
- Tap count 3-5 → x1.2
- Tap count 6-9 → x1.5
- Tap count 10+ → x2.0 (cap, plafond)

Window: 1.5s entre 2 taps consécutifs. Si dépassé, reset à 0.

**Raison**: 02_GAME_DESIGN.md ligne 111 énumère 4 paliers nommés (x1.0 → x1.2 → x1.5 → x2.0). 08_ROADMAP Sprint 1 dit "à 10 taps consécutifs". Discret est plus punchy qu'un lerp continu, et le "near-miss" entre x1.5 (tap 9) et x2.0 (tap 10) incite à tap rapidement. Implémentation cleaner aussi (lookup table). Ajwad/coordinateur peuvent reswap au lerp en changeant la classe ComboSystem si besoin.

**Conséquence**: `ComboSystem.cs` implémenté avec lookup table `_tiers`. Multiplier exposé en float, tier en int 0..3.

---

## 2026-05-27 — Sprint 0: Localization tables setup manuel par Ajwad

**Décision**: Le package `com.unity.localization` est installé et résolu (✅), mais la création des tables `UI_Common` (FR + EN) sera faite manuellement par Ajwad via l'Editor UI quand il rouvrira Unity. Étapes:
1. Window > Asset Management > Localization Tables > Create > String Table Collection
2. Nom: `UI_Common`, locales: FR + EN, location: `Assets/_Project/Data/Localization/`
3. Window > Asset Management > Localization Settings > Available Locales: ajouter Locale French (fr) + Locale English (en)

**Raison**: Écrire à la main les assets YAML du Localization package (LocalizationSettings.asset, Locale_fr.asset, Locale_en.asset, StringTableCollection, StringTable) est risqué — format propriétaire, fileIDs spécifiques. L'Editor UI fait ça en 30 secondes. Pas bloquant pour le critère de succès Sprint 0 (app boot + GameManager OK).

**Conséquence**: 
- Sprint 0 marqué "Localization-pending" jusqu'à l'étape manuelle.
- Ajout du folder `Assets/_Project/Data/Localization/` à créer au moment du setup.

---

## 2026-05-27 — Sprint 0: Switch Platform Android manuel par Ajwad

**Décision**: Le switch de la plateforme cible vers Android ne peut pas se faire en filesystem direct (Unity stocke l'état dans `Library/EditorUserBuildSettings.asset` qui est gitignored et créé lazily par le Build Settings window). Ajwad fait le switch via File > Build Settings > Android > Switch Platform au prochain focus Unity (30 sec, Android Build Support déjà installé).

**Raison**: Le bridge MCP était down pendant Sprint 0, et même via MCP `manage_editor` n'expose pas de "switch active build target". Le switch déclenche aussi une recompile pour le platform Android — c'est une action Unity native qu'on ne peut pas reproduire en filesystem proprement.

**Conséquence**: ProjectSettings est correctement configuré pour Android (bundle id `com.hiddenlab.saga`, ARM64 + ARMv7, IL2CPP backend), il manque juste le switch effectif.

---

## 2026-05-27 — Sprint 0: package versions Unity 6 pinées dans manifest.json

**Décision**: Versions installées pour Unity 6.3 (6000.3.9f1):
- Addressables 2.6.0
- Cinemachine 3.1.4 (CM3 pour Unity 6)
- Localization 1.5.5
- Mobile Notifications 2.4.1
- Newtonsoft Json 3.2.1

**Raison**: Fallback filesystem direct sur `Packages/manifest.json` car le bridge MCP a perdu la connexion pendant la session. Versions choisies = stable et connues compatibles Unity 6.

**Conséquence**: Unity re-resolve auto au refocus. Si une version pose problème, ajustement trivial dans le manifest. À vérifier au checkpoint compile.

---

## Template pour nouvelles entrées

```
## YYYY-MM-DD — [Titre court]

**Décision**: ...

**Raison**: ...

**Conséquence**: ...
```
