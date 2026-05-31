using Saga.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 9 — invisible click receiver for the combat tap area in Designer-First mode.
    ///
    /// Pourquoi : en Designer Mode, l'authored Background_Dojo couvre tout l'écran avec
    /// raycastTarget=true par défaut → la check <c>TapHandler.IsPointerOverUI</c> retourne
    /// toujours true → l'InputAction tap path est toujours rejeté. Aucun tap ne registre.
    ///
    /// Solution : un GO transparent en TOP sibling, raycastTarget=true, qui catch les clicks
    /// dans la zone combat et route vers <see cref="TapHandler.ProcessTap"/> (qui bypass la
    /// vérif IsPointerOverUI). Les boutons UI (Cards, Skills, Tabs) restent au-dessus en
    /// raycast order → ils continuent à recevoir leurs propres clicks.
    ///
    /// Posé par MainSceneBootstrap si Designer Mode actif. Lookup d'abord un GO "TapZone"
    /// authored par Ajwad ; si absent, auto-créé runtime dans la zone combat.
    /// </summary>
    [DisallowMultipleComponent]
    public class TapZoneHandler : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (TapHandler.Instance == null) return;
            TapHandler.Instance.ProcessTap(eventData.position);
        }
    }
}
