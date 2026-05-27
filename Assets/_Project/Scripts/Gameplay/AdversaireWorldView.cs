using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// World-space sprite for the engaged adversaire (Sprint 4 — procedural placeholder).
    /// 60×100 px rectangle tinted per Voie, with subtle idle scale loop, shake-on-damage,
    /// fade-in on spawn, fade-out + final pose on victory, semi-translucent during death overlay.
    /// </summary>
    [DisallowMultipleComponent]
    public class AdversaireWorldView : MonoBehaviour
    {
        // Per-voie tint per GAME_DESIGN_v2 + 05_VISUAL_STYLE
        private static readonly Color TintNone      = new Color(0.62f, 0.62f, 0.62f, 1f); // gris
        private static readonly Color TintSamurai   = new Color(0.98f, 0.78f, 0.46f, 1f); // ambre
        private static readonly Color TintViking    = new Color(0.49f, 0.65f, 0.79f, 1f); // bleu acier
        private static readonly Color TintWuxia     = new Color(0.62f, 0.75f, 0.66f, 1f); // vert pâle
        private static readonly Color TintSpartiate = new Color(0.79f, 0.47f, 0.29f, 1f); // bronze
        private static readonly Color TintMongol    = new Color(0.58f, 0.66f, 0.61f, 1f);
        private static readonly Color TintSaladin   = new Color(0.85f, 0.65f, 0.28f, 1f);
        private static readonly Color TintAztec     = new Color(0.36f, 0.72f, 0.60f, 1f);
        private static readonly Color TintGaulois   = new Color(0.65f, 0.48f, 0.25f, 1f);

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private float _shakeStrength = 0.10f;
        [SerializeField] private float _shakeDuration = 0.18f;

        private Tween _idleScaleLoop;
        private Color _baseColor = Color.white;

        public SpriteRenderer Renderer { get => _renderer; set => _renderer = value; }

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            SetVisible(false, instant: true);
        }

        private void OnEnable()
        {
            GameEvents.OnAdversaireSpawned += HandleSpawned;
            GameEvents.OnAdversaireDamaged += HandleDamaged;
            GameEvents.OnAdversaireDefeated += HandleDefeated;
            GameEvents.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnAdversaireSpawned -= HandleSpawned;
            GameEvents.OnAdversaireDamaged -= HandleDamaged;
            GameEvents.OnAdversaireDefeated -= HandleDefeated;
            GameEvents.OnPhaseChanged -= HandlePhaseChanged;
            _idleScaleLoop?.Kill();
        }

        private void HandleSpawned(AdversaireData data)
        {
            if (_renderer == null || data == null) return;
            _renderer.sprite = GetOrCreateBodySprite();
            _baseColor = ColorForVoie(data.Voie);
            _renderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f);
            transform.localScale = Vector3.one;

            // Fade in
            DOTween.To(() => _renderer.color.a, a =>
            {
                if (_renderer == null) return;
                var c = _renderer.color; c.a = a; _renderer.color = c;
            }, 1f, 0.3f).SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            // Restart idle loop
            _idleScaleLoop?.Kill();
            _idleScaleLoop = transform.DOScale(1.03f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HandleDamaged(BigDouble damage, BigDouble currentHp, BigDouble maxHp)
        {
            transform.DOComplete();
            transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 12, randomness: 90f, snapping: false, fadeOut: true);
        }

        private void HandleDefeated(AdversaireData data, BigDouble reward)
        {
            _idleScaleLoop?.Kill();
            // Slump + fade out
            transform.DOScale(0.7f, 0.4f).SetEase(Ease.InQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            DOTween.To(() => _renderer.color.a, a =>
            {
                if (_renderer == null) return;
                var c = _renderer.color; c.a = a; _renderer.color = c;
            }, 0f, 0.5f).SetEase(Ease.InQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void HandlePhaseChanged(CombatPhase prev, CombatPhase next)
        {
            switch (next)
            {
                case CombatPhase.Training:
                    SetVisible(false, instant: false);
                    break;
                case CombatPhase.PlayerDeathTemporary:
                    // Semi-translucent freeze.
                    if (_renderer != null)
                    {
                        var c = _renderer.color; c.a = 0.4f; _renderer.color = c;
                    }
                    _idleScaleLoop?.Kill();
                    break;
            }
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (_renderer == null) return;
            if (instant)
            {
                var c = _renderer.color; c.a = visible ? 1f : 0f; _renderer.color = c;
                return;
            }
            var target = visible ? 1f : 0f;
            DOTween.To(() => _renderer.color.a, a =>
            {
                if (_renderer == null) return;
                var c = _renderer.color; c.a = a; _renderer.color = c;
            }, target, 0.3f).SetEase(Ease.OutQuad).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private static Color ColorForVoie(Voie voie)
        {
            switch (voie)
            {
                case Voie.Samurai:   return TintSamurai;
                case Voie.Viking:    return TintViking;
                case Voie.Wuxia:     return TintWuxia;
                case Voie.Spartiate: return TintSpartiate;
                case Voie.Mongol:    return TintMongol;
                case Voie.Saladin:   return TintSaladin;
                case Voie.Aztec:     return TintAztec;
                case Voie.Gaulois:   return TintGaulois;
                default:             return TintNone;
            }
        }

        // -- Procedural body sprite -------------------------------------

        private static Sprite _bodySprite;

        private static Sprite GetOrCreateBodySprite()
        {
            if (_bodySprite != null) return _bodySprite;
            const int w = 60, h = 100;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    // Solid silhouette with a 1px transparent border so the tint reads cleanly.
                    var edge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    pixels[y * w + x] = edge ? new Color32(0, 0, 0, 0) : new Color32(255, 255, 255, 255);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            _bodySprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            _bodySprite.name = "AdversaireProcedural";
            return _bodySprite;
        }
    }
}
