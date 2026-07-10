using UnityEngine;

namespace SVGImporter.Utils
{
	public struct SVGColor
	{
		public SVGColorType colorType;

		public Color color;

		public SVGColor(string colorString)
		{
			if (SVGColorExtractor.IsRGBColor(colorString))
			{
				colorType = SVGColorType.RGB;
				color = SVGColorExtractor.RGBColor(colorString);
			}
			else if (SVGColorExtractor.IsHexColor(colorString))
			{
				colorType = SVGColorType.RGB;
				color = SVGColorExtractor.HexColor(colorString);
			}
			else if (SVGColorExtractor.IsConstName(colorString))
			{
				colorType = SVGColorType.RGB;
				color = SVGColorExtractor.ConstColor(colorString);
			}
			else if (colorString.ToLower() == "current")
			{
				colorType = SVGColorType.Current;
				color = Color.black;
			}
			else if (colorString.ToLower() == "none")
			{
				colorType = SVGColorType.None;
				color = Color.black;
			}
			else
			{
				colorType = SVGColorType.Unknown;
				color = Color.black;
			}
		}
	}
}
