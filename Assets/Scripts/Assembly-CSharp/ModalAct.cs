using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModalAct : MonoBehaviour
{
	private RectTransform content;

	private RectTransform box;

	private bool isSkipped;

	public ContentBox scbo;

	public static ModalAct diff;

	public GameObject objectivePrefab;

	public GameObject deathPrefab;

	public GameObject achievementPrefab;

	public GameObject highscorePrefab;

	public Transform contentBox;

	public GameObject but;

	private float skipdelay;

	private GameObject obj;

	private Color oricol;

	private Graphic gra;

	private bool isReadyToSkip;

	public void Init(ModalTypes typ, object instance, float delay = 0f, string txtid = "", bool decal = true)
	{
		skipdelay = delay;
		box = GetComponent<RectTransform>();
		if (obj != null)
		{
			Object.Destroy(obj);
			obj = null;
		}
		switch (typ)
		{
		case ModalTypes.objective:
			obj = Object.Instantiate(objectivePrefab);
			break;
		case ModalTypes.death:
			obj = Object.Instantiate(deathPrefab);
			break;
		case ModalTypes.achievement:
			obj = Object.Instantiate(achievementPrefab);
			break;
		case ModalTypes.highscore:
			obj = Object.Instantiate(highscorePrefab);
			break;
		case ModalTypes.custom:
			obj = Object.Instantiate((GameObject)instance);
			break;
		}
		obj.transform.SetParent(contentBox, worldPositionStays: false);
		gra = obj.GetComponent<Graphic>();
		if (gra != null)
		{
			oricol = gra.color;
			gra.color = Color.Lerp(oricol, Color.white, 0.2f);
		}
		content = obj.GetComponent<RectTransform>();
		content.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(10000f, content.sizeDelta.y + 20f);
		scbo = content.GetComponent<ContentBox>();
		if (scbo != null)
		{
			scbo.Init(instance, txtid);
		}
	}

	public IEnumerator Fire()
	{
		diff = this;
		StartCoroutine("MoveContent");
		if (scbo != null)
		{
			scbo.Validate();
		}
		yield return StartCoroutine("MoveBox");
	}

	private IEnumerator MoveContent()
	{
		Vector2 anchoredPosition = new Vector2(0f, 0f);
		content.anchoredPosition = anchoredPosition;
		yield break;
	}

	private IEnumerator YieldAndReadySkip()
	{
		if (InputAct.diff.curInput == Inputs.twitch)
		{
			isReadyToSkip = true;
			yield break;
		}
		InputAct.diff.SuspendSlideFocus();
		yield return new WaitForSeconds(1.2f);
		if ((bool)gra)
		{
			gra.CrossFadeColor(oricol, 0.2f, ignoreTimeScale: true, useAlpha: false);
		}
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.SwitchSize(tall: true);
			AnimBut.diff.UnLock(ControlModes.next);
		}
		InputAct.diff.GetActionFocus(Close);
		isReadyToSkip = true;
	}

	private IEnumerator MoveBox()
	{
		float t = 0f;
		Vector2 opos = new Vector2(0f, 130f);
		Vector2 tpos = new Vector2(0f, 10f);
		StartCoroutine("YieldAndReadySkip");
		while (t < 1f)
		{
			if (isSkipped)
			{
				yield break;
			}
			box.anchoredPosition = Vector2.LerpUnclamped(opos, tpos, Easing.CircEaseOut(t, 0f, 1f, 1f));
			t += Time.deltaTime;
			yield return null;
		}
		but.SetActive(value: true);
		while (skipdelay <= 0f || !isReadyToSkip)
		{
			if (isSkipped)
			{
				yield break;
			}
			yield return null;
		}
		t = skipdelay;
		while (t > 0f)
		{
			if (isSkipped)
			{
				yield break;
			}
			t -= Time.deltaTime;
			yield return null;
		}
		Close();
	}

	private IEnumerator CloseBox(Vector2 tpos)
	{
		but.SetActive(value: false);
		Vector2 opos = new Vector2(0f, -700f);
		float t = 0f;
		while (t < 1f)
		{
			box.anchoredPosition = Vector2.Lerp(tpos, opos, Easing.CircEaseIn(t, 0f, 1f, 1f));
			t += Time.deltaTime * 4f;
			yield return null;
		}
		if (scbo != null)
		{
			scbo.Close();
		}
		yield return null;
		Object.Destroy(base.gameObject);
	}

	public bool Close(bool n = false)
	{
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock();
		}
		InputAct.diff.RestoreSlideFocus();
		skipdelay = 0.01f;
		isSkipped = true;
		if (box != null)
		{
			StopCoroutine("CloseBox");
			StartCoroutine("CloseBox", box.anchoredPosition);
		}
		return true;
	}

	public void ForceClose()
	{
		InputAct.diff.TapAction();
	}
}
