using System;
using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGBezier
	{
		private const int maxAdaptiveBezierIteration = 200;

		private static int currentAdaptiveBezierIteration;

		private const float PI = (float)Math.PI;

		private const float PI2 = (float)Math.PI * 2f;

		public static Vector3[] QuadraticBezierCurve(int segments, Vector3 start, Vector3 handle, Vector3 end)
		{
			Vector3[] array = new Vector3[segments];
			float num = (float)segments - 1f;
			for (int i = 0; i < segments; i++)
			{
				float num2 = (float)i / num;
				float num3 = 1f - num2;
				float num4 = num3 * num3;
				float num5 = num3 * num2 * 2f;
				float num6 = num2 * num2;
				array[i].x = num4 * start.x + num5 * handle.x + num6 * end.x;
				array[i].y = num4 * start.y + num5 * handle.y + num6 * end.y;
				array[i].z = num4 * start.z + num5 * handle.z + num6 * end.z;
			}
			return array;
		}

		public static Vector2[] QuadraticBezierCurve(int segments, Vector2 start, Vector2 handle, Vector2 end)
		{
			Vector2[] array = new Vector2[segments];
			float num = (float)segments - 1f;
			for (int i = 0; i < segments; i++)
			{
				float num2 = (float)i / num;
				float num3 = 1f - num2;
				float num4 = num3 * num3;
				float num5 = num3 * num2 * 2f;
				float num6 = num2 * num2;
				array[i].x = num4 * start.x + num5 * handle.x + num6 * end.x;
				array[i].y = num4 * start.y + num5 * handle.y + num6 * end.y;
			}
			return array;
		}

		public static Vector3[] CubicBezierCurve(int segments, Vector3 start, Vector3 handle0, Vector3 handle1, Vector3 end)
		{
			Vector3[] array = new Vector3[segments];
			float num = (float)segments - 1f;
			for (int i = 0; i < segments; i++)
			{
				float num2 = (float)i / num;
				float num3 = num2 * num2;
				float num4 = num3 * num2;
				float num5 = num2 * 3f;
				float num6 = 1f - num2;
				float num7 = num3 * 3f;
				float num8 = num6 * num6;
				float num9 = num8 * num6;
				array[i].x = num9 * start.x + num5 * num8 * handle0.x + num7 * num6 * handle1.x + num4 * end.x;
				array[i].y = num9 * start.y + num5 * num8 * handle0.y + num7 * num6 * handle1.y + num4 * end.y;
				array[i].z = num9 * start.z + num5 * num8 * handle0.z + num7 * num6 * handle1.z + num4 * end.z;
			}
			return array;
		}

		public static float ClosestPointOnCubicBezierCurve(int segments, Vector3 start, Vector3 handle0, Vector3 handle1, Vector3 end, Vector3 point, out Vector3 pointOnLine)
		{
			float num = (float)segments - 1f;
			Vector3 zero = Vector3.zero;
			Vector3 lineStart = Vector3.zero;
			float num2 = float.MaxValue;
			pointOnLine = Vector3.zero;
			for (int i = 0; i < segments; i++)
			{
				float num3 = (float)i / num;
				float num4 = num3 * num3;
				float num5 = num4 * num3;
				float num6 = num3 * 3f;
				float num7 = 1f - num3;
				float num8 = num4 * 3f;
				float num9 = num7 * num7;
				float num10 = num9 * num7;
				zero.x = num10 * start.x + num6 * num9 * handle0.x + num8 * num7 * handle1.x + num5 * end.x;
				zero.y = num10 * start.y + num6 * num9 * handle0.y + num8 * num7 * handle1.y + num5 * end.y;
				zero.z = num10 * start.z + num6 * num9 * handle0.z + num8 * num7 * handle1.z + num5 * end.z;
				if (i != 0)
				{
					Vector3 pointOnLine2;
					float num11 = ClosestPointToLine(lineStart, zero, point, out pointOnLine2);
					if (num11 < num2)
					{
						num2 = num11;
						pointOnLine = pointOnLine2;
					}
				}
				lineStart = zero;
			}
			return num2;
		}

		public static float ClosestPointToLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point, out Vector3 pointOnLine)
		{
			Vector3 vector = new Vector3(lineEnd.x - lineStart.x, lineEnd.y - lineStart.y, lineEnd.z - lineStart.z);
			float num = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
			float num2 = (point.x - lineStart.x) * vector.x + (point.y - lineStart.y) * vector.y + (point.z - lineStart.z) * vector.z;
			if (num != 0f)
			{
				num2 /= num;
			}
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			else if (num2 < 0f)
			{
				num2 = 0f;
			}
			pointOnLine.x = lineStart.x + num2 * vector.x;
			pointOnLine.y = lineStart.y + num2 * vector.y;
			pointOnLine.z = lineStart.z + num2 * vector.z;
			float num3 = pointOnLine.x - point.x;
			float num4 = pointOnLine.y - point.y;
			float num5 = pointOnLine.z - point.z;
			return Mathf.Sqrt(num3 * num3 + num4 * num4 + num5 * num5);
		}

		public static Vector2[] CubicBezierCurve(int segments, Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			Vector2[] array = new Vector2[segments];
			float num = (float)segments - 1f;
			for (int i = 0; i < segments; i++)
			{
				float num2 = (float)i / num;
				float num3 = num2 * num2;
				float num4 = num3 * num2;
				float num5 = num2 * 3f;
				float num6 = 1f - num2;
				float num7 = num3 * 3f;
				float num8 = num6 * num6;
				float num9 = num8 * num6;
				array[i].x = num9 * start.x + num5 * num8 * handle0.x + num7 * num6 * handle1.x + num4 * end.x;
				array[i].y = num9 * start.y + num5 * num8 * handle0.y + num7 * num6 * handle1.y + num4 * end.y;
			}
			return array;
		}

		public static List<Vector2> Optimise(List<Vector2> pointList, float precision, int startPosition = 0, int endPosition = -1)
		{
			precision *= 0.1f;
			return RamerDouglasPeucker(pointList, precision, startPosition, endPosition);
		}

		public static Vector2[] Optimise(Vector2[] pointList, float precision, int startPosition = 0, int endPosition = -1)
		{
			precision *= 0.1f;
			return RamerDouglasPeucker(new List<Vector2>(pointList), precision, startPosition, endPosition).ToArray();
		}

		public static List<int> RamerDouglasPeuckerIndex(Vector2[] pointArray, float precision, int startPosition = 0, int endPosition = -1)
		{
			float num = 0f;
			int num2 = -1;
			List<int> list = new List<int>();
			if (endPosition < 0)
			{
				endPosition = pointArray.Length - 1;
			}
			for (int i = startPosition; i < endPosition; i++)
			{
				float num3 = PerpendicularDistance(pointArray[i], pointArray[startPosition], pointArray[endPosition]);
				if (num3 > num)
				{
					num2 = i;
					num = num3;
				}
			}
			if (num >= precision)
			{
				List<int> list2 = RamerDouglasPeuckerIndex(pointArray, precision, num2, endPosition);
				list = RamerDouglasPeuckerIndex(pointArray, precision, startPosition, num2);
				list2.RemoveAt(0);
				list.AddRange(list2);
			}
			else
			{
				list.Add(startPosition);
				list.Add(endPosition);
			}
			return list;
		}

		protected static List<Vector2> RamerDouglasPeucker(List<Vector2> pointList, float precision, int startPosition = 0, int endPosition = -1)
		{
			float num = 0f;
			int num2 = -1;
			List<Vector2> list = new List<Vector2>();
			if (endPosition < 0)
			{
				endPosition = pointList.Count - 1;
			}
			for (int i = startPosition; i < endPosition; i++)
			{
				float num3 = PerpendicularDistance(pointList[i], pointList[startPosition], pointList[endPosition]);
				if (num3 > num)
				{
					num2 = i;
					num = num3;
				}
			}
			if (num >= precision)
			{
				List<Vector2> list2 = RamerDouglasPeucker(pointList, precision, num2, endPosition);
				list = RamerDouglasPeucker(pointList, precision, startPosition, num2);
				list2.RemoveAt(0);
				list.AddRange(list2);
			}
			else
			{
				list.Add(pointList[startPosition]);
				list.Add(pointList[endPosition]);
			}
			return list;
		}

		private static float PointDistance(Vector2 point1, Vector2 point2)
		{
			float num = point1.x - point2.x;
			float num2 = point1.y - point2.y;
			return Mathf.Sqrt(num * num + num2 * num2);
		}

		private static float AngularCoefficient(Vector2 point1, Vector2 point2)
		{
			float num = point1.y - point2.y;
			float num2 = point1.x - point2.x;
			if (num2 == 0f)
			{
				num2 = 0.0001f;
			}
			return num / num2;
		}

		private static float YIntercept(Vector2 point, float angularCoef)
		{
			return point.y - angularCoef * point.x;
		}

		private static float PerpendicularDistance(Vector2 point, Vector2 lineStartPoint, Vector2 lineEndPoint)
		{
			float num = 0f;
			float num2 = 0.0001f;
			if (lineStartPoint.x == lineEndPoint.x)
			{
				return Mathf.Abs(point.x - lineStartPoint.x);
			}
			float num3 = AngularCoefficient(lineStartPoint, lineEndPoint);
			float num4 = YIntercept(lineStartPoint, num3);
			num = Mathf.Abs(num3 * point.x - point.y + num4);
			num2 = Mathf.Sqrt(num3 * num3 + 1f);
			return num / num2;
		}

		public static float CurveLength(Vector3[] points)
		{
			float num = 0f;
			if (points == null || points.Length <= 1)
			{
				return 0f;
			}
			Vector3 vector = points[0];
			Vector3 vector2 = default(Vector3);
			for (int i = 1; i < points.Length; i++)
			{
				vector2.x = points[i].x - vector.x;
				vector2.y = points[i].y - vector.y;
				vector2.z = points[i].z - vector.z;
				num += Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z);
				vector.x = points[i].x;
				vector.y = points[i].y;
				vector.z = points[i].z;
			}
			return num;
		}

		public static float CurveLength(Vector2[] points)
		{
			float num = 0f;
			if (points == null || points.Length <= 1)
			{
				return 0f;
			}
			Vector2 vector = points[0];
			Vector2 vector2 = default(Vector2);
			for (int i = 1; i < points.Length; i++)
			{
				vector2.x = points[i].x - vector.x;
				vector2.y = points[i].y - vector.y;
				num += Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
				vector.x = points[i].x;
				vector.y = points[i].y;
			}
			return num;
		}

		public static SVGBounds GetLooseBounds(Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			SVGBounds infiniteInverse = SVGBounds.InfiniteInverse;
			infiniteInverse.Encapsulate(start);
			infiniteInverse.Encapsulate(handle0);
			infiniteInverse.Encapsulate(handle1);
			infiniteInverse.Encapsulate(end);
			return infiniteInverse;
		}

		public static SVGBounds GetLooseBounds(Vector2 start, Vector2 handle, Vector2 end)
		{
			SVGBounds infiniteInverse = SVGBounds.InfiniteInverse;
			infiniteInverse.Encapsulate(start);
			infiniteInverse.Encapsulate(handle);
			infiniteInverse.Encapsulate(end);
			return infiniteInverse;
		}

		public static List<float> GetExtremes(Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			List<float> list = new List<float>();
			for (int i = 0; i < 2; i++)
			{
				float num;
				float num2;
				float num3;
				if (i == 0)
				{
					num = 6f * start.x - 12f * handle0.x + 6f * handle1.x;
					num2 = -3f * start.x + 9f * handle0.x - 9f * handle1.x + 3f * end.x;
					num3 = 3f * handle0.x - 3f * start.x;
				}
				else
				{
					num = 6f * start.y - 12f * handle0.y + 6f * handle1.y;
					num2 = -3f * start.y + 9f * handle0.y - 9f * handle1.y + 3f * end.y;
					num3 = 3f * handle0.y - 3f * start.y;
				}
				if ((double)Mathf.Abs(num2) < 1E-12)
				{
					if (!((double)Mathf.Abs(num) < 1E-12))
					{
						float num4 = (0f - num3) / num;
						if (0f < num4 && num4 < 1f)
						{
							list.Add(num4);
						}
					}
					continue;
				}
				float num5 = num * num - 4f * num3 * num2;
				float num6 = Mathf.Sqrt(num5);
				if (!(num5 < 0f))
				{
					float num7 = (0f - num + num6) / (2f * num2);
					if (0f < num7 && num7 < 1f)
					{
						list.Add(num7);
					}
					float num8 = (0f - num - num6) / (2f * num2);
					if (0f < num8 && num8 < 1f)
					{
						list.Add(num8);
					}
				}
			}
			list.Sort();
			return list;
		}

		public static List<Vector2> AdaptiveCubicCurve(float distanceTolerance, Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			currentAdaptiveBezierIteration = 0;
			if (start == handle0 && handle0 == handle1 && handle1 == end)
			{
				return new List<Vector2>();
			}
			if (distanceTolerance < 0.01f)
			{
				distanceTolerance = 0.01f;
			}
			List<Vector2> obj = new List<Vector2> { start };
			RecursiveBezier(obj, distanceTolerance * distanceTolerance, start.x, start.y, handle0.x, handle0.y, handle1.x, handle1.y, end.x, end.y);
			obj.Add(end);
			return obj;
		}

		private static void RecursiveBezier(List<Vector2> points, float distanceTolerance, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			if (currentAdaptiveBezierIteration++ < 200)
			{
				float num = (x1 + x2) * 0.5f;
				float num2 = (y1 + y2) * 0.5f;
				float num3 = (x2 + x3) * 0.5f;
				float num4 = (y2 + y3) * 0.5f;
				float num5 = (x3 + x4) * 0.5f;
				float num6 = (y3 + y4) * 0.5f;
				float num7 = (num + num3) * 0.5f;
				float num8 = (num2 + num4) * 0.5f;
				float num9 = (num3 + num5) * 0.5f;
				float num10 = (num4 + num6) * 0.5f;
				float num11 = (num7 + num9) * 0.5f;
				float num12 = (num8 + num10) * 0.5f;
				float num13 = x4 - x1;
				float num14 = y4 - y1;
				float num15 = Mathf.Abs((x2 - x4) * num14 - (y2 - y4) * num13);
				float num16 = Mathf.Abs((x3 - x4) * num14 - (y3 - y4) * num13);
				if ((num15 + num16) * (num15 + num16) < distanceTolerance * (num13 * num13 + num14 * num14))
				{
					points.Add(new Vector2(num11, num12));
					return;
				}
				RecursiveBezier(points, distanceTolerance, x1, y1, num, num2, num7, num8, num11, num12);
				RecursiveBezier(points, distanceTolerance, num11, num12, num9, num10, num5, num6, x4, y4);
			}
		}

		public static Vector2[] GetExtremePoints(Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			List<float> extremes = GetExtremes(start, handle0, handle1, end);
			int count = extremes.Count;
			Vector2[] array = new Vector2[count];
			for (int i = 0; i < count; i++)
			{
				float num = extremes[i];
				float num2 = 1f - num;
				float num3 = num * num;
				float num4 = num3 * num;
				float num5 = num2 * num2;
				float num6 = num5 * num2;
				array[i].x = num6 * start.x + 3f * num5 * num * handle0.x + 3f * num2 * num3 * handle1.x + num4 * end.x;
				array[i].y = num6 * start.y + 3f * num5 * num * handle0.y + 3f * num2 * num3 * handle1.y + num4 * end.y;
			}
			return array;
		}

		public static float GetApproxLength(Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			List<float> list = new List<float> { 0f, 1f };
			list.InsertRange(1, GetExtremes(start, handle0, handle1, end));
			int count = list.Count;
			float num = 0f;
			float num2 = list[0];
			float num3 = 1f - num2;
			float num4 = num2 * num2;
			float num5 = num4 * num2;
			float num6 = num3 * num3;
			float num7 = num6 * num3;
			Vector2 vector = default(Vector2);
			vector.x = num7 * start.x + 3f * num6 * num2 * handle0.x + 3f * num3 * num4 * handle1.x + num5 * end.x;
			vector.y = num7 * start.y + 3f * num6 * num2 * handle0.y + 3f * num3 * num4 * handle1.y + num5 * end.y;
			Vector2 a = default(Vector2);
			for (int i = 1; i < count; i++)
			{
				num2 = list[i];
				num3 = 1f - num2;
				num4 = num2 * num2;
				num5 = num4 * num2;
				num6 = num3 * num3;
				num7 = num6 * num3;
				a.x = num7 * start.x + 3f * num6 * num2 * handle0.x + 3f * num3 * num4 * handle1.x + num5 * end.x;
				a.y = num7 * start.y + 3f * num6 * num2 * handle0.y + 3f * num3 * num4 * handle1.y + num5 * end.y;
				num += Vector2.Distance(a, vector);
				a = vector;
			}
			return num;
		}

		public static SVGBounds GetBounds(Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end)
		{
			float num = Mathf.Min(start.x, end.x);
			float num2 = Mathf.Max(start.x, end.x);
			float num3 = Mathf.Min(start.y, end.y);
			float num4 = Mathf.Max(start.y, end.y);
			List<float> extremes = GetExtremes(start, handle0, handle1, end);
			int count = extremes.Count;
			for (int i = 0; i < count; i++)
			{
				float num5 = extremes[i];
				float num6 = 1f - num5;
				float num7 = num5 * num5;
				float num8 = num7 * num5;
				float num9 = num6 * num6;
				float num10 = num9 * num6;
				float num11 = num10 * start.x + 3f * num9 * num5 * handle0.x + 3f * num6 * num7 * handle1.x + num8 * end.x;
				float num12 = num10 * start.y + 3f * num9 * num5 * handle0.y + 3f * num6 * num7 * handle1.y + num8 * end.y;
				if (num11 < num)
				{
					num = num11;
				}
				if (num11 > num2)
				{
					num2 = num11;
				}
				if (num12 < num3)
				{
					num3 = num12;
				}
				if (num12 > num4)
				{
					num4 = num12;
				}
			}
			return new SVGBounds(num, num3, num2, num4);
		}

		public static Vector3[] PathControlPointGenerator(Vector3[] path)
		{
			int num = 2;
			Vector3[] array = new Vector3[path.Length + num];
			Array.Copy(path, 0, array, 1, path.Length);
			array[0] = array[1] + (array[1] - array[2]);
			array[array.Length - 1] = array[array.Length - 2] + (array[array.Length - 2] - array[array.Length - 3]);
			if (array[1] == array[array.Length - 2])
			{
				Vector3[] array2 = new Vector3[array.Length];
				Array.Copy(array, array2, array.Length);
				array2[0] = array2[array2.Length - 3];
				array2[array2.Length - 1] = array2[2];
				array = new Vector3[array2.Length];
				Array.Copy(array2, array, array2.Length);
			}
			return array;
		}

		public static Vector2[] PathControlPointGenerator(Vector2[] path)
		{
			int num = 2;
			Vector2[] array = new Vector2[path.Length + num];
			Array.Copy(path, 0, array, 1, path.Length);
			array[0] = array[1] + (array[1] - array[2]);
			array[array.Length - 1] = array[array.Length - 2] + (array[array.Length - 2] - array[array.Length - 3]);
			if (array[1] == array[array.Length - 2])
			{
				Vector2[] array2 = new Vector2[array.Length];
				Array.Copy(array, array2, array.Length);
				array2[0] = array2[array2.Length - 3];
				array2[array2.Length - 1] = array2[2];
				array = new Vector2[array2.Length];
				Array.Copy(array2, array, array2.Length);
			}
			return array;
		}

		public static Vector3 Interpolate(Vector3[] points, float t)
		{
			float num = points.Length - 3;
			int num2 = (int)Mathf.Min(Mathf.FloorToInt(t * num), num - 1f);
			float num3 = t * num - (float)num2;
			Vector3 vector = points[num2];
			Vector3 vector2 = points[num2 + 1];
			Vector3 vector3 = points[num2 + 2];
			Vector3 vector4 = points[num2 + 3];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num3 * num3) + (-vector + vector3) * num3 + 2f * vector2);
		}

		public static Vector2 Interpolate(Vector2[] points, float t)
		{
			float num = points.Length - 3;
			int num2 = (int)Mathf.Min(Mathf.FloorToInt(t * num), num - 1f);
			float num3 = t * num - (float)num2;
			Vector3 vector = points[num2];
			Vector3 vector2 = points[num2 + 1];
			Vector3 vector3 = points[num2 + 2];
			Vector3 vector4 = points[num2 + 3];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num3 * num3) + (-vector + vector3) * num3 + 2f * vector2);
		}
	}
}
