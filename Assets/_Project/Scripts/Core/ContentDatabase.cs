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
    /// Sprint 6: <see cref="MaitreData"/> from Resources/Maitres/.
    /// Sprint 7: <see cref="SpriteLayerSet"/> from Resources/SpriteLayerSets/ and <see cref="VoieData"/> from Resources/Voies/.
    /// Sprint 8+ migrate to Addressables.
    ///
    /// Test seam: pass explicit collections (e.g. via *.CreateForTests factories)
    /// to bypass Resources scanning in EditMode tests.
    /// </summary>
    public sealed class ContentDatabase
    {
        private readonly Dictionary<string, UpgradeData> _upgradesById = new Dictionary<string, UpgradeData>();
        private readonly Dictionary<string, AdversaireData> _adversairesById = new Dictionary<string, AdversaireData>();
        private readonly Dictionary<string, CapitaineData> _capitainesById = new Dictionary<string, CapitaineData>();
        private readonly Dictionary<string, MaitreData> _maitresById = new Dictionary<string, MaitreData>();
        private readonly Dictionary<string, SpriteLayerSet> _layerSetsById = new Dictionary<string, SpriteLayerSet>();
        private readonly Dictionary<Voie, VoieData> _voiesByEnum = new Dictionary<Voie, VoieData>();
        private UpgradeData[] _orderedUpgrades = System.Array.Empty<UpgradeData>();
        private AdversaireData[] _orderedAdversaires = System.Array.Empty<AdversaireData>();
        private CapitaineData[] _orderedCapitaines = System.Array.Empty<CapitaineData>();
        private MaitreData[] _orderedMaitres = System.Array.Empty<MaitreData>();
        private SpriteLayerSet[] _orderedLayerSets = System.Array.Empty<SpriteLayerSet>();
        private VoieData[] _orderedVoies = System.Array.Empty<VoieData>();

        public IReadOnlyList<UpgradeData> AllUpgrades => _orderedUpgrades;
        public IReadOnlyList<AdversaireData> AllAdversaires => _orderedAdversaires;
        public IReadOnlyList<CapitaineData> AllCapitaines => _orderedCapitaines;
        public IReadOnlyList<MaitreData> AllMaitres => _orderedMaitres;
        public IReadOnlyList<SpriteLayerSet> AllSpriteLayerSets => _orderedLayerSets;
        public IReadOnlyList<VoieData> AllVoies => _orderedVoies;

        public ContentDatabase() : this(null, null, null, null, null, null) { }

        public ContentDatabase(IEnumerable<UpgradeData> upgrades,
            IEnumerable<AdversaireData> adversaires = null,
            IEnumerable<CapitaineData> capitaines = null,
            IEnumerable<MaitreData> maitres = null,
            IEnumerable<SpriteLayerSet> layerSets = null,
            IEnumerable<VoieData> voies = null)
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

            if (maitres != null)
                RegisterMaitres(maitres, sourceLabel: "injected");
            else
                RegisterMaitres(Resources.LoadAll<MaitreData>("Maitres"), sourceLabel: "Resources/Maitres");

            if (layerSets != null)
                RegisterSpriteLayerSets(layerSets, sourceLabel: "injected");
            else
                RegisterSpriteLayerSets(Resources.LoadAll<SpriteLayerSet>("SpriteLayerSets"), sourceLabel: "Resources/SpriteLayerSets");

            if (voies != null)
                RegisterVoies(voies, sourceLabel: "injected");
            else
                RegisterVoies(Resources.LoadAll<VoieData>("Voies"), sourceLabel: "Resources/Voies");
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

        public MaitreData GetMaitre(string id)
        {
            return id != null && _maitresById.TryGetValue(id, out var m) ? m : null;
        }

        public SpriteLayerSet GetSpriteLayerSet(string id)
        {
            return id != null && _layerSetsById.TryGetValue(id, out var s) ? s : null;
        }

        public VoieData GetVoie(Voie voie)
        {
            return _voiesByEnum.TryGetValue(voie, out var v) ? v : null;
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

        private void RegisterMaitres(IEnumerable<MaitreData> maitres, string sourceLabel)
        {
            _orderedMaitres = maitres?.Where(m => m != null).ToArray() ?? System.Array.Empty<MaitreData>();
            _maitresById.Clear();
            foreach (var m in _orderedMaitres)
            {
                if (string.IsNullOrEmpty(m.Id)) continue;
                if (_maitresById.ContainsKey(m.Id))
                {
                    Debug.LogWarning($"[ContentDatabase] Duplicate MaitreId '{m.Id}' in {m.name} — keeping first.");
                    continue;
                }
                _maitresById[m.Id] = m;
            }
            Debug.Log($"[ContentDatabase] Registered {_maitresById.Count} maitres from {sourceLabel}.");
        }

        private void RegisterSpriteLayerSets(IEnumerable<SpriteLayerSet> layerSets, string sourceLabel)
        {
            _orderedLayerSets = layerSets?.Where(s => s != null).ToArray() ?? System.Array.Empty<SpriteLayerSet>();
            _layerSetsById.Clear();
            foreach (var s in _orderedLayerSets)
            {
                if (string.IsNullOrEmpty(s.Id)) continue;
                if (_layerSetsById.ContainsKey(s.Id))
                {
                    Debug.LogWarning($"[ContentDatabase] Duplicate SpriteLayerSet id '{s.Id}' in {s.name} — keeping first.");
                    continue;
                }
                _layerSetsById[s.Id] = s;
            }
            Debug.Log($"[ContentDatabase] Registered {_layerSetsById.Count} sprite layer sets from {sourceLabel}.");
        }

        private void RegisterVoies(IEnumerable<VoieData> voies, string sourceLabel)
        {
            _orderedVoies = voies?.Where(v => v != null).ToArray() ?? System.Array.Empty<VoieData>();
            _voiesByEnum.Clear();
            foreach (var v in _orderedVoies)
            {
                if (v.VoieEnum == Voie.None) continue;
                if (_voiesByEnum.ContainsKey(v.VoieEnum))
                {
                    Debug.LogWarning($"[ContentDatabase] Duplicate VoieData enum '{v.VoieEnum}' in {v.name} — keeping first.");
                    continue;
                }
                _voiesByEnum[v.VoieEnum] = v;
            }
            Debug.Log($"[ContentDatabase] Registered {_voiesByEnum.Count} voies from {sourceLabel}.");
        }
    }
}
