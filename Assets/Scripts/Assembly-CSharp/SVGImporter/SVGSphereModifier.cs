using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ISVGRenderer))]
	[AddComponentMenu("Rendering/SVG Modifiers/Sphere Modifier", 22)]
	public class SVGSphereModifier : SVGModifier
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
			if (layers == null)
			{
				return;
			}
			int num = layers.Length;
			if (!useSelection)
			{
				for (int i = 0; i < num; i++)
				{
					if (layers[i].shapes == null)
					{
						continue;
					}
					int num2 = layers[i].shapes.Length;
					for (int j = 0; j < num2; j++)
					{
						int vertexCount = layers[i].shapes[j].vertexCount;
						for (int k = 0; k < vertexCount; k++)
						{
							Vector2 vector2 = vector - layers[i].shapes[j].vertices[k];
							float num3 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
							Vector2 zero = Vector2.zero;
							if (num3 > 0f)
							{
								zero.x = vector2.x / num3;
								zero.y = vector2.y / num3;
							}
							layers[i].shapes[j].vertices[k] += zero * (1f - Mathf.Clamp01(num3 / radius)) * intensity;
						}
					}
				}
				return;
			}
			for (int l = 0; l < num; l++)
			{
				if (layers[l].shapes == null || !layerSelection.Contains(l))
				{
					continue;
				}
				int num4 = layers[l].shapes.Length;
				for (int m = 0; m < num4; m++)
				{
					int vertexCount2 = layers[l].shapes[m].vertexCount;
					for (int n = 0; n < vertexCount2; n++)
					{
						Vector2 vector2 = vector - layers[l].shapes[m].vertices[n];
						float num3 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
						Vector2 zero = Vector2.zero;
						if (num3 > 0f)
						{
							zero.x = vector2.x / num3;
							zero.y = vector2.y / num3;
						}
						layers[l].shapes[m].vertices[n] += zero * (1f - Mathf.Clamp01(num3 / radius)) * intensity;
					}
				}
			}
		}
	}
}
