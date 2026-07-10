using System;

namespace SVGImporter.Document
{
	[Serializable]
	public class DOMException : Exception
	{
		private DOMExceptionType code;

		public DOMExceptionType Code => code;

		protected DOMException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		public DOMException(DOMExceptionType code)
			: this(code, string.Empty)
		{
		}

		public DOMException(DOMExceptionType code, string msg)
			: this(code, msg, null)
		{
		}

		public DOMException(DOMExceptionType code, string msg, Exception innerException)
			: base(msg, innerException)
		{
			this.code = code;
		}
	}
}
