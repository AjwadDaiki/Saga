using DG.Tweening;
using Saga.Core;
using Saga.Data;
using Saga.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 8 Phase A — first-session onboarding overlay.
    ///
    /// Posed by <c>MainSceneBootstrap</c> AFTER the regular UI builders so anchor target lookups
    /// (ForcePill, VagueButton, etc.) succeed. Listens to GameEvents.OnTutorialStepShown to render
    /// a dim + highlight ring + speech bubble. Listens to OnTutorialStepCompleted to dismiss.
    /// Listens to OnTutorialFinished to tear down completely.
    ///
    /// Layout (per coordinateur précisions) :
    /// - Dim background full-screen alpha 0.55, raycastTarget=false (joueur peut interagir
    ///   normalement avec le target — pas de bloquage).
    /// - HighlightRing : pulse continu scale 1.0↔1.1 + alpha 0.7↔1.0 sur 1.0s (Sin yoyo).
    /// - SpeechBubble : RhosGFX frameBasicGrey Sliced + texte Lilita 24sp outline 0.20
    ///   (max 2 lignes ~60 chars), placée au-dessus ou au-dessous du target selon position.
    /// - Skip button : bottom-right anchored au-dessus du Bottom Nav (offset Y = 200 px).
    /// </summary>
    public static class TutorialOverlayBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            var gm = GameManager.Instance;
            if (gm?.Tutorial == null || gm.State == null) return;
            if (gm.State.tutorialDone) return; // skip if legacy / completed

            var parent = ctx.Canvas != null ? ctx.Canvas.transform : null;
            if (parent == null) return;

            // Root overlay GO + view component that handles event subscriptions + child build.
            var root = new GameObject("TutorialOverlay",
                typeof(RectTransform), typeof(CanvasGroup), typeof(TutorialOverlayView));
            root.transform.SetParent(parent, false);
            root.transform.SetAsLastSibling(); // au-dessus du UI normal (z visuel)
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false; // visual only — joueur peut interagir avec targets

            var view = root.GetComponent<TutorialOverlayView>();
            view.Initialize(rt, cg);

            // Start tutorial service AFTER overlay is wired — service raises Shown immediately
            // for first step, which the view will pick up via its OnEnable subscriptions.
            gm.Tutorial.Start();
        }
    }
}
