using System;

[Serializable]
public struct MusEvent
{
	public float timestamp;

	public float value;

	public MusEvent(float t, float v)
	{
		timestamp = t;
		value = v;
	}
}
