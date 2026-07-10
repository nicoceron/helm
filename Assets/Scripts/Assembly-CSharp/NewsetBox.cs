using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NewsetBox : ContentBox
{
	public Text setTxt;

	public override void Init(object instance, string txtid, bool trig, bool stayHidden = false)
	{
		setTxt.text = txtid;
	}

	public override void Validate()
	{
		StartCoroutine("PlayAnim");
	}

	private IEnumerator PlayAnim()
	{
		yield return new WaitForSeconds(0.2f);
		JukeBox.diff.PlaySound(SFXTypes.ui_new_cards);
		yield return new WaitForSeconds(0.5f);
		BackgroundAct.diff.ShowBacks(keepone: true);
	}
}
