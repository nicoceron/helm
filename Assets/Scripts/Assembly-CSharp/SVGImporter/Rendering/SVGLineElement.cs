using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGLineElement : SVGParentable, ISVGDrawable, ISVGElement
	{
		private SVGLength _x1;

		private SVGLength _y1;

		private SVGLength _x2;

		private SVGLength _y2;

		private AttributeList _attrList;

		private SVGPaintable _paintable;

		public AttributeList attrList => _attrList;

		public SVGPaintable paintable => _paintable;

		public SVGLength x1 => _x1;

		public SVGLength y1 => _y1;

		public SVGLength x2 => _x2;

		public SVGLength y2 => _y2;

		public SVGLineElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null)
			: base(inheritTransformList)
		{
			_attrList = node.attributes;
			_paintable = new SVGPaintable(inheritPaintable, node);
			_x1 = new SVGLength(attrList.GetValue("x1"));
			_y1 = new SVGLength(attrList.GetValue("y1"));
			_x2 = new SVGLength(attrList.GetValue("x2"));
			_y2 = new SVGLength(attrList.GetValue("y2"));
			base.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));
			Rect viewport = _paintable.viewport;
			base.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(_attrList, ref viewport)));
			paintable.SetViewport(viewport);
		}

		public void BeforeRender(SVGTransformList transformList)
		{
			base.inheritTransformList = transformList;
		}

		public List<List<Vector2>> GetPath()
		{
			List<Vector2> item = new List<Vector2>
			{
				base.transformMatrix.Transform(new Vector2(x1.value, y1.value)),
				base.transformMatrix.Transform(new Vector2(x2.value, y2.value))
			};
			return new List<List<Vector2>> { item };
		}

		public List<List<Vector2>> GetClipPath()
		{
			List<List<Vector2>> path = GetPath();
			if (path == null || path.Count == 0 || path[0] == null || path[0].Count == 0)
			{
				return null;
			}
			List<List<Vector2>> list = new List<List<Vector2>>();
			List<List<Vector2>> list2 = SVGLineUtils.StrokeShape(new List<StrokeSegment[]> { SVGSimplePath.GetSegments(path[0]) }, paintable.strokeWidth, Color.black, SVGSimplePath.GetStrokeLineJoin(paintable.strokeLineJoin), SVGSimplePath.GetStrokeLineCap(paintable.strokeLineCap), paintable.miterLimit, paintable.dashArray, paintable.dashOffset, ClosePathRule.NEVER, SVGGraphics.roundQuality);
			if (list2 != null && list2.Count > 0)
			{
				list.AddRange(list2);
			}
			return list;
		}

		public void Render()
		{
			SVGGraphics.Create(this, "Line Element", ClosePathRule.NEVER);
		}
	}
}
