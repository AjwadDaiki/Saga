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
