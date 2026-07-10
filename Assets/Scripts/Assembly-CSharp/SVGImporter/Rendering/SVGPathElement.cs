using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathElement : SVGParentable, ISVGDrawable, ISVGElement
	{
		private SVGPathSegList _segList;

		private AttributeList _attrList;

		private SVGPaintable _paintable;

		private static SVGPathSegTypes lastCommand;

		public SVGPathSegList segList => _segList;

		public AttributeList attrList => _attrList;

		public SVGPaintable paintable => _paintable;

		public SVGPathElement(Node node, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable = null)
			: base(inheritTransformList)
		{
			_attrList = node.attributes;
			_paintable = new SVGPaintable(inheritPaintable, node);
			base.currentTransformList = new SVGTransformList(_attrList.GetValue("transform"));
			Rect viewport = _paintable.viewport;
			base.currentTransformList.AppendItem(new SVGTransform(SVGTransformable.GetViewBoxTransform(_attrList, ref viewport)));
			paintable.SetViewport(viewport);
			Initial();
		}

		private void Initial()
		{
			string value = _attrList.GetValue("d");
			SVGPathSeg sVGPathSeg = null;
			SVGPathSeg sVGPathSeg2 = null;
			List<char> charList = new List<char>();
			List<string> valueList = new List<string>();
			SVGStringExtractor.ExtractPathSegList(value, ref charList, ref valueList);
			_segList = new SVGPathSegList(charList.Count);
			for (int i = 0; i < charList.Count; i++)
			{
				char c = charList[i];
				float[] array = SVGStringExtractor.ExtractTransformValueAsPX(valueList[i]);
				int num = array.Length;
				switch (c)
				{
				case 'Z':
				case 'z':
					if (_segList.Count > 0 && sVGPathSeg2 != null)
					{
						sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoAbs(sVGPathSeg2.currentPoint.x, sVGPathSeg2.currentPoint.y, sVGPathSeg));
					}
					_segList.AppendItem(CreateSVGPathSegClosePath());
					sVGPathSeg2 = null;
					break;
				case 'M':
				{
					if (sVGPathSeg != null && sVGPathSeg.type != SVGPathSegTypes.Close && sVGPathSeg.type != SVGPathSegTypes.MoveTo_Abs && sVGPathSeg.type != SVGPathSegTypes.MoveTo_Rel)
					{
						sVGPathSeg2 = null;
					}
					if (num < 2)
					{
						break;
					}
					for (int j = 0; j < num; j += 2)
					{
						if (num - j >= 2)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegMovetoAbs(array[j], array[j + 1], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'm':
				{
					if (sVGPathSeg != null && sVGPathSeg.type != SVGPathSegTypes.Close && sVGPathSeg.type != SVGPathSegTypes.MoveTo_Abs && sVGPathSeg.type != SVGPathSegTypes.MoveTo_Rel)
					{
						sVGPathSeg2 = null;
					}
					if (num < 2)
					{
						break;
					}
					for (int j = 0; j < num; j += 2)
					{
						if (num - j >= 2)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegMovetoRel(array[j], array[j + 1], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'L':
				{
					if (num < 2)
					{
						break;
					}
					for (int j = 0; j < num; j += 2)
					{
						if (num - j >= 2)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoAbs(array[j], array[j + 1], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'l':
				{
					if (num < 2)
					{
						break;
					}
					for (int j = 0; j < num; j += 2)
					{
						if (num - j >= 2)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoRel(array[j], array[j + 1], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'C':
				{
					if (num < 6)
					{
						break;
					}
					for (int j = 0; j < num; j += 6)
					{
						if (num - j >= 6)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoCubicAbs(array[j], array[j + 1], array[j + 2], array[j + 3], array[j + 4], array[j + 5], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'c':
				{
					if (num < 6)
					{
						break;
					}
					for (int j = 0; j < num; j += 6)
					{
						if (num - j >= 6)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoCubicRel(array[j], array[j + 1], array[j + 2], array[j + 3], array[j + 4], array[j + 5], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'S':
				{
					if (num < 4)
					{
						break;
					}
					for (int j = 0; j < num; j += 4)
					{
						if (num - j >= 4)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoCubicSmoothAbs(array[j], array[j + 1], array[j + 2], array[j + 3], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 's':
				{
					if (num < 4)
					{
						break;
					}
					for (int j = 0; j < num; j += 4)
					{
						if (num - j >= 4)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoCubicSmoothRel(array[j], array[j + 1], array[j + 2], array[j + 3], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'Q':
				{
					if (num < 4)
					{
						break;
					}
					for (int j = 0; j < num; j += 4)
					{
						if (num - j >= 4)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoQuadraticAbs(array[j], array[j + 1], array[j + 2], array[j + 3], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'q':
				{
					if (num < 4)
					{
						break;
					}
					for (int j = 0; j < num; j += 4)
					{
						if (num - j >= 4)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoQuadraticRel(array[j], array[j + 1], array[j + 2], array[j + 3], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'T':
				{
					if (num < 2)
					{
						break;
					}
					for (int j = 0; j < num; j += 2)
					{
						if (num - j >= 2)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoQuadraticSmoothAbs(array[j], array[j + 1], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 't':
				{
					if (num < 2)
					{
						break;
					}
					for (int j = 0; j < num; j += 2)
					{
						if (num - j >= 2)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegCurvetoQuadraticSmoothRel(array[j], array[j + 1], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'A':
				{
					if (num < 7)
					{
						break;
					}
					for (int j = 0; j < num; j += 7)
					{
						if (num - j >= 7)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegArcAbs(array[j], array[j + 1], array[j + 2], array[j + 3] == 1f, array[j + 4] == 1f, array[j + 5], array[j + 6], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'a':
				{
					if (num < 7)
					{
						break;
					}
					for (int j = 0; j < num; j += 7)
					{
						if (num - j >= 7)
						{
							sVGPathSeg = _segList.AppendItem(new SVGPathSegArcRel(array[j], array[j + 1], array[j + 2], array[j + 3] == 1f, array[j + 4] == 1f, array[j + 5], array[j + 6], sVGPathSeg));
							if (sVGPathSeg2 == null)
							{
								sVGPathSeg2 = sVGPathSeg;
							}
						}
					}
					break;
				}
				case 'H':
				{
					for (int j = 0; j < num; j++)
					{
						sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoHorizontalAbs(array[j], sVGPathSeg));
						if (sVGPathSeg2 == null)
						{
							sVGPathSeg2 = sVGPathSeg;
						}
					}
					break;
				}
				case 'h':
				{
					for (int j = 0; j < num; j++)
					{
						sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoHorizontalRel(array[j], sVGPathSeg));
						if (sVGPathSeg2 == null)
						{
							sVGPathSeg2 = sVGPathSeg;
						}
					}
					break;
				}
				case 'V':
				{
					for (int j = 0; j < num; j++)
					{
						sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoVerticalAbs(array[j], sVGPathSeg));
						if (sVGPathSeg2 == null)
						{
							sVGPathSeg2 = sVGPathSeg;
						}
					}
					break;
				}
				case 'v':
				{
					for (int j = 0; j < num; j++)
					{
						sVGPathSeg = _segList.AppendItem(new SVGPathSegLinetoVerticalRel(array[j], sVGPathSeg));
						if (sVGPathSeg2 == null)
						{
							sVGPathSeg2 = sVGPathSeg;
						}
					}
					break;
				}
				}
			}
		}

		private SVGPathSegClosePath CreateSVGPathSegClosePath()
		{
			SVGPathSeg lastItem = _segList.GetLastItem();
			SVGPathSeg item = _segList.GetItem(0);
			if (item != null)
			{
				return new SVGPathSegClosePath(item.currentPoint, lastItem);
			}
			return null;
		}

		public void BeforeRender(SVGTransformList transformList)
		{
			base.inheritTransformList = transformList;
			for (int i = 0; i < _segList.Count; i++)
			{
				if (_segList.GetItem(i) is ISVGDrawable iSVGDrawable)
				{
					iSVGDrawable.BeforeRender(base.summaryTransformList);
				}
			}
		}

		public List<List<Vector2>> GetPath()
		{
			lastCommand = SVGPathSegTypes.Unknown;
			List<Vector2> list = new List<Vector2>();
			List<List<Vector2>> list2 = new List<List<Vector2>>();
			for (int i = 0; i < segList.Count; i++)
			{
				GetSegment(this, segList.GetItem(i), list2, list, base.transformMatrix);
			}
			if (lastCommand != SVGPathSegTypes.Close && list.Count > 0)
			{
				list2.Add(new List<Vector2>(list.ToArray()));
			}
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j] != null && list2[j].Count >= 3)
				{
					list2[j] = SVGBezier.Optimise(list2[j], SVGGraphics.vpm);
				}
			}
			return list2;
		}

		public List<List<Vector2>> GetClipPath()
		{
			List<List<Vector2>> path = GetPath();
			if (path == null || path.Count == 0)
			{
				return null;
			}
			List<List<Vector2>> list = new List<List<Vector2>>();
			if (paintable.IsFill())
			{
				list.AddRange(path);
			}
			if (paintable.IsStroke())
			{
				List<StrokeSegment[]> list2 = new List<StrokeSegment[]>();
				for (int i = 0; i < path.Count; i++)
				{
					if (path[i] != null && path[i].Count >= 2)
					{
						list2.Add(SVGSimplePath.GetSegments(path[i]));
					}
				}
				List<List<Vector2>> list3 = SVGLineUtils.StrokeShape(list2, paintable.strokeWidth, Color.black, SVGSimplePath.GetStrokeLineJoin(paintable.strokeLineJoin), SVGSimplePath.GetStrokeLineCap(paintable.strokeLineCap), paintable.miterLimit, paintable.dashArray, paintable.dashOffset, ClosePathRule.AUTO, SVGGraphics.roundQuality);
				if (list3 != null && list3.Count > 0)
				{
					list.AddRange(list3);
				}
			}
			return list;
		}

		public void Render()
		{
			SVGGraphics.Create(this, "Path Element", ClosePathRule.AUTO);
		}

		private bool GetSegment(SVGPathElement svgElement, SVGPathSeg segment, List<List<Vector2>> output, List<Vector2> positionBuffer, SVGMatrix matrix)
		{
			if (segment == null)
			{
				return false;
			}
			switch (segment.type)
			{
			case SVGPathSegTypes.Arc_Abs:
			{
				SVGPathSegArcAbs sVGPathSegArcAbs = segment as SVGPathSegArcAbs;
				positionBuffer.AddRange(SVGGeomUtils.Arc(SVGGeomUtils.TransformPoint(sVGPathSegArcAbs.previousPoint, matrix), sVGPathSegArcAbs.r1, sVGPathSegArcAbs.r2, sVGPathSegArcAbs.angle, sVGPathSegArcAbs.largeArcFlag, sVGPathSegArcAbs.sweepFlag, SVGGeomUtils.TransformPoint(sVGPathSegArcAbs.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.Arc_Rel:
			{
				SVGPathSegArcRel sVGPathSegArcRel = segment as SVGPathSegArcRel;
				positionBuffer.AddRange(SVGGeomUtils.Arc(SVGGeomUtils.TransformPoint(sVGPathSegArcRel.previousPoint, matrix), sVGPathSegArcRel.r1, sVGPathSegArcRel.r2, sVGPathSegArcRel.angle, sVGPathSegArcRel.largeArcFlag, sVGPathSegArcRel.sweepFlag, SVGGeomUtils.TransformPoint(sVGPathSegArcRel.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.Close:
				if (positionBuffer.Count > 0)
				{
					output.Add(new List<Vector2>(positionBuffer.ToArray()));
				}
				positionBuffer.Clear();
				break;
			case SVGPathSegTypes.CurveTo_Cubic_Abs:
			{
				SVGPathSegCurvetoCubicAbs sVGPathSegCurvetoCubicAbs = segment as SVGPathSegCurvetoCubicAbs;
				positionBuffer.AddRange(SVGGeomUtils.CubicCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicAbs.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicAbs.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicAbs.controlPoint2, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicAbs.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Cubic_Rel:
			{
				SVGPathSegCurvetoCubicRel sVGPathSegCurvetoCubicRel = segment as SVGPathSegCurvetoCubicRel;
				positionBuffer.AddRange(SVGGeomUtils.CubicCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicRel.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicRel.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicRel.controlPoint2, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicRel.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Cubic_Smooth_Abs:
			{
				SVGPathSegCurvetoCubicSmoothAbs sVGPathSegCurvetoCubicSmoothAbs = segment as SVGPathSegCurvetoCubicSmoothAbs;
				positionBuffer.AddRange(SVGGeomUtils.CubicCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothAbs.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothAbs.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothAbs.controlPoint2, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothAbs.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Cubic_Smooth_Rel:
			{
				SVGPathSegCurvetoCubicSmoothRel sVGPathSegCurvetoCubicSmoothRel = segment as SVGPathSegCurvetoCubicSmoothRel;
				positionBuffer.AddRange(SVGGeomUtils.CubicCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothRel.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothRel.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothRel.controlPoint2, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoCubicSmoothRel.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Quadratic_Abs:
			{
				SVGPathSegCurvetoQuadraticAbs sVGPathSegCurvetoQuadraticAbs = segment as SVGPathSegCurvetoQuadraticAbs;
				positionBuffer.AddRange(SVGGeomUtils.QuadraticCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticAbs.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticAbs.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticAbs.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Quadratic_Rel:
			{
				SVGPathSegCurvetoQuadraticRel sVGPathSegCurvetoQuadraticRel = segment as SVGPathSegCurvetoQuadraticRel;
				positionBuffer.AddRange(SVGGeomUtils.QuadraticCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticRel.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticRel.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticRel.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Quadratic_Smooth_Abs:
			{
				SVGPathSegCurvetoQuadraticSmoothAbs sVGPathSegCurvetoQuadraticSmoothAbs = segment as SVGPathSegCurvetoQuadraticSmoothAbs;
				positionBuffer.AddRange(SVGGeomUtils.QuadraticCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticSmoothAbs.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticSmoothAbs.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticSmoothAbs.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.CurveTo_Quadratic_Smooth_Rel:
			{
				SVGPathSegCurvetoQuadraticSmoothRel sVGPathSegCurvetoQuadraticSmoothRel = segment as SVGPathSegCurvetoQuadraticSmoothRel;
				positionBuffer.AddRange(SVGGeomUtils.QuadraticCurve(SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticSmoothRel.previousPoint, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticSmoothRel.controlPoint1, matrix), SVGGeomUtils.TransformPoint(sVGPathSegCurvetoQuadraticSmoothRel.currentPoint, matrix)));
				break;
			}
			case SVGPathSegTypes.LineTo_Abs:
			{
				SVGPathSegLinetoAbs sVGPathSegLinetoAbs = segment as SVGPathSegLinetoAbs;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegLinetoAbs.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.LineTo_Horizontal_Abs:
			{
				SVGPathSegLinetoHorizontalAbs sVGPathSegLinetoHorizontalAbs = segment as SVGPathSegLinetoHorizontalAbs;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegLinetoHorizontalAbs.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.LineTo_Horizontal_Rel:
			{
				SVGPathSegLinetoHorizontalRel sVGPathSegLinetoHorizontalRel = segment as SVGPathSegLinetoHorizontalRel;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegLinetoHorizontalRel.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.LineTo_Rel:
			{
				SVGPathSegLinetoRel sVGPathSegLinetoRel = segment as SVGPathSegLinetoRel;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegLinetoRel.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.LineTo_Vertical_Abs:
			{
				SVGPathSegLinetoVerticalAbs sVGPathSegLinetoVerticalAbs = segment as SVGPathSegLinetoVerticalAbs;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegLinetoVerticalAbs.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.LineTo_Vertical_Rel:
			{
				SVGPathSegLinetoVerticalRel sVGPathSegLinetoVerticalRel = segment as SVGPathSegLinetoVerticalRel;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegLinetoVerticalRel.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.MoveTo_Abs:
			{
				if (lastCommand != SVGPathSegTypes.Close && lastCommand != SVGPathSegTypes.MoveTo_Abs && lastCommand != SVGPathSegTypes.MoveTo_Rel && positionBuffer.Count > 0)
				{
					output.Add(new List<Vector2>(positionBuffer.ToArray()));
					positionBuffer.Clear();
				}
				SVGPathSegMovetoAbs sVGPathSegMovetoAbs = segment as SVGPathSegMovetoAbs;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegMovetoAbs.currentPoint, matrix));
				break;
			}
			case SVGPathSegTypes.MoveTo_Rel:
			{
				if (lastCommand != SVGPathSegTypes.Close && lastCommand != SVGPathSegTypes.MoveTo_Abs && lastCommand != SVGPathSegTypes.MoveTo_Rel && positionBuffer.Count > 0)
				{
					output.Add(new List<Vector2>(positionBuffer.ToArray()));
					positionBuffer.Clear();
				}
				SVGPathSegMovetoRel sVGPathSegMovetoRel = segment as SVGPathSegMovetoRel;
				positionBuffer.Add(SVGGeomUtils.TransformPoint(sVGPathSegMovetoRel.currentPoint, matrix));
				break;
			}
			}
			lastCommand = segment.type;
			return true;
		}
	}
}
