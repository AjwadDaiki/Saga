using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Listens to <see cref="GameEvents.OnTapResolved"/> and spawns floating "+X" numbers
    /// + a lightweight dust burst at the tap screen position.
    ///
    /// Implementation note: dust uses 4 UI Image dots that radiate + fade via DOTween,
    /// not ParticleSystem — works inside ScreenSpaceOverlay Canvas without camera setup,
    /// and keeps the cost trivial for mobile (4 transforms per tap, GC-light via Destroy).
    /// </summary>
    [DisallowMultipleComponent]
    public class TapFxSpawner : MonoBehaviour
    {
        private Canvas _canvas;
        private RectTransform _canvasRect;

        // Tunables
        private const float DustDuration = 0.55f;
        private const float DustRadius = 70f;
        private const int DustCount = 5;

        private static readonly Color DustColor = new Color(0.98f, 0.78f, 0.46f, 0.8f); // accent-primary

        public void Init(Canvas canvas)
        {
            _canvas = canvas;
            _canvasRect = canvas.transform as RectTransform;
        }

        private void OnEnable()
        {
            GameEvents.OnTapResolved += HandleTapResolved;
            GameEvents.OnComboChanged += HandleComboChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTapResolved;
            GameEvents.OnComboChanged -= HandleComboChanged;
        }

        private int _currentTier;
        private void HandleComboChanged(int tier, float baseMultiplier) => _currentTier = tier;

        private void HandleTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            if (_canvas == null || _canvasRect == null) return;
            var local = ScreenToCanvasLocal(screenPos);
            SpawnFloatingNumber(local, gain, _currentTier);
            SpawnDust(local);
        }

        private Vector2 ScreenToCanvasLocal(Vector2 screenPos)
        {
            // Camera is null for ScreenSpaceOverlay canvas (per Unity docs).
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, null, out var local);
            return local;
        }

        private void SpawnFloatingNumber(Vector2 anchored, BigDouble gain, int tier)
        {
            var go = new GameObject("FloatingNumber",
                typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(FloatingNumberView));
            go.transform.SetParent(_canvasRect, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchored;
            rt.sizeDelta = new Vector2(300, 80);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 56;
            label.text = "";

            var view = go.GetComponent<FloatingNumberView>();
            view.Label = label;
            view.Group = go.GetComponent<CanvasGroup>();
            view.Init(gain, tier);
            view.Play();
        }

        private void SpawnDust(Vector2 anchored)
        {
            for (var i = 0; i < DustCount; i++)
            {
                var angle = (i / (float)DustCount) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
                var radiusJitter = DustRadius + Random.Range(-12f, 12f);
                var target = anchored + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radiusJitter;
                SpawnSingleDust(anchored, target);
            }
        }

        private void SpawnSingleDust(Vector2 from, Vector2 to)
        {
            var go = new GameObject("Dust", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            go.transform.SetParent(_canvasRect, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = from;
            rt.sizeDelta = new Vector2(8, 8);

            var img = go.GetComponent<Image>();
            img.color = DustColor;
            img.raycastTarget = false;

            var cg = go.GetComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // DOLocalMove + DOTween.To are core DOTween (DOTween.dll), independent of UI module asmdef.
            var endLocal = new Vector3(to.x, to.y, rt.localPosition.z);

            var seq = DOTween.Sequence();
            seq.Append(rt.DOLocalMove(endLocal, DustDuration).SetEase(Ease.OutQuad));
            seq.Join(DOTween.To(() => cg.alpha, a => cg.alpha = a, 0f, DustDuration).SetEase(Ease.InQuad));
            seq.Join(rt.DOScale(0.4f, DustDuration).SetEase(Ease.InQuad));
            seq.OnComplete(() => Destroy(go));
        }
    }
}
