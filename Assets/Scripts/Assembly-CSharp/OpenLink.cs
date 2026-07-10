using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenLink : MonoBehaviour
{
	public GameObject loadDialog;

	private bool isunlocked;

	public GameObject quit;

	private DialogAct scDia;

	private void OnEnable()
	{
	}

	private void Awake()
	{
		scDia = loadDialog.GetComponent<DialogAct>();
	}

	private IEnumerator YieldReset()
	{
		yield return 0;
		Open();
		yield return new WaitForSeconds(5f);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.UnLock(ControlModes.resetmenu);
		}
		isunlocked = true;
	}

	public void Open()
	{
		loadDialog.SetActive(value: true);
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}

	public void Close()
	{
		if (GameAct.diff.state == GameStates.gameover)
		{
			if (isunlocked)
			{
				InputAct.diff.ResetGame();
			}
		}
		else
		{
			JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
			InputAct.diff.SetQuit(quit);
		}
	}

	public void Reset()
	{
		if ((bool)scDia)
		{
			scDia.gameObject.SetActive(value: true);
			scDia.Init("label_confirm", "action_confirm", ReConfirm);
		}
	}

	private void ReConfirm(bool valid)
	{
		if (valid)
		{
			scDia.gameObject.SetActive(value: true);
			scDia.Init("label_reconfirm", "yes", "no", ReReConfirm);
		}
		else
		{
			Disable();
		}
	}

	private void ReReConfirm(bool valid)
	{
		if (valid)
		{
			Disable();
			return;
		}
		scDia.gameObject.SetActive(value: true);
		scDia.Init("label_rereconfirm", "action_confirm", DoReset);
	}

	private void DoReset(bool valid)
	{
		if (valid)
		{
			int num = 0;
			if (DataStore.localSaveFileSystem.HasKey("slot"))
			{
				num = DataStore.localSaveFileSystem.GetInt("slot");
			}
			DataStore.Remove("GameSave" + num);
			DataStore.Remove("ResurrectSave" + num);
			PlayerPrefs.SetInt("justpassing", 1);
			DataStore.localSaveFileSystem.Save();
			SceneManager.LoadSceneAsync("disclaimer");
		}
		else
		{
			Disable();
		}
	}

	private void Disable()
	{
		InputAct.diff.DisableMenuNav(true);
	}
}
