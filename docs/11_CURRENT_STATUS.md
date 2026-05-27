# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 0 - Setup (95% — 2 items "Ajwad-pending" UI Unity)

**Dernière session**: 2026-05-27, durée ~1h, dev Claude (Opus 4.7) sur Claude Code

## Sprint 0 — ce qui est fait ✅

### Repo
- Rename du projet `VOIE` → `SAGA` dans tous les docs/*.md (terme game-design `voies` préservé)
- `00_README (1).md` normalisé en `00_README.md`
- `git init -b main` à la racine `C:\Users\daiki\Saga`
- `.gitignore` Unity officiel + `.claude/` exclu
- `.gitattributes` avec Git LFS tracking pour les binaires lourds (psd, png, jpg, wav, mp3, ogg, fbx, ttf, mp4, zip, dll, etc.) et merge=unityyamlmerge pour les YAML Unity
- `git lfs install` OK (git 2.52, lfs 3.7)

### Folder structure
- `Assets/_Project/{Art, Audio, Data, Prefabs, Scenes, Scripts, Shaders}` créés conformément à 06_TECH_STACK.md
- 28 sous-folders avec `.gitkeep` pour préserver l'arbo vide en git
- `Assets/Plugins/BreakInfinity/` créé pour la lib BreakInfinity

### Packages Unity (ajoutés dans `Packages/manifest.json`, résolus par Unity au refocus)
- `com.unity.addressables` 2.6.0
- `com.unity.cinemachine` 3.1.4 (CM3 pour Unity 6)
- `com.unity.localization` 1.5.5
- `com.unity.mobile.notifications` 2.4.1
- `com.unity.nuget.newtonsoft-json` 3.2.1

Déjà présents via template URP 2D (vérifiés) : 2D Animation, 2D Sprite, 2D PSD Importer, 2D Aseprite, URP 17.3, Input System 1.18, TextMeshPro (via ugui 2.0), Test Framework, IDE Rider/VS.

### Plugins tiers
- BreakInfinity.cs téléchargé depuis GitHub (Razenpok/BreakInfinity.cs master)
  - `Assets/Plugins/BreakInfinity/BigDouble.cs` (runtime, 42KB)
  - `Assets/Plugins/BreakInfinity/Editor/BigDoubleEditor.cs` (custom inspector, 2KB, dans `Editor/` pour ne pas casser le build runtime)
- DOTween free importé manuellement par Ajwad via Asset Store (location `Assets/Plugins/Demigiant/DOTween/` — `DOTween.dll` + `DOTweenEditor.dll` géreées par Git LFS via pattern `*.dll`)

### ProjectSettings
- `companyName: HiddenLab` (était `DefaultCompany`)
- `productName: Saga`
- `applicationIdentifier` unifié sur `com.hiddenlab.saga` pour Android + Standalone + iPhone
- `AndroidTargetArchitectures: 3` (ARM64 + ARMv7, bitflag 1+2)
- `scriptingBackend.Android: 1` (IL2CPP — déjà OK du template)

### Scenes
- 4 scenes créées dans `Assets/_Project/Scenes/` (clonées depuis la SampleScene du template URP 2D, GUIDs frais générés) :
  - `Boot.unity` (guid 6cb89f73...)
  - `Main.unity` (guid ebf273de...)
  - `Map.unity` (guid 692ab006...)
  - `Prestige.unity` (guid bd954e3b...)
- `EditorBuildSettings.asset` patché : 4 scenes enabled dans cet ordre (Boot=0, Main=1, Map=2, Prestige=3)
- SampleScene supprimée (`Assets/Scenes/` retirée)

### Code skeletons
- `Assets/_Project/Scripts/Core/GameManager.cs`
  - Singleton MonoBehaviour avec `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` qui s'auto-instantie (pas besoin de wire dans Boot.unity manuellement)
  - Log `"GameManager OK"` au Awake → satisfait le critère de succès Sprint 0
- `Assets/_Project/Scripts/Save/SaveService.cs`
  - POCO service async load/save via Newtonsoft.Json
  - `SaveData` placeholder avec `SaveVersion = 1` pour la future migration

## Sprint 0 — Ajwad-pending ⏳ (manipulations Unity UI)

2 items qui ne peuvent pas se faire en filesystem direct, à faire par Ajwad au prochain focus Unity. Tous logués dans `DESIGN_DECISIONS_LOG.md`.

1. **Switch Platform Android** : `File > Build Settings > Android > Switch Platform`. ProjectSettings sont déjà configurés pour Android, il manque juste le switch effectif (recompile pour Android).
2. **Localization tables UI_Common** : `Window > Asset Management > Localization Tables > Create > String Table Collection`, nom `UI_Common`, locales FR + EN, location `Assets/_Project/Data/Localization/`.

## Sprint 0 — critère de succès

> "app démarre sur device, écran noir, log 'GameManager OK', build moins de 80MB"

- Le log "GameManager OK" est garanti par le RuntimeInitializeOnLoadMethod
- L'écran noir est OK (Boot.unity est minimaliste, juste un Camera)
- Le build mobile <80MB est skip cette session (décision Ajwad : on valide le build réel quand on aura du gameplay réel à tester, fin Sprint 2 ou 3). Pour Sprint 0 on s'arrête à "le projet compile pour Android" → ce qui sera validable au Switch Platform.

## Questions ouvertes pour le coordinateur

Aucune pour l'instant. Toutes les décisions Sprint 0 sont dans le scope des docs ou loguées en interne.

## Décisions à valider en cours de route

- **Spine 2D vs Unity 2D Animation** : à trancher au Sprint 3
- **FMOD vs Unity Audio** : à trancher au Sprint 4
- **Online sync de save** : à trancher post-MVP

## Métriques de projet

- **Sprint actuel** : 0/11 (95%)
- **Lignes de code C#** : ~50 (GameManager + SaveService) + 42K de BreakInfinity (lib tierce)
- **ScriptableObjects créés** : 0
- **Assets art** : 0
- **Voies implémentées** : 0/8
- **Voies dans le MVP target** : Samurai, Wuxia, Spartiate, Viking

## Prochaine session (Sprint 1)

**Objectif** : Core tap loop — le joueur tap, voit Force monter avec juice.

À faire (cf 08_ROADMAP.md Sprint 1) :
- Implémenter `GameState` avec champ Force (BigDouble)
- Implémenter `NumberFormatter` avec tests unitaires (K, M, B, T, aa, bb...)
- Setup scene Main : background flat dark, zone de tap centrale, compteur Force TextMeshPro
- Tap detection (Input System) + animation +1 floating + ticker compteur
- Particules tap (poussière)
- Système de combo (1.5s window, x1 → x2 sur 10 taps consécutifs)
- Save auto throttlée à 1/sec max
- Son tap placeholder

**Pré-requis avant Sprint 1** : Ajwad finit les 2 items Ajwad-pending ci-dessus (Switch Platform + Localization tables).

## Notes libres

- DOTween free importé dans `Assets/Plugins/Demigiant/DOTween/` (DOTween crée son propre subfolder `Demigiant/` sous `Plugins/` — c'est sous `Plugins/` comme le doc 06 le préconise, juste avec un niveau de plus). Pas critique.
- Le bridge Coplay MCP a déconnecté à plusieurs reprises pendant la session (bug connu beta 9.7.2-beta.8). Fallback filesystem direct a permis de tout faire sauf 3 items qui nécessitent vraiment l'Editor (Switch Platform + Localization tables).
- Modules Unity activés au moment de l'import DOTween (Audio, Physics, Physics2D, Sprites, UI, UI Toolkit). Présent pour info, pas critique.
