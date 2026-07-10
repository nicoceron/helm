using System;
using UnityEngine.Events;

[Serializable]
public class AnimationAction
{
	[Serializable]
	public class ActionEvent : UnityEvent
	{
	}

	public string name;

	public ActionEvent actionEvent = new ActionEvent();

	public void InvokeAnimationAction()
	{
		actionEvent.Invoke();
	}
}
