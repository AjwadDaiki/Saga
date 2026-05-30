using Saga.Core;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.6 polish — bridges <see cref="GameEvents.OnElanChanged"/> to a
    /// <see cref="ReadyGlowView"/> so the VAGUE button halo pulses while Élan is at max.
    /// Lives on the VagueButton GameObject. Self-unsubscribes on disable / destroy.
    /// </summary>
    [DisallowMultipleComponent]
    public class VagueReadyGlowBridge : MonoBehaviour
    {
        private ReadyGlowView _glow;

        public void Setup(ReadyGlowView glow) => _glow = glow;

        private void OnEnable()
        {
            GameEvents.OnElanChanged += HandleElanChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnElanChanged -= HandleElanChanged;
        }

        private void HandleElanChanged(float current, float max)
        {
            if (_glow == null) return;
            _glow.SetReady(max > 0f && current >= max - 0.001f);
        }
    }
}
