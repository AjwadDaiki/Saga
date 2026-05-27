using BreakInfinity;
using Saga.Core;
using Saga.Data;

namespace Saga.Gameplay
{
    /// <summary>
    /// Owns upgrade-level mutations of <see cref="GameState"/>.
    /// Read paths (<see cref="GetLevel"/>, <see cref="GetCostForNextLevel"/>, <see cref="CanAfford"/>)
    /// are side-effect-free and safe to call from UI on Update.
    ///
    /// <see cref="TryPurchase"/> applies the cost atomically: deducts Force, increments level,
    /// raises <see cref="GameEvents.OnForceChanged"/> + <see cref="GameEvents.OnUpgradePurchased"/>,
    /// and marks the save dirty.
    /// </summary>
    public sealed class UpgradeService
    {
        private readonly ContentDatabase _content;

        public UpgradeService(ContentDatabase content)
        {
            _content = content;
        }

        public int GetLevel(GameState state, string upgradeId)
        {
            if (state == null || state.upgradeLevels == null || upgradeId == null) return 0;
            return state.upgradeLevels.TryGetValue(upgradeId, out var lvl) ? lvl : 0;
        }

        public BigDouble GetCostForNextLevel(GameState state, string upgradeId)
        {
            var data = _content.GetUpgrade(upgradeId);
            if (data == null) return new BigDouble(0);
            return data.GetCostForLevel(GetLevel(state, upgradeId));
        }

        public bool CanAfford(GameState state, string upgradeId)
        {
            if (state == null) return false;
            var cost = GetCostForNextLevel(state, upgradeId);
            return state.force >= cost;
        }

        /// <summary>
        /// Atomic purchase. Returns true if successful (Force was sufficient and the upgrade
        /// exists in ContentDatabase). Returns false silently otherwise.
        /// </summary>
        public bool TryPurchase(GameState state, string upgradeId)
        {
            if (state == null) return false;
            var data = _content.GetUpgrade(upgradeId);
            if (data == null) return false;

            var cost = GetCostForNextLevel(state, upgradeId);
            if (state.force < cost) return false;

            state.force -= cost;
            state.upgradeLevels.TryGetValue(upgradeId, out var current);
            var newLevel = current + 1;
            state.upgradeLevels[upgradeId] = newLevel;

            GameManager.Instance?.Save?.MarkDirty();
            GameEvents.RaiseForceChanged();
            GameEvents.RaiseUpgradePurchased(upgradeId, newLevel);
            return true;
        }
    }
}
