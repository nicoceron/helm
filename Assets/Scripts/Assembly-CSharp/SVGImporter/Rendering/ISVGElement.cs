using System.Collections.Generic;
using SVGImporter.Document;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public interface ISVGElement
	{
		AttributeList attrList { get; }

		SVGPaintable paintable { get; }

		SVGMatrix transformMatrix { get; }

		List<List<Vector2>> GetPath();

		List<List<Vector2>> GetClipPath();
	}
}
