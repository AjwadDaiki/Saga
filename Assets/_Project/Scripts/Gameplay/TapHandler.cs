using BreakInfinity;
using Saga.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Saga.Gameplay
{
    /// <summary>
    /// Captures pointer presses (mouse + touch) anywhere on screen, applies combo multiplier,
    /// commits gains to <see cref="GameManager.State"/>, raises events for UI/FX.
    ///
    /// Sprint 1: full-screen tap zone (no upgrade buttons yet). When UI lands in Sprint 2,
    /// EventSystem filter blocks taps on UI elements automatically.
    /// </summary>
    [DisallowMultipleComponent]
    public class TapHandler : MonoBehaviour
    {
        /// <summary>Base gain per tap before combo multiplier. Replaced by upgrades formula in Sprint 2.</summary>
        public BigDouble BaseGainPerTap { get; set; } = new BigDouble(1);

        private InputAction _tapAction;
        private ComboSystem _combo;

        private void Awake()
        {
            _combo = new ComboSystem();
            _tapAction = new InputAction(name: "Tap", type: InputActionType.Button, binding: "<Pointer>/press");
            _tapAction.performed += OnTapPerformed;
        }

        private void OnEnable() => _tapAction?.Enable();
        private void OnDisable() => _tapAction?.Disable();

        private void OnDestroy()
        {
            if (_tapAction != null)
            {
                _tapAction.performed -= OnTapPerformed;
                _tapAction.Dispose();
            }
        }

        private void Update()
        {
            // ComboSystem timeout. Ticked here (not GameTicker) so the 1.5s window has frame-precise
            // resolution — feels tighter on touch.
            _combo?.Tick(Time.deltaTime);
        }

        private void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            // Filter UI clicks once UI buttons exist (Sprint 2+).
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;

            var multiplier = _combo.RegisterTap();
            var gain = BaseGainPerTap * multiplier;

            gm.State.force += gain;
            gm.State.totalTaps++;
            gm.Save?.MarkDirty();

            GameEvents.RaiseForceChanged();
            var screenPos = Pointer.current != null ? Pointer.current.position.ReadValue() : (Vector2)Input.mousePosition;
            GameEvents.RaiseTapResolved(gain, multiplier, screenPos);
        }
    }
}
