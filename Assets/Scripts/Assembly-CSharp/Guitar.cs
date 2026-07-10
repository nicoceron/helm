using UnityEngine;

[CreateAssetMenu(fileName = "guitar", menuName = "ScriptableObjects/Guitar", order = 2)]
public class Guitar : ScriptableObject
{
	public new string name;

	public int id;

	public SongProfile profile;

	public AudioClip clip;

	public GameObject prefab;

	public Color mainColor;

	public Color mainBackColor;

	public Color complementaryColor;

	public Color complementaryBackColor;

	public AnimationCurve verticalCurve;

	public BackgroundStyles style;

	public BackgroundStyles styleVariation;

	public bool regularRotation;

	public float smooth;
}
