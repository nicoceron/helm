using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathBox : ContentBox
{
	public Text title;

	public Text description;

	public GameObject starPrefab;

	public override void Init(object instance, string txtid, bool trig, bool stayHidden = false)
	{
		description.text = txtid;
	}

	public override void Validate()
	{
	}

	private IEnumerator PlayAnim()
	{
		yield return new WaitForSeconds(0.2f);
	}
}
