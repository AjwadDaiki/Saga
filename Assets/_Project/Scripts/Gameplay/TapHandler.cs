using System.Collections.Generic;
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

        // Reused buffer for the RaycastAll-based UI check (avoids per-tap GC alloc).
        // Static OK since OnTapPerformed runs on the main thread synchronously.
        private static readonly List<RaycastResult> _uiRaycastBuffer = new List<RaycastResult>();

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
            // Resolve pointer position up-front; we need it for the UI raycast filter AND for FX events.
            var screenPos = Pointer.current != null ? Pointer.current.position.ReadValue() : (Vector2)Input.mousePosition;

            // Filter UI clicks (upgrade buttons + Vague button + death overlay etc.).
            // Unity 6 deprecates EventSystem.IsPointerOverGameObject() from inside InputAction
            // callbacks (queries last frame's UI state). Replacement: manual RaycastAll with a
            // fresh PointerEventData. See coordinator brief 2026-05-27.
            if (IsPointerOverUI(screenPos)) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;

            // Sprint 6: phase gating expanded with MaitreActive. Sprint 5 added CapitaineActive.
            // Taps are also blocked while Souffle is in the Meditating phase (player is mid-medit).
            var phase = gm.State.currentPhase;
            var phaseAccepts = phase == Saga.Data.CombatPhase.Training
                            || phase == Saga.Data.CombatPhase.AdversaireActive
                            || phase == Saga.Data.CombatPhase.CapitaineActive
                            || phase == Saga.Data.CombatPhase.MaitreActive;
            if (!phaseAccepts) return;
            if (gm.Souffle != null && gm.Souffle.IsMeditating) return;

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

            // Souffle buff: ×1.5 Force (Training only — combat damage isn't boosted by Souffle Sprint 6).
            if (phase == Saga.Data.CombatPhase.Training && gm.Souffle != null && gm.Souffle.IsBuffActive)
            {
                value = value * Saga.Data.SouffleConstants.BuffForceMultiplier;
            }

            if (phase == Saga.Data.CombatPhase.Training)
            {
                gm.State.force += value;
                // Track max force for prestige Échos calc.
                if (gm.State.force > gm.State.currentRunForceMax) gm.State.currentRunForceMax = gm.State.force;
                GameEvents.RaiseForceChanged();
            }
            // In AdversaireActive/CapitaineActive/MaitreActive: DamageDealer applies `value` as damage. No Force here.

            gm.State.totalTaps++;
            gm.Save?.MarkDirty();

            GameEvents.RaiseTapResolved(value, finalMult, screenPos);
        }

        private static bool IsPointerOverUI(Vector2 screenPosition)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            // Fresh PointerEventData — doesn't reuse the engine's stale processing state.
            var data = new PointerEventData(es) { position = screenPosition };
            _uiRaycastBuffer.Clear();
            es.RaycastAll(data, _uiRaycastBuffer);
            return _uiRaycastBuffer.Count > 0;
        }
    }
}
