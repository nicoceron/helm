using System;

namespace SVGImporter.Document
{
	internal sealed class SmallXmlParserException : Exception
	{
		private int line;

		private int column;

		public int Line => line;

		public int Column => column;

		public SmallXmlParserException(string msg, int line, int column)
			: base($"{msg}. At ({line},{column})")
		{
			this.line = line;
			this.column = column;
		}
	}
}
