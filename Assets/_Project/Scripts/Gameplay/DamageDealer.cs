using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Routes tap damage to the engaged adversaire's HP during <see cref="CombatPhase.AdversaireActive"/>.
    /// Subscribes to <see cref="GameEvents.OnTapResolved"/> at construction. Single instance lives for
    /// the GameManager lifetime — no Dispose plumbing needed.
    ///
    /// Defeat detection lives in <see cref="CombatProcessor.Tick"/> (HP ≤ 0 check); DamageDealer just
    /// commits the damage value and raises the per-tap event.
    /// </summary>
    public sealed class DamageDealer
    {
        private readonly ContentDatabase _content;

        public DamageDealer(ContentDatabase content)
        {
            _content = content;
            GameEvents.OnTapResolved += HandleTapResolved;
        }

        private void HandleTapResolved(BigDouble damage, float multiplier, Vector2 screenPos)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null) return;
            if (gm.State.currentPhase != CombatPhase.AdversaireActive) return;

            var adversaire = _content?.GetAdversaire(gm.State.currentAdversaireId);
            if (adversaire == null) return;
            if (damage.Sign() <= 0) return;

            gm.State.currentAdversaireHp -= damage;
            if (gm.State.currentAdversaireHp.Sign() < 0) gm.State.currentAdversaireHp = new BigDouble(0);
            gm.Save?.MarkDirty();

            GameEvents.RaiseAdversaireDamaged(damage, gm.State.currentAdversaireHp, adversaire.Hp);
        }
    }
}
