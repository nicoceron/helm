using System;
using System.Collections.Generic;

[Serializable]
public class Card
{
	public string name;

	public int id;

	public GText question;

	public GText override_yes;

	public GText override_no;

	public GText answer_yes;

	public GText answer_no;

	public Bearers bearer = Bearers.none;

	public Bearers bearerIsAlso = Bearers.none;

	public Bearers bearerIsNot = Bearers.none;

	public string bearerVariation = "";

	public List<Condition> conditions = new List<Condition>();

	public int weight;

	public int weightVar;

	public int weightReal;

	public int weightNocond;

	public int lockturn;

	public int nextturn;

	public bool isLocked;

	public bool wasSeen;

	public List<Outcome> yes_outcomes = new List<Outcome>();

	public List<Outcome> no_outcomes = new List<Outcome>();

	public List<Outcome> load_outcomes = new List<Outcome>();

	public Backgrounds place;

	public string place_name;

	public Card()
	{
	}

	public Card(string[] rawele, string[] columns, char dele, Dictionary<string, string[]> i18n, int lastid)
	{
		string[] array = new string[5];
		bool flag = false;
		id = -1;
		for (int i = 0; i < columns.Length; i++)
		{
			string text = rawele[i];
			string text2 = columns[i];
			switch (text2)
			{
			case "id":
				if (i18n.Count > 0)
				{
					if (i18n.ContainsKey(text))
					{
						array = i18n[text];
					}
					flag = true;
				}
				int.TryParse(text, out id);
				if (id == -1)
				{
					InputAct.diff.OfferReset(all: false, "It seems an uninvited return carriage is crashing the spreadsheet. \n Around card id: " + lastid);
				}
				continue;
			case "card":
				if (string.IsNullOrEmpty(text))
				{
					text = id.ToString();
				}
				if (text == "_")
				{
					text = "_" + id;
				}
				name = text;
				continue;
			case "bearer":
				if (text.Contains(">"))
				{
					string[] array2 = text.Split(new string[1] { ">" }, StringSplitOptions.None);
					text = array2[0];
					bearerVariation = array2[1];
				}
				if (text.Contains("&"))
				{
					string[] array3 = text.Split(new string[1] { "&" }, StringSplitOptions.None);
					text = array3[0];
					bearerIsAlso = (Bearers)Enum.Parse(typeof(Bearers), array3[1]);
				}
				if (text.Contains("!"))
				{
					string[] array4 = text.Split(new string[1] { "!" }, StringSplitOptions.None);
					text = array4[0];
					bearerIsNot = (Bearers)Enum.Parse(typeof(Bearers), array4[1]);
				}
				if (string.IsNullOrEmpty(text))
				{
					bearer = Bearers.none;
				}
				try
				{
					bearer = (Bearers)Enum.Parse(typeof(Bearers), text);
				}
				catch
				{
					bearer = Bearers.anyone;
				}
				if (bearer == Bearers.end)
				{
					GameAct.diff.AddEndCard(bearerVariation);
				}
				continue;
			case "conditions":
				conditions.AddRange(Condition.TreatCondition(text));
				continue;
			case "lockturn":
				if (text == "del")
				{
					lockturn = -1;
				}
				else if (text == "space")
				{
					lockturn = -10;
				}
				else
				{
					int.TryParse(text, out lockturn);
				}
				nextturn = 0;
				continue;
			case "weight":
				if (text == "lock")
				{
					weight = (weightReal = 106);
					continue;
				}
				if (text.Contains("c"))
				{
					string[] array5 = text.Split(new string[1] { "c" }, StringSplitOptions.None);
					text = array5[0];
					weightNocond = TreatWeight(array5[1]);
				}
				if (text.Contains("+"))
				{
					string[] array6 = text.Split(new string[1] { "+" }, StringSplitOptions.None);
					text = array6[0];
					weightVar = TreatWeight(array6[1]);
				}
				if (text.Contains("-"))
				{
					string[] array7 = text.Split(new string[1] { "-" }, StringSplitOptions.None);
					text = array7[0];
					weightVar = -TreatWeight(array7[1]);
				}
				weight = (weightReal = TreatWeight(text));
				continue;
			case "question":
				question = ((flag && !text.StartsWith("shortcut")) ? TreatText(array[0]) : TreatText(text));
				continue;
			case "override_yes":
				override_yes = ((flag && !text.Equals("...")) ? TreatText(array[1]) : TreatText(text));
				continue;
			case "override_no":
				override_no = ((flag && !text.Equals("...")) ? TreatText(array[2]) : TreatText(text));
				continue;
			case "answer_yes":
				answer_yes = (flag ? TreatText(array[3]) : TreatText(text));
				continue;
			case "answer_no":
				answer_no = (flag ? TreatText(array[4]) : TreatText(text));
				continue;
			case "yes":
				yes_outcomes.AddRange(Outcome.TreatOutcome(text));
				continue;
			case "no":
				no_outcomes.AddRange(Outcome.TreatOutcome(text));
				continue;
			case "load":
				load_outcomes.AddRange(Outcome.TreatOutcome(text));
				continue;
			case "place":
				if (string.IsNullOrEmpty(text))
				{
					place = Backgrounds.defaut;
					continue;
				}
				try
				{
					place = (Backgrounds)Enum.Parse(typeof(Backgrounds), text);
				}
				catch
				{
					place = Backgrounds.defaut;
				}
				continue;
			case "place_name":
				place_name = text;
				continue;
			case "thematic":
				continue;
			}
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2) || text2.Contains("vide"))
			{
				continue;
			}
			Variables var = (Variables)Enum.Parse(typeof(Variables), text2);
			if (text == "pos")
			{
				yes_outcomes.Add(new Outcome(var, "1", DataDisplay.towards));
				no_outcomes.Add(new Outcome(var, "-1", DataDisplay.towards));
				continue;
			}
			if (text == "neg")
			{
				yes_outcomes.Add(new Outcome(var, "-1", DataDisplay.towards));
				no_outcomes.Add(new Outcome(var, "1", DataDisplay.towards));
				continue;
			}
			int result = 0;
			int result2 = 0;
			if (text.Length == 4)
			{
				int.TryParse(text.Substring(2, 2), out result);
				text = text.Substring(0, 2);
			}
			else if (text.Length == 3)
			{
				int.TryParse(text.Substring(1, 2), out result);
				text = text.Substring(0, 1);
				switch (text)
				{
				case "y":
					yes_outcomes.Add(new Outcome(var, result));
					continue;
				case "n":
					no_outcomes.Add(new Outcome(var, result));
					continue;
				case "l":
					load_outcomes.Add(new Outcome(var, result));
					continue;
				}
			}
			int.TryParse(text, out result2);
			if (result2 + result != 0)
			{
				yes_outcomes.Add(new Outcome(var, result2 + result));
			}
			if (-result2 + result != 0)
			{
				no_outcomes.Add(new Outcome(var, -result2 + result));
			}
		}
	}

	private int TreatWeight(string val)
	{
		if (val == "max")
		{
			return -1;
		}
		if (val == "prime")
		{
			return 100000000;
		}
		int.TryParse(val, out var result);
		return result;
	}

	public GText TreatText(string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			return new GText("");
		}
		return new GText(SpeechAct.diff.InitialFormat(val));
	}
}
