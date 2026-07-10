using System;
using SVGImporter;
using UnityEngine;

public class AutoSetQuit : MonoBehaviour
{
	public bool keepImg;

	private void OnEnable()
	{
		InputAct diff = InputAct.diff;
		diff.OnSwitchControl = (Action<Inputs>)Delegate.Combine(diff.OnSwitchControl, new Action<Inputs>(NewControl));
		InputAct.diff.SetQuit(base.gameObject);
		if (keepImg)
		{
			return;
		}
		SVGImage componentInChildren = GetComponentInChildren<SVGImage>();
		if (componentInChildren != null)
		{
			if (InputAct.diff.NavigationMode())
			{
				componentInChildren.enabled = false;
			}
			else
			{
				componentInChildren.enabled = true;
			}
		}
	}

	private void NewControl(Inputs type)
	{
		SVGImage componentInChildren = GetComponentInChildren<SVGImage>();
		if (componentInChildren != null)
		{
			if (InputAct.diff.NavigationMode())
			{
				componentInChildren.enabled = false;
			}
			else
			{
				componentInChildren.enabled = true;
			}
		}
	}

	private void OnDisable()
	{
		InputAct diff = InputAct.diff;
		diff.OnSwitchControl = (Action<Inputs>)Delegate.Remove(diff.OnSwitchControl, new Action<Inputs>(NewControl));
	}
}
