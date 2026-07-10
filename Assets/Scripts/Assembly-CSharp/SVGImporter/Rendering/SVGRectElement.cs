using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGRectElement : SVGParentable, ISVGDrawable, ISVGElement
	{
		private SVGLength _x;

		private SVGLength _y;

		private SVGLength _width;

		private SVGLength _height;

		private SVGLength _rx;

		private SVGLength _ry;

		private AttributeList _attrList;

		private SVGPaintable _paintable;

		public AttributeList attrList => _attrList;

		public SVGPaintable paintable => _paintable;

		public SVGLength x => _x;

		public SVGLength y => _y;

		public SVGLength width => _width;

		public SVGLength height => _height;

		public SVGLength rx => _rx;

		public SVGLength ry => _ry;

		public SVGRectElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null)
			: base(inheritTransformList)
		{
			_attrList = node.attributes;
			_paintable = new SVGPaintable(inheritPaintable, node);
			_x = new SVGLength(attrList.GetValue("x"));
			_y = new SVGLength(attrList.GetValue("y"));
			_width = new SVGLength(attrList.GetValue("width"));
			_height = new SVGLength(attrList.GetValue("height"));
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
			List<Vector2> list = new List<Vector2>();
			float value = width.value;
			float value2 = height.value;
			float value3 = x.value;
			float value4 = y.value;
			float value5 = rx.value;
			float value6 = ry.value;
			Vector2 point = new Vector2(value3, value4);
			Vector2 point2 = new Vector2(value3 + value, value4);
			Vector2 point3 = new Vector2(value3 + value, value4 + value2);
			Vector2 point4 = new Vector2(value3, value4 + value2);
			if (value5 == 0f && value6 == 0f)
			{
				list = new List<Vector2>(new Vector2[4]
				{
					base.transformMatrix.Transform(point),
					base.transformMatrix.Transform(point2),
					base.transformMatrix.Transform(point3),
					base.transformMatrix.Transform(point4)
				});
			}
			else
			{
				float num = ((value5 == 0f) ? value6 : value5);
				float num2 = ((value6 == 0f) ? value5 : value6);
				num = ((num > value * 0.5f - 2f) ? (value * 0.5f - 2f) : num);
				num2 = ((num2 > value2 * 0.5f - 2f) ? (value2 * 0.5f - 2f) : num2);
				float angle = base.transformAngle;
				Vector2 p = base.transformMatrix.Transform(new Vector2(point.x + num, point.y));
				Vector2 p2 = base.transformMatrix.Transform(new Vector2(point2.x - num, point2.y));
				Vector2 p3 = base.transformMatrix.Transform(new Vector2(point2.x, point2.y + num2));
				Vector2 p4 = base.transformMatrix.Transform(new Vector2(point3.x, point3.y - num2));
				Vector2 p5 = base.transformMatrix.Transform(new Vector2(point3.x - num, point3.y));
				Vector2 p6 = base.transformMatrix.Transform(new Vector2(point4.x + num, point4.y));
				Vector2 p7 = base.transformMatrix.Transform(new Vector2(point4.x, point4.y - num2));
				Vector2 p8 = base.transformMatrix.Transform(new Vector2(point.x, point.y + num2));
				list = SVGGeomUtils.RoundedRect(p, p2, p3, p4, p5, p6, p7, p8, num, num2, angle);
			}
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
			SVGGraphics.Create(this, "Rectangle Element");
		}
	}
}
