using UnityEngine;

namespace Saga.Core
{
    /// <summary>
    /// Decoupled 10Hz tick loop for the incremental engine.
    /// Per 07_ARCHITECTURE.md, all gameplay tickers (disciples, esprits, voie passive)
    /// drive off this — never Update() directly. 100ms cadence is enough for fluidity
    /// without trashing mobile battery.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameTicker : MonoBehaviour
    {
        public const float TickIntervalSeconds = 0.1f;

        private float _accumulator;

        private void Update()
        {
            _accumulator += Time.deltaTime;
            while (_accumulator >= TickIntervalSeconds)
            {
                DoTick(TickIntervalSeconds);
                _accumulator -= TickIntervalSeconds;
            }
        }

        private void DoTick(float dt)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;

            // Sprint 2: passive disciple gain. Sprint 3+ adds:
            //   EspritsProcessor.Tick(state, dt);
            //   voie.passive?.OnTick(state, dt);
            //   StatsCalculator.Recompute(state); (cache invalidation)
            gm.Disciples?.Tick(gm.State, dt);
            gm.Save?.TickThrottledSave(gm.State, dt);
        }
    }
}
