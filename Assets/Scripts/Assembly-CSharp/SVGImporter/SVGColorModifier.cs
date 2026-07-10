using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ISVGRenderer))]
	[AddComponentMenu("Rendering/SVG Modifiers/Color Modifier", 22)]
	public class SVGColorModifier : SVGModifier
	{
		public Color color;

		protected override void PrepareForRendering(SVGLayer[] layers, SVGAsset svgAsset, bool force)
		{
			if (layers == null)
			{
				return;
			}
			int num = layers.Length;
			if (!useSelection)
			{
				for (int i = 0; i < num; i++)
				{
					if (layers[i].shapes != null)
					{
						int num2 = layers[i].shapes.Length;
						for (int j = 0; j < num2; j++)
						{
							SVGFill fill = layers[i].shapes[j].fill;
							fill.color *= color;
						}
					}
				}
				return;
			}
			for (int k = 0; k < num; k++)
			{
				if (layers[k].shapes != null && layerSelection.Contains(k))
				{
					int num3 = layers[k].shapes.Length;
					for (int l = 0; l < num3; l++)
					{
						SVGFill fill2 = layers[k].shapes[l].fill;
						fill2.color *= color;
					}
				}
			}
		}
	}
}
