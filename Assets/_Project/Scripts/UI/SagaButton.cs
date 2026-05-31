using DG.Tweening;
using Saga.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.5 refonte — puffy 3D button (reproduces vague_button_puffy.html).
    /// Composite hierarchy built by <see cref="Create"/>:
    ///   container (Button + this) ─ Floor (darker rounded rect, peeks <c>floorPx</c> at bottom)
    ///                              └ Face  ─ Fill (rounded, tinted) / Outline (charcoal ring) / Gloss / Label
    /// Press : Face slides down <c>floorPx</c> + Floor hides → physical click.
    /// Variants: Standard / Primary (CTA, pulse when ready) / Special (voie border) / Pill.
    /// </summary>
    [DisallowMultipleComponent]
    public class SagaButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Variant { Standard, Primary, Special, Pill }

        private RectTransform _face;
        private GameObject _floor;
        private Image _glow;
        private int _floorPx;
        private bool _ready;
        private Tween _pulse;
        private bool _pressed;

        // Sprint 7.6 — RhosGFX path : sprite-swap on press (no Tween Y, no Floor procédural).
        private Image _rhosFace;
        private RhosGFXAssetCatalog.ButtonStateSet _rhosStates;
        private bool _useRhosSpriteSwap;

        public Button Button { get; private set; }
        public TextMeshProUGUI Label { get; private set; }
        public Image Fill { get; private set; }

        /// <summary>
        /// Build a puffy button. Caller sets the RectTransform anchors/size on the returned GO.
        /// </summary>
        public static SagaButton Create(Transform parent, string name, Variant variant,
            Color faceColor, string label, int radius = 16, int floorPx = 6,
            float labelSize = 22, Color? labelColor = null, Color? voieTint = null)
        {
            var tokens = DesignTokens.Get();
            var go = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(SagaButton));
            go.transform.SetParent(parent, false);
            var sb = go.GetComponent<SagaButton>();
            sb._floorPx = floorPx;
            sb.Button = go.GetComponent<Button>();
            sb.Button.transition = Selectable.Transition.None;

            // ---- Floor (darker, peeks at the bottom) ----
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(go.transform, false);
            var floorRt = (RectTransform)floor.transform;
            floorRt.anchorMin = Vector2.zero; floorRt.anchorMax = Vector2.one;
            floorRt.offsetMin = new Vector2(0, -floorPx); floorRt.offsetMax = Vector2.zero;
            var floorImg = floor.GetComponent<Image>();
            floorImg.sprite = PuffySprite.RoundedFill(radius);
            floorImg.type = Image.Type.Sliced;
            floorImg.color = DesignTokens.Darken(faceColor, 0.28f);
            floorImg.raycastTarget = false;
            sb._floor = floor;

            // ---- Face (the visible top) ----
            var face = new GameObject("Face", typeof(RectTransform));
            face.transform.SetParent(go.transform, false);
            var faceRt = (RectTransform)face.transform;
            faceRt.anchorMin = Vector2.zero; faceRt.anchorMax = Vector2.one;
            faceRt.offsetMin = Vector2.zero; faceRt.offsetMax = Vector2.zero;
            sb._face = faceRt;

            // Glow halo (behind face fill) — Primary/Special.
            if (variant == Variant.Primary || variant == Variant.Special)
            {
                var glow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
                glow.transform.SetParent(face.transform, false);
                var glowRt = (RectTransform)glow.transform;
                glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
                glowRt.offsetMin = new Vector2(-10, -10); glowRt.offsetMax = new Vector2(10, 6);
                var gimg = glow.GetComponent<Image>();
                gimg.sprite = PuffySprite.RoundedFill(radius + 4);
                gimg.type = Image.Type.Sliced;
                var gc = (voieTint ?? tokens.accentPrimary);
                gimg.color = new Color(gc.r, gc.g, gc.b, variant == Variant.Primary ? 0.45f : 0.30f);
                gimg.raycastTarget = false;
                sb._glow = gimg;
            }

            // Fill.
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(face.transform, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            sb.Fill = fill.GetComponent<Image>();
            sb.Fill.sprite = PuffySprite.RoundedFill(radius);
            sb.Fill.type = Image.Type.Sliced;
            sb.Fill.color = faceColor;
            sb.Button.targetGraphic = sb.Fill;

            // Outline (charcoal ring on top).
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(face.transform, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, tokens.puffyOutline);
            olImg.type = Image.Type.Sliced;
            olImg.color = (variant == Variant.Special && voieTint.HasValue) ? voieTint.Value : tokens.navyContour;
            olImg.raycastTarget = false;

            // Gloss (top-left specular).
            var gloss = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            gloss.transform.SetParent(face.transform, false);
            var glRt = (RectTransform)gloss.transform;
            glRt.anchorMin = new Vector2(0.06f, 0.55f); glRt.anchorMax = new Vector2(0.5f, 0.92f);
            glRt.offsetMin = Vector2.zero; glRt.offsetMax = Vector2.zero;
            var glImg = gloss.GetComponent<Image>();
            glImg.sprite = PuffySprite.Gloss();
            glImg.color = new Color(1f, 1f, 1f, 0.35f);
            glImg.raycastTarget = false;
            ((RectTransform)gloss.transform).localRotation = Quaternion.Euler(0, 0, -12f);

            // Label.
            if (!string.IsNullOrEmpty(label))
            {
                var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lblGo.transform.SetParent(face.transform, false);
                var lblRt = (RectTransform)lblGo.transform;
                lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
                lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
                var tmp = lblGo.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.font = tokens.DisplayFont;
                tmp.fontSize = labelSize;
                tmp.color = labelColor ?? Color.white;
                tmp.text = label;
                tmp.raycastTarget = false;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                // Charcoal outline on the label (puffy text recipe). outlineWidth>0 enables it.
                tmp.outlineColor = tokens.navyContour;
                tmp.outlineWidth = 0.2f;
                sb.Label = tmp;
            }

            if (variant == Variant.Primary) sb.SetReady(true);
            return sb;
        }

        /// <summary>
        /// Sprint 7.6 — RhosGFX puffy 3D button using pre-baked Cartoony UI Pack sprites.
        /// Replaces the procedural PuffySprite stack (Floor + Outline + Gloss) with a single
        /// Image whose sprite carries the relief / contour / shine baked-in. Press behavior =
        /// sprite swap to the <c>pressed</c> variant (RhosGFX states), no Tween Y.
        ///
        /// Tint to DA palette via <paramref name="tint"/> (Image.color multiplier) — works
        /// cleanly on neutral-ish color sprites; for full-saturation sprites pass <c>Color.white</c>.
        ///
        /// Hierarchy : root (Button + this + Image) ─ Label (optional). No Floor/Outline/Gloss
        /// children — RhosGFX already provides them. Ready-pulse and Primary variant still supported.
        /// </summary>
        public static SagaButton Create(Transform parent, string name,
            RhosGFXAssetCatalog.ButtonStateSet sprites, Variant variant,
            Color tint, string label = "", float labelSize = 22,
            Color? labelColor = null, TMPro.TMP_FontAsset labelFont = null)
        {
            var tokens = DesignTokens.Get();
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(SagaButton));
            go.transform.SetParent(parent, false);

            var sb = go.GetComponent<SagaButton>();
            sb.Button = go.GetComponent<Button>();
            sb.Button.transition = Selectable.Transition.None;
            sb._floorPx = 0; // no procedural floor — RhosGFX 3D baked.
            sb._face = (RectTransform)go.transform;
            sb._useRhosSpriteSwap = true;
            sb._rhosStates = sprites;

            var img = go.GetComponent<Image>();
            sb._rhosFace = img;
            sb.Fill = img;
            sb.Button.targetGraphic = img;
            img.sprite = sprites.standard;
            img.type = Image.Type.Simple; // RhosGFX state sprites match the host RectTransform size; switch to Sliced if 9-slice borders are set.
            img.preserveAspect = false;
            img.color = tint;

            // Optional label (RhosGFX sprite has no built-in text).
            if (!string.IsNullOrEmpty(label))
            {
                var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lblGo.transform.SetParent(go.transform, false);
                var lblRt = (RectTransform)lblGo.transform;
                lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
                lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
                var tmp = lblGo.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.font = labelFont != null ? labelFont : tokens.DisplayFont;
                tmp.fontSize = labelSize;
                tmp.color = labelColor ?? Color.white;
                tmp.text = label;
                tmp.raycastTarget = false;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.outlineColor = tokens.navyContour;
                tmp.outlineWidth = 0.2f;
                sb.Label = tmp;
            }

            if (variant == Variant.Primary) sb.SetReady(true);
            return sb;
        }

        /// <summary>
        /// Back-compat: apply puffy styling in-place to an existing Image+Button GO (used by legacy
        /// buttons not yet rebuilt via <see cref="Create"/>). Adds a floor + outline + gloss around the
        /// existing fill image. Press moves the whole host down.
        /// </summary>
        public static SagaButton Wrap(GameObject buttonGo, Variant variant, Color? voieTint = null,
            int radius = 16, int floorPx = 6)
        {
            var tokens = DesignTokens.Get();
            var sb = buttonGo.GetOrAdd<SagaButton>();
            sb.Button = buttonGo.GetComponent<Button>();
            if (sb.Button != null) sb.Button.transition = Selectable.Transition.None;
            sb._floorPx = floorPx;
            sb._face = (RectTransform)buttonGo.transform; // press moves the whole host

            var img = buttonGo.GetComponent<Image>();
            var faceColor = img != null ? img.color : tokens.surfaceMid;
            if (img != null)
            {
                img.sprite = PuffySprite.RoundedFill(radius);
                img.type = Image.Type.Sliced;
            }

            // Floor behind.
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(buttonGo.transform, false);
            floor.transform.SetAsFirstSibling();
            var floorRt = (RectTransform)floor.transform;
            floorRt.anchorMin = Vector2.zero; floorRt.anchorMax = Vector2.one;
            floorRt.offsetMin = new Vector2(0, -floorPx); floorRt.offsetMax = Vector2.zero;
            var floorImg = floor.GetComponent<Image>();
            floorImg.sprite = PuffySprite.RoundedFill(radius);
            floorImg.type = Image.Type.Sliced;
            floorImg.color = DesignTokens.Darken(faceColor, 0.28f);
            floorImg.raycastTarget = false;
            sb._floor = floor;

            // Outline on top.
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.transform.SetParent(buttonGo.transform, false);
            var olRt = (RectTransform)outline.transform;
            olRt.anchorMin = Vector2.zero; olRt.anchorMax = Vector2.one;
            olRt.offsetMin = Vector2.zero; olRt.offsetMax = Vector2.zero;
            var olImg = outline.GetComponent<Image>();
            olImg.sprite = PuffySprite.RoundedOutline(radius, tokens.puffyOutline);
            olImg.type = Image.Type.Sliced;
            olImg.color = (variant == Variant.Special && voieTint.HasValue) ? voieTint.Value : tokens.navyContour;
            olImg.raycastTarget = false;

            if (variant == Variant.Primary) sb.SetReady(true);
            return sb;
        }

        /// <summary>Toggle the "ready" pulse (Primary CTA + skill buttons when usable).</summary>
        public void SetReady(bool ready)
        {
            _ready = ready;
            _pulse?.Kill();
            transform.localScale = Vector3.one;
            if (ready)
            {
                _pulse = transform.DOScale(1.05f, 0.9f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        public void OnPointerDown(PointerEventData _)
        {
            if (Button != null && !Button.interactable) return;
            _pressed = true;
            _pulse?.Kill();
            if (_useRhosSpriteSwap && _rhosFace != null && _rhosStates.pressed != null)
            {
                _rhosFace.sprite = _rhosStates.pressed;
            }
            else
            {
                TweenFaceY(-_floorPx, 0.06f, Ease.OutQuad);
                if (_floor != null) _floor.SetActive(false);
            }
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (!_pressed) return;
            _pressed = false;
            if (_useRhosSpriteSwap && _rhosFace != null && _rhosStates.standard != null)
            {
                _rhosFace.sprite = _rhosStates.standard;
            }
            else
            {
                TweenFaceY(0f, 0.10f, Ease.OutBack);
                if (_floor != null) _floor.SetActive(true);
            }
            if (_ready) SetReady(true);
        }

        // Core DOTween.To on anchoredPosition — the RectTransform.DOAnchorPosY extension lives in the
        // UI module which our asmdef doesn't reference (same constraint as FloatingNumberView).
        private void TweenFaceY(float targetY, float duration, Ease ease)
        {
            if (_face == null) return;
            var face = _face;
            DOTween.To(() => face.anchoredPosition, p => face.anchoredPosition = p,
                    new Vector2(face.anchoredPosition.x, targetY), duration)
                .SetEase(ease)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
