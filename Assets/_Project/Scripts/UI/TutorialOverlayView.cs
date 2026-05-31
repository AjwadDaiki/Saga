using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 8 Phase A — runtime view for the tutorial overlay. Subscribes to GameEvents
    /// tutorial channels and rebuilds the dim + ring + bubble + skip per step.
    ///
    /// Built by <see cref="Builders.TutorialOverlayBuilder"/>. Designed to be self-contained :
    /// the builder constructs only the root + this component, then this view materializes
    /// the rest on demand based on incoming OnTutorialStepShown events.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialOverlayView : MonoBehaviour
    {
        private const float FadeInDuration = 0.25f;
        private const float FadeOut = 0.20f;
        private const float DimAlphaTarget = 0.55f;
        private const float RingPulsePeriod = 1.0f;
        private const float RingMinScale = 1.00f;
        private const float RingMaxScale = 1.10f;
        private const float RingMinAlpha = 0.70f;
        private const float RingMaxAlpha = 1.00f;
        private const float BubbleVerticalGap = 28f;

        private RectTransform _root;
        private CanvasGroup _cg;
        private GameObject _dim;
        private GameObject _ring;
        private Image _ringImage;
        private GameObject _bubble;
        private TextMeshProUGUI _bubbleLabel;
        private GameObject _skipBtn;
        private Tween _ringPulse;
        private Tween _ringFade;

        public void Initialize(RectTransform root, CanvasGroup cg)
        {
            _root = root;
            _cg = cg;
            BuildDim();
            BuildSkipButton();
            // Hide initially; we fade in on first OnTutorialStepShown.
        }

        private void OnEnable()
        {
            GameEvents.OnTutorialStepShown += HandleStepShown;
            GameEvents.OnTutorialStepCompleted += HandleStepCompleted;
            GameEvents.OnTutorialFinished += HandleFinished;
        }

        private void OnDisable()
        {
            GameEvents.OnTutorialStepShown -= HandleStepShown;
            GameEvents.OnTutorialStepCompleted -= HandleStepCompleted;
            GameEvents.OnTutorialFinished -= HandleFinished;
            _ringPulse?.Kill();
            _ringFade?.Kill();
        }

        // ----- Event handlers -----

        private void HandleStepShown(TutorialStep step)
        {
            var target = ResolveTarget(step.anchor);
            if (target == null)
            {
                Debug.LogWarning($"[Tutorial] Anchor target '{step.anchor}' not found for step '{step.id}'. Skipping bubble + ring (joueur ne sera pas guidé sur ce step).");
            }

            PlaceRing(target);
            PlaceBubble(target, step.messageFr);
            FadeIn();
        }

        private void HandleStepCompleted(string stepId)
        {
            // Dismiss current bubble + ring (will be re-shown for next step if any).
            if (_bubble != null) _bubble.SetActive(false);
            if (_ring != null) _ring.SetActive(false);
        }

        private void HandleFinished()
        {
            // Fade out then destroy.
            if (_cg == null) { if (_root != null) Destroy(_root.gameObject); return; }
            UIFadeUtil.Fade(_dim != null ? _dim.GetComponent<Image>() : null, 0f, FadeOut);
            DOTween.To(() => _cg.alpha, a => _cg.alpha = a, 0f, FadeOut)
                .OnComplete(() => { if (_root != null) Destroy(_root.gameObject); })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        // ----- Builders for overlay sub-elements -----

        private void BuildDim()
        {
            _dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            _dim.transform.SetParent(_root, false);
            _dim.transform.SetAsFirstSibling();
            var rt = (RectTransform)_dim.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = _dim.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // alpha 0 — animé via FadeIn()
            img.raycastTarget = false; // joueur peut tap les targets
        }

        private void BuildSkipButton()
        {
            var tokens = Saga.Data.DesignTokens.Get();
            var catalog = Saga.Data.RhosGFXAssetCatalog.Get();

            _skipBtn = new GameObject("SkipButton",
                typeof(RectTransform), typeof(Image), typeof(Button));
            _skipBtn.transform.SetParent(_root, false);
            var rt = (RectTransform)_skipBtn.transform;
            rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-32f, 200f); // above bottom nav (192) + 8 gap
            rt.sizeDelta = new Vector2(220f, 56f);

            var img = _skipBtn.GetComponent<Image>();
            if (catalog != null && catalog.IsMaterialized)
            {
                img.sprite = catalog.round3DGrey1.standard;
                img.type = Image.Type.Sliced;
            }
            img.color = new Color(tokens.panelSombre.r, tokens.panelSombre.g, tokens.panelSombre.b, 0.92f);
            img.raycastTarget = true;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(_skipBtn.transform, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            var tmp = lblGo.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = tokens.DisplayFont;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = tokens.cremeText;
            tmp.text = "Passer le tuto";
            tmp.outlineColor = tokens.navyContour;
            tmp.outlineWidth = 0.22f;
            tmp.characterSpacing = 3f;
            tmp.raycastTarget = false;

            _skipBtn.GetComponent<Button>().onClick.AddListener(OnSkipClicked);
        }

        private void OnSkipClicked()
        {
            GameManager.Instance?.Tutorial?.Skip();
        }

        private void PlaceRing(RectTransform target)
        {
            if (_ring == null) BuildRing();
            _ring.SetActive(target != null);
            if (target == null) return;
            // Center the ring over the target's screen-space rect.
            var ringRt = (RectTransform)_ring.transform;
            var canvasRt = (RectTransform)_root;
            var targetCenter = WorldToCanvasLocal(target, canvasRt);
            var targetSize = target.rect.size;
            ringRt.anchorMin = new Vector2(0.5f, 0.5f);
            ringRt.anchorMax = new Vector2(0.5f, 0.5f);
            ringRt.pivot = new Vector2(0.5f, 0.5f);
            ringRt.anchoredPosition = targetCenter;
            ringRt.sizeDelta = new Vector2(
                Mathf.Max(targetSize.x, 80f) + 24f,
                Mathf.Max(targetSize.y, 80f) + 24f);
            StartRingPulse();
        }

        private void BuildRing()
        {
            _ring = new GameObject("HighlightRing", typeof(RectTransform), typeof(Image));
            _ring.transform.SetParent(_root, false);
            var img = _ring.GetComponent<Image>();
            // Procedural rounded outline ring tinted cyan DA (skyBlue per coordinateur Q4).
            img.sprite = PuffySprite.RoundedOutline(28, 6);
            img.type = Image.Type.Sliced;
            var tokens = Saga.Data.DesignTokens.Get();
            img.color = new Color(tokens.skyBlue.r, tokens.skyBlue.g, tokens.skyBlue.b, 0.85f);
            img.raycastTarget = false;
            _ringImage = img;
        }

        private void StartRingPulse()
        {
            if (_ringImage == null) return;
            _ringPulse?.Kill();
            _ringFade?.Kill();
            var ringRt = (RectTransform)_ring.transform;
            ringRt.localScale = Vector3.one * RingMinScale;
            _ringPulse = ringRt.DOScale(RingMaxScale, RingPulsePeriod * 0.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            var c = _ringImage.color;
            _ringImage.color = new Color(c.r, c.g, c.b, RingMinAlpha);
            _ringFade = UIFadeUtil.Fade(_ringImage, RingMaxAlpha, RingPulsePeriod * 0.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void PlaceBubble(RectTransform target, string messageFr)
        {
            if (_bubble == null) BuildBubble();
            _bubble.SetActive(true);
            _bubbleLabel.text = messageFr ?? string.Empty;

            var bubbleRt = (RectTransform)_bubble.transform;
            var canvasRt = (RectTransform)_root;
            if (target == null)
            {
                // Center fallback.
                bubbleRt.anchoredPosition = Vector2.zero;
                return;
            }
            var targetCenter = WorldToCanvasLocal(target, canvasRt);
            var targetSize = target.rect.size;
            // Place above if target is in lower half, else below.
            var bubbleHeight = bubbleRt.rect.height;
            var below = targetCenter.y > 0f; // upper half of canvas → bubble below
            var y = below
                ? targetCenter.y - targetSize.y * 0.5f - BubbleVerticalGap - bubbleHeight * 0.5f
                : targetCenter.y + targetSize.y * 0.5f + BubbleVerticalGap + bubbleHeight * 0.5f;
            bubbleRt.anchorMin = new Vector2(0.5f, 0.5f);
            bubbleRt.anchorMax = new Vector2(0.5f, 0.5f);
            bubbleRt.pivot = new Vector2(0.5f, 0.5f);
            bubbleRt.anchoredPosition = new Vector2(targetCenter.x, y);
        }

        private void BuildBubble()
        {
            var tokens = Saga.Data.DesignTokens.Get();
            var catalog = Saga.Data.RhosGFXAssetCatalog.Get();
            _bubble = new GameObject("SpeechBubble", typeof(RectTransform), typeof(Image));
            _bubble.transform.SetParent(_root, false);
            var rt = (RectTransform)_bubble.transform;
            rt.sizeDelta = new Vector2(640f, 130f);

            var img = _bubble.GetComponent<Image>();
            if (catalog != null && catalog.IsMaterialized)
            {
                img.sprite = catalog.frameBasicGrey;
                img.type = Image.Type.Sliced;
            }
            img.color = new Color(tokens.panelSombre.r, tokens.panelSombre.g, tokens.panelSombre.b, 0.95f);
            img.raycastTarget = false;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(_bubble.transform, false);
            var lblRt = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = new Vector2(24f, 16f); lblRt.offsetMax = new Vector2(-24f, -16f);
            _bubbleLabel = lblGo.GetComponent<TextMeshProUGUI>();
            _bubbleLabel.alignment = TextAlignmentOptions.Center;
            _bubbleLabel.font = tokens.DisplayFont;
            _bubbleLabel.fontSize = 24;
            _bubbleLabel.fontStyle = FontStyles.Bold;
            _bubbleLabel.color = tokens.cremeText;
            _bubbleLabel.outlineColor = tokens.navyContour;
            _bubbleLabel.outlineWidth = 0.20f;
            _bubbleLabel.characterSpacing = 3f;
            _bubbleLabel.raycastTarget = false;
            _bubbleLabel.textWrappingMode = TextWrappingModes.Normal;
        }

        private void FadeIn()
        {
            if (_cg == null) return;
            DOTween.Kill(_cg);
            var cg = _cg;
            DOTween.To(() => cg.alpha, a => cg.alpha = a, 1f, FadeInDuration)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            var dimImg = _dim != null ? _dim.GetComponent<Image>() : null;
            if (dimImg != null)
            {
                UIFadeUtil.Fade(dimImg, DimAlphaTarget, FadeInDuration)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        // ----- Helpers -----

        private static RectTransform ResolveTarget(TutorialStep.AnchorTarget anchor)
        {
            var name = anchor switch
            {
                TutorialStep.AnchorTarget.CombatZone => "CombatZone",
                TutorialStep.AnchorTarget.ForcePill => "ForcePill",
                TutorialStep.AnchorTarget.UpgradeStrike => "Card_frappe",
                TutorialStep.AnchorTarget.StageChip => "StageChip",
                TutorialStep.AnchorTarget.VagueButton => "VagueButton",
                TutorialStep.AnchorTarget.SettingsButton => "SettingsButton",
                _ => null,
            };
            if (string.IsNullOrEmpty(name)) return null;
            var go = GameObject.Find(name);
            return go != null ? go.transform as RectTransform : null;
        }

        /// <summary>Convert target RectTransform world center → canvas-local coords.</summary>
        private static Vector2 WorldToCanvasLocal(RectTransform target, RectTransform canvas)
        {
            var worldCenter = (Vector2)target.TransformPoint(target.rect.center);
            // Canvas is ScreenSpaceOverlay → world == screen. Convert to canvas-local via inverse pivot.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas, worldCenter, null, out var local);
            return local;
        }
    }
}
