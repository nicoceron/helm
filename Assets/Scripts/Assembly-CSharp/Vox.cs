using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vox
{
	public VoxTypes type;

	public DataVariable dataRef;

	public AudioSource[] speakers;

	[HideInInspector]
	public List<AudioClip> clips;

	[HideInInspector]
	public AudioClip curclip;
}
