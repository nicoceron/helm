using UnityEngine;
using UnityEngine.UI;

public class IntercaleCard : CardAct
{
	public Text intercaleTxt;

	private void Awake()
	{
		_Awake();
	}

	private void Start()
	{
		if (SpeechAct.diff.asiaLayout)
		{
			intercaleTxt.fontSize = 13;
			intercaleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
		else
		{
			intercaleTxt.fontSize = 15;
			intercaleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
	}

	public override void InitCard(string yesText, string noText, string otherText, int decision, bool withanim = true)
	{
		intercaleTxt.text = otherText;
		base.InitCard(yesText, noText, otherText, decision);
	}

	public override void HideCard()
	{
		base.HideCard();
	}

	public override void HideFond()
	{
		base.HideFond();
	}

	public override void ShowDecision(int dec)
	{
	}
}
