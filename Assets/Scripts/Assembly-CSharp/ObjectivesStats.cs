using System.Collections.Generic;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivesStats : MonoBehaviour
{
	public GameObject objectivePrefab;

	public ObjectiveAct scOb;

	private List<Objective> allO;

	private List<GameObject> objs = new List<GameObject>();

	public int scrollBarSteps = 30;

	private ScrollRect objectivesScrollRect;

	private void OnDisable()
	{
		Dismantle();
	}

	private void Dismantle()
	{
		if (objs.Count > 0)
		{
			foreach (GameObject obj in objs)
			{
				Object.Destroy(obj);
			}
		}
		objs = new List<GameObject>();
	}

	private void OnEnable()
	{
		if (objectivesScrollRect == null && GameAct.diff != null && GameAct.diff.scKi != null)
		{
			objectivesScrollRect = GameAct.diff.scKi.GetComponent<ScrollRect>();
		}
		if (objectivesScrollRect != null)
		{
			if (InputAct.diff.curInput == Inputs.keyboard || InputAct.diff.curInput == Inputs.ninSwitch || InputAct.diff.curInput == Inputs.ps || InputAct.diff.curInput == Inputs.xbox)
			{
				if (objectivesScrollRect != null && objectivesScrollRect.verticalScrollbar.numberOfSteps != scrollBarSteps)
				{
					objectivesScrollRect.verticalScrollbar.numberOfSteps = scrollBarSteps;
				}
			}
			else
			{
				objectivesScrollRect.verticalScrollbar.numberOfSteps = 0;
			}
		}
		Dismantle();
		allO = scOb.GetAll();
		List<Objective> list = allO.FindAll((Objective it) => it.fulfilled);
		list.AddRange(allO.FindAll((Objective it) => it.visible && !it.fulfilled));
		list.AddRange(allO.FindAll((Objective it) => !it.visible && !it.fulfilled));
		int num = -80;
		int num2 = -65;
		int num3 = 0;
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			GameObject gameObject = Object.Instantiate(objectivePrefab);
			objs.Add(gameObject);
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			num3 = num + num2 * num4;
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, num3);
			ObjectiveBox component = gameObject.GetComponent<ObjectiveBox>();
			if (list[num4].fulfilled)
			{
				component.Init(list[num4], "", trig: true);
			}
			else if (list[num4].visible)
			{
				component.Init(list[num4], "", trig: false, stayHidden: false);
				component.transform.Find("star").GetComponent<SVGImage>().enabled = true;
			}
			else
			{
				component.Init(list[num4], "", trig: false, stayHidden: true);
			}
		}
		GetComponent<RectTransform>().sizeDelta = new Vector2(0f, -num3 - num2);
	}
}
