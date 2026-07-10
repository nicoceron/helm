using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogAct : MonoBehaviour
{
	public GameObject actionIcon;

	public Text actionTxt;

	public GameObject cancelIcon;

	public Text cancelTxt;

	public Text labelTxt;

	private Action<bool> OnValidate;

	private float time;

	public void Init(string labelId, string actionId, Action<bool> onvalid, string overridetext = null)
	{
		Init(labelId, actionId, "cancel", onvalid, overridetext);
	}

	public void Init(string labelId, string actionId, string cancelId, Action<bool> onvalid, string overridetext = null, string overrideaction = null, string overridecancel = null)
	{
		base.transform.SetAsLastSibling();
		time = Time.realtimeSinceStartup;
		OnValidate = onvalid;
		actionTxt.text = (string.IsNullOrEmpty(overrideaction) ? SpeechAct.diff.GetSceneTextFinal(actionId).ToUpper() : overrideaction);
		if (string.IsNullOrEmpty(cancelId))
		{
			cancelIcon.gameObject.SetActive(value: false);
			cancelTxt.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			cancelIcon.gameObject.SetActive(value: true);
			cancelTxt.transform.parent.gameObject.SetActive(value: true);
			cancelTxt.text = (string.IsNullOrEmpty(overridecancel) ? SpeechAct.diff.GetSceneTextFinal(cancelId).ToUpper() : overridecancel);
		}
		labelTxt.text = (string.IsNullOrEmpty(overridetext) ? SpeechAct.diff.GetSceneTextFinal(labelId) : overridetext);
		if ((bool)CardReader.diff)
		{
			CardReader.diff.GetComponent<JourneyAct>().CloseWindows();
		}
		InputAct.diff.MenuNav();
	}

	private void OnEnable()
	{
		time = Time.realtimeSinceStartup;
	}

	public void ActionDo()
	{
		if (!(Time.realtimeSinceStartup - time < 0.2f))
		{
			base.gameObject.SetActive(value: false);
			InputAct.diff.DisableMenuNav(true);
			if (OnValidate != null)
			{
				OnValidate(obj: true);
			}
		}
	}

	public void Cancel()
	{
		if (!(Time.realtimeSinceStartup - time < 0.2f))
		{
			base.gameObject.SetActive(value: false);
			InputAct.diff.DisableMenuNav(true);
			if (OnValidate != null)
			{
				OnValidate(obj: false);
			}
		}
	}

	public void OnDisable()
	{
		DisclaimerAct component = InputAct.diff.gameObject.GetComponent<DisclaimerAct>();
		if (component != null)
		{
			InputAct.diff.GetActionFocus(component.StartGame);
		}
	}
}
