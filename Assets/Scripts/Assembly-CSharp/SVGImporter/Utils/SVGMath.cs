using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGMath
	{
		public static Vector2 RotateVectorClockwise(Vector2 vector)
		{
			return new Vector2(vector.y, 0f - vector.x);
		}

		public static Vector2 RotateVectorAntiClockwise(Vector2 vector)
		{
			return new Vector2(0f - vector.y, vector.x);
		}

		public static int PositiveModulo(int a, int b)
		{
			return (Mathf.Abs(a * b) + a) % b;
		}

		public static Vector3 AddVectorLength(Vector3 vector, float size)
		{
			float num = Vector3.Magnitude(vector);
			num += size;
			return Vector3.Scale(Vector3.Normalize(vector), new Vector3(num, num, num));
		}

		public static Vector3 SetVectorLength(Vector3 vector, float size)
		{
			return Vector3.Normalize(vector) * size;
		}

		public static Quaternion SubtractRotation(Quaternion B, Quaternion A)
		{
			return Quaternion.Inverse(A) * B;
		}

		public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
		{
			linePoint = Vector3.zero;
			lineVec = Vector3.zero;
			lineVec = Vector3.Cross(plane1Normal, plane2Normal);
			Vector3 vector = Vector3.Cross(plane2Normal, lineVec);
			float num = Vector3.Dot(plane1Normal, vector);
			if (Mathf.Abs(num) > 0.006f)
			{
				Vector3 rhs = plane1Position - plane2Position;
				float num2 = Vector3.Dot(plane1Normal, rhs) / num;
				linePoint = plane2Position + num2 * vector;
				return true;
			}
			return false;
		}

		public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
		{
			intersection = Vector3.zero;
			float num = Vector3.Dot(planePoint - linePoint, planeNormal);
			float num2 = Vector3.Dot(lineVec, planeNormal);
			if (num2 != 0f)
			{
				float size = num / num2;
				Vector3 vector = SetVectorLength(lineVec, size);
				intersection = linePoint + vector;
				return true;
			}
			return false;
		}

		public static bool LineLineIntersection(out Vector3 intersection, Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
		{
			intersection = Vector3.zero;
			Vector3 lhs = line2Start - line1Start;
			Vector3 rhs = Vector3.Cross(line1End, line2End);
			Vector3 lhs2 = Vector3.Cross(lhs, line2End);
			float num = Vector3.Dot(lhs, rhs);
			if (num >= 1E-05f || num <= -1E-05f)
			{
				return false;
			}
			float num2 = Vector3.Dot(lhs2, rhs) / rhs.sqrMagnitude;
			if (num2 >= 0f && num2 <= 1f)
			{
				intersection = line1Start + line1End * num2;
				return true;
			}
			return false;
		}

		public static bool LineLineIntersection(out Vector2 intersection, Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
		{
			intersection = Vector2.zero;
			float num = line1End.x - line1Start.x;
			float num2 = line1End.y - line1Start.y;
			float num3 = line2End.x - line2Start.x;
			float num4 = line2End.y - line2Start.y;
			float num5 = ((0f - num2) * (line1Start.x - line2Start.x) + num * (line1Start.y - line2Start.y)) / ((0f - num3) * num2 + num * num4);
			float num6 = (num3 * (line1Start.y - line2Start.y) - num4 * (line1Start.x - line2Start.x)) / ((0f - num3) * num2 + num * num4);
			intersection.x = line1Start.x + num6 * num;
			intersection.y = line1Start.y + num6 * num2;
			if (num5 >= 0f && num5 <= 1f && num6 >= 0f && num6 <= 1f)
			{
				return true;
			}
			return false;
		}

		public static float ClosestDistanceToLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
		{
			Vector2 vector = new Vector2(lineEnd.x - lineStart.x, lineEnd.y - lineStart.y);
			float num = vector.x * vector.x + vector.y * vector.y;
			float num2 = (point.x - lineStart.x) * vector.x + (point.y - lineStart.y) * vector.y;
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
			float num3 = lineStart.x + num2 * vector.x;
			float num4 = lineStart.y + num2 * vector.y;
			float num5 = num3 - point.x;
			float num6 = num4 - point.y;
			return Mathf.Sqrt(num5 * num5 + num6 * num6);
		}

		public static float ClosestDistanceToPolygon(Vector2[] points, Vector2 point)
		{
			int num = points.Length;
			if (num <= 1)
			{
				return 0f;
			}
			if (num == 2)
			{
				return ClosestDistanceToLine(points[0], points[1], point);
			}
			float num2 = float.MaxValue;
			Vector2 lineStart = points[0];
			for (int i = 1; i < num; i++)
			{
				float num3 = ClosestDistanceToLine(lineStart, points[i], point);
				if (num3 < num2)
				{
					num2 = num3;
				}
				lineStart = points[i];
			}
			return num2;
		}

		public static float ClosestPointToPolygon(Vector2[] points, Vector2 point, out Vector2 pointOnLine)
		{
			float pointIndex;
			return ClosestPointToPolygon(points, point, out pointOnLine, out pointIndex);
		}

		public static float ClosestPointToPolygon(Vector2[] points, Vector2 point, out Vector2 pointOnLine, out float pointIndex)
		{
			pointOnLine = Vector2.zero;
			pointIndex = 0f;
			if (points == null)
			{
				return 0f;
			}
			int num = points.Length;
			switch (num)
			{
			case 0:
				return 0f;
			case 1:
				pointOnLine = points[0];
				return 0f;
			case 2:
			{
				float result = ClosestPointToLine(points[0], points[1], point, out pointOnLine);
				pointIndex = Vector2.Distance(points[0], pointOnLine) / Vector2.Distance(points[0], points[1]);
				return result;
			}
			default:
			{
				float num2 = float.MaxValue;
				Vector2 vector = points[0];
				for (int i = 1; i < num; i++)
				{
					Vector2 pointOnLine2;
					float num3 = ClosestPointToLine(vector, points[i], point, out pointOnLine2);
					if (num3 < num2)
					{
						pointOnLine = pointOnLine2;
						num2 = num3;
						pointIndex = (float)(i - 1) + Vector2.Distance(vector, pointOnLine) / Vector2.Distance(vector, points[i]);
					}
					vector = points[i];
				}
				return num2;
			}
			}
		}

		public static float ClosestPointToLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point, out Vector2 pointOnLine)
		{
			Vector2 vector = new Vector2(lineEnd.x - lineStart.x, lineEnd.y - lineStart.y);
			float num = vector.x * vector.x + vector.y * vector.y;
			float num2 = (point.x - lineStart.x) * vector.x + (point.y - lineStart.y) * vector.y;
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
			float num3 = pointOnLine.x - point.x;
			float num4 = pointOnLine.y - point.y;
			return Mathf.Sqrt(num3 * num3 + num4 * num4);
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

		public static Vector3 DeCasteljau(Vector3 start, Vector3 startHandle, Vector3 endHandle, Vector3 end, float progress)
		{
			Vector3 vector = start + progress * (startHandle - start);
			Vector3 vector2 = startHandle + progress * (endHandle - startHandle);
			Vector3 vector3 = endHandle + progress * (end - endHandle);
			Vector3 vector4 = vector + progress * (vector2 - vector);
			Vector3 vector5 = vector2 + progress * (vector3 - vector2);
			return vector4 + progress * (vector5 - vector4);
		}

		public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
		{
			closestPointLine1 = Vector3.zero;
			closestPointLine2 = Vector3.zero;
			float num = Vector3.Dot(line1End, line1End);
			float num2 = Vector3.Dot(line1End, line2End);
			float num3 = Vector3.Dot(line2End, line2End);
			float num4 = num * num3 - num2 * num2;
			if (num4 != 0f)
			{
				Vector3 rhs = line1Start - line2Start;
				float num5 = Vector3.Dot(line1End, rhs);
				float num6 = Vector3.Dot(line2End, rhs);
				float num7 = (num2 * num6 - num5 * num3) / num4;
				float num8 = (num * num6 - num5 * num2) / num4;
				closestPointLine1 = line1Start + line1End * num7;
				closestPointLine2 = line2Start + line2End * num8;
				return true;
			}
			return false;
		}

		public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
		{
			float num = Vector3.Dot(point - linePoint, lineVec);
			return linePoint + lineVec * num;
		}

		public static Vector3 ProjectPointOnLineSegment(Vector3 line1Start, Vector3 line2Start, Vector3 point)
		{
			Vector3 vector = ProjectPointOnLine(line1Start, (line2Start - line1Start).normalized, point);
			return PointOnWhichSideOfLineSegment(line1Start, line2Start, vector) switch
			{
				0 => vector, 
				1 => line1Start, 
				2 => line2Start, 
				_ => Vector3.zero, 
			};
		}

		public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
		{
			float num = SignedDistancePlanePoint(planeNormal, planePoint, point);
			num *= -1f;
			Vector3 vector = SetVectorLength(planeNormal, num);
			return point + vector;
		}

		public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
		{
			return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
		}

		public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
		{
			return Vector3.Dot(planeNormal, point - planePoint);
		}

		public static float SignedDotProduct(Vector3 vectorA, Vector3 vectorB, Vector3 normal)
		{
			return Vector3.Dot(Vector3.Cross(normal, vectorA), vectorB);
		}

		public static float SignedVectorAngle(Vector3 referenceVector, Vector3 otherVector, Vector3 normal)
		{
			Vector3 lhs = Vector3.Cross(normal, referenceVector);
			return Vector3.Angle(referenceVector, otherVector) * Mathf.Sign(Vector3.Dot(lhs, otherVector));
		}

		public static float AngleVectorPlane(Vector3 vector, Vector3 normal)
		{
			float num = (float)Math.Acos(Vector3.Dot(vector, normal));
			return (float)Math.PI / 2f - num;
		}

		public static float DotProductAngle(Vector3 vec1, Vector3 vec2)
		{
			double num = Vector3.Dot(vec1, vec2);
			if (num < -1.0)
			{
				num = -1.0;
			}
			if (num > 1.0)
			{
				num = 1.0;
			}
			return (float)Math.Acos(num);
		}

		public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC)
		{
			planeNormal = Vector3.zero;
			planePoint = Vector3.zero;
			Vector3 vector = pointB - pointA;
			Vector3 vector2 = pointC - pointA;
			planeNormal = Vector3.Normalize(Vector3.Cross(vector, vector2));
			Vector3 vector3 = pointA + vector / 2f;
			Vector3 vector4 = pointA + vector2 / 2f;
			Vector3 line1End = pointC - vector3;
			Vector3 line2End = pointB - vector4;
			ClosestPointsOnTwoLines(out planePoint, out var _, vector3, line1End, vector4, line2End);
		}

		public static Vector3 GetForwardVector(Quaternion q)
		{
			return q * Vector3.forward;
		}

		public static Vector3 GetUpVector(Quaternion q)
		{
			return q * Vector3.up;
		}

		public static Vector3 GetRightVector(Quaternion q)
		{
			return q * Vector3.right;
		}

		public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
		{
			return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
		}

		public static Vector3 PositionFromMatrix(Matrix4x4 m)
		{
			Vector4 column = m.GetColumn(3);
			return new Vector3(column.x, column.y, column.z);
		}

		public static void LookRotationExtended(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 customForward, Vector3 customUp)
		{
			Quaternion quaternion = Quaternion.LookRotation(alignWithVector, alignWithNormal);
			Quaternion rotation = Quaternion.LookRotation(customForward, customUp);
			gameObjectInOut.transform.rotation = quaternion * Quaternion.Inverse(rotation);
		}

		public static void PreciseAlign(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 alignWithPosition, Vector3 triangleForward, Vector3 triangleNormal, Vector3 trianglePosition)
		{
			LookRotationExtended(ref gameObjectInOut, alignWithVector, alignWithNormal, triangleForward, triangleNormal);
			Vector3 vector = gameObjectInOut.transform.TransformPoint(trianglePosition);
			Vector3 translation = alignWithPosition - vector;
			gameObjectInOut.transform.Translate(translation, Space.World);
		}

		private void VectorsToTransform(ref GameObject gameObjectInOut, Vector3 positionVector, Vector3 directionVector, Vector3 normalVector)
		{
			gameObjectInOut.transform.position = positionVector;
			gameObjectInOut.transform.rotation = Quaternion.LookRotation(directionVector, normalVector);
		}

		public static int PointOnWhichSideOfLineSegment(Vector3 line1Start, Vector3 line2Start, Vector3 point)
		{
			Vector3 rhs = line2Start - line1Start;
			Vector3 lhs = point - line1Start;
			if (Vector3.Dot(lhs, rhs) > 0f)
			{
				if (lhs.magnitude <= rhs.magnitude)
				{
					return 0;
				}
				return 2;
			}
			return 1;
		}

		public static float MouseDistanceToLine(Vector3 line1Start, Vector3 line2Start)
		{
			Camera main = Camera.main;
			Vector3 mousePosition = Input.mousePosition;
			Vector3 line1Start2 = main.WorldToScreenPoint(line1Start);
			Vector3 line2Start2 = main.WorldToScreenPoint(line2Start);
			Vector3 vector = ProjectPointOnLineSegment(line1Start2, line2Start2, mousePosition);
			vector = new Vector3(vector.x, vector.y, 0f);
			return (vector - mousePosition).magnitude;
		}

		public static float MouseDistanceToCircle(Vector3 point, float radius)
		{
			Camera main = Camera.main;
			Vector3 mousePosition = Input.mousePosition;
			Vector3 vector = main.WorldToScreenPoint(point);
			vector = new Vector3(vector.x, vector.y, 0f);
			return (vector - mousePosition).magnitude - radius;
		}

		public static bool IsLineInRectangle(Vector3 line1Start, Vector3 line2Start, Vector3 rectA, Vector3 rectB, Vector3 rectC, Vector3 rectD)
		{
			bool flag = false;
			bool num = IsPointInRectangle(line1Start, rectA, rectC, rectB, rectD);
			if (!num)
			{
				flag = IsPointInRectangle(line2Start, rectA, rectC, rectB, rectD);
			}
			if (!num && !flag)
			{
				bool num2 = AreLineSegmentsCrossing(line1Start, line2Start, rectA, rectB);
				bool flag2 = AreLineSegmentsCrossing(line1Start, line2Start, rectB, rectC);
				bool flag3 = AreLineSegmentsCrossing(line1Start, line2Start, rectC, rectD);
				bool flag4 = AreLineSegmentsCrossing(line1Start, line2Start, rectD, rectA);
				if (num2 || flag2 || flag3 || flag4)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public static bool IsPointInRectangle(Vector3 point, Vector3 rectA, Vector3 rectC, Vector3 rectB, Vector3 rectD)
		{
			Vector3 vector = rectC - rectA;
			float size = 0f - vector.magnitude / 2f;
			vector = AddVectorLength(vector, size);
			Vector3 linePoint = rectA + vector;
			Vector3 vector2 = rectB - rectA;
			float num = vector2.magnitude / 2f;
			Vector3 vector3 = rectD - rectA;
			float num2 = vector3.magnitude / 2f;
			float magnitude = (ProjectPointOnLine(linePoint, vector2.normalized, point) - point).magnitude;
			if ((ProjectPointOnLine(linePoint, vector3.normalized, point) - point).magnitude <= num && magnitude <= num2)
			{
				return true;
			}
			return false;
		}

		public static bool AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2)
		{
			Vector3 vector = pointA2 - pointA1;
			Vector3 vector2 = pointB2 - pointB1;
			if (ClosestPointsOnTwoLines(out var closestPointLine, out var closestPointLine2, pointA1, vector.normalized, pointB1, vector2.normalized))
			{
				int num = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointLine);
				int num2 = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointLine2);
				if (num == 0 && num2 == 0)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static Bounds GetWorldBounds(Transform transform, Bounds bounds)
		{
			Bounds result = new Bounds(transform.TransformPoint(bounds.center), Vector3.zero);
			Vector3 vector = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
			Vector3 vector2 = new Vector3(0f - bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
			Vector3 vector3 = new Vector3(bounds.size.x, 0f - bounds.size.y, bounds.size.z) * 0.5f;
			Vector3 vector4 = new Vector3(0f - bounds.size.x, 0f - bounds.size.y, bounds.size.z) * 0.5f;
			Vector3 vector5 = new Vector3(bounds.size.x, bounds.size.y, 0f - bounds.size.z) * 0.5f;
			Vector3 vector6 = new Vector3(0f - bounds.size.x, bounds.size.y, 0f - bounds.size.z) * 0.5f;
			Vector3 vector7 = new Vector3(bounds.size.x, 0f - bounds.size.y, 0f - bounds.size.z) * 0.5f;
			Vector3 vector8 = new Vector3(0f - bounds.size.x, 0f - bounds.size.y, 0f - bounds.size.z) * 0.5f;
			result.Encapsulate(transform.TransformPoint(bounds.center + vector));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector2));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector3));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector4));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector5));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector6));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector7));
			result.Encapsulate(transform.TransformPoint(bounds.center + vector8));
			return result;
		}

		public static bool IsPolygonsIntersecting(Vector2[] a, Vector2[] b)
		{
			Vector2[][] array = new Vector2[2][] { a, b };
			foreach (Vector2[] array2 in array)
			{
				for (int j = 0; j < array2.Length; j++)
				{
					int num = (j + 1) % array2.Length;
					Vector2 vector = array2[j];
					Vector2 vector2 = array2[num];
					Vector2 vector3 = new Vector2(vector2.y - vector.y, vector.x - vector2.x);
					float num2 = float.MaxValue;
					float num3 = float.MinValue;
					Vector2[] array3 = a;
					for (int k = 0; k < array3.Length; k++)
					{
						Vector2 vector4 = array3[k];
						float num4 = vector3.x * vector4.x + vector3.y * vector4.y;
						if (num4 < num2)
						{
							num2 = num4;
						}
						if (num4 > num3)
						{
							num3 = num4;
						}
					}
					float num5 = float.MaxValue;
					float num6 = float.MinValue;
					array3 = b;
					for (int k = 0; k < array3.Length; k++)
					{
						Vector2 vector5 = array3[k];
						float num7 = vector3.x * vector5.x + vector3.y * vector5.y;
						if (num7 < num5)
						{
							num5 = num7;
						}
						if (num7 > num6)
						{
							num6 = num7;
						}
					}
					if (num3 < num5 || num6 < num2)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static bool PolygonContainsPoint(Vector2[] polyPoints, Vector2 point)
		{
			int num = polyPoints.Length;
			int num2 = num - 1;
			bool flag = false;
			int num3 = 0;
			while (num3 < num)
			{
				if (((polyPoints[num3].y <= point.y && point.y < polyPoints[num2].y) || (polyPoints[num2].y <= point.y && point.y < polyPoints[num3].y)) && point.x < (polyPoints[num2].x - polyPoints[num3].x) * (point.y - polyPoints[num3].y) / (polyPoints[num2].y - polyPoints[num3].y) + polyPoints[num3].x)
				{
					flag = !flag;
				}
				num2 = num3++;
			}
			return flag;
		}

		public static bool PolygonContainsPoint(List<Vector2> polyPoints, Vector2 point)
		{
			int count = polyPoints.Count;
			int index = count - 1;
			bool flag = false;
			int num = 0;
			while (num < count)
			{
				if (((polyPoints[num].y <= point.y && point.y < polyPoints[index].y) || (polyPoints[index].y <= point.y && point.y < polyPoints[num].y)) && point.x < (polyPoints[index].x - polyPoints[num].x) * (point.y - polyPoints[num].y) / (polyPoints[index].y - polyPoints[num].y) + polyPoints[num].x)
				{
					flag = !flag;
				}
				index = num++;
			}
			return flag;
		}
	}
}
