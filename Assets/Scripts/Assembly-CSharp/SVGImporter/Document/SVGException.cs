using System;

namespace SVGImporter.Document
{
	public class SVGException : DOMException
	{
		private SVGExceptionType code;

		public new SVGExceptionType Code => code;

		public SVGException(SVGExceptionType errorCode)
			: this(errorCode, string.Empty, null)
		{
		}

		public SVGException(SVGExceptionType errorCode, string message)
			: this(errorCode, message, null)
		{
		}

		public SVGException(SVGExceptionType errorCode, string message, Exception innerException)
			: base(message, innerException)
		{
			code = errorCode;
		}
	}
}
