using UnityEngine;

namespace SVGImporter.Rendering
{
	public abstract class SVGPathSeg
	{
		protected SVGPathSegTypes _type;

		protected int _index = -1;

		protected SVGPathSeg _prevSeg;

		protected Vector2 _currentPoint = Vector2.zero;

		protected Vector2 _previousPoint = Vector2.zero;

		protected SVGPathSegList _segList;

		public SVGPathSegTypes type => _type;

		public int index => _index;

		public SVGPathSeg previousSeg => _segList.GetPreviousSegment(_index);

		public Vector2 currentPoint => _currentPoint;

		public Vector2 previousPoint => _previousPoint;

		public int SetIndex(int value)
		{
			_index = value;
			return value;
		}

		public void SetPosition(Vector2 value)
		{
			_currentPoint = value;
		}

		public void SetPreviousSegment(SVGPathSeg prevSeg)
		{
			if (prevSeg != null)
			{
				_prevSeg = prevSeg;
				_previousPoint = prevSeg.currentPoint;
			}
		}

		internal void SetList(SVGPathSegList segList)
		{
			_segList = segList;
		}
	}
}
