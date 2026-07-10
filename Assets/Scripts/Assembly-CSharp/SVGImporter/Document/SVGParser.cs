using System.Collections.Generic;
using System.IO;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter.Document
{
	public class SVGParser : SmallXmlParser.IContentHandler
	{
		public static Dictionary<string, Node> _defs;

		private SmallXmlParser _parser = new SmallXmlParser();

		private int _currentDepth;

		private Node _lastParent;

		private static string STYLE_BLOCK;

		public List<Node> nodes = new List<Node>();

		private int idx;

		private static List<SVGNodeName> dontPutInNodes = new List<SVGNodeName>();

		public Node node => nodes[idx];

		public bool isEOF => idx >= nodes.Count;

		public SVGParser()
		{
		}

		public static void Clear()
		{
			if (_defs != null)
			{
				_defs.Clear();
				_defs = null;
			}
			if (SVGAssetImport.errors != null)
			{
				SVGAssetImport.errors.Clear();
				SVGAssetImport.errors = null;
			}
		}

		public static void Init()
		{
			if (SVGAssetImport.errors == null)
			{
				SVGAssetImport.errors = new List<SVGError>();
			}
			else
			{
				SVGAssetImport.errors.Clear();
			}
			if (_defs == null)
			{
				_defs = new Dictionary<string, Node>();
			}
			else
			{
				_defs.Clear();
			}
		}

		public SVGParser(string text)
		{
			_parser.Parse(new StringReader(text), this);
		}

		public void AddNode(Node node)
		{
			nodes.Add(node);
		}

		public bool Next()
		{
			idx++;
			return !isEOF;
		}

		public void OnStartParsing(SmallXmlParser parser)
		{
			idx = 0;
			_currentDepth = 0;
			_lastParent = null;
			if (dontPutInNodes == null)
			{
				dontPutInNodes = new List<SVGNodeName>();
			}
			else
			{
				dontPutInNodes.Clear();
			}
		}

		private void DontPutInNodesAdd(Node node)
		{
			if (!(node is InlineNode))
			{
				dontPutInNodes.Add(node.name);
			}
		}

		private void DontPutInNodesRemove(Node node)
		{
			if (!(node is InlineNode))
			{
				dontPutInNodes.RemoveAt(dontPutInNodes.Count - 1);
			}
		}

		public void OnNode(Node node)
		{
			string value = node.attributes.GetValue("id");
			if (!string.IsNullOrEmpty(value))
			{
				if (_defs.ContainsKey(value))
				{
					_defs[value] = node;
					Debug.LogWarning("Element: " + node.name.ToString() + ", ID: " + value + " already exists! Overwriting with new element!");
				}
				else
				{
					_defs.Add(value, node);
				}
			}
			switch (node.name)
			{
			case SVGNodeName.LinearGradient:
			case SVGNodeName.RadialGradient:
			case SVGNodeName.ConicalGradient:
			case SVGNodeName.Stop:
				AddNode(node);
				return;
			case SVGNodeName.Defs:
				DontPutInNodesAdd(node);
				return;
			case SVGNodeName.Symbol:
				DontPutInNodesAdd(node);
				return;
			case SVGNodeName.Image:
				DontPutInNodesAdd(node);
				return;
			case SVGNodeName.ClipPath:
				DontPutInNodesAdd(node);
				return;
			case SVGNodeName.Mask:
				DontPutInNodesAdd(node);
				return;
			}
			if (dontPutInNodes.Count == 0)
			{
				AddNode(node);
			}
		}

		public void OnInlineElement(string name, AttributeList attrs)
		{
			Node node = new InlineNode(Lookup(name), new AttributeList(attrs), _currentDepth);
			node.parent = _lastParent;
			if (_lastParent != null)
			{
				_lastParent.children.Add(node);
			}
			OnNode(node);
		}

		public void OnStartElement(string name, AttributeList attrs)
		{
			Node node = new BlockOpenNode(Lookup(name), new AttributeList(attrs), _currentDepth++);
			node.parent = _lastParent;
			if (_lastParent != null)
			{
				_lastParent.children.Add(node);
			}
			_lastParent = node;
			OnNode(node);
		}

		public void OnEndElement(string name)
		{
			Node node = new BlockCloseNode(Lookup(name), default(AttributeList), --_currentDepth);
			if (_lastParent != null)
			{
				_lastParent = _lastParent.parent;
			}
			else
			{
				_lastParent = null;
			}
			node.parent = _lastParent;
			switch (node.name)
			{
			case SVGNodeName.LinearGradient:
			case SVGNodeName.RadialGradient:
			case SVGNodeName.ConicalGradient:
				AddNode(node);
				return;
			case SVGNodeName.Defs:
				DontPutInNodesRemove(node);
				return;
			case SVGNodeName.Symbol:
				DontPutInNodesRemove(node);
				return;
			case SVGNodeName.Image:
				DontPutInNodesRemove(node);
				return;
			case SVGNodeName.ClipPath:
				DontPutInNodesRemove(node);
				return;
			case SVGNodeName.Mask:
				DontPutInNodesRemove(node);
				return;
			}
			if (dontPutInNodes.Count == 0)
			{
				AddNode(node);
			}
		}

		public bool IsInlineElement(Node node)
		{
			SVGNodeName name = node.name;
			if ((uint)name <= 6u || name == SVGNodeName.Stop)
			{
				return true;
			}
			return false;
		}

		public void OnStyleElement(string name, AttributeList attrs, string style)
		{
			Node node = new InlineNode(Lookup(name), new AttributeList(attrs), _currentDepth);
			node.content = style;
			node.parent = _lastParent;
			if (_lastParent != null)
			{
				_lastParent.children.Add(node);
			}
			AddNode(node);
		}

		public void GetElementList(List<object> elementList, SVGPaintable paintable, SVGTransformList summaryTransformList)
		{
			bool flag = false;
			while (!flag && Next())
			{
				if (this.node is BlockCloseNode)
				{
					flag = true;
					continue;
				}
				switch (this.node.name)
				{
				case SVGNodeName.Rect:
					elementList.Add(new SVGRectElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.Line:
					elementList.Add(new SVGLineElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.Circle:
					elementList.Add(new SVGCircleElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.Ellipse:
					elementList.Add(new SVGEllipseElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.PolyLine:
					elementList.Add(new SVGPolylineElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.Polygon:
					elementList.Add(new SVGPolygonElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.Path:
					elementList.Add(new SVGPathElement(this.node, summaryTransformList, paintable));
					break;
				case SVGNodeName.SVG:
					if (!(this.node is InlineNode))
					{
						elementList.Add(new SVGElement(this, summaryTransformList, paintable));
					}
					break;
				case SVGNodeName.Symbol:
					if (!(this.node is InlineNode))
					{
						elementList.Add(new SVGElement(this, summaryTransformList, paintable));
					}
					break;
				case SVGNodeName.G:
					if (!(this.node is InlineNode))
					{
						elementList.Add(new SVGElement(this, summaryTransformList, paintable));
					}
					break;
				case SVGNodeName.LinearGradient:
					ResolveGradientLinks();
					paintable.AppendLinearGradient(new SVGLinearGradientElement(this, this.node));
					break;
				case SVGNodeName.RadialGradient:
					ResolveGradientLinks();
					paintable.AppendRadialGradient(new SVGRadialGradientElement(this, this.node));
					break;
				case SVGNodeName.ConicalGradient:
					ResolveGradientLinks();
					paintable.AppendConicalGradient(new SVGConicalGradientElement(this, this.node));
					break;
				case SVGNodeName.Defs:
					GetElementList(elementList, paintable, summaryTransformList);
					break;
				case SVGNodeName.Title:
					GetElementList(elementList, paintable, summaryTransformList);
					break;
				case SVGNodeName.Desc:
					GetElementList(elementList, paintable, summaryTransformList);
					break;
				case SVGNodeName.Style:
					paintable.AddCSS(this.node.content);
					break;
				case SVGNodeName.Use:
				{
					string text = this.node.attributes.GetValue("xlink:href");
					if (string.IsNullOrEmpty(text))
					{
						break;
					}
					if (text[0] == '#')
					{
						text = text.Remove(0, 1);
					}
					if (!_defs.ContainsKey(text))
					{
						break;
					}
					Node node = _defs[text];
					if (node != null && node != this.node)
					{
						List<Node> list = node.GetNodes();
						if (list != null && list.Count > 0)
						{
							nodes[idx] = new BlockOpenNode(SVGNodeName.Use, this.node.attributes, this.node.depth);
							list.Add(new BlockCloseNode(SVGNodeName.Use, default(AttributeList), this.node.depth));
							nodes.InsertRange(idx + 1, list);
							elementList.Add(new SVGElement(this, summaryTransformList, paintable));
						}
					}
					break;
				}
				}
			}
		}

		protected void ResolveGradientLinks()
		{
			string text = this.node.attributes.GetValue("xlink:href");
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (text[0] == '#')
			{
				text = text.Remove(0, 1);
			}
			if (!_defs.ContainsKey(text))
			{
				return;
			}
			Node node = _defs[text];
			if (node == null || node == this.node)
			{
				return;
			}
			MergeNodeAttributes(node, this.node);
			List<Node> list = node.GetNodes();
			if (list != null && list.Count > 0)
			{
				bool num = nodes[idx] is InlineNode;
				if (num)
				{
					nodes[idx] = new BlockOpenNode(nodes[idx].name, nodes[idx].attributes, nodes[idx].depth);
				}
				list.RemoveAt(0);
				if (list.Count > 0)
				{
					list.RemoveAt(list.Count - 1);
				}
				if (list.Count > 0)
				{
					nodes[idx].children = list;
					nodes.InsertRange(idx + 1, list);
				}
				if (num)
				{
					nodes.Insert(idx + 1 + list.Count, new BlockCloseNode(this.node.name, default(AttributeList), this.node.depth));
				}
			}
		}

		private static void MergeNodeAttributes(Node source, Node target)
		{
			Dictionary<string, string> get = source.attributes.Get;
			Dictionary<string, string> get2 = target.attributes.Get;
			foreach (KeyValuePair<string, string> item in get)
			{
				if (!(item.Key == "id") && !(item.Key == "xlink"))
				{
					if (get2.ContainsKey(item.Key))
					{
						get2[item.Key] = item.Value;
					}
					else
					{
						get2.Add(item.Key, item.Value);
					}
				}
			}
		}

		private static SVGNodeName Lookup(string name)
		{
			SVGNodeName sVGNodeName = SVGNodeName.G;
			return name.ToLower() switch
			{
				"rect" => SVGNodeName.Rect, 
				"line" => SVGNodeName.Line, 
				"circle" => SVGNodeName.Circle, 
				"ellipse" => SVGNodeName.Ellipse, 
				"polyline" => SVGNodeName.PolyLine, 
				"polygon" => SVGNodeName.Polygon, 
				"path" => SVGNodeName.Path, 
				"svg" => SVGNodeName.SVG, 
				"g" => SVGNodeName.G, 
				"lineargradient" => SVGNodeName.LinearGradient, 
				"radialgradient" => SVGNodeName.RadialGradient, 
				"conicalgradient" => SVGNodeName.ConicalGradient, 
				"defs" => SVGNodeName.Defs, 
				"title" => SVGNodeName.Title, 
				"desc" => SVGNodeName.Desc, 
				"stop" => SVGNodeName.Stop, 
				"symbol" => SVGNodeName.Symbol, 
				"clippath" => SVGNodeName.ClipPath, 
				"mask" => SVGNodeName.Mask, 
				"image" => SVGNodeName.Image, 
				"use" => SVGNodeName.Use, 
				"style" => SVGNodeName.Style, 
				_ => SVGNodeName.G, 
			};
		}
	}
}
