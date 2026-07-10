using System.Collections.Generic;
using SVGImporter.ClipperLib;
using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGGeom
	{
		private const int decimalPointInt = 1000;

		private const float decimalPointFloat = 0.001f;

		public static List<IntPoint> ConvertFloatToInt(List<Vector2> polygon)
		{
			int num = polygon.Count;
			if (num > 1 && polygon[0] == polygon[polygon.Count - 1])
			{
				num--;
			}
			List<IntPoint> list = new List<IntPoint>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(new IntPoint((int)(polygon[i].x * 1000f), (int)(polygon[i].y * 1000f)));
			}
			return list;
		}

		public static List<Vector2> ConvertIntToFloat(List<IntPoint> polygonInt)
		{
			int num = polygonInt.Count;
			if (num > 1 && polygonInt[0] == polygonInt[polygonInt.Count - 1])
			{
				num--;
			}
			List<Vector2> list = new List<Vector2>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(new Vector2((float)polygonInt[i].X * 0.001f, (float)polygonInt[i].Y * 0.001f));
			}
			return list;
		}

		public static List<List<Vector2>> SimplifyPolygon(List<Vector2> polygon, PolyFillType polyFillType = PolyFillType.pftNonZero)
		{
			if (polygon == null || polygon.Count == 0)
			{
				return null;
			}
			List<List<IntPoint>> list = Clipper.SimplifyPolygon(ConvertFloatToInt(polygon), polyFillType);
			int count = list.Count;
			List<List<Vector2>> list2 = new List<List<Vector2>>(count);
			for (int i = 0; i < count; i++)
			{
				list2.Add(ConvertIntToFloat(list[i]));
			}
			if (list2 == null || list2.Count == 0)
			{
				return null;
			}
			return list2;
		}

		public static List<List<Vector2>> SimplifyPolygons(List<List<Vector2>> polygon, PolyFillType polyFillType = PolyFillType.pftNonZero)
		{
			if (polygon == null || polygon.Count == 0)
			{
				return null;
			}
			List<List<IntPoint>> list = new List<List<IntPoint>>();
			for (int i = 0; i < polygon.Count; i++)
			{
				list.Add(ConvertFloatToInt(polygon[i]));
			}
			list = Clipper.SimplifyPolygons(list, polyFillType);
			int count = list.Count;
			List<List<Vector2>> list2 = new List<List<Vector2>>(count);
			for (int i = 0; i < count; i++)
			{
				list2.Add(ConvertIntToFloat(list[i]));
			}
			if (list2 == null || list2.Count == 0)
			{
				return null;
			}
			return list2;
		}

		public static List<List<Vector2>> MergePolygon(List<List<Vector2>> polygon)
		{
			if (polygon == null || polygon.Count == 0)
			{
				return null;
			}
			List<List<IntPoint>> list = new List<List<IntPoint>> { ConvertFloatToInt(polygon[0]) };
			for (int i = 1; i < polygon.Count; i++)
			{
				list = MergePolygon(list, ConvertFloatToInt(polygon[i]));
			}
			List<List<Vector2>> list2 = new List<List<Vector2>>();
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(ConvertIntToFloat(list[j]));
			}
			return list2;
		}

		public static List<List<IntPoint>> MergePolygon(List<List<IntPoint>> polygonA, List<IntPoint> polygonB)
		{
			Clipper clipper = new Clipper();
			clipper.AddPaths(polygonA, PolyType.ptSubject, closed: true);
			clipper.AddPath(polygonB, PolyType.ptClip, Closed: true);
			List<List<IntPoint>> list = new List<List<IntPoint>>();
			clipper.Execute(ClipType.ctUnion, list);
			return list;
		}

		public static List<List<Vector2>> ClipPolygon(List<List<Vector2>> polygon, List<List<Vector2>> clipPath)
		{
			if (polygon == null || polygon.Count == 0)
			{
				return null;
			}
			if (clipPath == null || clipPath.Count == 0)
			{
				return polygon;
			}
			List<List<IntPoint>> list = new List<List<IntPoint>>();
			List<List<IntPoint>> list2 = new List<List<IntPoint>>();
			for (int i = 0; i < polygon.Count; i++)
			{
				list.Add(ConvertFloatToInt(polygon[i]));
			}
			for (int i = 0; i < clipPath.Count; i++)
			{
				list2.Add(ConvertFloatToInt(clipPath[i]));
			}
			list = ClipPolygon(list, list2);
			int count = list.Count;
			List<List<Vector2>> list3 = new List<List<Vector2>>(count);
			for (int i = 0; i < count; i++)
			{
				list3.Add(ConvertIntToFloat(list[i]));
			}
			if (list3 == null || list3.Count == 0)
			{
				return null;
			}
			return list3;
		}

		public static List<List<IntPoint>> ClipPolygon(List<IntPoint> polygon, List<IntPoint> clipPath)
		{
			Clipper clipper = new Clipper();
			clipper.AddPath(polygon, PolyType.ptSubject, Closed: true);
			clipper.AddPath(clipPath, PolyType.ptClip, Closed: true);
			List<List<IntPoint>> list = new List<List<IntPoint>>();
			clipper.Execute(ClipType.ctIntersection, list);
			return list;
		}

		public static List<List<IntPoint>> ClipPolygon(List<List<IntPoint>> polygons, List<IntPoint> clipPath)
		{
			Clipper clipper = new Clipper();
			clipper.AddPaths(polygons, PolyType.ptSubject, closed: true);
			clipper.AddPath(clipPath, PolyType.ptClip, Closed: true);
			List<List<IntPoint>> list = new List<List<IntPoint>>();
			clipper.Execute(ClipType.ctIntersection, list);
			return list;
		}

		public static List<List<IntPoint>> ClipPolygon(List<List<IntPoint>> polygons, List<List<IntPoint>> clipPaths)
		{
			Clipper clipper = new Clipper();
			clipper.AddPaths(polygons, PolyType.ptSubject, closed: true);
			clipper.AddPaths(clipPaths, PolyType.ptClip, closed: true);
			List<List<IntPoint>> list = new List<List<IntPoint>>();
			clipper.Execute(ClipType.ctIntersection, list);
			return list;
		}
	}
}
