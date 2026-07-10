using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoSelectMe : MonoBehaviour
{
	private void OnEnable()
	{
		Activate();
	}

	private IEnumerator YieldSelect()
	{
		yield return null;
		GetComponent<Selectable>().enabled = true;
		InputAct.diff.SetSelect(base.gameObject);
	}

	public void DeActivate()
	{
		GetComponent<Selectable>().enabled = false;
	}

	public bool Activate(bool ison, bool isnav)
	{
		if (!base.enabled)
		{
			return false;
		}
		if (!isnav)
		{
			return false;
		}
		Activate();
		return true;
	}

	public void Activate()
	{
		if (base.enabled && (!InputAct.diff || InputAct.diff.NavigationMode()))
		{
			StartCoroutine("YieldSelect");
		}
	}
}
