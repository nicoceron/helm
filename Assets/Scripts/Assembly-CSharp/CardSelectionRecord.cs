using System;

[Serializable]
public class CardSelectionRecord : IAutomationRecord
{
	public string CardName;

	public int CardId;

	public string Print()
	{
		return $"Selection of card {CardId} ({CardName})";
	}
}
