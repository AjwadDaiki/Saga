using System.Collections.Generic;
using System.Linq;
using Saga.Data;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Holds runtime references to all content ScriptableObjects.
    /// Sprint 2: loads <see cref="UpgradeData"/> from Resources/Upgrades/. Sprint 7+ migrate to Addressables.
    ///
    /// Test seam: pass an explicit <c>upgrades</c> collection (e.g. via <see cref="UpgradeData"/>.CreateForTests)
    /// to bypass Resources scanning in EditMode tests.
    /// </summary>
    public sealed class ContentDatabase
    {
        private readonly Dictionary<string, UpgradeData> _upgradesById = new Dictionary<string, UpgradeData>();
        private UpgradeData[] _orderedUpgrades = System.Array.Empty<UpgradeData>();

        public IReadOnlyList<UpgradeData> AllUpgrades => _orderedUpgrades;

        public ContentDatabase() : this(null) { }

        public ContentDatabase(IEnumerable<UpgradeData> upgrades)
        {
            if (upgrades != null)
            {
                Register(upgrades, sourceLabel: "injected");
            }
            else
            {
                Register(Resources.LoadAll<UpgradeData>("Upgrades"), sourceLabel: "Resources/Upgrades");
            }
        }

        public UpgradeData GetUpgrade(string id)
        {
            return id != null && _upgradesById.TryGetValue(id, out var u) ? u : null;
        }

        private void Register(IEnumerable<UpgradeData> upgrades, string sourceLabel)
        {
            _orderedUpgrades = upgrades?.Where(u => u != null).ToArray() ?? System.Array.Empty<UpgradeData>();
            _upgradesById.Clear();
            foreach (var u in _orderedUpgrades)
            {
                if (string.IsNullOrEmpty(u.UpgradeId)) continue;
                if (_upgradesById.ContainsKey(u.UpgradeId))
                {
                    Debug.LogWarning($"[ContentDatabase] Duplicate UpgradeId '{u.UpgradeId}' in {u.name} — keeping first.");
                    continue;
                }
                _upgradesById[u.UpgradeId] = u;
            }
            Debug.Log($"[ContentDatabase] Registered {_upgradesById.Count} upgrades from {sourceLabel}.");
        }
    }
}
