using Saga.Core;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.5 — Stage chip label. Updates "Stade N" on <see cref="GameEvents.OnStadeChanged"/>.
    /// Used by <see cref="Saga.UI.Builders.StageBuilder"/> for the zone-3 stage progress chip.
    /// </summary>
    [DisallowMultipleComponent]
    public class StagePillView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        public TextMeshProUGUI Label { get => _label; set => _label = value; }

        private void OnEnable()
        {
            GameEvents.OnStadeChanged += HandleStadeChanged;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnStadeChanged -= HandleStadeChanged;
        }

        private void HandleStadeChanged(int previous, int next) => Refresh();

        private void Refresh()
        {
            if (_label == null) return;
            var gm = GameManager.Instance;
            var stade = gm?.State != null ? gm.State.currentStade : 1;
            _label.text = $"Stade {stade}";
        }
    }
}
