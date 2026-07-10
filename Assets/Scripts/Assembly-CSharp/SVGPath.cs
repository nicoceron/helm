using System;
using UnityEngine;

[Serializable]
public class SVGPath
{
	public Vector2[] points;

	public Rect bounds;

	public int pointCount
	{
		get
		{
			if (points == null)
			{
				return 0;
			}
			return points.Length;
		}
	}

	public SVGPath()
	{
	}

	public SVGPath(Vector2[] points)
	{
		this.points = points;
		RecalculateBounds();
	}

	public SVGPath(Vector2[] points, Rect bounds)
	{
		this.points = points;
		this.bounds = bounds;
	}

	public void RecalculateBounds()
	{
		if (points == null || points.Length == 0)
		{
			bounds = default(Rect);
			return;
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		int num5 = points.Length;
		for (int i = 0; i < num5; i++)
		{
			if (points[i].x < num)
			{
				num = points[i].x;
			}
			if (points[i].x > num2)
			{
				num2 = points[i].x;
			}
			if (points[i].y < num3)
			{
				num3 = points[i].x;
			}
			if (points[i].y > num4)
			{
				num4 = points[i].x;
			}
		}
		bounds = new Rect(num, num4, num2 - num, num4 - num3);
	}
}
