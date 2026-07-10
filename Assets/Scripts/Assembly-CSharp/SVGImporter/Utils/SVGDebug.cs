using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SVGImporter.Utils
{
	public static class SVGDebug
	{
		public static void DebugArray(ICollection array)
		{
			if (array == null)
			{
				Debug.Log("Array is null!");
				return;
			}
			IEnumerator enumerator = array.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				Debug.Log("i: " + num + ", " + enumerator.Current);
				num++;
			}
		}

		public static void DebugPoint(Vector3 point)
		{
			GameObject gameObject = new GameObject("Debug Points");
			gameObject.transform.position = point;
			gameObject.AddComponent<SVGDebugPoints>();
		}

		public static void DebugPoints(List<List<Vector2>> path)
		{
			GameObject gameObject = new GameObject("Debug Points");
			for (int i = 0; i < path.Count; i++)
			{
				GameObject gameObject2 = new GameObject("Path");
				gameObject2.transform.SetParent(gameObject.transform);
				gameObject2.AddComponent<SVGDebugPoints>();
				for (int j = 0; j < path[i].Count; j++)
				{
					GameObject gameObject3 = new GameObject("Point");
					gameObject3.transform.SetParent(gameObject2.transform);
					Vector3 localPosition = path[i][j];
					localPosition.y *= -1f;
					gameObject3.transform.localPosition = localPosition;
				}
			}
		}

		public static void DebugPoints(List<List<Vector3>> path)
		{
			GameObject gameObject = new GameObject("Debug Points");
			for (int i = 0; i < path.Count; i++)
			{
				GameObject gameObject2 = new GameObject("Path");
				gameObject2.transform.SetParent(gameObject.transform);
				gameObject2.AddComponent<SVGDebugPoints>();
				for (int j = 0; j < path[i].Count; j++)
				{
					GameObject gameObject3 = new GameObject("Point");
					gameObject3.transform.SetParent(gameObject2.transform);
					Vector3 localPosition = path[i][j];
					localPosition.y *= -1f;
					gameObject3.transform.localPosition = localPosition;
				}
			}
		}

		public static void DebugPoints(List<Vector2> path)
		{
			DebugPoints(new List<List<Vector2>> { path });
		}

		public static void DebugPoints(List<Vector3> path)
		{
			DebugPoints(new List<List<Vector3>> { path });
		}

		public static void DebugSegments(StrokeSegment[] segments)
		{
			GameObject gameObject = new GameObject("Debug Segments");
			for (int i = 0; i < segments.Length; i++)
			{
				GameObject gameObject2 = new GameObject("Segment");
				gameObject2.transform.SetParent(gameObject.transform);
				gameObject2.AddComponent<SVGDebugPoints>();
				GameObject gameObject3 = new GameObject("StartPoint");
				gameObject3.transform.SetParent(gameObject2.transform);
				Vector3 localPosition = segments[i].startPoint;
				localPosition.y *= -1f;
				gameObject3.transform.localPosition = localPosition;
				GameObject gameObject4 = new GameObject("EndPoint");
				gameObject4.transform.SetParent(gameObject2.transform);
				Vector3 localPosition2 = segments[i].endPoint;
				localPosition2.y *= -1f;
				gameObject4.transform.localPosition = localPosition2;
			}
		}
	}
}
