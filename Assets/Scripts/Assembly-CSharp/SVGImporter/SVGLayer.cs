using System;
using UnityEngine;

namespace SVGImporter
{
	[Serializable]
	public struct SVGLayer
	{
		public string name;

		public SVGShape[] shapes;

		public SVGLayer Clone()
		{
			SVGLayer result = this;
			if (shapes != null)
			{
				int num = shapes.Length;
				result.shapes = new SVGShape[num];
				for (int i = 0; i < num; i++)
				{
					result.shapes[i] = shapes[i];
					if (shapes[i].vertices != null)
					{
						result.shapes[i].vertices = shapes[i].vertices.Clone() as Vector2[];
					}
					if (shapes[i].triangles != null)
					{
						result.shapes[i].triangles = shapes[i].triangles.Clone() as int[];
					}
					if (shapes[i].colors != null)
					{
						result.shapes[i].colors = shapes[i].colors.Clone() as Color32[];
					}
					if (shapes[i].angles != null)
					{
						result.shapes[i].angles = shapes[i].angles.Clone() as Vector2[];
					}
					if (shapes[i].fill != null)
					{
						result.shapes[i].fill = shapes[i].fill.Clone();
					}
				}
			}
			return result;
		}
	}
}
