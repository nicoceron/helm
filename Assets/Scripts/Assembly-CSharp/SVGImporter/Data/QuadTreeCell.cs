using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Data
{
	public class QuadTreeCell<T>
	{
		private const int DEFAULT_MAX_CAPACITY = 1;

		public int maxCapacity = 1;

		public SVGBounds bounds;

		public QuadTreeCell<T> parent;

		public QuadTreeCell<T> topLeft;

		public QuadTreeCell<T> topRight;

		public QuadTreeCell<T> bottomLeft;

		public QuadTreeCell<T> bottomRight;

		public List<QuadTreeNode<T>> nodes;

		public QuadTree<T> quadTree;

		protected internal int _depth;

		public int depth => _depth;

		public QuadTreeCell<T> root => quadTree._root;

		public bool isCellEmpty
		{
			get
			{
				if ((nodes == null || nodes.Count == 0) && topLeft == null && topRight == null && bottomLeft == null)
				{
					return bottomRight == null;
				}
				return false;
			}
		}

		internal QuadTreeCell<T> FindRoot(QuadTreeCell<T> current)
		{
			if (current.parent != null)
			{
				return FindRoot(current.parent);
			}
			return current;
		}

		public QuadTreeCell(SVGBounds bounds)
		{
			this.bounds = bounds;
			parent = null;
			quadTree = null;
			maxCapacity = 1;
		}

		public QuadTreeCell(SVGBounds bounds, int maxCapacity)
		{
			this.bounds = bounds;
			parent = null;
			quadTree = null;
			this.maxCapacity = maxCapacity;
		}

		public QuadTreeCell(SVGBounds bounds, QuadTreeCell<T> parent, int maxCapacity)
		{
			this.bounds = bounds;
			this.parent = parent;
			quadTree = null;
			this.maxCapacity = maxCapacity;
		}

		public QuadTreeCell(SVGBounds bounds, QuadTreeCell<T> parent, QuadTree<T> quadTree, int maxCapacity)
		{
			this.bounds = bounds;
			this.parent = parent;
			this.quadTree = quadTree;
			this.maxCapacity = maxCapacity;
		}

		public QuadTreeNode<T> Add(T data, SVGBounds bounds)
		{
			return Add(new QuadTreeNode<T>(data, bounds));
		}

		public QuadTreeNode<T> Add(QuadTreeNode<T> node)
		{
			if (nodes == null)
			{
				nodes = new List<QuadTreeNode<T>>();
			}
			if (bounds.Contains(node.bounds))
			{
				bool flag = node.bounds.maxX <= bounds.center.x;
				bool flag2 = node.bounds.minY >= bounds.center.y;
				bool num = node.bounds.minX < bounds.center.x;
				bool flag3 = node.bounds.maxX > bounds.center.x;
				bool flag4 = node.bounds.maxY > bounds.center.y;
				bool flag5 = node.bounds.minY < bounds.center.y;
				if ((num && flag3) || (flag4 && flag5))
				{
					node.cell = this;
					node._depth = _depth;
					nodes.Add(node);
				}
				else if (nodes.Count < maxCapacity)
				{
					node.cell = this;
					node._depth = _depth;
					nodes.Add(node);
				}
				else if (flag2)
				{
					if (flag)
					{
						if (topLeft == null)
						{
							topLeft = new QuadTreeCell<T>(new SVGBounds(bounds.minX, bounds.center.y, bounds.center.x, bounds.maxY), this, quadTree, maxCapacity);
						}
						topLeft._depth = _depth + 1;
						topLeft.Add(node);
					}
					else
					{
						if (topRight == null)
						{
							topRight = new QuadTreeCell<T>(new SVGBounds(bounds.center.x, bounds.center.y, bounds.maxX, bounds.maxY), this, quadTree, maxCapacity);
						}
						topRight._depth = _depth + 1;
						topRight.Add(node);
					}
				}
				else if (flag)
				{
					if (bottomLeft == null)
					{
						bottomLeft = new QuadTreeCell<T>(new SVGBounds(bounds.minX, bounds.minY, bounds.center.x, bounds.center.y), this, quadTree, maxCapacity);
					}
					bottomLeft._depth = _depth + 1;
					bottomLeft.Add(node);
				}
				else
				{
					if (bottomRight == null)
					{
						bottomRight = new QuadTreeCell<T>(new SVGBounds(bounds.center.x, bounds.minY, bounds.maxX, bounds.center.y), this, quadTree, maxCapacity);
					}
					bottomRight._depth = _depth + 1;
					bottomRight.Add(node);
				}
			}
			else
			{
				node.cell = this;
				node._depth = _depth;
				nodes.Add(node);
			}
			return node;
		}

		public List<QuadTreeNode<T>> Contains(Vector2 point)
		{
			if (!bounds.Contains(point))
			{
				return null;
			}
			List<QuadTreeNode<T>> list = null;
			if (nodes != null && nodes.Count > 0)
			{
				list = new List<QuadTreeNode<T>>();
				for (int i = 0; i < nodes.Count; i++)
				{
					if (nodes[i].bounds.Contains(point))
					{
						list.Add(nodes[i]);
					}
				}
				if (list.Count == 0)
				{
					list = null;
				}
			}
			if (topLeft != null)
			{
				List<QuadTreeNode<T>> list2 = topLeft.Contains(point);
				if (list2 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list2);
				}
			}
			if (topRight != null)
			{
				List<QuadTreeNode<T>> list3 = topRight.Contains(point);
				if (list3 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list3);
				}
			}
			if (bottomLeft != null)
			{
				List<QuadTreeNode<T>> list4 = bottomLeft.Contains(point);
				if (list4 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list4);
				}
			}
			if (bottomRight != null)
			{
				List<QuadTreeNode<T>> list5 = bottomRight.Contains(point);
				if (list5 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list5);
				}
			}
			return list;
		}

		public List<QuadTreeNode<T>> Contains(SVGBounds bounds)
		{
			if (!this.bounds.Intersects(bounds))
			{
				return null;
			}
			List<QuadTreeNode<T>> list = null;
			if (nodes != null && nodes.Count > 0)
			{
				list = new List<QuadTreeNode<T>>();
				for (int i = 0; i < nodes.Count; i++)
				{
					if (bounds.Contains(nodes[i].bounds))
					{
						list.Add(nodes[i]);
					}
				}
				if (list.Count == 0)
				{
					list = null;
				}
			}
			if (topLeft != null)
			{
				List<QuadTreeNode<T>> list2 = topLeft.Contains(bounds);
				if (list2 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list2);
				}
			}
			if (topRight != null)
			{
				List<QuadTreeNode<T>> list3 = topRight.Contains(bounds);
				if (list3 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list3);
				}
			}
			if (bottomLeft != null)
			{
				List<QuadTreeNode<T>> list4 = bottomLeft.Contains(bounds);
				if (list4 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list4);
				}
			}
			if (bottomRight != null)
			{
				List<QuadTreeNode<T>> list5 = bottomRight.Contains(bounds);
				if (list5 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list5);
				}
			}
			return list;
		}

		public List<QuadTreeNode<T>> Intersects(SVGBounds bounds)
		{
			if (!this.bounds.Intersects(bounds))
			{
				return null;
			}
			List<QuadTreeNode<T>> list = null;
			if (nodes != null && nodes.Count > 0)
			{
				list = new List<QuadTreeNode<T>>();
				for (int i = 0; i < nodes.Count; i++)
				{
					if (nodes[i].bounds.Intersects(bounds))
					{
						list.Add(nodes[i]);
					}
				}
				if (list.Count == 0)
				{
					list = null;
				}
			}
			if (topLeft != null)
			{
				List<QuadTreeNode<T>> list2 = topLeft.Intersects(bounds);
				if (list2 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list2);
				}
			}
			if (topRight != null)
			{
				List<QuadTreeNode<T>> list3 = topRight.Intersects(bounds);
				if (list3 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list3);
				}
			}
			if (bottomLeft != null)
			{
				List<QuadTreeNode<T>> list4 = bottomLeft.Intersects(bounds);
				if (list4 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list4);
				}
			}
			if (bottomRight != null)
			{
				List<QuadTreeNode<T>> list5 = bottomRight.Intersects(bounds);
				if (list5 != null)
				{
					if (list == null)
					{
						list = new List<QuadTreeNode<T>>();
					}
					list.AddRange(list5);
				}
			}
			return list;
		}

		public List<QuadTreeNode<T>> NearestNeighbour(Vector2 point)
		{
			return null;
		}

		public void Clear()
		{
			if (nodes != null)
			{
				nodes.Clear();
				nodes = null;
			}
			if (topLeft != null)
			{
				topLeft.Clear();
				topLeft = null;
			}
			if (topRight != null)
			{
				topRight.Clear();
				topRight = null;
			}
			if (bottomLeft != null)
			{
				bottomLeft.Clear();
				bottomLeft = null;
			}
			if (bottomRight != null)
			{
				bottomRight.Clear();
				bottomRight = null;
			}
			_depth = 0;
		}

		public void Remove()
		{
			if (parent != null)
			{
				if (parent.topLeft != null && parent.topLeft == this)
				{
					parent.topLeft.Clear();
					parent.topLeft = null;
				}
				else if (parent.topRight != null && parent.topRight == this)
				{
					parent.topRight.Clear();
					parent.topRight = null;
				}
				else if (parent.bottomLeft != null && parent.bottomLeft == this)
				{
					parent.bottomLeft.Clear();
					parent.bottomLeft = null;
				}
				else if (parent.bottomRight != null && parent.bottomRight == this)
				{
					parent.bottomRight.Clear();
					parent.bottomRight = null;
				}
			}
		}

		public void CleanUnusedCells()
		{
			CleanUnusedCells(this);
		}

		public static void CleanUnusedCells(QuadTreeCell<T> cell)
		{
			if (cell != null && cell.isCellEmpty)
			{
				cell.Remove();
				CleanUnusedCells(cell.parent);
			}
		}
	}
}
