using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegLinetoAbs : SVGPathSeg
	{
		public SVGPathSegLinetoAbs(float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.LineTo_Abs;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = new Vector2(x, y);
		}
	}
}
