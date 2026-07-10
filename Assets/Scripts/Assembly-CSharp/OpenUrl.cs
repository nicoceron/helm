using UnityEngine;

public class OpenUrl : MonoBehaviour
{
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}
}
