using UnityEngine;
using UnityEngine.UI;

public class AchieveButton : MonoBehaviour
{
	public Text followers;

	private Transform butfond;

	private Button button;

	public void OpenAchievements()
	{
		SocialAct.diff.OpenAchievements();
	}

	public void OpenLeaderboard()
	{
		SocialAct.diff.OpenLeaderBoard();
	}

	private void OnEnable()
	{
		followers.text = SpeechAct.diff.GetSmartTextFinal("band_stats");
		if (button == null)
		{
			button = GetComponent<Button>();
		}
		if (button != null)
		{
			button.interactable = false;
		}
		if (butfond == null)
		{
			butfond = base.gameObject.transform.Find("butfond");
		}
		if (button != null)
		{
			butfond.gameObject.SetActive(value: false);
		}
	}
}
