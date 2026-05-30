using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Audio
{
    /// <summary>
    /// MonoBehaviour bridge from <see cref="GameEvents"/> to <see cref="HapticService"/>.
    /// Strength matrix follows the Sprint 7.5 brief :
    ///   Light  : tap basic
    ///   Medium : combo tier up, upgrade purchased, button important
    ///   Heavy  : Vague trigger, Maître spawn, player death
    /// </summary>
    [DisallowMultipleComponent]
    public class HapticBindings : MonoBehaviour
    {
        private HapticService _haptic;
        private int _lastComboTier;

        public void Init(HapticService haptic) { _haptic = haptic; }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTap;
            GameEvents.OnComboChanged += HandleCombo;
            GameEvents.OnUpgradePurchased += HandleUpgrade;
            GameEvents.OnVagueTriggered += HandleVague;
            GameEvents.OnMaitreSpawned += HandleMaitre;
            GameEvents.OnPlayerDiedTemporary += HandleDeath;
            GameEvents.OnPrestigeTriggered += HandlePrestige;
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTap;
            GameEvents.OnComboChanged -= HandleCombo;
            GameEvents.OnUpgradePurchased -= HandleUpgrade;
            GameEvents.OnVagueTriggered -= HandleVague;
            GameEvents.OnMaitreSpawned -= HandleMaitre;
            GameEvents.OnPlayerDiedTemporary -= HandleDeath;
            GameEvents.OnPrestigeTriggered -= HandlePrestige;
        }

        private void HandleTap(BigDouble gain, float multiplier, Vector2 screenPos) => _haptic?.Light();
        private void HandleCombo(int tier, float baseMultiplier)
        {
            if (tier > _lastComboTier) _haptic?.Medium();
            _lastComboTier = tier;
        }
        private void HandleUpgrade(string upgradeId, int newLevel) => _haptic?.Medium();
        private void HandleVague(CombatPhase phase) => _haptic?.Heavy();
        private void HandleMaitre(MaitreData data) => _haptic?.Heavy();
        private void HandleDeath() => _haptic?.Heavy();
        private void HandlePrestige(MaitreData defeatedBy, BigDouble echos) => _haptic?.Heavy();
    }
}
