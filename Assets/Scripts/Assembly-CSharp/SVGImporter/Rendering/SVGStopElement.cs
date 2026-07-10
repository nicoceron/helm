using System.Globalization;
using SVGImporter.Document;
using SVGImporter.Utils;

namespace SVGImporter.Rendering
{
	public class SVGStopElement
	{
		private float _offset;

		private SVGColor _stopColor;

		public float offset => _offset;

		public SVGColor stopColor => _stopColor;

		public SVGStopElement(AttributeList attrList)
		{
			string text = attrList.GetValue("stop-color");
			string text2 = attrList.GetValue("offset");
			string text3 = attrList.GetValue("stop-opacity");
			string value = attrList.GetValue("style");
			if (value != null)
			{
				string[] array = value.Split(';');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Contains("stop-color"))
					{
						text = array[i].Split(':')[1];
					}
					else if (array[i].Contains("stop-opacity"))
					{
						text3 = array[i].Split(':')[1];
					}
					else if (array[i].Contains("offset"))
					{
						text2 = array[i].Split(':')[1];
					}
				}
			}
			if (text == null)
			{
				text = "black";
			}
			if (text2 == null)
			{
				text2 = "0%";
			}
			_stopColor = new SVGColor(text);
			if (!string.IsNullOrEmpty(text3))
			{
				if (text3.EndsWith("%"))
				{
					_stopColor.color.a = float.Parse(text3.TrimEnd('%'), CultureInfo.InvariantCulture) * 0.01f;
				}
				else
				{
					_stopColor.color.a = float.Parse(text3, CultureInfo.InvariantCulture);
				}
			}
			string text4 = text2.Trim();
			if (text4 != "")
			{
				if (text4.EndsWith("%"))
				{
					_offset = float.Parse(text4.TrimEnd('%'), CultureInfo.InvariantCulture);
				}
				else
				{
					_offset = float.Parse(text4, CultureInfo.InvariantCulture) * 100f;
				}
			}
		}
	}
}
