using BreakInfinity;
using Saga.Core;
using Saga.Data;

namespace Saga.Gameplay
{
    /// <summary>
    /// Pure aggregation over GameState × ContentDatabase. No mutation, no side effects.
    /// Cheap enough to call per-tap; if it ever becomes hot, cache via a dirty flag from
    /// <see cref="GameEvents.OnUpgradePurchased"/>.
    /// </summary>
    public static class StatsCalculator
    {
        /// <summary>
        /// Base Force/tap before combo. 1 (constant) + Σ ForcePerTap upgrades × level + Σ equipment Force bonus.
        /// Sprint 7 adds the equipment bonus aggregated from currently-equipped <see cref="SpriteLayerSet"/>s.
        /// </summary>
        public static BigDouble GetForcePerTap(GameState state, ContentDatabase content)
        {
            if (state == null || content == null) return new BigDouble(1);
            var total = new BigDouble(1);
            foreach (var upgrade in content.AllUpgrades)
            {
                if (upgrade.EffectType != UpgradeEffectType.ForcePerTap) continue;
                var level = GetLevel(state, upgrade.UpgradeId);
                if (level <= 0) continue;
                total += upgrade.EffectValue * level;
            }
            total += GetEquipmentForceBonus(state, content);
            return total;
        }

        /// <summary>
        /// Sum of Force bonuses across all equipped SpriteLayerSets. Mirrors
        /// <see cref="Saga.Gameplay.EquipmentService.GetTotalStatsBonus"/> but as a pure read of GameState IDs
        /// (so it can be called without a service handle, e.g. from offline progress code).
        /// </summary>
        public static BigDouble GetEquipmentForceBonus(GameState state, ContentDatabase content)
        {
            if (state == null || content == null) return new BigDouble(0);
            var total = new BigDouble(0);
            total += LayerForce(content, state.equippedBodyId);
            total += LayerForce(content, state.equippedArmorId);
            total += LayerForce(content, state.equippedWeaponId);
            return total;
        }

        private static BigDouble LayerForce(ContentDatabase content, string id)
        {
            if (string.IsNullOrEmpty(id)) return new BigDouble(0);
            var l = content.GetSpriteLayerSet(id);
            return l != null ? l.StatsBonusForce : new BigDouble(0);
        }

        /// <summary>Total Force/second from passive upgrades (Disciple etc.). 0 if no levels.</summary>
        public static BigDouble GetForcePerSecond(GameState state, ContentDatabase content)
        {
            if (state == null || content == null) return new BigDouble(0);
            var total = new BigDouble(0);
            foreach (var upgrade in content.AllUpgrades)
            {
                if (upgrade.EffectType != UpgradeEffectType.ForcePerSecond) continue;
                var level = GetLevel(state, upgrade.UpgradeId);
                if (level <= 0) continue;
                total += upgrade.EffectValue * level;
            }
            return total;
        }

        /// <summary>
        /// Multiplicative bonus over the base combo tier multiplier. 1.0 = no bonus.
        /// Final combo multiplier = base_tier × GetComboMultiplierBonus(state).
        /// Each Méditation level adds <c>effectValue</c> (e.g. 0.05 = +5%).
        /// </summary>
        public static float GetComboMultiplierBonus(GameState state, ContentDatabase content)
        {
            if (state == null || content == null) return 1f;
            var bonus = 1f;
            foreach (var upgrade in content.AllUpgrades)
            {
                if (upgrade.EffectType != UpgradeEffectType.ComboMultiplierBonus) continue;
                var level = GetLevel(state, upgrade.UpgradeId);
                if (level <= 0) continue;
                bonus += upgrade.EffectValue * level;
            }
            return bonus;
        }

        private static int GetLevel(GameState state, string id)
        {
            return state.upgradeLevels != null && state.upgradeLevels.TryGetValue(id, out var l) ? l : 0;
        }
    }
}
