namespace Saga.Data
{
    /// <summary>
    /// Equipment rarity tiers per 04_PROGRESSION.md §Équipement.
    /// "Divin" was rejected in design — top tier is "Sacré" to stay in the mythique-humain ton.
    /// </summary>
    public enum Rarity
    {
        Commun = 0,
        Affute = 1,
        Legendaire = 2,
        Mythique = 3,
        Sacre = 4
    }
}
