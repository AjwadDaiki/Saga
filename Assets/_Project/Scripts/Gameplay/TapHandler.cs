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

            // Sprint 5: phase gating. Only Training, AdversaireActive, CapitaineActive accept taps.
            //   - *Incoming / Victory / PlayerDeathTemporary = cinematic phases, no input.
            // Same combo math runs either way (so combo doesn't reset crossing phases).
            var phase = gm.State.currentPhase;
            var phaseAccepts = phase == Saga.Data.CombatPhase.Training
                            || phase == Saga.Data.CombatPhase.AdversaireActive
                            || phase == Saga.Data.CombatPhase.CapitaineActive;
            if (!phaseAccepts) return;

            var tierMult = _combo.RegisterTap();
            var bonus = StatsCalculator.GetComboMultiplierBonus(gm.State, gm.Content);
            var finalMult = tierMult * bonus;

            var baseGain = StatsCalculator.GetForcePerTap(gm.State, gm.Content);
            var value = baseGain * finalMult;

            // Vague Training buff: ×5 Force during the 5s window.
            if (phase == Saga.Data.CombatPhase.Training && gm.Vague != null && gm.Vague.IsTrainingBuffActive)
            {
                value = value * Saga.Data.ElanConstants.VagueTrainingForceMultiplier;
            }

            if (phase == Saga.Data.CombatPhase.Training)
            {
                gm.State.force += value;
                GameEvents.RaiseForceChanged();
            }
            // In AdversaireActive/CapitaineActive: DamageDealer applies `value` as damage. No Force here.

            gm.State.totalTaps++;
            gm.Save?.MarkDirty();

            var screenPos = Pointer.current != null ? Pointer.current.position.ReadValue() : (Vector2)Input.mousePosition;
            GameEvents.RaiseTapResolved(value, finalMult, screenPos);
        }
    }
}
