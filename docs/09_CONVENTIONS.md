# 09 - CONVENTIONS

## Folder structure

Voir 06_TECH_STACK.md pour l'arbo complète. Règles clés:

- Tout le custom dans `Assets/_Project/` (préfixe `_` pour qu'il remonte en haut)
- Plugins tiers dans `Assets/Plugins/`
- Scripts par domaine (Core, Gameplay, UI, Data, Save, Math, Audio)
- ScriptableObjects par type (Voies, Esprits, Upgrades, Regions, Lore)
- Pas de fichier orphelin à la racine de `_Project/Scripts/`

## Naming

### Fichiers de script
- `PascalCase.cs`
- Un fichier = une classe publique (exceptions: classes nested ou interfaces dans le même fichier OK si <100 lignes)

### Classes
- `PascalCase`
- Suffix selon le rôle:
  - `*Manager` pour les MonoBehaviour singletons (rare, juste GameManager)
  - `*Service` pour les services POCO (SaveService, AudioService)
  - `*Controller` pour orchestration UI (UpgradePanelController)
  - `*View` pour l'affichage (ForceCounterView, UpgradeCardView)
  - `*Data` pour les ScriptableObject de données (VoieData, EspritData)
  - `*Processor` pour les calculateurs / tickers (DisciplesProcessor)

### Méthodes
- `PascalCase` pour public, protected, internal
- `PascalCase` aussi pour private (style Microsoft, plus lisible)
- Verbes d'action: `Compute`, `Process`, `Apply`, `Raise`, `Refresh`

### Fields
- Privés: `camelCase` (pas de `_` prefix sauf si serialisé)
- Privés sérialisés `[SerializeField]`: `_camelCase`
- Publics: éviter (utiliser properties)
- Constants: `UPPER_SNAKE_CASE` (`MAX_OFFLINE_HOURS = 8`)
- Static readonly: `PascalCase`

### IDs (string keys)
- Snake case lowercase: `samurai`, `wuxia`, `mortal_strike`, `crow_spirit`
- Pas d'espaces, pas de majuscules
- Centralisés dans des constants si possible (`VoieIds.Samurai`, `EspritIds.Crow`)

### Assets (sprites, prefabs, etc.)
- Prefabs: `pf_*` (`pf_TapZone`, `pf_UpgradeCard`)
- ScriptableObjects: catégorie en préfixe (`Voie_Samurai`, `Esprit_Corbeau`, `Upgrade_Frappe`)
- Sprites: `spr_*` (`spr_Character_Stade1`, `spr_Mannequin_Stade2`)
- Animations: `anim_*` (`anim_Character_Idle`, `anim_Character_TapReact`)
- Materials: `mat_*`
- Scenes: `PascalCase.unity` (`Main.unity`, `Map.unity`)

## C# style

### Brackets
Allman (Microsoft) style:
```csharp
public void Foo()
{
    if (condition)
    {
        DoThing();
    }
}
```

Single-line if acceptable pour les guards:
```csharp
if (state == null) return;
```

### Properties vs fields
Properties pour tout ce qui est exposé:
```csharp
public int Level { get; private set; }
public BigDouble Force { get; private set; }
```

### Nullability
Préfixer les paramètres et returns nullable avec `?` quand C# 8+ activé:
```csharp
public VoieData? GetVoieById(string id) { ... }
```

### Async/await
Préférer à coroutines pour la nouvelle logique:
```csharp
public async Task<SaveData> LoadAsync()
{
    var json = await File.ReadAllTextAsync(path);
    return JsonConvert.DeserializeObject<SaveData>(json);
}
```

### Using statements
- Ordre: System.* en haut, puis third-party (BreakInfinity, DG.Tweening), puis Unity.*, puis le projet
- Pas de `using` non utilisé (Rider/VS le détectera)

### Comments
- XML doc sur les API publiques (résumé, params, returns)
- `// TODO:` pour les choses à finir, avec contexte
- `// HACK:` si on fait quelque chose qui devra être refactor (avec raison)
- Pas de commentaires triviaux (`// increment i` non)
- Commentaires en anglais (le code est en anglais)
- Documentation Markdown reste en français

### Logging
- `Debug.Log` pour info dev
- `Debug.LogWarning` pour cas inattendu non bloquant
- `Debug.LogError` pour erreur réelle
- En release: stripper les Debug.Log via `[Conditional("UNITY_EDITOR")]` ou ifdef

## ScriptableObject conventions

### Création
Toujours via `[CreateAssetMenu]` avec un menu structuré:
```csharp
[CreateAssetMenu(fileName = "Voie_New", menuName = "Voie/Voie Data")]
public class VoieData : ScriptableObject { ... }
```

### Champs
- Tous `[SerializeField]` privés avec property getters publics si exposé
- Pour les listes, préférer `IReadOnlyList<T>` en property publique pour empêcher modif externe

### Validation
- Implémenter `OnValidate()` pour vérifier la cohérence à la save dans l'éditeur:
```csharp
private void OnValidate()
{
    if (string.IsNullOrEmpty(voieId)) Debug.LogError($"VoieData {name} has no voieId");
    if (skinsByStade.Length < 4) Debug.LogError($"VoieData {name} missing skins (need 4 minimum)");
}
```

## UI conventions

### Hiérarchie des Canvas
- Un Canvas par "screen" (Main, Map, Prestige, Settings, Modals)
- Sub-canvas pour les éléments qui changent souvent (compteurs, FX) - optimisation des dirty rects

### Anchors
- Toujours configurer les anchors selon la zone (top-left, top-right, etc.) pour responsive
- Tester sur différents aspect ratios (16:9, 19.5:9, 20:9, iPad)

### Tap targets
- **Minimum 48dp = ~48px** sur device standard
- Padding invisible OK pour augmenter la hit area sans changer le visuel

### Couleurs
- **Toujours via constants ou ScriptableObject de palette**, jamais hardcoded dans le View
- Voir 05_VISUAL_STYLE.md pour la palette officielle

## Audio conventions

### Naming
- `sfx_*` pour sound effects (one-shots)
- `music_*` pour musiques de fond
- `amb_*` pour ambient loops
- `vox_*` pour voix (si on en ajoute plus tard)

### Format
- SFX: WAV ou OGG, 44.1kHz, mono pour les courts (économise sur la taille)
- Music/Ambient: OGG, 44.1kHz, stereo
- Import settings Unity: Compressed in Memory pour SFX, Streaming pour music

### Volume
- Pas de clip avant +0dB (utiliser Audacity / Reaper pour vérifier)
- Normaliser les SFX entre eux pour cohérence

## Localization

### Tables
- Une table par domaine: `UI_Common`, `UI_Voies`, `Lore_Samurai`, `Lore_Wuxia`, etc.
- Pas de string > 100 chars dans une table (le splitter en plusieurs entrées)

### Keys
- Format: `domain.section.key`
- Exemple: `ui.main.upgrade_button`, `lore.samurai.fragment_001`

### Code
- Jamais de string hardcodée dans le View
- Toujours via `LocalizedString` field ou `LocalizationSettings.StringDatabase.GetLocalizedString(...)`

## Git workflow

### Branches
- `main` - stable
- `dev` - daily work
- `feat/sprint-N-description` - feature
- `fix/issue-description` - bug fix

### Commits
Conventional Commits, voir 06_TECH_STACK.md:
- `feat(scope): description`
- `fix(scope): description`
- `chore: description`
- `docs: description`
- `art: description`
- `refactor(scope): description`

### Pull / Merge
- Pas de force-push sur `main` ou `dev`
- Squash de la feat branch en mergeant vers `dev`
- Tag de version sur `main` au format `vX.Y.Z` (X=majeur, Y=mineur, Z=patch)

## Quality gates

### Avant de commit
- Le projet compile sans warning
- Pas de TODO en gros sur du code committé (les flagger)
- Tests unitaires passent si on en a écrit pour le module

### Avant de pusher sur `dev`
- Le jeu démarre et le core loop fonctionne
- Pas de regression visible sur les features précédentes

### Avant de merger sur `main`
- Tag de version
- CURRENT_STATUS.md mis à jour
- Build test sur device

## Outils recommandés (côté dev)

- **Rider** ou **Visual Studio 2022** (Rider préféré pour Unity)
- **Git client**: ligne de commande ou GitKraken
- **Audacity** pour edit audio léger
- **Aseprite** ou **Photoshop** pour les sprites
- **Spine** si on passe sur Spine 2D
- **TextMeshPro Font Asset Creator** pour les polices
