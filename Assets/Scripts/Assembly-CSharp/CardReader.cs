using System;
using System.Collections.Generic;
using UnityEngine;

public class CardReader : MonoBehaviour
{
	private int generatedId;

	private List<Bearer> authenticBearers;

	public List<Bearer> bearerModels;

	public List<BearerGen> bearerGenModels;

	private char dele = ';';

	public string nextname = "";

	private string currentname = "";

	public string previousname = "";

	public int previousid = -1;

	public static CardReader diff;

	private Dictionary<string, string> tempCards = new Dictionary<string, string>();

	private List<Card> cacheHiddenCards;

	private List<Card> cacheCards;

	private void Awake()
	{
		diff = this;
		// Helm keeps its cast in the editable campaign table instead of the
		// serialized recovery snapshot embedded in each legacy scene.
		UpdateListBearers();
		authenticBearers = new List<Bearer>(bearerModels);
	}

	private void OnEnable()
	{
		generatedId = DataStore.localSaveFileSystem.GetInt("generatedId");
	}

	private void OnDisable()
	{
		DataStore.localSaveFileSystem.SetInt("generatedId", generatedId);
	}

	public int GetGenerated()
	{
		generatedId++;
		return generatedId;
	}

	private string[] GetRawCards()
	{
		return (tempCards.ContainsKey("cards") ? tempCards["cards"] : Util.GetTextFile("texts/cards")).Split('\n');
	}

	public bool StartDownload()
	{
		return false;
	}

	public string GetTempText(string name)
	{
		if (tempCards.ContainsKey(name))
		{
			return tempCards[name];
		}
		return Util.GetTextFile("texts/" + name);
	}

	public List<Card> GetCards(bool hidden)
	{
		Dictionary<string, string[]> languageStrings = GetLanguageStrings(hidden);
		List<Card> list = new List<Card>();
		string[] rawCards = GetRawCards();
		string[] columns = rawCards[0].Split(dele);
		string[] array = rawCards[1].Split(dele);
		string[] array2 = new string[array.Length];
		array.CopyTo(array2, 0);
		for (int i = 1; i < rawCards.Length; i++)
		{
			previousname = currentname;
			if (array2[1] == "_")
			{
				currentname = "_" + array2[2];
			}
			else if (string.IsNullOrEmpty(array2[1]))
			{
				currentname = array2[2];
			}
			else
			{
				currentname = array2[1];
			}
			bool flag = array2[1].Length > 0 && array2[1].Substring(0, 1) == "_";
			if (i < rawCards.Length - 1)
			{
				array = rawCards[i + 1].Split(dele);
				if (string.IsNullOrEmpty(array[1]))
				{
					array[1] = array[2];
				}
				else if (array[1] == "_")
				{
					array[1] = "_" + array[2];
				}
				nextname = array[1];
			}
			else
			{
				nextname = "";
			}
			if ((hidden && flag) || (!hidden && !flag))
			{
				Card card = new Card(array2, columns, dele, languageStrings, previousid);
				previousid = card.id;
				list.Add(card);
			}
			array.CopyTo(array2, 0);
		}
		nextname = "";
		list.RemoveAll((Card it) => string.IsNullOrEmpty(it.question.mMsM));
		return list;
	}

	private Dictionary<string, string[]> GetLanguageStrings(bool hidden)
	{
		string lang = SpeechAct.diff.lang;
		if (lang == "en")
		{
			return new Dictionary<string, string[]>();
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		string[] array = (tempCards.ContainsKey("cards_i18n") ? tempCards["cards_i18n"] : Util.GetTextFile("texts/cards_i18n")).Split('\n');
		string[] array2 = array[0].Split(dele);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array3 = array[i].Split(dele);
			string[] array4 = new string[5];
			string key = "";
			for (int j = 0; j < array2.Length; j++)
			{
				string text = array2[j];
				if (text == "id")
				{
					key = array3[j];
				}
				else if (text == lang + "_question")
				{
					array4[0] = array3[j];
				}
				else if (text == lang + "_override_yes")
				{
					array4[1] = array3[j];
				}
				else if (text == lang + "_override_no")
				{
					array4[2] = array3[j];
				}
				else if (text == lang + "_answer_yes")
				{
					array4[3] = array3[j];
				}
				else if (text == lang + "_answer_no")
				{
					array4[4] = array3[j];
				}
			}
			dictionary.Add(key, array4);
		}
		return dictionary;
	}

	public void ResetSystemChara()
	{
		if (authenticBearers != null)
		{
			bearerModels = new List<Bearer>(authenticBearers);
		}
		foreach (Bearer bearerModel in bearerModels)
		{
			bearerModel.character.Remove(Bearers.antagonist);
		}
		foreach (Bearer item in bearerModels.FindAll((Bearer it) => it.type == BearerTypes.tag))
		{
			foreach (Bearer bearerModel2 in bearerModels)
			{
				bearerModel2.character.Remove(item.bearer);
			}
		}
	}

	public void AddCharacterToModels(Bearers charac, List<Bearers> tochange)
	{
		foreach (Bearers item in tochange)
		{
			AddCharacterToModel(charac, item);
		}
	}

	public void AddCharacterToModel(Bearers charac, Bearers be)
	{
		foreach (Bearer item in bearerModels.FindAll((Bearer it) => it.bearer == be || it.character.Contains(be)))
		{
			if (!item.character.Contains(charac))
			{
				item.character.Add(charac);
			}
		}
	}

	public void RemoveCharacterFromModel(Bearers charac, Bearers be)
	{
		foreach (Bearer item in bearerModels.FindAll((Bearer it) => it.bearer == be || it.character.Contains(be)))
		{
			if (item.character.Contains(charac))
			{
				item.character.Remove(charac);
			}
		}
	}

	public bool HasModelCharacter(Bearers target, Bearers charac)
	{
		if (bearerModels.Find((Bearer it) => it.bearer == target && it.character.Contains(charac)) != null)
		{
			return true;
		}
		return false;
	}

	private void Start()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnGameInit = (Action<GameSave>)Delegate.Combine(gameAct.OnGameInit, new Action<GameSave>(CheckChara));
	}

	private void CheckChara(GameSave ga)
	{
		CheckChara();
	}

	private void CheckChara()
	{
		if (SpeechAct.diff.lang != "en")
		{
			UpdateListBearers(withgen: false);
		}
	}

	private Dictionary<string, string[]> GetCharaLanguageStrings()
	{
		if (SpeechAct.diff == null)
		{
			return new Dictionary<string, string[]>();
		}
		string lang = SpeechAct.diff.lang;
		if (lang == "en")
		{
			return new Dictionary<string, string[]>();
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		string[] array = (tempCards.ContainsKey("characters_i18n") ? tempCards["characters_i18n"] : Util.GetTextFile("texts/characters_i18n")).Split('\n');
		string[] array2 = array[0].Split(dele);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array3 = array[i].Split(dele);
			string[] array4 = new string[2];
			string key = "";
			for (int j = 0; j < array2.Length; j++)
			{
				string text = array2[j];
				if (text == "id")
				{
					key = array3[j];
				}
				else if (text == lang + "_generated")
				{
					array4[0] = array3[j];
				}
				else if (text == lang + "_title")
				{
					array4[1] = array3[j];
				}
			}
			dictionary.Add(key, array4);
		}
		return dictionary;
	}

	public void UpdateListBearers(bool withgen = true)
	{
		authenticBearers = (bearerModels = new List<Bearer>());
		Dictionary<string, string[]> charaLanguageStrings = GetCharaLanguageStrings();
		bearerModels = new List<Bearer>();
		if (withgen)
		{
			bearerGenModels = new List<BearerGen>();
		}
		char[] array = new char[1] { ';' };
		string[] array2 = Util.GetTextFile("texts/characters").Split('\n');
		string[] columns = array2[0].Split(array, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < array2.Length; i++)
		{
			Bearer bearer = new Bearer(array2[i].Split(array), columns, array[0], charaLanguageStrings);
			bearerModels.Add(bearer);
			if (bearer.type == BearerTypes.generated && withgen)
			{
				bearerGenModels.Add(new BearerGen(bearer.bearer));
			}
		}
		authenticBearers = new List<Bearer>(bearerModels);
	}
}
