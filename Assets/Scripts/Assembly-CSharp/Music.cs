using System;
using UnityEngine;

[Serializable]
public class Music
{
	public AudioClip sample;

	public string command;

	public bool loop;

	public Music()
	{
	}

	public Music(AudioClip clip, string folder)
	{
		sample = clip;
		if (clip != null)
		{
			loop = clip.name.Contains("loop");
		}
		command = folder;
	}
}
