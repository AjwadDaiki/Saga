using System;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// One named frame-by-frame animation. Frames are direct Sprite asset references
    /// (so the asset DB tracks them via GUID — robust to file renames).
    /// </summary>
    [Serializable]
    public class SpriteAnimationData
    {
        [Tooltip("Lookup key, e.g. \"idle\", \"attack\", \"hurt\".")]
        public string animationName;

        [Tooltip("Frames in playback order.")]
        public Sprite[] frames;

        [Tooltip("Seconds per frame. 0.1s ≈ 10 FPS, classic pixel-art feel.")]
        public float frameDuration = 0.1f;

        [Tooltip("True for idle/run; false for attack/hurt one-shots.")]
        public bool loop;
    }

    /// <summary>
    /// Collection of named animations for one character. Built by the Editor utility
    /// (<c>Saga > Sprint 3 > Configure Adventurer Assets</c>), loaded by <c>SpriteAnimator</c>
    /// at runtime via Resources.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationLibrary_New", menuName = "Saga/Sprite Animation Library")]
    public class SpriteAnimationLibrary : ScriptableObject
    {
        [SerializeField] private SpriteAnimationData[] _animations = Array.Empty<SpriteAnimationData>();

        public SpriteAnimationData[] Animations => _animations;

        public SpriteAnimationData Find(string animationName)
        {
            if (_animations == null || animationName == null) return null;
            foreach (var a in _animations)
            {
                if (a != null && a.animationName == animationName) return a;
            }
            return null;
        }

#if UNITY_INCLUDE_TESTS
        public void SetAnimations_ForTests(SpriteAnimationData[] anims) => _animations = anims ?? Array.Empty<SpriteAnimationData>();
#endif

#if UNITY_EDITOR
        /// <summary>Editor-only setter used by AdventurerAssetsConfigurator.</summary>
        public void EditorSetAnimations(SpriteAnimationData[] anims) => _animations = anims ?? Array.Empty<SpriteAnimationData>();
#endif
    }
}
