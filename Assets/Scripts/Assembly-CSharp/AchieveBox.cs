using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchieveBox : ContentBox
{
	public Text titleTxt;

	public Text descriptTxt;

	public string sfx;

	public override void Init(object instance, string txtid, bool trig, bool stayHidden = false)
	{
		List<string> list = (List<string>)instance;
		titleTxt.text = list[0];
		descriptTxt.text = list[1];
		if (list.Count == 3)
		{
			StartCoroutine("ShowHighScore", list[2]);
		}
	}

	public override void Validate()
	{
		StartCoroutine("PlayAnim");
	}

	private IEnumerator PlayAnim()
	{
		yield return new WaitForSeconds(0.2f);
	}

	private IEnumerator ShowHighScore(string alt)
	{
		string txt = titleTxt.text;
		WaitForSeconds swait = new WaitForSeconds(0.4f);
		WaitForSeconds lwait = new WaitForSeconds(1f);
		while (true)
		{
			titleTxt.text = txt;
			yield return lwait;
			titleTxt.text = "";
			yield return swait;
			for (int i = 0; i < 3; i++)
			{
				titleTxt.text = alt;
				yield return swait;
				titleTxt.text = "";
				yield return swait;
			}
		}
	}
}
