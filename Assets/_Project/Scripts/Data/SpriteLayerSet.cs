using System;
using BreakInfinity;
using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// One visual layer + stats packet (Sprint 7 modular sprite system).
    /// A character is composed of up to 3 SpriteLayerSets stacked on top of each other:
    /// Body (z=0) &lt; Armor (z=1) &lt; Weapon (z=2).
    ///
    /// Each layer carries its own per-animation Sprite[] frames. The
    /// <see cref="Saga.Gameplay.LayeredCharacterRenderer"/> drives all 3 layers off a single
    /// frame index per Update — they stay in sync visually even though each ScriptableObject
    /// is independent.
    ///
    /// If a layer doesn't define frames for the active animation, the renderer falls back
    /// to that layer's <see cref="spriteIdle"/> (or null if even idle is empty).
    /// </summary>
    [CreateAssetMenu(fileName = "LayerSet_New", menuName = "Saga/Sprite Layer Set")]
    public class SpriteLayerSet : ScriptableObject
    {
        [Tooltip("Stable lowercase snake_case identifier. Used as save key and Resources lookup.")]
        [SerializeField] private string _id;

        [SerializeField] private EquipmentSlot _slotType;
        [SerializeField] private string _displayName;
        [SerializeField] private Voie _voie = Voie.None;
        [SerializeField] private Rarity _rarity = Rarity.Commun;

        [Header("Animation frames (Sprite[] per state)")]
        [SerializeField] private Sprite[] _spriteIdle = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] _spriteAttack1 = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] _spriteAttack2 = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] _spriteAttack3 = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] _spriteHurt = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] _spriteMeditation = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] _spriteDie = Array.Empty<Sprite>();

        [Header("Stats (applied when equipped)")]
        [Tooltip("Flat Force-per-tap bonus added when equipped (additive with upgrade Frappe).")]
        [SerializeField] private double _statsBonusForce;

        [Tooltip("Crit-chance bonus in absolute terms (e.g. 0.05 = +5% crit). Sprint 8+ consumed by combat formula.")]
        [SerializeField] private float _statsBonusCrit;

        [Header("UI")]
        [TextArea(2, 4)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _iconSprite;

        [Tooltip("Seconds per frame. Default 0.1 = 10fps pixel-art cadence.")]
        [Min(0.01f)]
        [SerializeField] private float _frameDuration = EquipmentConstants.DefaultFrameDuration;

        public string Id => _id;
        public EquipmentSlot SlotType => _slotType;
        public string DisplayName => _displayName;
        public Voie Voie => _voie;
        public Rarity Rarity => _rarity;
        public Sprite[] SpriteIdle => _spriteIdle;
        public Sprite[] SpriteAttack1 => _spriteAttack1;
        public Sprite[] SpriteAttack2 => _spriteAttack2;
        public Sprite[] SpriteAttack3 => _spriteAttack3;
        public Sprite[] SpriteHurt => _spriteHurt;
        public Sprite[] SpriteMeditation => _spriteMeditation;
        public Sprite[] SpriteDie => _spriteDie;
        public BigDouble StatsBonusForce => new BigDouble(_statsBonusForce);
        public float StatsBonusCrit => _statsBonusCrit;
        public string Description => _description;
        public Sprite IconSprite => _iconSprite;
        public float FrameDuration => _frameDuration;

        /// <summary>Look up the frame array for a given animation id. Null if unset/unknown.</summary>
        public Sprite[] GetFrames(string animationId)
        {
            switch (animationId)
            {
                case "idle": return _spriteIdle;
                case "attack1": return _spriteAttack1;
                case "attack2": return _spriteAttack2;
                case "attack3": return _spriteAttack3;
                case "hurt": return _spriteHurt;
                case "meditation": return _spriteMeditation;
                case "die": return _spriteDie;
                default: return null;
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id)) Debug.LogError($"[SpriteLayerSet] {name} missing id", this);
            if (_spriteIdle == null || _spriteIdle.Length == 0)
                Debug.LogWarning($"[SpriteLayerSet] {name} has no idle frames (will render null in fallback)", this);
        }

        /// <summary>
        /// Runtime factory used by <c>MainSceneBootstrap</c> when the body SpriteLayerSet is missing
        /// or has no idle frames — so the player is never invisible. Available in all build configs
        /// (the test-only <c>CreateForTests</c> below is for unit tests).
        /// </summary>
        public static SpriteLayerSet CreateRuntime(string id, EquipmentSlot slot,
            Sprite[] idle, Sprite[] attack1 = null, Sprite[] attack2 = null, Sprite[] attack3 = null,
            Sprite[] hurt = null, Sprite[] meditation = null, Sprite[] die = null,
            double statsBonusForce = 0, float frameDuration = EquipmentConstants.DefaultFrameDuration,
            string displayName = null)
        {
            var s = CreateInstance<SpriteLayerSet>();
            s._id = id;
            s._slotType = slot;
            s._displayName = displayName ?? id;
            s._spriteIdle = idle ?? Array.Empty<Sprite>();
            s._spriteAttack1 = attack1 ?? Array.Empty<Sprite>();
            s._spriteAttack2 = attack2 ?? Array.Empty<Sprite>();
            s._spriteAttack3 = attack3 ?? Array.Empty<Sprite>();
            s._spriteHurt = hurt ?? Array.Empty<Sprite>();
            s._spriteMeditation = meditation ?? Array.Empty<Sprite>();
            s._spriteDie = die ?? Array.Empty<Sprite>();
            s._statsBonusForce = statsBonusForce;
            s._frameDuration = frameDuration;
            return s;
        }

#if UNITY_INCLUDE_TESTS
        public static SpriteLayerSet CreateForTests(string id, EquipmentSlot slot, Voie voie, Rarity rarity,
            double statsBonusForce, float statsBonusCrit = 0f, string displayName = null,
            Sprite[] idle = null, Sprite[] attack1 = null, Sprite[] attack2 = null, Sprite[] attack3 = null,
            Sprite[] hurt = null, Sprite[] meditation = null, Sprite[] die = null)
        {
            var s = CreateInstance<SpriteLayerSet>();
            s._id = id;
            s._slotType = slot;
            s._voie = voie;
            s._rarity = rarity;
            s._statsBonusForce = statsBonusForce;
            s._statsBonusCrit = statsBonusCrit;
            s._displayName = displayName ?? id;
            s._spriteIdle = idle ?? Array.Empty<Sprite>();
            s._spriteAttack1 = attack1 ?? Array.Empty<Sprite>();
            s._spriteAttack2 = attack2 ?? Array.Empty<Sprite>();
            s._spriteAttack3 = attack3 ?? Array.Empty<Sprite>();
            s._spriteHurt = hurt ?? Array.Empty<Sprite>();
            s._spriteMeditation = meditation ?? Array.Empty<Sprite>();
            s._spriteDie = die ?? Array.Empty<Sprite>();
            return s;
        }
#endif
    }
}
