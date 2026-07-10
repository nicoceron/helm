using System;
using System.Collections.Generic;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	[Serializable]
	public struct SVGShape
	{
		public SVGShapeType type;

		public Vector2[] vertices;

		public int[] triangles;

		public Color32[] colors;

		public Vector2[] angles;

		public float depth;

		public Rect bounds;

		public SVGFill fill;

		public int vertexCount
		{
			get
			{
				if (vertices == null)
				{
					return 0;
				}
				return vertices.Length;
			}
		}

		public SVGShape(Vector2[] vertices, int[] triangles, Color32[] colors, Vector2[] angles, int depth, Rect bounds, SVGFill fill)
		{
			type = SVGShapeType.NONE;
			this.vertices = vertices;
			this.triangles = triangles;
			this.colors = colors;
			this.angles = angles;
			this.depth = depth;
			this.bounds = bounds;
			this.fill = fill;
		}

		public void RecalculateBounds()
		{
			int num = vertexCount;
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			float num4 = float.MaxValue;
			float num5 = float.MinValue;
			for (int i = 0; i < num; i++)
			{
				if (vertices[i].x < num2)
				{
					num2 = vertices[i].x;
				}
				else if (vertices[i].x > num3)
				{
					num3 = vertices[i].x;
				}
				if (vertices[i].y < num4)
				{
					num4 = vertices[i].y;
				}
				else if (vertices[i].y > num5)
				{
					num5 = vertices[i].y;
				}
			}
			if (num2 != float.MaxValue || num3 != float.MinValue || num4 != float.MaxValue || num5 != float.MinValue)
			{
				bounds = new Rect(num2, num4, num3 - num2, num5 - num4);
			}
		}

		public SVGShape Clone()
		{
			return new SVGShape
			{
				angles = angles,
				bounds = bounds
			};
		}

		public static SVGShape MergeShapes(IList<SVGShape> svgShapes)
		{
			SVGShape result = default(SVGShape);
			int count = svgShapes.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < count; i++)
			{
				if (svgShapes[i].vertices != null)
				{
					num += svgShapes[i].vertices.Length;
				}
				if (svgShapes[i].triangles != null)
				{
					num2 += svgShapes[i].triangles.Length;
				}
				if (svgShapes[i].colors != null)
				{
					num3 += svgShapes[i].colors.Length;
				}
				if (svgShapes[i].angles != null)
				{
					num4 += svgShapes[i].angles.Length;
				}
			}
			if (num > 0)
			{
				result.vertices = new Vector2[num];
			}
			if (num2 > 0)
			{
				result.triangles = new int[num2];
			}
			if (num3 > 0)
			{
				result.colors = new Color32[num3];
			}
			if (num4 > 0)
			{
				result.angles = new Vector2[num4];
			}
			num = 0;
			num2 = 0;
			num3 = 0;
			num4 = 0;
			float num5 = float.MaxValue;
			float num6 = float.MinValue;
			float num7 = float.MaxValue;
			float num8 = float.MinValue;
			for (int j = 0; j < count; j++)
			{
				Vector2 min = svgShapes[j].bounds.min;
				Vector2 max = svgShapes[j].bounds.max;
				if (min.x < num5)
				{
					num5 = min.x;
				}
				else if (max.x > num6)
				{
					num6 = max.x;
				}
				if (min.y < num7)
				{
					num7 = min.y;
				}
				else if (max.y > num8)
				{
					num8 = max.y;
				}
				int num9;
				if (svgShapes[j].vertices != null)
				{
					num9 = svgShapes[j].vertices.Length;
					for (int k = 0; k < num9; k++)
					{
						result.vertices[num + k] = svgShapes[j].vertices[k];
					}
				}
				else
				{
					num9 = 0;
				}
				int num10;
				if (svgShapes[j].triangles != null)
				{
					num10 = svgShapes[j].triangles.Length;
					for (int l = 0; l < num10; l++)
					{
						result.triangles[num2 + l] = num + svgShapes[j].triangles[l];
					}
				}
				else
				{
					num10 = 0;
				}
				int num11;
				if (svgShapes[j].colors != null)
				{
					num11 = svgShapes[j].colors.Length;
					for (int m = 0; m < num11; m++)
					{
						result.colors[num3 + m] = svgShapes[j].colors[m];
					}
				}
				else
				{
					num11 = 0;
				}
				int num12;
				if (svgShapes[j].angles != null)
				{
					num12 = svgShapes[j].angles.Length;
					for (int n = 0; n < num12; n++)
					{
						result.angles[num4 + n] = svgShapes[j].angles[n];
					}
				}
				else
				{
					num12 = 0;
				}
				num += num9;
				num2 += num10;
				num3 += num11;
				num4 += num12;
			}
			if (num5 != float.MaxValue || num6 != float.MinValue || num7 != float.MaxValue || num8 != float.MinValue)
			{
				result.bounds = new Rect(num5, num7, num6 - num5, num8 - num7);
			}
			return result;
		}

		public static SVGShape MergeLayersToShape(IList<SVGLayer> svgLayers)
		{
			SVGShape result = default(SVGShape);
			int count = svgLayers.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < count; i++)
			{
				int num5 = svgLayers[i].shapes.Length;
				for (int j = 0; j < num5; j++)
				{
					if (svgLayers[i].shapes[j].vertices != null)
					{
						num += svgLayers[i].shapes[j].vertices.Length;
					}
					if (svgLayers[i].shapes[j].triangles != null)
					{
						num2 += svgLayers[i].shapes[j].triangles.Length;
					}
					if (svgLayers[i].shapes[j].colors != null)
					{
						num3 += svgLayers[i].shapes[j].colors.Length;
					}
					if (svgLayers[i].shapes[j].angles != null)
					{
						num4 += svgLayers[i].shapes[j].angles.Length;
					}
				}
			}
			if (num > 0)
			{
				result.vertices = new Vector2[num];
			}
			if (num2 > 0)
			{
				result.triangles = new int[num2];
			}
			if (num3 > 0)
			{
				result.colors = new Color32[num3];
			}
			if (num4 > 0)
			{
				result.angles = new Vector2[num4];
			}
			num = 0;
			num2 = 0;
			num3 = 0;
			num4 = 0;
			float num6 = float.MaxValue;
			float num7 = float.MinValue;
			float num8 = float.MaxValue;
			float num9 = float.MinValue;
			for (int k = 0; k < count; k++)
			{
				int num10 = svgLayers[k].shapes.Length;
				for (int l = 0; l < num10; l++)
				{
					Vector2 min = svgLayers[k].shapes[l].bounds.min;
					Vector2 max = svgLayers[k].shapes[l].bounds.max;
					if (min.x < num6)
					{
						num6 = min.x;
					}
					else if (max.x > num7)
					{
						num7 = max.x;
					}
					if (min.y < num8)
					{
						num8 = min.y;
					}
					else if (max.y > num9)
					{
						num9 = max.y;
					}
					int num11;
					if (svgLayers[k].shapes[l].vertices != null)
					{
						num11 = svgLayers[k].shapes[l].vertices.Length;
						for (int m = 0; m < num11; m++)
						{
							result.vertices[num + m] = svgLayers[k].shapes[l].vertices[m];
						}
					}
					else
					{
						num11 = 0;
					}
					int num12;
					if (svgLayers[k].shapes[l].triangles != null)
					{
						num12 = svgLayers[k].shapes[l].triangles.Length;
						for (int n = 0; n < num12; n++)
						{
							result.triangles[num2 + n] = num + svgLayers[k].shapes[l].triangles[n];
						}
					}
					else
					{
						num12 = 0;
					}
					int num13;
					if (svgLayers[k].shapes[l].colors != null)
					{
						num13 = svgLayers[k].shapes[l].colors.Length;
						for (int num14 = 0; num14 < num13; num14++)
						{
							result.colors[num3 + num14] = svgLayers[k].shapes[l].colors[num14];
						}
					}
					else
					{
						num13 = 0;
					}
					int num15;
					if (svgLayers[k].shapes[l].angles != null)
					{
						num15 = svgLayers[k].shapes[l].angles.Length;
						for (int num16 = 0; num16 < num15; num16++)
						{
							result.angles[num4 + num16] = svgLayers[k].shapes[l].angles[num16];
						}
					}
					else
					{
						num15 = 0;
					}
					num += num11;
					num2 += num12;
					num3 += num13;
					num4 += num15;
				}
			}
			if (num6 != float.MaxValue || num7 != float.MinValue || num8 != float.MaxValue || num9 != float.MinValue)
			{
				result.bounds = new Rect(num6, num8, num7 - num6, num9 - num8);
			}
			return result;
		}

		public static SVGShape[] MergeLayersToShapes(IList<SVGLayer> svgLayers)
		{
			if (svgLayers == null)
			{
				return null;
			}
			int count = svgLayers.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				int num2 = svgLayers[i].shapes.Length;
				num += num2;
			}
			if (num == 0)
			{
				return null;
			}
			SVGShape[] array = new SVGShape[num];
			int num3 = 0;
			for (int j = 0; j < count; j++)
			{
				int num4 = svgLayers[j].shapes.Length;
				for (int k = 0; k < num4; k++)
				{
					array[num3++] = svgLayers[j].shapes[k];
				}
			}
			return array;
		}
	}
}
