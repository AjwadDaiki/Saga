using Saga.Data;
using UnityEngine;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Sprint 7.5 ambient background — 3 visual layers built in world space, behind everything.
    ///   z=10 : illustrated dojo sprite OR dusk gradient fallback
    ///   z=9.5 : vertical shade overlay (UI legibility)
    ///   z=8  : ambient ParticleSystem (subtle amber motes drifting up — "dojo qui respire")
    /// All purely procedural so no art assets are required.
    /// </summary>
    public static class BackgroundBuilder
    {
        public static void Build(BuilderContext ctx)
        {
            var parent = ctx.WorldRoot;
            var tokens = DesignTokens.Get();
            var root = new GameObject("Ambient");
            root.transform.SetParent(parent, false);

            // Sprint 7.5 refonte : fond illustré coloré (Vibrant Quest). On charge le sprite depuis
            // Resources/Backgrounds/dojo_bg (copié par 'Saga > Design > Generate Font Assets + Background').
            // Fallback : gradient dusk procédural si le sprite n'est pas là.
            var illustrated = Resources.Load<Sprite>("Backgrounds/dojo_bg");
            var bgGo = new GameObject("BgIllustrated", typeof(SpriteRenderer));
            bgGo.transform.SetParent(root.transform, false);
            bgGo.transform.position = new Vector3(0, 0, 10f);
            var bgSr = bgGo.GetComponent<SpriteRenderer>();
            bgSr.sortingOrder = -10;
            // Cover the ACTUAL camera frustum (+10% margin), not an arbitrary 13u — covering 13u tall
            // while the ortho cam only shows 6u was zooming the bg ~2× ("gros plan"). Compute from cam.
            var cam = Camera.main;
            var viewH = (cam != null ? cam.orthographicSize : 3f) * 2f;
            var aspect = (cam != null && cam.aspect > 0.01f) ? cam.aspect : (1080f / 1920f);
            var viewW = viewH * aspect;
            var coverW = viewW * 1.12f;
            var coverH = viewH * 1.12f;

            if (illustrated != null)
            {
                bgSr.sprite = illustrated;
                var sp = illustrated.bounds.size;
                if (sp.x > 0.01f && sp.y > 0.01f)
                {
                    var scale = Mathf.Max(coverW / sp.x, coverH / sp.y); // cover-fit (fills, crops overflow)
                    bgGo.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            else
            {
                // Fallback dusk gradient (fun, coloré — pas de noir plat). 8-wide tex, 256-tall.
                bgSr.sprite = CreateDuskGradientSprite();
                var sp = bgSr.sprite.bounds.size;
                bgGo.transform.localScale = new Vector3(coverW / Mathf.Max(0.01f, sp.x), coverH / Mathf.Max(0.01f, sp.y), 1f);
            }

            // Overlay gradient subtil haut+bas pour lisibilité de l'UI (léger, garde le fond visible).
            var ovGo = new GameObject("BgOverlay", typeof(SpriteRenderer));
            ovGo.transform.SetParent(root.transform, false);
            ovGo.transform.position = new Vector3(0, 0, 9.5f);
            var ovSr = ovGo.GetComponent<SpriteRenderer>();
            ovSr.sprite = CreateVerticalShadeSprite();
            ovSr.sortingOrder = -9;
            var ovSp = ovSr.sprite.bounds.size;
            ovGo.transform.localScale = new Vector3(coverW / Mathf.Max(0.01f, ovSp.x), coverH / Mathf.Max(0.01f, ovSp.y), 1f);

            // Ambient particles (amber motes drifting up) — garde le côté vivant.
            BuildAmbientParticles(root.transform, tokens);
        }

        /// <summary>Fallback dusk gradient (purple→pink→orange) when no illustrated bg is present.</summary>
        private static Sprite CreateDuskGradientSprite()
        {
            const int w = 8, h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var top = new Color(1.00f, 0.60f, 0.42f);    // orange chaud
            var mid = new Color(0.94f, 0.38f, 0.48f);    // rose
            var bot = new Color(0.23f, 0.14f, 0.31f);    // violet sombre
            var px = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                var t = y / (float)(h - 1); // 0 bottom → 1 top
                Color c = t > 0.5f ? Color.Lerp(mid, top, (t - 0.5f) * 2f) : Color.Lerp(bot, mid, t * 2f);
                for (var x = 0; x < w; x++) px[y * w + x] = c;
            }
            tex.SetPixels32(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
            s.name = "DuskGradient";
            return s;
        }

        /// <summary>Subtle top+bottom shade for UI legibility over the illustrated bg.</summary>
        private static Sprite CreateVerticalShadeSprite()
        {
            const int w = 8, h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                var t = y / (float)(h - 1);
                // darker at very top (8%) and very bottom (15%), clear in the middle.
                var aTop = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.92f, 1f, t)) * 0.35f;
                var aBot = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0f, t)) * 0.45f;
                var a = Mathf.Max(aTop, aBot);
                for (var x = 0; x < w; x++) px[y * w + x] = new Color32(10, 8, 14, (byte)(a * 255));
            }
            tex.SetPixels32(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
            s.name = "VerticalShade";
            return s;
        }

        private static void BuildAmbientParticles(Transform parent, DesignTokens tokens)
        {
            var go = new GameObject("AmbientParticles", typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0, -2.5f, 8f);

            var ps = go.GetComponent<ParticleSystem>();
            // Sprint 7.5 fix: a freshly-added ParticleSystem auto-plays; mutating main.duration while
            // it's playing throws "Setting the duration while system is still playing". Stop first.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 8f;
            main.loop = true;
            main.startLifetime = 6f;
            main.startSpeed = 0.25f;
            main.startSize = 0.04f;
            main.startColor = new Color(tokens.accentPrimary.r, tokens.accentPrimary.g, tokens.accentPrimary.b, 0.13f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = 0f;
            main.gravityModifier = -0.02f; // very slight upward drift

            var emission = ps.emission;
            emission.rateOverTime = 4.5f; // ~25-30 particles in flight at a time

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(5.5f, 0.2f, 1f); // emit along a horizontal strip at the bottom

            // Fade in then out across lifetime.
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(tokens.accentPrimary, 0f),
                    new GradientColorKey(tokens.accentPrimary, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.15f, 0.3f),
                    new GradientAlphaKey(0.10f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = grad;

            // Renderer settings.
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sortingOrder = -7;
            r.material = new Material(Shader.Find("Sprites/Default"));

            // Restart now that configuration is done (we stopped it above to set duration safely).
            ps.Play();
        }

        /// <summary>Procedural radial gradient sprite (256×256) — bright center, dark edges.</summary>
        private static Sprite CreateRadialGradientSprite(Color center, Color edge)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            var maxDist = size * 0.5f * 1.2f; // soft falloff
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - size * 0.5f;
                    var dy = y - size * 0.5f;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var t = Mathf.Clamp01(d / maxDist);
                    // Ease out so the bright center has more presence.
                    t = t * t;
                    var c = Color.Lerp(center, edge, t);
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            s.name = "RadialGradientBg";
            return s;
        }

        /// <summary>
        /// Procedural floor sprite: vertical gradient from dark wood to almost-black, with subtle
        /// vertical plank divisions. 256×64.
        /// </summary>
        private static Sprite CreateFloorSprite()
        {
            const int w = 256, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var top = new Color32(42, 26, 16, 255);     // #2a1a10
            var bottom = new Color32(26, 14, 8, 255);    // #1a0e08
            var plank = new Color32(18, 10, 6, 255);
            var pixels = new Color32[w * h];
            // 6 planks
            for (var y = 0; y < h; y++)
            {
                var t = 1f - (y / (float)h); // y=0 bottom -> t=1, y=h-1 top -> t=~0
                var rowColor = new Color32(
                    (byte)Mathf.Lerp(bottom.r, top.r, 1f - t),
                    (byte)Mathf.Lerp(bottom.g, top.g, 1f - t),
                    (byte)Mathf.Lerp(bottom.b, top.b, 1f - t),
                    255);
                for (var x = 0; x < w; x++)
                {
                    // Subtle plank divisions every ~42px.
                    var isDivider = (x % 42) < 2;
                    pixels[y * w + x] = isDivider ? plank : rowColor;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32);
            s.name = "FloorProcedural";
            return s;
        }
    }
}
