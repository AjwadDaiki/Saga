using Saga.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Saga.Gameplay
{
    /// <summary>
    /// Captures pointer presses (mouse + touch) anywhere on screen, applies combo multiplier
    /// (base tier × Méditation bonus), commits gains to <see cref="GameManager.State"/>,
    /// raises events for UI/FX.
    ///
    /// Sprint 2: base gain pulled from <see cref="StatsCalculator.GetForcePerTap"/> (Frappe upgrades),
    /// final combo multiplier = base_tier × <see cref="StatsCalculator.GetComboMultiplierBonus"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class TapHandler : MonoBehaviour
    {
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
            // Filter UI clicks (upgrade buttons land in Sprint 2 — keep this in place).
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;

            // Base tier × Méditation bonus = effective combo multiplier this tap.
            var tierMult = _combo.RegisterTap();
            var bonus = StatsCalculator.GetComboMultiplierBonus(gm.State, gm.Content);
            var finalMult = tierMult * bonus;

            var baseGain = StatsCalculator.GetForcePerTap(gm.State, gm.Content);
            var gain = baseGain * finalMult;

            gm.State.force += gain;
            gm.State.totalTaps++;
            gm.Save?.MarkDirty();

            GameEvents.RaiseForceChanged();
            var screenPos = Pointer.current != null ? Pointer.current.position.ReadValue() : (Vector2)Input.mousePosition;
            GameEvents.RaiseTapResolved(gain, finalMult, screenPos);
        }
    }
}
