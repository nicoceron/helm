using System;

namespace SVGImporter.Rendering
{
	[Serializable]
	public struct CCGradientAlphaKey
	{
		public float time;

		public float alpha;

		public CCGradientAlphaKey(float alpha, float time)
		{
			this.time = time;
			this.alpha = alpha;
		}

		public override string ToString()
		{
			return $"[CCGradientAlphaKey: time={time}, alpha={alpha}]";
		}
	}
}
