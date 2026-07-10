using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PortraitOption : MonoBehaviour
{
	public Toggle togglePortrait;

	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ChangePortrait(bool isOn)
	{
		StartCoroutine("DoChangePortrait");
	}

	private IEnumerator DoChangePortrait()
	{
		if (togglePortrait.isOn)
		{
			PlayerPrefs.SetInt("forceportrait", 1);
			Screen.orientation = ScreenOrientation.Portrait;
		}
		else
		{
			PlayerPrefs.DeleteKey("forceportrait");
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			yield return 0;
			Screen.orientation = ScreenOrientation.AutoRotation;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToPortrait = false;
		}
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}
}
