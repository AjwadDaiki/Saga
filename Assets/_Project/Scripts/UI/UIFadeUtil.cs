using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.6 — Replacement for the missing <c>Image.DOFade</c> extension
    /// (DOTween's UI-module is not referenced by Saga.Runtime — same constraint as
    /// the FloatingNumberView / SagaButton TweenFaceY workaround). Uses pure
    /// <see cref="DOTween.To"/> core to animate <c>Image.color.a</c>.
    /// </summary>
    public static class UIFadeUtil
    {
        /// <summary>
        /// Tween a UI Image's alpha to <paramref name="toAlpha"/> over <paramref name="duration"/>.
        /// <c>SetTarget(image)</c> wires the tween so <c>image.DOKill()</c> properly cancels it
        /// (otherwise the targetless DOTween.To would survive a DOKill call on the host image).
        /// </summary>
        public static Tween Fade(Image image, float toAlpha, float duration)
        {
            if (image == null) return null;
            return DOTween.To(() => image.color.a, a =>
            {
                var c = image.color;
                c.a = a;
                image.color = c;
            }, toAlpha, duration).SetTarget(image);
        }
    }
}
