using System;
using System.Collections.Generic;
using SVGImporter;

[Serializable]
public struct BackgroundGroup
{
	public BackgroundStyles style;

	public List<SVGAsset> assets;
}
