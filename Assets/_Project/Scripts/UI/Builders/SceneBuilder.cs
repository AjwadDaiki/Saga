using DG.Tweening;
using Saga.Core;
using Saga.Data;
using Saga.Gameplay;
using UnityEngine;

namespace Saga.UI.Builders
{
    /// <summary>
    /// Zone 4 — world-space entities: character + mannequin + adversaire/capitaine/maitre,
    /// FX spawners, combo meter and adversaire spawn view. Writes the resolved Transforms
    /// into the BuilderContext so Bootstrap can copy them to its public properties.
    /// </summary>
    public static class SceneBuilder
    {
        // World layout constants — mirrored verbatim from MainSceneBootstrap (Sprint 7.5 portrait pivot).
        // Sprint 7.5 portrait pivot: world X positions tightened so character + mannequin both fit
        // inside a 9:16 ortho frustum (orthoSize 3 → ±1.69 horizontal). Was (-1.8, 3.0) for landscape.
        // Y lowered to -1.3 so both stand on the floor strip top (floor centered at -2.3, half-height 1).
        private static readonly Vector3 CharacterPosition = new Vector3(-0.95f, -1.3f, 0f);
        private static readonly Vector3 MannequinPosition = new Vector3(1.05f, -1.3f, 0f);
        // Mannequin sprite is 80×140 @ PPU32 = 2.5×4.375 world. 0.7 scale → ~3.06 tall so the 2× chibi
        // (≈2 tall) reads at ~65% of its height — the balance Ajwad asked for.
        private const float MannequinScale = 0.7f;

        public static void Build(BuilderContext ctx)
        {
            var worldRoot = ctx.WorldRoot;

            ctx.CharacterTransform = BuildCharacter(worldRoot);
            ctx.MannequinTransform = BuildMannequin(worldRoot);
            ctx.AdversaireTransform = BuildAdversaire(worldRoot);
            ctx.CapitaineTransform = BuildCapitaine(worldRoot);
            ctx.MaitreTransform = BuildMaitre(worldRoot);

            BuildSlashFxSpawner(worldRoot, ctx.CharacterTransform, ctx.MannequinTransform);
        }

        /// <summary>
        /// Builds the per-canvas TapFxSpawner (NB: needs the canvas, called from Bootstrap after canvas exists).
        /// </summary>
        public static void BuildTapFxSpawner(Canvas canvas, Transform adversaireTransform)
        {
            var go = new GameObject("TapFxSpawner", typeof(TapFxSpawner));
            go.transform.SetParent(canvas.transform, false);
            var spawner = go.GetComponent<TapFxSpawner>();
            spawner.Init(canvas);
            // Combat "-X" damage numbers spawn from the enemy anchor. Adversaire + Capitaine share
            // the same world position, so either transform works — we pick Adversaire by convention.
            spawner.EnemyAnchor = adversaireTransform;
        }

        public static void BuildComboMeter(Canvas canvas)
        {
            var root = new GameObject("ComboMeter", typeof(RectTransform), typeof(CanvasGroup), typeof(ComboMeterView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            // Sprint 7.5 refonte : sous la Force pill (top-left) pour ne pas heurter Échos pill + settings (top-right).
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(24, -92);
            rt.sizeDelta = new Vector2(220, 60);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            labelGo.transform.SetParent(root.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<TMPro.TextMeshProUGUI>();
            label.alignment = TMPro.TextAlignmentOptions.MidlineRight;
            label.color = MainSceneBootstrap.TextSecondaryColor;
            label.fontSize = 56;
            label.text = "x1.0";

            var view = root.GetComponent<ComboMeterView>();
            view.Label = label;
            view.Group = group;
        }

        public static void BuildAdversaireSpawnView(Canvas canvas)
        {
            var root = new GameObject("AdversaireSpawnView",
                typeof(RectTransform), typeof(CanvasGroup), typeof(AdversaireSpawnView));
            root.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0, 0.55f); rt.anchorMax = new Vector2(1, 0.7f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var label = new GameObject("Name", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            label.transform.SetParent(rt, false);
            var labelRt = (RectTransform)label.transform;
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var labelTmp = label.GetComponent<TMPro.TextMeshProUGUI>();
            labelTmp.alignment = TMPro.TextAlignmentOptions.Center;
            labelTmp.color = new Color(0.98f, 0.78f, 0.46f, 1f); // ambre
            labelTmp.fontSize = 96;
            labelTmp.fontStyle = TMPro.FontStyles.Bold;
            labelTmp.text = "";
            labelTmp.raycastTarget = false;

            var view = root.GetComponent<AdversaireSpawnView>();
            view.Group = group;
            view.NameLabel = labelTmp;
        }

        // ------- World entities ---------------------------------------------

        private static Transform BuildCharacter(Transform parent)
        {
            // Sprint 7: layered character = 3 stacked SpriteRenderers (Body / Armor / Weapon),
            // driven by a single LayeredCharacterRenderer on the root. SaveService.Migrate guarantees
            // equippedBodyId defaults to body_chibi_neutral so the body slot is always visible.
            //
            // Sprint 7.5 fix: if the body SpriteLayerSet hasn't been generated yet (Editor utility
            // not run, or rvros sprites missing), fall back to a runtime placeholder so the player
            // is never invisible. Logs a warning telling the user how to fix it permanently.
            var go = new GameObject("Character", typeof(LayeredCharacterRenderer), typeof(CharacterView));
            go.transform.SetParent(parent, false);
            go.transform.position = CharacterPosition;

            // Ground shadow under the character. Built on a child so the parent's CharacterView
            // breathing scale doesn't squash the shadow with it (child compensates via inverse scale).
            BuildGroundShadow(go.transform, scale: new Vector3(0.45f, 0.22f, 1f));

            var body = new GameObject("Body", typeof(SpriteRenderer));
            body.transform.SetParent(go.transform, false);
            var bodySr = body.GetComponent<SpriteRenderer>();
            bodySr.sortingOrder = 5;

            var armor = new GameObject("Armor", typeof(SpriteRenderer));
            armor.transform.SetParent(go.transform, false);
            var armorSr = armor.GetComponent<SpriteRenderer>();
            armorSr.sortingOrder = 6;

            var weapon = new GameObject("Weapon", typeof(SpriteRenderer));
            weapon.transform.SetParent(go.transform, false);
            var weaponSr = weapon.GetComponent<SpriteRenderer>();
            weaponSr.sortingOrder = 7;

            var renderer = go.GetComponent<LayeredCharacterRenderer>();
            renderer.BodyRenderer = bodySr;
            renderer.ArmorRenderer = armorSr;
            renderer.WeaponRenderer = weaponSr;

            var gm = GameManager.Instance;
            var content = gm?.Content;
            var state = gm?.State;

            // Resolve body layer with safety fallback.
            SpriteLayerSet bodyLayer = null;
            if (content != null && state != null)
                bodyLayer = content.GetSpriteLayerSet(state.equippedBodyId);
            if (bodyLayer == null || bodyLayer.SpriteIdle == null || bodyLayer.SpriteIdle.Length == 0)
            {
                Debug.LogWarning("[MainSceneBootstrap] body_chibi_neutral SpriteLayerSet missing or has no idle frames — using runtime placeholder. Run 'Saga > Sprint 7 > Generate Sprite Layer Sets' to populate proper sprites.");
                bodyLayer = CreatePlaceholderBodyLayerSet();
            }

            renderer.SetLayer(EquipmentSlot.Body, bodyLayer);
            renderer.SetLayer(EquipmentSlot.Armor, content?.GetSpriteLayerSet(state?.equippedArmorId));
            renderer.SetLayer(EquipmentSlot.Weapon, content?.GetSpriteLayerSet(state?.equippedWeaponId));

            var view = go.GetComponent<CharacterView>();
            view.Renderer = renderer;
            // Sprint 7.5 fix: set base scale AFTER construction (Awake/OnEnable already ran during
            // `new GameObject`). 2x so the chibi reads at ~60% of the mannequin height in portrait.
            view.SetBaseScale(new Vector3(2f, 2f, 1f));

            return go.transform;
        }

        /// <summary>
        /// Runtime fallback used when the body SpriteLayerSet asset is missing. Produces a
        /// 64×96 ambre rectangle so the character is at least visible at the canonical
        /// CharacterPosition. Replaced as soon as the player equips a real body layer set.
        /// </summary>
        private static SpriteLayerSet CreatePlaceholderBodyLayerSet()
        {
            var sprite = CreatePlaceholderBodySprite();
            return SpriteLayerSet.CreateRuntime(
                id: EquipmentConstants.DefaultBodyId,
                slot: EquipmentSlot.Body,
                idle: new[] { sprite },
                displayName: "Corps (placeholder)");
        }

        private static Sprite CreatePlaceholderBodySprite()
        {
            // 64×96 ambre block with a subtle darker outline so the silhouette reads as
            // "humanoid placeholder" without faking detail.
            const int w = 64;
            const int h = 96;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var amber = new Color32(250, 199, 117, 255);     // 0.98, 0.78, 0.46
            var outline = new Color32(160, 110, 60, 255);
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var isEdge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    pixels[y * w + x] = isEdge ? outline : amber;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            sprite.name = "BodyPlaceholderProcedural";
            return sprite;
        }

        private static Transform BuildMannequin(Transform parent)
        {
            var go = new GameObject("Mannequin", typeof(SpriteRenderer), typeof(MannequinView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;
            go.transform.localScale = new Vector3(MannequinScale, MannequinScale, 1f);

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = CreateMannequinSprite();
            sr.color = Color.white;
            sr.sortingOrder = 5;

            // Sprint 7.5: subtle ground shadow + gentle sway DOTween so the dojo feels alive.
            BuildGroundShadow(go.transform, scale: new Vector3(1.3f, 0.7f, 1f), yOffset: 0.02f);

            // Slow ±2° rotation, infinite yoyo. SetLink ensures the tween dies with the GO.
            go.transform.rotation = Quaternion.Euler(0, 0, -2f);
            go.transform.DORotate(new Vector3(0, 0, 2f), 2.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(go, LinkBehaviour.KillOnDestroy);

            return go.transform;
        }

        private static Transform BuildAdversaire(Transform parent)
        {
            // Same position as mannequin — they're mutually exclusive (phase-driven visibility).
            var go = new GameObject("Adversaire", typeof(SpriteRenderer), typeof(AdversaireWorldView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            sr.color = new Color(1, 1, 1, 0); // start invisible

            var view = go.GetComponent<AdversaireWorldView>();
            view.Renderer = sr;

            return go.transform;
        }

        private static Transform BuildCapitaine(Transform parent)
        {
            // Same world position as Adversaire (mutually exclusive). Aura is a child renderer.
            var go = new GameObject("Capitaine", typeof(CapitaineWorldView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            // Aura first (child), drawn behind body.
            var auraGo = new GameObject("Aura", typeof(SpriteRenderer));
            auraGo.transform.SetParent(go.transform, false);
            var aura = auraGo.GetComponent<SpriteRenderer>();
            aura.sortingOrder = 3;
            aura.color = new Color(1, 1, 1, 0);

            // Body
            var bodyGo = new GameObject("Body", typeof(SpriteRenderer));
            bodyGo.transform.SetParent(go.transform, false);
            var body = bodyGo.GetComponent<SpriteRenderer>();
            body.sortingOrder = 6;
            body.color = new Color(1, 1, 1, 0);

            var view = go.GetComponent<CapitaineWorldView>();
            view.BodyRenderer = body;
            view.AuraRenderer = aura;

            return go.transform;
        }

        private static Transform BuildMaitre(Transform parent)
        {
            var go = new GameObject("Maitre", typeof(MaitreWorldView));
            go.transform.SetParent(parent, false);
            go.transform.position = MannequinPosition;

            var auraGo = new GameObject("Aura", typeof(SpriteRenderer));
            auraGo.transform.SetParent(go.transform, false);
            var aura = auraGo.GetComponent<SpriteRenderer>();
            aura.sortingOrder = 3;
            aura.color = new Color(1, 1, 1, 0);

            var bodyGo = new GameObject("Body", typeof(SpriteRenderer));
            bodyGo.transform.SetParent(go.transform, false);
            var body = bodyGo.GetComponent<SpriteRenderer>();
            body.sortingOrder = 6;
            body.color = new Color(1, 1, 1, 0);

            var view = go.GetComponent<MaitreWorldView>();
            view.BodyRenderer = body;
            view.AuraRenderer = aura;

            return go.transform;
        }

        private static void BuildSlashFxSpawner(Transform parent, Transform characterTransform, Transform mannequinTransform)
        {
            var go = new GameObject("SlashFxSpawner", typeof(SlashFxSpawner));
            go.transform.SetParent(parent, false);
            var s = go.GetComponent<SlashFxSpawner>();
            s.CharacterTransform = characterTransform;
            s.MannequinTransform = mannequinTransform;
        }

        /// <summary>
        /// Adds a soft elliptical shadow at the feet of a world entity. Sorted just above the floor
        /// (-7) and below all character/mannequin/adversaire renderers (5+).
        /// </summary>
        private static SpriteRenderer BuildGroundShadow(Transform parent, Vector3 scale, float yOffset = 0f)
        {
            var go = new GameObject("Shadow", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, yOffset, 0.1f);
            go.transform.localScale = scale;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = CreateShadowSprite();
            sr.sortingOrder = -5; // above floor (-8/-7), below characters (5+)
            return sr;
        }

        /// <summary>
        /// Sprint 7.5 mannequin redesign — 3 distinct sections (head sphere / torso cylinder / wider socle)
        /// with cordage rings, dark wood gradient and subtle edge shadow. 80×140 procedural.
        /// </summary>
        private static Sprite CreateMannequinSprite()
        {
            const int w = 80, h = 140;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];

            // Wood palette (dark base, lighter highlights along the centerline).
            var woodBase = new Color32(58, 42, 24, 255);    // #3a2a18
            var woodLight = new Color32(90, 64, 40, 255);   // #5a4028 — center highlight
            var woodDark = new Color32(34, 22, 12, 255);    // edge shadow
            var cord = new Color32(15, 9, 5, 255);          // pitch black cordage

            // Section heights (from bottom).
            const int socleTop = 22;   // 0..22  : socle (wider)
            const int torsoTop = 100;  // 22..100 : torso (medium cylinder)
            // 100..140 : head (narrowest, capped silhouette)

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var dx = Mathf.Abs(x - w * 0.5f);

                    float halfWidth;
                    if (y < socleTop)
                    {
                        // Socle: widest at the bottom, tapers slightly up.
                        var t = y / (float)socleTop;
                        halfWidth = Mathf.Lerp(0.40f, 0.32f, t) * w;
                    }
                    else if (y < torsoTop)
                    {
                        // Torso: medium cylinder with a very subtle barrel.
                        var t = (y - socleTop) / (float)(torsoTop - socleTop);
                        halfWidth = (0.26f + 0.02f * Mathf.Sin(t * Mathf.PI)) * w;
                    }
                    else
                    {
                        // Head: smaller sphere capped on top.
                        var t = (y - torsoTop) / (float)(h - torsoTop);
                        // Half-circle outline.
                        var r = 0.22f * w;
                        var dyHead = (y - torsoTop) - r * 0.6f;
                        var dist = Mathf.Sqrt(dx * dx + dyHead * dyHead);
                        if (dist > r) { pixels[y * w + x] = new Color32(0, 0, 0, 0); continue; }
                        halfWidth = w; // already inside the circle test
                    }

                    var inside = dx <= halfWidth;
                    if (!inside) { pixels[y * w + x] = new Color32(0, 0, 0, 0); continue; }

                    // Vertical highlight along the centerline → 3D feel.
                    var centerWeight = 1f - Mathf.Clamp01(dx / Mathf.Max(1f, halfWidth));
                    var edgeWeight = 1f - centerWeight;
                    var r2 = (byte)Mathf.Lerp(woodBase.r, woodLight.r, centerWeight * 0.6f);
                    var g2 = (byte)Mathf.Lerp(woodBase.g, woodLight.g, centerWeight * 0.6f);
                    var b2 = (byte)Mathf.Lerp(woodBase.b, woodLight.b, centerWeight * 0.6f);
                    if (edgeWeight > 0.85f)
                    {
                        r2 = woodDark.r; g2 = woodDark.g; b2 = woodDark.b;
                    }

                    // Cordage rings: thin horizontal stripes at torso y=42, y=72.
                    var isCord = (y == 42 || y == 43 || y == 72 || y == 73) && y > socleTop && y < torsoTop;
                    if (isCord) { r2 = cord.r; g2 = cord.g; b2 = cord.b; }

                    pixels[y * w + x] = new Color32(r2, g2, b2, 255);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            // PPU 32: 80×140 px → 2.5×4.375 world units. Pivot bottom-center keeps the socle on the floor.
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), pixelsPerUnit: 32);
            sprite.name = "MannequinProcedural";
            return sprite;
        }

        /// <summary>Soft black ellipse — used as ground shadow under the player + mannequin.</summary>
        private static Sprite CreateShadowSprite()
        {
            const int w = 96, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var dx = (x - w * 0.5f) / (w * 0.5f);
                    var dy = (y - h * 0.5f) / (h * 0.5f);
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var a = Mathf.Clamp01(1f - d);
                    a = a * a * 0.55f; // softer falloff
                    pixels[y * w + x] = new Color32(0, 0, 0, (byte)(a * 255));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit: 96);
            s.name = "ShadowEllipseProcedural";
            return s;
        }
    }
}
