using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// World-space view for an engaged Capitaine (Sprint 5 placeholder).
    /// 90×150 procedural sprite tinted by phase color (from <see cref="CapitaineData.PhaseColors"/>),
    /// pulsing aura ring, shake on damage, enrage tint + shake at phase 3.
    /// </summary>
    [DisallowMultipleComponent]
    public class CapitaineWorldView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private SpriteRenderer _auraRenderer;
        [SerializeField] private float _shakeStrength = 0.14f;
        [SerializeField] private float _shakeDuration = 0.22f;

        public SpriteRenderer BodyRenderer { get => _bodyRenderer; set => _bodyRenderer = value; }
        public SpriteRenderer AuraRenderer { get => _auraRenderer; set => _auraRenderer = value; }

        private CapitaineData _data;
        private Tween _idleScale;
        private Tween _auraPulse;
        private Tween _enrageShake;

        private void Awake()
        {
            SetVisible(false, instant: true);
        }

        private void OnEnable()
        {
            GameEvents.OnCapitaineSpawned += HandleSpawned;
            GameEvents.OnAdversaireDamaged += HandleDamaged;
            GameEvents.OnCapitainePhaseChanged += HandlePhaseChanged;
            GameEvents.OnCapitaineEnraged += HandleEnraged;
            GameEvents.OnCapitaineDefeated += HandleDefeated;
            GameEvents.OnPhaseChanged += HandlePhaseTransition;
        }

        private void OnDisable()
        {
            GameEvents.OnCapitaineSpawned -= HandleSpawned;
            GameEvents.OnAdversaireDamaged -= HandleDamaged;
            GameEvents.OnCapitainePhaseChanged -= HandlePhaseChanged;
            GameEvents.OnCapitaineEnraged -= HandleEnraged;
            GameEvents.OnCapitaineDefeated -= HandleDefeated;
            GameEvents.OnPhaseChanged -= HandlePhaseTransition;
            _idleScale?.Kill();
            _auraPulse?.Kill();
            _enrageShake?.Kill();
        }

        private void HandleSpawned(CapitaineData data)
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
                _auraRenderer.transform.localScale = Vector3.one * 1.2f;
            }

            // Fade in body + aura
            DOTween.To(() => _bodyRenderer.color.a, a =>
            {
                if (_bodyRenderer == null) return;
                var c = _bodyRenderer.color; c.a = a; _bodyRenderer.color = c;
            }, 1f, 0.5f).SetEase(Ease.OutQuad).SetTarget(transform);

            if (_auraRenderer != null)
            {
                DOTween.To(() => _auraRenderer.color.a, a =>
                {
                    if (_auraRenderer == null) return;
                    var c = _auraRenderer.color; c.a = a; _auraRenderer.color = c;
                }, 0.4f, 0.5f).SetEase(Ease.OutQuad).SetTarget(transform);
            }

            // Idle body scale + aura pulse
            _idleScale?.Kill();
            _idleScale = transform.DOScale(1.03f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(transform);

            if (_auraRenderer != null)
            {
                _auraPulse?.Kill();
                _auraPulse = _auraRenderer.transform.DOScale(1.35f, 1.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetTarget(_auraRenderer.transform);
            }
        }

        private void HandleDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)
        {
            transform.DOComplete();
            transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 14, randomness: 90f, snapping: false, fadeOut: true);
        }

        private void HandlePhaseChanged(int prev, int next)
        {
            ApplyPhaseColor(next);
        }

        private void HandleEnraged()
        {
            // Permanent gentle shake while enraged
            _enrageShake?.Kill();
            _enrageShake = transform.DOShakePosition(99f, 0.05f, vibrato: 6, randomness: 90f, snapping: false, fadeOut: false)
                .SetTarget(transform)
                .SetLoops(-1);
        }

        private void HandleDefeated(CapitaineData data, BigDouble reward)
        {
            _idleScale?.Kill();
            _auraPulse?.Kill();
            _enrageShake?.Kill();
            transform.DOScale(0.7f, 0.5f).SetEase(Ease.InQuad).SetTarget(transform);
            FadeOut(0.6f);
        }

        private void HandlePhaseTransition(CombatPhase prev, CombatPhase next)
        {
            switch (next)
            {
                case CombatPhase.Training:
                    SetVisible(false, instant: false);
                    _enrageShake?.Kill();
                    break;
                case CombatPhase.PlayerDeathTemporary:
                    if (_bodyRenderer != null)
                    {
                        var c = _bodyRenderer.color; c.a = 0.4f; _bodyRenderer.color = c;
                    }
                    _idleScale?.Kill();
                    _enrageShake?.Kill();
                    break;
            }
        }

        private void ApplyPhaseColor(int phase)
        {
            if (_data == null || _data.PhaseColors == null || _data.PhaseColors.Length == 0) return;
            var idx = Mathf.Clamp(phase, 0, _data.PhaseColors.Length - 1);
            var color = _data.PhaseColors[idx];
            if (_bodyRenderer != null)
            {
                var bodyAlpha = _bodyRenderer.color.a;
                _bodyRenderer.color = new Color(color.r, color.g, color.b, bodyAlpha);
            }
            if (_auraRenderer != null)
            {
                var auraAlpha = _auraRenderer.color.a;
                _auraRenderer.color = new Color(color.r, color.g, color.b, auraAlpha);
            }
        }

        private void SetVisible(bool visible, bool instant)
        {
            var bodyTarget = visible ? 1f : 0f;
            var auraTarget = visible ? 0.4f : 0f;
            if (instant)
            {
                if (_bodyRenderer != null) { var c = _bodyRenderer.color; c.a = bodyTarget; _bodyRenderer.color = c; }
                if (_auraRenderer != null) { var c = _auraRenderer.color; c.a = auraTarget; _auraRenderer.color = c; }
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
                }, 0f, duration).SetEase(Ease.InQuad).SetTarget(transform);
            }
            if (_auraRenderer != null)
            {
                DOTween.To(() => _auraRenderer.color.a, a =>
                {
                    if (_auraRenderer == null) return; var c = _auraRenderer.color; c.a = a; _auraRenderer.color = c;
                }, 0f, duration).SetEase(Ease.InQuad).SetTarget(transform);
            }
        }

        // -- Procedural sprites ---------------------------------------

        private static Sprite _bodySprite;
        private static Sprite _auraSprite;

        private static Sprite GetOrCreateBodySprite()
        {
            if (_bodySprite != null) return _bodySprite;
            const int w = 90, h = 150;
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
            _bodySprite.name = "CapitaineBody";
            return _bodySprite;
        }

        private static Sprite GetOrCreateAuraSprite()
        {
            if (_auraSprite != null) return _auraSprite;
            const int s = 128;
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
                // Soft radial gradient, full white inside, alpha fades out.
                var alpha = (byte)Mathf.RoundToInt(t * t * 220f);
                pixels[y * s + x] = new Color32(255, 255, 255, alpha);
            }
            tex.SetPixels32(pixels); tex.Apply();
            _auraSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            _auraSprite.name = "CapitaineAura";
            return _auraSprite;
        }
    }
}
