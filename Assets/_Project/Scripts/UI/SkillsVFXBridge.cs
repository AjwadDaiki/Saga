using DG.Tweening;
using Saga.Core;
using Saga.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 10 V2 Phase 6 — bridge VAGUE / SOUFFLE GameEvents → HeroAnimatorV2 anims +
    /// VFX d'écran (flash blanc + wave sprite slide horizontal + screen shake léger
    /// + zen aura mint pour SOUFFLE).
    ///
    /// Subscribe OnVagueTriggered + OnSouffleStarted. Attaché par MainSceneBootstrap au
    /// Canvas en designer mode. Trouve HeroAnimatorV2 via GameObject.Find("Hero_Samurai")
    /// pour route les anims hero.
    /// </summary>
    [DisallowMultipleComponent]
    public class SkillsVFXBridge : MonoBehaviour
    {
        private Canvas _canvas;
        private RectTransform _canvasRt;
        private HeroAnimatorV2 _hero;

        public void Configure(Canvas canvas, HeroAnimatorV2 hero)
        {
            _canvas = canvas;
            _canvasRt = canvas.transform as RectTransform;
            _hero = hero;
        }

        private void OnEnable()
        {
            GameEvents.OnVagueTriggered += HandleVague;
            GameEvents.OnSouffleStarted += HandleSouffle;
        }

        private void OnDisable()
        {
            GameEvents.OnVagueTriggered -= HandleVague;
            GameEvents.OnSouffleStarted -= HandleSouffle;
        }

        private void HandleVague(CombatPhase _)
        {
            _hero?.PlayVague();
            SpawnScreenFlash(Color.white, 0.2f, 0.18f);
            SpawnWaveSprite();
            ShakeCanvas(8f, 0.25f);
        }

        private void HandleSouffle()
        {
            _hero?.PlaySouffle();
            SpawnScreenFlash(new Color(1f, 1f, 1f, 0.6f), 0.10f, 0.30f);
            SpawnZenAura();
        }

        // ===== VFX helpers =====

        private void SpawnScreenFlash(Color color, float peakAlpha, float duration)
        {
            if (_canvasRt == null) return;
            var go = new GameObject("VagueFlash", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_canvasRt, false);
            go.transform.SetAsLastSibling();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = new Color(color.r, color.g, color.b, 0f);
            img.raycastTarget = false;

            Object.Destroy(go, duration + 0.05f);
            var c = color; c.a = peakAlpha;
            DOTween.Sequence()
                .Append(DOTween.To(() => img.color, x => img.color = x, c, duration * 0.4f).SetEase(Ease.OutQuad))
                .Append(DOTween.To(() => img.color, x => img.color = x, new Color(c.r, c.g, c.b, 0f), duration * 0.6f).SetEase(Ease.InQuad))
                .SetLink(go, LinkBehaviour.KillOnDestroy);
        }

        private void SpawnWaveSprite()
        {
            if (_canvasRt == null) return;
            var tokens = DesignTokens.Get();
            var go = new GameObject("VagueWave", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_canvasRt, false);
            go.transform.SetAsLastSibling();
            var rt = (RectTransform)go.transform;
            // Horizontal band 80px tall au milieu de l'écran, démarre hors écran gauche.
            rt.anchorMin = new Vector2(0, 0.4f); rt.anchorMax = new Vector2(0, 0.6f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(400, 0);
            rt.anchoredPosition = new Vector2(-500f, 0);
            var img = go.GetComponent<Image>();
            img.color = new Color(tokens.skyBlue.r, tokens.skyBlue.g, tokens.skyBlue.b, 0.65f);
            img.raycastTarget = false;

            // Calculer largeur canvas pour atteindre hors écran droit.
            var canvasWidth = _canvasRt.rect.width;
            Object.Destroy(go, 0.65f);
            DOTween.Sequence()
                .Append(rt.DOAnchorPosX(canvasWidth + 500f, 0.55f).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => img.color, c => img.color = c,
                    new Color(img.color.r, img.color.g, img.color.b, 0f), 0.55f).SetEase(Ease.InQuad))
                .SetLink(go, LinkBehaviour.KillOnDestroy);
        }

        private void SpawnZenAura()
        {
            // Mint green aura ring around Hero (centre canvas approximatif).
            if (_canvasRt == null) return;
            var tokens = DesignTokens.Get();
            var go = new GameObject("SouffleAura", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(_canvasRt, false);
            go.transform.SetAsLastSibling();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.45f); // approximé sur Hero center
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(200, 200);
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = new Color(tokens.mintPositif.r, tokens.mintPositif.g, tokens.mintPositif.b, 0.45f);
            img.raycastTarget = false;
            var cg = go.GetComponent<CanvasGroup>();
            cg.alpha = 1f; cg.blocksRaycasts = false;

            Object.Destroy(go, 1.05f);
            DOTween.Sequence()
                .Append(rt.DOScale(1.6f, 0.9f).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => cg.alpha, a => { if (cg != null) cg.alpha = a; }, 0f, 0.9f).SetEase(Ease.InQuad))
                .SetLink(go, LinkBehaviour.KillOnDestroy);
        }

        private void ShakeCanvas(float strength, float duration)
        {
            if (_canvasRt == null) return;
            var origPos = _canvasRt.localPosition;
            _canvasRt.DOKill();
            _canvasRt.DOShakePosition(duration, strength, vibrato: 12, randomness: 90f, snapping: false, fadeOut: true)
                .OnComplete(() => { if (_canvasRt != null) _canvasRt.localPosition = origPos; })
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
