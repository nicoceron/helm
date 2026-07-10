using System;
using System.Collections.Generic;

[Serializable]
public class BearerSave
{
	public Bearers type;

	public float vote;

	public string name;

	public List<Bearers> character;

	public BearerSave(Bearers t, float v, string n, List<Bearers> chara)
	{
		type = t;
		vote = v;
		name = n;
		character = new List<Bearers>(chara);
	}
}
