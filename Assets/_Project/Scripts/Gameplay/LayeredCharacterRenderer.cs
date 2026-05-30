using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Sprint 7 modular sprite engine. Drives 3 stacked <see cref="SpriteRenderer"/>s
    /// (Body / Armor / Weapon) from independent <see cref="SpriteLayerSet"/>s, keeping
    /// them in lockstep on a single shared frame index.
    ///
    /// Why a custom driver instead of Unity Animator? Animator wants per-clip controllers
    /// and per-layer state graphs — too rigid for hot-swappable per-slot anims. This driver
    /// reads <c>SpriteLayerSet.GetFrames(animName)</c> on demand; any slot can omit any
    /// animation and the renderer falls back to that slot's idle.
    ///
    /// Looping policy follows rvros pack conventions:
    ///   loop: idle, meditation
    ///   one-shot: attack1, attack2, attack3, hurt, die
    /// One-shots that finish with a queued animation transition into it; otherwise the last
    /// frame is held (e.g. die).
    /// </summary>
    [DisallowMultipleComponent]
    public class LayeredCharacterRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private SpriteRenderer _armorRenderer;
        [SerializeField] private SpriteRenderer _weaponRenderer;

        [SerializeField] private SpriteLayerSet _bodyLayer;
        [SerializeField] private SpriteLayerSet _armorLayer;
        [SerializeField] private SpriteLayerSet _weaponLayer;

        [SerializeField] private string _initialAnimation = "idle";

        private string _currentAnimation = "idle";
        private string _queuedNext;
        private float _elapsed;
        private int _frameIndex;

        public SpriteRenderer BodyRenderer { get => _bodyRenderer; set => _bodyRenderer = value; }
        public SpriteRenderer ArmorRenderer { get => _armorRenderer; set => _armorRenderer = value; }
        public SpriteRenderer WeaponRenderer { get => _weaponRenderer; set => _weaponRenderer = value; }

        public SpriteLayerSet BodyLayer => _bodyLayer;
        public SpriteLayerSet ArmorLayer => _armorLayer;
        public SpriteLayerSet WeaponLayer => _weaponLayer;

        public string CurrentAnimation => _currentAnimation;
        public int CurrentFrameIndex => _frameIndex;

        private void Start()
        {
            // Snap initial frame so first visible state is anim[0] rather than null sprite.
            ApplyFrameToAllRenderers();
            PlayAnimation(_initialAnimation);
        }

        /// <summary>Replace one slot. Visual update is immediate (re-applies the current frame).</summary>
        public void SetLayer(EquipmentSlot slot, SpriteLayerSet layer)
        {
            switch (slot)
            {
                case EquipmentSlot.Body: _bodyLayer = layer; break;
                case EquipmentSlot.Armor: _armorLayer = layer; break;
                case EquipmentSlot.Weapon: _weaponLayer = layer; break;
            }
            ApplyFrameToAllRenderers();
        }

        /// <summary>
        /// Switch to <paramref name="animationId"/>. For one-shots, <paramref name="queueNext"/>
        /// is what plays after the last frame.
        /// </summary>
        public void PlayAnimation(string animationId, string queueNext = null)
        {
            if (animationId == null) return;
            // Don't restart a looping anim that's already running — keeps idle smooth across re-calls.
            if (_currentAnimation == animationId && IsLooping(animationId)) return;

            _currentAnimation = animationId;
            _queuedNext = queueNext;
            _frameIndex = 0;
            _elapsed = 0f;
            ApplyFrameToAllRenderers();
        }

        private void Update()
        {
            // Body is the authoritative timing source. Without a body, nothing animates.
            var bodyFrames = _bodyLayer != null ? _bodyLayer.GetFrames(_currentAnimation) : null;
            if (bodyFrames == null || bodyFrames.Length == 0)
            {
                // Body has no frames for this anim — try idle as fallback so we always show *something*.
                if (_currentAnimation != "idle") PlayAnimation("idle");
                return;
            }

            var frameDuration = _bodyLayer.FrameDuration <= 0f
                ? EquipmentConstants.DefaultFrameDuration
                : _bodyLayer.FrameDuration;

            _elapsed += Time.deltaTime;
            while (_elapsed >= frameDuration)
            {
                _elapsed -= frameDuration;
                _frameIndex++;

                if (_frameIndex >= bodyFrames.Length)
                {
                    if (IsLooping(_currentAnimation))
                    {
                        _frameIndex = 0;
                    }
                    else if (!string.IsNullOrEmpty(_queuedNext))
                    {
                        var next = _queuedNext;
                        _queuedNext = null;
                        _currentAnimation = next;
                        _frameIndex = 0;
                        _elapsed = 0f;
                    }
                    else
                    {
                        // Hold last frame.
                        _frameIndex = bodyFrames.Length - 1;
                        _elapsed = 0f;
                        ApplyFrameToAllRenderers();
                        return;
                    }
                }
            }
            ApplyFrameToAllRenderers();
        }

        private void ApplyFrameToAllRenderers()
        {
            SetSpriteFromLayer(_bodyRenderer, _bodyLayer);
            SetSpriteFromLayer(_armorRenderer, _armorLayer);
            SetSpriteFromLayer(_weaponRenderer, _weaponLayer);
        }

        private void SetSpriteFromLayer(SpriteRenderer renderer, SpriteLayerSet layer)
        {
            if (renderer == null) return;
            if (layer == null) { renderer.sprite = null; return; }

            var frames = layer.GetFrames(_currentAnimation);
            // Fallback chain: requested anim → idle → null.
            if (frames == null || frames.Length == 0) frames = layer.SpriteIdle;
            if (frames == null || frames.Length == 0) { renderer.sprite = null; return; }

            var idx = Mathf.Clamp(_frameIndex, 0, frames.Length - 1);
            renderer.sprite = frames[idx];
        }

        /// <summary>Pure helper — looping policy per animation id. Exposed for tests.</summary>
        public static bool IsLooping(string animationId)
        {
            return animationId == "idle" || animationId == "meditation";
        }
    }
}
