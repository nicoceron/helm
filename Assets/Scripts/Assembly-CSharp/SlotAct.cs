using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlotAct : MonoBehaviour
{
	public GameObject[] cloudicons;

	private bool[] newgame = new bool[3];

	public Text[] slotText;

	public GameObject[] slotLabel;

	private float time;

	public DialogAct scDia;

	private int curSlot = -1;

	public void OnValidateSlot1()
	{
		SelectSlot(0);
	}

	public void OnValidateSlot2()
	{
		SelectSlot(1);
	}

	public void OnValidateSlot3()
	{
		SelectSlot(2);
	}

	private void SelectSlot(int id)
	{
		curSlot = id;
		if (newgame[id])
		{
			scDia.gameObject.SetActive(value: true);
			scDia.Init("label_new", "action_confirm", ValidNewGame);
			return;
		}
		int num = 0;
		if (DataStore.localSaveFileSystem.HasKey("slot"))
		{
			num = DataStore.localSaveFileSystem.GetInt("slot");
		}
		if (curSlot == num)
		{
			Disable();
			return;
		}
		DataStore.localSaveFileSystem.SetInt("slot", curSlot);
		PlayerPrefs.SetInt("justpassing", 1);
		DataStore.localSaveFileSystem.Save();
		SceneManager.LoadSceneAsync("disclaimer");
	}

	private void ValidNewGame(bool valid)
	{
		Disable();
		if (valid)
		{
			DataStore.localSaveFileSystem.SetInt("slot", curSlot);
			PlayerPrefs.SetInt("justpassing", 1);
			DataStore.localSaveFileSystem.Save();
			SceneManager.LoadSceneAsync("disclaimer");
		}
	}

	private void ValidExisting(bool valid)
	{
		Disable();
		if (curSlot != -1)
		{
			if (valid)
			{
				DataStore.localSaveFileSystem.SetInt("slot", curSlot);
				PlayerPrefs.SetInt("justpassing", 1);
				DataStore.localSaveFileSystem.Save();
				SceneManager.LoadSceneAsync("disclaimer");
			}
			else
			{
				scDia.gameObject.SetActive(value: true);
				scDia.Init("label_confirm", "action_confirm", ConfirmNewGame);
			}
		}
	}

	private void ConfirmNewGame(bool valid)
	{
		Disable();
		if (curSlot != -1 && valid)
		{
			DataStore.Remove("GameSave" + curSlot);
			DataStore.Remove("ResurrectSave" + curSlot);
			DataStore.localSaveFileSystem.SetInt("slot", curSlot);
			DataStore.localSaveFileSystem.Save();
			PlayerPrefs.SetInt("justpassing", 1);
			SceneManager.LoadSceneAsync("disclaimer");
		}
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
		InputAct.diff.DisableMenuNav(true);
	}

	public void OnEnable()
	{
		GameAct.diff.SaveGame();
		curSlot = -1;
		base.transform.SetAsLastSibling();
		time = Time.realtimeSinceStartup;
		if ((bool)CardReader.diff)
		{
			CardReader.diff.GetComponent<JourneyAct>().CloseWindows();
		}
		InputAct.diff.MenuNav();
		GameObject[] array = cloudicons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		DataStore.LoadGame(LoadGame1, local: true, 0);
		DataStore.LoadGame(LoadGame2, local: true, 1);
		DataStore.LoadGame(LoadGame3, local: true, 2);
		int num = 0;
		if (DataStore.localSaveFileSystem.HasKey("slot"))
		{
			num = DataStore.localSaveFileSystem.GetInt("slot");
		}
		slotLabel[num].SetActive(value: true);
	}

	public void LoadGame1(GameSave save)
	{
		if (save == null)
		{
			DataStore.LoadGame(LoadGameCloud1, local: false, 0);
		}
		else
		{
			InitButton(0, save);
		}
	}

	public void LoadGameCloud1(GameSave save)
	{
		InitButton(0, save, iscloud: true);
	}

	public void LoadGame2(GameSave save)
	{
		if (save == null)
		{
			DataStore.LoadGame(LoadGameCloud2, local: false, 1);
		}
		else
		{
			InitButton(1, save);
		}
	}

	public void LoadGameCloud2(GameSave save)
	{
		InitButton(1, save, iscloud: true);
	}

	public void LoadGame3(GameSave save)
	{
		if (save == null)
		{
			DataStore.LoadGame(LoadGameCloud3, local: false, 2);
		}
		else
		{
			InitButton(2, save);
		}
	}

	public void LoadGameCloud3(GameSave save)
	{
		InitButton(2, save, iscloud: true);
	}

	private void InitButton(int id, GameSave save, bool iscloud = false)
	{
		if (save == null)
		{
			newgame[id] = true;
			slotText[id].text = SpeechAct.diff.GetSceneTextFinal("action_reset");
			return;
		}
		if (iscloud)
		{
			cloudicons[id].SetActive(value: true);
		}
		newgame[id] = false;
		slotText[id].text = DataStore.FormatSave(save);
	}
}
