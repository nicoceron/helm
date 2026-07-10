public class LocalSaveFileSystem
{
	private JSONSaveData_PlayerPrefsReplacer jsonSaveData = new JSONSaveData_PlayerPrefsReplacer();

	public void Initalization()
	{
		Initalization_impl();
	}

	public void DeleteAll()
	{
		DeleteAll_impl();
	}

	public void DeleteKey(string key)
	{
		DeleteKey_impl(key);
	}

	public float GetFloat(string key)
	{
		return GetFloat_impl(key);
	}

	public int GetInt(string key)
	{
		return GetInt_impl(key);
	}

	public string GetString(string key)
	{
		return GetString_impl(key);
	}

	public void SetFloat(string key, float value)
	{
		SetFloat_impl(key, value);
	}

	public void SetInt(string key, int value)
	{
		SetInt_impl(key, value);
	}

	public void SetString(string key, string value)
	{
		SetString_impl(key, value);
	}

	public bool HasKey(string key)
	{
		return HasKey_impl(key);
	}

	public void Save()
	{
		Save_impl();
	}

	public void Initalization_impl()
	{
		jsonSaveData.Initialization();
	}

	public void DeleteAll_impl()
	{
		jsonSaveData.DeleteAll();
	}

	public void DeleteKey_impl(string key)
	{
		jsonSaveData.DeleteKey(key);
	}

	public float GetFloat_impl(string key)
	{
		return jsonSaveData.GetFloat(key);
	}

	public int GetInt_impl(string key)
	{
		return jsonSaveData.GetInt(key);
	}

	public string GetString_impl(string key)
	{
		return jsonSaveData.GetString(key);
	}

	public void SetFloat_impl(string key, float value)
	{
		jsonSaveData.SetFloat(key, value);
	}

	public void SetInt_impl(string key, int value)
	{
		jsonSaveData.SetInt(key, value);
	}

	public void SetString_impl(string key, string value)
	{
		jsonSaveData.SetString(key, value);
	}

	public bool HasKey_impl(string key)
	{
		return jsonSaveData.HasKey(key);
	}

	public void Save_impl()
	{
		jsonSaveData.Save();
	}
}
