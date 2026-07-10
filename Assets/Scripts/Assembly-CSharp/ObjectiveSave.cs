using System;

[Serializable]
public class ObjectiveSave
{
	public int id;

	public bool accessible;

	public bool fulfilled;

	public bool visible;

	public ObjectiveSave(int i, bool a, bool f, bool v)
	{
		id = i;
		accessible = a;
		fulfilled = f;
		visible = v;
	}
}
