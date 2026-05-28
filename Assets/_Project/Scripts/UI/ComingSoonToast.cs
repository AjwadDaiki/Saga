using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.5 — tiny transient toast shown when a not-yet-built nav tab (Shop/Hero/Artifacts/Legend)
    /// is tapped. Fades in, holds, fades out. The 4 placeholder screens land in Sprint 8+.
    /// </summary>
    [DisallowMultipleComponent]
    public class ComingSoonToast : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        private CanvasGroup _cg;
        private Tween _seq;

        private void Awake()
        {
            _cg = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _cg.alpha = 0f;
        }

        public void Show(string text)
        {
            if (Label != null) Label.text = text;
            if (_cg == null) _cg = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _seq?.Kill();
            _cg.alpha = 0f;
            transform.localScale = Vector3.one * 0.9f;
            _seq = DOTween.Sequence()
                .Append(_cg.DOFade(1f, 0.18f))
                .Join(transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack))
                .AppendInterval(1.1f)
                .Append(_cg.DOFade(0f, 0.3f))
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
