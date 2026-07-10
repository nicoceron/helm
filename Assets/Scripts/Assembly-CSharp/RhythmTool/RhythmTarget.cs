using UnityEngine;

namespace RhythmTool
{
	public abstract class RhythmTarget : ScriptableObject
	{
		public abstract void Process(RhythmData rhythmData, float start, float end);

		public abstract void Reset(RhythmData rhythmData, float time);
	}
}
