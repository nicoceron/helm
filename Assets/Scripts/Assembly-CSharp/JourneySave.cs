using System;

[Serializable]
public class JourneySave
{
	public string cloneNick;

	public int distance;

	public int cloneNumber;

	public JourneySave(string nick, int dist, int number)
	{
		cloneNick = nick;
		distance = dist;
		cloneNumber = number;
	}
}
