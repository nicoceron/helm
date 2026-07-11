using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class HelmAllocationMinigame : CardAct
{
	private const float Duration = 12f;
	private const float RequiredLockRatio = 0.42f;
	private const float TrackWidth = 310f;

	private Text prompt;
	private Text timerText;
	private Text statusText;
	private RectTransform targetWindow;
	private RectTransform commandPulse;
	private RectTransform progressFill;
	private Image targetImage;
	private Image pulseImage;
	private float remaining;
	private float lockedTime;
	private float cursor = 0.5f;
	private float elapsed;
	private float nextSound;
	private bool running;
	private bool inputSuspended;

	public static void Install(Transform repository)
	{
		if (repository.Find("fight") != null)
		{
			return;
		}

		GameObject root = new GameObject("fight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		root.transform.SetParent(repository, false);
		RectTransform rootRect = root.GetComponent<RectTransform>();
		rootRect.anchorMin = new Vector2(0.5f, 0.5f);
		rootRect.anchorMax = new Vector2(0.5f, 0.5f);
		rootRect.pivot = new Vector2(0.5f, 0.5f);
		rootRect.sizeDelta = new Vector2(400f, 400f);
		Image background = root.GetComponent<Image>();
		background.color = new Color(0.025f, 0.035f, 0.045f, 0.97f);
		background.raycastTarget = false;

		HelmAllocationMinigame game = root.AddComponent<HelmAllocationMinigame>();
		game.mytrans = rootRect;
		game.BuildInterface(rootRect);
		root.SetActive(false);
	}

	private void BuildInterface(RectTransform root)
	{
		for (int i = 0; i < 10; i++)
		{
			Image scanline = CreateImage(root, "Scanline", new Vector2(360f, 1f), new Vector2(0f, 150f - i * 32f),
				new Color(0.88f, 0.03f, 0.12f, i % 2 == 0 ? 0.18f : 0.08f));
			scanline.raycastTarget = false;
		}

		CreateText(root, "Header", "HELM // ALLOCATION PULSE", 17, new Vector2(0f, 164f), new Vector2(360f, 28f), Color.white, FontStyle.Bold);
		prompt = CreateText(root, "Prompt", "", 14, new Vector2(0f, 105f), new Vector2(350f, 82f),
			new Color(0.82f, 0.84f, 0.86f, 1f), FontStyle.Normal);

		Image track = CreateImage(root, "Allocation Track", new Vector2(TrackWidth, 74f), new Vector2(0f, 18f),
			new Color(0.08f, 0.1f, 0.12f, 1f));
		Outline trackOutline = track.gameObject.AddComponent<Outline>();
		trackOutline.effectColor = new Color(0.88f, 0.03f, 0.12f, 0.7f);
		trackOutline.effectDistance = new Vector2(2f, -2f);

		targetImage = CreateImage(track.rectTransform, "Allocation Window", new Vector2(76f, 66f), Vector2.zero,
			new Color(0.88f, 0.03f, 0.12f, 0.42f));
		targetWindow = targetImage.rectTransform;
		pulseImage = CreateImage(track.rectTransform, "Command Pulse", new Vector2(7f, 88f), Vector2.zero, Color.white);
		commandPulse = pulseImage.rectTransform;

		Image progressBack = CreateImage(root, "Lock Progress", new Vector2(TrackWidth, 11f), new Vector2(0f, -47f),
			new Color(0.12f, 0.14f, 0.16f, 1f));
		Image fill = CreateImage(progressBack.rectTransform, "Fill", new Vector2(0f, 11f), Vector2.zero,
			new Color(0.88f, 0.03f, 0.12f, 1f));
		progressFill = fill.rectTransform;
		progressFill.anchorMin = new Vector2(0f, 0.5f);
		progressFill.anchorMax = new Vector2(0f, 0.5f);
		progressFill.pivot = new Vector2(0f, 0.5f);
		progressFill.anchoredPosition = new Vector2(-TrackWidth * 0.5f, 0f);

		statusText = CreateText(root, "Status", "MOVE THE WHITE PULSE INTO THE RED WINDOW", 13,
			new Vector2(0f, -82f), new Vector2(350f, 28f), Color.white, FontStyle.Bold);
		timerText = CreateText(root, "Timer", "12.0", 22, new Vector2(0f, -125f), new Vector2(180f, 34f),
			new Color(0.88f, 0.03f, 0.12f, 1f), FontStyle.Bold);
		CreateText(root, "Controls", "MOUSE / TOUCH / ARROW KEYS", 11, new Vector2(0f, -164f), new Vector2(300f, 24f),
			new Color(0.55f, 0.58f, 0.61f, 1f), FontStyle.Normal);
	}

	private static Image CreateImage(RectTransform parent, string name, Vector2 size, Vector2 position, Color color)
	{
		GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		item.transform.SetParent(parent, false);
		RectTransform rect = item.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = size;
		rect.anchoredPosition = position;
		Image image = item.GetComponent<Image>();
		image.color = color;
		image.raycastTarget = false;
		return image;
	}

	private static Text CreateText(RectTransform parent, string name, string value, int size, Vector2 position, Vector2 dimensions, Color color, FontStyle style)
	{
		GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
		item.transform.SetParent(parent, false);
		RectTransform rect = item.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = dimensions;
		rect.anchoredPosition = position;
		Text text = item.GetComponent<Text>();
		text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		text.text = value;
		text.fontSize = size;
		text.fontStyle = style;
		text.alignment = TextAnchor.MiddleCenter;
		text.color = color;
		text.horizontalOverflow = HorizontalWrapMode.Wrap;
		text.verticalOverflow = VerticalWrapMode.Truncate;
		text.raycastTarget = false;
		return text;
	}

	public override void InitCard(string yesText = "", string noText = "", string otherText = "", int decision = 0, bool withanim = true)
	{
		prompt.text = otherText;
		remaining = Duration;
		lockedTime = 0f;
		elapsed = 0f;
		nextSound = 0f;
		cursor = 0.5f;
		running = true;
		statusText.text = "MOVE THE WHITE PULSE INTO THE RED WINDOW";
		statusText.color = Color.white;
		progressFill.sizeDelta = new Vector2(0f, 11f);
		gameObject.SetActive(true);
		if (InputAct.diff != null)
		{
			InputAct.diff.SuspendSlideFocus();
			inputSuspended = true;
		}
	}

	private void Update()
	{
		if (!running)
		{
			return;
		}

		float dt = Time.unscaledDeltaTime;
		remaining -= dt;
		elapsed += dt;
		float target = 0.5f + Mathf.Sin(elapsed * 1.7f) * 0.28f + Mathf.Sin(elapsed * 4.3f) * 0.05f;
		target = Mathf.Clamp01(target);

		if (Input.touchCount > 0)
		{
			cursor = Mathf.Clamp01(Input.touches[0].position.x / Screen.width);
		}
		else if (InputAct.diff != null && InputAct.diff.curInput == Inputs.mouse)
		{
			cursor = Mathf.Clamp01(Input.mousePosition.x / Screen.width);
		}
		else
		{
			float direction = 0f;
			if (Input.GetKey(KeyCode.LeftArrow)) direction -= 1f;
			if (Input.GetKey(KeyCode.RightArrow)) direction += 1f;
			cursor = Mathf.Clamp01(cursor + direction * dt * 0.7f);
		}

		targetWindow.anchoredPosition = new Vector2((target - 0.5f) * TrackWidth, 0f);
		commandPulse.anchoredPosition = new Vector2((cursor - 0.5f) * TrackWidth, 0f);
		bool locked = Mathf.Abs(cursor - target) < 0.12f;
		if (locked)
		{
			lockedTime += dt;
			if (elapsed >= nextSound)
			{
				nextSound = elapsed + 1.1f;
				JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_collect);
			}
		}

		float lockRatio = Mathf.Clamp01(lockedTime / (Duration * RequiredLockRatio));
		progressFill.sizeDelta = new Vector2(TrackWidth * lockRatio, 11f);
		targetImage.color = new Color(0.88f, 0.03f, 0.12f, locked ? 0.78f : 0.42f);
		pulseImage.color = locked ? new Color(0.75f, 1f, 0.82f, 1f) : Color.white;
		statusText.text = locked ? "ALLOCATION LOCKED" : "FOLLOW THE ALLOCATION WINDOW";
		timerText.text = Mathf.Max(0f, remaining).ToString("0.0");

		if (remaining <= 0f)
		{
			running = false;
			StartCoroutine(Finish(lockRatio >= 1f));
		}
	}

	private IEnumerator Finish(bool won)
	{
		statusText.text = won ? "NETWORK STABLE" : "CASCADE DETECTED";
		statusText.color = won ? new Color(0.55f, 1f, 0.7f, 1f) : new Color(1f, 0.22f, 0.26f, 1f);
		JukeBox.diff.PlaySound(won ? SFXTypes.sfx_fight_rebalance : SFXTypes.sfx_minigame_fail);
		yield return new WaitForSecondsRealtime(1.1f);
		if (inputSuspended && InputAct.diff != null)
		{
			InputAct.diff.RestoreSlideFocus();
			inputSuspended = false;
		}
		GameAct.diff.ForceDecision(won);
		InputAct.diff.TapAction();
	}

	public override void HideCard()
	{
		running = false;
	}

	public override void Unset()
	{
		StopAllCoroutines();
		running = false;
		if (inputSuspended && InputAct.diff != null)
		{
			InputAct.diff.RestoreSlideFocus();
			inputSuspended = false;
		}
	}
}
