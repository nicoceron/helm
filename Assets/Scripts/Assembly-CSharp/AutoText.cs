using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoText : MonoBehaviour
{
	public Vector2 DefaultTransfo;

	private Vector2 modifTransfoPos;

	private Vector2 oriPos;

	public bool retrieveText = true;

	public float clearAndBack = 1f;

	public Vector2 moveAndBack = Vector2.zero;

	public float sizeAndBack = 1f;

	public float speed = 1f;

	private Graphic txt;

	private RectTransform trans;

	public float delay;

	public int txtId;

	public string suffix;

	public bool thenalphaback;

	private Color oriCol;

	public bool suspendMove;

	private void Awake()
	{
		trans = GetComponent<RectTransform>();
		txt = GetComponent<Graphic>();
		if ((bool)txt)
		{
			txt.enabled = false;
		}
		oriPos = trans.anchoredPosition;
		oriCol = txt.color;
	}

	private void OnEnable()
	{
		if ((bool)txt)
		{
			txt.enabled = false;
		}
		StartCoroutine("Delay");
	}

	private void OnDisable()
	{
		trans.anchoredPosition = oriPos;
		if ((bool)txt)
		{
			txt.color = oriCol;
		}
	}

	private IEnumerator Delay()
	{
		yield return null;
		yield return new WaitForSeconds(delay);
		if (retrieveText)
		{
			GetComponent<Text>().text = SpeechAct.diff.GetSceneTextFinal(base.name, txtId);
		}
		if (!suspendMove)
		{
			if ((bool)txt)
			{
				txt.enabled = true;
			}
			if (clearAndBack != 1f)
			{
				StartCoroutine("AlphaAndBack", clearAndBack);
			}
			if (moveAndBack != Vector2.zero)
			{
				StartCoroutine("MoveAndBack", moveAndBack);
			}
			if (sizeAndBack != 1f)
			{
				StartCoroutine("SizeAndBack", sizeAndBack);
			}
			yield return new WaitForSeconds(1f);
			if (thenalphaback)
			{
				StopCoroutine("AlphaAndBack");
				Color color = txt.color;
				float a = color.a;
				txt.color = new Color(color.r, color.g, color.b, clearAndBack);
				StopCoroutine("AlphaAndBack");
				StartCoroutine("AlphaAndBack", a);
			}
		}
	}

	private IEnumerator AlphaAndBack(float a)
	{
		Color ocol = txt.color;
		Color ncol = new Color(ocol.r, ocol.g, ocol.b, a);
		float t = 0f;
		while (t < 1f)
		{
			txt.color = Color.Lerp(ncol, ocol, 0.1f + t);
			t += Time.deltaTime * 2f * speed;
			yield return null;
		}
		txt.color = ocol;
	}

	public void MoveDirect()
	{
		GetComponent<RectTransform>().anchoredPosition = oriPos + moveAndBack;
	}

	public void Move()
	{
		if (clearAndBack != 1f)
		{
			StopCoroutine("AlphaAndBack");
			StartCoroutine("AlphaAndBack", clearAndBack);
		}
		StopCoroutine("DoMove");
		StopCoroutine("DoMoveDelay");
		StartCoroutine("DoMoveDelay", oriPos + moveAndBack);
	}

	public void AnteMove()
	{
		if (clearAndBack != 1f)
		{
			Color color = txt.color;
			float a = color.a;
			txt.color = new Color(color.r, color.g, color.b, clearAndBack);
			StopCoroutine("AlphaAndBack");
			StartCoroutine("AlphaAndBack", a);
		}
		StopCoroutine("DoMove");
		StopCoroutine("DoMoveDelay");
		StartCoroutine("DoMoveDelay", oriPos);
	}

	private IEnumerator DoMoveDelay(Vector2 move)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		Vector2 npos = trans.anchoredPosition;
		float t = 0f;
		while (t < 1f)
		{
			trans.anchoredPosition = Vector2.Lerp(npos, move, 0.1f + t);
			t += Time.deltaTime * speed;
			yield return 0;
		}
		trans.anchoredPosition = move;
	}

	private IEnumerator DoMove(Vector2 move)
	{
		Vector2 npos = trans.anchoredPosition;
		float t = 0f;
		while (t < 1f)
		{
			trans.anchoredPosition = Vector2.Lerp(npos, move, 0.1f + t);
			t += Time.deltaTime * speed;
			yield return 0;
		}
		trans.anchoredPosition = move;
	}

	private IEnumerator MoveAndBack(Vector2 moveAndBack)
	{
		Vector2 opos = trans.anchoredPosition;
		Vector2 npos = trans.anchoredPosition + moveAndBack;
		float t = 0f;
		while (t < 1f)
		{
			trans.anchoredPosition = Vector2.Lerp(npos, opos, 0.1f + t);
			t += Time.deltaTime * speed;
			yield return 0;
		}
		trans.anchoredPosition = opos;
	}

	private IEnumerator SizeAndBack(float size)
	{
		Vector3 osize = trans.localScale;
		Vector3 nsize = new Vector3(size, size, size);
		float t = 0f;
		while (t < 1f)
		{
			trans.localScale = Vector3.Lerp(nsize, osize, 0.1f + t);
			t += Time.deltaTime * speed;
			yield return 0;
		}
		trans.localScale = osize;
	}
}
