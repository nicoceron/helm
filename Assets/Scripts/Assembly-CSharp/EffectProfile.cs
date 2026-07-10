using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "effectProfile", menuName = "ScriptableObjects/EffectProfile", order = 4)]
public class EffectProfile : ScriptableObject
{
	[SerializeField]
	public EffectStyles style;

	[SerializeField]
	public AnimationCurve intensity;

	[SerializeField]
	public float time;

	[SerializeField]
	public VolumeProfile volumeprofile;

	[SerializeField]
	public float gotone;

	[SerializeField]
	public bool loop;

	[SerializeField]
	public SFXTypes sound;
}
