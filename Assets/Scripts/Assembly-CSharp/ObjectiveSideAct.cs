using UnityEngine;

public class ObjectiveSideAct : MonoBehaviour
{
	public ObjectiveAct scOb;

	private void OnEnable()
	{
		if (!((float)Screen.width / (float)Screen.height < 1.26f))
		{
			scOb.ShowObjectives(base.transform, 0);
		}
	}

	private void OnDisable()
	{
		scOb.DestroyBoxes();
	}
}
