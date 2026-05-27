using System.Collections.Generic;
using System.Linq;
using Saga.Data;
using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Holds runtime references to all content ScriptableObjects.
    /// Sprint 2: <see cref="UpgradeData"/> from Resources/Upgrades/.
    /// Sprint 4: <see cref="AdversaireData"/> from Resources/Adversaires/.
    /// Sprint 7+ migrate to Addressables.
    ///
    /// Test seam: pass explicit collections (e.g. via *.CreateForTests factories)
    /// to bypass Resources scanning in EditMode tests.
    /// </summary>
    public sealed class ContentDatabase
    {
        private readonly Dictionary<string, UpgradeData> _upgradesById = new Dictionary<string, UpgradeData>();
        private readonly Dictionary<string, AdversaireData> _adversairesById = new Dictionary<string, AdversaireData>();
        private UpgradeData[] _orderedUpgrades = System.Array.Empty<UpgradeData>();
        private AdversaireData[] _orderedAdversaires = System.Array.Empty<AdversaireData>();

        public IReadOnlyList<UpgradeData> AllUpgrades => _orderedUpgrades;
        public IReadOnlyList<AdversaireData> AllAdversaires => _orderedAdversaires;

        public ContentDatabase() : this(null, null) { }

        public ContentDatabase(IEnumerable<UpgradeData> upgrades, IEnumerable<AdversaireData> adversaires = null)
        {
            if (upgrades != null)
                RegisterUpgrades(upgrades, sourceLabel: "injected");
            else
                RegisterUpgrades(Resources.LoadAll<UpgradeData>("Upgrades"), sourceLabel: "Resources/Upgrades");

            if (adversaires != null)
                RegisterAdversaires(adversaires, sourceLabel: "injected");
            else
                RegisterAdversaires(Resources.LoadAll<AdversaireData>("Adversaires"), sourceLabel: "Resources/Adversaires");
        }

        public UpgradeData GetUpgrade(string id)
        {
            return id != null && _upgradesById.TryGetValue(id, out var u) ? u : null;
        }

        public AdversaireData GetAdversaire(string id)
        {
            return id != null && _adversairesById.TryGetValue(id, out var a) ? a : null;
        }

        private void RegisterUpgrades(IEnumerable<UpgradeData> upgrades, string sourceLabel)
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

        private void RegisterAdversaires(IEnumerable<AdversaireData> adversaires, string sourceLabel)
        {
            _orderedAdversaires = adversaires?.Where(a => a != null).ToArray() ?? System.Array.Empty<AdversaireData>();
            _adversairesById.Clear();
            foreach (var a in _orderedAdversaires)
            {
                if (string.IsNullOrEmpty(a.Id)) continue;
                if (_adversairesById.ContainsKey(a.Id))
                {
                    Debug.LogWarning($"[ContentDatabase] Duplicate AdversaireId '{a.Id}' in {a.name} — keeping first.");
                    continue;
                }
                _adversairesById[a.Id] = a;
            }
            Debug.Log($"[ContentDatabase] Registered {_adversairesById.Count} adversaires from {sourceLabel}.");
        }
    }
}
