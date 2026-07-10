using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegCurvetoQuadraticRel : SVGPathSegCurvetoQuadratic
	{
		protected Vector2 _controlPoint1 = Vector2.zero;

		public override Vector2 controlPoint1 => _controlPoint1;

		public SVGPathSegCurvetoQuadraticRel(float x1, float y1, float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.CurveTo_Quadratic_Rel;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = _previousPoint + new Vector2(x, y);
			_controlPoint1 = _previousPoint + new Vector2(x1, y1);
		}
	}
}
