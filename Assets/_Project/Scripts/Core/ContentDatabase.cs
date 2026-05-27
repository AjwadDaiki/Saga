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
    /// Sprint 5: <see cref="CapitaineData"/> from Resources/Capitaines/.
    /// Sprint 7+ migrate to Addressables.
    ///
    /// Test seam: pass explicit collections (e.g. via *.CreateForTests factories)
    /// to bypass Resources scanning in EditMode tests.
    /// </summary>
    public sealed class ContentDatabase
    {
        private readonly Dictionary<string, UpgradeData> _upgradesById = new Dictionary<string, UpgradeData>();
        private readonly Dictionary<string, AdversaireData> _adversairesById = new Dictionary<string, AdversaireData>();
        private readonly Dictionary<string, CapitaineData> _capitainesById = new Dictionary<string, CapitaineData>();
        private UpgradeData[] _orderedUpgrades = System.Array.Empty<UpgradeData>();
        private AdversaireData[] _orderedAdversaires = System.Array.Empty<AdversaireData>();
        private CapitaineData[] _orderedCapitaines = System.Array.Empty<CapitaineData>();

        public IReadOnlyList<UpgradeData> AllUpgrades => _orderedUpgrades;
        public IReadOnlyList<AdversaireData> AllAdversaires => _orderedAdversaires;
        public IReadOnlyList<CapitaineData> AllCapitaines => _orderedCapitaines;

        public ContentDatabase() : this(null, null, null) { }

        public ContentDatabase(IEnumerable<UpgradeData> upgrades,
            IEnumerable<AdversaireData> adversaires = null,
            IEnumerable<CapitaineData> capitaines = null)
        {
            if (upgrades != null)
                RegisterUpgrades(upgrades, sourceLabel: "injected");
            else
                RegisterUpgrades(Resources.LoadAll<UpgradeData>("Upgrades"), sourceLabel: "Resources/Upgrades");

            if (adversaires != null)
                RegisterAdversaires(adversaires, sourceLabel: "injected");
            else
                RegisterAdversaires(Resources.LoadAll<AdversaireData>("Adversaires"), sourceLabel: "Resources/Adversaires");

            if (capitaines != null)
                RegisterCapitaines(capitaines, sourceLabel: "injected");
            else
                RegisterCapitaines(Resources.LoadAll<CapitaineData>("Capitaines"), sourceLabel: "Resources/Capitaines");
        }

        public UpgradeData GetUpgrade(string id)
        {
            return id != null && _upgradesById.TryGetValue(id, out var u) ? u : null;
        }

        public AdversaireData GetAdversaire(string id)
        {
            return id != null && _adversairesById.TryGetValue(id, out var a) ? a : null;
        }

        public CapitaineData GetCapitaine(string id)
        {
            return id != null && _capitainesById.TryGetValue(id, out var c) ? c : null;
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

        private void RegisterCapitaines(IEnumerable<CapitaineData> capitaines, string sourceLabel)
        {
            _orderedCapitaines = capitaines?.Where(c => c != null).ToArray() ?? System.Array.Empty<CapitaineData>();
            _capitainesById.Clear();
            foreach (var c in _orderedCapitaines)
            {
                if (string.IsNullOrEmpty(c.Id)) continue;
                if (_capitainesById.ContainsKey(c.Id))
                {
                    Debug.LogWarning($"[ContentDatabase] Duplicate CapitaineId '{c.Id}' in {c.name} — keeping first.");
                    continue;
                }
                _capitainesById[c.Id] = c;
            }
            Debug.Log($"[ContentDatabase] Registered {_capitainesById.Count} capitaines from {sourceLabel}.");
        }
    }
}
