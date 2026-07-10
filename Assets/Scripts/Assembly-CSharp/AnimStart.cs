using System.Collections;
using System.Collections.Generic;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class AnimStart : MonoBehaviour
{
	public List<SVGImage> ima;

	private RectTransform trans;

	public GameObject but;

	private void OnEnable()
	{
		trans = GetComponent<RectTransform>();
		StopAllCoroutines();
		float num = 0f;
		foreach (SVGImage item in ima)
		{
			StartCoroutine(Anim(item, num));
			num += 0.2f;
		}
		StartCoroutine("YieldAndText");
	}

	private IEnumerator YieldAndText()
	{
		yield return null;
		Text componentInChildren = GetComponentInChildren<Text>();
		if (InputAct.diff.curInput != Inputs.touch)
		{
			componentInChildren.text = SpeechAct.diff.GetSceneTextFinal("action_" + InputAct.diff.curInput);
		}
	}

	private IEnumerator Anim(SVGImage ima, float delay)
	{
		yield return new WaitForSeconds(delay);
		WaitForSeconds swait = new WaitForSeconds(0.4f);
		WaitForSeconds lwait = new WaitForSeconds(1f);
		while (true)
		{
			ima.CrossFadeAlpha(0.01f, 0.2f, ignoreTimeScale: true);
			yield return swait;
			ima.CrossFadeAlpha(1f, 0.3f, ignoreTimeScale: true);
			yield return lwait;
		}
	}

	public void HideText()
	{
		but.SetActive(value: false);
		trans.anchoredPosition = new Vector2(-566f, trans.anchoredPosition.y);
	}

	public void ShowText()
	{
		StopCoroutine("DoShowTxt");
		StartCoroutine("DoShowTxt");
	}

	private IEnumerator DoShowTxt()
	{
		yield return null;
		but.SetActive(value: true);
		float t = 0f;
		Vector2 tpo = new Vector2(-616f, trans.anchoredPosition.y);
		while (t < 1f)
		{
			trans.anchoredPosition = Vector2.Lerp(trans.anchoredPosition, tpo, Time.deltaTime * 6f);
			t += Time.deltaTime;
			yield return null;
		}
		trans.anchoredPosition = tpo;
	}
}
