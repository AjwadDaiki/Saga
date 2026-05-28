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
        /// Load the save file, with backup fallback on corruption, and apply schema migrations.
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
                return Migrate(loaded ?? new GameState());
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Primary save corrupted ({e.Message}), trying backups...");
                var fromBackup = TryLoadBackup();
                return fromBackup != null ? Migrate(fromBackup) : new GameState();
            }
        }

        /// <summary>
        /// Apply incremental schema migrations. New fields default to safe values;
        /// renamed/removed fields are mapped explicitly. Migrations are idempotent.
        /// </summary>
        private static GameState Migrate(GameState state)
        {
            const int currentVersion = 8;

            if (state.saveVersion < 2)
            {
                // v1 -> v2: introduced upgradeLevels (Sprint 2).
                if (state.upgradeLevels == null)
                {
                    state.upgradeLevels = new System.Collections.Generic.Dictionary<string, int>();
                }
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v2 (added upgradeLevels).");
            }

            if (state.saveVersion < 3)
            {
                // v2 -> v3: introduced currentStade (Sprint 3). Default 1 (Mendiant).
                if (state.currentStade <= 0) state.currentStade = 1;
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v3 (added currentStade).");
            }

            if (state.saveVersion < 4)
            {
                // v3 -> v4: combat active system (Sprint 4). Default to Training, no engaged adversaire.
                state.currentPhase = Saga.Data.CombatPhase.Training;
                state.currentAdversaireId = null;
                state.currentAdversaireHp = default;
                state.chronoRemaining = 0f;
                state.tapsTowardsNextAdversaire = 0;
                state.totalAdversairesDefeated = 0;
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v4 (added combat active system fields).");
            }

            if (state.saveVersion < 5)
            {
                // v4 -> v5: Élan / Vague AOE (Sprint 5). Default Élan 0.
                state.currentElan = 0f;
                state.lastVagueTime = 0f;
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v5 (added Élan + lastVagueTime).");
            }

            if (state.saveVersion < 6)
            {
                // v5 -> v6: Capitaines (Sprint 5). Default no engaged Capitaine.
                state.totalCapitainesDefeated = 0;
                state.currentCapitaineId = null;
                state.currentCapitainePhase = 0;
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v6 (added Capitaine fields).");
            }

            if (state.saveVersion < 7)
            {
                // v6 -> v7: Souffle + Maître engagement state (Sprint 6).
                state.lastSouffleTime = 0f;
                state.souffleBuffActiveUntil = -1f;
                state.currentMaitreId = null;
                state.currentMaitrePhase = 0;
                state.maitreInvocationSlots = 0;
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v7 (Souffle + Maître engagement).");
            }

            if (state.saveVersion < 8)
            {
                // v7 -> v8: Prestige currency + Hall des Légendes + citation + relics + titles (Sprint 6).
                if (state.totalEchos.Equals(default(BreakInfinity.BigDouble))) state.totalEchos = new BreakInfinity.BigDouble(0);
                state.currentRunEchosEarned = new BreakInfinity.BigDouble(0);
                if (state.currentRunForceMax.Equals(default(BreakInfinity.BigDouble))) state.currentRunForceMax = state.force;
                if (state.playerCitation == null) state.playerCitation = string.Empty;
                state.playerCitationLockedForRun = false;
                if (state.relicsOwned == null) state.relicsOwned = new System.Collections.Generic.List<string>();
                if (state.relicsConserved == null) state.relicsConserved = new System.Collections.Generic.List<string>();
                if (state.titlesUnlocked == null) state.titlesUnlocked = new System.Collections.Generic.List<string>();
                if (state.achievementsUnlocked == null) state.achievementsUnlocked = new System.Collections.Generic.List<string>();
                if (state.deathRecords == null) state.deathRecords = new System.Collections.Generic.List<Saga.Data.DeathRecord>();
                Debug.Log($"[SaveService] Migrated save v{state.saveVersion} -> v8 (Prestige + Hall des Légendes).");
            }

            // Defensive: always ensure non-null collections + valid scalars post-deserialization.
            if (state.upgradeLevels == null)
                state.upgradeLevels = new System.Collections.Generic.Dictionary<string, int>();
            if (state.relicsOwned == null) state.relicsOwned = new System.Collections.Generic.List<string>();
            if (state.relicsConserved == null) state.relicsConserved = new System.Collections.Generic.List<string>();
            if (state.titlesUnlocked == null) state.titlesUnlocked = new System.Collections.Generic.List<string>();
            if (state.achievementsUnlocked == null) state.achievementsUnlocked = new System.Collections.Generic.List<string>();
            if (state.deathRecords == null) state.deathRecords = new System.Collections.Generic.List<Saga.Data.DeathRecord>();
            if (state.playerCitation == null) state.playerCitation = string.Empty;
            if (state.currentStade <= 0) state.currentStade = 1;

            // Always boot in Training to avoid loading mid-combat with a stale chrono / dangling enemy ref.
            state.currentPhase = Saga.Data.CombatPhase.Training;
            state.currentAdversaireId = null;
            state.currentCapitaineId = null;
            state.currentCapitainePhase = 0;
            state.currentMaitreId = null;
            state.currentMaitrePhase = 0;
            state.chronoRemaining = 0f;
            // Élan resets to 0 on boot (decay model — no point persisting a partial gauge).
            state.currentElan = 0f;
            // Souffle: cooldown is persistent but buff active flag resets on boot (player wasn't tapping during quit).
            state.souffleBuffActiveUntil = -1f;
            // playerCitationLockedForRun resets so a fresh boot lets the player re-prompt at next death.
            state.playerCitationLockedForRun = false;

            state.saveVersion = currentVersion;
            return state;
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
