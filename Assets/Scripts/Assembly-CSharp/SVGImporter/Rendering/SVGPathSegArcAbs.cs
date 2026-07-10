using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGPathSegArcAbs : SVGPathSeg
	{
		private float _r1;

		private float _r2;

		private float _angle;

		private bool _largeArcFlag;

		private bool _sweepFlag;

		public float r1 => _r1;

		public float r2 => _r2;

		public float angle => _angle;

		public bool largeArcFlag => _largeArcFlag;

		public bool sweepFlag => _sweepFlag;

		public SVGPathSegArcAbs(float r1, float r2, float angle, bool largeArcFlag, bool sweepFlag, float x, float y, SVGPathSeg segment)
		{
			_type = SVGPathSegTypes.Arc_Abs;
			if (segment != null)
			{
				_previousPoint = segment.currentPoint;
			}
			_currentPoint = new Vector2(x, y);
			_r1 = r1;
			_r2 = r2;
			_angle = angle;
			_largeArcFlag = largeArcFlag;
			_sweepFlag = sweepFlag;
		}
	}
}
