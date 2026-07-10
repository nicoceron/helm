using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPolygonElement : SVGParentable, ISVGDrawable, ISVGElement
	{
		private List<Vector2> _listPoints;

		private AttributeList _attrList;

		private SVGPaintable _paintable;

		public AttributeList attrList => _attrList;

		public SVGPaintable paintable => _paintable;

		public List<Vector2> listPoints => _listPoints;

		public SVGPolygonElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null)
			: base(inheritTransformList)
		{
			_attrList = node.attributes;
			_paintable = new SVGPaintable(inheritPaintable, node);
			_listPoints = ExtractPoints(_attrList.GetValue("points"));
			base.currentTransformList = new SVGTransformList(attrList.GetValue("transform"));
			Rect viewport = _paintable.viewport;
			base.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(_attrList, ref viewport)));
			paintable.SetViewport(viewport);
		}

		private List<Vector2> ExtractPoints(string inputText)
		{
			List<Vector2> list = new List<Vector2>();
			string[] array = SVGStringExtractor.ExtractTransformValue(inputText);
			int num = array.Length;
			int num2;
			for (num2 = 0; num2 < num - 1; num2++)
			{
				string valueText = array[num2];
				string valueText2 = array[num2 + 1];
				SVGLength sVGLength = new SVGLength(valueText);
				SVGLength sVGLength2 = new SVGLength(valueText2);
				Vector2 item = new Vector2(sVGLength.value, sVGLength2.value);
				list.Add(item);
				num2++;
			}
			return list;
		}

		public void BeforeRender(SVGTransformList transformList)
		{
			base.inheritTransformList = transformList;
		}

		public List<List<Vector2>> GetPath()
		{
			List<Vector2> list = new List<Vector2>(listPoints.Count + 1);
			for (int i = 0; i < listPoints.Count; i++)
			{
				list.Add(base.transformMatrix.Transform(listPoints[i]));
			}
			list.Add(list[0]);
			return new List<List<Vector2>> { SVGBezier.Optimise(list, SVGGraphics.vpm) };
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
			SVGGraphics.Create(this, "Polygon Element");
		}
	}
}
