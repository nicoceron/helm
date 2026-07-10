using System;
using System.Collections.Generic;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGGeomUtils
	{
		private struct Vector2Ext
		{
			private float _delta;

			private Vector2 _point;

			public float t => _delta;

			public Vector2 point => _point;

			public Vector2Ext(Vector2 point, float t)
			{
				_point = point;
				_delta = t;
			}
		}

		private static LiteStack<Vector2Ext> _stack = new LiteStack<Vector2Ext>();

		private static List<Vector2Ext> _limitList = new List<Vector2Ext>();

		public static List<Vector2> RoundedRect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, Vector2 p7, Vector2 p8, float r1, float r2, float angle)
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(p1);
			list.Add(p2);
			list.AddRange(Arc(p2, r1, r2, angle, largeArcFlag: false, sweepFlag: true, p3));
			list.Add(p3);
			list.Add(p4);
			list.AddRange(Arc(p4, r1, r2, angle, largeArcFlag: false, sweepFlag: true, p5));
			list.Add(p5);
			list.Add(p6);
			list.AddRange(Arc(p6, r1, r2, angle, largeArcFlag: false, sweepFlag: true, p7));
			list.Add(p7);
			list.Add(p8);
			list.AddRange(Arc(p8, r1, r2, angle, largeArcFlag: false, sweepFlag: true, p1));
			return list;
		}

		public static List<Vector2> Arc(Vector2 p1, float rx, float ry, float angle, bool largeArcFlag, bool sweepFlag, Vector2 p2)
		{
			List<Vector2> list = new List<Vector2>();
			float f = angle * (float)Math.PI / 180f;
			float num = Mathf.Cos(f);
			float num2 = Mathf.Sin(f);
			float num3 = (p1.x - p2.x) / 2f;
			float num4 = (p1.y - p2.y) / 2f;
			float num5 = num * num3 + num2 * num4;
			float num6 = (0f - num2) * num3 + num * num4;
			double num7 = rx * rx;
			double num8 = ry * ry;
			double num9 = num5 * num5;
			double num10 = num6 * num6;
			double num11 = num9 / num7 + num10 / num8;
			if (num11 > 1.0)
			{
				rx = Mathf.Sqrt((float)num11) * rx;
				ry = Mathf.Sqrt((float)num11) * ry;
				num7 = rx * rx;
				num8 = ry * ry;
			}
			double num12 = (num7 * num8 - num7 * num10 - num8 * num9) / (num7 * num10 + num8 * num9);
			num12 = ((num12 < 0.0) ? 0.0 : num12);
			float num13 = ((largeArcFlag == sweepFlag) ? (0f - Mathf.Sqrt((float)num12)) : Mathf.Sqrt((float)num12));
			float num14 = num13 * (rx * num6 / ry);
			float num15 = num13 * ((0f - ry * num5) / rx);
			float num16 = num * num14 - num2 * num15 + (p1.x + p2.x) / 2f;
			float num17 = num2 * num14 + num * num15 + (p1.y + p2.y) / 2f;
			float num18 = (num5 - num14) / rx;
			float num19 = (num6 - num15) / ry;
			float num20 = (0f - num5 - num14) / rx;
			float num21 = (0f - num6 - num15) / ry;
			float num22 = Mathf.Sqrt(num18 * num18 + num19 * num19);
			float num23 = num18;
			float num24 = ((num19 < 0f) ? (0f - Mathf.Acos(num23 / num22)) : Mathf.Acos(num23 / num22));
			num24 = num24 * 180f / (float)Math.PI;
			num24 %= 360f;
			num22 = Mathf.Sqrt((num18 * num18 + num19 * num19) * (num20 * num20 + num21 * num21));
			num23 = num18 * num20 + num19 * num21;
			float num25 = num23 / num22;
			if (Mathf.Abs(num25) >= 0.99999f && Mathf.Abs(num25) < 1.000009f)
			{
				num25 = ((!(num25 > 0f)) ? (-1f) : 1f);
			}
			float num26 = ((num18 * num21 - num19 * num20 < 0f) ? (0f - Mathf.Acos(num25)) : Mathf.Acos(num25));
			num26 = num26 * 180f / (float)Math.PI;
			if (!sweepFlag && num26 > 0f)
			{
				num26 -= 360f;
			}
			else if (sweepFlag && num26 < 0f)
			{
				num26 += 360f;
			}
			num26 %= 360f;
			int num27 = Mathf.RoundToInt(Mathf.Clamp(100f / SVGGraphics.vpm * Mathf.Abs(num26) / 360f, 2f, 100f));
			float num28 = num26 / (float)num27;
			Vector2 item = new Vector2(0f, 0f);
			for (int i = 0; i <= num27; i++)
			{
				float f2 = (num28 * (float)i + num24) * (float)Math.PI / 180f;
				item.x = num * rx * Mathf.Cos(f2) - num2 * ry * Mathf.Sin(f2) + num16;
				item.y = num2 * rx * Mathf.Cos(f2) + num * ry * Mathf.Sin(f2) + num17;
				list.Add(item);
			}
			return list;
		}

		public static Vector2 TransformPoint(Vector2 point, SVGMatrix matrix)
		{
			point = matrix.Transform(point);
			return point;
		}

		private static float BelongPosition(Vector2 a, Vector2 b, Vector2 c)
		{
			float num = (a.y - c.y) * (b.x - a.x) - (a.x - c.x) * (b.y - a.y);
			float num2 = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
			return num / num2;
		}

		private static int NumberOfLimitForCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			float num = BelongPosition(a, d, b);
			float num2 = BelongPosition(a, d, c);
			if (num * num2 > 0f)
			{
				return 0;
			}
			return 1;
		}

		private static float Distance(Vector2 a, Vector2 b, Vector2 c)
		{
			float num = (a.y - c.y) * (b.x - a.x) - (a.x - c.x) * (b.y - a.y);
			float num2 = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
			return Mathf.Abs(num / num2) * Mathf.Sqrt(num2);
		}

		private static Vector2 EvaluateForCubic(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			Vector2 result = new Vector2(0f, 0f);
			float num = 1f - t;
			float num2 = num * num * num;
			float num3 = 3f * t * num * num;
			float num4 = 3f * t * t * num;
			float num5 = t * t * t;
			result.x = num2 * p1.x + num3 * p2.x + num4 * p3.x + num5 * p4.x;
			result.y = num2 * p1.y + num3 * p2.y + num4 * p3.y + num5 * p4.y;
			return result;
		}

		private static Vector2 EvaluateForQuadratic(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			Vector2 result = new Vector2(0f, 0f);
			float num = 1f - t;
			float num2 = num * num;
			float num3 = 2f * t * num;
			float num4 = t * t;
			result.x = num2 * p1.x + num3 * p2.x + num4 * p3.x;
			result.y = num2 * p1.y + num3 * p2.y + num4 * p3.y;
			return result;
		}

		private static List<Vector2> CubicCurve(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, int numberOfLimit, bool cubic)
		{
			List<Vector2> list = new List<Vector2>();
			float num = 0f;
			float num2 = 1f;
			float num3 = 1f;
			Vector2Ext vector2Ext = new Vector2Ext(cubic ? EvaluateForCubic(num, p1, p2, p3, p4) : EvaluateForQuadratic(num, p1, p2, p3, p4), num);
			Vector2Ext obj = new Vector2Ext(cubic ? EvaluateForCubic(num2, p1, p2, p3, p4) : EvaluateForQuadratic(num2, p1, p2, p3, p4), num2);
			_stack.Clear();
			_stack.Push(obj);
			_limitList.Clear();
			if (_limitList.Capacity < numberOfLimit + 1)
			{
				_limitList.Capacity = numberOfLimit + 1;
			}
			int num4 = 0;
			while (true)
			{
				num4++;
				float num5 = (num + num2) / 2f;
				Vector2Ext obj2 = new Vector2Ext(cubic ? EvaluateForCubic(num5, p1, p2, p3, p4) : EvaluateForQuadratic(num5, p1, p2, p3, p4), num5);
				float num6 = Distance(vector2Ext.point, _stack.Peek().point, obj2.point);
				bool flag = false;
				if (num6 < num3)
				{
					int num7 = 0;
					float num8 = 0f;
					for (num7 = 0; num7 < numberOfLimit; num7++)
					{
						num8 = (num + num5) / 2f;
						Vector2Ext vector2Ext2 = new Vector2Ext(cubic ? EvaluateForCubic(num8, p1, p2, p3, p4) : EvaluateForQuadratic(num8, p1, p2, p3, p4), num8);
						if (_limitList.Count - 1 < num7)
						{
							_limitList.Add(vector2Ext2);
						}
						else
						{
							_limitList[num7] = vector2Ext2;
						}
						if (Distance(vector2Ext.point, obj2.point, vector2Ext2.point) >= num3)
						{
							break;
						}
						num5 = num8;
					}
					if (num7 == numberOfLimit)
					{
						flag = true;
					}
					else
					{
						_stack.Push(obj2);
						for (int i = 0; i <= num7; i++)
						{
							_stack.Push(_limitList[i]);
						}
						num2 = num8;
					}
				}
				if (flag)
				{
					list.Add(vector2Ext.point);
					list.Add(obj2.point);
					vector2Ext = _stack.Pop();
					if (_stack.Count == 0)
					{
						break;
					}
					obj2 = _stack.Peek();
					num = num2;
					num2 = obj2.t;
				}
				else if (num2 > num5)
				{
					_stack.Push(obj2);
					num2 = num5;
				}
			}
			list.Add(vector2Ext.point);
			return list;
		}

		public static List<Vector2> CubicCurve(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			return new List<Vector2>(SVGBezier.AdaptiveCubicCurve(SVGGraphics.vpm, p1, p2, p3, p4));
		}

		public static List<Vector2> QuadraticCurve(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			Vector2 handle = p1 + 2f / 3f * (p2 - p1);
			Vector2 handle2 = p3 + 2f / 3f * (p2 - p3);
			return new List<Vector2>(SVGBezier.AdaptiveCubicCurve(SVGGraphics.vpm, p1, handle, handle2, p3));
		}

		public static bool IsWindingClockWise(List<Vector2> points)
		{
			if (points == null || points.Count < 2)
			{
				return false;
			}
			int count = points.Count;
			Vector2 vector = points[0];
			float num = 0f;
			for (int i = 1; i < count; i++)
			{
				num += (points[i].x - vector.x) * (points[i].y + vector.y);
				vector = points[i];
			}
			return num >= 0f;
		}

		public static bool IsWindingClockWise(Vector2[] points)
		{
			if (points == null || points.Length < 2)
			{
				return false;
			}
			int num = points.Length;
			Vector2 vector = points[0];
			float num2 = 0f;
			for (int i = 1; i < num; i++)
			{
				num2 += (points[i].x - vector.x) * (points[i].y + vector.y);
				vector = points[i];
			}
			return num2 >= 0f;
		}

		public static Vector2[] GetPathNormals(List<Vector2> points)
		{
			if (points == null || points.Count < 2)
			{
				return null;
			}
			Vector2[] array = new Vector2[points.Count];
			int count = points.Count;
			Vector2 vector = points[0];
			Vector2 normalized;
			for (int i = 1; i < count; i++)
			{
				normalized = (points[i] - vector).normalized;
				array[i].x = normalized.y;
				array[i].y = 0f - normalized.x;
				vector = points[i];
			}
			normalized = (points[0] - vector).normalized;
			array[0] = new Vector2(normalized.y, 0f - normalized.x);
			return array;
		}

		public static Vector2[] GetPathNormals(Vector2[] points)
		{
			if (points == null || points.Length < 2)
			{
				return null;
			}
			Vector2[] array = new Vector2[points.Length];
			int num = points.Length;
			Vector2 vector = points[0];
			Vector2 normalized;
			for (int i = 1; i < num; i++)
			{
				normalized = (points[i] - vector).normalized;
				array[i].x = normalized.y;
				array[i].y = 0f - normalized.x;
				vector = points[i];
			}
			normalized = (points[0] - vector).normalized;
			array[0] = new Vector2(normalized.y, 0f - normalized.x);
			return array;
		}

		public static Vector2[] OffsetVerts(Vector2[] aSegment, float scale)
		{
			Vector2[] array = (Vector2[])aSegment.Clone();
			for (int num = aSegment.Length - 1; num >= 0; num--)
			{
				array[num] += GetNormal(aSegment, num, aClosed: false) * scale;
			}
			return array;
		}

		public static Vector2 GetNormal(Vector2[] aSegment, int i, bool aClosed)
		{
			if (aSegment.Length < 2)
			{
				return Vector2.up;
			}
			Vector2 vector = ((aClosed && i == aSegment.Length - 1) ? aSegment[0] : aSegment[i]);
			Vector2 zero = Vector2.zero;
			zero = ((i - 1 >= 0) ? aSegment[i - 1] : ((!aClosed) ? (vector - (aSegment[i + 1] - vector)) : aSegment[aSegment.Length - 2]));
			Vector2 zero2 = Vector2.zero;
			zero2 = ((i + 1 <= aSegment.Length - 1) ? aSegment[i + 1] : ((!aClosed) ? (vector - (aSegment[i - 1] - vector)) : aSegment[1]));
			zero -= vector;
			zero2 -= vector;
			zero.Normalize();
			zero2.Normalize();
			zero = new Vector2(0f - zero.y, zero.x);
			zero2 = new Vector2(zero2.y, 0f - zero2.x);
			Vector2 result = (zero + zero2) / 2f;
			result.Normalize();
			return result;
		}
	}
}
