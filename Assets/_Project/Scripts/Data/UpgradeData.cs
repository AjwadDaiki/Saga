using BreakInfinity;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// ScriptableObject blueprint for one upgrade (Frappe, Disciple, Méditation, …).
    /// Following 07_ARCHITECTURE.md §1: all content lives as SOs so game designers
    /// (Ajwad / coordinator) can tweak balancing without touching code.
    ///
    /// Cost progression: cost(level) = costBase × costMultiplier^level
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade_New", menuName = "Saga/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Tooltip("Stable, lowercase, snake_case identifier. Used as save key.")]
        [SerializeField] private string _upgradeId;

        [Tooltip("UI display name (FR). Sprint 2 = plain string. Sprint 4+ migrate to LocalizedString.")]
        [SerializeField] private string _displayName;

        [TextArea(2, 4)]
        [SerializeField] private string _descriptionFr;

        [Tooltip("Cost at level 0 (first purchase brings level to 1).")]
        [SerializeField] private double _costBase = 10;

        [Tooltip("Multiplied into the cost per level — typically 1.10 to 1.50 for idle games.")]
        [SerializeField] private float _costMultiplier = 1.15f;

        [SerializeField] private UpgradeEffectType _effectType;

        [Tooltip("Per-level effect magnitude. ForcePerTap=BigDouble at level scaling; ForcePerSecond=same; ComboMultiplierBonus=absolute add to combo cap (e.g. 0.05 = +5%).")]
        [SerializeField] private float _effectValue = 1f;

        [SerializeField] private Sprite _icon;

        public string UpgradeId => _upgradeId;
        public string DisplayName => _displayName;
        public string DescriptionFr => _descriptionFr;
        public BigDouble CostBase => new BigDouble(_costBase);
        public float CostMultiplier => _costMultiplier;
        public UpgradeEffectType EffectType => _effectType;
        public float EffectValue => _effectValue;
        public Sprite Icon => _icon;

        /// <summary>
        /// Returns the cost to BUY THE NEXT LEVEL given the current level.
        /// At currentLevel=0 → returns costBase. At currentLevel=N → costBase × multiplier^N.
        /// </summary>
        public BigDouble GetCostForLevel(int currentLevel)
        {
            if (currentLevel <= 0) return CostBase;
            return CostBase * BigDouble.Pow(_costMultiplier, currentLevel);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_upgradeId))
                Debug.LogError($"[UpgradeData] {name} missing upgradeId", this);
            if (_costBase <= 0)
                Debug.LogError($"[UpgradeData] {name} costBase must be > 0", this);
            if (_costMultiplier < 1f)
                Debug.LogWarning($"[UpgradeData] {name} costMultiplier < 1 — cost decreases per level (intended?)", this);
        }

#if UNITY_INCLUDE_TESTS
        /// <summary>Test-only factory. Bypasses the asset workflow by building an in-memory SO with the given values.</summary>
        public static UpgradeData CreateForTests(string id, double costBase, float costMultiplier,
            UpgradeEffectType effectType, float effectValue, string displayName = null, string descriptionFr = null)
        {
            var u = CreateInstance<UpgradeData>();
            u._upgradeId = id;
            u._displayName = displayName ?? id;
            u._descriptionFr = descriptionFr ?? string.Empty;
            u._costBase = costBase;
            u._costMultiplier = costMultiplier;
            u._effectType = effectType;
            u._effectValue = effectValue;
            return u;
        }
#endif
    }
}
