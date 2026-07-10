using UnityEngine;

public class StarAct : MonoBehaviour
{
	public void DestroyStar()
	{
		Object.Destroy(base.transform.parent.gameObject);
	}
}
