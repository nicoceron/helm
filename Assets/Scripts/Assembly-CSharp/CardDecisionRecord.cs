using System;

[Serializable]
public class CardDecisionRecord : IAutomationRecord
{
	public string CardName;

	public int CardId;

	public AutomationController.CardSlideDirection Decision;

	public string Print()
	{
		return $"Making desicion on card {CardId} ({CardName}). Decision: {Decision.ToString()}";
	}
}
