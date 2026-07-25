using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataStore : MonoBehaviour
{
	public static LocalSaveFileSystem localSaveFileSystem = new LocalSaveFileSystem();

	private void Awake()
	{
#if !UNITY_WEBGL
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
	}

	public static void LoadGame(Action<GameSave> onloadGame, bool local = false, int overrideslot = -1)
	{
		Load("GameSave" + ((overrideslot > -1) ? overrideslot : (localSaveFileSystem.HasKey("slot") ? localSaveFileSystem.GetInt("slot") : 0)), onloadGame, local);
	}

	public static void LoadOverall(Action<OverallSave> onloadOverall, bool local = false)
	{
		Load("OverallSave", onloadOverall, local);
	}

	public static void Gameover(GameAct gameact)
	{
		GameSave gameSave = new GameSave(gameact, withresurrect: false);
		gameSave.bearers = new List<BearerSave>();
		gameSave.cards = new CardSave[0];
		gameSave.datacustom = new List<DataCustom>();
		gameSave.datavar = new List<DataVariable>();
		gameSave.goals = new List<NavPoint>();
		gameSave.goaltoremove = -1;
		gameSave.journeys = new List<JourneySave>();
		gameSave.navigation = new List<NavPoint>();
		gameSave.nickname = SpeechAct.diff.GetSceneTextFinal("defaultnick");
		gameSave.objectives = new List<ObjectiveSave>();
		gameSave.place = Backgrounds.defaut;
		gameSave.place_name = "SectorAlpha";
		gameSave.place_cache = new List<string>();
		gameSave.currentCard = 1;
		gameSave.postponeEvents = new List<PostponeEvent>();
		gameSave.ressurectCard = 1;
		RemoveSlot("ResurrectSave");
		SaveSlot("GameSave", gameSave);
		SceneManager.LoadSceneAsync("disclaimer");
	}

	public static string FormatSave(GameSave save)
	{
		if (save == null)
		{
			return null;
		}
		string sceneText = SpeechAct.diff.GetSceneText("objective_stats", 1);
		List<ObjectiveSave> list = save.objectives.FindAll((ObjectiveSave it) => it.fulfilled);
		sceneText = sceneText.Replace("<number>", list.Count.ToString());
		sceneText = sceneText.Replace("<total>", save.objectives.Count.ToString());
		DateTime dateTime = new DateTime(save.time, DateTimeKind.Utc).ToLocalTime();
		string text = dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString();
		return SpeechAct.diff.FinalFormat(save.nickname + " * " + sceneText + "\n" + save.device + " " + text);
	}

	public static string Prepare<T>(T instance)
	{
		if (instance == null)
		{
			return "";
		}
		return JsonUtility.ToJson(instance);
	}

	public static void Reset(bool all = false, bool nosave = false)
	{
		Remove("GameSave0");
		Remove("GameSave1");
		Remove("GameSave2");
		Remove("ResurrectSave0");
		Remove("ResurrectSave1");
		Remove("ResurrectSave2");
		localSaveFileSystem.DeleteKey("slot");
	}

	public static void Remove(string name)
	{
		SuperPrefs.DeleteKey(name);
		localSaveFileSystem.DeleteKey(name + "_local");
	}

	public static void RemoveSlot(string name)
	{
		int num = (localSaveFileSystem.HasKey("slot") ? localSaveFileSystem.GetInt("slot") : 0);
		SuperPrefs.DeleteKey(name + num);
		localSaveFileSystem.DeleteKey(name + num + "_local");
	}

	public static void Save(string name, string _json, bool onlylocal = false)
	{
		string value = Compressor.CompressString(_json);
		if (!onlylocal)
		{
			SuperPrefs.SetString(name, value);
		}
		localSaveFileSystem.SetString(name + "_local", value);
		localSaveFileSystem.Save();
	}

	public static void SaveSlot(string name, string _json, bool onlylocal = false)
	{
		if (name != null && _json != null)
		{
			name += (localSaveFileSystem.HasKey("slot") ? localSaveFileSystem.GetInt("slot") : 0);
			string value = Compressor.CompressString(_json);
			if (!onlylocal)
			{
				SuperPrefs.SetString(name, value);
			}
			localSaveFileSystem.SetString(name + "_local", value);
			localSaveFileSystem.Save();
		}
	}

	public static void Save<T>(string name, T instance, bool onlylocal = false)
	{
		if (instance != null)
		{
			Save(name, Prepare(instance), onlylocal);
		}
	}

	public static void SaveSlot<T>(string name, T instance, bool onlylocal = false)
	{
		if (instance != null)
		{
			SaveSlot(name, Prepare(instance), onlylocal);
		}
	}

	public static bool HasFile(string name)
	{
		if (SuperPrefs.HasKey(name))
		{
			return true;
		}
		return localSaveFileSystem.HasKey(name + "_local");
	}

	public static string GetJson(string name, bool local = false)
	{
		string text = (local ? PlayerPrefs.GetString(name + "_local") : SuperPrefs.GetString(name));
		if (string.IsNullOrEmpty(text) && !local)
		{
			text = PlayerPrefs.GetString(name + "_local");
		}
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		return Compressor.DecompressString(text);
	}

	public static void LoadJson<T>(string _json, Action<T> completedCallback)
	{
		if (string.IsNullOrEmpty(_json))
		{
			completedCallback(default(T));
			return;
		}
		T obj = JsonUtility.FromJson<T>(_json);
		completedCallback(obj);
	}

	public static void Load<T>(string name, Action<T> completedCallback, bool local = false)
	{
		string text = (local ? localSaveFileSystem.GetString(name + "_local") : SuperPrefs.GetString(name));
		if (string.IsNullOrEmpty(text))
		{
			completedCallback(default(T));
			return;
		}
		T obj = JsonUtility.FromJson<T>(Compressor.DecompressString(text));
		completedCallback(obj);
	}

	public static void LoadSlot<T>(string name, Action<T> completedCallback, bool local = false)
	{
		int num = (localSaveFileSystem.HasKey("slot") ? localSaveFileSystem.GetInt("slot") : 0);
		string text = (local ? localSaveFileSystem.GetString(name + num + "_local") : SuperPrefs.GetString(name + num));
		if (string.IsNullOrEmpty(text))
		{
			completedCallback(default(T));
			return;
		}
		T obj = JsonUtility.FromJson<T>(Compressor.DecompressString(text));
		completedCallback(obj);
	}
}
