using System;
using System.Collections.Generic;
using ArabicSupport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpeechAct : MonoBehaviour
{
	public string lang = "en";

	private string[] langs = new string[15]
	{
		"ar", "sc", "tc", "ge", "du", "en", "sp", "fr", "ca", "it",
		"ja", "ko", "po", "ru", "tu"
	};

	[HideInInspector]
	private string[] langsDisplay = new string[15]
	{
		"عربى", "简体中文", "繁体中文", "Deutsch", "Dutch", "English", "Español", "Français", "Fr. Canada", "Italiano",
		"日本語", "한국어", "Português", "РУССКИЙ", "Türkçe"
	};

	public static SpeechAct diff;

	public Dictionary<string, Dictionary<string, List<string>>> PhasesTextes = new Dictionary<string, Dictionary<string, List<string>>>();

	public Dictionary<string, List<string>> UITexts = new Dictionary<string, List<string>>();

	public Dictionary<string, List<GText>> OtherTexts = new Dictionary<string, List<GText>>();

	public bool asiaLayout;

	public bool isMonarkMale = true;

	public bool isSelfMale = true;

	private string lasttext = "zozo";

	private string lastnam;

	private Dictionary<string, List<GText>> nameCache = new Dictionary<string, List<GText>>();

	private int cachevalue = 1;

	private void Awake()
	{
		CheckLang();
		if (lang == "ja" || lang == "sc" || lang == "tc")
		{
			asiaLayout = true;
		}
		diff = this;
		OtherTexts = TreatTexts();
		langsDisplay[0] = ArabicFixer.Fix(langsDisplay[0]);
	}

	public string InitialFormat(string text)
	{
		if (lang == "ar")
		{
			text = text.Replace("((", "</i>");
			text = text.Replace("))", "<i>");
			text = text.Replace("*>", "<color=#e2081e>");
			text = text.Replace("<*", "</color>");
			return text;
		}
		text = text.Replace("+", "\n");
		text = text.Replace("((", "<i>");
		text = text.Replace("))", "</i>");
		text = text.Replace("<*", "<color=#e2081e>");
		text = text.Replace("*>", "</color>");
		return text;
	}

	public string FinalFormat(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (lang == "ar")
		{
			text = text.Replace("<color=#e2081e>", "");
			text = text.Replace("</color>", "");
			string text2 = ArabicFixer.Fix(text, showTashkeel: false, useHinduNumbers: false);
			text2 = text2.Replace('<', '§');
			text2 = text2.Replace('>', '<');
			return lasttext = text2.Replace('§', '>');
		}
		return text;
	}

	public void CheckLang()
	{
		if (PlayerPrefs.HasKey("language"))
		{
			lang = PlayerPrefs.GetString("language");
			return;
		}
		string text = "";
		switch (Application.systemLanguage)
		{
		case SystemLanguage.English:
			lang = "en";
			break;
		case SystemLanguage.French:
			lang = (text.Contains("ca") ? "ca" : "fr");
			break;
		case SystemLanguage.Spanish:
			lang = "sp";
			break;
		case SystemLanguage.German:
			lang = "ge";
			break;
		case SystemLanguage.Arabic:
			lang = "ar";
			break;
		case SystemLanguage.Dutch:
			lang = "du";
			break;
		case SystemLanguage.Russian:
			lang = "ru";
			break;
		case SystemLanguage.Portuguese:
			lang = "po";
			break;
		case SystemLanguage.Turkish:
			lang = "tu";
			break;
		case SystemLanguage.Japanese:
			lang = "ja";
			break;
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
			lang = (text.Contains("Hant") ? "tc" : "sc");
			break;
		case SystemLanguage.ChineseTraditional:
			lang = "tc";
			break;
		case SystemLanguage.Italian:
			lang = "it";
			break;
		case SystemLanguage.Korean:
			lang = "ko";
			break;
		default:
			lang = "en";
			break;
		}
		PlayerPrefs.SetString("language", lang);
	}

	public void SetLang(string nlang)
	{
		lang = nlang;
		PlayerPrefs.SetString("language", nlang);
		asiaLayout = ((lang == "ja" || lang == "sc" || lang == "tc") ? true : false);
		OtherTexts = TreatTexts();
		DataStore.localSaveFileSystem.SetString("version", "changeloc");
		SceneManager.LoadScene("disclaimer");
	}

	private string[] GetFile(string file)
	{
		TextAsset textAsset = (TextAsset)Resources.Load("texts/" + lang + "/" + file, typeof(TextAsset));
		if (textAsset == null)
		{
			return new string[0];
		}
		return textAsset.text.Split('\n');
	}

	private string[] GetFile()
	{
		TextAsset textAsset = (TextAsset)Resources.Load("texts/" + lang, typeof(TextAsset));
		if (textAsset == null)
		{
			return new string[0];
		}
		return textAsset.text.Split('\n');
	}

	public string[] GetLangIds()
	{
		return langs;
	}

	public string[] GetLangDisp()
	{
		return langsDisplay;
	}

	public string GetBarname(string seed)
	{
		List<GText> list = OtherTexts["barname"];
		return list[Util.GetInt(seed, 0, list.Count)].Get();
	}

	public string GetName(Bearers bearer)
	{
		string text = "";
		List<GText> list = OtherTexts[bearer.ToString()];
		text = list[0].Get();
		if (text.Contains("<...>"))
		{
			int index = Util.RandInt(1, list.Count);
			text = text.Replace("<...>", list[index].Get());
		}
		return GenericName(text);
	}

	public string GenericName(string nam, string seed = "")
	{
		nam = FindAndReplace(nam, "first_male", seed);
		nam = FindAndReplace(nam, "first_female", seed);
		nam = FindAndReplace(nam, "last", seed);
		nam = FindAndReplace(nam, "place_1", seed);
		return nam;
	}

	private string FindAndReplace(string txt, string key, string seed = "")
	{
		bool flag = false;
		if (string.IsNullOrEmpty(seed))
		{
			flag = true;
			seed = Util.Rand().ToString();
		}
		seed += txt;
		if (!OtherTexts.ContainsKey(key))
		{
			return txt;
		}
		if (!nameCache.ContainsKey(key))
		{
			nameCache.Add(key, new List<GText>(OtherTexts[key]));
		}
		if (nameCache[key].Count == 0)
		{
			nameCache[key] = new List<GText>(OtherTexts[key]);
		}
		GText gText = (flag ? nameCache[key][Util.RandInt(0, nameCache[key].Count)] : OtherTexts[key][Util.GetInt(seed, 0, OtherTexts[key].Count)]);
		lastnam = gText.Get();
		nameCache[key].Remove(gText);
		if (string.IsNullOrEmpty(txt))
		{
			return null;
		}
		return txt.Replace("<" + key + ">", lastnam);
	}

	private string RandName(string key, string seed = "")
	{
		return OtherTexts[key][Util.GetInt(seed, 0, OtherTexts[key].Count)].Get();
	}

	public string GenerateName(string seed)
	{
		float num = Util.GetFloat(seed + "nbsyl");
		int num2 = ((num < 0.05f) ? 1 : ((num < 0.15f) ? 2 : 3));
		string text = "";
		for (int i = 0; i < num2; i++)
		{
			text += RandName("place_" + i, seed + "syl" + (float)i * (float)Math.PI);
		}
		return text;
	}

	public string JapanNum(string tid, int nb)
	{
		return GetSceneText(tid) + nb + GetSceneText(tid, 1);
	}

	private int SpanishNum(int nb)
	{
		string text = nb.ToString();
		string text2 = text.Substring(text.Length - 1, 1);
		if ((!(text2 == "1") && !(text2 == "3")) || nb == 11)
		{
			return 1;
		}
		return 0;
	}

	private int EnglishNum(int nb)
	{
		string text = nb.ToString();
		string text2 = text.Substring(text.Length - 1, 1);
		string text3 = ((text.Length > 1) ? text.Substring(text.Length - 2, 2) : "00");
		if (!(text2 == "1") || !(text3 != "11"))
		{
			if (!(text2 == "2") || !(text3 != "12"))
			{
				if (!(text2 == "3") || !(text3 != "13"))
				{
					return 3;
				}
				return 2;
			}
			return 1;
		}
		return 0;
	}

	private int RussianNum(int nb)
	{
		string text = nb.ToString();
		string text2 = text.Substring(text.Length - 1, 1);
		string text3 = ((text.Length > 1) ? text.Substring(text.Length - 2, 2) : "00");
		if (!(text2 == "1") || !(text3 != "11"))
		{
			if (!(text2 == "2") || !(text3 != "12"))
			{
				if (!(text2 == "3") || !(text3 != "13"))
				{
					if (!(text2 == "4") || !(text3 != "14"))
					{
						return 2;
					}
					return 1;
				}
				return 1;
			}
			return 1;
		}
		return 0;
	}

	public string GetEnumeral(int nb, bool withoutnb = false)
	{
		string text = nb.ToString();
		switch (lang)
		{
		case "ja":
		case "sc":
		case "tc":
			return GetSceneText("num") + text + GetSceneText("num", 1);
		case "fr":
			if (nb != 1)
			{
				if (!withoutnb)
				{
					return text + GetSceneText("num", 1);
				}
				return GetSceneText("num", 1);
			}
			if (!withoutnb)
			{
				return text + GetSceneText("num");
			}
			return GetSceneText("num");
		case "en":
			if (!withoutnb)
			{
				return text + GetSceneText("num", EnglishNum(nb));
			}
			return GetSceneText("num", EnglishNum(nb));
		case "es":
			if (!withoutnb)
			{
				return text + GetSceneText("num", SpanishNum(nb));
			}
			return GetSceneText("num", SpanishNum(nb));
		default:
			if (!withoutnb)
			{
				return text + GetSceneText("num");
			}
			return GetSceneText("num");
		}
	}

	private Dictionary<string, List<GText>> TreatTexts()
	{
		Dictionary<string, List<GText>> dictionary = new Dictionary<string, List<GText>>();
		string[] array = Util.GetTextFile("texts/all_i18n").Split('\n');
		string[] array2 = array[0].Split(';');
		int num = 1;
		int num2 = num;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].Contains(lang))
			{
				num2 = i;
			}
		}
		string text = "";
		List<GText> list = new List<GText>();
		for (int j = 1; j < array.Length; j++)
		{
			string[] array3 = array[j].Split(';');
			if (!string.IsNullOrEmpty(array3[0]))
			{
				if (text != "")
				{
					dictionary.Add(text, list);
				}
				text = array3[0];
				list = new List<GText>();
			}
			int num3 = num2;
			if (array3[num2].Length == 0 && lang != "en")
			{
				num3 = num;
			}
			string[] array4 = array3[num3].Split('|');
			if (array4.Length > 1)
			{
				string[] array5 = array4;
				foreach (string text2 in array5)
				{
					list.Add(new GText(InitialFormat(text2)));
				}
			}
			else
			{
				list.Add(new GText(InitialFormat(array3[num3])));
			}
		}
		return dictionary;
	}

	public string GetSceneNum(string name, int num)
	{
		if (!OtherTexts.ContainsKey(name))
		{
			return "";
		}
		int value = 0;
		if (lang == "ru" && OtherTexts[name].Count == 3)
		{
			value = RussianNum(num);
		}
		else if (num != 1 && OtherTexts[name].Count == 2)
		{
			value = 1;
		}
		return OtherTexts[name][Mathf.Clamp(value, 0, OtherTexts[name].Count)].Get();
	}

	public string GetSceneTextFinal(string name, int id = 0)
	{
		return FinalFormat(GetSceneText(name, id));
	}

	public string GetSceneText(string name, int id = 0)
	{
		string text = name;
		if (text != name && !OtherTexts.ContainsKey(name))
		{
			name = text;
		}
		if (!OtherTexts.ContainsKey(name))
		{
			return name;
		}
		if (asiaLayout)
		{
			return OtherTexts[name][id].Get().Replace("+", "\n");
		}
		return OtherTexts[name][Mathf.Clamp(id, 0, OtherTexts[name].Count)].Get();
	}

	public string GetSmartTextFinal(string name, int id = 0, int override_int = -1, string override_string = "")
	{
		return FinalFormat(GetSmartText(name, id, override_int, override_string));
	}

	public string GetSmartText(string name, int id = 0, int override_int = -1, string override_string = "")
	{
		string sceneText = GetSceneText(name, id);
		sceneText = Replace(Variables.money, override_int, sceneText);
		sceneText = Replace(Variables.journey, override_int, sceneText);
		sceneText = Replace(Variables.length, override_int, sceneText);
		sceneText = Replace(Variables.money, override_int, sceneText, num: true);
		sceneText = Replace(Variables.journey, override_int, sceneText, num: true);
		sceneText = Replace(Variables.length, override_int, sceneText, num: true);
		sceneText = Replace(Variables.nb_fame, override_int, sceneText);
		if (sceneText.Contains("<band>"))
		{
			sceneText = Replace("band", GameAct.diff.GetGroupName(), sceneText);
		}
		if (sceneText.Contains("<rank>"))
		{
			sceneText = Replace("rank", DeadCloneAct.diff.GetRank(), sceneText);
		}
		sceneText = (string.IsNullOrEmpty(override_string) ? Replace("nickname", ObjectiveAct.diff.nickname, sceneText) : Replace("nickname", override_string, sceneText));
		if (override_int > -1)
		{
			sceneText = Replace("number", override_int, sceneText);
		}
		if (sceneText.Contains("<num>"))
		{
			sceneText = ((override_int != -1) ? Replace("num", GetEnumeral(override_int, withoutnb: true), sceneText) : Replace("num", GetEnumeral(cachevalue, withoutnb: true), sceneText));
		}
		return sceneText;
	}

	private string Replace(Variables var, int amo, string text, bool num = false)
	{
		string text2 = (num ? ("<" + var.ToString() + "_num>") : ("<" + var.ToString() + ">"));
		if (!text.Contains(text2))
		{
			return text;
		}
		int nb = ((amo == -1) ? GameAct.diff.GetInt(var) : amo);
		text = ((!num) ? text.Replace(text2, nb.ToString()) : text.Replace(text2, GetEnumeral(nb)));
		cachevalue = nb;
		return text;
	}

	private string Replace(string var, int amo, string text)
	{
		string text2 = "<" + var + ">";
		if (!text.Contains(text2))
		{
			return text;
		}
		int num = ((amo > -1) ? amo : GameAct.diff.GetInt(var));
		text = text.Replace(text2, num.ToString());
		cachevalue = num;
		return text;
	}

	private string Replace(string var, string value, string text)
	{
		string text2 = "<" + var + ">";
		if (!text.Contains(text2))
		{
			return text;
		}
		text = text.Replace(text2, value);
		return text;
	}

	public List<string> GetSceneTexts(string name)
	{
		List<GText> list = OtherTexts[name];
		List<string> list2 = new List<string>();
		foreach (GText item in list)
		{
			if (asiaLayout)
			{
				list2.Add(item.Get().Replace("+", "\n"));
			}
			else
			{
				list2.Add(item.Get());
			}
		}
		return list2;
	}

	public Dictionary<string, List<string>> GetNonPersonalElements(string phase, string prefixe = "item_")
	{
		Dictionary<string, List<string>> dictionary = PhasesTextes[phase];
		Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<string>> item in dictionary)
		{
			if (item.Key.Length > prefixe.Length && item.Key.Substring(0, prefixe.Length) == prefixe && !item.Value.Contains(">personal"))
			{
				dictionary2.Add(item.Key.Substring(prefixe.Length, item.Key.Length - prefixe.Length), item.Value);
			}
		}
		return dictionary2;
	}

	public Dictionary<string, List<string>> GetElements(string phase, string prefixe = "item_")
	{
		Dictionary<string, List<string>> dictionary = PhasesTextes[phase];
		Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<string>> item in dictionary)
		{
			if (item.Key.Length > prefixe.Length && item.Key.Substring(0, prefixe.Length) == prefixe)
			{
				dictionary2.Add(item.Key.Substring(prefixe.Length, item.Key.Length - prefixe.Length), item.Value);
			}
		}
		return dictionary2;
	}

	public List<string> GetElementsById(string phase, string item, string prefixe = "item_")
	{
		Dictionary<string, List<string>> dictionary = PhasesTextes[phase];
		if (!dictionary.ContainsKey(prefixe + item))
		{
			return new List<string>();
		}
		return dictionary[prefixe + item];
	}
}
