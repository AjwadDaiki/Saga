namespace Saga.Data
{
    /// <summary>
    /// Character visual tier per 04_PROGRESSION.md §"Tiers visuels".
    /// 6 stades total. No "Divin" tier — design decision verrouillée (cf DESIGN_DECISIONS_LOG.md).
    ///
    /// Thresholds (Force) per doc:
    /// - Mendiant   : 0
    /// - Apprenti   : ~1 000 (1e3)
    /// - Guerrier   : ~100 000 (1e5)
    /// - Maître     : ~10 000 000 (1e7) or prestige >= 1
    /// - Légende    : ~1 000 000 000 (1e9) or prestige >= 2
    /// - Mythe      : ~1e15 (1aa) or prestige >= 3
    ///
    /// Sprint 3 implements 1→2 visually (Apprenti sprite). 3+ deferred.
    /// </summary>
    public enum Stade
    {
        Mendiant = 1,
        Apprenti = 2,
        Guerrier = 3,
        Maitre = 4,
        Legende = 5,
        Mythe = 6
    }
}
