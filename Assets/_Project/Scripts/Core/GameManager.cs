using Saga.Audio;
using Saga.Data;
using Saga.Gameplay;
using Saga.Save;
using Saga.Tutorial;
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
        public CapitaineSpawner Capitaines { get; private set; }
        public MaitreSpawner Maitres { get; private set; }
        public DamageDealer Damage { get; private set; }
        public ElanService Elan { get; private set; }
        public VagueResolver Vague { get; private set; }
        public SouffleService Souffle { get; private set; }
        public PrestigeService Prestige { get; private set; }
        public EquipmentService Equipment { get; private set; }
        public AudioService Audio { get; private set; }
        public HapticService Haptic { get; private set; }
        public TutorialService Tutorial { get; private set; }

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

            // Sprint 7.5 Polish Phase 1 — diagnostic try/catch around the service init chain. The
            // user reported that Awake silently failed mid-way: State was populated (L.61) but
            // Souffle (L.76) remained null, with ZERO error in the Editor console. The catch below
            // surfaces the swallowed exception so we can identify and fix the root cause. Once the
            // cause is fixed and verified, this guard stays in place as a safety net (services
            // staying null degrades the game gracefully instead of crashing the whole UI build).
            try
            {
                Save = new SaveService();
                State = Save.Load();
                // NOTE: SaveService refreshes State.lastSession on every ForceSave; do not stamp here.

                Content = new ContentDatabase();
                Upgrades = new UpgradeService(Content);
                Disciples = new DisciplesProcessor(Content);
                Stades = new StadeManager();
                Combat = new CombatProcessor(Content);
                Adversaires = new AdversaireSpawner(Content, Combat);
                Capitaines = new CapitaineSpawner(Content);
                Maitres = new MaitreSpawner(Content);
                Combat.AttachCapitaineSpawner(Capitaines);
                Damage = new DamageDealer(Content);
                Elan = new ElanService();
                Vague = new VagueResolver(Content);
                Souffle = new SouffleService();
                Prestige = new PrestigeService();
                Equipment = new EquipmentService(Content);

                // Sprint 7.5: audio + haptic services. AudioSource lives on the GameManager GO so
                // PlayOneShot survives scene reloads (DontDestroyOnLoad above).
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                Audio = new AudioService(audioSource);
                Haptic = new HapticService();
                var audioBindings = gameObject.AddComponent<AudioBindings>();
                audioBindings.Init(Audio);
                var hapticBindings = gameObject.AddComponent<HapticBindings>();
                hapticBindings.Init(Haptic);

                // Sprint 8 Phase A — tutorial onboarding. Service loads steps from
                // Resources/Tutorial/ ; Start() is no-op if State.tutorialDone (legacy or skipped).
                // TutorialOverlayBuilder consomme GameEvents.OnTutorialStepShown/Completed/Finished
                // pour rendre l'overlay. Start() différé à OnEnable du Main scene (voir
                // MainSceneBootstrap) pour s'assurer que les builders UI sont posés avant le 1er prompt.
                Tutorial = new TutorialService(State, Save);

                // Route per-tap progress to the spawner. DamageDealer, ElanService, VagueResolver,
                // CapitaineSpawner, MaitreSpawner all subscribe themselves in their constructors.
                GameEvents.OnTapResolved += HandleTapForSpawner;

                // Initialize currentRunForceMax if it's behind the actual force (e.g. fresh state).
                if (State.currentRunForceMax < State.force) State.currentRunForceMax = State.force;

                Debug.Log($"GameManager OK | force={State.force} | taps={State.totalTaps} | upgrades={State.upgradeLevels.Count} | adv={Content.AllAdversaires.Count}/{State.totalAdversairesDefeated} | cap={Content.AllCapitaines.Count}/{State.totalCapitainesDefeated} | maitres={Content.AllMaitres.Count} | echos={State.totalEchos} | prestiges={State.prestigeCount} | savePath={Save.SavePath}");
                GameEvents.RaiseForceChanged();
                GameEvents.RaiseElanChanged(State.currentElan, Saga.Data.ElanConstants.ElanMax);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameManager.Awake] SWALLOWED EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Debug.LogException(ex);
                // Re-raise so any tooling that watches errors gets it too — Unity will then print
                // it visibly in the Editor (which was the silent failure before).
                throw;
            }
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
