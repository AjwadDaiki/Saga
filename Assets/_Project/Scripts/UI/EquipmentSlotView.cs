using Saga.Data;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Lightweight tag component attached to each row in the inventory modal.
    /// Holds a reference to the SpriteLayerSet the row represents + whether it's the
    /// currently-equipped one. Lets EditMode tests introspect built rows without scraping
    /// the UI hierarchy by string.
    /// </summary>
    [DisallowMultipleComponent]
    public class EquipmentSlotView : MonoBehaviour
    {
        public SpriteLayerSet LayerSet { get; set; }
        public bool IsEquipped { get; set; }
    }
}
