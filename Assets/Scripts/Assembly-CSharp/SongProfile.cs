using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "songProfile", menuName = "ScriptableObjects/SongProfile", order = 1)]
public class SongProfile : ScriptableObject
{
	public List<MusEvent> beatChange;

	public List<MusEvent> noteChange;

	public List<MusEffect> customChange;
}
