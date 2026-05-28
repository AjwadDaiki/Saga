using BreakInfinity;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Audio
{
    /// <summary>
    /// MonoBehaviour glue: subscribes to <see cref="GameEvents"/> and dispatches the right
    /// <see cref="AudioService.Sfx"/> on each. Spawned by <see cref="GameManager"/> at boot.
    ///
    /// One-stop-shop for "what plays when" — the rest of the codebase never references
    /// AudioService directly, all coupling goes through events. Simplifies muting / disabling
    /// audio (just don't spawn the binding).
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioBindings : MonoBehaviour
    {
        private AudioService _audio;
        private int _lastComboTier;

        public void Init(AudioService audio)
        {
            _audio = audio;
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTap;
            GameEvents.OnComboChanged += HandleCombo;
            GameEvents.OnUpgradePurchased += HandleUpgrade;
            GameEvents.OnVagueTriggered += HandleVague;
            GameEvents.OnAdversaireSpawned += HandleAdversaire;
            GameEvents.OnCapitaineSpawned += HandleCapitaine;
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
            GameEvents.OnAdversaireSpawned -= HandleAdversaire;
            GameEvents.OnCapitaineSpawned -= HandleCapitaine;
            GameEvents.OnMaitreSpawned -= HandleMaitre;
            GameEvents.OnPlayerDiedTemporary -= HandleDeath;
            GameEvents.OnPrestigeTriggered -= HandlePrestige;
        }

        private void HandleTap(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            _audio?.Play(AudioService.Sfx.TapBasic, volumeScale: 0.85f);
        }

        private void HandleCombo(int tier, float baseMultiplier)
        {
            // Only ping on tier UP (not on reset to 0).
            if (tier > _lastComboTier) _audio?.Play(AudioService.Sfx.TapCombo);
            _lastComboTier = tier;
        }

        private void HandleUpgrade(string upgradeId, int newLevel) => _audio?.Play(AudioService.Sfx.UpgradeBuy);
        private void HandleVague(CombatPhase phase) => _audio?.Play(AudioService.Sfx.VagueTrigger);
        private void HandleAdversaire(AdversaireData data) => _audio?.Play(AudioService.Sfx.AdversaireArrive);
        private void HandleCapitaine(CapitaineData data) => _audio?.Play(AudioService.Sfx.CapitaineArrive);
        private void HandleMaitre(MaitreData data) => _audio?.Play(AudioService.Sfx.MaitreArrive);
        private void HandleDeath() => _audio?.Play(AudioService.Sfx.Death);
        private void HandlePrestige(MaitreData defeatedBy, BigDouble echos) => _audio?.Play(AudioService.Sfx.PrestigePhase);
    }
}
