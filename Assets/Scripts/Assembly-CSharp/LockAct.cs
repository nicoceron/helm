using UnityEngine;

public class LockAct : MonoBehaviour
{
	public void AnimFinished()
	{
		base.gameObject.SetActive(value: false);
	}
}
