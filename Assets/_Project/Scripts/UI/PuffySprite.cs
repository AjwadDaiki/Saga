using System.Collections.Generic;
using UnityEngine;

namespace Saga.UI
{
    /// <summary>
    /// Sprint 7.5 — procedural rounded-rect sprite factory for the "puffy 3D" look.
    /// Generates 9-sliced sprites at runtime so we don't ship any sprite assets:
    ///   - <see cref="RoundedFill"/> : solid white rounded rect (tint via Image.color).
    ///   - <see cref="RoundedOutline"/> : transparent center + white ring tinted via Image.color (heavy outline).
    /// Both are cached by (radius, outline) so we create each texture once.
    ///
    /// Usage in uGUI: set Image.sprite + Image.type = Sliced. The 9-slice border = corner radius,
    /// so the sprite scales to any button size while keeping crisp rounded corners.
    /// </summary>
    public static class PuffySprite
    {
        private static readonly Dictionary<int, Sprite> _fillCache = new Dictionary<int, Sprite>();
        private static readonly Dictionary<long, Sprite> _outlineCache = new Dictionary<long, Sprite>();

        /// <summary>Solid rounded-rect (white, tintable). 9-slice border = radius.</summary>
        public static Sprite RoundedFill(int radius = 16)
        {
            radius = Mathf.Clamp(radius, 2, 64);
            if (_fillCache.TryGetValue(radius, out var s) && s != null) return s;

            var size = radius * 2 + 4; // +4 center strip for the slice
            var tex = NewTex(size);
            var px = new Color32[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    px[y * size + x] = InsideRoundedRect(x, y, size, size, radius) ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            tex.SetPixels32(px); tex.Apply();

            var border = new Vector4(radius, radius, radius, radius);
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s.name = $"PuffyFill_r{radius}";
            _fillCache[radius] = s;
            return s;
        }

        /// <summary>
        /// Rounded-rect outline ring (white-baked, tint via Image.color). 9-slice border = radius.
        /// Sprint 7.5 Phase 2 Polish : ne baked plus le charcoal #161d1f hardcodé — consommateurs
        /// doivent maintenant assigner <c>image.color = tokens.navyContour</c> (ou variant DA §4.2).
        /// Évite de générer 1 texture par couleur d'outline.
        /// </summary>
        public static Sprite RoundedOutline(int radius = 16, int thickness = 3)
        {
            radius = Mathf.Clamp(radius, 2, 64);
            thickness = Mathf.Clamp(thickness, 1, 12);
            var key = ((long)radius << 8) | (uint)thickness;
            if (_outlineCache.TryGetValue(key, out var s) && s != null) return s;

            var size = radius * 2 + 4;
            var tex = NewTex(size);
            var white = new Color32(255, 255, 255, 255); // tint runtime via Image.color
            var clear = new Color32(0, 0, 0, 0);
            var px = new Color32[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var outer = InsideRoundedRect(x, y, size, size, radius);
                    var inner = InsideRoundedRect(x, y, size, size, radius, inset: thickness);
                    px[y * size + x] = (outer && !inner) ? white : clear;
                }
            tex.SetPixels32(px); tex.Apply();

            var border = new Vector4(radius, radius, radius, radius);
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s.name = $"PuffyOutline_r{radius}_t{thickness}";
            _outlineCache[key] = s;
            return s;
        }

        /// <summary>Soft elliptical gloss highlight (white→transparent), for the top-left specular shine.</summary>
        public static Sprite Gloss()
        {
            const int w = 64, h = 32;
            var tex = NewTex(w, h);
            var px = new Color32[w * h];
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var dx = (x - w * 0.5f) / (w * 0.5f);
                    var dy = (y - h * 0.5f) / (h * 0.5f);
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var a = Mathf.Clamp01(1f - d);
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(a * a * 255));
                }
            tex.SetPixels32(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            s.name = "PuffyGloss";
            return s;
        }

        private static Texture2D NewTex(int w, int h = -1)
        {
            if (h < 0) h = w;
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        private static bool InsideRoundedRect(int x, int y, int w, int h, int radius, int inset = 0)
        {
            float left = inset, right = w - 1 - inset, bottom = inset, top = h - 1 - inset;
            var r = Mathf.Max(0, radius - inset);
            if (x < left || x > right || y < bottom || y > top) return false;
            // Corner circles.
            float cx, cy;
            if (x < left + r && y < bottom + r) { cx = left + r; cy = bottom + r; }
            else if (x > right - r && y < bottom + r) { cx = right - r; cy = bottom + r; }
            else if (x < left + r && y > top - r) { cx = left + r; cy = top - r; }
            else if (x > right - r && y > top - r) { cx = right - r; cy = top - r; }
            else return true; // straight edges / center
            var ddx = x - cx; var ddy = y - cy;
            return ddx * ddx + ddy * ddy <= r * r;
        }
    }
}
