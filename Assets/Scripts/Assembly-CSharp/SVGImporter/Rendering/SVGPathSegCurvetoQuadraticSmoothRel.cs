using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegCurvetoQuadraticSmoothRel : SVGPathSegCurvetoQuadratic
	{
		protected Vector2 _controlPoint1 = Vector2.zero;

		public override Vector2 controlPoint1 => _controlPoint1;

		public SVGPathSegCurvetoQuadraticSmoothRel(float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.CurveTo_Quadratic_Smooth_Rel;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = _previousPoint + new Vector2(x, y);
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
