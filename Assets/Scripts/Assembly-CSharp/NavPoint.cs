using System;

[Serializable]
public class NavPoint
{
	public string name;

	public Backgrounds type;

	public int distance;

	public int cid = -1;

	public bool auto;

	public NavPoint(string n, Backgrounds t, int d, int c, bool automatic)
	{
		name = n;
		type = t;
		distance = d;
		cid = c;
		auto = automatic;
	}
}
