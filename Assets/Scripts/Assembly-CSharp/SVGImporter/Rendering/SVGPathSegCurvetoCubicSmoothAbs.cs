using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegCurvetoCubicSmoothAbs : SVGPathSegCurvetoCubic
	{
		protected Vector2 _controlPoint1 = Vector2.zero;

		protected Vector2 _controlPoint2 = Vector2.zero;

		public override Vector2 controlPoint1 => _controlPoint1;

		public override Vector2 controlPoint2 => _controlPoint2;

		public SVGPathSegCurvetoCubicSmoothAbs(float x2, float y2, float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.CurveTo_Cubic_Smooth_Abs;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = new Vector2(x, y);
			if (segment is SVGPathSegCurvetoCubic sVGPathSegCurvetoCubic)
			{
				_controlPoint1 = _previousPoint + (_previousPoint - sVGPathSegCurvetoCubic.controlPoint2);
			}
			else
			{
				_controlPoint1 = _previousPoint;
			}
			_controlPoint2 = new Vector2(x2, y2);
		}
	}
}
