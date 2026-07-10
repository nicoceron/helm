using SVGImporter.Utils;

namespace SVGImporter.Data
{
	public class QuadTreeNode<T>
	{
		public T data;

		public SVGBounds bounds;

		public QuadTreeCell<T> cell;

		protected internal int _depth;

		public QuadTree<T> quadTree => cell.quadTree;

		public int depth => _depth;

		public QuadTreeNode(T data, SVGBounds bounds)
		{
			this.data = data;
			this.bounds = bounds;
		}

		public QuadTreeNode(T data, SVGBounds bounds, QuadTreeCell<T> cell)
		{
			this.data = data;
			this.bounds = bounds;
			this.cell = cell;
		}

		public void Move(SVGBounds bounds)
		{
			if (!this.bounds.Compare(bounds))
			{
				this.bounds.ApplyBounds(bounds);
				if (!cell.bounds.Contains(bounds))
				{
					QuadTreeCell<T> root = cell.root;
					Remove();
					root.Add(this);
				}
			}
		}

		public void Remove()
		{
			cell.nodes.Remove(this);
			if (cell.nodes.Count == 0)
			{
				cell.CleanUnusedCells();
			}
		}
	}
}
