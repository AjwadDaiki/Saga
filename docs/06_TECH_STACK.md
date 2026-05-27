# 06 - TECH STACK

## Moteur

**Unity 6 LTS** (latest LTS au moment du démarrage). Vérifier `Unity Hub` pour la version courante.

Render pipeline: **URP** (Universal Render Pipeline) avec **2D Renderer + 2D Lights**. Performance mobile + lighting dynamique pour le perso.

Target platforms: **iOS** + **Android**. Build IL2CPP en release, Mono en dev pour itération rapide.

Architecture support: ARM64 obligatoire (iOS), ARMv7 + ARM64 (Android).

## Packages Unity (Package Manager)

À ajouter via Package Manager au setup initial:

- **2D Animation** (com.unity.2d.animation) - skeletal animation 2D, alternative gratuite à Spine
- **2D Sprite** (com.unity.2d.sprite)
- **2D PSD Importer** (com.unity.2d.psdimporter) - pour les rigs multi-couches
- **Universal RP** (com.unity.render-pipelines.universal)
- **TextMeshPro** (com.unity.textmeshpro) - **obligatoire** pour les chiffres énormes stylés
- **Addressables** (com.unity.addressables) - load des assets de cultures par paliers
- **Input System** (com.unity.inputsystem) - new input system, mieux que legacy
- **Cinemachine** (com.unity.cinemachine) - camera shake et effets
- **Mobile Notifications** (com.unity.mobile.notifications) - notifications mobile (avec parcimonie, voir GAME_DESIGN.md)
- **Localization** (com.unity.localization) - i18n, on commencera en FR/EN

## Packages tiers (GitHub / Asset Store)

### Obligatoires

1. **BreakInfinity.cs** (Razenpok, MIT, GitHub)
   - URL: https://github.com/Razenpok/BreakInfinity.cs
   - Install: dropper `BigDouble.cs` dans `Assets/Plugins/BreakInfinity/`
   - **Critique** pour les gros nombres au-delà de 1e308 (sinon le jeu cape à 3.4e38)
   - Usage: `using BreakInfinity;` puis `BigDouble myForce = 1000;`

2. **DOTween Pro** (Asset Store, 15$)
   - URL: https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416
   - Critique pour toute la juice UI/animation
   - Importer après setup de base
   - DOTween free aussi acceptable au début, on upgrade en Pro pour le visual scripting et path

### Recommandés (post-MVP)

3. **Odin Inspector** (Asset Store, 50-80$ selon promos)
   - Énorme gain de productivité éditeur
   - Pas critique pour le MVP, à acheter en promo seasonal

4. **Spine 2D Essential** (Esoteric Software, 69$)
   - URL: http://esotericsoftware.com/spine-purchase
   - Skeletal animation pro pour le perso
   - Alternative: rester sur Unity 2D Animation built-in (gratuit mais moins puissant)
   - Décision à prendre quand le rig devient complexe

### Optionnels

5. **Lottie for Unity** (gratuit via Package Manager)
   - Pour animations UI vectorielles importées d'After Effects
   - Useful pour les icônes animées

6. **FMOD for Unity** (gratuit pour indie, sous certaines conditions de revenus)
   - URL: https://www.fmod.com/unity
   - Sound design pro avec mixing dynamique
   - Alternative: Unity Audio built-in suffisant pour MVP

## Architecture libs (références, pas à installer)

### uClicker (philipcass, GitHub)
- URL: https://github.com/philipcass/uClicker
- **Pas à utiliser direct**, mais à lire pour comprendre le pattern ScriptableObject-driven pour idle games
- On adapte ses patterns à notre archi (voir 07_ARCHITECTURE.md)

## MCP et IA dev

### Coplay Unity MCP (CoplayDev/unity-mcp)
- URL: https://github.com/CoplayDev/unity-mcp
- Version actuelle: 9.7.0+ (mai 2026)
- **Déjà installé sur la machine d'Ajwad**, utilisé pour WORLDSIM
- Setup: Unity → Window → MCP for Unity → Configure All Detected Clients
- Vérifier que Claude Desktop est listé dans les clients
- Le bridge doit être Started avant les sessions

### Skill / référence pour le dev Claude
- URL: https://lobehub.com/mcp/coplaydev-unity-mcp - reference guide pour comprendre les tools MCP Unity
- À lire en début de session si le dev Claude n'est pas familier avec les tools Coplay

## Structure de projet Unity recommandée

```
Saga/                              <- racine du projet Unity
├── Assets/
│   ├── _Project/                  <- tout ce qui est custom au projet (préfixé _ pour qu'il soit en haut)
│   │   ├── Art/
│   │   │   ├── Characters/
│   │   │   ├── Backgrounds/
│   │   │   ├── UI/
│   │   │   ├── FX/
│   │   │   └── Fonts/
│   │   ├── Audio/
│   │   │   ├── SFX/
│   │   │   ├── Music/
│   │   │   └── Ambience/
│   │   ├── Data/                  <- ScriptableObjects
│   │   │   ├── Voies/             <- VoieData SO
│   │   │   ├── Esprits/           <- EspritData SO
│   │   │   ├── Upgrades/          <- UpgradeData SO
│   │   │   ├── Regions/           <- RegionData SO
│   │   │   └── Lore/              <- LoreFragmentData SO
│   │   ├── Prefabs/
│   │   │   ├── UI/
│   │   │   ├── Characters/
│   │   │   ├── FX/
│   │   │   └── Game/
│   │   ├── Scenes/
│   │   │   ├── Boot.unity         <- scène d'entrée
│   │   │   ├── Main.unity         <- gameplay principal
│   │   │   ├── Map.unity          <- carte du monde
│   │   │   └── Prestige.unity     <- cinématique prestige
│   │   ├── Scripts/
│   │   │   ├── Core/              <- managers, singletons, game state
│   │   │   ├── Gameplay/          <- mécaniques (tap, upgrades, combat)
│   │   │   ├── UI/                <- vues, controllers UI
│   │   │   ├── Data/              <- ScriptableObject definitions
│   │   │   ├── Save/              <- système de sauvegarde
│   │   │   ├── Math/              <- helpers, formatters
│   │   │   └── Audio/             <- audio managers
│   │   └── Shaders/
│   ├── Plugins/                   <- libs tierces
│   │   ├── BreakInfinity/
│   │   ├── DOTween/
│   │   └── ...
│   └── Settings/                  <- URP, project settings
├── Packages/
│   └── manifest.json
├── ProjectSettings/
├── docs/                          <- ce dossier de documentation
│   ├── 00_README.md
│   ├── 01_VISION.md
│   ├── ...
│   └── 11_CURRENT_STATUS.md
└── README.md                      <- README du repo Git
```

## Versioning et Git

### Configuration .gitignore
Utiliser le `.gitignore` officiel Unity (https://github.com/github/gitignore/blob/main/Unity.gitignore). Ne PAS committer:
- `Library/`
- `Temp/`
- `Logs/`
- `obj/` et `Build/`
- `*.csproj`, `*.sln` (générés)

### Git LFS
**Obligatoire** pour les assets binaires lourds. Configurer dès le départ:
```bash
git lfs install
git lfs track "*.psd" "*.png" "*.jpg" "*.wav" "*.mp3" "*.ogg" "*.fbx" "*.anim" "*.unity" "*.prefab" "*.asset"
```

### Convention de commits
Style **Conventional Commits**:
- `feat:` nouvelle feature
- `fix:` correction de bug
- `chore:` maintenance, setup
- `refactor:` refactor sans changement de comportement
- `docs:` documentation
- `art:` ajout/update d'asset
- `wip:` work in progress (à squash avant merge)

Exemple: `feat(voie-samurai): implement Mortal Strike passive`

### Branches
- `main` - stable, release-ready
- `dev` - intégration, daily work
- `feat/*` - features individuelles
- `fix/*` - corrections

## Build et déploiement

### Profils de build
- **Development** - Mono, debug logs, profiler attached, build interne
- **Release Beta** - IL2CPP, logs minimes, build pour TestFlight / Internal Testing
- **Release Public** - IL2CPP, logs off, optimisations max

### CI/CD (post-MVP)
- GitHub Actions ou Unity Cloud Build pour les builds nocturnes
- Distribution beta via TestFlight (iOS) et Internal Testing (Google Play Console)

### Estimation de taille
- MVP visé: <80MB download (sans assets bundle dynamiques)
- Post-MVP avec toutes les voies: <150MB

## Performance targets

### Mobile
- 60fps stable sur iPhone 12+ et Android Snapdragon 870+
- 30fps acceptable sur iPhone X et Android Snapdragon 730
- Battery drain: <8%/h en gameplay actif

### Loading
- App start to playable: <3s sur device mid-range
- Scene transitions: <500ms perçues (avec ink wash transition pour masquer)

### Memory
- <300MB RAM en gameplay normal
- <500MB lors des cinématiques de prestige
