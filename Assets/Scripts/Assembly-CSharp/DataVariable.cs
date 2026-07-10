using System;

[Serializable]
public class DataVariable
{
	public Variables var;

	public int val;

	public DataVariable()
	{
	}

	public DataVariable(Variables variable, int value)
	{
		var = variable;
		val = value;
	}
}
