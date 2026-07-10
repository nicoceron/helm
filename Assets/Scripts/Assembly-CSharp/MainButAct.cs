using System.Collections;
using UnityEngine;

public class MainButAct : MonoBehaviour
{
	private RectTransform trans;

	private Vector2 opos;

	private void OnEnable()
	{
		trans = GetComponent<RectTransform>();
		trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, 357f);
	}

	public void Move()
	{
		trans = GetComponent<RectTransform>();
		opos = new Vector2(trans.anchoredPosition.x, 257f);
		StopCoroutine("DoMove");
		trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, 357f);
		StartCoroutine("DoMove");
	}

	private IEnumerator DoMove()
	{
		float t = 0f;
		while (t < 1f)
		{
			trans.anchoredPosition = Vector2.Lerp(trans.anchoredPosition, opos, Time.deltaTime * 8f);
			t += Time.deltaTime;
			yield return null;
		}
		trans.anchoredPosition = opos;
	}
}
