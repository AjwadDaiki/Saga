using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 Polish 6 — Shadow sync sous Hero_Samurai + Mannequin.
    ///
    /// Pas de Shadow custom GO dans Main.unity V2 (Ajwad n'a pas authored). On crée
    /// dynamiquement un disque flat noir alpha 0.35 à la base de chaque sprite, posé
    /// sibling FIRST (rendu derrière les autres parts via SetAsFirstSibling).
    ///
    /// Chaque ShadowLink fournit un Func&lt;float&gt; PhaseSource normalisé [-1..+1] :
    ///   - Hero    : (body.localPosition.y - bodyBaseY) / amplitude
    ///   - Mannequin: (top.localEulerAngles.z wrap → -180..180) / amplitudeDeg, ou base
    ///                breath scaleY.
    /// LateUpdate mappe phase → shadow scaleX (1.10 ↔ 0.85) + alpha (1.30 ↔ 0.55 * baseAlpha)
    /// pour donner l'illusion de hauteur (body up = shadow smaller/fainter).
    /// </summary>
    [DisallowMultipleComponent]
    public class ShadowSyncBridge : MonoBehaviour
    {
        private readonly List<ShadowLink> _links = new List<ShadowLink>();

        private class ShadowLink
        {
            public Func<float> PhaseSource; // returns clamped[-1..+1]
            public RectTransform Shadow;
            public Image ShadowImage;
            public Vector3 BaseScale;
            public float BaseAlpha;
        }

        /// <summary>Register a shadow under parent, syncing visuals to phaseSource() ∈ [-1..+1].</summary>
        public void Register(Transform parent, Vector2 localOffset, Vector2 shadowSize,
            float baseAlpha, Func<float> phaseSource)
        {
            if (parent == null || phaseSource == null) return;
            var go = new GameObject("Shadow_Sync",
                typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = shadowSize;
            rt.anchoredPosition = localOffset;

            var img = go.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, baseAlpha);
            img.raycastTarget = false;

            _links.Add(new ShadowLink
            {
                PhaseSource = phaseSource,
                Shadow = rt,
                ShadowImage = img,
                BaseScale = rt.localScale,
                BaseAlpha = baseAlpha,
            });
        }

        /// <summary>Convenience: Y oscillation sync (body.localPosition.y around baseY ± amp).</summary>
        public void RegisterYOscillation(Transform parent, Transform body, Vector2 localOffset,
            Vector2 shadowSize, float baseAlpha, float amplitude)
        {
            if (body == null) return;
            var baseY = body.localPosition.y;
            var amp = Mathf.Max(0.1f, amplitude);
            Register(parent, localOffset, shadowSize, baseAlpha,
                () => Mathf.Clamp((body.localPosition.y - baseY) / amp, -1f, 1f));
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                if (link.Shadow == null || link.PhaseSource == null) continue;
                var phase = Mathf.Clamp(link.PhaseSource(), -1f, 1f);
                var t = (phase + 1f) * 0.5f; // 0 (down) → 1 (up)
                var scaleMul = Mathf.Lerp(1.10f, 0.85f, t);
                var alphaMul = Mathf.Lerp(1.30f, 0.55f, t);
                link.Shadow.localScale = new Vector3(
                    link.BaseScale.x * scaleMul,
                    link.BaseScale.y * scaleMul, 1f);
                link.ShadowImage.color = new Color(0f, 0f, 0f,
                    Mathf.Clamp01(link.BaseAlpha * alphaMul));
            }
        }
    }
}
