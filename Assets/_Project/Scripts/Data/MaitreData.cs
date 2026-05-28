using BreakInfinity;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// ScriptableObject blueprint for a Maître légendaire (Boss Majeur, Sprint 6).
    /// 8 total — one per voie, all inspired by real mythology (zero copyright per GAME_DESIGN_v2).
    /// HP 10-20× a regular adversaire. Defeating one drops a unique <see cref="ReliqueUniqueName"/>
    /// relic. Dying to one triggers <see cref="Gameplay.PrestigeService"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Maitre_New", menuName = "Saga/Maitre Data")]
    public class MaitreData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Voie _voie = Voie.None;

        [SerializeField] private double _hp = 8000;
        [SerializeField] private double _rewardForce = 4000;

        [Min(1f)]
        [SerializeField] private float _chronoSeconds = 180f;

        [SerializeField] private float[] _phaseThresholds = { 0.75f, 0.5f, 0.25f };
        [SerializeField] private Color[] _phaseColors = new Color[4];

        [TextArea(2, 3)] [SerializeField] private string _introCitation;
        [TextArea(2, 3)] [SerializeField] private string _victoryCitation; // affichée quand joueur GAGNE
        [TextArea(2, 3)] [SerializeField] private string _defeatCitation;  // affichée dans cinématique Prestige quand joueur PERD

        [Header("Relique unique")]
        [SerializeField] private string _reliqueUniqueName;
        [TextArea(2, 3)] [SerializeField] private string _reliqueUniqueDescription;
        [SerializeField] private double _reliqueStatsBonus = 500;

        [Header("Identité visuelle/sonore")]
        [SerializeField] private Color _arenaBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        [SerializeField] private float _drumHitPitch = 1f;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Voie Voie => _voie;
        public BigDouble Hp => new BigDouble(_hp);
        public BigDouble RewardForce => new BigDouble(_rewardForce);
        public float ChronoSeconds => _chronoSeconds;
        public float[] PhaseThresholds => _phaseThresholds;
        public Color[] PhaseColors => _phaseColors;
        public string IntroCitation => _introCitation;
        public string VictoryCitation => _victoryCitation;
        public string DefeatCitation => _defeatCitation;
        public string ReliqueUniqueName => _reliqueUniqueName;
        public string ReliqueUniqueDescription => _reliqueUniqueDescription;
        public BigDouble ReliqueStatsBonus => new BigDouble(_reliqueStatsBonus);
        public Color ArenaBackgroundColor => _arenaBackgroundColor;
        public float DrumHitPitch => _drumHitPitch;

        /// <summary>0..3 phase index given a current HP / max HP ratio.</summary>
        public int ComputePhase(double hpRatio)
        {
            if (_phaseThresholds == null || _phaseThresholds.Length == 0) return 0;
            for (var i = 0; i < _phaseThresholds.Length; i++)
            {
                if (hpRatio > _phaseThresholds[i]) return i;
            }
            return _phaseThresholds.Length;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id)) Debug.LogError($"[MaitreData] {name} missing id", this);
            if (_hp <= 0) Debug.LogError($"[MaitreData] {name} hp must be > 0", this);
            if (_chronoSeconds <= 0) Debug.LogError($"[MaitreData] {name} chronoSeconds must be > 0", this);
        }

#if UNITY_INCLUDE_TESTS
        public static MaitreData CreateForTests(string id, string displayName, Voie voie,
            double hp, double rewardForce, float chronoSeconds,
            string introCitation = null, string victoryCitation = null, string defeatCitation = null,
            string reliqueName = null, double reliqueBonus = 500)
        {
            var m = CreateInstance<MaitreData>();
            m._id = id;
            m._displayName = displayName ?? id;
            m._voie = voie;
            m._hp = hp;
            m._rewardForce = rewardForce;
            m._chronoSeconds = chronoSeconds;
            m._phaseThresholds = new[] { 0.75f, 0.5f, 0.25f };
            m._phaseColors = new[] { Color.white, Color.white, Color.white, Color.red };
            m._introCitation = introCitation ?? string.Empty;
            m._victoryCitation = victoryCitation ?? string.Empty;
            m._defeatCitation = defeatCitation ?? string.Empty;
            m._reliqueUniqueName = reliqueName ?? $"Relique de {displayName ?? id}";
            m._reliqueUniqueDescription = string.Empty;
            m._reliqueStatsBonus = reliqueBonus;
            m._arenaBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
            m._drumHitPitch = 1f;
            return m;
        }
#endif
    }
}
