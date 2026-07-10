using System.Collections.Generic;

namespace SVGImporter.Document
{
	public class Node
	{
		public Node parent;

		public List<Node> children;

		public SVGNodeName name;

		public AttributeList attributes;

		public int depth;

		public string content;

		public Node(SVGNodeName name, AttributeList attributes, int depth)
		{
			parent = null;
			children = new List<Node>();
			this.name = name;
			this.attributes = attributes;
			this.depth = depth;
		}

		public List<Node> GetNodes()
		{
			List<Node> list = new List<Node>();
			GetNodesInternal(this, list);
			return list;
		}

		protected void GetNodesInternal(Node node, List<Node> nodes)
		{
			if (node != null)
			{
				nodes.Add(node);
				int count = node.children.Count;
				for (int i = 0; i < count; i++)
				{
					GetNodesInternal(node.children[i], nodes);
				}
				if (node is BlockOpenNode)
				{
					Node item = new BlockCloseNode(node.name, default(AttributeList), node.depth);
					nodes.Add(item);
				}
			}
		}
	}
}
