using Saga.Data;
using UnityEngine;

namespace Saga.Gameplay
{
    /// <summary>
    /// Lightweight frame-by-frame sprite animator. Reads a <see cref="SpriteAnimationLibrary"/>,
    /// drives <see cref="SpriteRenderer.sprite"/> per <c>frameDuration</c>.
    ///
    /// Designed for the Sprint 3 chibi pixel adventurer — no AnimatorController, no AnimationClips.
    /// Play(name) sets current; loop or one-shot per the data. Optional queue:
    /// Play("attack1", queueNext: "idle") returns to idle (looping) after attack completes.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private SpriteAnimationLibrary _library;
        [SerializeField] private string _defaultAnimation = "idle";

        private SpriteAnimationData _current;
        private SpriteAnimationData _queuedNext;
        private float _elapsed;
        private int _frameIndex;

        public SpriteRenderer Renderer
        {
            get => _renderer;
            set => _renderer = value;
        }

        public SpriteAnimationLibrary Library
        {
            get => _library;
            set => _library = value;
        }

        public string CurrentAnimationName => _current?.animationName;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_library == null)
            {
                _library = Resources.Load<SpriteAnimationLibrary>("Animations/AdventurerAnimationLibrary");
            }
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(_defaultAnimation)) Play(_defaultAnimation);
        }

        /// <summary>
        /// Switch to <paramref name="animationName"/>. If the anim is one-shot and
        /// <paramref name="queueNext"/> is supplied, that anim plays after completion.
        /// </summary>
        public void Play(string animationName, string queueNext = null)
        {
            if (_library == null)
            {
                Debug.LogWarning($"[SpriteAnimator] No library — cannot play '{animationName}'.", this);
                return;
            }
            var data = _library.Find(animationName);
            if (data == null)
            {
                Debug.LogWarning($"[SpriteAnimator] Animation '{animationName}' not found in library.", this);
                return;
            }

            // For a looping anim that's already playing, don't restart (keeps idle smooth across re-calls).
            // For one-shot anims (attack/hurt), always restart from frame 0 for snappy feedback.
            if (_current == data && data.loop) return;

            _current = data;
            _queuedNext = !string.IsNullOrEmpty(queueNext) ? _library.Find(queueNext) : null;
            _frameIndex = 0;
            _elapsed = 0f;
            UpdateSprite();
        }

        private void Update()
        {
            if (_current == null || _current.frames == null || _current.frames.Length == 0) return;

            _elapsed += Time.deltaTime;

            // While-loop instead of if-loop: if dt > frameDuration (slow frame, alt-tab, etc.),
            // we still catch up correctly instead of skipping frames silently.
            while (_elapsed >= _current.frameDuration)
            {
                _elapsed -= _current.frameDuration;
                _frameIndex++;

                if (_frameIndex >= _current.frames.Length)
                {
                    if (_current.loop)
                    {
                        _frameIndex = 0;
                    }
                    else if (_queuedNext != null)
                    {
                        // Transition into queued anim with clean state — fresh elapsed avoids the
                        // queued anim starting partway into its first frame.
                        _current = _queuedNext;
                        _queuedNext = null;
                        _frameIndex = 0;
                        _elapsed = 0f;
                    }
                    else
                    {
                        // Hold the last frame indefinitely.
                        _frameIndex = _current.frames.Length - 1;
                        _elapsed = 0f;
                        UpdateSprite();
                        return;
                    }
                }
            }
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (_renderer != null && _current != null && _frameIndex < _current.frames.Length)
            {
                _renderer.sprite = _current.frames[_frameIndex];
            }
        }
    }
}
