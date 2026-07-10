using SVGImporter.Document;

namespace SVGImporter.Rendering
{
	public class SVGClipPathElement
	{
		private string _id;

		private SVGParser _xmlImp;

		protected AttributeList _attrList;

		public string id => _id;

		public SVGClipPathElement(SVGParser xmlImp, Node node)
		{
			_attrList = node.attributes;
			_xmlImp = xmlImp;
			_id = _attrList.GetValue("id");
			GetElementList();
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
			}
		}
	}
}
