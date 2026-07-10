using System;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ISVGRenderer))]
	[AddComponentMenu("Rendering/SVG Modifiers/Twirl Modifier", 22)]
	public class SVGTwirlModifier : SVGModifier
	{
		public Transform center;

		public float radius;

		public float intensity;

		protected override void PrepareForRendering(SVGLayer[] layers, SVGAsset svgAsset, bool force)
		{
			if (center == null)
			{
				return;
			}
			Vector2 vector = center.position;
			vector = base.transform.InverseTransformPoint(vector);
			float num = intensity / ((float)Math.PI * 2f);
			if (layers == null)
			{
				return;
			}
			int num2 = layers.Length;
			Vector2 vector2 = default(Vector2);
			Vector2 vector3 = default(Vector2);
			if (!useSelection)
			{
				for (int i = 0; i < num2; i++)
				{
					if (layers[i].shapes == null)
					{
						continue;
					}
					int num3 = layers[i].shapes.Length;
					for (int j = 0; j < num3; j++)
					{
						int vertexCount = layers[i].shapes[j].vertexCount;
						for (int k = 0; k < vertexCount; k++)
						{
							vector2.x = layers[i].shapes[j].vertices[k].x - vector.x;
							vector2.y = layers[i].shapes[j].vertices[k].y - vector.y;
							float num4 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
							float num5 = Mathf.Atan2(vector2.y, vector2.x);
							num5 += num * (1f - Mathf.Clamp01(num4 / radius));
							vector3.x = Mathf.Cos(num5) * num4 + vector.x;
							vector3.y = Mathf.Sin(num5) * num4 + vector.y;
							layers[i].shapes[j].vertices[k] = vector3;
						}
					}
				}
				return;
			}
			for (int l = 0; l < num2; l++)
			{
				if (layers[l].shapes == null || !layerSelection.Contains(l))
				{
					continue;
				}
				int num6 = layers[l].shapes.Length;
				for (int m = 0; m < num6; m++)
				{
					int vertexCount2 = layers[l].shapes[m].vertexCount;
					for (int n = 0; n < vertexCount2; n++)
					{
						vector2.x = layers[l].shapes[m].vertices[n].x - vector.x;
						vector2.y = layers[l].shapes[m].vertices[n].y - vector.y;
						float num4 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
						float num7 = Mathf.Atan2(vector2.y, vector2.x);
						num7 += num * (1f - Mathf.Clamp01(num4 / radius));
						vector3.x = Mathf.Cos(num7) * num4 + vector.x;
						vector3.y = Mathf.Sin(num7) * num4 + vector.y;
						layers[l].shapes[m].vertices[n] = vector3;
					}
				}
			}
		}
	}
}
