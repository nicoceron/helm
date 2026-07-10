using UnityEngine;
using UnityEngine.UI;

public class InitVolumeLevel : MonoBehaviour
{
	public Slider slide;

	public string sample;

	public string id;

	private void OnEnable()
	{
		slide.value = PlayerPrefs.GetFloat(id);
	}

	public void PlaySample()
	{
		JukeBox.diff.PlaySound(SFXTypes.ui_high_score_menu);
	}

	public void StopSample()
	{
		JukeBox.diff.FadeStopSound(SFXTypes.ui_high_score_menu, 2f);
	}
}
