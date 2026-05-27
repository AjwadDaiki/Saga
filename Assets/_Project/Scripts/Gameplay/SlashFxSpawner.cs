using BreakInfinity;
using DG.Tweening;
using Saga.Core;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Spawns a procedural slash sprite between the character and the mannequin on each tap.
    /// Sprint 3: a programmatically-generated 8×2 white sprite scaled + rotated + faded via DOTween
    /// core shortcuts (no UI-module extensions needed). Sprint 4+ polish can swap for an animated
    /// frame-by-frame from the weapon-hit spritesheet.
    ///
    /// Lifecycle: scheduled <c>Destroy(go, duration + 0.05)</c> is the primary cleanup — guaranteed
    /// even if a DOTween callback misfires. Setting tween target = go.transform also kills tweens
    /// safely when Unity destroys the GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    public class SlashFxSpawner : MonoBehaviour
    {
        [SerializeField] private Transform _characterTransform;
        [SerializeField] private Transform _mannequinTransform;
        [SerializeField] private float _duration = 0.22f;
        [SerializeField] private float _initialScale = 0.3f;
        [SerializeField] private float _finalScale = 1.4f;

        // Cached procedural sprite so we don't allocate a texture per tap.
        private static Sprite _slashSprite;
        private static readonly Color _slashColor = new Color(0.99f, 0.78f, 0.46f, 0.9f); // accent-primary ambre

        public Transform CharacterTransform
        {
            get => _characterTransform;
            set => _characterTransform = value;
        }

        public Transform MannequinTransform
        {
            get => _mannequinTransform;
            set => _mannequinTransform = value;
        }

        private void OnEnable() => GameEvents.OnTapResolved += HandleTapResolved;
        private void OnDisable() => GameEvents.OnTapResolved -= HandleTapResolved;

        private void HandleTapResolved(BigDouble gain, float multiplier, Vector2 screenPos)
        {
            if (_characterTransform == null || _mannequinTransform == null) return;

            // Midpoint between character and mannequin in world space.
            var mid = (_characterTransform.position + _mannequinTransform.position) * 0.5f;
            var angle = Random.Range(-30f, 30f);
            var comboScale = Mathf.Lerp(1f, 1.6f, Mathf.InverseLerp(1f, 2f, multiplier));

            SpawnSlash(mid, angle, comboScale);
        }

        private void SpawnSlash(Vector3 worldPos, float baseAngle, float comboScale)
        {
            var go = new GameObject("SlashFx", typeof(SpriteRenderer));
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(0f, 0f, baseAngle);
            go.transform.localScale = Vector3.one * (_initialScale * comboScale);

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateSlashSprite();
            sr.color = _slashColor;
            sr.sortingOrder = 10;

            // Belt-and-braces destruction: Unity's native timed Destroy is the source of truth.
            // Whatever DOTween does (or fails to do), the GameObject is gone within duration+50ms.
            Destroy(go, _duration + 0.05f);

            // SetLink(go, KillOnDestroy) makes DOTween auto-kill the tween when Unity destroys the
            // GameObject — eliminates the "Target or field is missing/null" warning spam when the
            // timed Destroy races the tween's tail.
            go.transform.DOScale(Vector3.one * (_finalScale * comboScale), _duration)
                .SetEase(Ease.OutQuad)
                .SetLink(go, LinkBehaviour.KillOnDestroy);

            DOTween.To(
                () => sr != null ? sr.color.a : 0f,
                a =>
                {
                    if (sr == null) return;
                    var c = sr.color;
                    c.a = a;
                    sr.color = c;
                },
                0f,
                _duration)
                .SetEase(Ease.InQuad)
                .SetLink(go, LinkBehaviour.KillOnDestroy);
        }

        private static Sprite GetOrCreateSlashSprite()
        {
            if (_slashSprite != null) return _slashSprite;

            const int w = 8, h = 2;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(pixels);
            tex.Apply();

            _slashSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit: 8);
            _slashSprite.name = "SlashProcedural";
            return _slashSprite;
        }
    }
}
