using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SFX
{
	public string name;

	public SFXTypes type;

	public SFXSources source;

	public List<AudioClip> clips;

	[HideInInspector]
	public AudioClip lastclip;

	[HideInInspector]
	public AudioSource lastsource;

	public bool loop;

	public SFX()
	{
	}

	public SFX(string nam, List<AudioClip> cls)
	{
		if (nam.StartsWith("amb_") || nam.StartsWith("env_"))
		{
			source = SFXSources.ambient;
			loop = true;
		}
		else if (nam.StartsWith("sting_"))
		{
			source = SFXSources.songs;
		}
		else if (nam.StartsWith("ui_"))
		{
			source = SFXSources.ui;
		}
		else
		{
			source = SFXSources.sfx;
		}
		type = (SFXTypes)Enum.Parse(typeof(SFXTypes), nam);
		clips = cls;
		name = nam;
	}
}
