# 07 - ARCHITECTURE

## Philosophie générale

- **Data-driven** via ScriptableObjects: tout ce qui est "contenu" (voies, esprits, upgrades, régions, lore) est un asset SO, pas du code.
- **Composition over inheritance** pour les comportements: pattern Strategy pour les passifs de voie.
- **Event-driven** pour la communication entre systèmes: pas de couplage direct UI-Gameplay.
- **Single source of truth** pour le state: un `GameState` central, persisté via save system.
- **Modularité**: chaque "système" (Tap, Upgrades, Esprits, Map, Prestige) doit pouvoir être désactivé/testé en isolation.

## Patterns à utiliser

### 1. ScriptableObject-driven content

Chaque type de "contenu" a son SO:

```csharp
[CreateAssetMenu(fileName = "Voie_Samurai", menuName = "Voie/Voie Data")]
public class VoieData : ScriptableObject {
    public string voieId;          // "samurai"
    public string displayName;     // "Samurai"
    public LocalizedString lore;
    public Color accentColor;
    public Sprite icon;
    public AudioClip ambientLoop;
    public AudioClip tapSound;
    
    public CharacterSkin[] skinsByStade;  // index 0 = stade 2 apprenti
    
    [SerializeReference]
    public IVoiePassive passive;   // strategy injectée
    
    public LoreFragmentData[] loreFragments;
}
```

Avantages:
- Game designer (Ajwad ou le coordinateur) peut créer/modifier du contenu sans code
- Données chargeables via Addressables
- Reflection / Editor scripting facile

### 2. Strategy pattern pour les passifs de voie

```csharp
public interface IVoiePassive {
    string PassiveName { get; }
    string DescriptionFr { get; }
    void OnTap(GameState state, ref TapResult result);
    void OnTick(GameState state, float deltaTime);
    void OnPrestige(GameState state);
}

[Serializable]
public class SamuraiPassive : IVoiePassive {
    public string PassiveName => "Coup Mortel";
    public string DescriptionFr => "Chaque 10e tap inflige x50.";
    
    private int tapCount;
    
    public void OnTap(GameState state, ref TapResult result) {
        tapCount++;
        if (tapCount % 10 == 0) {
            result.DamageMultiplier *= 50;
            result.IsCritical = true;
        }
    }
    // ... etc
}
```

Sérialisé via `[SerializeReference]` dans VoieData pour pouvoir picker la classe dans l'editor.

### 3. Event system (channels SO)

Pour communication UI ↔ Gameplay sans couplage:

```csharp
[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEventSO : ScriptableObject {
    private List<GameEventListener> listeners = new();
    public void Raise() { foreach (var l in listeners) l.OnEventRaised(); }
    public void Register(GameEventListener l) { listeners.Add(l); }
    public void Unregister(GameEventListener l) { listeners.Remove(l); }
}
```

Avec variants pour payloads typés (`IntEventSO`, `BigDoubleEventSO`, etc.).

### 4. Service Locator / Single GameManager

Un seul `GameManager` singleton (acceptable ici, c'est un jeu solo offline-first). Il expose:

```csharp
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    
    public GameState State { get; private set; }
    public SaveService Save { get; private set; }
    public AudioService Audio { get; private set; }
    public ContentDatabase Content { get; private set; }
    public ProgressionService Progression { get; private set; }
    
    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitServices();
        Save.Load();
    }
}
```

## Le GameState

Source unique de vérité du jeu. Sérialisable.

```csharp
[Serializable]
public class GameState {
    // Currencies (BigDouble pour les nombres énormes)
    public BigDouble force;
    public BigDouble technique;
    public BigDouble renom;
    public BigDouble echos;  // persistante entre prestiges
    
    // Tier visuel
    public int currentStade;  // 1 à 6
    
    // Voie active
    public string activeVoieId;
    public string secondaryVoieId;  // null si pas de hybrid (prestige < 2)
    
    // Disciples
    public DiscipleState disciples;
    
    // Esprits équipés (3 slots)
    public string[] equippedEsprits = new string[3];
    public List<string> unlockedEsprits = new();
    
    // Upgrades possédés
    public Dictionary<string, int> upgradeLevels = new();
    
    // Inventaire
    public List<EquipmentInstance> inventory = new();
    public EquipmentLoadout equipped;
    
    // Carte
    public List<string> conqueredRegions = new();
    
    // Prestige
    public int prestigeLevel;
    public List<string> mastedVoieIds = new();
    public List<string> hiddenVoieIds = new();
    
    // Stats globales
    public int totalTaps;
    public int totalPrestiges;
    public DateTime gameStarted;
    public DateTime lastSession;
    
    // Lore
    public List<string> readLoreFragmentIds = new();
    
    // Settings utilisateur
    public PlayerPrefs settings;
}
```

## Save system

### Stratégie

- **JSON-based**, lisible humain pour debug
- **Sauvegarde locale** dans `Application.persistentDataPath/savegame.json`
- **Backup automatique** rotation 3 fichiers (en cas de corruption)
- **Save fréquente**: après chaque action majeure (upgrade, prestige, conquête) + auto-save toutes les 30s

### Implémentation

```csharp
public class SaveService {
    private const string SAVE_FILE = "savegame.json";
    private const string BACKUP_PREFIX = "savegame_backup_";
    
    public void Save(GameState state) {
        var json = JsonConvert.SerializeObject(state, Formatting.Indented);
        var path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        
        // Rotation backups
        RotateBackups(path);
        
        File.WriteAllText(path, json);
    }
    
    public GameState Load() {
        var path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        if (!File.Exists(path)) return new GameState();
        
        try {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<GameState>(json);
        } catch (Exception e) {
            Debug.LogError($"Save corrupted, trying backup: {e.Message}");
            return LoadBackup();
        }
    }
}
```

Use **Newtonsoft.Json for Unity** (package gratuit, via Package Manager).

### Migration de schema
Versionner le GameState (`int saveVersion`) et avoir un système de migration entre versions pour éviter de casser les saves des joueurs après update.

## Le tick loop (incremental engine)

### Frequency
- Tick principal: **10 Hz** (toutes les 100ms) - assez pour la fluidité, pas trop pour la batterie
- Decoupled du framerate

### Processus
```csharp
public class GameTicker : MonoBehaviour {
    private const float TICK_INTERVAL = 0.1f;
    private float accumulator;
    
    private void Update() {
        accumulator += Time.deltaTime;
        while (accumulator >= TICK_INTERVAL) {
            DoTick(TICK_INTERVAL);
            accumulator -= TICK_INTERVAL;
        }
    }
    
    private void DoTick(float dt) {
        var state = GameManager.Instance.State;
        
        // 1. Disciples auto-tap
        DisciplesProcessor.Tick(state, dt);
        
        // 2. Esprits passive effects
        EspritsProcessor.Tick(state, dt);
        
        // 3. Voie passive
        var voie = ContentDatabase.GetVoie(state.activeVoieId);
        voie.passive?.OnTick(state, dt);
        
        // 4. Stat updates
        StatsCalculator.Recompute(state);
        
        // 5. Event raises (UI updates)
        Events.OnTick.Raise(state);
    }
}
```

### Offline progression
Au boot, calculer le temps écoulé depuis `state.lastSession`. Capper à 8h (= 28800 sec = 288000 ticks).

Plutôt que de lancer 288000 itérations de DoTick, calculer analytiquement le gain par seconde × secondes écoulées, avec arrondi conservateur.

```csharp
public BigDouble ComputeOfflineGains(GameState state, float secondsElapsed) {
    var cappedSeconds = Mathf.Min(secondsElapsed, 8 * 3600);
    var forcePerSec = StatsCalculator.GetForcePerSecond(state);
    return forcePerSec * cappedSeconds;
}
```

## UI architecture (MVC-ish)

### Pattern
- **View**: MonoBehaviour qui gère l'affichage et capte les inputs UI
- **Controller**: classe POCO qui orchestre la logique UI et appelle le Gameplay layer
- **Pas de Model séparé**: le GameState est le model

### Bindings réactifs
Plutôt que d'updater l'UI à chaque frame, utiliser un système de notification:

```csharp
public class ForceCounterView : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI label;
    
    private void OnEnable() {
        Events.OnForceChanged.Register(UpdateDisplay);
        UpdateDisplay();
    }
    
    private void OnDisable() {
        Events.OnForceChanged.Unregister(UpdateDisplay);
    }
    
    private void UpdateDisplay() {
        var state = GameManager.Instance.State;
        label.text = NumberFormatter.Format(state.force);
    }
}
```

## Number formatter

```csharp
public static class NumberFormatter {
    private static readonly string[] Suffixes = {
        "", "K", "M", "B", "T",
        "aa", "bb", "cc", "dd", "ee",
        "ff", "gg", "hh", "ii", "jj"
        // ... continue jusqu'à zz minimum
    };
    
    public static string Format(BigDouble value) {
        if (value < 1000) return value.ToString("F0");
        
        var magnitude = (int)Math.Floor(Math.Log10((double)value) / 3);
        var scaled = value / BigDouble.Pow(10, magnitude * 3);
        var suffix = magnitude < Suffixes.Length ? Suffixes[magnitude] : $"e{magnitude * 3}";
        
        return $"{(double)scaled:F2}{suffix}";
    }
}
```

## Localization

Setup Unity Localization package dès le départ. Tous les strings affichés au joueur passent par `LocalizedString`. Locales MVP: **FR (par défaut), EN**.

Pas de hard-coded strings dans le code. Toujours via tables de locales.

## Testing

### Unit tests
- `Tests/EditMode/` pour les helpers, formatters, calcul de stats
- Tests sur BigDouble, NumberFormatter, StatsCalculator, OfflineGains

### Play mode tests
- `Tests/PlayMode/` pour les flows complets
- Test "from scratch to first prestige" en mode accéléré (tick × 100)

## Anti-patterns à éviter

- **Singletons partout**: seul GameManager est singleton, le reste passe par lui
- **GameObject.Find** ou **FindObjectOfType** dans le code de gameplay (uniquement en setup éditeur)
- **Update() lourd** dans les MonoBehaviours: tout passe par le GameTicker à 10Hz
- **String literals pour les IDs**: utiliser des constantes ou des enums
- **MonoBehaviour partout**: les services (Save, Audio, Content) sont des classes POCO instanciées par GameManager
- **Coroutines pour la logique gameplay**: préférer DOTween + async/await pour les flows séquentiels

## Coding style C#

- PascalCase pour classes, méthodes, propriétés publiques
- camelCase pour champs privés et locaux
- `_camelCase` pour fields privés sérialisés (`[SerializeField]`)
- Pas de `m_` prefix (style ancien Unity)
- Properties avec auto-getter quand possible: `public int Level { get; private set; }`
- Async/await pour I/O et flows séquentiels
- Pattern matching et `switch` expressions (C# 9+) bienvenus

## Notes pour le dev Claude

- **NE PAS implémenter de mécaniques non documentées** sans demander au coordinateur via le user.
- **PRÉFÉRER les SO pour le contenu** plutôt que hardcoder.
- **EXPOSER les flags debug** (cheat codes, skip cinematics, dev-only buttons) via une `DebugConsole` accessible en build dev seulement.
- Toujours **logger les décisions de design tech** dans `DESIGN_DECISIONS_LOG.md` si elles n'étaient pas spécifiées.
