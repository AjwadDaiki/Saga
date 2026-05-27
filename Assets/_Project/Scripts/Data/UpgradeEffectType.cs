namespace Saga.Data
{
    /// <summary>
    /// What an upgrade actually does. Each value maps to a code path in StatsCalculator
    /// (and eventually visual feedback in the card).
    /// </summary>
    public enum UpgradeEffectType
    {
        /// <summary>Add <c>EffectValue × level</c> base Force per tap. Linear scaling. (Frappe.)</summary>
        ForcePerTap = 0,

        /// <summary>Add <c>EffectValue × level</c> Force per second passive. Driven by DisciplesProcessor. (Disciple.)</summary>
        ForcePerSecond = 1,

        /// <summary>Add <c>EffectValue × level</c> (in absolute multiplier units, e.g. 0.05 = +5%) to the combo cap multiplier. (Méditation.)</summary>
        ComboMultiplierBonus = 2
    }
}
