using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Méditation / Souffle (Sprint 6). 3-state machine:
    ///   Idle ──(TryStartMeditation)──&gt; Meditating ──(5s)──&gt; Buffing ──(30s)──&gt; Idle
    ///
    /// While Meditating: taps blocked (TapHandler checks <see cref="IsMeditating"/>).
    /// While Buffing: TapHandler multiplies Force/tap by <see cref="SouffleConstants.BuffForceMultiplier"/>.
    /// Cooldown starts at meditation begin (so total visible cooldown ≈ 120s - 35s = 85s after buff ends).
    ///
    /// Per DESIGN_DECISIONS_LOG: cooldown is PERSISTENT across prestige (skill doesn't unlearn).
    /// </summary>
    public sealed class SouffleService
    {
        private enum SouffleState { Idle, Meditating, Buffing }

        private SouffleState _state = SouffleState.Idle;
        private float _meditationTimer;
        private float _buffTimer;
        private float _cooldownTimer; // counts down toward 0

        public bool IsMeditating => _state == SouffleState.Meditating;
        public bool IsBuffActive => _state == SouffleState.Buffing;
        public bool IsOnCooldown => _cooldownTimer > 0f;
        public float CooldownRemaining => _cooldownTimer;
        public float BuffTimeRemaining => _state == SouffleState.Buffing ? _buffTimer : 0f;

        public bool TryStartMeditation(GameState state)
        {
            if (state == null) return false;
            if (_state != SouffleState.Idle) return false;
            if (_cooldownTimer > 0f) return false;

            _state = SouffleState.Meditating;
            _meditationTimer = SouffleConstants.MeditationDurationSeconds;
            // Cooldown starts at meditation begin so the total Souffle "loop" honors the spec.
            _cooldownTimer = SouffleConstants.CooldownSeconds;
            state.lastSouffleTime = Time.time;
            GameManager.Instance?.Save?.MarkDirty();

            GameEvents.RaiseSouffleStarted();
            return true;
        }

        public void Tick(GameState state, float dt)
        {
            if (state == null) return;

            // Drive cooldown countdown regardless of state.
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= dt;
                if (_cooldownTimer < 0f) _cooldownTimer = 0f;
                GameEvents.RaiseSouffleCooldownUpdated(_cooldownTimer, SouffleConstants.CooldownSeconds);
            }

            switch (_state)
            {
                case SouffleState.Meditating:
                    _meditationTimer -= dt;
                    if (_meditationTimer <= 0f)
                    {
                        _state = SouffleState.Buffing;
                        _buffTimer = SouffleConstants.BuffDurationSeconds;
                        state.souffleBuffActiveUntil = Time.time + _buffTimer;
                        GameEvents.RaiseSouffleEnded();
                        GameEvents.RaiseSouffleBuffStarted(SouffleConstants.BuffDurationSeconds);
                    }
                    break;

                case SouffleState.Buffing:
                    _buffTimer -= dt;
                    if (_buffTimer <= 0f)
                    {
                        _state = SouffleState.Idle;
                        state.souffleBuffActiveUntil = -1f;
                        GameEvents.RaiseSouffleBuffEnded();
                    }
                    break;
            }
        }
    }
}
