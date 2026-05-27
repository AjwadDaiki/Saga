using BreakInfinity;
using Saga.Core;
using Saga.Data;

namespace Saga.Gameplay
{
    /// <summary>
    /// Each tick (driven by <see cref="GameTicker"/>), accrues passive Force from upgrades
    /// of effect type <see cref="UpgradeEffectType.ForcePerSecond"/>.
    /// </summary>
    public sealed class DisciplesProcessor
    {
        private readonly ContentDatabase _content;

        public DisciplesProcessor(ContentDatabase content)
        {
            _content = content;
        }

        public void Tick(GameState state, float dt)
        {
            if (state == null || _content == null) return;
            var fps = StatsCalculator.GetForcePerSecond(state, _content);
            if (fps.Sign() <= 0) return;

            // BigDouble * float (implicit promotion) gives BigDouble.
            state.force += fps * dt;
            GameEvents.RaiseForceChanged();
            GameManager.Instance?.Save?.MarkDirty();
        }
    }
}
