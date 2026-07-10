using System;

[Serializable]
public class DataCustom
{
	public string var;

	public int val;

	public DataCustom()
	{
	}

	public DataCustom(string variable, int value)
	{
		var = variable;
		val = value;
	}
}
