using System;

namespace RhythmTool
{
	[Serializable]
	public struct Chroma : IFeature
	{
		public float timestamp;

		public float length;

		public Note note;

		float IFeature.timestamp => timestamp;

		float IFeature.length => length;
	}
}
