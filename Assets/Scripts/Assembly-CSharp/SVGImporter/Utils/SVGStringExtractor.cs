using System;
using System.Collections.Generic;
using SVGImporter.Rendering;

namespace SVGImporter.Utils
{
	public static class SVGStringExtractor
	{
		public static string pathCommands = "ZzMmLlCcSsQqTtAaHhVv";

		private static char[] splitPipe = new char[1] { ')' };

		public static char[] splitSpaceComma = new char[5] { ' ', ',', '\n', '\t', '\r' };

		private static List<int> _break = new List<int>();

		private static char[] splitColonSemicolon = new char[6] { ':', ';', ' ', '\n', '\t', '\r' };

		public static List<SVGTransform> ExtractTransformList(string inputText)
		{
			List<SVGTransform> list = new List<SVGTransform>();
			string[] array = inputText.Split(splitPipe, StringSplitOptions.RemoveEmptyEntries);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				if (string.IsNullOrEmpty(array[i]))
				{
					continue;
				}
				int num2 = array[i].IndexOf('(');
				if (num2 > 0)
				{
					string text = array[i].Substring(0, num2).Trim();
					string strValue = array[i].Substring(num2 + 1).Trim();
					if (!string.IsNullOrEmpty(text))
					{
						list.Add(new SVGTransform(text, strValue));
					}
				}
			}
			return list;
		}

		public static float[] ExtractTransformValueAsPX(string inputText)
		{
			string[] array = ExtractTransformValue(inputText);
			float[] array2 = new float[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = SVGLength.GetPXLength(array[i]);
			}
			return array2;
		}

		public static string[] ExtractTransformValue(string inputText)
		{
			if (inputText.Length > 1)
			{
				for (int i = 1; i < inputText.Length; i++)
				{
					if (inputText[i] == '-' && inputText[i - 1] != 'e')
					{
						inputText = inputText.Insert(i++, " ");
					}
				}
			}
			char[] array = new char[1] { '.' };
			List<string> list = new List<string>(inputText.Split(splitSpaceComma, StringSplitOptions.RemoveEmptyEntries));
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j][0] == array[0])
				{
					list[j] = list[j].Insert(0, "0");
				}
				string[] array2 = list[j].Split(array, StringSplitOptions.RemoveEmptyEntries);
				int num = array2.Length;
				if (num > 2)
				{
					list[j] = array2[0] + "." + array2[1];
					for (int k = 2; k < num; k++)
					{
						list.Insert(++j, "0." + array2[k]);
					}
				}
			}
			return list.ToArray();
		}

		public static void ExtractPathSegList(string inputText, ref List<char> charList, ref List<string> valueList)
		{
			_break.Clear();
			for (int i = 0; i < inputText.Length; i++)
			{
				if (pathCommands.IndexOf(inputText[i]) >= 0)
				{
					_break.Add(i);
				}
			}
			_break.Add(inputText.Length);
			charList.Capacity = _break.Count - 1;
			valueList.Capacity = _break.Count - 1;
			for (int j = 0; j < _break.Count - 1; j++)
			{
				int num = _break[j];
				int num2 = _break[j + 1];
				string item = inputText.Substring(num + 1, num2 - num - 1);
				charList.Add(inputText[num]);
				valueList.Add(item);
			}
		}

		public static string[] ExtractStringArray(string inputText)
		{
			return inputText.Split(splitSpaceComma, StringSplitOptions.RemoveEmptyEntries);
		}

		public static void ExtractStyleValue(string inputText, ref Dictionary<string, string> dic)
		{
			string[] array = inputText.Split(splitColonSemicolon, StringSplitOptions.RemoveEmptyEntries);
			int num = array.Length - 1;
			for (int i = 0; i < num; i += 2)
			{
				dic.Add(array[i], array[i + 1]);
			}
		}

		public static string ExtractUrl(string inputText)
		{
			inputText = inputText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ')
				.Replace(" ", "");
			int num = inputText.IndexOf("url(#");
			int num2 = inputText.IndexOf(")");
			if (num2 < 0)
			{
				num2 = inputText.Length;
			}
			return inputText.Substring(num + 5, num2 - num - 5);
		}
	}
}
