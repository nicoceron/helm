using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
	public Transform content;

	public Scrollbar scrollbar;

	public void OnEnable()
	{
		StartCoroutine("ScrollUpdate");
	}

	private void OnDisable()
	{
		StopCoroutine("ScrollUpdate");
	}

	private IEnumerator ScrollUpdate()
	{
		while (true)
		{
			if (!InputAct.diff.NavigationMode())
			{
				yield return 0;
				continue;
			}
			if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(content) && EventSystem.current.currentSelectedGameObject.name.StartsWith("Item "))
			{
				Transform parent = EventSystem.current.currentSelectedGameObject.transform.parent;
				int num = 0;
				int childCount = parent.childCount;
				for (int i = 0; i < childCount; i++)
				{
					if (parent.GetChild(i).gameObject.activeInHierarchy)
					{
						num++;
					}
				}
				int num2 = 0;
				for (int j = 0; j < childCount && !(parent.GetChild(j).gameObject == EventSystem.current.currentSelectedGameObject); j++)
				{
					if (parent.GetChild(j).gameObject.activeInHierarchy)
					{
						num2++;
					}
				}
				if (num > 1)
				{
					if (scrollbar.direction == Scrollbar.Direction.TopToBottom)
					{
						scrollbar.value = (float)num2 / (float)(num - 1);
					}
					else
					{
						scrollbar.value = 1f - (float)num2 / (float)(num - 1);
					}
				}
			}
			yield return 0;
		}
	}
}
