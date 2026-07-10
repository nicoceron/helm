using System;
using System.Collections.Generic;

[Serializable]
public class Outcome
{
	public Variables variable;

	public string custom_name;

	public int value;

	public bool orlimit;

	public DataDisplay display;

	public Bearers bearer = Bearers.none;

	public Outcome()
	{
	}

	public Outcome(Outcome outco)
	{
		variable = outco.variable;
		custom_name = outco.custom_name;
		value = outco.value;
		orlimit = outco.orlimit;
		display = outco.display;
		bearer = outco.bearer;
	}

	public Outcome(Variables var, int val)
	{
		variable = var;
		value = val;
	}

	public Outcome(Variables var, string val, DataDisplay disp = DataDisplay.none)
	{
		variable = var;
		int.TryParse(val, out value);
		display = disp;
	}

	public Outcome(string var, string val, bool or = false)
	{
		orlimit = or;
		try
		{
			variable = (Variables)Enum.Parse(typeof(Variables), var);
			if ((bool)GameAct.diff)
			{
				GameAct.diff.AddKnownVariable(variable);
			}
		}
		catch
		{
			variable = Variables.custom;
			custom_name = var;
			if ((bool)GameAct.diff)
			{
				GameAct.diff.AddCustomVariable(custom_name);
			}
		}
		if (int.TryParse(val, out value))
		{
			return;
		}
		if (val == "lock")
		{
			display = DataDisplay.locked;
			value = 1;
		}
		else if (val.Contains("?"))
		{
			string[] array = val.Split(new string[1] { "?" }, StringSplitOptions.RemoveEmptyEntries);
			int num = ExtractInt(array[0]);
			if (array.Length > 1)
			{
				int num2 = ExtractInt(array[1]);
				value = ((num > num2) ? Util.RandInt(num2, num) : Util.RandInt(num, num2));
			}
			else
			{
				value = num;
			}
			display = DataDisplay.hidden;
		}
		else
		{
			string[] array2 = val.Split(new string[1] { "*" }, StringSplitOptions.RemoveEmptyEntries);
			value = ExtractInt(array2[0]);
			display = DataDisplay.moving;
		}
	}

	private int ExtractInt(string val)
	{
		int.TryParse(val, out var result);
		return result;
	}

	public Outcome(string var, bool or = false)
	{
		string s = "";
		orlimit = or;
		if (var.StartsWith(">"))
		{
			int num = -1;
			while (var.Contains(">"))
			{
				var = var.Substring(1, var.Length - 1);
				num++;
			}
			value = num;
			custom_name = var;
			if (custom_name == "")
			{
				custom_name = CardReader.diff.nextname;
			}
			variable = Variables.chain;
			return;
		}
		if (var.Contains("+"))
		{
			bool flag = true;
			for (int i = 1; i < var.Length - 2; i++)
			{
				if (var.Substring(var.Length - i, 1) != "+")
				{
					if (flag)
					{
						string[] array = var.Split('+');
						var = array[0];
						s = array[1];
					}
					else
					{
						s = (i - 1).ToString();
						var = var.Substring(0, var.Length - i + 1);
					}
					break;
				}
				flag = false;
			}
		}
		else if (var.Contains("-"))
		{
			bool flag2 = true;
			for (int j = 1; j < var.Length - 2; j++)
			{
				if (var.Substring(var.Length - j, 1) != "-")
				{
					if (flag2)
					{
						string[] array2 = var.Split('-');
						var = array2[0];
						s = "-" + array2[1];
					}
					else
					{
						s = "-" + (j - 1);
						var = var.Substring(0, var.Length - j + 1);
					}
					break;
				}
				flag2 = false;
			}
		}
		else if (var.Contains("="))
		{
			string[] array3 = var.Split('=');
			var = array3[0];
			s = array3[1];
			display = DataDisplay.fullamount;
		}
		else if (var.StartsWith("!"))
		{
			s = ((!var.StartsWith("!nb_") && !var.StartsWith("!inc_")) ? "-1" : "0");
			var = var.Substring(1);
			display = DataDisplay.fullamount;
		}
		else
		{
			if (var.StartsWith("add_"))
			{
				bearer = (Bearers)Enum.Parse(typeof(Bearers), var.Substring(4));
				variable = Variables.add;
				return;
			}
			if (var.StartsWith("nav_") || var.StartsWith("set_"))
			{
				string[] array4 = var.Substring(4).Split('_');
				if (array4.Length > 1)
				{
					int.TryParse(array4[1], out value);
				}
				else
				{
					value = 4;
				}
				custom_name = array4[0];
				if (var.StartsWith("set_"))
				{
					bearer = Bearers.map;
				}
				variable = Variables.set;
				return;
			}
			if (var.Contains(">!"))
			{
				string[] array5 = var.Split(new string[1] { ">!" }, StringSplitOptions.None);
				bearer = (Bearers)Enum.Parse(typeof(Bearers), array5[1]);
				custom_name = array5[0];
				variable = Variables.remove;
				return;
			}
			if (var.Contains(">"))
			{
				string[] array6 = var.Split('>');
				bearer = (Bearers)Enum.Parse(typeof(Bearers), array6[1]);
				custom_name = array6[0];
				variable = Variables.set;
				return;
			}
			if (var.StartsWith("kill_"))
			{
				try
				{
					bearer = (Bearers)Enum.Parse(typeof(Bearers), var.Substring(5));
					variable = Variables.destroy;
					return;
				}
				catch
				{
					return;
				}
			}
			if (var.StartsWith("del_"))
			{
				try
				{
					bearer = (Bearers)Enum.Parse(typeof(Bearers), var.Substring(4));
					variable = Variables.remove;
					return;
				}
				catch
				{
					s = "-1";
					var = var.Substring(4);
					return;
				}
			}
			if (var.StartsWith("mus_") || var.StartsWith("sfx_") || var.StartsWith("eff_"))
			{
				custom_name = var;
				variable = Variables.custom;
				return;
			}
			if (var == "destroy")
			{
				variable = Variables.destroy;
				return;
			}
			s = "1";
			display = DataDisplay.fullamount;
		}
		try
		{
			variable = (Variables)Enum.Parse(typeof(Variables), var);
			if ((bool)GameAct.diff)
			{
				GameAct.diff.AddKnownVariable(variable);
			}
		}
		catch
		{
			if (var.Contains("_"))
			{
				string[] array7 = var.Split('_');
				try
				{
					variable = (Variables)Enum.Parse(typeof(Variables), array7[0]);
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
			custom_name = var;
			if ((bool)GameAct.diff)
			{
				GameAct.diff.AddCustomVariable(custom_name);
			}
		}
		int.TryParse(s, out value);
	}

	public static List<Outcome> TreatOutcome(string val)
	{
		List<Outcome> list = new List<Outcome>();
		bool or = false;
		string[] array = val.Split(new string[1] { " or " }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new string[1] { " and " }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string var in array2)
			{
				list.Add(new Outcome(var, or));
				or = false;
			}
			or = true;
		}
		return list;
	}
}
