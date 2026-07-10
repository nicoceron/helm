using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "backProfile", menuName = "ScriptableObjects/BackProfile", order = 3)]
public class BackProfile : ScriptableObject
{
	[SerializeField]
	public Backgrounds type;

	[SerializeField]
	public new string name;

	[SerializeField]
	public string conditions;

	[SerializeField]
	public GameObject prefab;

	[SerializeField]
	public GameObject alternative;

	[SerializeField]
	public List<Condition> treatedConditions;

	[SerializeField]
	public SFXTypes appearSFX;

	public void TreatConditions()
	{
		treatedConditions = Condition.TreatCondition(conditions);
	}
}
