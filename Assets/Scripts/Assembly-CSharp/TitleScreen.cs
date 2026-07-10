using System.Collections;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
	private RectTransform mytrans;

	private void Awake()
	{
		mytrans = GetComponent<RectTransform>();
	}

	public virtual IEnumerator Open()
	{
		mytrans.anchoredPosition = new Vector2(0f, 0f);
		yield return null;
	}

	public void LerpToPos(Vector2 target, float amount)
	{
		mytrans.anchoredPosition = Vector2.Lerp(mytrans.anchoredPosition, target, amount);
	}

	public void AddToPos()
	{
		mytrans.anchoredPosition = Vector2.Lerp(mytrans.anchoredPosition, mytrans.anchoredPosition * 2f, Time.deltaTime * 4f);
	}
}
