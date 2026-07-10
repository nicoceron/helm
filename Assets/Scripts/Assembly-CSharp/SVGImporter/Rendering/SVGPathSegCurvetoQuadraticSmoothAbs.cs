using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegCurvetoQuadraticSmoothAbs : SVGPathSegCurvetoQuadratic
	{
		protected Vector2 _controlPoint1 = Vector2.zero;

		public override Vector2 controlPoint1 => _controlPoint1;

		public SVGPathSegCurvetoQuadraticSmoothAbs(float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.CurveTo_Quadratic_Smooth_Abs;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = new Vector2(x, y);
			if (segment is SVGPathSegCurvetoQuadratic sVGPathSegCurvetoQuadratic)
			{
				_controlPoint1 = _previousPoint + (_previousPoint - sVGPathSegCurvetoQuadratic.controlPoint1);
			}
			else
			{
				_controlPoint1 = _previousPoint;
			}
		}
	}
}
