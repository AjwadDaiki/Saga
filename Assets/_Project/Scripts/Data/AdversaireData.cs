using BreakInfinity;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// ScriptableObject blueprint for one adversaire (Sprint 4 — 5 starter mobs).
    /// Bosses (Capitaines + Maîtres) get their own data types in Sprint 5-6.
    /// </summary>
    [CreateAssetMenu(fileName = "Adversaire_New", menuName = "Saga/Adversaire Data")]
    public class AdversaireData : ScriptableObject
    {
        [Tooltip("Stable lowercase snake_case identifier. Used as save key + Resources lookup.")]
        [SerializeField] private string _id;

        [Tooltip("UI display name shown on the spawn cinematic + HP bar.")]
        [SerializeField] private string _displayName;

        [Tooltip("Cultural school. None = neutral wanderer.")]
        [SerializeField] private Voie _voie = Voie.None;

        [Tooltip("Hit points the player must deplete via taps within chrono.")]
        [SerializeField] private double _hp = 100;

        [Tooltip("Force granted on victory.")]
        [SerializeField] private double _rewardForce = 30;

        [Tooltip("Probability (0..1) of dropping loot. Sprint 4 = stat tracked only, no visual yet.")]
        [Range(0f, 1f)]
        [SerializeField] private float _lootChance = 0.15f;

        [Tooltip("Resources-relative sprite name for the idle placeholder. Sprint 4 uses procedural sprites.")]
        [SerializeField] private string _spriteIdleName;

        [Tooltip("Seconds available to defeat the adversaire. Failure → mort temporaire.")]
        [Min(1f)]
        [SerializeField] private float _chronoSeconds = 30f;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Voie Voie => _voie;
        public BigDouble Hp => new BigDouble(_hp);
        public BigDouble RewardForce => new BigDouble(_rewardForce);
        public float LootChance => _lootChance;
        public string SpriteIdleName => _spriteIdleName;
        public float ChronoSeconds => _chronoSeconds;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id)) Debug.LogError($"[AdversaireData] {name} missing id", this);
            if (_hp <= 0) Debug.LogError($"[AdversaireData] {name} hp must be > 0", this);
            if (_rewardForce < 0) Debug.LogWarning($"[AdversaireData] {name} rewardForce < 0 (intended?)", this);
            if (_chronoSeconds <= 0) Debug.LogError($"[AdversaireData] {name} chronoSeconds must be > 0", this);
        }

#if UNITY_INCLUDE_TESTS
        public static AdversaireData CreateForTests(string id, string displayName, Voie voie,
            double hp, double rewardForce, float lootChance, float chronoSeconds, string spriteIdleName = null)
        {
            var a = CreateInstance<AdversaireData>();
            a._id = id;
            a._displayName = displayName ?? id;
            a._voie = voie;
            a._hp = hp;
            a._rewardForce = rewardForce;
            a._lootChance = lootChance;
            a._chronoSeconds = chronoSeconds;
            a._spriteIdleName = spriteIdleName ?? string.Empty;
            return a;
        }
#endif
    }
}
