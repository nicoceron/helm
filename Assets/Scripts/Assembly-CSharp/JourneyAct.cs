using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JourneyAct : MonoBehaviour
{
	private ObjectiveAct scObj;

	private EffectAct scEff;

	public Bearer monarch;

	private char dele = ';';

	public GameObject statBloc;

	public GameObject effectsBloc;

	public GameObject objectivesBloc;

	public GameObject optionsBloc;

	public Text[] achieveTxts;

	public Slider[] achieveSliders;

	public Text[] scoreJourneyTxts;

	public Text[] scoreLengthTxts;

	public GameObject[] newSigns;

	private List<Achievement> achieves = new List<Achievement>();

	public Transform allobjBoxInStat;

	private List<Achievement> curAchieve = new List<Achievement>();

	public GameObject DiaPrefab;

	public Transform canvas;

	private void Awake()
	{
		scObj = GetComponent<ObjectiveAct>();
		scEff = GetComponent<EffectAct>();
	}

	private void ClearWindows(bool openmenu = false)
	{
		InputAct.diff.SuspendSlideFocus();
		StopAllCoroutines();
		optionsBloc.SetActive(value: false);
		statBloc.SetActive(value: false);
		effectsBloc.SetActive(value: false);
		if (openmenu)
		{
			JukeBox.diff.PlaySound(SFXTypes.ui_menu_open);
		}
		else
		{
			JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
		}
	}

	public void CloseWindows()
	{
		InputAct.diff.RestoreSlideFocus();
		optionsBloc.SetActive(value: false);
		statBloc.SetActive(value: false);
		effectsBloc.SetActive(value: false);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.ResetLock();
		}
		JukeBox.diff.PlaySound(SFXTypes.ui_menu_close);
	}

	public void OpenStats(bool openmenu = false)
	{
		ClearWindows(openmenu);
		statBloc.SetActive(value: true);
		UpdateStats();
	}

	public List<Achievement> GetAchieves(AchieveTypes type)
	{
		if (curAchieve != null && curAchieve.Count > 0)
		{
			return curAchieve.FindAll((Achievement it) => it.type == type);
		}
		return new List<Achievement>();
	}

	public void AddAchieve(string nam, AchieveTypes typ)
	{
		achieves.Add(new Achievement(nam, typ));
	}

	public void OpenObjectives()
	{
		statBloc.SetActive(value: false);
		objectivesBloc.SetActive(value: true);
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}

	public void OpenBearers()
	{
		statBloc.SetActive(value: false);
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}

	public void OpenDeaths()
	{
		statBloc.SetActive(value: false);
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}

	public void CloseStats()
	{
		InputAct.diff.RestoreSlideFocus();
		statBloc.SetActive(value: false);
	}

	public void CloseBlocs()
	{
		objectivesBloc.SetActive(value: false);
		OpenStats();
	}

	public void CloseMore()
	{
		OpenOptions(openmenu: false);
	}

	public bool IsInSubMenu()
	{
		if (!objectivesBloc.activeSelf)
		{
			return false;
		}
		return true;
	}

	private void UpdateStats()
	{
		float num = GameAct.diff.timespent / 60f;
		float num2 = Mathf.Floor(num / 60f);
		Mathf.Round(num - num2 * 60f);
		SetAchieve("objective_stats", 0, scObj.GetFulfilled().Count, scObj.GetAll().Count);
		foreach (Transform item in allobjBoxInStat)
		{
			Object.Destroy(item.gameObject);
		}
		scObj.ShowObjectives(allobjBoxInStat, 0, replace: false);
		List<JourneySave> rankedJourneys = DeadCloneAct.diff.GetRankedJourneys();
		JourneySave currentJourney = DeadCloneAct.diff.GetCurrentJourney();
		StopAllCoroutines();
		for (int i = 0; i < 4 && rankedJourneys.Count > i; i++)
		{
			JourneySave journeySave = rankedJourneys[i];
			bool num3 = journeySave.cloneNumber == currentJourney.cloneNumber;
			string smartTextFinal = SpeechAct.diff.GetSmartTextFinal("highscore_label", 0, journeySave.cloneNumber, journeySave.cloneNick);
			string smartTextFinal2 = SpeechAct.diff.GetSmartTextFinal("highscore_label", 1, journeySave.distance);
			scoreJourneyTxts[i].text = smartTextFinal;
			scoreLengthTxts[i].text = smartTextFinal2;
			if (num3)
			{
				StartCoroutine(StillAlive(i, smartTextFinal2));
			}
		}
	}

	public string FormatPower(bool justnum = false)
	{
		return "";
	}

	public bool CheckHighScore(int age)
	{
		return true;
	}

	public bool IsHighScore()
	{
		return false;
	}

	private IEnumerator StillAlive(int id, string alt)
	{
		yield return new WaitForSeconds(1f);
		WaitForSeconds lwait = new WaitForSeconds(0.4f);
		WaitForSeconds swait = new WaitForSeconds(0.2f);
		while (true)
		{
			for (int j = 0; j < 4; j++)
			{
				scoreLengthTxts[id].text = SpeechAct.diff.GetSceneTextFinal("highscore_label", 2);
				yield return lwait;
				scoreLengthTxts[id].text = "";
				yield return swait;
			}
			scoreLengthTxts[id].text = alt;
			yield return new WaitForSeconds(2f);
		}
	}

	private void SetAchieve(string txtid, int id, int cur, int max, bool shorttxt = false)
	{
		string sceneText = SpeechAct.diff.GetSceneText(txtid, 1);
		sceneText = sceneText.Replace("<number>", cur.ToString());
		sceneText = sceneText.Replace("<total>", max.ToString());
		achieveTxts[id].text = SpeechAct.diff.FinalFormat(sceneText);
		float value = (float)cur / (float)max;
		achieveSliders[id].value = value;
	}

	public void OpenEffects(bool openmenu = false)
	{
		ClearWindows(openmenu);
		effectsBloc.SetActive(value: true);
	}

	public void CloseEffects()
	{
		InputAct.diff.RestoreSlideFocus();
		effectsBloc.SetActive(value: false);
	}

	public void OpenOptions(bool openmenu)
	{
		ClearWindows(openmenu);
		optionsBloc.SetActive(value: true);
	}

	public void CloseOptions()
	{
		InputAct.diff.RestoreSlideFocus();
		optionsBloc.SetActive(value: false);
	}
}
