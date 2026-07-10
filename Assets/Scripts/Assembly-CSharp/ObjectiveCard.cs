public class ObjectiveCard : CardAct
{
	private void Awake()
	{
		_Awake();
	}

	public override void InitCard(string yesText, string noText, string otherText, int decision, bool withanim = true)
	{
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
