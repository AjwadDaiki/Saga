using Saga.Core;

namespace Saga.Gameplay
{
    /// <summary>
    /// Tap combo state machine. Discrete 4-tier paliers per DESIGN_DECISIONS_LOG.md 2026-05-27.
    /// - 0-2 taps  → x1.0 (warmup)
    /// - 3-5 taps  → x1.2
    /// - 6-9 taps  → x1.5
    /// - 10+ taps  → x2.0 (cap)
    ///
    /// Combo resets if no tap registered within <see cref="WindowSeconds"/>.
    /// </summary>
    public sealed class ComboSystem
    {
        public const float WindowSeconds = 1.5f;

        // Lookup: [minTapCount, multiplier]. Sorted by minTapCount ascending.
        private static readonly (int MinTapCount, float Multiplier)[] _tiers =
        {
            (0,  1.0f),
            (3,  1.2f),
            (6,  1.5f),
            (10, 2.0f)
        };

        private int _tapCount;
        private float _secondsSinceLastTap;
        private int _lastEmittedTier = -1;

        /// <summary>Number of consecutive taps inside the combo window. Resets to 0 on expire.</summary>
        public int TapCount => _tapCount;

        /// <summary>Current multiplier applied to next tap gain.</summary>
        public float Multiplier => TierToMultiplier(CurrentTier);

        /// <summary>Current tier index (0..3). Use for UI styling.</summary>
        public int CurrentTier => GetTierForTapCount(_tapCount);

        /// <summary>Register a tap. Returns the multiplier that should apply to THIS tap's gain.</summary>
        public float RegisterTap()
        {
            _tapCount++;
            _secondsSinceLastTap = 0f;

            var tier = CurrentTier;
            var mult = Multiplier;
            if (tier != _lastEmittedTier)
            {
                _lastEmittedTier = tier;
                GameEvents.RaiseComboChanged(tier, mult);
            }

            return mult;
        }

        /// <summary>Drive timeout. Call from GameTicker each tick.</summary>
        public void Tick(float dt)
        {
            if (_tapCount == 0) return;
            _secondsSinceLastTap += dt;
            if (_secondsSinceLastTap >= WindowSeconds)
            {
                Reset();
            }
        }

        /// <summary>Manually reset the combo (e.g. on prestige, scene transition).</summary>
        public void Reset()
        {
            _tapCount = 0;
            _secondsSinceLastTap = 0f;
            if (_lastEmittedTier != 0)
            {
                _lastEmittedTier = 0;
                GameEvents.RaiseComboChanged(0, _tiers[0].Multiplier);
            }
        }

        private static int GetTierForTapCount(int tapCount)
        {
            // Walk down from highest tier; return index of first tier whose min is <= tapCount.
            for (var i = _tiers.Length - 1; i >= 0; i--)
            {
                if (tapCount >= _tiers[i].MinTapCount) return i;
            }
            return 0;
        }

        private static float TierToMultiplier(int tier)
        {
            if (tier < 0) return 1f;
            if (tier >= _tiers.Length) return _tiers[_tiers.Length - 1].Multiplier;
            return _tiers[tier].Multiplier;
        }
    }
}
