using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ISVGShape), typeof(ISVGRenderer))]
	[AddComponentMenu("Rendering/SVG Modifiers/UV Modifier", 22)]
	public class SVGUVModifier : SVGModifier
	{
		public enum TransformOrder
		{
			TRS = 0,
			TSR = 1,
			RTS = 2,
			RST = 3,
			STR = 4,
			SRT = 5
		}

		public Vector2 position;

		public float rotation;

		public Vector2 scale = Vector2.one;

		public bool preprocess = true;

		public TransformOrder transformOrder;

		protected override void PrepareForRendering(SVGLayer[] layers, SVGAsset svgAsset, bool force)
		{
			SVGMatrix sVGMatrix = SVGMatrix.identity.Translate(-position);
			SVGMatrix sVGMatrix2 = SVGMatrix.identity.Rotate(rotation);
			SVGMatrix sVGMatrix3 = SVGMatrix.identity.Scale(scale);
			SVGMatrix sVGMatrix4 = SVGMatrix.identity;
			if (preprocess)
			{
				sVGMatrix4 = sVGMatrix4.Translate(Vector2.one * 0.5f).Scale(0.25f, 0.25f);
			}
			switch (transformOrder)
			{
			case TransformOrder.TRS:
				sVGMatrix4 *= sVGMatrix3 * sVGMatrix2 * sVGMatrix;
				break;
			case TransformOrder.TSR:
				sVGMatrix4 *= sVGMatrix2 * sVGMatrix3 * sVGMatrix;
				break;
			case TransformOrder.RTS:
				sVGMatrix4 *= sVGMatrix3 * sVGMatrix * sVGMatrix2;
				break;
			case TransformOrder.RST:
				sVGMatrix4 *= sVGMatrix * sVGMatrix3 * sVGMatrix2;
				break;
			case TransformOrder.STR:
				sVGMatrix4 *= sVGMatrix3 * sVGMatrix * sVGMatrix3;
				break;
			case TransformOrder.SRT:
				sVGMatrix4 *= sVGMatrix * sVGMatrix2 * sVGMatrix3;
				break;
			}
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
							layers[i].shapes[j].fill.fillType = FILL_TYPE.TEXTURE;
							layers[i].shapes[j].fill.transform = sVGMatrix4;
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
				int num3 = layers[l].shapes.Length;
				for (int m = 0; m < num3; m++)
				{
					int vertexCount2 = layers[l].shapes[m].vertexCount;
					for (int n = 0; n < vertexCount2; n++)
					{
						layers[l].shapes[m].fill.fillType = FILL_TYPE.TEXTURE;
						layers[l].shapes[m].fill.transform = sVGMatrix4;
					}
				}
			}
		}
	}
}
