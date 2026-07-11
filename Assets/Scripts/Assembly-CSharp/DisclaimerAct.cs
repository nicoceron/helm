using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class DisclaimerAct : MonoBehaviour
{
	public GameObject controlTouch;

	public GameObject controlRemote;

	public GameObject controlController;

	public GameObject loadText;

	public GameObject knowninput;

	public GameObject unknowninput;

	public Transform Canvas;

	private bool isAlreadyLoaded;

	public VideoPlayer player;

	public VideoClip l_4_3;

	public VideoClip p_4_3;

	public VideoClip l_43_3;

	public VideoClip p_43_3;

	public VideoClip l_16_9;

	public VideoClip p_16_9;

	public VideoClip l_16_10;

	public VideoClip p_16_10;

	public VideoClip l_195_9;

	public VideoClip p_195_9;

	private bool hasskip;

	private bool startRequested;

	private void Awake()
	{
		isAlreadyLoaded = ((Time.time != 0f) ? true : false);
		DataStore.localSaveFileSystem.Initalization();
	}

	private void ShowControl(bool show)
	{
	}

	public void Start()
	{
		InstallHelmTitle();
		if (PlayerPrefs.HasKey("justpassing"))
		{
			PlayerPrefs.DeleteKey("justpassing");
			LoadGameScene();
		}
		else
		{
			StartCoroutine("Demarre");
		}
	}

	private void InstallHelmTitle()
	{
		Transform logoRoot = Canvas.Find("logo");
		if (logoRoot == null)
		{
			return;
		}

		foreach (Transform child in logoRoot)
		{
			child.gameObject.SetActive(false);
		}

		Color signalRed = new Color(0.886f, 0.031f, 0.118f, 1f);
		Color quietWhite = new Color(0.78f, 0.81f, 0.84f, 1f);
		CreateTitleLine(logoRoot, "HELM", 72, new Vector2(0f, 84f), Color.white);
		CreateTitleLine(logoRoot, "RULER CAPABILITY EXAMINATION", 14, new Vector2(0f, 35f), quietWhite);
		CreateRule(logoRoot, new Vector2(0f, 10f), signalRed);
		CreateTitleLine(logoRoot, "SCENARIO S1", 18, new Vector2(0f, -18f), signalRed);
		CreateTitleLine(logoRoot, "BIG BROTHER IS WATCHING", 30, new Vector2(0f, -58f), Color.white);
		CreateTitleLine(logoRoot, "OBSERVATION ACTIVE", 12, new Vector2(0f, -101f), signalRed);
	}

	private static void CreateRule(Transform parent, Vector2 position, Color color)
	{
		GameObject rule = new GameObject("Scenario Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		rule.transform.SetParent(parent, false);
		RectTransform rect = rule.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = new Vector2(390f, 2f);
		rect.anchoredPosition = position;
		Image image = rule.GetComponent<Image>();
		image.color = new Color(color.r, color.g, color.b, 0.65f);
		image.raycastTarget = false;
	}

	private static void CreateTitleLine(Transform parent, string value, int size, Vector2 position, Color color)
	{
		GameObject line = new GameObject(value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
		line.transform.SetParent(parent, false);
		RectTransform rect = line.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = new Vector2(760f, size + 32f);
		rect.anchoredPosition = position;

		Text text = line.GetComponent<Text>();
		text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		text.text = value;
		text.fontSize = size;
		text.fontStyle = FontStyle.Bold;
		text.alignment = TextAnchor.MiddleCenter;
		text.color = color;
		text.raycastTarget = false;
	}

	private void Update()
	{
		if (!startRequested && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
		{
			StartGame();
		}
	}

	private IEnumerator CheckSkip()
	{
		while (hasskip)
		{
			if (Input.anyKeyDown)
			{
				StopCoroutine("Demarre");
				player.enabled = false;
				Canvas.gameObject.SetActive(value: true);
				loadText.SetActive(value: true);
				LoadGameScene();
				hasskip = false;
				break;
			}
			yield return 0;
		}
	}

	private IEnumerator Demarre()
	{
		if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.OSXPlayer && Application.platform != RuntimePlatform.LinuxPlayer && Application.platform != RuntimePlatform.WindowsEditor)
		{
			Application.targetFrameRate = 60;
		}
		else
		{
			QualitySettings.vSyncCount = 1;
		}
		if (InputAct.diff != null)
		{
			InputAct.diff.isLandscape();
		}
		Canvas.gameObject.SetActive(value: true);
		AnimBut.diff.Lock(direct: true);
		yield return new WaitForSeconds(0.2f);
		foreach (Transform canva in Canvas)
		{
			canva.gameObject.SetActive(value: true);
			if (canva.name == "logo")
			{
				Transform legacyPrompt = canva.Find("pcStart");
				if (legacyPrompt != null)
				{
					legacyPrompt.gameObject.SetActive(value: false);
				}
			}
		}
		yield return new WaitForSeconds(1.5f);
		hasskip = false;
		if (isAlreadyLoaded)
		{
			StartGame();
			yield break;
		}
		ShowControl(show: true);
		if (InputAct.diff != null)
		{
			InputAct.diff.GetActionFocus(StartGame);
		}
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.UnLock(ControlModes.next);
		}
	}

	public bool StartGame(bool n = false)
	{
		if (startRequested)
		{
			return false;
		}
		startRequested = true;
		StartCoroutine("DoStart");
		return true;
	}

	private IEnumerator DoStart()
	{
		ShowControl(show: false);
		loadText.SetActive(value: true);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock();
		}
		yield return new WaitForSeconds(1f);
		LoadGameScene();
	}

	private void LoadGameScene()
	{
		if (SocialAct.diff != null)
		{
			SocialAct.diff.SocialAuthent();
		}
		if (SpeechAct.diff.lang == "ar")
		{
			SceneManager.LoadSceneAsync("reigns_arabic");
		}
		else if (SpeechAct.diff.asiaLayout)
		{
			SceneManager.LoadSceneAsync("reigns_asia");
		}
		else
		{
			SceneManager.LoadSceneAsync("reigns_west");
		}
	}
}
