using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAct : MonoBehaviour
{
	public static ObjectiveAct diff;

	public List<Objective> list1 = new List<Objective>();

	public List<Objective> list2 = new List<Objective>();

	public List<Objective> list3 = new List<Objective>();

	private char[] dele = new char[1] { ';' };

	public List<Objective> objectives;

	public List<Objective> objectivesActive = new List<Objective>();

	private List<Objective> displayedObj = new List<Objective>();

	private List<ObjectiveBox> objBoxes = new List<ObjectiveBox>();

	private List<bool> objTypes = new List<bool>();

	public GameObject objectBoxPrefab;

	public GameObject newcardsPrefab;

	public Action OnNewObjective;

	public string nickname = "the Unknown";

	private void Awake()
	{
		diff = this;
	}

	private void Start()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnGameInit = (Action<GameSave>)Delegate.Combine(gameAct.OnGameInit, new Action<GameSave>(InitObjectives));
		GameAct gameAct2 = GameAct.diff;
		gameAct2.OnUpdate = (Action<Card>)Delegate.Combine(gameAct2.OnUpdate, new Action<Card>(CheckObjectives));
		ResetNick();
	}

	private List<Objective> GetObjectives(bool nostatechange = false)
	{
		Dictionary<string, string[]> languageStrings = GetLanguageStrings();
		List<Objective> list = new List<Objective>();
		string[] array = CardReader.diff.GetTempText("objectives").Split('\n');
		string[] columns = array[0].Split(dele, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < array.Length; i++)
		{
			list.Add(new Objective(array[i].Split(dele), columns, list, i, languageStrings, nostatechange));
		}
		return list;
	}

	private Dictionary<string, string[]> GetLanguageStrings()
	{
		string lang = SpeechAct.diff.lang;
		if (lang == "en")
		{
			return new Dictionary<string, string[]>();
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		string[] array = Util.GetTextFile("texts/objectives_i18n").Split('\n');
		string[] array2 = array[0].Split(dele);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array3 = array[i].Split(dele);
			string[] array4 = new string[3];
			string key = "";
			for (int j = 0; j < array2.Length; j++)
			{
				string text = array2[j];
				if (text == "name")
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
				else if (text == lang + "_achievement")
				{
					array4[2] = array3[j];
				}
			}
			dictionary.Add(key, array4);
		}
		return dictionary;
	}

	private void InitObjectives(GameSave save = null)
	{
		objectives = GetObjectives();
		UpdateActives();
		if (save == null)
		{
			list1[0].visible = (list2[0].visible = (list3[0].visible = true));
			return;
		}
		nickname = save.nickname;
		foreach (Objective ob in objectives)
		{
			ObjectiveSave objectiveSave = save.objectives.Find((ObjectiveSave it) => it.id == ob.id);
			if (objectiveSave != null)
			{
				ob.accessible = objectiveSave.accessible;
				ob.fulfilled = objectiveSave.fulfilled;
				ob.visible = objectiveSave.visible;
			}
		}
	}

	private void UpdateActives()
	{
		int num = 0;
		list1 = new List<Objective>();
		list2 = new List<Objective>();
		list3 = new List<Objective>();
		foreach (Objective objective in objectives)
		{
			if (objective.pid == -1)
			{
				num++;
			}
			switch (num)
			{
			case 1:
				list1.Add(objective);
				break;
			case 2:
				list2.Add(objective);
				break;
			case 3:
				list3.Add(objective);
				break;
			}
		}
	}

	public void ResetNick()
	{
		nickname = SpeechAct.diff.GetSceneTextFinal("defaultnick");
	}

	private void CheckObjectives(Card card)
	{
		foreach (Objective obj in objectives)
		{
			if (GameAct.diff.TestCond(obj.conditions) && !obj.fulfilled)
			{
				obj.fulfilled = true;
				nickname = obj.title.Get();
				ObjectiveBox objectiveBox = objBoxes.Find((ObjectiveBox it) => it.obj.name == obj.name);
				if (objectiveBox != null)
				{
					objectiveBox.Validate();
				}
				GameAct.diff.AddInt(Variables.nb_fame, Util.RandInt(995, 1100));
				GameAct.diff.PlayModal(ModalTypes.objective, obj, 3f);
				if (!string.IsNullOrEmpty(obj.achievement))
				{
					GameAct.diff.PlayModal(ModalTypes.custom, newcardsPrefab, 3f, SpeechAct.diff.FinalFormat(obj.achievement));
				}
				if (OnNewObjective != null)
				{
					OnNewObjective();
				}
				GetComponent<JourneyAct>().AddAchieve(obj.name, AchieveTypes.objective);
			}
		}
	}

	private void UnlockCards(List<Condition> conds)
	{
		foreach (Condition cond in conds)
		{
			if ((cond.value > 0 && cond.bearer == Bearers.none) || (cond.value == 0 && cond.bearer != Bearers.none && cond.condition == Conditions.equal))
			{
				GameAct.diff.LockCards(cond, locked: false);
			}
			if ((cond.value == -1 && cond.bearer == Bearers.none && cond.condition == Conditions.equal) || (cond.value == 0 && cond.bearer != Bearers.none && cond.condition == Conditions.notequal))
			{
				GameAct.diff.LockCardsOutcome(cond, locked: false);
			}
		}
	}

	public List<ObjectiveSave> PrepareSave()
	{
		List<ObjectiveSave> list = new List<ObjectiveSave>();
		foreach (Objective objective in objectives)
		{
			list.Add(new ObjectiveSave(objective.id, objective.accessible, objective.fulfilled, objective.visible));
		}
		return list;
	}

	private List<Objective> GetNew()
	{
		List<Objective> list = new List<Objective>();
		Objective objective = list1.Find((Objective it) => !it.fulfilled);
		if (objective != null && !objective.visible)
		{
			list.Add(objective);
		}
		Objective objective2 = list2.Find((Objective it) => !it.fulfilled);
		if (objective2 != null && !objective2.visible)
		{
			list.Add(objective2);
		}
		Objective objective3 = list3.Find((Objective it) => !it.fulfilled);
		if (objective3 != null && !objective3.visible)
		{
			list.Add(objective3);
		}
		foreach (Objective item in list)
		{
			item.visible = true;
			UnlockCards(item.conditions);
		}
		return list;
	}

	public List<Objective> GetDisplayed()
	{
		List<Objective> list = new List<Objective>();
		Objective objective = list1.Find((Objective it) => it.visible);
		if (objective != null)
		{
			list.Add(objective);
		}
		Objective objective2 = list2.Find((Objective it) => it.visible);
		if (objective2 != null)
		{
			list.Add(objective2);
		}
		Objective objective3 = list3.Find((Objective it) => it.visible);
		if (objective3 != null)
		{
			list.Add(objective3);
		}
		foreach (Objective item in list)
		{
			if (!item.fulfilled)
			{
				UnlockCards(item.conditions);
			}
		}
		return list;
	}

	public List<Objective> GetFulfilled()
	{
		List<Objective> list = new List<Objective>();
		list.AddRange(list1.FindAll((Objective it) => it.fulfilled));
		list.AddRange(list2.FindAll((Objective it) => it.fulfilled));
		list.AddRange(list3.FindAll((Objective it) => it.fulfilled));
		return list;
	}

	public List<Objective> GetAll()
	{
		return new List<Objective>(objectives);
	}

	public void ShowObjectives(Transform par, int cstpos, bool replace = true, bool thenupdate = false, float timer = 2f)
	{
		StopAllCoroutines();
		StartCoroutine(DoShowObjectives(par, cstpos, thenupdate, timer, replace));
	}

	private IEnumerator DoShowObjectives(Transform par, int cstpos, bool thenupdate, float timer, bool replace)
	{
		if (replace)
		{
			DestroyBoxes();
		}
		displayedObj = GetDisplayed();
		int n = 0;
		if (replace)
		{
			objBoxes = new List<ObjectiveBox>();
		}
		WaitForSeconds swait = new WaitForSeconds(0.3f);
		foreach (Objective item in displayedObj)
		{
			ObjectiveBox objectiveBox = InstantiateBox(item, par, replace);
			objectiveBox.name = item.name;
			objectiveBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, cstpos - 240 - 10 * n);
			StartCoroutine(ReposObj(objectiveBox, new Vector2(0f, cstpos + displayedObj.Count * 15 - 30 * n)));
			n++;
			yield return swait;
		}
		yield return new WaitForSeconds(timer);
		if (thenupdate)
		{
			yield return StartCoroutine(DoHideAssigned(par, cstpos));
		}
	}

	private ObjectiveBox InstantiateBox(Objective obj, Transform par, bool replace = true)
	{
		GameObject obj2 = UnityEngine.Object.Instantiate(objectBoxPrefab);
		obj2.transform.SetParent(par, worldPositionStays: false);
		ObjectiveBox component = obj2.GetComponent<ObjectiveBox>();
		if (replace)
		{
			objBoxes.Add(component);
		}
		bool fulfilled = obj.fulfilled;
		component.Init(obj, "", fulfilled);
		return component;
	}

	public void HideAssignedAndAdd(Transform par, int cstpos)
	{
		StartCoroutine(DoHideAssigned(par, cstpos));
	}

	private IEnumerator DoHideAssigned(Transform par, int cstpos)
	{
		int max = displayedObj.Count;
		int i = 0;
		while (true)
		{
			if (i < displayedObj.Count)
			{
				if (displayedObj[i].fulfilled)
				{
					displayedObj[i].visible = false;
					max--;
					if (!objBoxes[i].isActiveAndEnabled)
					{
						break;
					}
					objBoxes[i].HideRight();
					JukeBox.diff.PlaySound(SFXTypes.ui_achievement_completed);
					HapticAct.diff.Tap(iOSHapticFeedback.iOSFeedbackType.Success);
					yield return new WaitForSeconds(0.25f - (float)i * 0.06f);
				}
				i++;
				continue;
			}
			List<Objective> newob = GetNew();
			max += newob.Count;
			int n = 0;
			WaitForSeconds swait = new WaitForSeconds(0.3f);
			i = 0;
			while (true)
			{
				if (i < objBoxes.Count)
				{
					if (displayedObj[i].visible)
					{
						if (objBoxes[i] == null)
						{
							break;
						}
						StartCoroutine(ReposObj(objBoxes[i], new Vector2(0f, cstpos + max * 15 - 30 * n)));
						n++;
						yield return swait;
					}
					i++;
					continue;
				}
				foreach (Objective item in newob)
				{
					ObjectiveBox objectiveBox = InstantiateBox(item, par);
					objectiveBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, cstpos - 240 - 10 * n);
					StartCoroutine(ReposObj(objectiveBox, new Vector2(0f, cstpos + max * 15 - 30 * n)));
					n++;
					yield return swait;
				}
				break;
			}
			break;
		}
	}

	private IEnumerator ReposObj(ObjectiveBox obj, Vector2 tpos)
	{
		float t = 0f;
		if (!(obj == null))
		{
			RectTransform rect = obj.GetComponent<RectTransform>();
			while (t < 1f && !(rect == null))
			{
				rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, tpos, 0.4f + t);
				t += Time.deltaTime * 2f;
				yield return null;
			}
		}
	}

	public void DestroyBoxes()
	{
		foreach (ObjectiveBox objBox in objBoxes)
		{
			if ((bool)objBox)
			{
				UnityEngine.Object.Destroy(objBox.gameObject);
			}
		}
	}
}
