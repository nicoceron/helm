using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionOption : MonoBehaviour
{
	public GameObject applyBut;

	public Dropdown dropResol;

	public Toggle toggleResol;

	private int resolid = -1;

	private string[] ids;

	private bool isFullscreen;

	private Resolution[] resol;

	private readonly List<Resolution> displayedResolutions = new List<Resolution>();

	public bool isReady;

	public static bool isAvailable;

	private Vector2 newPosition = Vector2.zero;

	private RectTransform toggleTransform;

	public float positionAlteration = 12.5f;

	public RectTransform butfondForToggle;

	private Vector2 newButfondPos = Vector2.zero;

	private void Start()
	{
		if (SteamManager.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Awake()
	{
		toggleTransform = toggleResol.GetComponent<RectTransform>();
		butfondForToggle = toggleResol.targetGraphic.GetComponent<RectTransform>();
		newPosition = new Vector2(toggleTransform.anchoredPosition.x, toggleTransform.anchoredPosition.y + positionAlteration);
		newButfondPos = new Vector2(butfondForToggle.anchoredPosition.x, butfondForToggle.anchoredPosition.y + positionAlteration);
		StartCoroutine(WaitForEOF_OptionsAwake());
	}

	private IEnumerator WaitForEOF_OptionsAwake()
	{
		yield return new WaitForEndOfFrame();
		if (HapticOption.isAvailable)
		{
			toggleTransform.anchoredPosition = newPosition;
			if (butfondForToggle != null)
			{
				butfondForToggle.sizeDelta = new Vector2(butfondForToggle.sizeDelta.x, 25f);
				butfondForToggle.anchoredPosition = newButfondPos;
			}
		}
	}

	private void OnDisable()
	{
		isAvailable = false;
	}

	private void OnEnable()
	{
		isAvailable = true;
		if (HapticOption.isAvailable)
		{
			toggleTransform.anchoredPosition = newPosition;
		}
		dropResol.ClearOptions();
		displayedResolutions.Clear();
		isReady = false;
		resolid = 0;
		resol = new Resolution[0];
		resol = Screen.resolutions;
		Array.Reverse((Array)resol);
		isFullscreen = Screen.fullScreen;
		List<string> list = new List<string>();
		for (int i = 0; i < resol.Length; i++)
		{
			Resolution resolution = resol[i];
			if ((float)resolution.width / (float)resolution.height > 1.2f && !list.Contains(resolution.width + "x" + resolution.height))
			{
				list.Add(resolution.width + "x" + resolution.height);
				displayedResolutions.Add(resolution);
				if (resolution.width == Screen.width && resolution.height == Screen.height)
				{
					resolid = displayedResolutions.Count - 1;
				}
			}
		}
		dropResol.AddOptions(list);
		dropResol.value = resolid;
		toggleResol.isOn = isFullscreen;
		isReady = true;
	}

	public void ChangeResol()
	{
		if (isReady)
		{
			if (dropResol.value == resolid && isFullscreen == toggleResol.isOn)
			{
				applyBut.SetActive(value: false);
			}
			else
			{
				applyBut.SetActive(value: true);
			}
			JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
		}
	}

	public void ApplyResol()
	{
		if (dropResol.value != resolid || isFullscreen != toggleResol.isOn)
		{
			if (dropResol.value < 0 || dropResol.value >= displayedResolutions.Count)
			{
				return;
			}
			Resolution resolution = displayedResolutions[dropResol.value];
			PlayerPrefs.SetInt("resol_width", resolution.width);
			PlayerPrefs.SetInt("resol_height", resolution.height);
			if (resolution.refreshRate != 0)
			{
				Screen.SetResolution(resolution.width, resolution.height, toggleResol.isOn, resolution.refreshRate);
			}
			else
			{
				Screen.SetResolution(resolution.width, resolution.height, toggleResol.isOn);
			}
		}
		applyBut.SetActive(value: false);
		if (InputAct.diff != null && InputAct.diff.curInput != Inputs.mouse && InputAct.diff.curInput != Inputs.touch)
		{
			EventSystem.current.SetSelectedGameObject(dropResol.gameObject);
		}
	}
}
