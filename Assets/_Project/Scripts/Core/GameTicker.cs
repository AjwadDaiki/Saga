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

            // Sprint 6: combat + stade + disciple passives + Élan + Vague + Souffle.
            // Sprint 7+ adds:
            //   EspritsProcessor.Tick(state, dt);
            //   voie.passive?.OnTick(state, dt);
            //   StatsCalculator.Recompute(state); (cache invalidation)
            gm.Disciples?.Tick(gm.State, dt);
            gm.Stades?.Tick(gm.State, dt);
            gm.Combat?.Tick(gm.State, dt);
            gm.Elan?.Tick(gm.State, dt);
            gm.Vague?.Tick(gm.State, dt);
            gm.Souffle?.Tick(gm.State, dt);

            // Track currentRunForceMax — feeds Prestige Échos formula. Updated each tick to capture
            // passive Disciples gains, Vague Training spikes, etc.
            if (gm.State.force > gm.State.currentRunForceMax)
                gm.State.currentRunForceMax = gm.State.force;

            gm.Save?.TickThrottledSave(gm.State, dt);
        }
    }
}
