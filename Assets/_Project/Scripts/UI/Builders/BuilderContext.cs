using Saga.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Mutable bag passed to UI zone builders. Each builder reads what it needs and
    /// writes back the GameObjects/Transforms the bootstrap exposes downstream.
    /// </summary>
    public sealed class BuilderContext
    {
        public Canvas Canvas;
        public Transform WorldRoot;
        public DesignTokens Tokens;
        // Filled by SceneBuilder.
        public Transform CharacterTransform;
        public Transform MannequinTransform;
        public Transform AdversaireTransform;
        public Transform CapitaineTransform;
        public Transform MaitreTransform;
        // Filled by SkillsBuilder, consumed by Bootstrap when wiring Affronter Maître button to its modal.
        public RectTransform ElanRow;
    }
}
