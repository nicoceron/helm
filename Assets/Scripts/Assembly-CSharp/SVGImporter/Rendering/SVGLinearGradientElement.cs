using SVGImporter.Document;
using SVGImporter.Utils;

namespace SVGImporter.Rendering
{
	public class SVGLinearGradientElement : SVGGradientElement
	{
		private SVGLength _x1;

		private SVGLength _y1;

		private SVGLength _x2;

		private SVGLength _y2;

		public SVGLength x1 => _x1;

		public SVGLength y1 => _y1;

		public SVGLength x2 => _x2;

		public SVGLength y2 => _y2;

		public SVGLinearGradientElement(SVGParser xmlImp, Node node)
			: base(xmlImp, node)
		{
			string value = _attrList.GetValue("x1");
			_x1 = new SVGLength((value == "") ? "0%" : value);
			value = _attrList.GetValue("y1");
			_y1 = new SVGLength((value == "") ? "0%" : value);
			value = _attrList.GetValue("x2");
			_x2 = new SVGLength((value == "") ? "100%" : value);
			value = _attrList.GetValue("y2");
			_y2 = new SVGLength((value == "") ? "0%" : value);
		}
	}
}
