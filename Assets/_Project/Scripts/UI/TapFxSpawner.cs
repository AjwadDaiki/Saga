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
            Debug.Log($"[TapFxSpawner] OnEnable — subscribed OnTapResolved + OnComboChanged. GO active={gameObject.activeInHierarchy}.");
        }

        private void OnDisable()
        {
            GameEvents.OnTapResolved -= HandleTapResolved;
            GameEvents.OnComboChanged -= HandleComboChanged;
            Debug.Log($"[TapFxSpawner] OnDisable — UNSUBSCRIBED OnTapResolved. Si ce log appara avant un tap, c'est la cause de absence de FX.");
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

            // FEATURE 7 — fontSize scaled by combo tier (more impactful damage numbers on high combo).
            var tierFontBoost = Mathf.Clamp(_currentTier, 0, 3) * 8f;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            // Sprint 7.5 zone 4 — JetBrains Mono Bold + outline charcoal pour damage numbers signature.
            label.font = tokens.NumbersFont;
            label.fontSize = 64 + tierFontBoost; // tier 0=64, tier 3=88
            label.fontStyle = FontStyles.Bold;
            label.color = DamageTierColor(_currentTier);
            label.outlineColor = tokens.navyContour;
            label.outlineWidth = 0.30f + Mathf.Clamp(_currentTier, 0, 3) * 0.05f;
            label.text = "-" + Saga.Math.NumberFormatter.Format(damage);

            // FEATURE 7 — POP scale 0 → 1.3 → 1.0 + arc trajectory (quadratic Bezier via DOTween.To core).
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            rt.localScale = Vector3.zero;

            var startLocal = (Vector3)anchored;
            startLocal.z = rt.localPosition.z;
            var endAnchored = new Vector2(anchored.x + Random.Range(-50f, 50f), anchored.y + 160f);
            var endLocal = new Vector3(endAnchored.x, endAnchored.y, rt.localPosition.z);
            var apex = new Vector3((startLocal.x + endLocal.x) * 0.5f,
                Mathf.Max(startLocal.y, endLocal.y) + 40f, startLocal.z);

            UnityEngine.Object.Destroy(go, 1.0f); // belt-and-braces
            DOTween.Sequence()
                .Append(rt.DOScale(1.3f, 0.12f).SetEase(Ease.OutBack, 3f))
                .Append(rt.DOScale(1.0f, 0.08f).SetEase(Ease.OutQuad))
                .SetLink(go, LinkBehaviour.KillOnDestroy);

            var seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => 0f, t =>
            {
                var oneMinusT = 1f - t;
                var pos = oneMinusT * oneMinusT * startLocal
                    + 2f * oneMinusT * t * apex
                    + t * t * endLocal;
                rt.localPosition = pos;
            }, 1f, 0.80f).SetEase(Ease.OutCubic));
            seq.Join(DOTween.To(() => group.alpha, a => { if (group != null) group.alpha = a; }, 0f, 0.80f).SetEase(Ease.InQuad));
            seq.SetLink(go, LinkBehaviour.KillOnDestroy);
        }

        private static Color DamageTierColor(int tier)
        {
            switch (Mathf.Clamp(tier, 0, 3))
            {
                case 0: return DamageColor;
                case 1: return new Color(1.000f, 0.624f, 0.239f, 1f);     // orangeChaud
                case 2: return new Color(1.000f, 0.847f, 0.302f, 1f);     // jauneReward
                default: return new Color(1.000f, 0.420f, 0.420f, 1f);    // coralAction tier 3+
            }
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
