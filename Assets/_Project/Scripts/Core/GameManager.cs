using Saga.Data;
using Saga.Gameplay;
using Saga.Save;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Service locator and entry point for the runtime. Auto-bootstraps before any scene
    /// loads (no need to wire it into Boot.unity manually).
    ///
    /// Holds the single mutable <see cref="GameState"/> and exposes core services.
    /// Future services (Audio, Progression) get plugged in as their sprints land.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public SaveService Save { get; private set; }
        public ContentDatabase Content { get; private set; }
        public UpgradeService Upgrades { get; private set; }
        public DisciplesProcessor Disciples { get; private set; }
        public StadeManager Stades { get; private set; }
        public CombatProcessor Combat { get; private set; }
        public AdversaireSpawner Adversaires { get; private set; }
        public DamageDealer Damage { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject(nameof(GameManager));
            DontDestroyOnLoad(go);
            go.AddComponent<GameManager>();
            // GameTicker lives on the same root GameObject so its lifecycle is bound to GameManager.
            go.AddComponent<GameTicker>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Save = new SaveService();
            State = Save.Load();
            // NOTE: SaveService refreshes State.lastSession on every ForceSave; do not stamp here.

            Content = new ContentDatabase();
            Upgrades = new UpgradeService(Content);
            Disciples = new DisciplesProcessor(Content);
            Stades = new StadeManager();
            Combat = new CombatProcessor(Content);
            Adversaires = new AdversaireSpawner(Content, Combat);
            Damage = new DamageDealer(Content);

            // Route per-tap progress to the spawner. DamageDealer subscribes itself in its constructor.
            GameEvents.OnTapResolved += HandleTapForSpawner;

            Debug.Log($"GameManager OK | force={State.force} | taps={State.totalTaps} | upgrades={State.upgradeLevels.Count} | adv={Content.AllAdversaires.Count} | savePath={Save.SavePath}");
            GameEvents.RaiseForceChanged();
        }

        private void HandleTapForSpawner(BreakInfinity.BigDouble gain, float multiplier, UnityEngine.Vector2 screenPos)
        {
            Adversaires?.OnTap(State);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && State != null) Save?.ForceSave(State);
        }

        private void OnApplicationQuit()
        {
            if (State != null) Save?.ForceSave(State);
        }
    }
}
