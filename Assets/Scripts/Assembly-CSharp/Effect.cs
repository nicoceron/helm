using System;
using System.Collections.Generic;

[Serializable]
public class Effect
{
	public string tag;

	public string title;

	public string description;

	public bool active;

	public int length;

	public bool seen;

	public bool alwayshowcard;

	public List<Outcome> outcomes = new List<Outcome>();

	public Effect()
	{
	}

	public Effect(string[] rawele, string[] columns, Dictionary<string, string[]> i18n, EffectAct scEf = null)
	{
		string[] array = new string[2];
		bool flag = false;
		for (int i = 0; i < columns.Length; i++)
		{
			string text = rawele[i];
			string text2 = columns[i];
			switch (text2)
			{
			case "tag":
				tag = text;
				if (i18n.Count > 0 && i18n.ContainsKey(text))
				{
					array = i18n[text];
					flag = true;
				}
				break;
			case "title":
				title = (flag ? array[0] : text);
				break;
			case "alwayshowcard":
				alwayshowcard = !string.IsNullOrEmpty(text);
				break;
			case "description":
				description = (flag ? TreatText(array[1]) : TreatText(text));
				break;
			case "custom":
				if (!string.IsNullOrEmpty(text))
				{
					outcomes.AddRange(Outcome.TreatOutcome(text));
				}
				break;
			default:
				if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text))
				{
					outcomes.Add(new Outcome(text2, text));
				}
				break;
			}
		}
	}

	public string TreatText(string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			return "";
		}
		val = val.Replace("<*", "<color=#e2081e>");
		val = val.Replace("*>", "</color>");
		if ((bool)SpeechAct.diff && SpeechAct.diff.asiaLayout)
		{
			val = val.Replace("+", "\n");
		}
		return val;
	}
}
