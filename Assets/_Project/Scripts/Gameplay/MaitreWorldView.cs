using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// World-space view for an engaged Maître (Sprint 6 placeholder). 120×180 procedural sprite +
    /// THICK radial aura (scale 1.5× body) tinted by current phase. Permanent enrage shake at 25% HP.
    /// </summary>
    [DisallowMultipleComponent]
    public class MaitreWorldView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private SpriteRenderer _auraRenderer;
        [SerializeField] private float _shakeStrength = 0.18f;
        [SerializeField] private float _shakeDuration = 0.25f;

        public SpriteRenderer BodyRenderer { get => _bodyRenderer; set => _bodyRenderer = value; }
        public SpriteRenderer AuraRenderer { get => _auraRenderer; set => _auraRenderer = value; }

        private MaitreData _data;
        private Tween _idleScale;
        private Tween _auraPulse;
        private Tween _enrageShake;

        private void Awake() => SetVisible(false, instant: true);

        private void OnEnable()
        {
            GameEvents.OnMaitreSpawned += HandleSpawned;
            GameEvents.OnAdversaireDamaged += HandleDamaged;
            GameEvents.OnMaitrePhaseChanged += HandlePhaseChanged;
            GameEvents.OnCapitaineEnraged += HandleEnraged; // reused enrage event Sprint 6 MVP
            GameEvents.OnMaitreDefeated += HandleDefeated;
            GameEvents.OnPhaseChanged += HandlePhaseTransition;
        }

        private void OnDisable()
        {
            GameEvents.OnMaitreSpawned -= HandleSpawned;
            GameEvents.OnAdversaireDamaged -= HandleDamaged;
            GameEvents.OnMaitrePhaseChanged -= HandlePhaseChanged;
            GameEvents.OnCapitaineEnraged -= HandleEnraged;
            GameEvents.OnMaitreDefeated -= HandleDefeated;
            GameEvents.OnPhaseChanged -= HandlePhaseTransition;
            _idleScale?.Kill();
            _auraPulse?.Kill();
            _enrageShake?.Kill();
        }

        private void HandleSpawned(MaitreData data)
        {
            if (data == null || _bodyRenderer == null) return;
            _data = data;
            _bodyRenderer.sprite = GetOrCreateBodySprite();
            ApplyPhaseColor(0);
            _bodyRenderer.color = new Color(_bodyRenderer.color.r, _bodyRenderer.color.g, _bodyRenderer.color.b, 0f);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            if (_auraRenderer != null)
            {
                _auraRenderer.sprite = GetOrCreateAuraSprite();
                _auraRenderer.color = new Color(_bodyRenderer.color.r, _bodyRenderer.color.g, _bodyRenderer.color.b, 0f);
                _auraRenderer.transform.localScale = Vector3.one * 1.5f; // épais
            }

            DOTween.To(() => _bodyRenderer.color.a, a =>
            {
                if (_bodyRenderer == null) return;
                var c = _bodyRenderer.color; c.a = a; _bodyRenderer.color = c;
            }, 1f, 0.6f).SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            if (_auraRenderer != null)
            {
                DOTween.To(() => _auraRenderer.color.a, a =>
                {
                    if (_auraRenderer == null) return;
                    var c = _auraRenderer.color; c.a = a; _auraRenderer.color = c;
                }, 0.55f, 0.6f).SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }

            _idleScale?.Kill();
            _idleScale = transform.DOScale(1.04f, 1.4f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            if (_auraRenderer != null)
            {
                _auraPulse?.Kill();
                _auraPulse = _auraRenderer.transform.DOScale(1.8f, 1.4f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        private void HandleDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)
        {
            // Only react during MaitreActive — adversaire/capitaine views handle their own phases.
            var gm = GameManager.Instance;
            if (gm?.State == null) return;
            if (gm.State.currentPhase != CombatPhase.MaitreActive) return;

            transform.DOComplete();
            transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 16, randomness: 90f, snapping: false, fadeOut: true)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HandlePhaseChanged(int prev, int next) => ApplyPhaseColor(next);

        private void HandleEnraged()
        {
            var gm = GameManager.Instance;
            if (gm?.State == null) return;
            if (gm.State.currentPhase != CombatPhase.MaitreActive) return;

            _enrageShake?.Kill();
            _enrageShake = transform.DOShakePosition(99f, 0.08f, vibrato: 8, randomness: 90f, snapping: false, fadeOut: false)
                .SetLoops(-1).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HandleDefeated(MaitreData data, BigDouble reward)
        {
            _idleScale?.Kill();
            _auraPulse?.Kill();
            _enrageShake?.Kill();
            transform.DOScale(0.7f, 0.6f).SetEase(Ease.InQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            FadeOut(0.7f);
        }

        private void HandlePhaseTransition(CombatPhase prev, CombatPhase next)
        {
            // Hide when leaving Maître phases entirely.
            if (next == CombatPhase.Training)
            {
                _enrageShake?.Kill();
                FadeOut(0.3f);
            }
            else if (next == CombatPhase.PlayerDeathTemporary)
            {
                // Player died vs Maître — leave the boss visible but translucent for the cinematic.
                if (_bodyRenderer != null) { var c = _bodyRenderer.color; c.a = 0.4f; _bodyRenderer.color = c; }
                _idleScale?.Kill();
                _enrageShake?.Kill();
            }
        }

        private void ApplyPhaseColor(int phase)
        {
            if (_data == null || _data.PhaseColors == null || _data.PhaseColors.Length == 0) return;
            var idx = Mathf.Clamp(phase, 0, _data.PhaseColors.Length - 1);
            var color = _data.PhaseColors[idx];
            if (_bodyRenderer != null)
            {
                var alpha = _bodyRenderer.color.a;
                _bodyRenderer.color = new Color(color.r, color.g, color.b, alpha);
            }
            if (_auraRenderer != null)
            {
                var alpha = _auraRenderer.color.a;
                _auraRenderer.color = new Color(color.r, color.g, color.b, alpha);
            }
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (instant)
            {
                if (_bodyRenderer != null) { var c = _bodyRenderer.color; c.a = visible ? 1f : 0f; _bodyRenderer.color = c; }
                if (_auraRenderer != null) { var c = _auraRenderer.color; c.a = visible ? 0.55f : 0f; _auraRenderer.color = c; }
                return;
            }
            FadeOut(0.3f);
        }

        private void FadeOut(float duration)
        {
            if (_bodyRenderer != null)
            {
                DOTween.To(() => _bodyRenderer.color.a, a =>
                {
                    if (_bodyRenderer == null) return; var c = _bodyRenderer.color; c.a = a; _bodyRenderer.color = c;
                }, 0f, duration).SetEase(Ease.InQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
            if (_auraRenderer != null)
            {
                DOTween.To(() => _auraRenderer.color.a, a =>
                {
                    if (_auraRenderer == null) return; var c = _auraRenderer.color; c.a = a; _auraRenderer.color = c;
                }, 0f, duration).SetEase(Ease.InQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        // -- Procedural sprites ---------------------------------------

        private static Sprite _bodySprite;
        private static Sprite _auraSprite;

        private static Sprite GetOrCreateBodySprite()
        {
            if (_bodySprite != null) return _bodySprite;
            const int w = 120, h = 180;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var edge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                pixels[y * w + x] = edge ? new Color32(0, 0, 0, 0) : new Color32(255, 255, 255, 255);
            }
            tex.SetPixels32(pixels); tex.Apply();
            _bodySprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            _bodySprite.name = "MaitreBody";
            return _bodySprite;
        }

        private static Sprite GetOrCreateAuraSprite()
        {
            if (_auraSprite != null) return _auraSprite;
            const int s = 192;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[s * s];
            var center = (s - 1) * 0.5f;
            var maxR = s * 0.5f;
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var dx = x - center; var dy = y - center;
                var d = Mathf.Sqrt(dx * dx + dy * dy);
                var t = Mathf.Clamp01(1f - d / maxR);
                // Slightly tighter falloff for a denser-looking aura than the Capitaine version.
                var alpha = (byte)Mathf.RoundToInt(t * t * t * 240f);
                pixels[y * s + x] = new Color32(255, 255, 255, alpha);
            }
            tex.SetPixels32(pixels); tex.Apply();
            _auraSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            _auraSprite.name = "MaitreAura";
            return _auraSprite;
        }
    }
}
