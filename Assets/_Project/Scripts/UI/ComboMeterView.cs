using System.Globalization;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Top-right combo display. Visible only when combo tier > 0.
    /// Shows "xN.N" — the FINAL multiplier including Méditation bonus, not just the base tier.
    ///
    /// Sprint 10 V2 Polish 5 — tier-up animation : scale POP 0→1.5→1.0, tier colors
    /// (1 crème, 2 jauneReward, 3+ coralAction), outline glow pulse, idle micro-rotation
    /// Z ±2°, 8-particle burst depuis label.
    /// </summary>
    [DisallowMultipleComponent]
    public class ComboMeterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _group;

        private int _lastTier;
        private float _lastBaseMult = 1f;

        private RectTransform _labelRt;
        private Vector3 _labelScale0 = Vector3.one;
        private Tween _idleRotation;
        private Sequence _activePop;

        public TextMeshProUGUI Label
        {
            get => _label;
            set => _label = value;
        }

        public CanvasGroup Group
        {
            get => _group;
            set => _group = value;
        }

        private void OnEnable()
        {
            if (_label != null)
            {
                _labelRt = _label.rectTransform;
                _labelScale0 = _labelRt.localScale;
            }
            GameEvents.OnComboChanged += HandleComboChanged;
            GameEvents.OnUpgradePurchased += HandleUpgradePurchased;
            Refresh(force: true);
            StartIdleRotation();
        }

        private void OnDisable()
        {
            GameEvents.OnComboChanged -= HandleComboChanged;
            GameEvents.OnUpgradePurchased -= HandleUpgradePurchased;
            _idleRotation?.Kill();
            _activePop?.Kill();
            if (_labelRt != null) _labelRt.localScale = _labelScale0;
        }

        private void HandleComboChanged(int tier, float baseMultiplier)
        {
            var tierUp = tier > _lastTier;
            _lastTier = tier;
            _lastBaseMult = baseMultiplier;
            Refresh(force: false);
            if (tierUp) PopTierUp(tier);
        }

        private void HandleUpgradePurchased(string upgradeId, int newLevel)
        {
            // Méditation purchase tweaks bonus → refresh display even without tier change.
            Refresh(force: false);
        }

        private void Refresh(bool force)
        {
            if (_label == null) return;

            var gm = GameManager.Instance;
            var bonus = gm != null
                ? StatsCalculator.GetComboMultiplierBonus(gm.State, gm.Content)
                : 1f;
            var finalMult = _lastBaseMult * bonus;

            // Sprint 7.5 zone 4 — signature "×N COMBO" tilted (mockup) instead of plain "x1.0".
            _label.text = "×" + finalMult.ToString("0.0", CultureInfo.InvariantCulture) + " COMBO";

            // Sprint 10 V2 Polish 5 — tier color tint sur Label.
            var tokens = DesignTokens.Get();
            _label.color = ResolveTierColor(_lastTier, tokens);
            _label.outlineColor = tokens.navyContour;
            // Outline thickness scale with tier pour effet "glow build-up".
            _label.outlineWidth = Mathf.Clamp(0.20f + _lastTier * 0.07f, 0.20f, 0.50f);

            if (_group != null)
            {
                _group.alpha = _lastTier <= 0 ? 0f : Mathf.Lerp(0.6f, 1f, _lastTier / 3f);
            }
        }

        private void StartIdleRotation()
        {
            if (_labelRt == null) return;
            _idleRotation?.Kill();
            _labelRt.localEulerAngles = new Vector3(0, 0, -2f);
            _idleRotation = _labelRt.DOLocalRotate(new Vector3(0, 0, 2f), 2.4f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void PopTierUp(int tier)
        {
            if (_labelRt == null) return;
            _activePop?.Kill();
            _labelRt.localScale = _labelScale0 * 0.0f; // shrink down before POP

            var pop = DOTween.Sequence();
            pop.Append(_labelRt.DOScale(_labelScale0 * 1.5f, 0.15f).SetEase(Ease.OutBack, 3f));
            pop.Append(_labelRt.DOScale(_labelScale0, 0.10f).SetEase(Ease.OutQuad));
            pop.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            _activePop = pop;

            // Particles burst depuis label center.
            var tokens = DesignTokens.Get();
            SpawnTierParticles(ResolveTierColor(tier, tokens));
        }

        private void SpawnTierParticles(Color color)
        {
            if (_labelRt == null) return;
            const int count = 8;
            for (var i = 0; i < count; i++)
            {
                var angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.15f, 0.15f);
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var dotGo = new GameObject("ComboTierParticle",
                    typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                dotGo.transform.SetParent(_labelRt, false);
                var rt = (RectTransform)dotGo.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(10, 10);

                var img = dotGo.GetComponent<Image>();
                img.color = new Color(color.r, color.g, color.b, 0.95f);
                img.raycastTarget = false;
                var cg = dotGo.GetComponent<CanvasGroup>();
                cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false;

                var target = dir * Random.Range(60f, 100f);
                Object.Destroy(dotGo, 0.55f);
                DOTween.Sequence()
                    .Append(rt.DOLocalMove(new Vector3(target.x, target.y, 0f), 0.50f).SetEase(Ease.OutCubic))
                    .Join(DOTween.To(() => cg.alpha, a => { if (cg != null) cg.alpha = a; }, 0f, 0.50f).SetEase(Ease.InQuad))
                    .Join(rt.DOScale(0.3f, 0.50f).SetEase(Ease.InQuad))
                    .SetLink(dotGo, LinkBehaviour.KillOnDestroy);
            }
        }

        private static Color ResolveTierColor(int tier, DesignTokens tokens)
        {
            switch (tier)
            {
                case 0: return tokens.cremeText;
                case 1: return tokens.cremeText;
                case 2: return tokens.jauneReward;
                default: return tokens.coralAction; // tier 3+
            }
        }
    }
}
