namespace Saga.Data
{
    /// <summary>
    /// Visible character layers (Sprint 7 modular sprite system).
    /// Body is always present; Armor and Weapon are optional.
    /// Sprint 8+ extends with Helmet, Cape, Accessory.
    /// </summary>
    public enum EquipmentSlot
    {
        Body = 0,
        Armor = 1,
        Weapon = 2
    }
}
