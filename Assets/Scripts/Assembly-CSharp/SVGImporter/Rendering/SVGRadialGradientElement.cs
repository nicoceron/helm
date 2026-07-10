using SVGImporter.Document;
using SVGImporter.Utils;

namespace SVGImporter.Rendering
{
	public class SVGRadialGradientElement : SVGGradientElement
	{
		private SVGLength _cx;

		private SVGLength _cy;

		private SVGLength _r;

		private SVGLength _fx;

		private SVGLength _fy;

		public SVGLength cx => _cx;

		public SVGLength cy => _cy;

		public SVGLength r => _r;

		public SVGLength fx => _fx;

		public SVGLength fy => _fy;

		public SVGRadialGradientElement(SVGParser xmlImp, Node node)
			: base(xmlImp, node)
		{
			string value = _attrList.GetValue("cx");
			_cx = new SVGLength((value == "") ? "50%" : value);
			value = _attrList.GetValue("cy");
			_cy = new SVGLength((value == "") ? "50%" : value);
			value = _attrList.GetValue("r");
			_r = new SVGLength((value == "") ? "50%" : value);
			value = _attrList.GetValue("fx");
			_fx = new SVGLength((value == "") ? "50%" : value);
			value = _attrList.GetValue("fy");
			_fy = new SVGLength((value == "") ? "50%" : value);
		}
	}
}
