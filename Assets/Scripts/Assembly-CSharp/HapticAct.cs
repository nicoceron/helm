using System;
using System.Collections;
using UnityEngine;

public class HapticAct : MonoBehaviour
{
	public static HapticAct diff;

	private bool isOn = true;

	private void Awake()
	{
		diff = this;
	}

	public void StartHaptic()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnJourneyEnd = (Action)Delegate.Combine(gameAct.OnJourneyEnd, new Action(Failure));
		isOn = true;
	}

	public void StopHaptic()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnJourneyEnd = (Action)Delegate.Remove(gameAct.OnJourneyEnd, new Action(Failure));
		isOn = false;
	}

	private void Start()
	{
		if (PlayerPrefs.HasKey("nohaptic"))
		{
			StopHaptic();
		}
		else
		{
			StartHaptic();
		}
	}

	private bool LightChange(int dec)
	{
		if (!isOn)
		{
			return false;
		}
		HapticInterface.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange, HapticInterface.HapticDuration.Short);
		return false;
	}

	private void NormalChange(Card card)
	{
		if (isOn)
		{
			HapticInterface.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium, HapticInterface.HapticDuration.Short);
		}
	}

	public void Tap(iOSHapticFeedback.iOSFeedbackType type = iOSHapticFeedback.iOSFeedbackType.ImpactLight)
	{
		if (isOn)
		{
			HapticInterface.Trigger(type, HapticInterface.HapticDuration.Tap);
		}
	}

	public void BigChange()
	{
		if (isOn)
		{
			HapticInterface.Trigger(iOSHapticFeedback.iOSFeedbackType.Failure, HapticInterface.HapticDuration.Short);
		}
	}

	private void Failure()
	{
		if (isOn)
		{
			HapticInterface.Trigger(iOSHapticFeedback.iOSFeedbackType.Warning, HapticInterface.HapticDuration.Short);
		}
	}

	public void OpenGlitch()
	{
		if (isOn)
		{
			StopCoroutine("ThreeGlitch");
			StartCoroutine("ThreeGlitch");
		}
	}

	public void StopGlitch()
	{
		StopCoroutine("ThreeGlitch");
	}

	private IEnumerator ThreeGlitch()
	{
		while (true)
		{
			float amo = Util.Rand(0.4f, 1.7f);
			yield return new WaitForSeconds(amo);
			if (amo < 0.8f)
			{
				NormalChange(null);
			}
			else if (amo < 1.2f)
			{
				HapticInterface.Trigger(iOSHapticFeedback.iOSFeedbackType.Success, HapticInterface.HapticDuration.Short);
			}
			else if (amo < 1.5f)
			{
				BigChange();
			}
			else
			{
				Failure();
			}
		}
	}
}
