using System;
using System.Collections.Generic;

[Serializable]
public class Condition
{
	public Variables variable;

	public string custom_name;

	public Conditions condition;

	public int value;

	public Bearers bearer = Bearers.none;

	public Bearers bearerIsAlso = Bearers.none;

	public Bearers bearerIsNot = Bearers.none;

	public Backgrounds place = Backgrounds.none;

	public bool orlimit;

	public Condition()
	{
	}

	public Condition(string condition_string, bool or)
	{
		orlimit = or;
		bearer = ExtractBearer("!has_", condition_string);
		if (bearer != Bearers.none)
		{
			variable = Variables.set;
			condition = Conditions.notequal;
			return;
		}
		bearer = ExtractBearer("has_", condition_string);
		if (bearer != Bearers.none)
		{
			variable = Variables.set;
			condition = Conditions.equal;
			return;
		}
		bearer = ExtractBearer("!seen_", condition_string);
		if (bearer != Bearers.none)
		{
			variable = Variables.seen;
			condition = Conditions.notequal;
			return;
		}
		bearer = ExtractBearer("seen_", condition_string);
		if (bearer != Bearers.none)
		{
			variable = Variables.seen;
			condition = Conditions.equal;
			return;
		}
		bearer = ExtractBearer("", condition_string);
		if (bearer != Bearers.none)
		{
			variable = Variables.set;
			condition = Conditions.round;
			return;
		}
		int num = 0;
		if (condition_string.StartsWith(">"))
		{
			condition = Conditions.equal;
			variable = Variables.chain;
			string text = condition_string.Substring(1);
			if (text.Length > 0)
			{
				int.TryParse(text, out value);
				if (value == 0)
				{
					bearer = (Bearers)Enum.Parse(typeof(Bearers), text);
				}
			}
			else
			{
				value = CardReader.diff.previousid;
			}
			return;
		}
		if (condition_string.StartsWith("<"))
		{
			condition = Conditions.notequal;
			value = 1;
			variable = Variables.chain;
			string text2 = condition_string.Substring(1);
			if (text2.Length > 0)
			{
				try
				{
					int.TryParse(text2, out value);
					if (value == 0)
					{
						bearer = (Bearers)Enum.Parse(typeof(Bearers), text2);
					}
					return;
				}
				catch
				{
					int.TryParse(text2, out value);
					return;
				}
			}
			value = CardReader.diff.previousid;
			return;
		}
		if (condition_string.StartsWith("seen_"))
		{
			variable = Variables.seen;
			condition = Conditions.equal;
			int.TryParse(condition_string.Substring(5), out value);
			return;
		}
		if (condition_string.StartsWith("!seen_"))
		{
			variable = Variables.seen;
			condition = Conditions.notequal;
			int.TryParse(condition_string.Substring(6), out value);
			return;
		}
		string[] array;
		if (condition_string.Contains("<"))
		{
			condition = Conditions.below;
			array = condition_string.Split('<');
			num = -1;
		}
		else if (condition_string.Contains(">"))
		{
			condition = Conditions.above;
			array = condition_string.Split('>');
			num = 1;
		}
		else if (condition_string.Contains("%"))
		{
			condition = Conditions.round;
			array = condition_string.Split('%');
		}
		else if (condition_string.Contains("="))
		{
			condition = Conditions.equal;
			array = condition_string.Split('=');
		}
		else if (condition_string.StartsWith("!"))
		{
			condition = Conditions.equal;
			array = ((!condition_string.StartsWith("!nb_") && !condition_string.StartsWith("!inc_")) ? new string[2]
			{
				condition_string.Substring(1),
				"-1"
			} : new string[2]
			{
				condition_string.Substring(1),
				"0"
			});
		}
		else if (condition_string.StartsWith("nb_") || condition_string.StartsWith("inc_"))
		{
			condition = Conditions.above;
			array = new string[2] { condition_string, "0" };
			num = 1;
		}
		else
		{
			condition = Conditions.equal;
			array = new string[2] { condition_string, "1" };
		}
		if (array.Length == 2)
		{
			int.TryParse(array[1], out value);
		}
		value += num;
		if (condition_string.StartsWith("nav_"))
		{
			string text3 = "";
			if (array.Length == 0)
			{
				text3 = condition_string.Substring(4);
				condition = Conditions.equal;
				value = 0;
			}
			else
			{
				text3 = array[0].Substring(4);
			}
			int result = 0;
			int.TryParse(text3, out result);
			if (result > 0)
			{
				place = Backgrounds.defaut;
				custom_name = text3;
			}
			else
			{
				place = (Backgrounds)Enum.Parse(typeof(Backgrounds), text3);
			}
			return;
		}
		bearer = ExtractBearer("no_", array[0]);
		if (bearer != Bearers.none)
		{
			value = -value;
			condition = ((condition == Conditions.above) ? Conditions.below : ((condition != Conditions.below) ? condition : Conditions.above));
			return;
		}
		bearer = ExtractBearer("yes_", array[0]);
		if (bearer != Bearers.none)
		{
			return;
		}
		try
		{
			variable = (Variables)Enum.Parse(typeof(Variables), array[0]);
		}
		catch
		{
			if (array[0].Contains("_"))
			{
				string[] array2 = array[0].Split('_');
				try
				{
					variable = (Variables)Enum.Parse(typeof(Variables), array2[0]);
				}
				catch
				{
					variable = Variables.custom;
				}
			}
			else
			{
				variable = Variables.custom;
			}
			custom_name = array[0];
			if ((bool)GameAct.diff)
			{
				GameAct.diff.AddCustomVariable(custom_name);
			}
		}
	}

	public static List<Condition> TreatCondition(string val)
	{
		List<Condition> list = new List<Condition>();
		bool or = false;
		string[] array = val.Split(new string[1] { " or " }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 1)
		{
			or = true;
		}
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(new string[1] { " and " }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string condition_string in array3)
			{
				list.Add(new Condition(condition_string, or));
				or = false;
			}
			or = true;
		}
		return list;
	}

	private Bearers ExtractBearer(string suffixe, string texte)
	{
		if (!texte.StartsWith(suffixe) && !string.IsNullOrEmpty(suffixe))
		{
			return Bearers.none;
		}
		string text = texte.Substring(suffixe.Length);
		string text2 = "";
		string text3 = "";
		if (text.Contains("&"))
		{
			string[] array = text.Split(new string[1] { "&" }, StringSplitOptions.None);
			text = array[0];
			text2 = array[1];
		}
		if (text.Contains("!"))
		{
			string[] array2 = text.Split(new string[1] { "!" }, StringSplitOptions.None);
			text = array2[0];
			text3 = array2[1];
		}
		try
		{
			if (!string.IsNullOrEmpty(text2))
			{
				bearerIsAlso = (Bearers)Enum.Parse(typeof(Bearers), text2);
			}
			if (!string.IsNullOrEmpty(text3))
			{
				bearerIsNot = (Bearers)Enum.Parse(typeof(Bearers), text3);
			}
			int result = 0;
			int.TryParse(text, out result);
			if (result != 0)
			{
				return Bearers.none;
			}
			return (Bearers)Enum.Parse(typeof(Bearers), text);
		}
		catch
		{
			return Bearers.none;
		}
	}
}
