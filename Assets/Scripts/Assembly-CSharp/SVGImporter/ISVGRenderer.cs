using System;
using System.Collections.Generic;

namespace SVGImporter
{
	public interface ISVGRenderer
	{
		Action<SVGLayer[], SVGAsset, bool> OnPrepareForRendering { get; set; }

		SVGAsset vectorGraphics { get; }

		int lastFrameChanged { get; }

		List<ISVGModify> modifiers { get; }

		void UpdateRenderer();

		void AddModifier(ISVGModify modifier);

		void RemoveModifier(ISVGModify modifier);
	}
}
