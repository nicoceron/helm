using System.Collections.Generic;
using SVGImporter.ClipperLib;
using SVGImporter.LibTessDotNet;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGSimplePath
	{
		private struct VertexData
		{
			public int shapeIndex;

			public int vertexIndex;

			public int totalIndex;

			public VertexData(int shapeIndex, int vertexIndex, int totalIndex)
			{
				this.shapeIndex = shapeIndex;
				this.vertexIndex = vertexIndex;
				this.totalIndex = totalIndex;
			}
		}

		public static StrokeLineCap GetStrokeLineCap(SVGStrokeLineCapMethod capMethod)
		{
			return capMethod switch
			{
				SVGStrokeLineCapMethod.Butt => StrokeLineCap.butt, 
				SVGStrokeLineCapMethod.Round => StrokeLineCap.round, 
				SVGStrokeLineCapMethod.Square => StrokeLineCap.square, 
				_ => StrokeLineCap.butt, 
			};
		}

		public static StrokeLineJoin GetStrokeLineJoin(SVGStrokeLineJoinMethod capMethod)
		{
			return capMethod switch
			{
				SVGStrokeLineJoinMethod.Miter => StrokeLineJoin.miter, 
				SVGStrokeLineJoinMethod.MiterClip => StrokeLineJoin.miterClip, 
				SVGStrokeLineJoinMethod.Round => StrokeLineJoin.round, 
				SVGStrokeLineJoinMethod.Bevel => StrokeLineJoin.bevel, 
				_ => StrokeLineJoin.bevel, 
			};
		}

		public static StrokeSegment[] GetSegments(List<Vector2> points)
		{
			if (points == null || points.Count < 2)
			{
				return null;
			}
			for (int i = 1; i < points.Count; i++)
			{
				if (points[i - 1] == points[i])
				{
					points.RemoveAt(i - 1);
					i--;
				}
			}
			List<StrokeSegment> list = new List<StrokeSegment>();
			for (int j = 1; j < points.Count; j++)
			{
				list.Add(new StrokeSegment(points[j - 1], points[j]));
			}
			return list.ToArray();
		}

		public static Color GetStrokeColor(SVGPaintable paintable)
		{
			Color color = paintable.strokeColor.Value.color;
			color.a *= paintable.strokeOpacity * paintable.opacity;
			paintable.svgFill = new SVGFill(color, FILL_BLEND.OPAQUE, FILL_TYPE.SOLID);
			if (color.a != 1f)
			{
				paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
			}
			return color;
		}

		public static List<List<Vector2>> CreateStroke(List<Vector2> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
		{
			if (inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
			{
				return null;
			}
			return CreateStroke(new List<List<Vector2>> { inputShapes }, paintable, closePath);
		}

		public static List<List<Vector2>> CreateStroke(List<List<Vector2>> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
		{
			if (inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
			{
				return null;
			}
			List<StrokeSegment[]> list = new List<StrokeSegment[]>();
			for (int i = 0; i < inputShapes.Count; i++)
			{
				if (inputShapes[i] != null && inputShapes[i].Count >= 2)
				{
					list.Add(GetSegments(inputShapes[i]));
				}
			}
			return SVGLineUtils.StrokeShape(list, paintable.strokeWidth, Color.black, GetStrokeLineJoin(paintable.strokeLineJoin), GetStrokeLineCap(paintable.strokeLineCap), paintable.miterLimit, paintable.dashArray, paintable.dashOffset, closePath, SVGGraphics.roundQuality);
		}

		public static UnityEngine.Mesh CreateStrokeSimple(List<List<Vector2>> inputShapes, SVGPaintable paintable, ClosePathRule closePath = ClosePathRule.NEVER)
		{
			if (inputShapes == null || inputShapes.Count == 0 || paintable == null || paintable.strokeWidth <= 0f)
			{
				return null;
			}
			AddInputShape(inputShapes);
			Color strokeColor = GetStrokeColor(paintable);
			float strokeWidth = paintable.strokeWidth;
			if (inputShapes.Count > 1)
			{
				CombineInstance[] array = new CombineInstance[inputShapes.Count];
				for (int i = 0; i < inputShapes.Count; i++)
				{
					array[i] = default(CombineInstance);
					SVGShape svgLayer = default(SVGShape);
					if (SVGMeshUtils.VectorLine(inputShapes[i].ToArray(), out svgLayer, strokeColor, strokeColor, strokeWidth, 0f, closePath))
					{
						UnityEngine.Mesh mesh = new UnityEngine.Mesh();
						int num = svgLayer.vertices.Length;
						Vector3[] array2 = new Vector3[num];
						for (int j = 0; j < num; j++)
						{
							array2[j] = svgLayer.vertices[j];
						}
						mesh.vertices = array2;
						mesh.triangles = svgLayer.triangles;
						mesh.colors32 = svgLayer.colors;
						array[i].mesh = mesh;
					}
				}
				UnityEngine.Mesh mesh2 = new UnityEngine.Mesh();
				mesh2.CombineMeshes(array, mergeSubMeshes: true, useMatrices: false);
				return mesh2;
			}
			SVGShape svgLayer2 = default(SVGShape);
			if (SVGMeshUtils.VectorLine(inputShapes[0].ToArray(), out svgLayer2, strokeColor, strokeColor, strokeWidth, 0f, closePath))
			{
				UnityEngine.Mesh mesh3 = new UnityEngine.Mesh();
				int num2 = svgLayer2.vertices.Length;
				Vector3[] array3 = new Vector3[num2];
				for (int k = 0; k < num2; k++)
				{
					array3[k] = svgLayer2.vertices[k];
				}
				mesh3.vertices = array3;
				mesh3.triangles = svgLayer2.triangles;
				mesh3.colors32 = svgLayer2.colors;
				return mesh3;
			}
			return null;
		}

		public static bool CreateAntialiasing(List<List<Vector2>> inputShapes, out SVGShape svgShape, Color colorA, float width, ClosePathRule closePath = ClosePathRule.NEVER)
		{
			svgShape = default(SVGShape);
			if (inputShapes == null || inputShapes.Count == 0)
			{
				return false;
			}
			Color color = new Color(colorA.r, colorA.g, colorA.b, 0f);
			if (inputShapes.Count > 1)
			{
				List<SVGShape> list = new List<SVGShape>();
				for (int i = 0; i < inputShapes.Count; i++)
				{
					if (SVGMeshUtils.VectorLine(inputShapes[i].ToArray(), out var svgLayer, colorA, color, width, width * 0.5f, closePath))
					{
						list.Add(svgLayer);
					}
				}
				if (list.Count > 0)
				{
					svgShape = SVGShape.MergeShapes(list);
					return true;
				}
				return false;
			}
			return SVGMeshUtils.VectorLine(inputShapes[0].ToArray(), out svgShape, colorA, color, width, width * 0.5f, closePath);
		}

		public static bool CreatePolygon(List<List<Vector2>> inputShapes, SVGPaintable paintable, SVGMatrix matrix, out SVGShape layer, out SVGShape antialiasingLayer, bool isStroke = false, bool antialiasing = false)
		{
			layer = default(SVGShape);
			antialiasingLayer = default(SVGShape);
			if (inputShapes == null || inputShapes.Count == 0)
			{
				return false;
			}
			List<List<Vector2>> list = new List<List<Vector2>>();
			PolyFillType polyFillType = PolyFillType.pftNonZero;
			if (paintable.fillRule == SVGFillRule.EvenOdd)
			{
				polyFillType = PolyFillType.pftEvenOdd;
			}
			list = SVGGeom.SimplifyPolygons(inputShapes, polyFillType);
			if (list == null || list.Count == 0)
			{
				return false;
			}
			AddInputShape(list);
			Rect rect = GetRect(list);
			Rect viewport = paintable.viewport;
			if (!isStroke)
			{
				switch (paintable.GetPaintType())
				{
				case SVGPaintMethod.SolidFill:
				{
					layer.type = SVGShapeType.FILL;
					Color color2 = Color.black;
					SVGColorType colorType2 = paintable.fillColor.Value.colorType;
					if (colorType2 == SVGColorType.Unknown || colorType2 == SVGColorType.None)
					{
						color2.a *= paintable.fillOpacity;
						paintable.svgFill = new SVGFill(color2);
					}
					else
					{
						color2 = paintable.fillColor.Value.color;
						color2.a *= paintable.fillOpacity;
						paintable.svgFill = new SVGFill(color2);
					}
					paintable.svgFill.fillType = FILL_TYPE.SOLID;
					if (color2.a == 1f)
					{
						paintable.svgFill.blend = FILL_BLEND.OPAQUE;
					}
					else
					{
						paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
					}
					break;
				}
				case SVGPaintMethod.LinearGradientFill:
				{
					layer.type = SVGShapeType.FILL;
					SVGLinearGradientBrush linearGradientBrush = paintable.GetLinearGradientBrush(rect, matrix, viewport);
					paintable.svgFill = linearGradientBrush.fill;
					break;
				}
				case SVGPaintMethod.RadialGradientFill:
				{
					layer.type = SVGShapeType.FILL;
					SVGRadialGradientBrush radialGradientBrush = paintable.GetRadialGradientBrush(rect, matrix, viewport);
					paintable.svgFill = radialGradientBrush.fill;
					break;
				}
				case SVGPaintMethod.ConicalGradientFill:
				{
					layer.type = SVGShapeType.FILL;
					SVGConicalGradientBrush conicalGradientBrush = paintable.GetConicalGradientBrush(rect, matrix, viewport);
					paintable.svgFill = conicalGradientBrush.fill;
					break;
				}
				case SVGPaintMethod.PathDraw:
				{
					layer.type = SVGShapeType.STROKE;
					Color color = Color.black;
					SVGColorType colorType = paintable.fillColor.Value.colorType;
					if (colorType == SVGColorType.Unknown || colorType == SVGColorType.None)
					{
						color.a *= paintable.strokeOpacity;
						paintable.svgFill = new SVGFill(color);
					}
					else
					{
						color = paintable.fillColor.Value.color;
						color.a *= paintable.strokeOpacity;
						paintable.svgFill = new SVGFill(color);
					}
					paintable.svgFill.fillType = FILL_TYPE.SOLID;
					if (color.a == 1f)
					{
						paintable.svgFill.blend = FILL_BLEND.OPAQUE;
					}
					else
					{
						paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
					}
					break;
				}
				}
			}
			else
			{
				layer.type = SVGShapeType.STROKE;
				Color color3 = paintable.strokeColor.Value.color;
				color3.a *= paintable.strokeOpacity;
				paintable.svgFill = new SVGFill(color3, FILL_BLEND.OPAQUE, FILL_TYPE.SOLID);
				if (color3.a != 1f)
				{
					paintable.svgFill.blend = FILL_BLEND.ALPHA_BLENDED;
				}
				paintable.svgFill.color = color3;
			}
			Tess tess = new Tess();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
				{
					int count = list[i].Count;
					ContourVertex[] array = new ContourVertex[count];
					for (int j = 0; j < count; j++)
					{
						Vector2 vector = list[i][j];
						array[j].Position = new Vec3
						{
							X = vector.x,
							Y = vector.y,
							Z = 0f
						};
					}
					tess.AddContour(array, ContourOrientation.Clockwise);
				}
			}
			tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
			int num = tess.Vertices.Length;
			if (num == 0)
			{
				return false;
			}
			int elementCount = tess.ElementCount;
			layer.triangles = new int[elementCount * 3];
			layer.vertices = new Vector2[num];
			for (int k = 0; k < elementCount; k++)
			{
				layer.triangles[k * 3] = tess.Elements[k * 3];
				layer.triangles[k * 3 + 1] = tess.Elements[k * 3 + 1];
				layer.triangles[k * 3 + 2] = tess.Elements[k * 3 + 2];
			}
			for (int l = 0; l < num; l++)
			{
				layer.vertices[l] = new Vector2(tess.Vertices[l].Position.X, tess.Vertices[l].Position.Y) * SVGAssetImport.meshScale;
			}
			layer.fill = paintable.svgFill;
			layer.fill.opacity = paintable.opacity;
			if (layer.fill.opacity < 1f && layer.fill.blend == FILL_BLEND.OPAQUE)
			{
				layer.fill.blend = FILL_BLEND.ALPHA_BLENDED;
			}
			if (layer.fill.fillType == FILL_TYPE.GRADIENT && layer.fill.gradientColors != null)
			{
				layer.fill.color = Color.white;
			}
			else if (layer.fill.fillType == FILL_TYPE.TEXTURE)
			{
				layer.fill.color = Color.white;
			}
			viewport.x *= SVGAssetImport.meshScale;
			viewport.y *= SVGAssetImport.meshScale;
			viewport.size *= SVGAssetImport.meshScale;
			layer.fill.viewport = viewport;
			SVGMatrix sVGMatrix = SVGMatrix.identity.Scale(SVGAssetImport.meshScale);
			layer.fill.transform = sVGMatrix.Multiply(layer.fill.transform);
			layer.fill.transform = layer.fill.transform.Multiply(sVGMatrix.Inverse());
			Vector2 vector2 = rect.min * SVGAssetImport.meshScale;
			Vector2 vector3 = rect.max * SVGAssetImport.meshScale;
			layer.bounds = new Rect(vector2.x, vector2.y, vector3.x - vector2.x, vector3.y - vector2.y);
			if (antialiasing && CreateAntialiasing(list, out antialiasingLayer, Color.white, -1f, ClosePathRule.ALWAYS))
			{
				int num2 = antialiasingLayer.vertices.Length;
				for (int m = 0; m < num2; m++)
				{
					antialiasingLayer.vertices[m] *= SVGAssetImport.meshScale;
				}
				antialiasingLayer.type = SVGShapeType.ANTIALIASING;
				antialiasingLayer.RecalculateBounds();
				antialiasingLayer.fill = layer.fill.Clone();
				antialiasingLayer.fill.blend = FILL_BLEND.ALPHA_BLENDED;
			}
			return true;
		}

		protected static void WriteUVGradientIndexType(ref Vector2[] uv, int meshVertexCount, SVGPaintable svgPaintable)
		{
			SVGFill svgFill = svgPaintable.svgFill;
			if (svgFill.fillType == FILL_TYPE.GRADIENT && svgFill.gradientColors != null)
			{
				Vector2 vector = new Vector2(svgFill.gradientColors.index, (int)svgFill.gradientType);
				uv = new Vector2[meshVertexCount];
				for (int i = 0; i < meshVertexCount; i++)
				{
					uv[i].x = vector.x;
					uv[i].y = vector.y;
				}
			}
		}

		private static void UpdateMesh(UnityEngine.Mesh mesh, SVGFill svgFill)
		{
			if (svgFill.fillType == FILL_TYPE.GRADIENT && svgFill.gradientColors != null)
			{
				SVGMeshUtils.ChangeMeshUV2(mesh, new Vector2(svgFill.gradientColors.index, (int)svgFill.gradientType));
			}
			else
			{
				SVGMeshUtils.ChangeMeshColor(mesh, svgFill.color);
			}
		}

		private static Bounds GetBounds(List<Vector2> array)
		{
			if (array == null || array.Count == 0)
			{
				return default(Bounds);
			}
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), new Vector3(float.MinValue, float.MinValue, float.MinValue));
			int count = array.Count;
			for (int i = 0; i < count; i++)
			{
				result.Encapsulate(array[i]);
			}
			return result;
		}

		private static Rect GetRect(List<Vector2> array)
		{
			if (array == null || array.Count == 0)
			{
				return default(Rect);
			}
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			int count = array.Count;
			for (int i = 0; i < count; i++)
			{
				if (array[i].x < vector.x)
				{
					vector.x = array[i].x;
				}
				if (array[i].y < vector.y)
				{
					vector.y = array[i].y;
				}
				if (array[i].x > vector2.x)
				{
					vector2.x = array[i].x;
				}
				if (array[i].y > vector2.y)
				{
					vector2.y = array[i].y;
				}
			}
			return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		}

		private static Rect GetRect(List<List<Vector2>> array)
		{
			if (array == null || array.Count == 0)
			{
				return default(Rect);
			}
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			int count = array.Count;
			for (int i = 0; i < count; i++)
			{
				if (array[i] == null)
				{
					continue;
				}
				int count2 = array[i].Count;
				for (int j = 0; j < count2; j++)
				{
					if (array[i][j].x < vector.x)
					{
						vector.x = array[i][j].x;
					}
					if (array[i][j].y < vector.y)
					{
						vector.y = array[i][j].y;
					}
					if (array[i][j].x > vector2.x)
					{
						vector2.x = array[i][j].x;
					}
					if (array[i][j].y > vector2.y)
					{
						vector2.y = array[i][j].y;
					}
				}
			}
			return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		}

		private static void OffsetPositions(Bounds bounds, List<Vector2> array)
		{
			if (array != null && array.Count != 0)
			{
				int count = array.Count;
				Vector2 vector = bounds.center;
				for (int i = 0; i < count; i++)
				{
					array[i] -= vector;
				}
			}
		}

		protected static Vector2 GetGradientVector(SVGLength posX, SVGLength posY, Rect bounds)
		{
			Vector2 zero = Vector2.zero;
			if (posX.unitType != SVGLengthType.Percentage)
			{
				zero.x = posX.value;
			}
			else
			{
				zero.x = bounds.x + bounds.width * (posX.value / 100f);
			}
			if (posY.unitType != SVGLengthType.Percentage)
			{
				zero.y = posY.value;
			}
			else
			{
				zero.y = bounds.y + bounds.height * (posY.value / 100f);
			}
			return zero;
		}

		public static SVGMatrix GetFillTransform(SVGFill svgFill, Rect bounds, SVGLength[] gradientStart, SVGLength[] gradientEnd, SVGMatrix fillTransform, SVGMatrix gradientTransform)
		{
			SVGMatrix result = SVGMatrix.identity;
			SVGLength posX = gradientStart[0];
			SVGLength posY = gradientStart[1];
			SVGLength posX2 = gradientEnd[0];
			SVGLength posY2 = gradientEnd[1];
			Rect viewport = svgFill.viewport;
			if (svgFill.fillType == FILL_TYPE.GRADIENT)
			{
				switch (svgFill.gradientType)
				{
				case GRADIENT_TYPE.LINEAR:
				{
					Vector2 gradientVector3 = GetGradientVector(posX, posY, bounds);
					Vector2 gradientVector4 = GetGradientVector(posX2, posY2, bounds);
					Vector2 vector = gradientVector4 - gradientVector3;
					Vector2 zero3 = Vector2.zero;
					float num5 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
					Vector2 vector2 = Vector2.Lerp(gradientVector3, gradientVector4, 0.5f);
					float magnitude = vector.magnitude;
					if (magnitude != 0f)
					{
						zero3.x = viewport.width / magnitude;
						zero3.y = viewport.height / magnitude;
					}
					result = result.Translate(viewport.center).Scale(zero3.x, zero3.y).Rotate(0f - num5)
						.Translate(-vector2)
						.Multiply(gradientTransform.Inverse())
						.Multiply(fillTransform.Inverse());
					break;
				}
				case GRADIENT_TYPE.RADIAL:
				{
					Vector2 gradientVector2 = GetGradientVector(posX, posY, bounds);
					float num3 = GetGradientVector(posX2, posY2, bounds).x;
					if (posX2.unitType == SVGLengthType.Percentage)
					{
						num3 *= 0.5f;
					}
					float num4 = num3 * 2f;
					Vector2 zero2 = Vector2.zero;
					if (num4 != 0f)
					{
						zero2.x = viewport.width / num4;
						zero2.y = viewport.height / num4;
					}
					result = result.Translate(viewport.center).Scale(zero2.x, zero2.y).Translate(-gradientVector2)
						.Multiply(gradientTransform.Inverse())
						.Multiply(fillTransform.Inverse());
					break;
				}
				case GRADIENT_TYPE.CONICAL:
				{
					Vector2 gradientVector = GetGradientVector(posX, posY, bounds);
					float num = GetGradientVector(posX2, posY2, bounds).x;
					if (posX2.unitType == SVGLengthType.Percentage)
					{
						num *= 0.5f;
					}
					float num2 = num * 2f;
					Vector2 zero = Vector2.zero;
					if (num2 != 0f)
					{
						zero.x = viewport.width / num2;
						zero.y = viewport.height / num2;
					}
					result = result.Translate(viewport.center).Scale(zero.x, zero.y).Translate(-gradientVector)
						.Multiply(gradientTransform.Inverse())
						.Multiply(fillTransform.Inverse());
					break;
				}
				}
			}
			return result;
		}

		protected static void AddInputShape(List<List<Vector2>> inputShapes)
		{
			if (inputShapes == null)
			{
				return;
			}
			for (int i = 0; i < inputShapes.Count; i++)
			{
				if (inputShapes[i] != null && inputShapes[i].Count != 0)
				{
					SVGGraphics.paths.Add(new SVGPath(inputShapes[i].ToArray()));
				}
			}
		}
	}
}
