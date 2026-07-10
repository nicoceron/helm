using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeadCloneAct : MonoBehaviour
{
	public GameObject[] stepsEnglish;

	public Text action;

	public Transform objectivePos;

	private string nick;

	public static DeadCloneAct diff;

	public List<JourneySave> journeys = new List<JourneySave>();

	public List<string> overallStats = new List<string>();

	public RectTransform backobjectives;

	private void Awake()
	{
		diff = this;
	}

	public bool AddStat(string title)
	{
		if (!overallStats.Contains(title))
		{
			overallStats.Add(title);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			foreach (string overallStat in overallStats)
			{
				switch (overallStat.Substring(0, 1))
				{
				case "b":
					num++;
					break;
				case "e":
					num2++;
					break;
				case "g":
					num3++;
					break;
				case "o":
					num4++;
					break;
				}
			}
			if (num > 65)
			{
				SocialAct.diff.AddAchieve("charactersBeyond");
			}
			if (num2 > 30)
			{
				SocialAct.diff.AddAchieve("deathsBeyond");
			}
			if (num3 > 8)
			{
				SocialAct.diff.AddAchieve("starBeyond");
			}
			if (num4 > 8)
			{
				SocialAct.diff.AddAchieve("endofendsBeyond");
			}
			return true;
		}
		return false;
	}

	private void Start()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnGameInit = (Action<GameSave>)Delegate.Combine(gameAct.OnGameInit, new Action<GameSave>(LoadJourneys));
		base.gameObject.SetActive(value: false);
	}

	private void LoadJourneys(GameSave save)
	{
		if (save == null)
		{
			journeys = new List<JourneySave>();
			overallStats = new List<string>();
		}
		else
		{
			journeys = save.journeys;
			overallStats = save.stats;
		}
	}

	public void Init()
	{
		base.gameObject.SetActive(value: true);
		stepsEnglish[0].SetActive(value: true);
		int num = 0;
		nick = ObjectiveAct.diff.nickname;
		foreach (Transform item in stepsEnglish[0].transform)
		{
			item.GetComponent<Text>().text = SpeechAct.diff.GetSmartTextFinal("death_0", num);
			num++;
		}
		backobjectives.GetComponent<SVGImage>().DOFade(1f, 0.6f).SetDelay(0.6f);
		JourneySave currentJourney = GetCurrentJourney();
		journeys.Add(currentJourney);
		if (currentJourney.cloneNumber > 100)
		{
			SocialAct.diff.AddAchieve("hundredBeyond");
		}
		if (currentJourney.distance < 6)
		{
			SocialAct.diff.AddAchieve("bellowBeyond");
		}
		JukeBox.diff.StopMusic();
		GameAct.diff.SetInt(Variables.stop, 1);
	}

	public void Trigger()
	{
		StartCoroutine("DoTrigger");
	}

	private IEnumerator DoTrigger()
	{
		MoneyUI.diff.HideMoney();
		GameAct.diff.DeleteQuestion();
		yield return new WaitForSeconds(0.6f);
		BackgroundAct.diff.ShowOptions();
		yield return new WaitForSeconds(2f);
		stepsEnglish[0].SetActive(value: false);
		stepsEnglish[1].SetActive(value: true);
		int num = 0;
		foreach (Transform item in stepsEnglish[1].transform)
		{
			Text component = item.GetComponent<Text>();
			if (component != null)
			{
				component.text = SpeechAct.diff.GetSmartTextFinal("death_1", num, -1, nick);
				num++;
			}
		}
		backobjectives.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutSine);
		ObjectiveAct.diff.ShowObjectives(objectivePos, -20, replace: true, thenupdate: true);
		yield return new WaitForSeconds(4f);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.SwitchSize(tall: false);
			AnimBut.diff.UnLock(ControlModes.next);
		}
		InputAct.diff.GetActionFocus(Close);
	}

	private bool Close(bool down)
	{
		PlayerPrefs.SetInt("justpassing", 1);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock();
		}
		InputAct.diff.RestoreSlideFocus();
		GameAct.diff.SetResurrect();
		SceneManager.LoadScene(0);
		return true;
	}

	public List<JourneySave> GetRankedJourneys()
	{
		List<JourneySave> list = new List<JourneySave>(journeys);
		JourneySave cur = GetCurrentJourney();
		if (list.Find((JourneySave it) => it.cloneNumber == cur.cloneNumber) == null)
		{
			list.Add(cur);
		}
		list.Sort((JourneySave p1, JourneySave p2) => p2.distance.CompareTo(p1.distance));
		return list;
	}

	public JourneySave GetCurrentJourney()
	{
		return new JourneySave(ObjectiveAct.diff.nickname, GameAct.diff.GetInt(Variables.length), journeys.Count + 1);
	}

	public int GetRank(int distance = -1)
	{
		if (distance == -1 && journeys.Count > 0)
		{
			distance = journeys[journeys.Count - 1].distance;
		}
		if (distance == 0 || journeys.Count == 0)
		{
			return journeys.Count + 1;
		}
		SocialAct.diff.SetScore(distance);
		List<JourneySave> list = new List<JourneySave>(journeys);
		list.Sort((JourneySave p1, JourneySave p2) => p1.distance.CompareTo(p2.distance));
		for (int num = 0; num < list.Count; num++)
		{
			if (distance < list[num].distance)
			{
				return list.Count - num + 1;
			}
		}
		return 1;
	}
}
