using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;

namespace SVGImporter.Rendering
{
	public class SVGTransformList
	{
		private List<SVGTransform> _listTransform;

		public int Count => _listTransform.Count;

		public SVGMatrix totalMatrix
		{
			get
			{
				if (_listTransform.Count == 0)
				{
					return SVGMatrix.identity;
				}
				SVGMatrix result = _listTransform[0].matrix;
				for (int i = 1; i < _listTransform.Count; i++)
				{
					result = result.Multiply(_listTransform[i].matrix);
				}
				return result;
			}
		}

		public SVGTransform this[int index]
		{
			get
			{
				if (index < 0 || index >= _listTransform.Count)
				{
					throw new DOMException(DOMExceptionType.IndexSizeErr);
				}
				return _listTransform[index];
			}
		}

		public SVGTransformList()
		{
			_listTransform = new List<SVGTransform>();
		}

		public SVGTransformList(int capacity)
		{
			_listTransform = new List<SVGTransform>(capacity);
		}

		public SVGTransformList(string listString)
		{
			_listTransform = SVGStringExtractor.ExtractTransformList(listString);
		}

		public void Clear()
		{
			_listTransform.Clear();
		}

		public void AppendItem(SVGTransform newItem)
		{
			_listTransform.Add(newItem);
		}

		public void AppendItemAt(SVGTransform newItem, int index)
		{
			_listTransform.Insert(index, newItem);
		}

		public void AppendItems(SVGTransformList newListItem)
		{
			_listTransform.AddRange(newListItem._listTransform);
		}

		public void AppendItemsAt(SVGTransformList newListItem, int index)
		{
			_listTransform.InsertRange(index, newListItem._listTransform);
		}

		public SVGTransform Consolidate()
		{
			return new SVGTransform(totalMatrix);
		}
	}
}
