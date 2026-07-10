using UnityEngine;

public class BackVariation : MonoBehaviour
{
	[Range(0f, 1f)]
	public float amount = 0.5f;

	private float lastamount = 0.5f;

	public bool swapSide = true;

	public Vector2 leftMax;

	private Vector2 lastLeftMax;

	public Vector2 rightMax;

	private Vector2 lastRightMax;

	[HideInInspector]
	public bool initiated;

	private RectTransform trans;

	private void Awake()
	{
		trans = GetComponent<RectTransform>();
	}

	public void InitEditor(RectTransform _trans)
	{
		if (!initiated)
		{
			trans = _trans;
			leftMax = trans.anchoredPosition;
			rightMax = trans.anchoredPosition;
			initiated = true;
		}
	}

	private void OnValidate()
	{
		if (initiated)
		{
			if (!lastamount.Equals(amount))
			{
				SetPosition(amount);
			}
			else if (!leftMax.Equals(lastLeftMax))
			{
				amount = 0f;
				SetPosition(amount);
			}
			else if (!rightMax.Equals(lastRightMax))
			{
				amount = 1f;
				SetPosition(amount);
			}
			lastRightMax = rightMax;
			lastLeftMax = leftMax;
			lastamount = amount;
		}
	}

	public void Reset()
	{
		swapSide = false;
		leftMax = GetComponent<RectTransform>().anchoredPosition;
		rightMax = GetComponent<RectTransform>().anchoredPosition;
		amount = 0.5f;
	}

	public Vector2 Generate(string n)
	{
		float position = Util.GetFloat(n);
		if (swapSide)
		{
			base.transform.localScale = ((Util.GetInt(n, 0, 2) == 0) ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f));
		}
		return SetPosition(position);
	}

	public Vector2 SetPosition(float amo)
	{
		Vector2 vector = Vector2.Lerp(leftMax, rightMax, amo);
		GetComponent<RectTransform>().anchoredPosition = vector;
		return vector;
	}
}
