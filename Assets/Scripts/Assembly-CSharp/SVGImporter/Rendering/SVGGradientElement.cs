using System.Collections.Generic;
using SVGImporter.Document;

namespace SVGImporter.Rendering
{
	public class SVGGradientElement
	{
		private SVGGradientUnit _gradientUnits;

		private SVGSpreadMethod _spreadMethod;

		private SVGTransformList _gradientTransform;

		private string _id;

		private SVGParser _xmlImp;

		private List<SVGStopElement> _stopList;

		protected AttributeList _attrList;

		public SVGGradientUnit gradientUnits => _gradientUnits;

		public SVGSpreadMethod spreadMethod => _spreadMethod;

		public string id => _id;

		public List<SVGStopElement> stopList => _stopList;

		public SVGTransformList gradientTransform => _gradientTransform;

		public SVGGradientElement(SVGParser xmlImp, Node node)
		{
			_attrList = node.attributes;
			_xmlImp = xmlImp;
			_stopList = new List<SVGStopElement>();
			_id = _attrList.GetValue("id");
			_gradientUnits = SVGGradientUnit.ObjectBoundingBox;
			if (_attrList.GetValue("gradiantUnits") == "userSpaceOnUse")
			{
				_gradientUnits = SVGGradientUnit.UserSpaceOnUse;
			}
			_gradientTransform = new SVGTransformList(_attrList.GetValue("gradientTransform"));
			_spreadMethod = SVGSpreadMethod.Pad;
			if (_attrList.GetValue("spreadMethod") == "reflect")
			{
				_spreadMethod = SVGSpreadMethod.Reflect;
			}
			else if (_attrList.GetValue("spreadMethod") == "repeat")
			{
				_spreadMethod = SVGSpreadMethod.Repeat;
			}
			if (node is BlockOpenNode)
			{
				GetElementList();
			}
		}

		protected void GetElementList()
		{
			bool flag = false;
			while (!flag && _xmlImp.Next())
			{
				if (_xmlImp.node is BlockCloseNode)
				{
					flag = true;
				}
				else if (_xmlImp.node.name == SVGNodeName.Stop)
				{
					_stopList.Add(new SVGStopElement(_xmlImp.node.attributes));
				}
			}
		}

		public SVGStopElement GetStopElement(int i)
		{
			if (i >= 0 && i < _stopList.Count)
			{
				return _stopList[i];
			}
			return null;
		}
	}
}
