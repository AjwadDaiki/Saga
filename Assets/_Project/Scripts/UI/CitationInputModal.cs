using System;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Modal for the player's final citation (max 80 chars). Used at run start ("Écris ta première
    /// phrase…") and at death vs Maître ("Modifie ta dernière phrase…"). Sprint 6.
    /// </summary>
    [DisallowMultipleComponent]
    public class CitationInputModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _skipButton; // optional — skip allowed at run start, not at death

        public CanvasGroup Group { get => _group; set => _group = value; }
        public TextMeshProUGUI TitleLabel { get => _titleLabel; set => _titleLabel = value; }
        public TMP_InputField Input { get => _input; set => _input = value; }
        public Button ConfirmButton { get => _confirmButton; set => _confirmButton = value; }
        public Button SkipButton { get => _skipButton; set => _skipButton = value; }

        private Action<string> _onCompleted;
        private bool _allowSkip;

        private void Awake()
        {
            CloseInstant();
            if (_input != null)
            {
                _input.characterLimit = PrestigeConstants.PlayerCitationMaxLength;
                _input.onValueChanged.AddListener(OnTextChanged);
            }
            if (_confirmButton != null) _confirmButton.onClick.AddListener(Confirm);
            if (_skipButton != null) _skipButton.onClick.AddListener(Skip);
        }

        public void Open(string title, string initialText, bool allowSkip, Action<string> onCompleted)
        {
            if (_group == null) return;
            if (_titleLabel != null) _titleLabel.text = title;
            if (_input != null) _input.text = initialText ?? string.Empty;
            _allowSkip = allowSkip;
            if (_skipButton != null) _skipButton.gameObject.SetActive(allowSkip);
            _onCompleted = onCompleted;

            _group.alpha = 0f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, 1f, 0.25f)
                .SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            OnTextChanged(_input != null ? _input.text : string.Empty);
        }

        private void OnTextChanged(string text)
        {
            if (_confirmButton == null) return;
            _confirmButton.interactable = !string.IsNullOrWhiteSpace(text);
        }

        private void Confirm()
        {
            var text = _input != null ? _input.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(text)) return;
            _onCompleted?.Invoke(text);
            Close();
        }

        private void Skip()
        {
            if (!_allowSkip) return;
            _onCompleted?.Invoke(null);
            Close();
        }

        public void Close()
        {
            if (_group == null) return;
            DOTween.To(() => _group.alpha, a => _group.alpha = a, 0f, 0.2f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    if (_group == null) return;
                    _group.interactable = false;
                    _group.blocksRaycasts = false;
                })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void CloseInstant()
        {
            if (_group == null) return;
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }
    }
}
