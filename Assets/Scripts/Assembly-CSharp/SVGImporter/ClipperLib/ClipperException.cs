using System;

namespace SVGImporter.ClipperLib
{
	internal class ClipperException : Exception
	{
		public ClipperException(string description)
			: base(description)
		{
		}
	}
}
