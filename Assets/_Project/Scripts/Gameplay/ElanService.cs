using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Élan gauge accumulator (Sprint 5). Fills on tap, decays after a grace window
    /// without input. Emits <see cref="GameEvents.OnElanFull"/> once when the cap is
    /// first reached (drives the Vague button reveal).
    ///
    /// Hooks:
    /// - <see cref="GameEvents.OnTapResolved"/>  → increment (with combo synergy ×2)
    /// - <see cref="GameTicker"/>.DoTick(dt)     → decay after idle grace
    /// </summary>
    public sealed class ElanService
    {
        private float _secondsSinceLastTap;
        private bool _hasEmittedFull;
        private int _currentComboTier;

        public ElanService()
        {
            GameEvents.OnTapResolved += HandleTapResolved;
            GameEvents.OnComboChanged += HandleComboChanged;
        }

        private void HandleComboChanged(int tier, float baseMultiplier)
        {
            _currentComboTier = tier;
        }

        /// <summary>Drive from <see cref="GameTicker"/> at 10Hz.</summary>
        public void Tick(GameState state, float dt)
        {
            if (state == null) return;

            _secondsSinceLastTap += dt;
            if (_secondsSinceLastTap >= ElanConstants.ElanIdleGraceSeconds && state.currentElan > 0f)
            {
                var before = state.currentElan;
                state.currentElan = Mathf.Max(0f, state.currentElan - ElanConstants.ElanDecayPerSec * dt);
                if (!Mathf.Approximately(before, state.currentElan))
                {
                    GameEvents.RaiseElanChanged(state.currentElan, ElanConstants.ElanMax);
                    if (state.currentElan < ElanConstants.ElanMax) _hasEmittedFull = false;
                }
            }
        }

        /// <summary>Reset Élan to zero (called after a Vague trigger).</summary>
        public void ResetForVague(GameState state)
        {
            if (state == null) return;
            state.currentElan = 0f;
            _hasEmittedFull = false;
            GameEvents.RaiseElanChanged(0f, ElanConstants.ElanMax);
        }

        private void HandleTapResolved(BreakInfinity.BigDouble _, float __, Vector2 ___)
        {
            var gm = GameManager.Instance;
            if (gm?.State == null) return;
            RegisterTap(gm.State, _currentComboTier);
            gm.Save?.MarkDirty();
        }

        /// <summary>Test-friendly entry point. Production calls go through <see cref="HandleTapResolved"/>.</summary>
        public void RegisterTap(GameState state, int comboTier)
        {
            if (state == null) return;
            _secondsSinceLastTap = 0f;

            var gain = ElanConstants.ElanPerTap * (comboTier > 0 ? ElanConstants.ElanComboBonusMultiplier : 1f);

            var before = state.currentElan;
            state.currentElan = Mathf.Min(ElanConstants.ElanMax, state.currentElan + gain);
            if (Mathf.Approximately(before, state.currentElan)) return;

            GameEvents.RaiseElanChanged(state.currentElan, ElanConstants.ElanMax);

            if (!_hasEmittedFull && state.currentElan >= ElanConstants.ElanMax)
            {
                _hasEmittedFull = true;
                GameEvents.RaiseElanFull();
            }
        }
    }
}
