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
    /// Combat HUD: adversaire name + HP bar + chrono.
    /// Visible only during AdversaireActive (with a small lead-in during AdversaireIncoming).
    /// Chrono label turns red under 5 seconds remaining.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatHudView : MonoBehaviour
    {
        private static readonly Color ChronoNormal = new Color(0.98f, 0.98f, 0.98f, 1f);
        private static readonly Color ChronoDanger = new Color(0.93f, 0.30f, 0.27f, 1f);
        private const float ChronoDangerThreshold = 5f;

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
            GameEvents.OnAdversaireSpawned += HandleSpawned;
            GameEvents.OnAdversaireDamaged += HandleDamaged;
            GameEvents.OnChronoUpdated += HandleChrono;
            if (_group != null) _group.alpha = 0f;
        }

        private void OnDisable()
        {
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
            GameEvents.OnAdversaireSpawned -= HandleSpawned;
            GameEvents.OnAdversaireDamaged -= HandleDamaged;
            GameEvents.OnChronoUpdated -= HandleChrono;
        }

        private void HandlePhaseChanged(CombatPhase prev, CombatPhase next)
        {
            if (_group == null) return;
            // Visible during Incoming + Active (and through Victory celebration so you see the kill).
            var visible = next == CombatPhase.AdversaireIncoming
                       || next == CombatPhase.AdversaireActive
                       || next == CombatPhase.AdversaireVictory;
            var target = visible ? 1f : 0f;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, target, 0.25f).SetEase(Ease.OutQuad);
        }

        private void HandleSpawned(AdversaireData data)
        {
            if (data == null) return;
            _maxHp = data.Hp;
            if (_nameLabel != null) _nameLabel.text = data.DisplayName;
            UpdateHpDisplay(data.Hp, data.Hp);
            UpdateChronoDisplay(data.ChronoSeconds);
        }

        private void HandleDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)
        {
            _maxHp = maxHp;
            UpdateHpDisplay(currentHp, maxHp);
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
                // Ratio computed in double space — adversaires won't approach BigDouble precision limits.
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
