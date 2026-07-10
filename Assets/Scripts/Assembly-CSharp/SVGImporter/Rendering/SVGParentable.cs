namespace SVGImporter.Rendering
{
	public class SVGParentable : SVGTransformable
	{
		public SVGParentable parent;

		public SVGParentable(SVGTransformList inheritTransformList)
			: base(inheritTransformList)
		{
		}
	}
}
