using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 — basculement Mannequin ↔ Wolf selon CombatPhase.
    ///
    /// Phase Training       → Mannequin actif, Wolf inactif
    /// Phase AdversaireActive / CapitaineActive / MaitreActive → Wolf actif, Mannequin inactif
    ///
    /// Attaché par MainSceneBootstrap au boot designer mode, wiré avec refs Mannequin + Wolf
    /// trouvés via SceneRegistry. Subscribe OnPhaseChanged + applique state initial selon
    /// gm.State.currentPhase courant.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatTargetSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject _mannequin;
        [SerializeField] private GameObject _wolf;

        public void Configure(GameObject mannequin, GameObject wolf)
        {
            _mannequin = mannequin;
            _wolf = wolf;
            ApplyState(GameManager.Instance?.State?.currentPhase ?? CombatPhase.Training);
        }

        private void OnEnable()
        {
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(CombatPhase prev, CombatPhase next) => ApplyState(next);

        private void ApplyState(CombatPhase phase)
        {
            var isCombat = phase == CombatPhase.AdversaireActive
                        || phase == CombatPhase.CapitaineActive
                        || phase == CombatPhase.MaitreActive;
            if (_mannequin != null) _mannequin.SetActive(!isCombat);
            if (_wolf != null) _wolf.SetActive(isCombat);
            Debug.Log($"[CombatTargetSwitcher] Phase={phase} → Mannequin {(!isCombat ? "ON" : "OFF")}, Wolf {(isCombat ? "ON" : "OFF")}");
        }
    }
}
