using System;
using UnityEngine;

namespace SVGImporter.Rendering
{
	[Serializable]
	public struct CCGradientColorKey
	{
		public float time;

		public Color32 color;

		public CCGradientColorKey(Color32 color, float time)
		{
			this.time = time;
			this.color = color;
		}

		public override string ToString()
		{
			return $"[CCGradientColorKey: time={time}, color={color}";
		}
	}
}
