using System;

namespace RhythmTool
{
	[Serializable]
	public struct Value : IFeature
	{
		public float timestamp;

		public float length;

		public float value;

		float IFeature.timestamp => timestamp;

		float IFeature.length => length;
	}
}
