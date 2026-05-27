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
            // Filter UI clicks (upgrade buttons + death overlay etc.).
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;

            // Sprint 4: phase gating. Only Training and AdversaireActive accept taps.
            //   - AdversaireIncoming / Victory / PlayerDeathTemporary = cinematic phases, no input.
            // Same combo math runs either way (so combo doesn't reset crossing phases).
            var phase = gm.State.currentPhase;
            if (phase != Saga.Data.CombatPhase.Training && phase != Saga.Data.CombatPhase.AdversaireActive) return;

            var tierMult = _combo.RegisterTap();
            var bonus = StatsCalculator.GetComboMultiplierBonus(gm.State, gm.Content);
            var finalMult = tierMult * bonus;

            var baseGain = StatsCalculator.GetForcePerTap(gm.State, gm.Content);
            var value = baseGain * finalMult;

            if (phase == Saga.Data.CombatPhase.Training)
            {
                // In Training: the value is Force gained. DamageDealer ignores OnTapResolved out of combat.
                gm.State.force += value;
                GameEvents.RaiseForceChanged();
            }
            // In AdversaireActive: DamageDealer applies `value` as damage to currentAdversaireHp.
            // No Force is added per tap — reward comes from AdversaireDefeated.

            gm.State.totalTaps++;
            gm.Save?.MarkDirty();

            var screenPos = Pointer.current != null ? Pointer.current.position.ReadValue() : (Vector2)Input.mousePosition;
            GameEvents.RaiseTapResolved(value, finalMult, screenPos);
        }
    }
}
