using SVGImporter.Rendering;

namespace SVGImporter.Document
{
	public class SVGDocument
	{
		private SVGElement _rootElement;

		private SVGParser parser;

		public SVGElement rootElement => _rootElement;

		public SVGDocument(string originalDocument, SVGGraphics r)
		{
			parser = new SVGParser(originalDocument);
			while (!parser.isEOF && parser.node.name != SVGNodeName.SVG)
			{
				parser.Next();
			}
			_rootElement = new SVGElement(parser, new SVGTransformList(), null, root: true);
		}

		public void Clear()
		{
			_rootElement = null;
			parser = null;
		}
	}
}
