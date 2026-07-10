using System;
using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGEllipseElement : SVGParentable, ISVGDrawable, ISVGElement
	{
		private SVGLength _cx;

		private SVGLength _cy;

		private SVGLength _rx;

		private SVGLength _ry;

		private AttributeList _attrList;

		private SVGPaintable _paintable;

		private const float PI2 = (float)Math.PI * 2f;

		private const float circleConstant = 0.55191505f;

		public AttributeList attrList => _attrList;

		public SVGPaintable paintable => _paintable;

		public SVGLength cx => _cx;

		public SVGLength cy => _cy;

		public SVGLength rx => _rx;

		public SVGLength ry => _ry;

		public SVGEllipseElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null)
			: base(inheritTransformList)
		{
			_attrList = node.attributes;
			_paintable = new SVGPaintable(inheritPaintable, node);
			_cx = new SVGLength(attrList.GetValue("cx"));
			_cy = new SVGLength(attrList.GetValue("cy"));
			_rx = new SVGLength(attrList.GetValue("rx"));
			_ry = new SVGLength(attrList.GetValue("ry"));
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
			List<Vector2> list = Ellipse(cx.value, cy.value, rx.value, ry.value, base.transformMatrix);
			list.Add(list[0]);
			return new List<List<Vector2>> { list };
		}

		public List<List<Vector2>> GetClipPath()
		{
			List<List<Vector2>> path = GetPath();
			if (path == null || path.Count == 0 || path[0] == null || path[0].Count == 0)
			{
				return null;
			}
			List<List<Vector2>> list = new List<List<Vector2>>();
			if (paintable.IsFill())
			{
				list.Add(path[0]);
			}
			if (paintable.IsStroke())
			{
				List<List<Vector2>> list2 = SVGLineUtils.StrokeShape(new List<StrokeSegment[]> { SVGSimplePath.GetSegments(path[0]) }, paintable.strokeWidth, Color.black, SVGSimplePath.GetStrokeLineJoin(paintable.strokeLineJoin), SVGSimplePath.GetStrokeLineCap(paintable.strokeLineCap), paintable.miterLimit, paintable.dashArray, paintable.dashOffset, ClosePathRule.ALWAYS, SVGGraphics.roundQuality);
				if (list2 != null && list2.Count > 0)
				{
					list.AddRange(list2);
				}
			}
			return list;
		}

		public void Render()
		{
			SVGGraphics.Create(this, "Ellipse Element");
		}

		public static List<Vector2> Ellipse(float cx, float cy, float rx, float ry, SVGMatrix matrix)
		{
			List<Vector2> list = new List<Vector2>();
			cx -= rx;
			cy -= ry;
			float num = 0.55191505f * rx;
			float num2 = 0.55191505f * ry;
			Vector2 vector = new Vector2(num, 0f);
			Vector2 vector2 = new Vector2(0f - num, 0f);
			Vector2 vector3 = new Vector2(0f, 0f - num2);
			Vector2 vector4 = new Vector2(0f, num2);
			Vector2 vector5 = new Vector2(cx + rx, cy);
			Vector2 vector6 = new Vector2(cx, cy + ry);
			Vector2 vector7 = new Vector2(cx + rx * 2f, cy + ry);
			Vector2 vector8 = new Vector2(cx + rx, cy + ry * 2f);
			list.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(vector5), matrix.Transform(vector5 + vector), matrix.Transform(vector7 + vector3), matrix.Transform(vector7)));
			list.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(vector7), matrix.Transform(vector7 + vector4), matrix.Transform(vector8 + vector), matrix.Transform(vector8)));
			list.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(vector8), matrix.Transform(vector8 + vector2), matrix.Transform(vector6 + vector4), matrix.Transform(vector6)));
			list.AddRange(SVGGeomUtils.CubicCurve(matrix.Transform(vector6), matrix.Transform(vector6 + vector3), matrix.Transform(vector5 + vector2), matrix.Transform(vector5)));
			return list;
		}
	}
}
