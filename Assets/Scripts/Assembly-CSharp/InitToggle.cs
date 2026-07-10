using UnityEngine;
using UnityEngine.UI;

public class InitToggle : MonoBehaviour
{
	public Toggle toggle;

	public string id;

	private void OnEnable()
	{
		toggle.isOn = ((PlayerPrefs.GetFloat(id) != 0f) ? true : false);
	}
}
