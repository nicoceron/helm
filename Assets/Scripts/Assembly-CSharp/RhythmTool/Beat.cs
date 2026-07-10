using System;

namespace RhythmTool
{
	[Serializable]
	public struct Beat : IFeature
	{
		public float timestamp;

		public float bpm;

		float IFeature.timestamp => timestamp;

		float IFeature.length => 0f;
	}
}
