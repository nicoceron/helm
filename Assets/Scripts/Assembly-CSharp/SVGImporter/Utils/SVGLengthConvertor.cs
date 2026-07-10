using System.Globalization;

namespace SVGImporter.Utils
{
	public static class SVGLengthConvertor
	{
		public static bool ExtractType(string text, ref float value, ref SVGLengthType lengthType)
		{
			string text2 = "";
			int i;
			for (i = 0; i < text.Length; i++)
			{
				if (('0' <= text[i] && text[i] <= '9') || text[i] == '+' || text[i] == '-' || text[i] == '.' || text[i] == 'e')
				{
					text2 += text[i];
				}
				else if (text[i] != ' ')
				{
					break;
				}
			}
			string text3 = text.Substring(i);
			if (text2 == "")
			{
				return false;
			}
			value = float.Parse(text2, CultureInfo.InvariantCulture);
			switch (text3.ToUpper())
			{
			case "EM":
				lengthType = SVGLengthType.EMs;
				break;
			case "EX":
				lengthType = SVGLengthType.EXs;
				break;
			case "PX":
				lengthType = SVGLengthType.PX;
				break;
			case "CM":
				lengthType = SVGLengthType.CM;
				break;
			case "MM":
				lengthType = SVGLengthType.MM;
				break;
			case "IN":
				lengthType = SVGLengthType.IN;
				break;
			case "PT":
				lengthType = SVGLengthType.PT;
				break;
			case "PC":
				lengthType = SVGLengthType.PC;
				break;
			case "%":
				lengthType = SVGLengthType.Percentage;
				break;
			default:
				lengthType = SVGLengthType.Unknown;
				break;
			}
			return true;
		}

		public static float ConvertToPX(float value, SVGLengthType lengthType)
		{
			return lengthType switch
			{
				SVGLengthType.IN => value * 90f, 
				SVGLengthType.CM => value * 35.43307f, 
				SVGLengthType.MM => value * 3.543307f, 
				SVGLengthType.PT => value * 1.25f, 
				SVGLengthType.PC => value * 15f, 
				_ => value, 
			};
		}
	}
}
