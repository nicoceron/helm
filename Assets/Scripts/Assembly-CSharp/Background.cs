using System;
using System.Collections.Generic;
using SVGImporter;

[Serializable]
public class Background
{
	public Backgrounds type;

	public SVGAsset image;

	public List<BackProfile> generated;

	public Background()
	{
	}

	public Background(Backgrounds ty, SVGAsset im, List<BackProfile> alt)
	{
		type = ty;
		image = im;
		if (alt.Count > 0)
		{
			generated = alt;
		}
	}
}
