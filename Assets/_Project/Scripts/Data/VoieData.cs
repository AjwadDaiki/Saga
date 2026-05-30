using UnityEngine;

namespace Saga.Data
{
    /// <summary>
    /// Cultural school (Sprint 7). One per <see cref="Voie"/> enum value.
    /// Holds palette + identity citations + lore preview. Mécaniques de voie
    /// (passive bonuses, voie-mastery flags) restent dans <see cref="GameState"/>
    /// pour rester serializable simplement.
    /// </summary>
    [CreateAssetMenu(fileName = "Voie_New", menuName = "Saga/Voie Data")]
    public class VoieData : ScriptableObject
    {
        [SerializeField] private Voie _voieEnum = Voie.None;
        [SerializeField] private string _displayName;
        [SerializeField] private Color _mainColor = Color.white;
        [SerializeField] private Color _accentColor = Color.white;

        [TextArea(2, 3)]
        [SerializeField] private string _introCitation;
        [TextArea(2, 4)]
        [SerializeField] private string _description;
        [TextArea(2, 3)]
        [SerializeField] private string _bonusDescription;

        public Voie VoieEnum => _voieEnum;
        public string DisplayName => _displayName;
        public Color MainColor => _mainColor;
        public Color AccentColor => _accentColor;
        public string IntroCitation => _introCitation;
        public string Description => _description;
        public string BonusDescription => _bonusDescription;

        private void OnValidate()
        {
            if (_voieEnum == Voie.None && string.IsNullOrEmpty(_displayName)) return;
            if (string.IsNullOrEmpty(_displayName))
                Debug.LogWarning($"[VoieData] {name} missing displayName", this);
        }

#if UNITY_INCLUDE_TESTS
        public static VoieData CreateForTests(Voie voie, string displayName, Color mainColor,
            string introCitation = null, string bonusDescription = null)
        {
            var v = CreateInstance<VoieData>();
            v._voieEnum = voie;
            v._displayName = displayName ?? voie.ToString();
            v._mainColor = mainColor;
            v._accentColor = mainColor;
            v._introCitation = introCitation ?? string.Empty;
            v._bonusDescription = bonusDescription ?? string.Empty;
            return v;
        }
#endif
    }
}
