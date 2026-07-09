using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Lionrise
{
    public sealed class ReignsBackdropController : MonoBehaviour
    {
        private static readonly Vector2[] ReferencePositions =
        {
            new Vector2(-6f, 303f), new Vector2(-37f, 174f), new Vector2(-7f, 116f),
            new Vector2(67f, 65f), new Vector2(69f, 85f), new Vector2(28f, 175f)
        };

        // BackAct in the reference begins each layer at endPos - startPos and
        // then eases it to endPos over three seconds.
        private static readonly Vector2[] ReferenceEntryOffsets =
        {
            new Vector2(0f, 82f), new Vector2(0f, -311f), new Vector2(0f, -303f),
            new Vector2(0f, -391f), new Vector2(0f, -573f), new Vector2(0f, -601f)
        };

        private BackdropSkyGraphic sky;
        private BackdropLayerGraphic[] graphics;
        private RectTransform[] layers;
        private Vector2[] restingPositions;
        private EdgeVignetteGraphic vignette;
        private int stage;
        private int seed;
        private bool transitioning;
        private float danger;

        public void Build()
        {
            sky = gameObject.AddComponent<BackdropSkyGraphic>();
            sky.raycastTarget = false;

            graphics = new BackdropLayerGraphic[6];
            layers = new RectTransform[6];
            restingPositions = new Vector2[6];
            for (var i = 0; i < layers.Length; i++)
            {
                var layer = LionriseUI.CreateRect("Generated Back Layer " + i, transform, new Vector2(.5f, 0f), new Vector2(.5f, 0f));
                layer.sizeDelta = new Vector2(1400f, 600f);
                layer.pivot = new Vector2(.5f, 0f);
                var graphic = layer.gameObject.AddComponent<BackdropLayerGraphic>();
                graphic.LayerIndex = i;
                graphic.raycastTarget = false;
                layers[i] = layer;
                graphics[i] = graphic;
            }

            var edge = LionriseUI.CreateRect("Cinematic Edge Shade", transform, Vector2.zero, Vector2.one);
            vignette = edge.gameObject.AddComponent<EdgeVignetteGraphic>();
            vignette.raycastTarget = false;
            SetScene(0, 2165, true);
        }

        public void SetScene(int newStage, int newSeed, bool immediate = false)
        {
            newStage = Mathf.Clamp(newStage, 0, 3);
            var changed = newStage != stage;
            stage = newStage;
            seed = newSeed;
            sky.Configure(stage, danger);
            vignette.Configure(stage, danger);

            for (var i = 0; i < graphics.Length; i++)
            {
                graphics[i].Configure(stage, seed);
                var jitter = new Vector2(Hash(seed + i * 71, -18f, 18f), Hash(seed + i * 131, -9f, 9f));
                var stageShift = new Vector2((stage - 1.5f) * (i - 2.5f) * 3f, stage * (i % 2 == 0 ? 7f : -5f));
                restingPositions[i] = ReferencePositions[i] + jitter + stageShift;
            }

            StopAllCoroutines();
            if (immediate)
            {
                transitioning = false;
                for (var i = 0; i < layers.Length; i++) layers[i].anchoredPosition = restingPositions[i];
            }
            else StartCoroutine(TransitionLayers(changed ? 3f : 1.15f, changed));
        }

        public void FlashDanger()
        {
            StopCoroutine(nameof(DangerPulse));
            StartCoroutine(nameof(DangerPulse));
        }

        private IEnumerator TransitionLayers(float duration, bool fullEntrance)
        {
            transitioning = true;
            var starts = new Vector2[layers.Length];
            for (var i = 0; i < layers.Length; i++)
            {
                starts[i] = fullEntrance ? restingPositions[i] + ReferenceEntryOffsets[i] : layers[i].anchoredPosition;
                layers[i].anchoredPosition = starts[i];
                graphics[i].TransitionAmount = 0f;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, fullEntrance ? 3f : 5f);
                for (var i = 0; i < layers.Length; i++)
                {
                    layers[i].anchoredPosition = Vector2.LerpUnclamped(starts[i], restingPositions[i], eased);
                    graphics[i].TransitionAmount = eased;
                }
                yield return null;
            }

            for (var i = 0; i < layers.Length; i++)
            {
                layers[i].anchoredPosition = restingPositions[i];
                graphics[i].TransitionAmount = 1f;
            }
            transitioning = false;
        }

        private IEnumerator DangerPulse()
        {
            danger = 1f;
            for (var elapsed = 0f; elapsed < 1.2f; elapsed += Time.unscaledDeltaTime)
            {
                danger = 1f - elapsed / 1.2f;
                sky.Configure(stage, danger);
                vignette.Configure(stage, danger);
                yield return null;
            }
            danger = 0f;
            sky.Configure(stage, 0f);
            vignette.Configure(stage, 0f);
        }

        private void Update()
        {
            if (transitioning || layers == null) return;
            var pointer = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                var position = Mouse.current.position.ReadValue();
                pointer = new Vector2(position.x / Mathf.Max(1f, Screen.width) - .5f,
                    position.y / Mathf.Max(1f, Screen.height) - .5f);
            }
#else
            pointer = new Vector2(Input.mousePosition.x / Mathf.Max(1f, Screen.width) - .5f,
                Input.mousePosition.y / Mathf.Max(1f, Screen.height) - .5f);
#endif
            var time = Time.unscaledTime;
            for (var i = 0; i < layers.Length; i++)
            {
                var depth = (i + 1f) / layers.Length;
                var breathe = new Vector2(Mathf.Sin(time * (.08f + i * .013f) + i) * (1f + i * .18f),
                    Mathf.Cos(time * (.06f + i * .011f) + i * .7f) * (.5f + i * .12f));
                layers[i].anchoredPosition = restingPositions[i] + breathe + pointer * depth * 9f;
            }
        }

        private static float Hash(int value, float min, float max)
        {
            unchecked
            {
                var x = (uint)value;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                return Mathf.Lerp(min, max, (x & 0xffff) / 65535f);
            }
        }
    }

    public sealed class BackdropSkyGraphic : MaskableGraphic
    {
        private int stage;
        private float danger;

        public void Configure(int newStage, float newDanger)
        {
            stage = newStage;
            danger = newDanger;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var bottom = Palette(stage, 0);
            var top = Palette(stage, 1);
            bottom = Color32.Lerp(bottom, new Color32(95, 8, 18, 255), danger * .7f);
            top = Color32.Lerp(top, new Color32(31, 0, 6, 255), danger * .7f);
            MeshPrimitives.AddGradient(vh, r.xMin, r.yMin, r.xMax, r.yMax, bottom, top);
        }

        internal static Color32 Palette(int stage, int tone)
        {
            var palettes = new[,]
            {
                { new Color32(77, 27, 19, 255), new Color32(27, 20, 31, 255), new Color32(164, 68, 38, 255), new Color32(245, 183, 97, 255), new Color32(72, 25, 27, 255) },
                { new Color32(42, 29, 49, 255), new Color32(20, 27, 43, 255), new Color32(110, 68, 91, 255), new Color32(244, 175, 83, 255), new Color32(48, 37, 58, 255) },
                { new Color32(11, 38, 52, 255), new Color32(13, 21, 39, 255), new Color32(32, 92, 113, 255), new Color32(240, 119, 65, 255), new Color32(17, 51, 65, 255) },
                { new Color32(16, 47, 45, 255), new Color32(16, 24, 39, 255), new Color32(48, 105, 86, 255), new Color32(241, 207, 130, 255), new Color32(22, 59, 54, 255) }
            };
            return palettes[Mathf.Clamp(stage, 0, 3), Mathf.Clamp(tone, 0, 4)];
        }
    }

    public sealed class BackdropLayerGraphic : MaskableGraphic
    {
        public int LayerIndex { get; set; }
        private float transitionAmount = 1f;
        public float TransitionAmount
        {
            get => transitionAmount;
            set
            {
                transitionAmount = value;
                SetVerticesDirty();
            }
        }
        private int stage;
        private int seed;

        public void Configure(int newStage, int newSeed)
        {
            stage = newStage;
            seed = newSeed;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var dark = BackdropSkyGraphic.Palette(stage, 4);
            var mid = BackdropSkyGraphic.Palette(stage, 2);
            var accent = BackdropSkyGraphic.Palette(stage, 3);
            var fade = Mathf.Lerp(.28f, 1f, transitionAmount);
            dark.a = (byte)(dark.a * fade);
            mid.a = (byte)(mid.a * fade);
            accent.a = (byte)(accent.a * fade);

            switch (LayerIndex)
            {
                case 0: DrawStars(vh, r, accent); break;
                case 1: DrawPlanet(vh, r, mid, accent); break;
                case 2: DrawSkyline(vh, r, dark, mid, 30, .16f, .42f); break;
                case 3: DrawSkyline(vh, r, mid, accent, 17, .08f, .31f); break;
                case 4: DrawInfrastructure(vh, r, dark, accent); break;
                default: DrawForeground(vh, r, dark, mid); break;
            }
        }

        private void DrawStars(VertexHelper vh, Rect r, Color32 color)
        {
            color.a = (byte)Mathf.Min(color.a, 145);
            for (var i = 0; i < 58; i++)
            {
                var x = Mathf.Lerp(r.xMin, r.xMax, Hash(i * 73 + seed, 0f, 1f));
                var y = Mathf.Lerp(r.yMin + r.height * .34f, r.yMax, Hash(i * 113 + seed, 0f, 1f));
                var size = i % 11 == 0 ? 4f : 1.4f;
                MeshPrimitives.AddDiamond(vh, new Vector2(x, y), size, color);
            }
        }

        private void DrawPlanet(VertexHelper vh, Rect r, Color32 mid, Color32 accent)
        {
            var radius = r.height * (.34f + stage * .025f);
            var center = new Vector2(r.center.x + Hash(seed, -180f, 180f), r.yMin + r.height * (.31f + stage * .035f));
            mid.a = 205;
            MeshPrimitives.AddCircle(vh, center, radius, 48, mid);
            accent.a = 95;
            MeshPrimitives.AddRing(vh, center, radius * 1.28f, 4f, 56, accent, .12f);
            MeshPrimitives.AddRing(vh, center, radius * .78f, 2f, 48, accent, .32f);
        }

        private void DrawSkyline(VertexHelper vh, Rect r, Color32 low, Color32 high, int count, float minHeight, float maxHeight)
        {
            var baseline = r.yMin + r.height * (.08f + LayerIndex * .025f);
            var width = r.width / count * 1.08f;
            for (var i = 0; i < count; i++)
            {
                var x = r.xMin + i * r.width / count;
                var h = r.height * Hash(seed + i * 97 + LayerIndex * 211, minHeight, maxHeight + stage * .025f);
                var c = Color32.Lerp(low, high, Hash(seed + i * 43, 0f, .62f));
                MeshPrimitives.AddTaperedTower(vh, x, baseline, width, h, c, i % 4);
                if (i % 3 == 0)
                {
                    var window = high;
                    window.a = 115;
                    MeshPrimitives.AddQuad(vh, x + width * .42f, baseline + h * .48f, x + width * .52f, baseline + h * .79f, window);
                }
            }
        }

        private void DrawInfrastructure(VertexHelper vh, Rect r, Color32 dark, Color32 accent)
        {
            var baseY = r.yMin + r.height * .09f;
            for (var i = 0; i < 8; i++)
            {
                var x = Mathf.Lerp(r.xMin + 80f, r.xMax - 80f, i / 7f);
                var radius = 30f + (i % 3) * 14f + stage * 5f;
                MeshPrimitives.AddRing(vh, new Vector2(x, baseY + radius * .25f), radius, 6f, 28, accent, .5f);
                MeshPrimitives.AddQuad(vh, x - radius, baseY - 8f, x + radius, baseY + 3f, dark);
            }
            accent.a = 150;
            MeshPrimitives.AddRing(vh, new Vector2(r.center.x, baseY + 44f), r.width * .34f, 3f, 64, accent, .08f);
        }

        private void DrawForeground(VertexHelper vh, Rect r, Color32 dark, Color32 mid)
        {
            var points = 26;
            var baseline = r.yMin;
            for (var i = 0; i < points; i++)
            {
                var x0 = Mathf.Lerp(r.xMin, r.xMax, i / (float)points);
                var x1 = Mathf.Lerp(r.xMin, r.xMax, (i + 1f) / points);
                var h = r.height * Hash(seed + i * 157, .035f, .15f + stage * .012f);
                var c = Color32.Lerp(dark, mid, i % 4 == 0 ? .35f : .06f);
                MeshPrimitives.AddTriangle(vh, new Vector2(x0, baseline), new Vector2(x1, baseline),
                    new Vector2((x0 + x1) * .5f, baseline + h), c);
            }
        }

        private static float Hash(int value, float min, float max)
        {
            unchecked
            {
                var x = (uint)value;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                return Mathf.Lerp(min, max, (x & 0xffff) / 65535f);
            }
        }
    }

    public sealed class EdgeVignetteGraphic : MaskableGraphic
    {
        private int stage;
        private float danger;

        public void Configure(int newStage, float newDanger)
        {
            stage = newStage;
            danger = newDanger;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var edge = new Color32(8, 5, 9, (byte)Mathf.Lerp(105f, 178f, danger));
            var clear = new Color32(8, 5, 9, 0);
            var width = r.width * .13f;
            MeshPrimitives.AddHorizontalGradient(vh, r.xMin, r.yMin, r.xMin + width, r.yMax, edge, clear);
            MeshPrimitives.AddHorizontalGradient(vh, r.xMax - width, r.yMin, r.xMax, r.yMax, clear, edge);
        }
    }

    public sealed class PortraitBlinkController : MonoBehaviour
    {
        private Image image;
        private RectTransform rect;
        private Sprite open;
        private Sprite blink;
        private float nextBlink;
        private float blinkUntil;
        private Vector2 basePosition;
        private float react;
        private float reactTarget;
        private bool hostile;

        public void Configure(Image target)
        {
            image = target;
            rect = target.rectTransform;
            basePosition = rect.anchoredPosition;
        }

        public void SetFrames(Sprite openFrame, Sprite blinkFrame, bool quickBlink)
        {
            open = openFrame;
            blink = blinkFrame;
            hostile = quickBlink;
            image.sprite = open;
            nextBlink = Time.unscaledTime + (hostile ? UnityEngine.Random.Range(2f, 2.9f) : UnityEngine.Random.Range(12.4f, 12.9f));
            blinkUntil = 0f;
            react = reactTarget = 0f;
        }

        public void React(ChoiceSide side, float strength)
        {
            reactTarget = (side == ChoiceSide.Left ? -1f : 1f) * Mathf.Clamp01(strength);
        }

        public void ClearReaction() => reactTarget = 0f;

        private void Update()
        {
            if (image == null || open == null) return;
            var now = Time.unscaledTime;
            if (blink != null && blinkUntil <= 0f && now >= nextBlink)
            {
                image.sprite = blink;
                blinkUntil = now + UnityEngine.Random.Range(.3f, .5f);
            }
            if (blinkUntil > 0f && now >= blinkUntil)
            {
                image.sprite = open;
                blinkUntil = 0f;
                nextBlink = now + (hostile ? UnityEngine.Random.Range(2f, 2.9f) : UnityEngine.Random.Range(12.4f, 12.9f));
            }

            react = Mathf.Lerp(react, reactTarget, Time.unscaledDeltaTime * 9f);
            var breathe = Mathf.Sin(now * 1.25f) * .75f;
            rect.anchoredPosition = basePosition + new Vector2(react * 3.2f, breathe);
            rect.localRotation = Quaternion.Euler(0f, react * -2.5f, react * -1.1f);
            var scale = 1f + Mathf.Sin(now * 1.25f) * .0035f + Mathf.Abs(react) * .012f;
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public enum SpecialCardMode { None, Route, Fight, Concert }

    public sealed class ReignsSpecialCardGraphic : MaskableGraphic
    {
        private SpecialCardMode mode;
        private float phase;
        private float input;

        public void SetMode(SpecialCardMode value)
        {
            mode = value;
            gameObject.SetActive(mode != SpecialCardMode.None);
            phase = 0f;
            input = 0f;
            SetVerticesDirty();
        }

        public void SetInput(ChoiceSide side, float strength)
        {
            input = (side == ChoiceSide.Left ? -1f : 1f) * Mathf.Clamp01(strength);
        }

        public void ClearInput() => input = 0f;

        private void Update()
        {
            if (mode == SpecialCardMode.None) return;
            phase += Time.unscaledDeltaTime;
            input = Mathf.Lerp(input, 0f, Time.unscaledDeltaTime * .8f);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            MeshPrimitives.AddQuad(vh, r.xMin, r.yMin, r.xMax, r.yMax, new Color32(15, 13, 24, 255));
            switch (mode)
            {
                case SpecialCardMode.Route: DrawRoute(vh, r); break;
                case SpecialCardMode.Fight: DrawFight(vh, r); break;
                case SpecialCardMode.Concert: DrawConcert(vh, r); break;
            }
        }

        private void DrawRoute(VertexHelper vh, Rect r)
        {
            var center = r.center + new Vector2(0f, -115f);
            var rotation = phase * 10f + input * 46f;
            MeshPrimitives.AddRing(vh, center, 285f, 2.5f, 72, new Color32(74, 188, 206, 155), rotation / 360f);
            for (var ring = 1; ring <= 3; ring++)
                MeshPrimitives.AddRing(vh, center, ring * 71f, 1.4f, 48, new Color32(239, 211, 132, (byte)(70 + ring * 20)), rotation / 360f);
            for (var i = 0; i < 22; i++)
            {
                var a = (i * 360f / 22f + rotation) * Mathf.Deg2Rad;
                var radius = 62f + (i % 3) * 67f;
                var p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                MeshPrimitives.AddDiamond(vh, p, i % 5 == 0 ? 8f : 4f, i % 5 == 0 ? new Color32(244, 112, 62, 255) : new Color32(241, 220, 155, 220));
            }
            MeshPrimitives.AddDiamond(vh, r.center + new Vector2(0f, 15f), 12f, new Color32(245, 184, 84, 255));
        }

        private void DrawFight(VertexHelper vh, Rect r)
        {
            var focus = r.center + new Vector2(input * -38f, Mathf.Sin(phase * 1.8f) * 18f);
            for (var i = 0; i < 34; i++)
            {
                var t = Mathf.Repeat(phase * .46f + i / 34f, 1f);
                var a = i * 2.399963f;
                var p = focus + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * t * r.width * .62f;
                MeshPrimitives.AddDiamond(vh, p, 1f + t * 5f, new Color32(214, 236, 235, (byte)(60 + t * 170f)));
            }
            var ship = r.center + new Vector2(input * 52f, -28f);
            MeshPrimitives.AddTriangle(vh, ship + new Vector2(0f, 26f), ship + new Vector2(-18f, -20f), ship + new Vector2(18f, -20f), new Color32(242, 190, 87, 255));
            MeshPrimitives.AddTriangle(vh, ship + new Vector2(0f, 14f), ship + new Vector2(-8f, -15f), ship + new Vector2(8f, -15f), new Color32(31, 82, 112, 255));
            var target = r.center + new Vector2(Mathf.Sin(phase * .83f) * 75f, 58f + Mathf.Cos(phase * .61f) * 45f);
            MeshPrimitives.AddRing(vh, target, 25f, 2f, 24, new Color32(240, 80, 74, 235), 0f);
            MeshPrimitives.AddQuad(vh, target.x - 36f, target.y - 1f, target.x + 36f, target.y + 1f, new Color32(240, 80, 74, 180));
        }

        private void DrawConcert(VertexHelper vh, Rect r)
        {
            var gold = new Color32(245, 187, 79, 255);
            var teal = new Color32(62, 195, 181, 255);
            var last = new Vector2(r.xMin, r.center.y);
            for (var i = 1; i <= 44; i++)
            {
                var x = Mathf.Lerp(r.xMin, r.xMax, i / 44f);
                var normalized = i / 44f;
                var y = r.center.y + Mathf.Sin(normalized * 19f + phase * 5.8f) * (18f + input * 13f) + Mathf.Sin(normalized * 7f - phase * 2f) * 9f;
                var next = new Vector2(x, y);
                MeshPrimitives.AddLine(vh, last, next, 2.4f, i % 2 == 0 ? gold : teal);
                last = next;
            }
            for (var i = 0; i < 14; i++)
            {
                var x = Mathf.Lerp(r.xMin + 6f, r.xMax - 6f, i / 13f);
                var h = 22f + (i % 4) * 8f + Mathf.Abs(Mathf.Sin(phase * 3f + i)) * 12f;
                MeshPrimitives.AddTriangle(vh, new Vector2(x - 12f, r.yMin), new Vector2(x + 12f, r.yMin), new Vector2(x, r.yMin + h), new Color32(49, 38, 61, 255));
            }
            for (var i = 0; i < 5; i++)
            {
                var t = Mathf.Repeat(phase * .22f + i / 5f, 1f);
                var p = new Vector2(r.center.x + Mathf.Sin(i * 3.1f) * 90f, r.yMin + t * r.height);
                MeshPrimitives.AddDiamond(vh, p, 5f + t * 4f, new Color32(239, 94, 91, (byte)(255f * (1f - t))));
            }
        }
    }

    internal static class MeshPrimitives
    {
        public static void AddQuad(VertexHelper vh, float x0, float y0, float x1, float y1, Color32 color)
        {
            AddGradient(vh, x0, y0, x1, y1, color, color);
        }

        public static void AddGradient(VertexHelper vh, float x0, float y0, float x1, float y1, Color32 bottom, Color32 top)
        {
            var i = vh.currentVertCount;
            vh.AddVert(new Vector3(x0, y0), bottom, Vector2.zero);
            vh.AddVert(new Vector3(x0, y1), top, Vector2.up);
            vh.AddVert(new Vector3(x1, y1), top, Vector2.one);
            vh.AddVert(new Vector3(x1, y0), bottom, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        public static void AddHorizontalGradient(VertexHelper vh, float x0, float y0, float x1, float y1, Color32 left, Color32 right)
        {
            var i = vh.currentVertCount;
            vh.AddVert(new Vector3(x0, y0), left, Vector2.zero);
            vh.AddVert(new Vector3(x0, y1), left, Vector2.up);
            vh.AddVert(new Vector3(x1, y1), right, Vector2.one);
            vh.AddVert(new Vector3(x1, y0), right, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        public static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            var i = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
        }

        public static void AddDiamond(VertexHelper vh, Vector2 center, float size, Color32 color)
        {
            var i = vh.currentVertCount;
            vh.AddVert(center + Vector2.up * size, color, Vector2.up);
            vh.AddVert(center + Vector2.right * size, color, Vector2.right);
            vh.AddVert(center + Vector2.down * size, color, Vector2.down);
            vh.AddVert(center + Vector2.left * size, color, Vector2.left);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        public static void AddCircle(VertexHelper vh, Vector2 center, float radius, int segments, Color32 color)
        {
            var c = vh.currentVertCount;
            vh.AddVert(center, color, Vector2.one * .5f);
            for (var i = 0; i <= segments; i++)
            {
                var a = i * Mathf.PI * 2f / segments;
                vh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color, Vector2.zero);
            }
            for (var i = 0; i < segments; i++) vh.AddTriangle(c, c + i + 1, c + i + 2);
        }

        public static void AddRing(VertexHelper vh, Vector2 center, float radius, float width, int segments, Color32 color, float rotation)
        {
            for (var i = 0; i < segments; i++)
            {
                var a0 = (i / (float)segments + rotation) * Mathf.PI * 2f;
                var a1 = ((i + 1f) / segments + rotation) * Mathf.PI * 2f;
                var inner = radius - width;
                var p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * inner;
                var p1 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
                var p2 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
                var p3 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * inner;
                var v = vh.currentVertCount;
                vh.AddVert(p0, color, Vector2.zero);
                vh.AddVert(p1, color, Vector2.zero);
                vh.AddVert(p2, color, Vector2.zero);
                vh.AddVert(p3, color, Vector2.zero);
                vh.AddTriangle(v, v + 1, v + 2);
                vh.AddTriangle(v, v + 2, v + 3);
            }
        }

        public static void AddTaperedTower(VertexHelper vh, float x, float baseline, float width, float height, Color32 color, int roof)
        {
            var inset = width * (roof % 2 == 0 ? .18f : .05f);
            AddQuad(vh, x + inset, baseline, x + width - inset, baseline + height, color);
            if (roof == 0 || roof == 3)
                AddTriangle(vh, new Vector2(x + inset, baseline + height), new Vector2(x + width - inset, baseline + height), new Vector2(x + width * .5f, baseline + height + width * .38f), color);
            else if (roof == 1)
                AddQuad(vh, x + width * .44f, baseline + height, x + width * .56f, baseline + height + width * .5f, color);
        }

        public static void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color32 color)
        {
            var dir = (b - a).normalized;
            var normal = new Vector2(-dir.y, dir.x) * width * .5f;
            var i = vh.currentVertCount;
            vh.AddVert(a - normal, color, Vector2.zero);
            vh.AddVert(a + normal, color, Vector2.up);
            vh.AddVert(b + normal, color, Vector2.one);
            vh.AddVert(b - normal, color, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
