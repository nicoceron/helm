using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashAct : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(DoSplash());
	}

	private IEnumerator DoSplash()
	{
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene("disclaimer");
	}
}
