using System;
using System.Collections;
using System.Collections.Generic;
using SVGImporter;
using UnityEngine;

public class EffectAct : MonoBehaviour
{
	public Action<Effect> OnOpenEffect;

	public Action<Effect> OnCloseEffect;

	public WhoAct scWho;

	public MetersAct scMeters;

	private char[] dele = new char[1] { ';' };

	public List<Effect> effects;

	private GameObject icon;

	private List<string> types = new List<string>();

	private List<List<string>> levels = new List<List<string>>();

	public GameObject iconSlotPrefab;

	public Transform bottomBloc;

	private RectTransform[] backSlots;

	private int nbEffect;

	public RectTransform icorect;

	private List<string> effectTags = new List<string>();

	private Dictionary<Variables, Effect> effVal = new Dictionary<Variables, Effect>();

	private char[] latinScramble1 = new char[9] { 'a', 'u', 'e', 'p', 'm', 'n', 'h', 'z', 'k' };

	private char[] latinScramble2 = new char[9] { 'u', 'i', 'o', 'j', 'j', 'p', 'p', 'x', 'h' };

	private char[] japanScramble1 = new char[18]
	{
		'ラ', 'ム', '了', '王', 'ロ', 'ン', 'の', 'シ', '大', 'ド',
		'ギ', 'し', '中', '立', 'っ', 'べ', '士', 'こ'
	};

	private char[] japanScramble2 = new char[18]
	{
		'ベ', 'ロ', 'ト', 'ェ', 'ラ', 'レ', 'ク', 'ブ', 'シ', 'て',
		'央', 'い', '下', 'れ', 'イ', 'り', '手', 'わ'
	};

	private char[] russianScramble1 = new char[9] { 'и', 'о', 'к', 'з', 'л', 'ш', 'г', 'д', 'т' };

	private char[] russianScramble2 = new char[9] { 'а', 'у', 'н', 'н', 'я', 'ж', 'ж', 'ь', 'ц' };

	private void Start()
	{
		GameAct diff = GameAct.diff;
		diff.OnGameInit = (Action<GameSave>)Delegate.Combine(diff.OnGameInit, new Action<GameSave>(InitEffects));
		GameAct diff2 = GameAct.diff;
		diff2.OnUpdate = (Action<Card>)Delegate.Combine(diff2.OnUpdate, new Action<Card>(CheckEffects));
		GameAct diff3 = GameAct.diff;
		diff3.OnLanding = (Action)Delegate.Combine(diff3.OnLanding, new Action(ResetEffects));
		GameAct diff4 = GameAct.diff;
		diff4.OnJourneyEnd = (Action)Delegate.Combine(diff4.OnJourneyEnd, new Action(CheckEffects));
		GameAct diff5 = GameAct.diff;
		diff5.OnUpdateCards = (Action)Delegate.Combine(diff5.OnUpdateCards, new Action(UpdateEffects));
		MetersAct metersAct = scMeters;
		metersAct.OnShowOutcome = (Func<Outcome, int>)Delegate.Combine(metersAct.OnShowOutcome, new Func<Outcome, int>(AffectValue));
	}

	private void ResetEffects()
	{
		CheckEffects();
		effVal = new Dictionary<Variables, Effect>();
	}

	public Effect GetEffect(string id)
	{
		return effects.Find((Effect it) => it.tag == id);
	}

	public List<Effect> GetEffects()
	{
		Dictionary<string, string[]> languageStrings = GetLanguageStrings();
		List<Effect> list = new List<Effect>();
		string[] array = CardReader.diff.GetTempText("effects").Split('\n');
		string[] columns = array[0].Split(dele, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < array.Length; i++)
		{
			list.Add(new Effect(array[i].Split(dele), columns, languageStrings, this));
		}
		return list;
	}

	public int GetType(string typ)
	{
		if (typ == "special" || typ == "-1")
		{
			return -1;
		}
		if (types.Contains(typ))
		{
			return types.IndexOf(typ);
		}
		types.Add(typ);
		int result = types.Count - 1;
		levels.Add(new List<string>());
		return result;
	}

	public int GetLevel(string tag, int typ)
	{
		if (typ == -1)
		{
			return -1;
		}
		if (levels[typ].Contains(tag))
		{
			return levels[typ].IndexOf(tag);
		}
		levels[typ].Add(tag);
		return levels[typ].Count - 1;
	}

	public bool HasEffect(string tag)
	{
		if (effects.Find((Effect it) => it.tag == tag && it.active) == null)
		{
			return false;
		}
		return true;
	}

	public bool HasItemOrBetter_notin(string tag)
	{
		if (effects.Find((Effect it) => it.tag == tag) == null)
		{
			return false;
		}
		return true;
	}

	private void UpdateEffects()
	{
		types = new List<string>();
		levels = new List<List<string>>();
		List<Effect> neweff = GetEffects();
		foreach (Effect neff in neweff)
		{
			Effect effect = effects.Find((Effect it) => it.tag == neff.tag);
			if (effect == null)
			{
				effects.Add(neff);
				continue;
			}
			effect.description = neff.description;
			effect.outcomes = neff.outcomes;
			effect.title = neff.title;
			effect.alwayshowcard = neff.alwayshowcard;
		}
		effects.RemoveAll((Effect it) => neweff.Find((Effect yt) => yt.tag == it.tag) == null);
	}

	private Dictionary<string, string[]> GetLanguageStrings()
	{
		string lang = SpeechAct.diff.lang;
		if (lang == "en")
		{
			return new Dictionary<string, string[]>();
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		string[] array = Util.GetTextFile("texts/effects_i18n").Split('\n');
		string[] array2 = array[0].Split(dele);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array3 = array[i].Split(dele);
			string[] array4 = new string[5];
			string key = "";
			for (int j = 0; j < array2.Length; j++)
			{
				string text = array2[j];
				if (text == "tag")
				{
					key = array3[j];
				}
				else if (text == lang + "_title")
				{
					array4[0] = array3[j];
				}
				else if (text == lang + "_description")
				{
					array4[1] = array3[j];
				}
			}
			dictionary.Add(key, array4);
		}
		return dictionary;
	}

	private void SetListTags()
	{
		foreach (Effect effect in effects)
		{
			effectTags.Add(effect.tag);
		}
	}

	private void OpenEffect(Effect effect, float delay = 0f, bool single = false)
	{
		string text = effect.tag;
		if (text.EndsWith("_super"))
		{
			if (icon != null)
			{
				UnityEngine.Object.Destroy(icon);
			}
			GameObject gameObject = (icon = UnityEngine.Object.Instantiate(iconSlotPrefab));
			gameObject.GetComponent<SVGImage>().vectorGraphics = (SVGAsset)Resources.Load("effects/" + text, typeof(SVGAsset));
			gameObject.transform.SetParent(icorect.transform, worldPositionStays: false);
			StartCoroutine(ShowSlotIcon(effect, single, delay));
		}
		nbEffect++;
	}

	private int AffectValue(Outcome outco)
	{
		if (outco == null)
		{
			return 0;
		}
		Variables variable = outco.variable;
		if ((uint)(variable - 11) <= 3u)
		{
			if (effVal.ContainsKey(outco.variable) && (outco.variable != Variables.people || GameAct.diff.GetBool("storm")))
			{
				scMeters.SendEffect(outco.variable, effVal[outco.variable]);
				if (outco.value >= 0)
				{
					return outco.value + 3;
				}
				return outco.value - 3;
			}
			return outco.value;
		}
		return 0;
	}

	private IEnumerator ShowSlotIcon(Effect effect, bool single, float delay)
	{
		float t = 0f;
		while (t < 1f)
		{
			icorect.localScale = Vector3.one * (1.5f - 0.5f * t);
			t += Time.deltaTime * 3f;
			yield return null;
		}
		icorect.localScale = Vector3.one;
	}

	private void InitEffects(GameSave save)
	{
		effects = GetEffects();
		SetListTags();
	}

	public bool HasLimit(Card card)
	{
		if (card.yes_outcomes.Find((Outcome it) => effectTags.Contains(it.custom_name) && it.value == 1) != null || card.no_outcomes.Find((Outcome it) => effectTags.Contains(it.custom_name) && it.value == 1) != null)
		{
			return true;
		}
		return false;
	}

	private void CheckEffects()
	{
		CheckEffects(null);
	}

	private void CheckEffects(Card card)
	{
		List<Effect> list = new List<Effect>();
		List<Effect> list2 = new List<Effect>();
		foreach (Effect effect in effects)
		{
			if (effect.active)
			{
				if (!GameAct.diff.Has(effect.tag))
				{
					list2.Add(effect);
				}
			}
			else if (GameAct.diff.Has(effect.tag))
			{
				list.Add(effect);
			}
		}
		foreach (Effect item in list2)
		{
			CloseEffect(item);
		}
		foreach (Effect item2 in list)
		{
			item2.active = true;
			OpenEffect(item2, 0f, single: true);
			if (item2.alwayshowcard || !item2.seen)
			{
				GameAct.diff.AddEffectCard(item2);
			}
			item2.seen = true;
		}
	}

	public void CloseEffect(Effect obj)
	{
		string var = obj.tag;
		GameAct.diff.SetInt(var, -1);
		obj.active = false;
		nbEffect--;
		if (obj.tag.EndsWith("_super") && icon != null)
		{
			UnityEngine.Object.Destroy(icon);
		}
	}

	private void CheckChurch(Bearers bearer)
	{
	}

	private string ScrambleQuestion(string question)
	{
		int length = question.Length;
		if (length > 7 && (question.Substring(0, 3) == "<i>" || question.Substring(length - 4, 4) == "</i>"))
		{
			return question;
		}
		char[] array = ((SpeechAct.diff.lang == "jp") ? japanScramble1 : ((SpeechAct.diff.lang == "ru") ? russianScramble1 : latinScramble1));
		char[] array2 = ((SpeechAct.diff.lang == "jp") ? japanScramble2 : ((SpeechAct.diff.lang == "ru") ? russianScramble2 : latinScramble2));
		for (int i = 0; i < array.Length; i++)
		{
			question = SwitchChar(question, array[i], array2[i]);
		}
		if (SpeechAct.diff.asiaLayout || SpeechAct.diff.lang == "ko")
		{
			List<char> list = new List<char> { '<', 'i', '>', '/', '+' };
			for (int j = 0; j < 7; j++)
			{
				if (question.Length > 6)
				{
					int num = Util.RandInt(0, question.Length - 1);
					if (!list.Contains(question[num]))
					{
						question = question.Remove(num, 1);
					}
				}
			}
		}
		return question;
	}

	private string SwitchChar(string source, char ch, char byc)
	{
		if (Util.Rand() > 0.5f)
		{
			return source;
		}
		source = source.Replace(ch, byc);
		return source;
	}

	public void OpenSun()
	{
		StartCoroutine(SunSlide(0f, 0.8f));
	}

	private IEnumerator SunSlide(float start, float end)
	{
		yield break;
	}

	private string EyeCurse(string str)
	{
		return str;
	}

	private bool PreVisit(int dec)
	{
		return false;
	}

	private bool PreCurse(int dec)
	{
		return true;
	}

	private void InitCurse()
	{
	}

	private void RemoveIconSlot(Effect eff)
	{
	}

	public List<Effect> GetLiveEffects()
	{
		return effects.FindAll((Effect it) => it.active);
	}

	public bool OpenAndSelectBut()
	{
		return true;
	}

	public void CloseBut()
	{
	}
}
