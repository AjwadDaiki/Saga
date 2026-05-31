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
        /// <summary>Sprint 7.5 Polish — safe-area-clamped parent for HUD elements (top bar, stage, skills,
        /// upgrades, bottom nav). Cinematics and modal backdrops should still parent to <see cref="Canvas"/>
        /// directly so they can bleed past the notch / home indicator.</summary>
        public RectTransform UIRoot;
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

        // Sprint 9 Phase 1 — Designer-First wiring.
        /// <summary>Populated par MainSceneBootstrap au boot. null si Ajwad n'a pas authored la scène.</summary>
        public SceneRegistry Registry;
        /// <summary>True si Registry est non-null ET sentinel (Force) trouvé. Les builders Sprint 9
        /// utilisent ce flag pour basculer entre <c>WireFromAuthored(ctx)</c> et le path procédural.</summary>
        public bool DesignerFirstActive;
    }
}
