public class EndCard : CardAct
{
	private bool isFinalFree;

	public DeadCloneAct cloneSc;

	private void Awake()
	{
		_Awake();
	}

	public override void InitCard(string yesText = "", string noText = "", string otherText = "", int decision = 0, bool withanim = true)
	{
		BackgroundAct.diff.HideTop();
		BackgroundAct.diff.HideBottom();
		CardReader.diff.GetComponent<ObjectiveAct>().DestroyBoxes();
		BackgroundAct.diff.FadeToBlack();
		base.InitCard("", "", "", -decision);
		cloneSc.Init();
	}

	public override void HideCard()
	{
		if (isFinalFree)
		{
			BackgroundAct.diff.FadeToBlack();
		}
		base.HideCard();
	}
}
