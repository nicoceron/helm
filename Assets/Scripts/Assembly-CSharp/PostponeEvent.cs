using System;

[Serializable]
public class PostponeEvent
{
	public int distance;

	public string card;

	public Bearers bear = Bearers.none;

	public PostponeEvent()
	{
	}

	public PostponeEvent(int di, string ca)
	{
		distance = di;
		card = ca;
	}

	public PostponeEvent(int di, Bearers be)
	{
		distance = di;
		bear = be;
	}
}
