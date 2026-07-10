using System;
using System.Collections.Generic;

[Serializable]
public class Objective
{
	public string name;

	public int id;

	public int pid;

	public GText title;

	public string description;

	public string achievement;

	public bool accessible;

	public bool fulfilled;

	public bool visible;

	public List<Condition> conditions = new List<Condition>();

	public Objective()
	{
	}

	public Objective(string[] rawele, string[] columns, List<Objective> objects, int nid, Dictionary<string, string[]> i18n, bool nostatechange = false)
	{
		string[] array = new string[3];
		bool flag = false;
		for (int i = 0; i < columns.Length; i++)
		{
			string val = rawele[i];
			switch (columns[i])
			{
			case "name":
				name = val;
				if (i18n.Count > 0 && i18n.ContainsKey(val))
				{
					array = i18n[val];
					flag = true;
				}
				break;
			case "id":
				int.TryParse(val, out id);
				break;
			case "title":
				title = (flag ? new GText(array[0]) : new GText(val));
				break;
			case "description":
				description = (flag ? array[1] : val);
				break;
			case "achievement":
				achievement = (flag ? array[2] : val);
				break;
			case "blocked_until_revealed":
				accessible = ((!(val == "x")) ? true : false);
				break;
			case "parent":
				pid = ((val == "") ? (-1) : (pid = objects.Find((Objective it) => it.name == val).id));
				break;
			case "conditions":
				conditions = Condition.TreatCondition(val);
				break;
			}
		}
		if (id == 1 || nostatechange || accessible)
		{
			return;
		}
		foreach (Condition condition in conditions)
		{
			if (((condition.value > 0 && condition.bearer == Bearers.none) || (condition.value == 0 && condition.bearer != Bearers.none)) && condition.condition == Conditions.equal)
			{
				GameAct.diff.LockCards(condition);
			}
			if ((condition.value == -1 && condition.bearer == Bearers.none && condition.condition == Conditions.equal) || (condition.value == 0 && condition.bearer != Bearers.none && condition.condition == Conditions.notequal))
			{
				GameAct.diff.LockCardsOutcome(condition);
			}
		}
	}
}
