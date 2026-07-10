using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class PlanetAct : MonoBehaviour
{
	public Text highscoreTxt;

	public Text yearsTxt;

	public GameObject isdead;

	public GameObject isgone;

	public SVGImage[] portraits;

	public Text[] names;

	public Text[] numbers;

	public Text[] inpowers;

	public GameObject[] stars;

	public ObjectiveAct scOb;

	public Transform allobjBox;

	private void OnEnable()
	{
		if (GameAct.diff.GetInt(Variables.journey) == 0)
		{
			portraits[0].gameObject.SetActive(value: false);
			portraits[1].gameObject.SetActive(value: false);
			portraits[2].gameObject.SetActive(value: false);
			portraits[3].gameObject.SetActive(value: false);
			scOb.ShowObjectives(allobjBox, 0, replace: true, thenupdate: true, 1f);
			isgone.SetActive(value: true);
			isdead.SetActive(value: false);
		}
		else
		{
			portraits[0].gameObject.SetActive(value: true);
			portraits[1].gameObject.SetActive(value: true);
			portraits[2].gameObject.SetActive(value: true);
			portraits[3].gameObject.SetActive(value: true);
			isgone.SetActive(value: false);
			isdead.SetActive(value: true);
			scOb.ShowObjectives(allobjBox, 0, replace: true, thenupdate: true, 1f);
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		GameAct.diff.DestroyModals();
	}
}
