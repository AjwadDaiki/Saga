using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Listens to <see cref="GameEvents.OnTapResolved"/> and spawns the floating number + dust.
    ///
    /// Phase-aware (Sprint 5):
    /// - Training : "+X" ambre at tap screen pos (Force gain feedback)
    /// - AdversaireActive / CapitaineActive : "-X" rouge at enemy screen pos (damage feedback)
    ///
    /// Dust burst spawns at the tap point regardless (tactile feedback always welcome).
    /// </summary>
    [DisallowMultipleComponent]
    public class TapFxSpawner : MonoBehaviour
    {
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Transform _enemyAnchor;

        // Tunables
        private const float DustDuration = 0.55f;
        private const float DustRadius = 70f;
        private const int DustCount = 5;

        private static readonly Color DustColor = new Color(0.98f, 0.78f, 0.46f, 0.8f);
        private static readonly Color DamageColor = new Color(0.93f, 0.30f, 0.27f, 1f); // rouge sang

        public void Init(Canvas canvas)
        {
            _canvas = canvas;
            _canvasRect = canvas.transform as RectTransform;
            Debug.Log($"[TapFxSpawner] Init OK — canvas '{canvas.name}' renderMode={canvas.renderMode} sortingOrder={canvas.sortingOrder}.");
        }

        /// <summary>Set by MainSceneBootstrap so combat damage numbers can spawn at the enemy.</summary>
        public Transform EnemyAnchor
        {
            get => _enemyAnchor;
            set => _enemyAnchor = value;
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

        private void HandleTapResolved(BigDouble value, float multiplier, Vector2 screenPos)
        {
            if (_canvas == null || _canvasRect == null)
            {
                Debug.LogWarning($"[TapFxSpawner] HandleTapResolved early-out — _canvas={_canvas != null} _canvasRect={_canvasRect != null}. Init() jamais appelé ?");
                return;
            }

            var phase = GameManager.Instance?.State?.currentPhase ?? CombatPhase.Training;
            var isCombat = phase == CombatPhase.AdversaireActive || phase == CombatPhase.CapitaineActive;
            Debug.Log($"[TapFxSpawner] HandleTapResolved fired — value={value}, phase={phase}, screenPos={screenPos}");

            // Floating number — Training "+X" at tap pos, Combat "-X" at enemy pos.
            if (isCombat && _enemyAnchor != null)
            {
                var enemyScreen = WorldToScreenWithCamera(_enemyAnchor.position);
                var local = ScreenToCanvasLocal(enemyScreen);
                local += new Vector2(Random.Range(-40f, 40f), Random.Range(20f, 60f));
                Debug.Log($"[TapFxSpawner] Spawning FloatingDamage at canvas local {local} (enemy screen {enemyScreen})");
                SpawnFloatingDamage(local, value);
            }
            else
            {
                var local = ScreenToCanvasLocal(screenPos);
                Debug.Log($"[TapFxSpawner] Spawning FloatingNumber at canvas local {local} (screen {screenPos})");
                SpawnFloatingNumber(local, value, _currentTier);
            }

            // Dust burst at tap point — same UX in both phases (tactile feedback).
            var dustLocal = ScreenToCanvasLocal(screenPos);
            SpawnDust(dustLocal);
        }

        private static Vector2 WorldToScreenWithCamera(Vector3 worldPos)
        {
            var cam = Camera.main;
            if (cam == null) return Vector2.zero;
            return cam.WorldToScreenPoint(worldPos);
        }

        private Vector2 ScreenToCanvasLocal(Vector2 screenPos)
        {
            // Sprint 9 fix R2 — pass camera for non-Overlay canvas modes (Ajwad's authored Canvas
            // peut être Screen Space - Camera ou World Space — null camera ne marche que pour Overlay).
            var cam = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main)
                : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, cam, out var local);
            return local;
        }

        private void SpawnFloatingNumber(Vector2 anchored, BigDouble gain, int tier)
        {
            var tokens = DesignTokens.Get();
            var go = new GameObject("FloatingNumber",
                typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(FloatingNumberView));
            go.transform.SetParent(_canvasRect, false);
            go.transform.SetAsLastSibling(); // R2 fix : render au-dessus de Background_Dojo authored
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchored;
            rt.sizeDelta = new Vector2(300, 80);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            // Sprint 7.5 zone 4 — JetBrains Mono Bold + outline charcoal (puffy text recipe).
            label.font = tokens.NumbersFont;
            label.fontSize = 56;
            label.fontStyle = FontStyles.Bold;
            label.outlineColor = tokens.navyContour;
            label.outlineWidth = 0.28f;
            label.text = "";

            var view = go.GetComponent<FloatingNumberView>();
            view.Label = label;
            view.Group = go.GetComponent<CanvasGroup>();
            view.Init(gain, tier);
            view.Play();
        }

        private void SpawnFloatingDamage(Vector2 anchored, BigDouble damage)
        {
            var tokens = DesignTokens.Get();
            var go = new GameObject("FloatingDamage",
                typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(FloatingNumberView));
            go.transform.SetParent(_canvasRect, false);
            go.transform.SetAsLastSibling();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchored;
            rt.sizeDelta = new Vector2(300, 80);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            // Sprint 7.5 zone 4 — JetBrains Mono Bold + outline charcoal pour damage numbers signature.
            label.font = tokens.NumbersFont;
            label.fontSize = 64;
            label.fontStyle = FontStyles.Bold;
            label.color = DamageColor;
            label.outlineColor = tokens.navyContour;
            label.outlineWidth = 0.30f;
            label.text = "-" + Saga.Math.NumberFormatter.Format(damage);

            // Tween directly on this damage label — bypass FloatingNumberView so we keep the red color.
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            var endLocal = new Vector3(anchored.x + Random.Range(-30f, 30f), anchored.y + 140f, rt.localPosition.z);

            UnityEngine.Object.Destroy(go, 0.9f); // belt-and-braces
            var seq = DOTween.Sequence();
            seq.Append(rt.DOLocalMove(endLocal, 0.8f).SetEase(Ease.OutCubic));
            seq.Join(DOTween.To(() => group.alpha, a => { if (group != null) group.alpha = a; }, 0f, 0.8f).SetEase(Ease.InQuad));
            seq.SetLink(go, LinkBehaviour.KillOnDestroy);
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
            go.transform.SetAsLastSibling();
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

            var endLocal = new Vector3(to.x, to.y, rt.localPosition.z);
            UnityEngine.Object.Destroy(go, DustDuration + 0.05f);

            var seq = DOTween.Sequence();
            seq.Append(rt.DOLocalMove(endLocal, DustDuration).SetEase(Ease.OutQuad));
            seq.Join(DOTween.To(() => cg.alpha, a => { if (cg != null) cg.alpha = a; }, 0f, DustDuration).SetEase(Ease.InQuad));
            seq.Join(rt.DOScale(0.4f, DustDuration).SetEase(Ease.InQuad));
            seq.SetLink(go, LinkBehaviour.KillOnDestroy);
        }
    }
}
