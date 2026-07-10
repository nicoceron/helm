using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Data
{
	public class QuadTree<T>
	{
		protected internal QuadTreeCell<T> _root;

		protected internal SVGBounds _originalBounds;

		protected internal int _originalMaxCapacity = 1;

		public QuadTreeCell<T> root => _root;

		public QuadTree(SVGBounds bounds)
		{
			_originalBounds = bounds;
			_root = new QuadTreeCell<T>(bounds, null, this, _originalMaxCapacity);
			_root._depth = 0;
		}

		public QuadTree(SVGBounds bounds, int maxCapacity)
		{
			_originalBounds = bounds;
			_originalMaxCapacity = maxCapacity;
			_root = new QuadTreeCell<T>(bounds, null, this, _originalMaxCapacity);
			_root._depth = 0;
		}

		public QuadTreeNode<T> Add(T data, SVGBounds bounds)
		{
			return _root.Add(data, bounds);
		}

		public List<QuadTreeNode<T>> Contains(Vector2 point)
		{
			return _root.Contains(point);
		}

		public List<QuadTreeNode<T>> Contains(SVGBounds bounds)
		{
			return _root.Contains(bounds);
		}

		public List<QuadTreeNode<T>> Intersects(SVGBounds bounds)
		{
			return _root.Intersects(bounds);
		}

		public void Clear()
		{
			_root.Clear();
		}

		public void Reset()
		{
			_root.Clear();
			_root = new QuadTreeCell<T>(_originalBounds, null, this, _originalMaxCapacity);
			_root._depth = 0;
		}
	}
}
