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
            // Sprint 7.5 Polish Phase 3 — refonte BG procédural simplifié palette DA.
            // 4 couches sans torii / foreground (reportés Sprint 7.6 par décision coordinator) :
            //   z=10  Sky dusk DA gradient (orange chaud → rose sakura → lavande UI → panel sombre2)
            //   z=9.7 Mountains silhouettes (montagneDark / montagneMid)
            //   z=9.5 Pagoda silhouette centrée (toitJaponaisDark + boisDojoMid)
            //   z=9.3 Floor wooden band bottom (boisDojoMid → boisDojoDark gradient + planches)
            //   z=9.0 Overlay vertical shade (lisibilité UI)
            //   z=8.5 Sakura particles (existing)
            var parent = ctx.WorldRoot;
            var tokens = DesignTokens.Get();
            var root = new GameObject("Ambient");
            root.transform.SetParent(parent, false);

            var cam = Camera.main;
            var viewH = (cam != null ? cam.orthographicSize : 3f) * 2f;
            var aspect = (cam != null && cam.aspect > 0.01f) ? cam.aspect : (1080f / 1920f);
            var viewW = viewH * aspect;
            var coverW = viewW * 1.12f;
            var coverH = viewH * 1.12f;

            BuildSkyLayer(root.transform, tokens, coverW, coverH);
            BuildMountainsLayer(root.transform, tokens, coverW, viewH);
            BuildPagodaLayer(root.transform, tokens, viewW, viewH);
            BuildFloorLayer(root.transform, tokens, coverW, viewH);
            BuildOverlayLayer(root.transform, coverW, coverH);
            BuildAmbientParticles(root.transform, tokens);
        }

        // ====================================================================================
        //  LAYER 1 — SKY : 4-stop DA gradient sunset (orange → sakura → lavande → panel sombre).
        // ====================================================================================

        private static void BuildSkyLayer(Transform root, DesignTokens tokens, float coverW, float coverH)
        {
            var go = new GameObject("Sky", typeof(SpriteRenderer));
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0, 0, 10f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -10;
            sr.sprite = CreateDaSkyGradientSprite(tokens);
            var sp = sr.sprite.bounds.size;
            go.transform.localScale = new Vector3(coverW / Mathf.Max(0.01f, sp.x), coverH / Mathf.Max(0.01f, sp.y), 1f);
        }

        private static Sprite CreateDaSkyGradientSprite(DesignTokens tokens)
        {
            const int w = 8, h = 384;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            // DA palette stops (haut → bas) :
            //   1.0 orangeChaud #FF9F3D
            //   0.7 sakura clair (interpolation)
            //   0.4 sakura mid #FFB1D1
            //   0.18 lavande UI (un peu désaturée)
            //   0.0 panelSombre2 #2F374A
            var stops = new[] {
                (1.0f, tokens.orangeChaud),
                (0.72f, tokens.sakuraMid),
                (0.42f, tokens.lavandeUI),
                (0.0f, tokens.panelSombre2),
            };
            var px = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                var t = y / (float)(h - 1);
                Color c = stops[0].Item2;
                for (var i = 1; i < stops.Length; i++)
                {
                    if (t >= stops[i].Item1)
                    {
                        var prev = stops[i - 1];
                        var cur = stops[i];
                        var localT = Mathf.InverseLerp(cur.Item1, prev.Item1, t);
                        c = Color.Lerp(cur.Item2, prev.Item2, localT);
                        break;
                    }
                    c = stops[i].Item2;
                }
                for (var x = 0; x < w; x++) px[y * w + x] = c;
            }
            tex.SetPixels32(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
            s.name = "DaSkyGradient";
            return s;
        }

        // ====================================================================================
        //  LAYER 2 — MOUNTAINS : two triangular silhouette layers, montagneDark + montagneMid.
        // ====================================================================================

        private static void BuildMountainsLayer(Transform root, DesignTokens tokens, float coverW, float viewH)
        {
            var go = new GameObject("Mountains", typeof(SpriteRenderer));
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0, -viewH * 0.06f, 9.7f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -9;
            sr.sprite = CreateMountainsSilhouetteSprite(tokens);
            var sp = sr.sprite.bounds.size;
            go.transform.localScale = new Vector3(coverW / Mathf.Max(0.01f, sp.x), (viewH * 0.55f) / Mathf.Max(0.01f, sp.y), 1f);
        }

        private static Sprite CreateMountainsSilhouetteSprite(DesignTokens tokens)
        {
            // 256-wide bands of triangular peaks. 2 layers : back (mid) + front (dark).
            const int w = 512, h = 192;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var clear = new Color32(0, 0, 0, 0);
            var back = (Color32)tokens.montagneMid;
            var front = (Color32)tokens.montagneDark;
            var snow = (Color32)tokens.blancChaud;
            var px = new Color32[w * h];
            for (var i = 0; i < px.Length; i++) px[i] = clear;
            // Back row of bigger peaks (5 peaks).
            for (var x = 0; x < w; x++)
            {
                var n = 5;
                var stride = w / (float)n;
                var idx = Mathf.FloorToInt(x / stride);
                var localX = (x - idx * stride) / stride; // 0..1
                var peakHeight = 0.55f + 0.10f * ((idx % 2 == 0) ? 1f : -1f);
                var yLimit = peakHeight * (1f - Mathf.Abs(localX - 0.5f) * 2f);
                var yPixel = Mathf.FloorToInt(yLimit * h);
                for (var y = 0; y <= yPixel; y++)
                {
                    px[y * w + x] = back;
                    // Snow tip
                    if (y > yPixel - 6 && yPixel > h * 0.45f) px[y * w + x] = snow;
                }
            }
            // Front row of smaller peaks (7 peaks).
            for (var x = 0; x < w; x++)
            {
                var n = 7;
                var stride = w / (float)n;
                var idx = Mathf.FloorToInt(x / stride);
                var localX = (x - idx * stride) / stride;
                var peakHeight = 0.36f + 0.06f * ((idx % 3 == 0) ? 1f : ((idx % 2 == 0) ? 0.5f : -0.5f));
                var yLimit = peakHeight * (1f - Mathf.Abs(localX - 0.5f) * 2f);
                var yPixel = Mathf.FloorToInt(yLimit * h);
                for (var y = 0; y <= yPixel; y++)
                {
                    if (px[y * w + x].a < 255 || y > 0)
                        px[y * w + x] = front;
                }
            }
            tex.SetPixels32(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 32);
            s.name = "MountainsSilhouette";
            return s;
        }

        // ====================================================================================
        //  LAYER 3 — PAGODA : central stylized silhouette (toits japonais + bois).
        // ====================================================================================

        private static void BuildPagodaLayer(Transform root, DesignTokens tokens, float viewW, float viewH)
        {
            var go = new GameObject("Pagoda", typeof(SpriteRenderer));
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0.6f, -viewH * 0.04f, 9.5f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -8;
            sr.sprite = CreatePagodaSilhouetteSprite(tokens);
            var sp = sr.sprite.bounds.size;
            var targetH = viewH * 0.42f;
            var scale = targetH / Mathf.Max(0.01f, sp.y);
            go.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static Sprite CreatePagodaSilhouetteSprite(DesignTokens tokens)
        {
            // 3-tier pagoda : roof gables + body stripes. ~160×240 px.
            const int w = 160, h = 240;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var clear = new Color32(0, 0, 0, 0);
            var roof = (Color32)tokens.toitJaponaisDark;
            var roofMid = (Color32)tokens.toitJaponaisMid;
            var bodyMid = (Color32)tokens.boisDojoMid;
            var bodyDark = (Color32)tokens.boisDojoDark;
            var outline = (Color32)tokens.navyContour;
            var px = new Color32[w * h];
            for (var i = 0; i < px.Length; i++) px[i] = clear;

            // Three roof tiers + bodies, stacked.
            int[] tierBaseY = { 30, 110, 190 };          // y where the body of each tier starts
            int[] tierBodyH = { 50, 50, 30 };            // body height of each tier
            int[] tierRoofH = { 22, 22, 24 };            // roof height (drooping gable)
            float[] tierWidth = { 0.95f, 0.78f, 0.55f }; // proportion of full width

            for (var t = 0; t < 3; t++)
            {
                var bodyY0 = tierBaseY[t];
                var bodyY1 = bodyY0 + tierBodyH[t];
                var roofY0 = bodyY1;
                var roofY1 = roofY0 + tierRoofH[t];
                var bodyW = (int)(w * tierWidth[t]);
                var bodyX0 = (w - bodyW) / 2;
                var bodyX1 = bodyX0 + bodyW;

                // Body rectangle.
                for (var y = bodyY0; y < bodyY1 && y < h; y++)
                {
                    for (var x = bodyX0; x < bodyX1; x++)
                    {
                        var alt = ((x - bodyX0) % 14 < 4) ? bodyDark : bodyMid;
                        px[y * w + x] = alt;
                    }
                    // Outline at body edges.
                    if (bodyX0 >= 0) px[y * w + bodyX0] = outline;
                    if (bodyX1 - 1 < w) px[y * w + bodyX1 - 1] = outline;
                }

                // Roof : trapezoidal gable that hangs past body by ~12px each side, drooping corners.
                var hang = 12;
                var roofW = bodyW + hang * 2;
                var roofX0 = bodyX0 - hang;
                var roofX1 = bodyX1 + hang;
                for (var y = roofY0; y < roofY1 && y < h; y++)
                {
                    var ry = (y - roofY0) / (float)tierRoofH[t];
                    var droop = 1f - ry; // wider at base
                    var thisW = Mathf.RoundToInt(roofW * droop + bodyW * (1f - droop));
                    var thisX0 = (w - thisW) / 2;
                    var thisX1 = thisX0 + thisW;
                    for (var x = thisX0; x < thisX1 && x < w; x++)
                    {
                        if (x < 0) continue;
                        var alt = (ry > 0.6f) ? roofMid : roof;
                        px[y * w + x] = alt;
                    }
                    if (thisX0 >= 0 && thisX0 < w) px[y * w + thisX0] = outline;
                    if (thisX1 - 1 >= 0 && thisX1 - 1 < w) px[y * w + thisX1 - 1] = outline;
                }
            }

            // Ridge spire on top.
            var ridgeX = w / 2;
            for (var y = tierBaseY[2] + tierBodyH[2] + tierRoofH[2]; y < h && y < tierBaseY[2] + tierBodyH[2] + tierRoofH[2] + 18; y++)
            {
                if (ridgeX - 2 >= 0) px[y * w + ridgeX - 2] = outline;
                if (ridgeX - 1 >= 0) px[y * w + ridgeX - 1] = roof;
                if (ridgeX < w) px[y * w + ridgeX] = roof;
                if (ridgeX + 1 < w) px[y * w + ridgeX + 1] = outline;
            }

            tex.SetPixels32(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 32);
            s.name = "PagodaSilhouette";
            return s;
        }

        // ====================================================================================
        //  LAYER 4 — FLOOR : wooden band bottom with DA bois palette + plank divisions.
        // ====================================================================================

        private static void BuildFloorLayer(Transform root, DesignTokens tokens, float coverW, float viewH)
        {
            var go = new GameObject("Floor", typeof(SpriteRenderer));
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0, -viewH * 0.40f, 9.3f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -7;
            sr.sprite = CreateDaFloorSprite(tokens);
            var sp = sr.sprite.bounds.size;
            go.transform.localScale = new Vector3(coverW / Mathf.Max(0.01f, sp.x), (viewH * 0.22f) / Mathf.Max(0.01f, sp.y), 1f);
        }

        private static Sprite CreateDaFloorSprite(DesignTokens tokens)
        {
            // Wooden floor with DA palette §5.3 — gradient boisDojoLight (top) → boisDojoMid → boisDojoDark
            // + subtle plank divisions every ~36px.
            const int w = 384, h = 96;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var top = (Color32)tokens.boisDojoLight;       // #E2A15C
            var mid = (Color32)tokens.boisDojoMid;         // #B96F3A
            var bottom = (Color32)tokens.boisDojoDark;     // #8F4F2E
            var plank = (Color32)Color.Lerp(tokens.boisDojoDark, tokens.navyContour, 0.4f);
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                var t = y / (float)(h - 1); // 0 bottom -> 1 top
                Color rowColor;
                if (t > 0.5f) rowColor = Color.Lerp((Color)mid, (Color)top, (t - 0.5f) * 2f);
                else rowColor = Color.Lerp((Color)bottom, (Color)mid, t * 2f);
                var row32 = (Color32)rowColor;
                for (var x = 0; x < w; x++)
                {
                    var isDivider = (x % 36) < 3;
                    pixels[y * w + x] = isDivider ? plank : row32;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
            s.name = "DaFloorWood";
            return s;
        }

        // ====================================================================================
        //  LAYER 5 — OVERLAY : subtle top+bottom shade for UI legibility.
        // ====================================================================================

        private static void BuildOverlayLayer(Transform root, float coverW, float coverH)
        {
            var go = new GameObject("Overlay", typeof(SpriteRenderer));
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0, 0, 9.0f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = -6;
            sr.sprite = CreateVerticalShadeSprite();
            var sp = sr.sprite.bounds.size;
            go.transform.localScale = new Vector3(coverW / Mathf.Max(0.01f, sp.x), coverH / Mathf.Max(0.01f, sp.y), 1f);
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
