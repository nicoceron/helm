using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SVGImporter.Utils
{
	public class CSSParser
	{
		private const char elementStartChar = '{';

		private const char elementEndChar = '}';

		private const char elementSplitChar = ',';

		private const char attributeStartChar = ':';

		private const char attributeEndChar = ';';

		public static CSSSelector GetSelector(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return CSSSelector.None;
			}
			if (value[0] == '.')
			{
				return CSSSelector.Class;
			}
			if (value[0] == '#')
			{
				return CSSSelector.Id;
			}
			return CSSSelector.Element;
		}

		public static string CleanCSS(string cssString)
		{
			cssString = Regex.Replace(cssString, "/\\*.+?\\*/", string.Empty, RegexOptions.Singleline);
			cssString = Regex.Replace(cssString, "\\s+", "");
			return cssString;
		}

		public static Dictionary<string, Dictionary<string, string>> Parse(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			string[] array = value.Split('}');
			Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
			for (int i = 0; i < array.Length; i++)
			{
				if (string.IsNullOrEmpty(array[i]))
				{
					continue;
				}
				string[] array2 = array[i].Split(new char[1] { '{' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2 == null || array2.Length != 2)
				{
					continue;
				}
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				string[] array3 = array2[1].Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				int num = array3.Length;
				for (int j = 0; j < num; j++)
				{
					if (string.IsNullOrEmpty(array3[j]))
					{
						continue;
					}
					string[] array4 = array3[j].Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (array4 != null && array4.Length == 2)
					{
						if (dictionary2.ContainsKey(array4[0]))
						{
							dictionary2[array4[0]] = array4[1];
						}
						else
						{
							dictionary2.Add(array4[0], array4[1]);
						}
					}
				}
				if (dictionary2.Count == 0)
				{
					continue;
				}
				string[] array5 = array2[0].Split(',');
				for (int j = 0; j < array5.Length; j++)
				{
					if (!string.IsNullOrEmpty(array5[j]))
					{
						if (dictionary.ContainsKey(array5[j]))
						{
							dictionary[array5[j]] = dictionary2;
						}
						else
						{
							dictionary.Add(array5[j], dictionary2);
						}
					}
				}
			}
			if (dictionary.Count == 0)
			{
				return null;
			}
			return dictionary;
		}
	}
}
