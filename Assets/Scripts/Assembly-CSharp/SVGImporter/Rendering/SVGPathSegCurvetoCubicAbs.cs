using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegCurvetoCubicAbs : SVGPathSegCurvetoCubic
	{
		protected Vector2 _controlPoint1 = Vector2.zero;

		protected Vector2 _controlPoint2 = Vector2.zero;

		public override Vector2 controlPoint1 => _controlPoint1;

		public override Vector2 controlPoint2 => _controlPoint2;

		public SVGPathSegCurvetoCubicAbs(float x1, float y1, float x2, float y2, float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.CurveTo_Cubic_Abs;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = new Vector2(x, y);
			_controlPoint1 = new Vector2(x1, y1);
			_controlPoint2 = new Vector2(x2, y2);
		}
	}
}
