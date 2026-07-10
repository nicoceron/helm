using System;

[Serializable]
public class UIRecord : IAutomationRecord
{
	public string Action;

	public string Print()
	{
		return Action;
	}
}
