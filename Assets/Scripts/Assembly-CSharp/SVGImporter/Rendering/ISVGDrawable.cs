namespace SVGImporter.Rendering
{
	public interface ISVGDrawable
	{
		void BeforeRender(SVGTransformList transformList);

		void Render();
	}
}
