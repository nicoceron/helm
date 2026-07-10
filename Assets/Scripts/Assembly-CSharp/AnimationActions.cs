using UnityEngine;

public class AnimationActions : MonoBehaviour
{
	public AnimationAction[] events;

	public void InvokeAnimationAction(string name)
	{
		if (events == null || events.Length == 0)
		{
			return;
		}
		for (int i = 0; i < events.Length; i++)
		{
			if (events[i] != null && events[i].actionEvent != null && !(events[i].name != name))
			{
				events[i].InvokeAnimationAction();
			}
		}
	}
}
