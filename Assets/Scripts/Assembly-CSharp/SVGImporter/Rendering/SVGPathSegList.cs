using System.Collections.Generic;

namespace SVGImporter.Rendering
{
	public class SVGPathSegList
	{
		private List<object> _segList;

		public int Count => _segList.Count;

		public SVGPathSegList(int size)
		{
			_segList = new List<object>(size);
		}

		public void Clear()
		{
			_segList.Clear();
		}

		public SVGPathSeg GetItem(int index)
		{
			if (index < 0 || index >= _segList.Count)
			{
				return null;
			}
			return (SVGPathSeg)_segList[index];
		}

		public SVGPathSeg GetLastItem()
		{
			if (_segList.Count == 0)
			{
				return null;
			}
			return (SVGPathSeg)_segList[_segList.Count - 1];
		}

		public SVGPathSeg AppendItem(SVGPathSeg newItem)
		{
			if (newItem == null)
			{
				return null;
			}
			int count = _segList.Count;
			newItem.SetIndex(count);
			_segList.Add(newItem);
			SetList(newItem);
			return newItem;
		}

		internal SVGPathSeg GetPreviousSegment(int index)
		{
			return GetItem(index - 1);
		}

		private void SetList(SVGPathSeg newItem)
		{
			newItem?.SetList(this);
		}
	}
}
