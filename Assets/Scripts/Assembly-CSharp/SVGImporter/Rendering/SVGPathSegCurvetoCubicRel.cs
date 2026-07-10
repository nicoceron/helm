using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegCurvetoCubicRel : SVGPathSegCurvetoCubic
	{
		protected Vector2 _controlPoint1 = Vector2.zero;

		protected Vector2 _controlPoint2 = Vector2.zero;

		public override Vector2 controlPoint1 => _controlPoint1;

		public override Vector2 controlPoint2 => _controlPoint2;

		public SVGPathSegCurvetoCubicRel(float x1, float y1, float x2, float y2, float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.CurveTo_Cubic_Rel;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = _previousPoint + new Vector2(x, y);
			_controlPoint1 = _previousPoint + new Vector2(x1, y1);
			_controlPoint2 = _previousPoint + new Vector2(x2, y2);
		}
	}
}
