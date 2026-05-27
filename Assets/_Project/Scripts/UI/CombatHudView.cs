using System.Globalization;
using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using Saga.Math;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Combat HUD: enemy name + HP bar + chrono.
    /// Visible during Adversaire AND Capitaine phases.
    /// HP bar color reflects the Capitaine phase (Sprint 5): vert → jaune → orange → rouge enrage.
    /// Chrono label turns red under 5 seconds remaining.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatHudView : MonoBehaviour
    {
        private static readonly Color ChronoNormal = new Color(0.98f, 0.98f, 0.98f, 1f);
        private static readonly Color ChronoDanger = new Color(0.93f, 0.30f, 0.27f, 1f);
        private const float ChronoDangerThreshold = 5f;

        // HP bar color per capitaine phase (0..3).
        private static readonly Color HpAdversaire = new Color(0.85f, 0.30f, 0.25f, 0.95f); // sang
        private static readonly Color[] HpCapitainePhases =
        {
            new Color(0.40f, 0.78f, 0.46f, 0.95f), // vert  — phase 0 full
            new Color(0.95f, 0.80f, 0.32f, 0.95f), // jaune — phase 1
            new Color(0.98f, 0.55f, 0.27f, 0.95f), // orange — phase 2
            new Color(0.95f, 0.20f, 0.15f, 0.98f), // rouge — enrage
        };

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _hpLabel;
        [SerializeField] private TextMeshProUGUI _chronoLabel;
        [SerializeField] private Image _hpFill;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public TextMeshProUGUI NameLabel { get => _nameLabel; set => _nameLabel = value; }
        public TextMeshProUGUI HpLabel { get => _hpLabel; set => _hpLabel = value; }
        public TextMeshProUGUI ChronoLabel { get => _chronoLabel; set => _chronoLabel = value; }
        public Image HpFill { get => _hpFill; set => _hpFill = value; }

        private BigDouble _maxHp;

        private void OnEnable()
        {
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
            GameEvents.OnAdversaireSpawned += HandleAdversaireSpawned;
            GameEvents.OnCapitaineSpawned += HandleCapitaineSpawned;
            GameEvents.OnAdversaireDamaged += HandleDamaged;
            GameEvents.OnCapitainePhaseChanged += HandleCapitainePhaseChanged;
            GameEvents.OnChronoUpdated += HandleChrono;
        }

        private void OnDisable()
        {
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
            GameEvents.OnAdversaireSpawned -= HandleAdversaireSpawned;
            GameEvents.OnCapitaineSpawned -= HandleCapitaineSpawned;
            GameEvents.OnAdversaireDamaged -= HandleDamaged;
            GameEvents.OnCapitainePhaseChanged -= HandleCapitainePhaseChanged;
            GameEvents.OnChronoUpdated -= HandleChrono;
        }

        private void HandlePhaseChanged(CombatPhase prev, CombatPhase next)
        {
            if (_group == null) return;
            var visible = next == CombatPhase.AdversaireIncoming
                       || next == CombatPhase.AdversaireActive
                       || next == CombatPhase.AdversaireVictory
                       || next == CombatPhase.CapitaineIncoming
                       || next == CombatPhase.CapitaineActive
                       || next == CombatPhase.CapitaineVictory;
            var target = visible ? 1f : 0f;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, target, 0.25f).SetEase(Ease.OutQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HandleAdversaireSpawned(AdversaireData data)
        {
            if (data == null) return;
            _maxHp = data.Hp;
            if (_nameLabel != null) _nameLabel.text = data.DisplayName;
            if (_hpFill != null) _hpFill.color = HpAdversaire;
            UpdateHpDisplay(data.Hp, data.Hp);
            UpdateChronoDisplay(data.ChronoSeconds);
        }

        private void HandleCapitaineSpawned(CapitaineData data)
        {
            if (data == null) return;
            _maxHp = data.Hp;
            if (_nameLabel != null) _nameLabel.text = data.DisplayName.ToUpperInvariant();
            if (_hpFill != null) _hpFill.color = HpCapitainePhases[0];
            UpdateHpDisplay(data.Hp, data.Hp);
            UpdateChronoDisplay(data.ChronoSeconds);
        }

        private void HandleDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)
        {
            _maxHp = maxHp;
            UpdateHpDisplay(currentHp, maxHp);
        }

        private void HandleCapitainePhaseChanged(int prev, int next)
        {
            if (_hpFill == null) return;
            var idx = Mathf.Clamp(next, 0, HpCapitainePhases.Length - 1);
            _hpFill.color = HpCapitainePhases[idx];
        }

        private void HandleChrono(float remaining, float total)
        {
            UpdateChronoDisplay(remaining);
        }

        private void UpdateHpDisplay(BigDouble currentHp, BigDouble maxHp)
        {
            if (_hpLabel != null)
            {
                _hpLabel.text = $"{NumberFormatter.Format(currentHp)} / {NumberFormatter.Format(maxHp)}";
            }
            if (_hpFill != null)
            {
                var maxD = maxHp.ToDouble();
                var ratio = maxD > 0 ? Mathf.Clamp01((float)(currentHp.ToDouble() / maxD)) : 0f;
                _hpFill.fillAmount = ratio;
            }
        }

        private void UpdateChronoDisplay(float remaining)
        {
            if (_chronoLabel == null) return;
            var clamped = Mathf.Max(0f, remaining);
            var minutes = Mathf.FloorToInt(clamped / 60f);
            var seconds = Mathf.FloorToInt(clamped - minutes * 60f);
            _chronoLabel.text = $"{minutes}:{seconds.ToString("00", CultureInfo.InvariantCulture)}";
            _chronoLabel.color = clamped <= ChronoDangerThreshold ? ChronoDanger : ChronoNormal;
        }
    }
}
