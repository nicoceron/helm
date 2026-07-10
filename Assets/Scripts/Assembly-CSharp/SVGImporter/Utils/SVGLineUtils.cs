using System;
using System.Collections.Generic;
using SVGImporter.ClipperLib;
using SVGImporter.LibTessDotNet;
using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGLineUtils
	{
		public static List<Vector2> Stroke(StrokeSegment[] segments, float thickness, Color32 color, StrokeLineJoin lineJoin, StrokeLineCap lineCap, float miterLimit = 4f, ClosePathRule closeLine = ClosePathRule.NEVER, float roundQuality = 10f)
		{
			if (segments == null || segments.Length == 0)
			{
				return null;
			}
			if (segments.Length == 1)
			{
				closeLine = ClosePathRule.NEVER;
			}
			else if (closeLine == ClosePathRule.AUTO)
			{
				closeLine = ((!(segments[0].startPoint == segments[segments.Length - 1].endPoint)) ? ClosePathRule.NEVER : ClosePathRule.ALWAYS);
			}
			if (segments[0].startPoint == segments[segments.Length - 1].endPoint)
			{
				List<StrokeSegment> list = new List<StrokeSegment>(segments);
				list.RemoveAt(list.Count - 1);
				segments = list.ToArray();
			}
			List<Vector2> list2 = new List<Vector2>();
			List<Vector2> list3 = new List<Vector2>();
			if (closeLine == ClosePathRule.ALWAYS)
			{
				segments = new List<StrokeSegment>(segments)
				{
					new StrokeSegment(segments[segments.Length - 1].endPoint, segments[0].startPoint),
					new StrokeSegment(segments[0].startPoint, segments[0].endPoint)
				}.ToArray();
			}
			miterLimit = (miterLimit - 1f) * thickness * 2f;
			if (miterLimit < 1f)
			{
				miterLimit = 1f;
			}
			int num = segments.Length;
			float num2 = thickness * 0.5f;
			float num3 = 0f;
			float num4 = miterLimit * 0.5f;
			float num5 = num4 * num4;
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector2.zero, Quaternion.Euler(0f, 0f, 90f), Vector2.one);
			if (lineCap == StrokeLineCap.butt || closeLine == ClosePathRule.ALWAYS)
			{
				list2.AddRange(new Vector2[2]
				{
					segments[0].startPoint - segments[0].directionNormalizedRotated * num2,
					segments[0].startPoint + segments[0].directionNormalizedRotated * num2
				});
			}
			else
			{
				switch (lineCap)
				{
				case StrokeLineCap.round:
				{
					Vector2 vector = Vector2.Lerp(segments[0].startPoint - segments[0].directionNormalizedRotated * num2, segments[0].startPoint + segments[0].directionNormalizedRotated * num2, 0.5f);
					num3 = Mathf.Atan2(segments[0].directionNormalizedRotated.y, segments[0].directionNormalizedRotated.x);
					float num6 = roundQuality * thickness;
					float num7 = num6 - 1f;
					if (num7 > 0f)
					{
						for (int i = 0; (float)i <= num6; i++)
						{
							float num8 = 1f - Mathf.Clamp01((float)i / num7);
							list2.Add(vector + new Vector2(Mathf.Cos(num3 + num8 * (float)Math.PI) * num2, Mathf.Sin(num3 + num8 * (float)Math.PI) * num2));
						}
					}
					list2.AddRange(new Vector2[1] { segments[0].startPoint + segments[0].directionNormalizedRotated * num2 });
					list3.AddRange(new Vector2[1] { segments[0].startPoint - segments[0].directionNormalizedRotated * num2 });
					break;
				}
				case StrokeLineCap.square:
					list2.AddRange(new Vector2[2]
					{
						segments[0].startPoint - segments[0].directionNormalized * num2 - segments[0].directionNormalizedRotated * num2,
						segments[0].startPoint - segments[0].directionNormalized * num2 + segments[0].directionNormalizedRotated * num2
					});
					break;
				}
			}
			Vector2 vector4;
			Vector2 vector5;
			if (num > 1)
			{
				for (int j = 1; j < num; j++)
				{
					int num9 = j - 1;
					float num10 = Vector2.Dot(segments[j].directionNormalized, segments[num9].directionNormalized);
					float num11 = Vector2.Dot(segments[j].directionNormalized, segments[num9].directionNormalizedRotated);
					float num12 = 1f / Mathf.Sin(((float)Math.PI - Mathf.Acos(num10)) * 0.5f) * thickness;
					float num13 = num12 * 0.5f;
					Vector2 normalized = Vector2.Lerp(segments[num9].directionNormalizedRotated, segments[j].directionNormalizedRotated, 0.5f).normalized;
					Vector2 vector2 = normalized * num13;
					Vector2 vector3 = matrix4x.MultiplyVector(normalized);
					vector4 = segments[j].startPoint - segments[j].directionNormalizedRotated * num2;
					zero = segments[j].endPoint - segments[j].directionNormalizedRotated * num2;
					vector5 = segments[j].startPoint + segments[j].directionNormalizedRotated * num2;
					zero2 = segments[j].endPoint + segments[j].directionNormalizedRotated * num2;
					Vector2 vector6 = segments[num9].endPoint - segments[num9].directionNormalizedRotated * num2;
					Vector2 vector7 = segments[num9].endPoint + segments[num9].directionNormalizedRotated * num2;
					if (lineJoin == StrokeLineJoin.miter && miterLimit < num12)
					{
						lineJoin = StrokeLineJoin.bevel;
					}
					switch (lineJoin)
					{
					case StrokeLineJoin.miter:
					case StrokeLineJoin.miterClip:
					{
						Vector2 intersection;
						Vector2 intersection2;
						if (num10 == 1f || num10 == -1f)
						{
							list2.AddRange(new Vector2[2] { vector7, vector5 });
							list3.AddRange(new Vector2[2] { vector6, vector4 });
						}
						else if (num11 < 0f)
						{
							if (miterLimit <= num12)
							{
								Vector2 vector9 = segments[num9].endPoint + normalized * num4;
								Vector2 line1Start = segments[num9].endPoint + vector2;
								Vector2 line2End = vector9 + vector3;
								SVGMath.LineLineIntersection(out intersection, line1Start, vector7, vector9, line2End);
								SVGMath.LineLineIntersection(out intersection2, line1Start, vector5, vector9, line2End);
								if (num5 <= (Vector2.Lerp(vector7, vector5, 0.5f) - segments[num9].endPoint).sqrMagnitude)
								{
									intersection = vector7;
									intersection2 = vector5;
								}
								list2.AddRange(new Vector2[2] { intersection, intersection2 });
								list3.AddRange(new Vector2[2] { vector6, vector4 });
							}
							else
							{
								intersection2 = segments[num9].endPoint + vector2;
								list2.AddRange(new Vector2[1] { intersection2 });
								list3.AddRange(new Vector2[2] { vector6, vector4 });
							}
						}
						else if (miterLimit <= num12)
						{
							Vector2 vector10 = segments[num9].endPoint - normalized * num4;
							Vector2 line1Start2 = segments[num9].endPoint - vector2;
							Vector2 line2End2 = vector10 + vector3;
							SVGMath.LineLineIntersection(out intersection, line1Start2, vector4, vector10, line2End2);
							SVGMath.LineLineIntersection(out intersection2, line1Start2, vector6, vector10, line2End2);
							if (num5 <= (Vector2.Lerp(vector4, vector6, 0.5f) - segments[num9].endPoint).sqrMagnitude)
							{
								intersection = vector4;
								intersection2 = vector6;
							}
							list3.AddRange(new Vector2[2] { intersection2, intersection });
							list2.AddRange(new Vector2[2] { vector7, vector5 });
						}
						else
						{
							intersection = segments[num9].endPoint - vector2;
							list3.AddRange(new Vector2[1] { intersection });
							list2.AddRange(new Vector2[2] { vector7, vector5 });
						}
						break;
					}
					case StrokeLineJoin.bevel:
						list2.AddRange(new Vector2[2] { vector7, vector5 });
						list3.AddRange(new Vector2[2] { vector6, vector4 });
						break;
					case StrokeLineJoin.round:
					{
						if (num10 == 1f)
						{
							list2.AddRange(new Vector2[2] { vector7, vector5 });
							list3.AddRange(new Vector2[2] { vector6, vector4 });
							break;
						}
						Vector2 vector;
						if (num11 < 0f)
						{
							list2.AddRange(new Vector2[1] { vector7 });
							list3.AddRange(new Vector2[2] { vector6, vector4 });
							vector = segments[j].startPoint;
							Vector2 directionNormalizedRotated = segments[num9].directionNormalizedRotated;
							float num14 = Mathf.Acos(Vector2.Dot(segments[num9].directionNormalized, segments[j].directionNormalized));
							num3 = Mathf.Atan2(directionNormalizedRotated.y, directionNormalizedRotated.x);
							float num15 = roundQuality * thickness * (Mathf.Acos(num10) / (float)Math.PI);
							if (num15 < 1f)
							{
								num15 = 1f;
							}
							float num16 = num15;
							if (num16 > 0f)
							{
								for (int i = 0; (float)i < num15; i++)
								{
									float num8 = Mathf.Clamp01((float)i / num16);
									list2.Add(vector + new Vector2(Mathf.Cos(num3 - num8 * num14) * num2, Mathf.Sin(num3 - num8 * num14) * num2));
								}
							}
							list2.AddRange(new Vector2[1] { vector5 });
							break;
						}
						list2.AddRange(new Vector2[2] { vector7, vector5 });
						list3.AddRange(new Vector2[1] { vector6 });
						vector = segments[j].startPoint;
						Vector2 vector8 = -segments[j].directionNormalizedRotated;
						float num17 = Mathf.Acos(Vector2.Dot(segments[num9].directionNormalized, segments[j].directionNormalized));
						num3 = Mathf.Atan2(vector8.y, vector8.x);
						float num18 = roundQuality * thickness * (Mathf.Acos(num10) / (float)Math.PI);
						if (num18 < 1f)
						{
							num18 = 1f;
						}
						float num19 = num18;
						if (num19 > 0f)
						{
							for (int i = 0; (float)i < num18; i++)
							{
								float num8 = Mathf.Clamp01(1f - (float)i / num19);
								list3.Add(vector + new Vector2(Mathf.Cos(num3 - num8 * num17) * num2, Mathf.Sin(num3 - num8 * num17) * num2));
							}
						}
						list3.AddRange(new Vector2[1] { vector4 });
						break;
					}
					}
				}
			}
			int num20 = segments.Length - 1;
			vector4 = segments[num20].startPoint - segments[num20].directionNormalizedRotated * num2;
			zero = segments[num20].endPoint - segments[num20].directionNormalizedRotated * num2;
			vector5 = segments[num20].startPoint + segments[num20].directionNormalizedRotated * num2;
			zero2 = segments[num20].endPoint + segments[num20].directionNormalizedRotated * num2;
			if (closeLine == ClosePathRule.NEVER)
			{
				switch (lineCap)
				{
				case StrokeLineCap.butt:
					list2.AddRange(new Vector2[2] { zero2, zero });
					break;
				case StrokeLineCap.round:
				{
					list2.AddRange(new Vector2[1] { zero2 });
					list3.AddRange(new Vector2[1] { zero });
					Vector2 vector = Vector2.Lerp(zero, zero2, 0.5f);
					num3 = Mathf.Atan2(0f - segments[num20].directionNormalizedRotated.y, 0f - segments[num20].directionNormalizedRotated.x);
					float num21 = roundQuality * thickness;
					float num22 = num21 - 1f;
					if (num22 > 0f)
					{
						for (int i = 0; (float)i <= num21; i++)
						{
							float num8 = 1f - Mathf.Clamp01((float)i / num22);
							list2.Add(vector + new Vector2(Mathf.Cos(num3 + num8 * (float)Math.PI) * num2, Mathf.Sin(num3 + num8 * (float)Math.PI) * num2));
						}
					}
					break;
				}
				case StrokeLineCap.square:
				{
					Vector2 vector11 = segments[num20].directionNormalized * num2;
					list2.AddRange(new Vector2[2]
					{
						zero2 + vector11,
						zero + vector11
					});
					break;
				}
				}
			}
			if ((closeLine == ClosePathRule.ALWAYS && lineJoin == StrokeLineJoin.miter) || lineJoin == StrokeLineJoin.miterClip)
			{
				list2.AddRange(new Vector2[2] { zero2, zero });
			}
			list3.Reverse();
			list2.AddRange(list3);
			return list2;
		}

		public static UnityEngine.Mesh StrokeMesh(StrokeSegment[] segments, float thickness, Color32 color, StrokeLineJoin lineJoin, StrokeLineCap lineCap, float miterLimit = 4f, float[] dashArray = null, float dashOffset = 0f, ClosePathRule closeLine = ClosePathRule.NEVER, float roundQuality = 10f)
		{
			if (segments == null || segments.Length == 0)
			{
				return null;
			}
			return TessellateStroke(StrokeShape(new List<StrokeSegment[]> { segments }, thickness, color, lineJoin, lineCap, miterLimit, dashArray, dashOffset, closeLine, roundQuality), color);
		}

		public static List<List<Vector2>> StrokeShape(List<StrokeSegment[]> segments, float thickness, Color32 color, StrokeLineJoin lineJoin, StrokeLineCap lineCap, float miterLimit = 4f, float[] dashArray = null, float dashOffset = 0f, ClosePathRule closeLine = ClosePathRule.NEVER, float roundQuality = 10f)
		{
			if (segments == null || segments.Count == 0)
			{
				return null;
			}
			float num = 0f;
			for (int i = 0; i < segments.Count; i++)
			{
				if (segments[i] != null)
				{
					for (int j = 0; j < segments[i].Length; j++)
					{
						num += segments[i][j].length;
					}
				}
			}
			if (num == 0f)
			{
				return null;
			}
			ProcessDashArray(ref dashArray, out var useDash);
			ClosePathRule closeLine2 = closeLine;
			List<StrokeSegment[]> list = new List<StrokeSegment[]>();
			for (int i = 0; i < segments.Count; i++)
			{
				if (segments[i] != null && segments[i].Length != 0)
				{
					if (!useDash)
					{
						list.Add(segments[i]);
					}
					else
					{
						list.AddRange(CreateDashedStroke(segments[i], dashArray, dashOffset, ref closeLine2));
					}
				}
			}
			if (list.Count > 0)
			{
				List<List<Vector2>> list2 = new List<List<Vector2>>();
				for (int i = 0; i < list.Count; i++)
				{
					List<Vector2> list3 = Stroke(list[i], thickness, color, lineJoin, lineCap, miterLimit, closeLine2, roundQuality);
					if (list3 == null || list3.Count < 2)
					{
						continue;
					}
					List<List<Vector2>> list4 = SVGGeom.SimplifyPolygon(list3);
					for (int j = 0; j < list4.Count; j++)
					{
						if (list4[j] != null && list4[j].Count != 0)
						{
							list2.Add(list4[j]);
						}
					}
				}
				return list2;
			}
			return null;
		}

		protected static List<StrokeSegment[]> CreateDashedStroke(StrokeSegment[] segments, float[] dashArray, float dashOffset, ref ClosePathRule closeLine)
		{
			if (segments == null || segments.Length == 0)
			{
				return null;
			}
			if (closeLine == ClosePathRule.ALWAYS || closeLine == ClosePathRule.AUTO)
			{
				Array.Resize(ref segments, segments.Length + 1);
				segments[segments.Length - 1] = new StrokeSegment(segments[segments.Length - 2].endPoint, segments[0].startPoint);
				closeLine = ClosePathRule.NEVER;
			}
			List<StrokeSegment[]> list = new List<StrokeSegment[]>();
			int num = dashArray.Length;
			int num2 = 0;
			int num3 = segments.Length;
			float num4 = dashOffset;
			List<StrokeSegment> list2 = new List<StrokeSegment>();
			int num5 = 0;
			while (num5 < num3)
			{
				if (num2 % 2 == 0)
				{
					float num6 = Mathf.Clamp(num4, 0f, segments[num5].length);
					float num7 = Mathf.Clamp(num4 + dashArray[num2], 0f, segments[num5].length);
					if (num7 - num6 > 0f)
					{
						list2.Add(new StrokeSegment(segments[num5].startPoint + segments[num5].directionNormalized * num6, segments[num5].startPoint + segments[num5].directionNormalized * num7));
					}
				}
				else if (list2.Count > 0)
				{
					list.Add(list2.ToArray());
					list2.Clear();
				}
				if (num4 + dashArray[num2] < segments[num5].length)
				{
					num4 += dashArray[num2];
					num2 = (num2 + 1) % num;
				}
				else
				{
					num4 -= segments[num5].length;
					num5++;
				}
			}
			if (list2.Count > 0)
			{
				list.Add(list2.ToArray());
				list2.Clear();
			}
			return list;
		}

		protected static void ProcessDashArray(ref float[] dashArray, out bool useDash)
		{
			useDash = dashArray != null && dashArray.Length != 0;
			float num = 0f;
			if (useDash)
			{
				int num2 = dashArray.Length;
				if (num2 % 2 == 1)
				{
					Array.Resize(ref dashArray, num2 * 2);
					int num3 = 0;
					for (int i = num2; i < dashArray.Length; i++)
					{
						dashArray[i] = dashArray[num3++];
					}
					num2 = dashArray.Length;
				}
				for (int i = 0; i < dashArray.Length; i++)
				{
					if (dashArray[i] < 0f)
					{
						dashArray[i] = 0f;
					}
					num += dashArray[i];
				}
			}
			if (num == 0f)
			{
				useDash = false;
			}
		}

		public static void TesselateStroke(List<List<Vector2>> inputShapes, Color32 color, out List<List<Vector2>> simplifiedShapes, out Vector3[] vertices, out int[] triangles, out Color32[] colors32)
		{
			simplifiedShapes = null;
			vertices = null;
			triangles = null;
			colors32 = null;
			if (inputShapes == null || inputShapes.Count == 0)
			{
				return;
			}
			simplifiedShapes = new List<List<Vector2>>();
			PolyFillType polyFillType = PolyFillType.pftNonZero;
			for (int i = 0; i < inputShapes.Count; i++)
			{
				if (inputShapes[i] != null && inputShapes.Count != 0)
				{
					List<List<Vector2>> list = SVGGeom.SimplifyPolygon(inputShapes[i], polyFillType);
					if (list == null || list.Count == 0)
					{
						simplifiedShapes.Add(inputShapes[i]);
					}
					else
					{
						simplifiedShapes.AddRange(list);
					}
				}
			}
			Tess tess = new Tess();
			for (int i = 0; i < simplifiedShapes.Count; i++)
			{
				if (simplifiedShapes[i] != null && simplifiedShapes[i].Count >= 2)
				{
					ContourVertex[] array = new ContourVertex[simplifiedShapes[i].Count];
					for (int j = 0; j < simplifiedShapes[i].Count; j++)
					{
						array[j].Position = new Vec3
						{
							X = simplifiedShapes[i][j].x,
							Y = simplifiedShapes[i][j].y,
							Z = 0f
						};
					}
					tess.AddContour(array);
				}
			}
			tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
			if (tess.Vertices != null && tess.Vertices.Length != 0)
			{
				int num = tess.Vertices.Length;
				int num2 = tess.ElementCount * 3;
				triangles = new int[num2];
				vertices = new Vector3[num];
				colors32 = new Color32[num];
				for (int i = 0; i < num; i++)
				{
					vertices[i] = new Vector3(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y, 0f);
					colors32[i] = color;
				}
				for (int i = 0; i < num2; i += 3)
				{
					triangles[i] = tess.Elements[i];
					triangles[i + 1] = tess.Elements[i + 1];
					triangles[i + 2] = tess.Elements[i + 2];
				}
			}
		}

		public static UnityEngine.Mesh TessellateStroke(List<List<Vector2>> inputShapes, Color32 color)
		{
			TesselateStroke(inputShapes, color, out var _, out var vertices, out var triangles, out var colors);
			if (vertices == null)
			{
				return null;
			}
			return new UnityEngine.Mesh
			{
				vertices = vertices,
				triangles = triangles,
				colors32 = colors
			};
		}

		public static float DeltaAngleRad(float current, float target)
		{
			float num = Mathf.Repeat(target - current, (float)Math.PI * 2f);
			if (num > (float)Math.PI)
			{
				num -= (float)Math.PI * 2f;
			}
			return num;
		}
	}
}
