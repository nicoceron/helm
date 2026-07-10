using System;
using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGGraphics
	{
		public static List<SVGPath> paths;

		public static List<SVGLayer> layers;

		protected static float _vpm;

		public static float _roundQuality = 0f;

		private static float _vertexPerMeter = 1000f;

		private static bool _antialiasing = false;

		private SVGStrokeLineCapMethod _strokeLineCap;

		private SVGStrokeLineJoinMethod _strokeLineJoin;

		public static float vpm => _vpm;

		public static float roundQuality => _roundQuality;

		public static float vertexPerMeter => _vertexPerMeter;

		public static bool antialiasing => _antialiasing;

		public SVGStrokeLineCapMethod strokeLineCap => _strokeLineCap;

		public SVGStrokeLineJoinMethod strokeLineJoin => _strokeLineJoin;

		public static void AddLayer(SVGLayer layer)
		{
			layers.Add(layer);
		}

		public static void Create(ISVGElement svgElement, string defaultName = null, ClosePathRule closePathRule = ClosePathRule.ALWAYS)
		{
			if (svgElement == null || svgElement.paintable.visibility != SVGVisibility.Visible || svgElement.paintable.display == SVGDisplay.None)
			{
				return;
			}
			List<SVGShape> list = new List<SVGShape>();
			List<List<Vector2>> path = svgElement.GetPath();
			if (path.Count == 1)
			{
				if (svgElement.paintable.IsFill())
				{
					List<List<Vector2>> inputShapes = path;
					if (svgElement.paintable.clipPathList != null && svgElement.paintable.clipPathList.Count > 0)
					{
						inputShapes = SVGGeom.ClipPolygon(new List<List<Vector2>> { path[0] }, svgElement.paintable.clipPathList);
					}
					SVGShape[] shapes = GetShapes(inputShapes, svgElement.paintable, svgElement.transformMatrix);
					if (shapes != null && shapes.Length != 0)
					{
						list.AddRange(shapes);
					}
				}
				if (svgElement.paintable.IsStroke())
				{
					List<List<Vector2>> list2 = SVGSimplePath.CreateStroke(path[0], svgElement.paintable, closePathRule);
					if (svgElement.paintable.clipPathList != null && svgElement.paintable.clipPathList.Count > 0)
					{
						list2 = SVGGeom.ClipPolygon(list2, svgElement.paintable.clipPathList);
					}
					SVGShape[] shapes2 = GetShapes(list2, svgElement.paintable, svgElement.transformMatrix, isStroke: true);
					if (shapes2 != null && shapes2.Length != 0)
					{
						list.AddRange(shapes2);
					}
				}
			}
			else
			{
				if (svgElement.paintable.IsFill())
				{
					List<List<Vector2>> inputShapes2 = path;
					if (svgElement.paintable.clipPathList != null && svgElement.paintable.clipPathList.Count > 0)
					{
						inputShapes2 = SVGGeom.ClipPolygon(path, svgElement.paintable.clipPathList);
					}
					SVGShape[] shapes3 = GetShapes(inputShapes2, svgElement.paintable, svgElement.transformMatrix);
					if (shapes3 != null && shapes3.Length != 0)
					{
						list.AddRange(shapes3);
					}
				}
				if (svgElement.paintable.IsStroke())
				{
					List<List<Vector2>> list3 = SVGSimplePath.CreateStroke(path, svgElement.paintable, closePathRule);
					if (svgElement.paintable.clipPathList != null && svgElement.paintable.clipPathList.Count > 0)
					{
						list3 = SVGGeom.ClipPolygon(list3, svgElement.paintable.clipPathList);
					}
					SVGShape[] shapes4 = GetShapes(list3, svgElement.paintable, svgElement.transformMatrix, isStroke: true);
					if (shapes4 != null && shapes4.Length != 0)
					{
						list.AddRange(shapes4);
					}
				}
			}
			if (list.Count > 0)
			{
				string text = svgElement.attrList.GetValue("id");
				if (string.IsNullOrEmpty(text))
				{
					text = defaultName;
				}
				AddLayer(new SVGLayer
				{
					shapes = list.ToArray(),
					name = text
				});
			}
		}

		public static void CorrectSVGLayers(List<SVGLayer> layers, Rect viewport, SVGAsset asset, out Vector2 offset)
		{
			offset = Vector2.zero;
			if (layers == null)
			{
				return;
			}
			int count = layers.Count;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				CorrectSVGLayerShape(layers[i].shapes);
			}
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			float num5 = float.MaxValue;
			float num6 = float.MinValue;
			for (int j = 0; j < count; j++)
			{
				if (layers[j].shapes == null)
				{
					continue;
				}
				num = layers[j].shapes.Length;
				for (int k = 0; k < num; k++)
				{
					Vector2 min = layers[j].shapes[k].bounds.min;
					Vector2 max = layers[j].shapes[k].bounds.max;
					if (min.x < num3)
					{
						num3 = min.x;
					}
					if (max.x > num4)
					{
						num4 = max.x;
					}
					if (min.y < num5)
					{
						num5 = min.y;
					}
					if (max.y > num6)
					{
						num6 = max.y;
					}
				}
			}
			Rect rect = new Rect(num3, num5, num4 - num3, num6 - num5);
			if (asset.ignoreSVGCanvas)
			{
				offset = new Vector2(rect.min.x + rect.size.x * asset.pivotPoint.x, rect.max.y - rect.size.y * asset.pivotPoint.y);
			}
			else
			{
				offset = new Vector2(viewport.min.x + viewport.size.x * asset.pivotPoint.x, viewport.max.y - viewport.size.y * asset.pivotPoint.y);
			}
			for (int l = 0; l < count; l++)
			{
				if (layers[l].shapes == null)
				{
					continue;
				}
				num = layers[l].shapes.Length;
				for (int m = 0; m < num; m++)
				{
					if (layers[l].shapes[m].vertices != null)
					{
						num2 = layers[l].shapes[m].vertices.Length;
						for (int n = 0; n < num2; n++)
						{
							layers[l].shapes[m].vertices[n] -= offset;
						}
						layers[l].shapes[m].bounds.center -= offset;
						if (layers[l].shapes[m].fill != null)
						{
							layers[l].shapes[m].fill.transform = layers[l].shapes[m].fill.transform.Translate(offset);
						}
					}
				}
			}
		}

		protected static void CorrectSVGLayerShape(SVGShape[] shapes)
		{
			for (int i = 0; i < shapes.Length; i++)
			{
				int vertexCount = shapes[i].vertexCount;
				if (vertexCount != 0)
				{
					if (shapes[i].fill != null)
					{
						shapes[i].fill.transform = shapes[i].fill.transform.Scale(1f, -1f);
					}
					for (int j = 0; j < vertexCount; j++)
					{
						shapes[i].vertices[j].y *= -1f;
					}
					shapes[i].bounds.center = new Vector2(shapes[i].bounds.center.x, shapes[i].bounds.center.y * -1f);
				}
			}
		}

		public static SVGShape[] GetShapes(List<List<Vector2>> inputShapes, SVGPaintable paintable, SVGMatrix matrix, bool isStroke = false)
		{
			SVGShape[] result = null;
			if (SVGSimplePath.CreatePolygon(inputShapes, paintable, matrix, out var layer, out var antialiasingLayer, isStroke, _antialiasing))
			{
				result = ((!_antialiasing) ? new SVGShape[1] { layer } : new SVGShape[2] { layer, antialiasingLayer });
			}
			return result;
		}

		public static void Clear()
		{
			if (layers != null)
			{
				layers.Clear();
				layers = null;
			}
			if (paths != null)
			{
				paths.Clear();
				paths = null;
			}
		}

		public static void Init()
		{
			if (layers == null)
			{
				layers = new List<SVGLayer>();
			}
			if (paths == null)
			{
				paths = new List<SVGPath>();
			}
		}

		public SVGGraphics(float vertexPerMeter = 1000f, bool antialiasing = false)
		{
			_vpm = 1f;
			if (vertexPerMeter > 0f)
			{
				_vpm = 1000f / vertexPerMeter;
			}
			else
			{
				_vpm = 1000f;
			}
			if (_vpm != 0f)
			{
				_roundQuality = 1f / _vpm * 0.5f;
			}
			else
			{
				_roundQuality = 0f;
			}
			_vertexPerMeter = vertexPerMeter;
			_antialiasing = antialiasing;
		}

		public void SetStrokeLineCap(SVGStrokeLineCapMethod strokeLineCap)
		{
			_strokeLineCap = strokeLineCap;
		}

		public void SetStrokeLineJoin(SVGStrokeLineJoinMethod strokeLineJoin)
		{
			_strokeLineJoin = strokeLineJoin;
		}

		public bool GetThickLine(Vector2 p1, Vector2 p2, float width, ref Vector2 rp1, ref Vector2 rp2, ref Vector2 rp3, ref Vector2 rp4)
		{
			int num = (int)(width / 2f);
			int num2 = (int)(width - (float)num + 0.5f);
			float num3 = p2.x - p1.x;
			float num4 = p2.y - p1.y;
			float num5 = num3 * num3 + num4 * num4;
			if (num5 == 0f)
			{
				rp1.x = p1.x - (float)num2;
				rp1.y = p1.y + (float)num2;
				rp2.x = p1.x - (float)num2;
				rp2.y = p1.y - (float)num2;
				rp3.x = p1.x + (float)num;
				rp3.y = p1.y + (float)num;
				rp4.x = p1.x + (float)num;
				rp4.y = p1.y - (float)num;
				return false;
			}
			float num6 = (float)num * num3 / (float)Math.Sqrt(num5) + p1.y;
			float num7 = ((num3 != 0f) ? ((0f - (num6 - p1.y)) * num4 / num3 + p1.x) : ((!(num4 > 0f)) ? (p1.x + (float)num) : (p1.x - (float)num)));
			float num8 = 0f - (float)num2 * num3 / (float)Math.Sqrt(num5) + p1.y;
			float x = ((num3 != 0f) ? ((0f - (num8 - p1.y)) * num4 / num3 + p1.x) : ((!(num4 > 0f)) ? (p1.x - (float)num2) : (p1.x + (float)num2)));
			num3 = p1.x - p2.x;
			num4 = p1.y - p2.y;
			num5 = num3 * num3 + num4 * num4;
			float num9 = (float)num * num3 / (float)Math.Sqrt(num5) + p2.y;
			float x2 = ((num3 != 0f) ? ((0f - (num9 - p2.y)) * num4 / num3 + p2.x) : ((!(num4 > 0f)) ? (p2.x + (float)num) : (p2.x - (float)num)));
			float num10 = 0f - (float)num2 * num3 / (float)Math.Sqrt(num5) + p2.y;
			float num11 = ((num3 != 0f) ? ((0f - (num10 - p2.y)) * num4 / num3 + p2.x) : ((!(num4 > 0f)) ? (p2.x - (float)num2) : (p2.x + (float)num2)));
			rp1.x = num7;
			rp1.y = num6;
			rp2.x = x;
			rp2.y = num8;
			float num12 = (p1.y - num6) * (p2.x - p1.x) - (p1.x - num7) * (p2.y - p1.y);
			float num13 = (p1.y - num10) * (p2.x - p1.x) - (p1.x - num11) * (p2.y - p1.y);
			if (num12 * num13 > 0f)
			{
				if (num != num2)
				{
					num9 = (float)num2 * num3 / (float)Math.Sqrt(num5) + p2.y;
					x2 = ((num3 != 0f) ? ((0f - (num9 - p2.y)) * num4 / num3 + p2.x) : ((!(num4 > 0f)) ? (p2.x + (float)num2) : (p2.x - (float)num2)));
					num10 = 0f - (float)num * num3 / (float)Math.Sqrt(num5) + p2.y;
					num11 = ((num3 != 0f) ? ((0f - (num10 - p2.y)) * num4 / num3 + p2.x) : ((!(num4 > 0f)) ? (p2.x - (float)num) : (p2.x + (float)num)));
				}
				rp3.x = num11;
				rp3.y = num10;
				rp4.x = x2;
				rp4.y = num9;
			}
			else
			{
				rp3.x = x2;
				rp3.y = num9;
				rp4.x = num11;
				rp4.y = num10;
			}
			return true;
		}

		public Vector2 GetCrossPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			Vector2 result = new Vector2(0f, 0f);
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = p1.x - p2.x;
			float num6 = p1.y - p2.y;
			float num7 = p3.x - p4.x;
			float num8 = p3.y - p4.y;
			if (num5 != 0f)
			{
				num = num6 / num5;
				num2 = p1.y - num * p1.x;
			}
			if (num7 != 0f)
			{
				num3 = num8 / num7;
				num4 = p3.y - num3 * p3.x;
			}
			float num9 = 0f;
			float y = 0f;
			if (num == num3 && num2 == num4)
			{
				Vector2 vector = p1;
				Vector2 vector2 = p1;
				if (num5 == 0f)
				{
					if (p2.y < vector.y)
					{
						vector = p2;
					}
					if (p3.y < vector.y)
					{
						vector = p3;
					}
					if (p4.y < vector.y)
					{
						vector = p4;
					}
					if (p2.y > vector2.y)
					{
						vector2 = p2;
					}
					if (p3.y > vector2.y)
					{
						vector2 = p3;
					}
					if (p4.y > vector2.y)
					{
						vector2 = p4;
					}
				}
				else
				{
					if (p2.x < vector.x)
					{
						vector = p2;
					}
					if (p3.x < vector.x)
					{
						vector = p3;
					}
					if (p4.x < vector.x)
					{
						vector = p4;
					}
					if (p2.x > vector2.x)
					{
						vector2 = p2;
					}
					if (p3.x > vector2.x)
					{
						vector2 = p3;
					}
					if (p4.x > vector2.x)
					{
						vector2 = p4;
					}
				}
				num9 = (vector.x - vector2.x) / 2f;
				num9 = vector2.x + num9;
				y = (vector.y - vector2.y) / 2f;
				y = vector2.y + y;
				result.x = num9;
				result.y = y;
				return result;
			}
			if (num5 != 0f && num7 != 0f)
			{
				num9 = (0f - (num2 - num4)) / (num - num3);
				y = num * num9 + num2;
			}
			else if (num5 == 0f && num7 != 0f)
			{
				num9 = p1.x;
				y = num3 * num9 + num4;
			}
			else if (num5 != 0f && num7 == 0f)
			{
				num9 = p3.x;
				y = num * num9 + num2;
			}
			result.x = num9;
			result.y = y;
			return result;
		}

		public float AngleBetween2Vector(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			Vector2 vector = new Vector2(p2.x - p1.x, p2.y - p1.y);
			Vector2 vector2 = new Vector2(p4.x - p3.x, p4.y - p3.y);
			float num = vector.x * vector2.x + vector.y * vector2.y;
			float num2 = (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y);
			float num3 = (float)Math.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
			float num4 = num2 * num3;
			return (float)Math.Acos(num / num4);
		}
	}
}
