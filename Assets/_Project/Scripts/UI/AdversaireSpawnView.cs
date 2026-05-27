using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Spawn cinematic overlay — shows the adversaire's name in a centered banner for 1s
    /// when AdversaireIncoming starts, then fades out. Doesn't block input (CanvasGroup with
    /// blocksRaycasts=false).
    /// </summary>
    [DisallowMultipleComponent]
    public class AdversaireSpawnView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _nameLabel;

        [SerializeField] private float _fadeIn = 0.25f;
        [SerializeField] private float _hold = 1.0f;
        [SerializeField] private float _fadeOut = 0.3f;

        public CanvasGroup Group { get => _group; set => _group = value; }
        public TextMeshProUGUI NameLabel { get => _nameLabel; set => _nameLabel = value; }

        private Sequence _running;

        private void Awake()
        {
            if (_group != null) _group.alpha = 0f;
        }

        private void OnEnable() => GameEvents.OnAdversaireSpawned += HandleSpawned;
        private void OnDisable() => GameEvents.OnAdversaireSpawned -= HandleSpawned;

        private void HandleSpawned(AdversaireData data)
        {
            if (_group == null || _nameLabel == null || data == null) return;
            _nameLabel.text = data.DisplayName;

            _running?.Kill();
            _group.alpha = 0f;

            _running = DOTween.Sequence();
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, _fadeIn).SetEase(Ease.OutQuad));
            _running.AppendInterval(_hold);
            _running.Append(DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, _fadeOut).SetEase(Ease.InQuad));
            _running.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
