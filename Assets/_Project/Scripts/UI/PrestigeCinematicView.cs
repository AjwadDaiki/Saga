using System.Collections;
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
    /// The big one (Sprint 6) — full prestige cinematic. 4 phases, ~17s total:
    ///  Phase 1 (5s)  : "TU ES MORT" + Maître name + defeat citation + drum hit
    ///  Phase 2 (4s)  : Voyage Intérieur — stats fade-in + Échos counter
    ///  Phase 3 (5s)  : Citation finale du joueur (prompt if absent, edit if exists)
    ///  Phase 4 (3s)  : Renaissance — fade to white + renaissance text → resume gameplay
    ///
    /// Subscribes to <see cref="GameEvents.OnPrestigeTriggered"/>. Calls
    /// <see cref="PrestigeService.CompletePrestige"/> at the end (resets RUN state).
    /// </summary>
    [DisallowMultipleComponent]
    public class PrestigeCinematicView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _root;
        [SerializeField] private Image _fadeImage;            // covers everything, fades black→clear
        [SerializeField] private TextMeshProUGUI _title;       // "TU ES MORT"
        [SerializeField] private TextMeshProUGUI _subtitle;    // "Face à Yoshitsune"
        [SerializeField] private TextMeshProUGUI _maitreCitation;
        [SerializeField] private TextMeshProUGUI _statsLine;
        [SerializeField] private TextMeshProUGUI _echosLabel;
        [SerializeField] private TextMeshProUGUI _heritageLabel;  // "Ton héritage:"
        [SerializeField] private TextMeshProUGUI _playerCitation;
        [SerializeField] private TextMeshProUGUI _renaissanceLabel;
        [SerializeField] private CitationInputModal _citationModal;

        public CanvasGroup Root { get => _root; set => _root = value; }
        public Image FadeImage { get => _fadeImage; set => _fadeImage = value; }
        public TextMeshProUGUI Title { get => _title; set => _title = value; }
        public TextMeshProUGUI Subtitle { get => _subtitle; set => _subtitle = value; }
        public TextMeshProUGUI MaitreCitation { get => _maitreCitation; set => _maitreCitation = value; }
        public TextMeshProUGUI StatsLine { get => _statsLine; set => _statsLine = value; }
        public TextMeshProUGUI EchosLabel { get => _echosLabel; set => _echosLabel = value; }
        public TextMeshProUGUI HeritageLabel { get => _heritageLabel; set => _heritageLabel = value; }
        public TextMeshProUGUI PlayerCitation { get => _playerCitation; set => _playerCitation = value; }
        public TextMeshProUGUI RenaissanceLabel { get => _renaissanceLabel; set => _renaissanceLabel = value; }
        public CitationInputModal CitationModal { get => _citationModal; set => _citationModal = value; }

        private void Awake() => HideInstant();

        private void OnEnable() => GameEvents.OnPrestigeTriggered += HandlePrestigeTriggered;
        private void OnDisable() => GameEvents.OnPrestigeTriggered -= HandlePrestigeTriggered;

        private void HandlePrestigeTriggered(MaitreData defeatedBy, BigDouble echosEarned)
        {
            StartCoroutine(RunCinematic(defeatedBy, echosEarned));
        }

        private IEnumerator RunCinematic(MaitreData defeatedBy, BigDouble echos)
        {
            var gm = GameManager.Instance;
            if (gm == null || _root == null) yield break;
            ShowAll(false);
            _root.alpha = 1f;
            _root.interactable = true;
            _root.blocksRaycasts = true;

            // ----- PHASE 1 — "TU ES MORT" (5s) ----------------------------
            // Fade to black
            if (_fadeImage != null)
            {
                _fadeImage.color = new Color(0f, 0f, 0f, 0f);
                yield return DOTween.To(() => _fadeImage.color.a, a =>
                {
                    if (_fadeImage == null) return;
                    var c = _fadeImage.color; c.a = a; _fadeImage.color = c;
                }, 1f, 1.2f).SetLink(gameObject, LinkBehaviour.KillOnDestroy).WaitForCompletion();
            }

            if (_title != null) { _title.text = "TU ES MORT"; _title.alpha = 0f; _title.gameObject.SetActive(true); }
            if (_subtitle != null) { _subtitle.text = defeatedBy != null ? $"Face à {defeatedBy.DisplayName}" : "Face à un Maître"; _subtitle.alpha = 0f; _subtitle.gameObject.SetActive(true); }
            if (_maitreCitation != null)
            {
                _maitreCitation.text = defeatedBy != null ? $"« {defeatedBy.DefeatCitation} »" : "";
                _maitreCitation.alpha = 0f;
                _maitreCitation.gameObject.SetActive(true);
            }

            yield return FadeTextIn(_title, 0.6f);
            yield return new WaitForSeconds(0.4f);
            yield return FadeTextIn(_subtitle, 0.5f);
            yield return new WaitForSeconds(0.5f);
            yield return FadeTextIn(_maitreCitation, 0.7f);
            yield return new WaitForSeconds(1.5f);

            // ----- PHASE 2 — Voyage Intérieur (4s) ------------------------
            yield return FadeTextOut(_title, 0.3f);
            yield return FadeTextOut(_subtitle, 0.3f);
            yield return FadeTextOut(_maitreCitation, 0.3f);

            if (_statsLine != null)
            {
                var prestigeNo = gm.State.prestigeCount;
                _statsLine.text = $"Stade atteint : {gm.State.currentStade}\n"
                                + $"Adversaires vaincus : {gm.State.totalAdversairesDefeated}\n"
                                + $"Capitaines vaincus : {gm.State.totalCapitainesDefeated}";
                _statsLine.alpha = 0f;
                _statsLine.gameObject.SetActive(true);
                yield return FadeTextIn(_statsLine, 0.5f);
                yield return new WaitForSeconds(1.6f);
            }

            if (_echosLabel != null)
            {
                _echosLabel.text = $"{NumberFormatter.Format(echos)} Échos accumulés";
                _echosLabel.alpha = 0f;
                _echosLabel.gameObject.SetActive(true);
                yield return FadeTextIn(_echosLabel, 0.6f);
                yield return new WaitForSeconds(1.4f);
                yield return FadeTextOut(_statsLine, 0.3f);
            }

            // ----- PHASE 3 — Citation finale du joueur (5s) ---------------
            if (_heritageLabel != null) { _heritageLabel.text = "Ton héritage :"; _heritageLabel.alpha = 0f; _heritageLabel.gameObject.SetActive(true); }
            yield return FadeTextIn(_heritageLabel, 0.4f);

            // If no citation, prompt; otherwise show + offer to edit.
            var needsPrompt = string.IsNullOrWhiteSpace(gm.State.playerCitation);
            if (needsPrompt && _citationModal != null)
            {
                var done = false;
                _citationModal.Open(
                    title: "Écris ta dernière phrase…",
                    initialText: string.Empty,
                    allowSkip: false,
                    onCompleted: text =>
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            gm.State.playerCitation = text.Trim();
                            gm.State.playerCitationLockedForRun = true;
                        }
                        done = true;
                    });
                while (!done) yield return null;
            }

            if (_playerCitation != null)
            {
                _playerCitation.text = $"« {gm.State.playerCitation} »";
                _playerCitation.alpha = 0f;
                _playerCitation.gameObject.SetActive(true);
                yield return FadeTextIn(_playerCitation, 0.6f);
                yield return new WaitForSeconds(2.5f);
            }

            yield return FadeTextOut(_heritageLabel, 0.3f);
            yield return FadeTextOut(_playerCitation, 0.4f);
            yield return FadeTextOut(_echosLabel, 0.3f);

            // ----- PHASE 4 — Renaissance (3s) -----------------------------
            // Fade to white
            if (_fadeImage != null)
            {
                yield return DOTween.To(() => _fadeImage.color, c =>
                {
                    if (_fadeImage == null) return; _fadeImage.color = c;
                }, new Color(1f, 1f, 1f, 1f), 1f).SetLink(gameObject, LinkBehaviour.KillOnDestroy).WaitForCompletion();
            }

            if (_renaissanceLabel != null)
            {
                _renaissanceLabel.text = "Tu renais. Que ta prochaine légende soit plus longue.";
                _renaissanceLabel.color = new Color(0.1f, 0.08f, 0.04f, 1f);
                _renaissanceLabel.alpha = 0f;
                _renaissanceLabel.gameObject.SetActive(true);
                yield return FadeTextIn(_renaissanceLabel, 0.5f);
                yield return new WaitForSeconds(1.8f);
                yield return FadeTextOut(_renaissanceLabel, 0.4f);
            }

            // Hand off to PrestigeService.CompletePrestige (resets RUN fields + raises OnPrestigeCompleted).
            gm.Prestige?.CompletePrestige(gm.State);

            // Fade overlay back to clear (gameplay reveal).
            if (_fadeImage != null)
            {
                yield return DOTween.To(() => _fadeImage.color, c =>
                {
                    if (_fadeImage == null) return; _fadeImage.color = c;
                }, new Color(1f, 1f, 1f, 0f), 0.8f).SetLink(gameObject, LinkBehaviour.KillOnDestroy).WaitForCompletion();
            }

            _root.interactable = false;
            _root.blocksRaycasts = false;
            _root.alpha = 0f;
        }

        private void ShowAll(bool visible)
        {
            foreach (var label in new[] { _title, _subtitle, _maitreCitation, _statsLine, _echosLabel, _heritageLabel, _playerCitation, _renaissanceLabel })
            {
                if (label != null) label.gameObject.SetActive(visible);
            }
        }

        private void HideInstant()
        {
            if (_root != null) { _root.alpha = 0f; _root.interactable = false; _root.blocksRaycasts = false; }
            if (_fadeImage != null) _fadeImage.color = new Color(0f, 0f, 0f, 0f);
            ShowAll(false);
        }

        private IEnumerator FadeTextIn(TextMeshProUGUI label, float dur)
        {
            if (label == null) yield break;
            yield return DOTween.To(() => label.alpha, a => { if (label == null) return; label.alpha = a; }, 1f, dur)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .WaitForCompletion();
        }

        private IEnumerator FadeTextOut(TextMeshProUGUI label, float dur)
        {
            if (label == null) yield break;
            yield return DOTween.To(() => label.alpha, a => { if (label == null) return; label.alpha = a; }, 0f, dur)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .WaitForCompletion();
        }
    }
}
