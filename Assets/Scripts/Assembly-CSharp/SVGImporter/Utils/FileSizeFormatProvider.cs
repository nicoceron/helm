using System;

namespace SVGImporter.Utils
{
	public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
	{
		private const string fileSizeFormat = "fs";

		private const decimal OneKiloByte = 1024m;

		private const decimal OneMegaByte = 1048576m;

		private const decimal OneGigaByte = 1073741824m;

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
			{
				return this;
			}
			return null;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (format == null || !format.StartsWith("fs"))
			{
				return defaultFormat(format, arg, formatProvider);
			}
			if (arg is string)
			{
				return defaultFormat(format, arg, formatProvider);
			}
			decimal num;
			try
			{
				num = Convert.ToDecimal(arg);
			}
			catch (InvalidCastException)
			{
				return defaultFormat(format, arg, formatProvider);
			}
			string arg2;
			if (num > 1073741824m)
			{
				num /= 1073741824m;
				arg2 = "GB";
			}
			else if (num > 1048576m)
			{
				num /= 1048576m;
				arg2 = "MB";
			}
			else if (num > 1024m)
			{
				num /= 1024m;
				arg2 = "kB";
			}
			else
			{
				arg2 = " B";
			}
			string text = format.Substring(2);
			if (string.IsNullOrEmpty(text))
			{
				text = "2";
			}
			return string.Format("{0:N" + text + "}{1}", num, arg2);
		}

		private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
		{
			if (arg is IFormattable formattable)
			{
				return formattable.ToString(format, formatProvider);
			}
			return arg.ToString();
		}
	}
}
