using System.Collections.Generic;
using UnityEngine;

public class TestInputJukebox : MonoBehaviour
{
	[Range(10f, 300f)]
	public float voPitch = 100f;

	[Range(20f, 20000f)]
	public float voCenterFrequ = 2900f;

	[Range(0f, 3f)]
	public float voFrequGain = 1.5f;

	public List<DataVariable> newvar;

	private void Start()
	{
		GetComponent<JukeBox>().LinkVar(newvar);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			Input.GetKeyDown(KeyCode.M);
		}
	}
}
