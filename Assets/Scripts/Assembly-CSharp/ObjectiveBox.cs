using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveBox : ContentBox
{
	public Color validCol;

	public GameObject starPrefab;

	public Color activeCol;

	public Color hiddenCol;

	public Image checkbox;

	public Text text;

	private RectTransform thisrect;

	public Objective obj;

	public Text nick;

	public override void Validate()
	{
		if (obj != null && obj.fulfilled)
		{
			StartCoroutine("MoveCheck");
		}
		base.Validate();
	}

	public override void Init(object instance, string txtid, bool trig, bool stayHidden = false)
	{
		obj = (Objective)instance;
		thisrect = GetComponent<RectTransform>();
		if (!stayHidden)
		{
			text.text = SpeechAct.diff.FinalFormat(obj.description);
		}
		if (trig)
		{
			ShowCheck();
			text.color = validCol;
			if ((bool)nick)
			{
				nick.color = validCol;
			}
		}
		else if (stayHidden)
		{
			text.color = hiddenCol;
			if ((bool)nick)
			{
				nick.color = hiddenCol;
			}
		}
		else
		{
			text.color = activeCol;
			if ((bool)nick)
			{
				nick.color = activeCol;
			}
		}
		if ((bool)nick)
		{
			nick.text = SpeechAct.diff.FinalFormat(obj.title.Get());
		}
	}

	private IEnumerator MoveCheck()
	{
		yield return new WaitForSeconds(0.5f);
		JukeBox.diff.PlaySound(SFXTypes.ui_achievement_unlock);
		WaitForSeconds swait = new WaitForSeconds(0.05f);
		for (int i = 0; i < 8; i++)
		{
			yield return swait;
			if (starPrefab != null)
			{
				GameObject obj = Object.Instantiate(starPrefab);
				obj.transform.SetParent(base.transform.parent.parent.parent, worldPositionStays: false);
				obj.transform.SetAsFirstSibling();
				float num = Util.Rand(0.5f);
				obj.transform.localScale = new Vector3(num, num, 1f);
				obj.transform.Rotate(Vector3.forward, Util.Rand(0f, 360f));
			}
		}
		float n = 0f;
		while (n < 1f)
		{
			checkbox.fillAmount = n;
			n += Time.deltaTime * 2f;
			yield return null;
		}
	}

	public void ShowCheck()
	{
		checkbox.fillAmount = 1f;
	}

	public void HideRight()
	{
		StartCoroutine("DoHideRight");
	}

	private IEnumerator DoHideRight()
	{
		float t = 0f;
		while (t < 1f)
		{
			thisrect.anchoredPosition = Vector2.Lerp(thisrect.anchoredPosition, thisrect.anchoredPosition + Vector2.right * 10f, 0.3f + t);
			t += Time.deltaTime * 4f;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	public override void Close()
	{
		base.Close();
	}

	private void OnDisable()
	{
		Object.Destroy(base.gameObject);
	}
}
