using System;

[Serializable]
public class Achievement
{
	public string name;

	public AchieveTypes type;

	public Achievement()
	{
	}

	public Achievement(string nam, AchieveTypes typ)
	{
		name = nam;
		type = typ;
	}
}
