using System;
using System.IO;
using Newtonsoft.Json;
using Saga.Data;
using UnityEngine;

namespace Saga.Save
{
    /// <summary>
    /// JSON-based persistence (Newtonsoft.Json) for <see cref="GameState"/>.
    /// Sync API — file IO is fast enough on mobile for our payload size,
    /// and matches the pattern in 07_ARCHITECTURE.md.
    ///
    /// Throttled writes: <see cref="MarkDirty"/> from gameplay, then <see cref="TickThrottledSave"/>
    /// from GameTicker — guarantees at most 1 disk write per <see cref="MinSaveIntervalSeconds"/>.
    /// </summary>
    public sealed class SaveService
    {
        public const string SaveFileName = "savegame.json";
        public const string BackupPrefix = "savegame_backup_";
        public const int BackupCount = 3;
        public const float MinSaveIntervalSeconds = 1f;

        private readonly JsonSerializerSettings _settings;
        private readonly string _savePath;
        private float _secondsSinceLastSave;
        private bool _dirty;

        public SaveService()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new BigDoubleJsonConverter() }
            };
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        public string SavePath => _savePath;

        /// <summary>Signal that state was mutated and should be persisted on the next throttle window.</summary>
        public void MarkDirty() => _dirty = true;

        /// <summary>Drive throttled saves from the GameTicker. Cheap when not dirty.</summary>
        public void TickThrottledSave(GameState state, float dt)
        {
            _secondsSinceLastSave += dt;
            if (!_dirty) return;
            if (_secondsSinceLastSave < MinSaveIntervalSeconds) return;
            ForceSave(state);
        }

        /// <summary>Persist immediately, bypassing throttle. Use on quit, milestone, prestige, etc.</summary>
        public void ForceSave(GameState state)
        {
            if (state == null) return;
            try
            {
                // Stamp lastSession at write time so offline progress (Sprint 9) gets correct delta.
                state.lastSession = DateTime.UtcNow;
                RotateBackups();
                var json = JsonConvert.SerializeObject(state, _settings);
                File.WriteAllText(_savePath, json);
                _dirty = false;
                _secondsSinceLastSave = 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Load the save file, with backup fallback on corruption.
        /// Returns a fresh <see cref="GameState"/> if nothing on disk.
        /// </summary>
        public GameState Load()
        {
            if (!File.Exists(_savePath))
            {
                return new GameState();
            }

            try
            {
                var json = File.ReadAllText(_savePath);
                var loaded = JsonConvert.DeserializeObject<GameState>(json, _settings);
                return loaded ?? new GameState();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Primary save corrupted ({e.Message}), trying backups...");
                return TryLoadBackup() ?? new GameState();
            }
        }

        private GameState TryLoadBackup()
        {
            for (var i = 1; i <= BackupCount; i++)
            {
                var path = Path.Combine(Application.persistentDataPath, $"{BackupPrefix}{i}.json");
                if (!File.Exists(path)) continue;
                try
                {
                    var json = File.ReadAllText(path);
                    var loaded = JsonConvert.DeserializeObject<GameState>(json, _settings);
                    if (loaded != null)
                    {
                        Debug.LogWarning($"[SaveService] Recovered from backup #{i}.");
                        return loaded;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveService] Backup #{i} unreadable: {e.Message}");
                }
            }
            return null;
        }

        private void RotateBackups()
        {
            if (!File.Exists(_savePath)) return;
            try
            {
                // _3 → drop, _2 → _3, _1 → _2, current save → _1
                var oldest = Path.Combine(Application.persistentDataPath, $"{BackupPrefix}{BackupCount}.json");
                if (File.Exists(oldest)) File.Delete(oldest);

                for (var i = BackupCount - 1; i >= 1; i--)
                {
                    var src = Path.Combine(Application.persistentDataPath, $"{BackupPrefix}{i}.json");
                    var dst = Path.Combine(Application.persistentDataPath, $"{BackupPrefix}{i + 1}.json");
                    if (File.Exists(src)) File.Move(src, dst);
                }

                var firstBackup = Path.Combine(Application.persistentDataPath, $"{BackupPrefix}1.json");
                File.Copy(_savePath, firstBackup, overwrite: true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Backup rotation failed (non-fatal): {e.Message}");
            }
        }
    }
}
