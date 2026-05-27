using Saga.Data;
using Saga.Save;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Service locator and entry point for the runtime. Auto-bootstraps before any scene
    /// loads (no need to wire it into Boot.unity manually).
    ///
    /// Holds the single mutable <see cref="GameState"/> and exposes core services.
    /// Future services (Audio, Content, Progression) get plugged in as their sprints land.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public SaveService Save { get; private set; }

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
            // NOTE: do not overwrite State.lastSession on boot — it represents the previous save
            // timestamp and is used by offline progression to compute elapsed time. SaveService
            // will refresh it on every ForceSave.

            Debug.Log($"GameManager OK | force={State.force} | taps={State.totalTaps} | savePath={Save.SavePath}");
            GameEvents.RaiseForceChanged();
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
