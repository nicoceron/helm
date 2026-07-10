using UnityEngine;
using UnityEngine.UI;

public class GPGSui : MonoBehaviour
{
	private bool unavailable;

	public Button gpgsBut;

	public Button achieveBut;

	public Button leaderBut;

	public Button cloudBut;

	public Button connectBut;

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}
}
