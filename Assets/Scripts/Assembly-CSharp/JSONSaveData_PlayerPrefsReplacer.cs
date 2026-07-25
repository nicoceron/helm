using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class JSONSaveData_PlayerPrefsReplacer
{
	[Serializable]
	public class JSONPlayerPrefItem
	{
		public string key;

		public string value;

		public JSONPlayerPrefItem(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}

	[SerializeField]
	private List<JSONPlayerPrefItem> playerPrefItems = new List<JSONPlayerPrefItem>();

	private const string testSavePath = "D:\\ReignsBeyond";

	private const string SaveFolder = "Saves";

	private const string saveFileName = "save.sav";

	private const string WebSaveKey = "helm_browser_save";

	private static string SavePath = string.Empty;

	private string SaveRoot = Application.persistentDataPath;

	public void Initialization()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		string browserSave = PlayerPrefs.GetString(WebSaveKey, string.Empty);
		if (!string.IsNullOrEmpty(browserSave))
		{
			JSONSaveData_PlayerPrefsReplacer saved = JsonUtility.FromJson<JSONSaveData_PlayerPrefsReplacer>(Compressor.DecompressString(browserSave));
			if (saved != null && saved.playerPrefItems != null)
			{
				playerPrefItems = saved.playerPrefItems;
			}
		}
		return;
#else
		// A macOS application bundle is not a save-data location. Keeping this in
		// Application.dataPath made development saves part of the next player build
		// and fails outright once the installed app is read-only.
		SaveRoot = Application.persistentDataPath;
		SavePath = Path.Combine(SaveRoot, "Saves", "save.sav");
		if (File.Exists(SavePath))
		{
			string text = File.ReadAllText(SavePath);
			if (text.Length > 0)
			{
				Debug.Log("We have file string");
			}
			else
			{
				Debug.LogError("File read went bad.");
			}
			JSONSaveData_PlayerPrefsReplacer jSONSaveData_PlayerPrefsReplacer = JsonUtility.FromJson<JSONSaveData_PlayerPrefsReplacer>(Compressor.DecompressString(text));
			playerPrefItems = jSONSaveData_PlayerPrefsReplacer.playerPrefItems;
		}
#endif
	}

	public void DeleteAll()
	{
		playerPrefItems.Clear();
	}

	public void DeleteKey(string key)
	{
		if (TryToGetPrefItem(key, out var item))
		{
			playerPrefItems.Remove(item);
		}
		else
		{
			Debug.LogError("Key " + key + " not found in JSON Save data");
		}
	}

	public float GetFloat(string key)
	{
		if (TryToGetPrefItem(key, out var item))
		{
			if (float.TryParse(item.value, out var result))
			{
				return result;
			}
			Debug.LogError("Float for " + key + " was unable to be parsed");
			return 0f;
		}
		Debug.LogError("Float for " + key + " not found");
		return 0f;
	}

	public int GetInt(string key)
	{
		if (TryToGetPrefItem(key, out var item))
		{
			if (int.TryParse(item.value, out var result))
			{
				return result;
			}
			Debug.LogError("Int for " + key + " was unable to be parsed");
			return 0;
		}
		Debug.LogError("Int for " + key + " not found");
		return 0;
	}

	public string GetString(string key)
	{
		if (TryToGetPrefItem(key, out var item))
		{
			return item.value;
		}
		Debug.LogError("String for " + key + " not found");
		return string.Empty;
	}

	public void SetFloat(string key, float value)
	{
		SetPrefItem(key, value.ToString());
	}

	public void SetInt(string key, int value)
	{
		SetPrefItem(key, value.ToString());
	}

	public void SetString(string key, string value)
	{
		SetPrefItem(key, value);
	}

	public bool HasKey(string key)
	{
		foreach (JSONPlayerPrefItem playerPrefItem in playerPrefItems)
		{
			if (playerPrefItem.key == key)
			{
				return true;
			}
		}
		return false;
	}

	private void SetPrefItem(string key, string value)
	{
		if (TryToGetPrefItem(key, out var item))
		{
			item.value = value;
			return;
		}
		item = new JSONPlayerPrefItem(key, value);
		playerPrefItems.Add(item);
	}

	private bool TryToGetPrefItem(string key, out JSONPlayerPrefItem item)
	{
		foreach (JSONPlayerPrefItem playerPrefItem in playerPrefItems)
		{
			if (playerPrefItem.key == key)
			{
				item = playerPrefItem;
				return true;
			}
		}
		item = null;
		return false;
	}

	public void Save()
	{
		string s = Compressor.CompressString(JsonUtility.ToJson(this));
#if UNITY_WEBGL && !UNITY_EDITOR
		PlayerPrefs.SetString(WebSaveKey, s);
		PlayerPrefs.Save();
#else
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string path = Path.Combine(SaveRoot, "Saves");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		File.WriteAllBytes(SavePath, bytes);
		Debug.Log("We have written!");
#endif
	}
}
