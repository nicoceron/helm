using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGMeshUtils
	{
		public enum ColorChannel
		{
			RED = 0,
			GREEN = 1,
			BLUE = 2,
			ALPHA = 3
		}

		private const float PI2 = (float)Math.PI * 2f;

		public static Vector2 lineUVScale = Vector2.one;

		public static Vector2 lineUVOffset = Vector2.zero;

		public static Mesh Quad()
		{
			return Quad(new Vector2(1f, 1f));
		}

		public static Mesh Quad(float size)
		{
			return Quad(new Vector2(size, size));
		}

		public static Mesh Quad(Vector2 size)
		{
			Mesh mesh = new Mesh();
			Vector3[] array = new Vector3[4];
			int[] array2 = new int[6];
			Vector2[] array3 = new Vector2[4];
			Color32[] array4 = new Color32[4];
			array[0] = new Vector3(0f - size.x, size.y, 0f);
			array[1] = new Vector3(size.x, size.y, 0f);
			array[2] = new Vector3(0f - size.x, 0f - size.y, 0f);
			array[3] = new Vector3(size.x, 0f - size.y, 0f);
			array2[0] = 0;
			array2[1] = 1;
			array2[2] = 2;
			array2[3] = 1;
			array2[4] = 3;
			array2[5] = 2;
			array3[0] = new Vector2(0f, 1f);
			array3[1] = new Vector2(1f, 1f);
			array3[2] = new Vector2(0f, 0f);
			array3[3] = new Vector2(1f, 0f);
			array4[0] = Color.black;
			array4[1] = Color.black;
			array4[2] = Color.black;
			array4[3] = Color.black;
			mesh.vertices = array;
			mesh.triangles = array2;
			mesh.uv = array3;
			mesh.colors32 = array4;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Mesh Quad(Vector2 size, int hSegments, int vSegments)
		{
			return Quad(size, hSegments, vSegments, Vector3.zero);
		}

		public static Mesh Quad(Vector2 size, int hSegments, int vSegments, Vector3 anchorOffset, Color32 color)
		{
			Mesh mesh = new Mesh();
			if (hSegments < 1)
			{
				hSegments = 1;
			}
			if (vSegments < 1)
			{
				vSegments = 1;
			}
			int num = hSegments;
			int num2 = vSegments;
			int num3 = num + 1;
			int num4 = num2 + 1;
			int num5 = num * num2 * 6;
			int num6 = num3 * num4;
			Vector3[] array = new Vector3[num6];
			Vector2[] array2 = new Vector2[num6];
			Color32[] array3 = new Color32[num6];
			int[] array4 = new int[num5];
			int num7 = 0;
			float num8 = 1f / (float)num;
			float num9 = 1f / (float)num2;
			float num10 = size.x / (float)num;
			float num11 = size.y / (float)num2;
			for (float num12 = 0f; num12 < (float)num4; num12 += 1f)
			{
				for (float num13 = 0f; num13 < (float)num3; num13 += 1f)
				{
					array[num7] = new Vector3(num13 * num10 - size.x / 2f + anchorOffset.x, num12 * num11 - size.y / 2f + anchorOffset.y, anchorOffset.z);
					array3[num7] = color;
					array2[num7++] = new Vector2(num13 * num8, num12 * num9);
				}
			}
			num7 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					array4[num7] = i * num3 + j;
					array4[num7 + 1] = (i + 1) * num3 + j;
					array4[num7 + 2] = i * num3 + j + 1;
					array4[num7 + 3] = (i + 1) * num3 + j;
					array4[num7 + 4] = (i + 1) * num3 + j + 1;
					array4[num7 + 5] = i * num3 + j + 1;
					num7 += 6;
				}
			}
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.colors32 = array3;
			mesh.triangles = array4;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Mesh Quad(Vector2 size, int hSegments, int vSegments, Vector3 anchorOffset)
		{
			return Quad(size, hSegments, vSegments, anchorOffset, Color.white);
		}

		public static Mesh Circle(int circuitSegments, Matrix4x4 meshTransform, Matrix4x4 uvTransform)
		{
			circuitSegments = Mathf.Clamp(circuitSegments, 4, int.MaxValue) + 1;
			int num = circuitSegments - 1;
			Mesh mesh = new Mesh();
			int num2 = circuitSegments + 1;
			int num3 = circuitSegments * 3;
			Vector3[] array = new Vector3[num2];
			Vector2[] array2 = new Vector2[num2];
			int[] array3 = new int[num3];
			Vector2 vector = default(Vector2);
			for (int i = 0; i < circuitSegments; i++)
			{
				float num4 = (float)i / (float)num;
				float x = Mathf.Cos(num4 * ((float)Math.PI * 2f)) * 0.5f;
				float y = Mathf.Sin(num4 * ((float)Math.PI * 2f)) * 0.5f;
				array[i].x = x;
				array[i].y = y;
				array[i] = meshTransform.MultiplyPoint(array[i]);
				array[i].z = 0f;
				vector.x = x;
				vector.y = y;
				vector = uvTransform.MultiplyPoint(vector);
				array2[i].x = vector.x + 0.5f;
				array2[i].y = vector.y + 0.5f;
			}
			array[circuitSegments] = meshTransform.MultiplyPoint(array[circuitSegments]);
			vector.x = (vector.y = 0f);
			vector = uvTransform.MultiplyPoint(vector);
			array2[circuitSegments].x = vector.x + 0.5f;
			array2[circuitSegments].y = vector.y + 0.5f;
			int num5 = 0;
			for (int j = 0; j < num3; j += 3)
			{
				array3[j] = num5;
				array3[j + 2] = num5 + 1;
				array3[j + 1] = circuitSegments;
				num5++;
			}
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.triangles = array3;
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Mesh Rectangle(Matrix4x4 meshTransform, Matrix4x4 uvTransform)
		{
			Mesh mesh = new Mesh();
			Vector3[] array = new Vector3[4];
			int[] array2 = new int[6];
			Vector2[] array3 = new Vector2[4];
			array[0].x = -0.5f;
			array[0].y = 0.5f;
			array[0] = meshTransform.MultiplyPoint(array[0]);
			array[0].z = 0f;
			array[1].x = 0.5f;
			array[1].y = 0.5f;
			array[1] = meshTransform.MultiplyPoint(array[1]);
			array[1].z = 0f;
			array[2].x = -0.5f;
			array[2].y = -0.5f;
			array[2] = meshTransform.MultiplyPoint(array[2]);
			array[2].z = 0f;
			array[3].x = 0.5f;
			array[3].y = -0.5f;
			array[3] = meshTransform.MultiplyPoint(array[3]);
			array[3].z = 0f;
			array2[0] = 0;
			array2[1] = 1;
			array2[2] = 2;
			array2[3] = 1;
			array2[4] = 3;
			array2[5] = 2;
			array3[0].x = -0.5f;
			array3[0].y = 0.5f;
			Vector2 vector = uvTransform.MultiplyPoint(new Vector3(array3[0].x, array3[0].y, 0f));
			array3[0].x = vector.x + 0.5f;
			array3[0].y = vector.y + 0.5f;
			array3[1].x = 0.5f;
			array3[1].y = 0.5f;
			vector = uvTransform.MultiplyPoint(new Vector3(array3[1].x, array3[1].y, 0f));
			array3[1].x = vector.x + 0.5f;
			array3[1].y = vector.y + 0.5f;
			array3[2].x = -0.5f;
			array3[2].y = -0.5f;
			vector = uvTransform.MultiplyPoint(new Vector3(array3[2].x, array3[2].y, 0f));
			array3[2].x = vector.x + 0.5f;
			array3[2].y = vector.y + 0.5f;
			array3[3].x = 0.5f;
			array3[3].y = -0.5f;
			vector = uvTransform.MultiplyPoint(new Vector3(array3[3].x, array3[3].y, 0f));
			array3[3].x = vector.x + 0.5f;
			array3[3].y = vector.y + 0.5f;
			mesh.vertices = array;
			mesh.triangles = array2;
			mesh.uv = array3;
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Mesh Rectangle()
		{
			return Rectangle(Matrix4x4.identity, Matrix4x4.identity);
		}

		public static Mesh Line(int tessellation, Vector3[] positions, Color32 color, float size = 1f, bool closeLine = false)
		{
			return Line(tessellation, positions, SVGArrayUtils.CreatePreinitializedArray(color, positions.Length), SVGArrayUtils.CreatePreinitializedArray(size, positions.Length), null, closeLine);
		}

		public static Mesh Line(int tessellation, Vector2[] positions, Color32 color, float size = 1f, bool closeLine = false)
		{
			Vector3[] array = new Vector3[positions.Length];
			for (int i = 0; i < positions.Length; i++)
			{
				array[i].x = positions[i].x;
				array[i].y = positions[i].y;
			}
			return Line(tessellation, array, SVGArrayUtils.CreatePreinitializedArray(color, positions.Length), SVGArrayUtils.CreatePreinitializedArray(size, positions.Length), null, closeLine);
		}

		public static Mesh Line(int tessellation, Vector2[] positions, Color32[] colors = null, float[] sizes = null, Vector3[] rotations = null, bool closeLine = false)
		{
			Vector3[] array = new Vector3[positions.Length];
			for (int i = 0; i < positions.Length; i++)
			{
				array[i].x = positions[i].x;
				array[i].y = positions[i].y;
			}
			return Line(tessellation, array, colors, sizes, rotations, closeLine);
		}

		public static void ResetLineSettings()
		{
			lineUVScale = Vector2.one;
			lineUVOffset = Vector2.zero;
		}

		public static Mesh Line(int tessellation, Vector3[] positions, Color32[] colors = null, float[] sizes = null, Vector3[] rotations = null, bool closeLine = false)
		{
			if (positions == null)
			{
				ResetLineSettings();
				return null;
			}
			if (positions.Length < 2)
			{
				ResetLineSettings();
				return null;
			}
			if (tessellation < 1)
			{
				tessellation = 1;
			}
			int num = tessellation * 2;
			int num2 = positions.Length * num;
			int num3 = positions.Length;
			int num4 = (num3 - 1) * (num - 1) * 6;
			bool flag = colors != null && colors.Length == num3;
			bool flag2 = rotations != null && rotations.Length == num3;
			Vector3[] array = new Vector3[num2];
			int[] array2 = new int[num4];
			Color32[] array3 = null;
			if (flag)
			{
				array3 = new Color32[num2];
			}
			if (sizes == null)
			{
				sizes = SVGArrayUtils.CreatePreinitializedArray(1f, num2);
			}
			Vector2[] array4 = new Vector2[num2];
			Vector3[] array5 = new Vector3[num3];
			float[] array6 = new float[num3];
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			Vector3 vector = positions[0];
			Vector3 vector2 = positions[0];
			float num8 = 0f;
			float num9 = 1f;
			float num10 = 0f;
			Color32 color = Color.white;
			for (num5 = 0; num5 < num3; num5++)
			{
				array5[num5].x = positions[num5].x - vector.x;
				array5[num5].y = positions[num5].y - vector.y;
				array5[num5].z = positions[num5].z - vector.z;
				array6[num5] = Mathf.Sqrt(array5[num5].x * array5[num5].x + array5[num5].y * array5[num5].y + array5[num5].z * array5[num5].z);
				if (array6[num5] != 0f)
				{
					array5[num5].x /= array6[num5];
					array5[num5].y /= array6[num5];
					array5[num5].z /= array6[num5];
				}
				if (flag2)
				{
					array5[num5].x += rotations[num5].x;
					array5[num5].y += rotations[num5].y;
					array5[num5].z += rotations[num5].z;
				}
				num10 += array6[num5];
				vector = positions[num5];
			}
			array5[0] = (positions[1] - positions[0]).normalized;
			if (flag2)
			{
				array5[0] += rotations[0];
			}
			vector = positions[0];
			int num11 = 0;
			Vector3 b = Vector3.Cross(array5[0], Vector3.forward);
			int num12 = array5.Length - 1;
			for (num5 = 0; num5 < num2; num5 += num)
			{
				int num13 = num5 / num;
				vector2 = positions[num13];
				num9 = sizes[num13] * 0.5f;
				if (flag)
				{
					color = colors[num13];
				}
				Vector3 vector3 = Vector3.Cross(array5[num13], Vector3.forward);
				if (num13 < num12)
				{
					b = Vector3.Cross(array5[num13 + 1], Vector3.forward);
				}
				num8 += array6[num13] / num10;
				for (num6 = 0; num6 < num; num6++)
				{
					num7 = num5 + num6;
					float num14 = (float)num6 / (float)(num - 1);
					array[num7] = vector2 + Vector3.Lerp(vector3, b, 0.5f).normalized * (-1f + num14 * 2f) * num9;
					if (flag)
					{
						array3[num7] = color;
					}
					array4[num7].x = num14 * lineUVScale.x + lineUVOffset.x;
					array4[num7].y = num8 * lineUVScale.y + lineUVOffset.y;
					if (num5 != 0 && num6 != 0)
					{
						array2[num11] = num7 - num - 1;
						array2[num11 + 1] = num7 - 1;
						array2[num11 + 2] = num7;
						array2[num11 + 3] = num7 - num - 1;
						array2[num11 + 4] = num7;
						array2[num11 + 5] = num7 - num;
						num11 += 6;
					}
				}
				b = vector3;
			}
			if (closeLine)
			{
				num9 = Mathf.Lerp(sizes[0], sizes[num3 - 1], 0.5f) * 0.5f;
				Vector3 vector4 = Vector3.Cross(Vector3.Lerp(array5[0], array5[num3 - 1], 0.5f).normalized, Vector3.forward);
				Vector3 vector5 = Vector3.Lerp(positions[0], positions[num3 - 1], 0.5f);
				Vector3 a = vector5 - vector4 * num9;
				Vector3 b2 = vector5 + vector4 * num9;
				float num15 = num - 1;
				for (num5 = 0; num5 < num; num5++)
				{
					float t = (float)num5 / num15;
					int num16 = num2 - num + num5;
					int num17 = num5;
					array[num16] = (array[num17] = Vector3.Lerp(a, b2, t));
					if (flag)
					{
						array3[num16] = (array3[num17] = Color32.Lerp(array3[num16], array3[num17], 0.5f));
					}
				}
			}
			else
			{
				num9 = sizes[num3 - 1] * 0.5f;
				Vector3 vector6 = Vector3.Cross(array5[num3 - 1], Vector3.forward);
				Vector3 vector7 = positions[num3 - 1];
				Vector3 a2 = vector7 - vector6 * num9;
				Vector3 b3 = vector7 + vector6 * num9;
				float num18 = num - 1;
				for (num5 = 0; num5 < num; num5++)
				{
					float t2 = (float)num5 / num18;
					int num19 = num2 - num + num5;
					array[num19] = Vector3.Lerp(a2, b3, t2);
					if (flag)
					{
						array3[num19] = array3[num19];
					}
				}
			}
			Mesh mesh = new Mesh();
			mesh.vertices = array;
			mesh.triangles = array2;
			mesh.uv = array4;
			if (flag)
			{
				mesh.colors32 = array3;
			}
			ResetLineSettings();
			return mesh;
		}

		public static bool VectorLine(Vector2[] positions, out SVGShape svgLayer, Color32 colorA, Color32 colorB, float size, float offset, ClosePathRule closeLine = ClosePathRule.NEVER)
		{
			svgLayer = default(SVGShape);
			if (positions == null)
			{
				return false;
			}
			if (positions.Length < 2)
			{
				return false;
			}
			if (positions.Length == 2)
			{
				closeLine = ClosePathRule.NEVER;
			}
			else if (closeLine == ClosePathRule.AUTO && positions[0] == positions[positions.Length - 1])
			{
				closeLine = ClosePathRule.ALWAYS;
			}
			SVGLineData sVGLineData = new SVGLineData(positions);
			sVGLineData.UpdateAll();
			size *= 0.5f;
			int capacity = positions.Length * 2;
			int capacity2 = (positions.Length - 1) * 6;
			int num = 0;
			int num2 = 0;
			List<Vector2> list = new List<Vector2>(capacity);
			List<int> list2 = new List<int>(capacity2);
			List<Color32> list3 = new List<Color32>(capacity);
			List<Vector2> list4 = new List<Vector2>(capacity);
			int edgeCount = sVGLineData.GetEdgeCount();
			int index = edgeCount - 1;
			float num3 = size;
			Vector2 intersection;
			for (num = 0; num <= edgeCount; num++)
			{
				Vector2 vector = positions[num];
				Vector2 vector2;
				Vector2 vector3;
				Vector2 vector4;
				Vector2 vector5;
				if (num == 0)
				{
					vector2 = positions[0];
					vector3 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(0));
					vector4 = positions[1];
					vector5 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(0));
				}
				else if (num == edgeCount)
				{
					vector2 = positions[num - 1];
					vector3 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(num - 1));
					if (closeLine == ClosePathRule.ALWAYS)
					{
						vector4 = positions[0];
						vector5 = SVGMath.RotateVectorClockwise((positions[positions.Length - 1] - positions[0]).normalized);
					}
					else
					{
						vector4 = positions[positions.Length - 1];
						vector5 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(index));
					}
				}
				else
				{
					vector2 = positions[num - 1];
					vector3 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(num - 1));
					vector5 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(num));
					vector4 = positions[num + 1];
				}
				Vector2 line1Start = vector2 + vector3 * num3 + vector3 * offset;
				Vector2 vector6 = vector + vector3 * num3 + vector3 * offset;
				Vector2 vector7 = vector + vector5 * num3 + vector5 * offset;
				Vector2 line2End = vector4 + vector5 * num3 + vector5 * offset;
				if (num == 0)
				{
					Vector2[] array = new Vector2[2]
					{
						vector + vector3 * (0f - num3) + vector3 * offset,
						vector + vector3 * num3 + vector3 * offset
					};
					list.AddRange(array);
					list3.AddRange(new Color32[2] { colorA, colorB });
					list4.AddRange(new Vector2[2]
					{
						Vector2.zero,
						array[1] - array[0]
					});
					num2 += 2;
				}
				else if (!SVGMath.LineLineIntersection(out intersection, line1Start, vector6, vector7, line2End))
				{
					Vector2 normalized = Vector2.Lerp(vector3, vector5, 0.5f).normalized;
					Vector2 vector8 = normalized * offset;
					if (num == edgeCount && closeLine != ClosePathRule.ALWAYS)
					{
						Vector2[] collection = new Vector2[2]
						{
							vector6,
							vector + normalized * (0f - num3) + vector8
						};
						list.AddRange(collection);
						list3.AddRange(new Color32[2] { colorB, colorA });
						list4.AddRange(new Vector2[2]
						{
							Vector2.zero,
							Vector2.zero
						});
						num2 += 2;
						list2.AddRange(new int[6]
						{
							num2 - 4,
							num2 - 2,
							num2 - 1,
							num2 - 2,
							num2 - 4,
							num2 - 3
						});
					}
					else
					{
						Vector2[] array2 = new Vector2[3]
						{
							vector6,
							vector + normalized * (0f - num3) + vector8,
							vector7
						};
						list.AddRange(array2);
						list3.AddRange(new Color32[3] { colorB, colorA, colorB });
						list4.AddRange(new Vector2[3]
						{
							array2[0] - vector,
							Vector2.zero,
							array2[2] - vector
						});
						num2 += 3;
						list2.AddRange(new int[9]
						{
							num2 - 3,
							num2 - 2,
							num2 - 5,
							num2 - 5,
							num2 - 4,
							num2 - 3,
							num2 - 1,
							num2 - 2,
							num2 - 3
						});
					}
				}
				else
				{
					Vector2 normalized = Vector2.Lerp(vector3, vector5, 0.5f).normalized;
					Vector2 vector8 = normalized * offset;
					Vector2[] array3 = new Vector2[2]
					{
						vector + normalized * (0f - num3) + vector8,
						intersection
					};
					list.AddRange(array3);
					list3.AddRange(new Color32[2] { colorA, colorB });
					list4.AddRange(new Vector2[2]
					{
						Vector2.zero,
						array3[1] - array3[0]
					});
					num2 += 2;
					list2.AddRange(new int[6]
					{
						num2 - 4,
						num2 - 2,
						num2 - 1,
						num2 - 4,
						num2 - 1,
						num2 - 3
					});
				}
			}
			if (closeLine == ClosePathRule.ALWAYS)
			{
				Vector2 vector2 = positions[positions.Length - 1];
				Vector2 vector = positions[0];
				Vector2 vector4 = positions[1];
				Vector2 vector3 = SVGMath.RotateVectorClockwise((vector2 - vector).normalized);
				Vector2 vector5 = SVGMath.RotateVectorClockwise(sVGLineData.GetNormal(0));
				Vector2 line1Start2 = vector2 + vector3 * num3 + vector3 * offset;
				Vector2 vector9 = vector + vector3 * num3 + vector3 * offset;
				Vector2 vector10 = vector + vector5 * num3 + vector5 * offset;
				Vector2 line2End2 = vector4 + vector5 * num3 + vector5 * offset;
				if (!SVGMath.LineLineIntersection(out intersection, line1Start2, vector9, vector10, line2End2))
				{
					Vector2 normalized = Vector2.Lerp(vector3, vector5, 0.5f).normalized;
					Vector2 vector8 = normalized * offset;
					Vector2[] array4 = new Vector2[3]
					{
						vector9,
						vector + normalized * (0f - num3) + vector8,
						vector10
					};
					list.AddRange(array4);
					list3.AddRange(new Color32[3] { colorB, colorA, colorB });
					list4.AddRange(new Vector2[3]
					{
						array4[0] - array4[1],
						Vector2.zero,
						array4[2] - array4[1]
					});
					num2 += 3;
					if (num != 0)
					{
						list2.AddRange(new int[9]
						{
							num2 - 3,
							num2 - 2,
							num2 - 5,
							num2 - 5,
							num2 - 4,
							num2 - 3,
							num2 - 1,
							num2 - 2,
							num2 - 3
						});
					}
				}
				else
				{
					Vector2 normalized = Vector2.Lerp(vector3, vector5, 0.5f).normalized;
					Vector2 vector8 = normalized * offset;
					Vector2[] array5 = new Vector2[2]
					{
						vector + normalized * (0f - num3) + vector8,
						intersection
					};
					list.AddRange(array5);
					list3.AddRange(new Color32[2] { colorA, colorB });
					list4.AddRange(new Vector2[2]
					{
						Vector2.zero,
						array5[1] - array5[0]
					});
					num2 += 2;
					if (num != 0)
					{
						list2.AddRange(new int[6]
						{
							num2 - 4,
							num2 - 2,
							num2 - 1,
							num2 - 4,
							num2 - 1,
							num2 - 3
						});
					}
				}
				list[1] = list[list.Count - 1];
				list[0] = list[list.Count - 2];
			}
			for (int i = 0; i < list4.Count; i++)
			{
				list[i] -= list4[i];
				list4[i].Normalize();
				list4[i] = new Vector2(list4[i].x, list4[i].y * -1f);
			}
			svgLayer.vertices = list.ToArray();
			svgLayer.triangles = list2.ToArray();
			svgLayer.colors = list3.ToArray();
			svgLayer.angles = list4.ToArray();
			svgLayer.RecalculateBounds();
			return true;
		}

		public static void ChangeMeshUV1(Mesh mesh, Vector2 uv)
		{
			Vector2[] array = mesh.uv;
			int vertexCount = mesh.vertexCount;
			if (array.Length != vertexCount)
			{
				array = new Vector2[vertexCount];
			}
			for (int i = 0; i < vertexCount; i++)
			{
				array[i].x = uv.x;
				array[i].y = uv.y;
			}
			mesh.uv = array;
		}

		public static void ChangeMeshUV2(Mesh mesh, Vector2 uv)
		{
			int num = mesh.vertices.Length;
			Vector2[] array = new Vector2[num];
			for (int i = 0; i < num; i++)
			{
				array[i].x = uv.x;
				array[i].y = uv.y;
			}
			mesh.uv2 = array;
		}

		public static void ChangeMeshUV3(Mesh mesh, Vector2 uv)
		{
			int num = mesh.vertices.Length;
			Vector2[] array = new Vector2[num];
			for (int i = 0; i < num; i++)
			{
				array[i].x = uv.x;
				array[i].y = uv.y;
			}
			mesh.uv3 = array;
		}

		public static void ChangeMeshColor(Mesh mesh, Color32 color)
		{
			Color32[] array = mesh.colors32;
			int vertexCount = mesh.vertexCount;
			if (array.Length != vertexCount)
			{
				array = new Color32[vertexCount];
			}
			for (int i = 0; i < vertexCount; i++)
			{
				array[i].r = color.r;
				array[i].g = color.g;
				array[i].b = color.b;
				array[i].a = color.a;
			}
			mesh.colors32 = array;
		}

		public static void ChangeMeshColor(Mesh mesh, ColorChannel channel, byte value)
		{
			Color32[] array = mesh.colors32;
			int vertexCount = mesh.vertexCount;
			if (array.Length != vertexCount)
			{
				array = new Color32[vertexCount];
			}
			switch (channel)
			{
			case ColorChannel.RED:
			{
				for (int l = 0; l < vertexCount; l++)
				{
					array[l].r = value;
				}
				break;
			}
			case ColorChannel.GREEN:
			{
				for (int j = 0; j < vertexCount; j++)
				{
					array[j].g = value;
				}
				break;
			}
			case ColorChannel.BLUE:
			{
				for (int k = 0; k < vertexCount; k++)
				{
					array[k].b = value;
				}
				break;
			}
			case ColorChannel.ALPHA:
			{
				for (int i = 0; i < vertexCount; i++)
				{
					array[i].a = value;
				}
				break;
			}
			}
			mesh.colors32 = array;
		}

		public static void ChangeMeshColor(Mesh mesh, ColorChannel channel, float value)
		{
			ChangeMeshColor(mesh, channel, (byte)Mathf.RoundToInt(Mathf.Lerp(0f, 255f, value)));
		}

		public static void ChangeMeshColor(Mesh mesh, Color color)
		{
			ChangeMeshColor(mesh, (Color32)color);
		}

		public static void ChengeMeshPosition(Mesh mesh, Vector3 offset)
		{
			Vector3[] vertices = mesh.vertices;
			int num = vertices.Length;
			for (int i = 0; i < num; i++)
			{
				vertices[i].x += offset.x;
				vertices[i].y += offset.y;
				vertices[i].z += offset.z;
			}
			mesh.vertices = vertices;
		}

		public static void ChangeMeshRotation(Mesh mesh, Quaternion rotation)
		{
			Vector3[] vertices = mesh.vertices;
			int num = vertices.Length;
			for (int i = 0; i < num; i++)
			{
				vertices[i] = rotation * vertices[i];
			}
			mesh.vertices = vertices;
		}

		public static void ChangeMeshScale(Mesh mesh, Vector3 scale)
		{
			if (!(mesh == null) && !(scale == Vector3.one))
			{
				Vector3[] vertices = mesh.vertices;
				int vertexCount = mesh.vertexCount;
				for (int i = 0; i < vertexCount; i++)
				{
					vertices[i].x *= scale.x;
					vertices[i].y *= scale.y;
					vertices[i].z *= scale.z;
				}
				mesh.vertices = vertices;
			}
		}

		public static void ChangeMeshScale(Mesh mesh, float scale)
		{
			if (scale != 1f)
			{
				ChangeMeshScale(mesh, new Vector3(scale, scale, scale));
			}
		}

		public static void AutoWeldVertices(Mesh mesh, float threshold)
		{
			float num = threshold * threshold;
			Vector3[] vertices = mesh.vertices;
			List<int> list = new List<int>();
			int num2 = vertices.Length;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			bool flag = false;
			Vector3 vector = default(Vector3);
			for (num4 = 0; num4 < num2; num4++)
			{
				flag = true;
				for (num5 = 0; num5 < num3; num5++)
				{
					vector.x = vertices[list[num5]].x - vertices[num4].x;
					vector.y = vertices[list[num5]].y - vertices[num4].y;
					vector.z = vertices[list[num5]].z - vertices[num4].z;
					if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z <= num)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					list.Add(num4);
					num3 = list.Count;
				}
			}
			int[] triangles = mesh.triangles;
			for (num4 = 0; num4 < triangles.Length; num4++)
			{
				for (num5 = 0; num5 < list.Count; num5++)
				{
					vector.x = vertices[list[num5]].x - vertices[triangles[num4]].x;
					vector.y = vertices[list[num5]].y - vertices[triangles[num4]].y;
					vector.z = vertices[list[num5]].z - vertices[triangles[num4]].z;
					if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z <= num)
					{
						triangles[num4] = num5;
						break;
					}
				}
			}
			int count = list.Count;
			Vector3[] array = new Vector3[count];
			for (num4 = 0; num4 < count; num4++)
			{
				int num6 = list[num4];
				array[num4] = vertices[num6];
			}
			mesh.triangles = null;
			mesh.vertices = array;
			mesh.triangles = triangles;
		}

		public static GameObject MergeMeshes(GameObject source)
		{
			string name = source.name;
			Transform parent = null;
			if (source.transform.parent != null)
			{
				parent = source.transform.parent.transform;
			}
			source.transform.parent = null;
			GameObject gameObject = UnityEngine.Object.Instantiate(source, source.transform.position, source.transform.rotation);
			source.transform.parent = parent;
			MeshFilter[] componentsInChildren = gameObject.GetComponentsInChildren<MeshFilter>();
			MeshRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<MeshRenderer>();
			Material sharedMaterial = null;
			int num = 0;
			for (num = 0; num < componentsInChildren2.Length; num++)
			{
				if (!(componentsInChildren2[num] == null) && !(componentsInChildren2[num].sharedMaterial == null))
				{
					sharedMaterial = componentsInChildren2[num].sharedMaterial;
					break;
				}
			}
			CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
			for (num = 0; num < componentsInChildren.Length; num++)
			{
				MeshFilter meshFilter = componentsInChildren[num];
				array[num].mesh = meshFilter.sharedMesh;
				array[num].transform = meshFilter.transform.localToWorldMatrix;
			}
			MeshFilter meshFilter2 = gameObject.GetComponent<MeshFilter>();
			if (meshFilter2 == null)
			{
				meshFilter2 = gameObject.AddComponent<MeshFilter>();
			}
			MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
			meshRenderer.sharedMaterial = sharedMaterial;
			meshFilter2.sharedMesh = new Mesh();
			meshFilter2.sharedMesh.CombineMeshes(array);
			meshFilter2.sharedMesh.RecalculateBounds();
			Vector3 center = meshFilter2.sharedMesh.bounds.center;
			int vertexCount = meshFilter2.sharedMesh.vertexCount;
			Vector3[] vertices = meshFilter2.sharedMesh.vertices;
			for (num = 0; num < vertexCount; num++)
			{
				vertices[num] -= center;
			}
			meshFilter2.sharedMesh.vertices = vertices;
			meshFilter2.sharedMesh.RecalculateBounds();
			Transform[] componentsInChildren3 = gameObject.GetComponentsInChildren<Transform>();
			for (num = 0; num < componentsInChildren3.Length; num++)
			{
				if (!(componentsInChildren3[num] == null) && componentsInChildren3[num].gameObject != gameObject)
				{
					UnityEngine.Object.Destroy(componentsInChildren3[num].gameObject);
				}
			}
			gameObject.transform.position = center;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.parent = parent;
			gameObject.name = name;
			return gameObject;
		}

		public static void Fill(Mesh source, Mesh destination)
		{
			if (destination == null || source == null)
			{
				return;
			}
			destination.name = source.name;
			destination.vertices = (Vector3[])source.vertices.Clone();
			destination.triangles = (int[])source.triangles.Clone();
			Color32[] colors = source.colors32;
			if (colors != null && colors.Length != 0)
			{
				destination.colors32 = (Color32[])colors.Clone();
			}
			Vector2[] uv = source.uv;
			if (uv != null && uv.Length != 0)
			{
				destination.uv = (Vector2[])uv.Clone();
			}
			Vector2[] uv2 = source.uv2;
			if (uv2 != null && uv2.Length != 0)
			{
				destination.uv2 = (Vector2[])uv2.Clone();
			}
			Vector2[] uv3 = source.uv3;
			if (uv3 != null && uv3.Length != 0)
			{
				destination.uv3 = (Vector2[])uv3.Clone();
			}
			Vector2[] uv4 = source.uv4;
			if (uv4 != null && uv4.Length != 0)
			{
				destination.uv4 = (Vector2[])uv4.Clone();
			}
			Vector3[] normals = source.normals;
			if (normals != null && normals.Length != 0)
			{
				destination.normals = (Vector3[])normals.Clone();
			}
			Vector4[] tangents = source.tangents;
			if (tangents != null && tangents.Length != 0)
			{
				destination.tangents = (Vector4[])tangents.Clone();
			}
			int num = (destination.subMeshCount = source.subMeshCount);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					destination.SetTriangles(source.GetTriangles(i), i);
				}
			}
			destination.bounds = source.bounds;
		}

		public static Mesh Clone(Mesh mesh)
		{
			if (mesh == null)
			{
				return null;
			}
			Mesh mesh2 = new Mesh();
			mesh2.name = mesh.name;
			Fill(mesh, mesh2);
			return mesh2;
		}

		public static Material CloneMaterial(Material original)
		{
			if (original == null)
			{
				return null;
			}
			Material material = new Material(original.shader);
			material.CopyPropertiesFromMaterial(original);
			return material;
		}

		public static List<Vector3> GetEdgePoints(int[] triangles, Vector3[] positions)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < triangles.Length; i += 3)
			{
				list.Add(triangles[i]);
				list.Add(triangles[i + 1]);
				list.Add(triangles[i + 1]);
				list.Add(triangles[i + 2]);
				list.Add(triangles[i + 2]);
				list.Add(triangles[i]);
			}
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			List<int> list2 = new List<int>();
			List<Vector3> list3 = new List<Vector3>();
			int num = -1;
			while (list3.Count < list.Count)
			{
				if (num < 0)
				{
					for (int j = 0; j < list.Count; j += 2)
					{
						if (!dictionary.ContainsKey(j))
						{
							num = list[j];
							break;
						}
					}
				}
				for (int k = 0; k < list.Count; k += 2)
				{
					if (!dictionary.ContainsKey(k))
					{
						int num2 = k + 1;
						int num3 = -1;
						if (list[k] == num)
						{
							num3 = num2;
						}
						else if (list[num2] == num)
						{
							num3 = k;
						}
						if (num3 >= 0)
						{
							int num4 = list[num3];
							dictionary[k] = true;
							list2.Add(num);
							list2.Add(num4);
							num = num4;
							k = 0;
						}
					}
				}
				List<Vector3> borderPoints = new List<Vector3>();
				list2.ForEach(delegate(int ei)
				{
					borderPoints.Add(positions[ei]);
				});
				if (CalculateWindingOrder(borderPoints) > 0)
				{
					borderPoints.Reverse();
				}
				list3.AddRange(borderPoints);
				list2.Clear();
				num = -1;
			}
			return list3;
		}

		public static int CalculateWindingOrder(IList<Vector3> points)
		{
			double num = CalculateSignedArea(points);
			if (num < 0.0)
			{
				return 1;
			}
			if (num > 0.0)
			{
				return -1;
			}
			return 0;
		}

		public static int CalculateWindingOrder(IList<Vector2> points)
		{
			double num = CalculateSignedArea(points);
			if (num < 0.0)
			{
				return 1;
			}
			if (num > 0.0)
			{
				return -1;
			}
			return 0;
		}

		public static int CalculateWindingOrder(Vector2[] points)
		{
			double num = CalculateSignedArea(points);
			if (num < 0.0)
			{
				return 1;
			}
			if (num > 0.0)
			{
				return -1;
			}
			return 0;
		}

		public static double CalculateSignedArea(IList<Vector3> points)
		{
			double num = 0.0;
			for (int i = 0; i < points.Count; i++)
			{
				int index = (i + 1) % points.Count;
				num += (double)(points[i].x * points[index].y);
				num -= (double)(points[i].y * points[index].x);
			}
			return num / 2.0;
		}

		public static double CalculateSignedArea(Vector3[] points)
		{
			double num = 0.0;
			for (int i = 0; i < points.Length; i++)
			{
				int num2 = (i + 1) % points.Length;
				num += (double)(points[i].x * points[num2].y);
				num -= (double)(points[i].y * points[num2].x);
			}
			return num / 2.0;
		}

		public static double CalculateSignedArea(IList<Vector2> points)
		{
			double num = 0.0;
			for (int i = 0; i < points.Count; i++)
			{
				int index = (i + 1) % points.Count;
				num += (double)(points[i].x * points[index].y);
				num -= (double)(points[i].y * points[index].x);
			}
			return num / 2.0;
		}

		public static double CalculateSignedArea(Vector2[] points)
		{
			double num = 0.0;
			for (int i = 0; i < points.Length; i++)
			{
				int num2 = (i + 1) % points.Length;
				num += (double)(points[i].x * points[num2].y);
				num -= (double)(points[i].y * points[num2].x);
			}
			return num / 2.0;
		}

		public static List<int[]> BuildManifoldPoints(Vector3[] vertices, int[] triangles)
		{
			List<int[]> list = new List<int[]>();
			Edge[] array = BuildManifoldEdges(vertices, triangles);
			int num = array.Length;
			List<int> list2 = new List<int>();
			switch (num)
			{
			case 0:
				return list;
			case 1:
				list.Add(new int[2]
				{
					array[0].vertexIndex[0],
					array[0].vertexIndex[1]
				});
				return list;
			default:
			{
				Edge edge = array[0];
				for (int i = 1; i < num; i++)
				{
					Edge edge2 = array[i];
					if (list2.Count > 0 && edge.vertexIndex[1] != edge2.vertexIndex[0])
					{
						list.Add(list2.ToArray());
						list2 = new List<int>();
					}
					list2.Add(edge2.vertexIndex[0]);
					edge = edge2;
				}
				if (list2.Count > 0)
				{
					list.Add(list2.ToArray());
				}
				return list;
			}
			}
		}

		public static Edge[] BuildManifoldEdges(Vector3[] vertices, int[] triangles)
		{
			Edge[] array = BuildEdges(vertices.Length, triangles);
			List<Edge> list = new List<Edge>();
			Edge[] array2 = array;
			foreach (Edge edge in array2)
			{
				if (edge.faceIndex[0] == edge.faceIndex[1])
				{
					list.Add(edge);
				}
			}
			return list.ToArray();
		}

		public static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
		{
			int num = triangleArray.Length;
			int[] array = new int[vertexCount + num];
			int num2 = triangleArray.Length / 3;
			for (int i = 0; i < vertexCount; i++)
			{
				array[i] = -1;
			}
			Edge[] array2 = new Edge[num];
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				int num4 = triangleArray[j * 3 + 2];
				for (int k = 0; k < 3; k++)
				{
					int num5 = triangleArray[j * 3 + k];
					if (num4 < num5)
					{
						Edge edge = new Edge();
						edge.vertexIndex[0] = num4;
						edge.vertexIndex[1] = num5;
						edge.faceIndex[0] = j;
						edge.faceIndex[1] = j;
						array2[num3] = edge;
						int num6 = array[num4];
						if (num6 == -1)
						{
							array[num4] = num3;
						}
						else
						{
							while (true)
							{
								int num7 = array[vertexCount + num6];
								if (num7 == -1)
								{
									break;
								}
								num6 = num7;
							}
							array[vertexCount + num6] = num3;
						}
						array[vertexCount + num3] = -1;
						num3++;
					}
					num4 = num5;
				}
			}
			for (int l = 0; l < num2; l++)
			{
				int num8 = triangleArray[l * 3 + 2];
				for (int m = 0; m < 3; m++)
				{
					int num9 = triangleArray[l * 3 + m];
					if (num8 > num9)
					{
						bool flag = false;
						for (int num10 = array[num9]; num10 != -1; num10 = array[vertexCount + num10])
						{
							Edge edge2 = array2[num10];
							if (edge2.vertexIndex[1] == num8 && edge2.faceIndex[0] == edge2.faceIndex[1])
							{
								array2[num10].faceIndex[1] = l;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Edge edge3 = new Edge();
							edge3.vertexIndex[0] = num8;
							edge3.vertexIndex[1] = num9;
							edge3.faceIndex[0] = l;
							edge3.faceIndex[1] = l;
							array2[num3] = edge3;
							num3++;
						}
					}
					num8 = num9;
				}
			}
			Edge[] array3 = new Edge[num3];
			for (int n = 0; n < num3; n++)
			{
				array3[n] = array2[n];
			}
			return array3;
		}
	}
}
