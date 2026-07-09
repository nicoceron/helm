using UnityEngine;
using UnityEngine.UI;

namespace Lionrise
{
    public sealed class CivicBackdrop : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            AddRect(vh, rect.xMin, rect.yMin, rect.xMax, rect.yMax,
                new Color32(37, 10, 19, 255), new Color32(92, 31, 43, 255));

            var line = new Color32(249, 232, 172, 10);
            for (var i = 0; i < 7; i++)
            {
                var x = Mathf.Lerp(rect.xMin, rect.xMax, i / 6f);
                AddSolid(vh, x, rect.yMin, x + 1f, rect.yMax, line);
            }
            for (var i = 0; i < 11; i++)
            {
                var y = Mathf.Lerp(rect.yMin, rect.yMax, i / 10f);
                AddSolid(vh, rect.xMin, y, rect.xMax, y + .7f, line);
            }

            var star = new Color32(255, 240, 181, 82);
            for (var i = 0; i < 31; i++)
            {
                var px = ((i * 73) % 101) / 100f;
                var py = ((i * 47 + 11) % 97) / 96f;
                var size = i % 9 == 0 ? 2.5f : 1f;
                var x = Mathf.Lerp(rect.xMin, rect.xMax, px);
                var y = Mathf.Lerp(rect.yMin + rect.height * .25f, rect.yMax, py);
                AddSolid(vh, x, y, x + size, y + size, star);
            }
        }

        private static void AddRect(VertexHelper vh, float x0, float y0, float x1, float y1, Color32 bottom, Color32 top)
        {
            var index = vh.currentVertCount;
            vh.AddVert(new Vector3(x0, y0), bottom, Vector2.zero);
            vh.AddVert(new Vector3(x0, y1), top, Vector2.up);
            vh.AddVert(new Vector3(x1, y1), top, Vector2.one);
            vh.AddVert(new Vector3(x1, y0), bottom, Vector2.right);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        internal static void AddSolid(VertexHelper vh, float x0, float y0, float x1, float y1, Color32 color)
        {
            AddRect(vh, x0, y0, x1, y1, color, color);
        }

    }

    public sealed class HoloCardGraphic : MaskableGraphic
    {
        public Color32 borderColor = new Color32(224, 194, 130, 225);
        public bool committed;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            var cut = Mathf.Min(rect.width, rect.height) * .012f;
            var points = new[]
            {
                new Vector2(rect.xMin + cut, rect.yMin), new Vector2(rect.xMax - cut, rect.yMin),
                new Vector2(rect.xMax, rect.yMin + cut), new Vector2(rect.xMax, rect.yMax - cut),
                new Vector2(rect.xMax - cut, rect.yMax), new Vector2(rect.xMin + cut, rect.yMax),
                new Vector2(rect.xMin, rect.yMax - cut), new Vector2(rect.xMin, rect.yMin + cut)
            };
            var fill = committed ? new Color32(73, 34, 20, 248) : new Color32(23, 12, 13, 248);
            var centerIndex = vh.currentVertCount;
            vh.AddVert(rect.center, fill, Vector2.one * .5f);
            for (var i = 0; i < points.Length; i++) vh.AddVert(points[i], fill, Vector2.zero);
            for (var i = 0; i < points.Length; i++)
                vh.AddTriangle(centerIndex, centerIndex + 1 + i, centerIndex + 1 + (i + 1) % points.Length);

            const float border = 2f;
            CivicBackdrop.AddSolid(vh, rect.xMin + cut, rect.yMin, rect.xMax - cut, rect.yMin + border, borderColor);
            CivicBackdrop.AddSolid(vh, rect.xMin + cut, rect.yMax - border, rect.xMax - cut, rect.yMax, borderColor);
            CivicBackdrop.AddSolid(vh, rect.xMin, rect.yMin + cut, rect.xMin + border, rect.yMax - cut, borderColor);
            CivicBackdrop.AddSolid(vh, rect.xMax - border, rect.yMin + cut, rect.xMax, rect.yMax - cut, borderColor);

        }

        public void SetCommitted(bool value)
        {
            committed = value;
            SetVerticesDirty();
        }
    }

    public sealed class LionMarkGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var c = new Color32(249, 232, 172, 235);
            AddTriangle(vh, new Vector2(r.center.x, r.yMax), new Vector2(r.xMin, r.yMin), new Vector2(r.center.x, r.yMin + r.height * .22f), c);
            AddTriangle(vh, new Vector2(r.center.x, r.yMax), new Vector2(r.center.x, r.yMin + r.height * .22f), new Vector2(r.xMax, r.yMin), c);
            AddTriangle(vh, new Vector2(r.center.x, r.yMax * .48f), new Vector2(r.xMin + r.width * .18f, r.yMin + r.height * .38f), new Vector2(r.center.x, r.yMin), c);
            AddTriangle(vh, new Vector2(r.center.x, r.yMax * .48f), new Vector2(r.center.x, r.yMin), new Vector2(r.xMax - r.width * .18f, r.yMin + r.height * .38f), c);
        }

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            var index = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
        }
    }
}
