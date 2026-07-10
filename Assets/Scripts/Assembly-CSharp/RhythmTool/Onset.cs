using System;

namespace RhythmTool
{
	[Serializable]
	public struct Onset : IFeature
	{
		public float timestamp;

		public float strength;

		float IFeature.timestamp => timestamp;

		float IFeature.length => 0f;
	}
}
