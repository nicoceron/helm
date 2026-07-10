using System.Collections.Generic;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGClipPath
	{
		public List<Vector2> path;

		public List<List<Vector2>> holes;

		public SVGClipPath()
		{
		}

		public SVGClipPath(List<Vector2> path)
		{
			this.path = path;
		}

		public SVGClipPath(List<Vector2> path, List<List<Vector2>> holes)
		{
			this.path = path;
			this.holes = holes;
		}
	}
}
