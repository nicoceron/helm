using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HapticOption : MonoBehaviour
{
	public Toggle toggleHaptic;

	public static bool isAvailable;

	private Vector2 newPosition = Vector2.zero;

	private RectTransform toggleTransform;

	public float positionAlteration = 12.5f;

	private RectTransform butfondForToggle;

	private void Awake()
	{
		toggleTransform = GetComponent<RectTransform>();
		butfondForToggle = toggleHaptic.targetGraphic.GetComponent<RectTransform>();
		newPosition = new Vector2(toggleTransform.anchoredPosition.x, toggleTransform.anchoredPosition.y - positionAlteration);
		StartCoroutine(WaitForEOF_OptionsAwake());
	}

	private IEnumerator WaitForEOF_OptionsAwake()
	{
		yield return new WaitForEndOfFrame();
		if (ResolutionOption.isAvailable)
		{
			toggleTransform.anchoredPosition = newPosition;
			if (butfondForToggle != null)
			{
				butfondForToggle.sizeDelta = new Vector2(butfondForToggle.sizeDelta.x, 25f);
			}
		}
	}

	private void OnEnable()
	{
		isAvailable = true;
		if (ResolutionOption.isAvailable)
		{
			toggleTransform.anchoredPosition = newPosition;
		}
	}

	private void Start()
	{
		if (!PlayerPrefs.HasKey("nohaptic"))
		{
			toggleHaptic.isOn = true;
		}
		else
		{
			toggleHaptic.isOn = false;
		}
	}

	private void OnDisable()
	{
		isAvailable = false;
	}

	public void ChangeHaptic(bool ison)
	{
		if (toggleHaptic.isOn)
		{
			PlayerPrefs.DeleteKey("nohaptic");
			HapticAct.diff.StartHaptic();
			HapticAct.diff.BigChange();
		}
		else
		{
			PlayerPrefs.SetInt("nohaptic", 1);
			HapticAct.diff.StopHaptic();
		}
		JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
	}
}
