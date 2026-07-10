using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageOption : MonoBehaviour
{
	public GameObject applyBut;

	public Dropdown dropLang;

	private int langid = -1;

	private string[] ids;

	private void OnEnable()
	{
		dropLang.ClearOptions();
		List<string> options = new List<string>(SpeechAct.diff.GetLangDisp());
		dropLang.AddOptions(options);
		ids = SpeechAct.diff.GetLangIds();
		for (int i = 0; i < ids.Length; i++)
		{
			if (ids[i] == SpeechAct.diff.lang)
			{
				langid = i;
				dropLang.value = i;
			}
		}
	}

	public void ChangeLang()
	{
		if (dropLang.value == langid)
		{
			applyBut.SetActive(value: false);
		}
		else
		{
			applyBut.SetActive(value: true);
		}
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}

	public void ApplyLang()
	{
		if (dropLang.value != langid)
		{
			SpeechAct.diff.SetLang(ids[dropLang.value]);
		}
		applyBut.SetActive(value: false);
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}
}
