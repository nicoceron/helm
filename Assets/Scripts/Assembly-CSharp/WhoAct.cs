using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WhoAct : MonoBehaviour
{
	private Text mytext;

	public Text titleText;

	private RectTransform myrect;

	private Vector2 showPos = new Vector2(0f, -343f);

	private Vector2 belowPos = new Vector2(0f, -375f);

	private Vector2 abovePos = new Vector2(0f, -325f);

	private string savename = "";

	private bool curisitem;

	private void Awake()
	{
		mytext = GetComponent<Text>();
		myrect = GetComponent<RectTransform>();
		showPos = myrect.anchoredPosition;
	}

	public void ShowNameIf(string old)
	{
		if (old == mytext.text || curisitem)
		{
			ShowName();
		}
	}

	public void ShowName()
	{
		ShowName(savename);
	}

	public void ShowName(string name, string title = "")
	{
		if (!(name == mytext.text))
		{
			savename = name;
			curisitem = false;
			StopAllCoroutines();
			StartCoroutine(DoShowName(name, title));
		}
	}

	public void ShowItem(string item)
	{
		if (!(item == titleText.text))
		{
			curisitem = true;
			StopAllCoroutines();
			StartCoroutine(DoShowItem(item));
		}
	}

	private IEnumerator DoShowName(string txt, string title)
	{
		yield return StartCoroutine("Move", belowPos);
		mytext.text = txt;
		titleText.text = title;
		myrect.anchoredPosition = abovePos;
		yield return StartCoroutine("Move", showPos);
	}

	private IEnumerator DoShowItem(string txt)
	{
		yield return StartCoroutine("Move", abovePos);
		mytext.text = "";
		titleText.text = txt;
		myrect.anchoredPosition = belowPos;
		yield return StartCoroutine("Move", showPos);
	}

	private IEnumerator Move(Vector2 targ)
	{
		while (Vector2.SqrMagnitude(myrect.anchoredPosition - targ) > 0.4f)
		{
			myrect.anchoredPosition = Vector2.Lerp(myrect.anchoredPosition, targ, Time.deltaTime * 36f);
			yield return null;
		}
		myrect.anchoredPosition = targ;
	}
}
