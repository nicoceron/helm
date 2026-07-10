using System;
using UnityEngine;

namespace SVGImporter.Rendering
{
	[Serializable]
	public class SVGFill
	{
		public FILL_TYPE fillType;

		public FILL_BLEND blend;

		public GRADIENT_TYPE gradientType;

		public Color32 color;

		public float opacity;

		public Rect viewport;

		public SVGMatrix transform;

		public string gradientId;

		public CCGradient gradientColors;

		public string gradientHash => gradientColors.hash;

		public Color32 finalColor => new Color32(color.r, color.g, color.b, (byte)Mathf.RoundToInt((float)(int)color.a * opacity));

		public SVGFill()
		{
		}

		public SVGFill(Color32 color)
		{
			this.color = color;
		}

		public SVGFill(Color32 color, FILL_BLEND blend)
		{
			this.color = color;
			this.blend = blend;
		}

		public SVGFill(Color32 color, FILL_BLEND blend, FILL_TYPE fillType)
		{
			this.color = color;
			this.blend = blend;
			this.fillType = fillType;
		}

		public SVGFill(Color32 color, FILL_BLEND blend, FILL_TYPE fillType, GRADIENT_TYPE gradientType)
		{
			this.color = color;
			this.blend = blend;
			this.fillType = fillType;
			this.gradientType = gradientType;
		}

		public SVGFill Clone()
		{
			SVGFill sVGFill = new SVGFill(color, blend, fillType, gradientType);
			sVGFill.gradientId = gradientId;
			sVGFill.transform = transform;
			sVGFill.opacity = opacity;
			sVGFill.viewport = viewport;
			if (gradientColors != null)
			{
				sVGFill.gradientColors = gradientColors.Clone();
			}
			return sVGFill;
		}
	}
}
