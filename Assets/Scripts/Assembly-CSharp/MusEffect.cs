using System;

[Serializable]
public struct MusEffect
{
	public float timestamp;

	public MusEffects effect;

	public MusEffect(float t, int v)
	{
		timestamp = t;
		effect = (MusEffects)v;
	}
}
