namespace Saga.Data
{
    /// <summary>
    /// Combat schools (voies) per 03_CULTURES.md.
    /// <see cref="None"/> = culturally neutral adversaire (used for the first wave of Sprint 4
    /// content where most adversaires don't yet belong to a school).
    /// </summary>
    public enum Voie
    {
        None = 0,
        Samurai = 1,
        Viking = 2,
        Wuxia = 3,
        Spartiate = 4,
        Mongol = 5,
        Saladin = 6,
        Aztec = 7,
        Gaulois = 8
    }
}
