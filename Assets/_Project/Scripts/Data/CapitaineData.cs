using BreakInfinity;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// ScriptableObject blueprint for a Capitaine (Boss Mineur, Sprint 5).
    /// 8 total — one per voie. HP ~5× a regular adversaire, drops guaranteed loot,
    /// has HP-phase thresholds (75% / 50% / 25%) for visual escalation + enrage at 25%.
    /// </summary>
    [CreateAssetMenu(fileName = "Capitaine_New", menuName = "Saga/Capitaine Data")]
    public class CapitaineData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Voie _voie = Voie.None;

        [Tooltip("Total HP. Sprint 5 = ~5× a normal adversaire.")]
        [SerializeField] private double _hp = 500;

        [Tooltip("Force granted on victory. ~5× a normal adversaire.")]
        [SerializeField] private double _rewardForce = 250;

        [Tooltip("Capitaines always drop loot — flag kept for future tuning.")]
        [SerializeField] private bool _guaranteedLoot = true;

        [Min(1f)]
        [SerializeField] private float _chronoSeconds = 60f;

        [Tooltip("HP ratio thresholds for the 3 visual phases. Must be in decreasing order, all in (0..1).")]
        [SerializeField] private float[] _phaseThresholds = { 0.75f, 0.5f, 0.25f };

        [Tooltip("Aura tint per phase (phase 0 = full HP, phase 3 = enrage). Length should match _phaseThresholds + 1 = 4.")]
        [SerializeField] private Color[] _phaseColors = new Color[4];

        [TextArea(2, 3)]
        [SerializeField] private string _introCitation;
        [TextArea(2, 3)]
        [SerializeField] private string _deathCitation;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Voie Voie => _voie;
        public BigDouble Hp => new BigDouble(_hp);
        public BigDouble RewardForce => new BigDouble(_rewardForce);
        public bool GuaranteedLoot => _guaranteedLoot;
        public float ChronoSeconds => _chronoSeconds;
        public float[] PhaseThresholds => _phaseThresholds;
        public Color[] PhaseColors => _phaseColors;
        public string IntroCitation => _introCitation;
        public string DeathCitation => _deathCitation;

        /// <summary>0 = full HP, 1 = below 75%, 2 = below 50%, 3 = below 25% (enrage).</summary>
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
            if (string.IsNullOrEmpty(_id)) Debug.LogError($"[CapitaineData] {name} missing id", this);
            if (_hp <= 0) Debug.LogError($"[CapitaineData] {name} hp must be > 0", this);
            if (_chronoSeconds <= 0) Debug.LogError($"[CapitaineData] {name} chronoSeconds must be > 0", this);
            if (_phaseThresholds != null)
            {
                for (var i = 0; i < _phaseThresholds.Length; i++)
                {
                    if (_phaseThresholds[i] <= 0f || _phaseThresholds[i] > 1f)
                        Debug.LogError($"[CapitaineData] {name} phaseThresholds[{i}] must be in (0..1]", this);
                    if (i > 0 && _phaseThresholds[i] >= _phaseThresholds[i - 1])
                        Debug.LogError($"[CapitaineData] {name} phaseThresholds must be strictly decreasing", this);
                }
            }
        }

#if UNITY_INCLUDE_TESTS
        public static CapitaineData CreateForTests(string id, string displayName, Voie voie,
            double hp, double rewardForce, float chronoSeconds,
            float[] phaseThresholds = null, string introCitation = null, string deathCitation = null)
        {
            var c = CreateInstance<CapitaineData>();
            c._id = id;
            c._displayName = displayName ?? id;
            c._voie = voie;
            c._hp = hp;
            c._rewardForce = rewardForce;
            c._chronoSeconds = chronoSeconds;
            c._phaseThresholds = phaseThresholds ?? new[] { 0.75f, 0.5f, 0.25f };
            c._phaseColors = new Color[c._phaseThresholds.Length + 1];
            for (var i = 0; i < c._phaseColors.Length; i++) c._phaseColors[i] = Color.white;
            c._introCitation = introCitation ?? string.Empty;
            c._deathCitation = deathCitation ?? string.Empty;
            c._guaranteedLoot = true;
            return c;
        }
#endif
    }
}
