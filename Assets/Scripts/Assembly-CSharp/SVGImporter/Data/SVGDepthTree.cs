using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Data
{
	public class SVGDepthTree
	{
		protected QuadTree<int> quadTree;

		public SVGDepthTree(SVGBounds bounds)
		{
			quadTree = new QuadTree<int>(new SVGBounds(bounds.center, bounds.size));
		}

		public SVGDepthTree(Rect bounds)
		{
			quadTree = new QuadTree<int>(new SVGBounds(bounds.center, bounds.size));
		}

		public int[] TestDepthAdd(int node, SVGBounds bounds)
		{
			List<QuadTreeNode<int>> list = quadTree.Intersects(bounds);
			int[] array = null;
			if (list != null && list.Count > 0)
			{
				array = new int[list.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = list[i].data;
				}
			}
			quadTree.Add(node, bounds);
			return array;
		}

		public void Clear()
		{
			quadTree.Clear();
		}
	}
}
