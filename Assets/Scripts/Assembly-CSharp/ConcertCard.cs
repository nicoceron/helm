using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConcertCard : CardAct
{
	private const int TutorialConcertCardId = 27;

	private const int HelmConcertCardId = 104;

	private const float DefaultMinimumAccuracy = 0.6f;

	private const float HelmMinimumAccuracy = 0.7f;

	public List<BackgroundGroup> backgrounds;

	public List<Guitar> guitars;

	private Guitar guitarObject;

	public AnimationCurve verticalCurve;

	private Action<int, float> OnBeat;

	private Action<float, float> OnNote;

	private Action<MusEffects> OnEffect;

	public SVGAsset wholeHeart;

	public SVGAsset brokenHeart;

	public List<MovingHeartAct> hearts;

	public List<SVGRenderer> heartsUI;

	public SVGAsset sparkleft;

	public SVGAsset sparkright;

	public SVGImage spark;

	public Transform guitar;

	private Transform guitchild;

	public LineRenderer line;

	public LineRenderer secondline;

	public List<Transform> people;

	private List<SVGRenderer> peopleImg = new List<SVGRenderer>();

	public RawImage peopleBack;

	private List<float> peopleTime = new List<float>();

	public Color frontColor;

	private Color frontColorCur;

	private Color backColorCur;

	public List<Color> backColors;

	private AudioClip music;

	private SongProfile profile;

	public Light spotlight;

	public int lineCount = 100;

	private float maxYLine = -500f;

	private float minYLine = 270f;

	private float segmentLength;

	private float timetotal;

	private Tweener lineTween;

	private List<float> linePos;

	private bool simulator;

	private float timer;

	private float totalTime;

	private float minpeople;

	private float maxpeople = -600f;

	private float minsize = 1f;

	private float maxsize = 2f;

	private float peopleamount;

	public bool isPlaying;

	private float lineLimit = 0.05f;

	private bool isOnline;

	private int _heartNb;

	public RectTransform heartResult;

	public TMP_Text beatResult;

	public TMP_Text maxBeatResult;

	public TMP_Text tutoText;

	private List<TunnelColor> tunnelColors = new List<TunnelColor>();

	private bool congratHeart;

	private int woawId;

	public List<string> woawText;

	public GameObject progressBack;

	public LineRenderer progressLine;

	public TMP_Text progressText;

	public GameObject progressBrokenPrefab;

	private float progressLineMin = -22f;

	private float progressGuitarMin = -15f;

	private float _lineWidth;

	private float lineBump;

	private int beatId;

	private int noteId;

	private int effectId;

	private bool _speedseen;

	private float lastbeat;

	private int beatspeed;

	private float note = 4.5f;

	private float _oldtimestamp;

	private List<MusEvent> beats = new List<MusEvent>();

	private float decal = 1.6f;

	private List<GameObject> progressBroken = new List<GameObject>();

	private int _heartCounter;

	private bool maxHeart;

	private bool alternateStyle;

	private float peopleX;

	private int backColId;

	private float currentBPM = 60f;

	private float modifier;

	private float side = 1f;

	private float woawamo;

	private float sensibility = 0.5f;

	private float xanchor;

	private float doubleline;

	private Vector2[] lineSmooth = new Vector2[6];

	private bool isFinishing;

	private int _beatMax;

	private BackgroundStyles _style;

	private float currentSide = 1f;

	private float currentDouble;

	private float currentSin;

	private bool heartfalling;

	private bool badhearts;

	private float steepCurve = 1f;

	private bool speedheart;

	private bool barrageOn;

	private bool firstStart = true;

	private Color lineColor1;

	private Color lineColor2;

	private Tweener moveTween;

	private Tweener rotateTween;

	public Tweener superTween;

	private float decalGuitar;

	private float yGuitar = 1f;

	private bool isInitialised;

	private bool isSuspended;

	public int heartNb
	{
		get
		{
			return _heartNb;
		}
		set
		{
			ConfigureHeart(value);
		}
	}

	private float lineWidth
	{
		get
		{
			return _lineWidth;
		}
		set
		{
			_lineWidth = value;
			SetLineWidth(value);
		}
	}

	private BackgroundStyles curStyle
	{
		get
		{
			return _style;
		}
		set
		{
			ConfigureStyle(value);
		}
	}

	private void Awake()
	{
		lineWidth = 0f;
		foreach (Transform person in people)
		{
			peopleImg.Add(person.GetComponent<SVGRenderer>());
			peopleTime.Add(0f);
		}
		peopleamount = maxpeople - minpeople;
	}

	private void FixedUpdate()
	{
		if (isPlaying)
		{
			timer += Time.fixedDeltaTime;
			UpdateSong();
			UpdateLine(note);
			UpdateProgress();
			UpdatePeople();
		}
	}

	private void UpdateSong()
	{
		if (OnBeat != null)
		{
			MusEvent musEvent = beats[beatId];
			float num = musEvent.value * 0.0095f;
			if (musEvent.timestamp - num <= timer)
			{
				_speedseen = false;
				if (speedheart)
				{
					lastbeat = (musEvent.timestamp - beats[beatId - 1].timestamp) / 4f;
				}
				currentBPM = (speedheart ? (musEvent.value * 1.7f) : musEvent.value);
				beatspeed = 3;
				OnBeat(beatId, currentBPM);
				beatId++;
				if (beatId == beats.Count)
				{
					OnBeat = null;
				}
			}
			else if (speedheart && beatspeed > 0 && musEvent.timestamp - lastbeat * (float)beatspeed <= timer)
			{
				beatspeed--;
				OnBeat(-1, currentBPM);
			}
		}
		if (OnNote != null)
		{
			MusEvent musEvent2 = profile.noteChange[noteId];
			float num2 = Mathf.Clamp(musEvent2.timestamp - 4.2f, 0f, 500f);
			if (num2 <= timer)
			{
				OnNote(num2, musEvent2.value);
				noteId++;
				if (noteId == profile.noteChange.Count)
				{
					OnNote = null;
				}
			}
		}
		if (OnEffect == null)
		{
			return;
		}
		MusEffect musEffect = profile.customChange[effectId];
		if (Mathf.Clamp(musEffect.timestamp - 4.2f, 0f, 500f) <= timer)
		{
			OnEffect(musEffect.effect);
			effectId++;
			if (effectId == profile.customChange.Count)
			{
				OnEffect = null;
			}
		}
	}

	private void NoteChange(float timestamp, float target)
	{
		float endValue = GetNote(target);
		float num = (timestamp - _oldtimestamp) * guitarObject.smooth;
		_oldtimestamp = timestamp;
		float delay = 0f;
		if (num > steepCurve)
		{
			num = steepCurve;
		}
		lineTween.Kill();
		lineTween = DOTween.To(() => note, delegate(float x)
		{
			note = x;
		}, endValue, num * steepCurve).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Fixed)
			.SetDelay(delay)
			.SetId(9);
	}

	private float GetNote(float input)
	{
		return (input / 0.9f - 5f) * 10f;
	}

	private void UpdateProgress()
	{
		float num = timer / totalTime;
		if (!(num > 1f))
		{
			progressLine.SetPosition(1, new Vector3(progressLineMin - 2f * progressLineMin * num, 0f, -350f));
		}
	}

	private void AddBrokenProgress()
	{
		float num = timer / totalTime;
		GameObject gameObject = UnityEngine.Object.Instantiate(progressBrokenPrefab, progressBack.transform);
		gameObject.transform.localPosition = new Vector3(progressLineMin - 2f * progressLineMin * num, -0.63f, -351f);
		progressBroken.Add(gameObject);
	}

	private void HideProgress(bool andreset = false)
	{
		if (andreset)
		{
			foreach (GameObject item in progressBroken)
			{
				UnityEngine.Object.Destroy(item);
			}
			progressBroken = new List<GameObject>();
		}
		progressBack.SetActive(value: false);
		progressText.gameObject.SetActive(value: false);
	}

	private void ShowProgress()
	{
		progressBack.SetActive(value: true);
		progressText.gameObject.SetActive(value: true);
		if (InputAct.diff.longPortrait)
		{
			RectTransform component = progressBack.GetComponent<RectTransform>();
			Vector3 anchoredPosition3D = component.anchoredPosition3D;
			anchoredPosition3D.z = -150f;
			anchoredPosition3D.y = 60f;
			component.anchoredPosition3D = anchoredPosition3D;
			RectTransform component2 = progressText.GetComponent<RectTransform>();
			anchoredPosition3D = component2.anchoredPosition3D;
			anchoredPosition3D.z = -560f;
			anchoredPosition3D.y = 22.4f;
			component2.anchoredPosition3D = anchoredPosition3D;
		}
	}

	private void MoveHeartsUI(float ypos, float duration = 0.3f)
	{
	}

	private void PopHeart(int beatNb, float bpm, float xpo)
	{
		bool flag = false;
		foreach (MovingHeartAct heart in hearts)
		{
			if (flag)
			{
				heart.Pop(bpm, maxHeart);
			}
			else if (heart.PopAndMove(bpm, timer, maxHeart, heartfalling, badhearts, xpo, beatNb))
			{
				flag = true;
			}
		}
	}

	private void PopHeart(int beatNb, float bpm)
	{
		bool flag = false;
		foreach (MovingHeartAct heart in hearts)
		{
			if (flag)
			{
				heart.Pop(bpm, maxHeart);
			}
			else if (heart.PopAndMove(bpm, timer, maxHeart, heartfalling, badhearts, 0f, beatNb))
			{
				if (beatNb == beats.Count && !simulator)
				{
					string t = woawText[woawText.Count - Util.RandInt(1, 4)];
					heart.ShowText(t);
				}
				else if (congratHeart && !simulator)
				{
					int value = Mathf.RoundToInt(((float)woawText.Count - 4f) * ((timer + Util.Rand(-3f, 3f)) / timetotal));
					string t2 = woawText[Mathf.Clamp(value, 0, woawText.Count - 4)];
					heart.ShowText(t2);
					congratHeart = false;
				}
				flag = true;
			}
		}
	}

	public void RemoveHeart(bool addbeatmax = false)
	{
		if (InputAct.diff.curInput == Inputs.automated)
		{
			return;
		}
		if (addbeatmax)
		{
			_beatMax++;
			UpdateProgressText();
		}
		if (!isFinishing)
		{
			if (heartNb > 0)
			{
				heartNb--;
				HapticAct.diff.Tap(iOSHapticFeedback.iOSFeedbackType.Failure);
				CameffectAct.diff.NormalScreenShake();
				AddBrokenProgress();
			}
			else if (!barrageOn)
			{
				EndGame(won: false);
			}
		}
	}

	public void AddHeart(bool silent = false, bool addbeatmax = false)
	{
		HapticAct.diff.Tap(iOSHapticFeedback.iOSFeedbackType.SelectionChange);
		CameffectAct.diff.SmoothScreenShake();
		_heartCounter++;
		if (addbeatmax)
		{
			_beatMax++;
		}
		UpdateProgressText();
		heartNb++;
		DOTween.Complete(99);
		if (!silent)
		{
			guitchild.DOPunchPosition(Vector3.down * 12f * (0.5f + decalGuitar), 0.3f, 2).SetId(99);
			spotlight.DOComplete();
			spotlight.DOIntensity(1.7f, 0.3f).From();
		}
	}

	private bool MeetsAccuracyRequirement()
	{
		if (InputAct.diff.curInput == Inputs.automated || GameAct.diff.card.id == TutorialConcertCardId)
		{
			return true;
		}
		float minimumAccuracy = ((GameAct.diff.card.id == HelmConcertCardId) ? HelmMinimumAccuracy : DefaultMinimumAccuracy);
		return _beatMax > 0 && (float)_heartCounter / (float)_beatMax >= minimumAccuracy;
	}

	private void UpdateProgressText()
	{
		progressText.text = _heartCounter + "/" + _beatMax;
		progressText.transform.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.3f).From().SetEase(Ease.InSine);
	}

	private void ConfigureHeart(int value)
	{
		_heartNb = Mathf.Clamp(value, 0, 2);
		maxHeart = _heartNb == 2;
		if (maxHeart && lineWidth.Equals(1f))
		{
			ChangeLineWidth(1.3f, 0.1f, 1f);
		}
	}

	private void SetHeartUI(int id, SVGAsset asset)
	{
		heartsUI[id].vectorGraphics = asset;
	}

	private void PopTunnelLight(bool backward = false)
	{
		if (backward)
		{
			CameffectAct.diff.PlayEffect(EffectStyles.concert);
		}
		else
		{
			CameffectAct.diff.PlayEffect(EffectStyles.concertlight);
		}
		float position = ((!backward) ? 1 : (-1));
		float endValue = (backward ? 1 : (-1));
		TunnelColor tunnel = new TunnelColor(Color.white, position);
		tunnelColors.Add(tunnel);
		DOTween.To(() => tunnel.position, delegate(float x)
		{
			tunnel.position = x;
		}, endValue, 6f).OnComplete(delegate
		{
			tunnelColors.Remove(tunnel);
		}).SetId(9);
	}

	private void InitPeople()
	{
		for (int i = 0; i < people.Count; i++)
		{
			peopleTime[i] = (float)i / (float)people.Count;
			people[i].localEulerAngles = new Vector3(-90f, Util.Rand(-180f, 180f), 0f);
			peopleImg[i].vectorGraphics = (simulator ? SelectAssetStyle(BackgroundStyles.mechanic) : SelectAssetStyle(BackgroundStyles.crowd));
		}
	}

	private void HidePeople()
	{
		line.enabled = false;
		secondline.enabled = false;
		foreach (SVGRenderer item in peopleImg)
		{
			item.enabled = false;
		}
	}

	private Vector2 GetLineStyle(float amo, float ypo)
	{
		return GetLinePos(ypo);
	}

	private SVGAsset SelectAssetStyle(BackgroundStyles style, int id = 0)
	{
		List<SVGAsset> assets = backgrounds.Find((BackgroundGroup it) => it.style == style).assets;
		if (assets.Count == 1)
		{
			return assets[0];
		}
		return assets[(id + 1) * 22 % assets.Count];
	}

	private SVGAsset SelectAssetStyle(int id = 0)
	{
		List<SVGAsset> assets = backgrounds.Find((BackgroundGroup it) => it.style == curStyle).assets;
		if (assets.Count == 1)
		{
			return assets[0];
		}
		return assets[(id + 1) * 22 % assets.Count];
	}

	private void SetStyle(bool def = false)
	{
		if (def)
		{
			Congrat();
			curStyle = (alternateStyle ? guitarObject.styleVariation : (simulator ? BackgroundStyles.mechanic : BackgroundStyles.crowd));
		}
		else
		{
			alternateStyle = false;
			curStyle = guitarObject.style;
		}
	}

	private void UpdatePeople()
	{
		frontColorCur = Color.Lerp(frontColorCur, frontColor, Time.deltaTime * 0.5f);
		Color color = Color.Lerp(peopleBack.color, backColorCur, Time.deltaTime * 0.8f);
		peopleBack.color = color;
		for (int i = 0; i < people.Count; i++)
		{
			Transform transform = people[i];
			SVGRenderer sVGRenderer = peopleImg[i];
			float num = Mathf.Repeat(peopleTime[i] + timer * 0.7f, 1f);
			Vector3 localPosition = transform.localPosition;
			Color color2 = Color.Lerp(color, frontColorCur, num * 1.4f);
			if (tunnelColors.Count == 0)
			{
				peopleImg[i].color = color2;
			}
			else
			{
				foreach (TunnelColor tunnelColor in tunnelColors)
				{
					float num2 = (Mathf.Abs(tunnelColor.position - num) + 0.01f) * 4f;
					if (num < 1f)
					{
						sVGRenderer.color = Color.Lerp(tunnelColor.color, color2, num2);
					}
					if (!(num2 < 2f))
					{
						continue;
					}
					SVGAsset sVGAsset = SelectAssetStyle(i);
					if (sVGRenderer.vectorGraphics != sVGAsset)
					{
						if (guitarObject.regularRotation)
						{
							transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
						}
						sVGRenderer.vectorGraphics = sVGAsset;
					}
				}
			}
			transform.localScale = new Vector3(5f + num * 16f, 5f + num * 16f, 1f);
			float num3 = minpeople + peopleamount * (num * num);
			Vector2 lineStyle = GetLineStyle(num * num, num3);
			float num4 = ((lineStyle.y < localPosition.y) ? ((lineStyle.x + localPosition.x) / 2f) : lineStyle.x);
			transform.localPosition = new Vector3(num4, num3, -24f + lineStyle.y);
			transform.localEulerAngles = new Vector3(-90f, transform.localEulerAngles.y - num4 * Time.deltaTime * 0.15f, 0f);
		}
	}

	private IEnumerator ColorCycle()
	{
		int colId = 0;
		while (true)
		{
			colId++;
			yield return new WaitForSeconds(6f);
			while (!isPlaying)
			{
				yield return 0;
			}
			if (colId == backColors.Count)
			{
				colId = 0;
			}
		}
	}

	private void InitLine()
	{
		InitPeople();
		line.positionCount = lineCount;
		secondline.positionCount = lineCount;
		segmentLength = (maxYLine - minYLine) / (float)lineCount;
		linePos = new List<float>();
		for (int i = 0; i < lineCount; i++)
		{
			line.SetPosition(i, new Vector3(0f, minYLine + (float)i * segmentLength, -4f));
			secondline.SetPosition(i, new Vector3(0f, minYLine + (float)i * segmentLength, -4f));
			linePos.Add(0f);
		}
	}

	private void UpdateLine(float amount)
	{
		float num = Time.fixedTime * (float)Math.PI * 2f * currentBPM / 120f;
		modifier = Mathf.Lerp(modifier, currentSin, Time.deltaTime * 0.5f);
		doubleline = Mathf.Lerp(doubleline, currentDouble, Time.deltaTime);
		amount += Mathf.Sin(modifier * num) * 30f * modifier;
		linePos.Insert(0, amount);
		linePos.RemoveAt(linePos.Count - 1);
		xanchor = Mathf.Lerp(xanchor, (0f - linePos[linePos.Count - 10]) * 0.1f, Time.deltaTime * 8f);
		float num2 = 0f;
		for (int num3 = lineCount - 1; num3 > -1; num3--)
		{
			float num4 = Mathf.Cos(modifier * (num * 0.5f + Time.deltaTime * (float)num3 * 0.5f));
			float num5 = (float)Mathf.Clamp(num3 - 10, 0, lineCount) / (float)lineCount;
			float num6 = 0.8f * (1f - Mathf.Pow(num5, 0.2f));
			float y = minYLine + (float)num3 * segmentLength;
			float num7 = num2 * (0.5f + steepCurve / 2f) + linePos[num3] * num6 / steepCurve;
			num2 = num7;
			float z = -4f + 30f * verticalCurve.Evaluate((3f + Time.realtimeSinceStartup - num5) * 0.1f) * (1f - num5);
			line.SetPosition(num3, new Vector3((num7 + xanchor + doubleline * 8f * num4) * currentSide, y, z));
			secondline.SetPosition(num3, new Vector3((num7 + xanchor - doubleline * 8f * num4) * currentSide, y, z));
		}
		for (int i = 0; i < lineSmooth.Length; i++)
		{
			float ypos = minYLine + segmentLength * (float)lineCount / (float)(lineSmooth.Length - i);
			lineSmooth[i] = GetLinePos(ypos);
		}
	}

	private void ChangeLineWidth(float multiplier, float duration = 0.3f, float fromto = -1f)
	{
		float endValue = multiplier;
		if (fromto > -1f)
		{
			lineWidth = multiplier;
			endValue = fromto;
		}
		DOTween.To(() => lineWidth, delegate(float x)
		{
			lineWidth = x;
		}, endValue, duration);
	}

	private void SetLineWidth(float multiplier = 1f)
	{
		line.startWidth = 1f * multiplier;
		line.endWidth = 8f * multiplier;
		secondline.startWidth = 1f * multiplier;
		secondline.endWidth = 8f * multiplier;
	}

	private Vector2 GetLineSmooth(float amo, float max = 1f)
	{
		int num = lineSmooth.Length - 1;
		int num2 = Mathf.FloorToInt(Mathf.Clamp((float)num * amo / max, 0f, lineSmooth.Length - 1));
		int num3 = Mathf.CeilToInt(Mathf.Clamp((float)num * amo / max, 0f, lineSmooth.Length - 1));
		if (num2 == num3)
		{
			return lineSmooth[num2];
		}
		float num4 = (float)num2 * max / (float)num;
		float num5 = (float)num3 * max / (float)num;
		float t = (amo - num4) / (num5 - num4);
		return Vector2.Lerp(lineSmooth[num2], lineSmooth[num3], t);
	}

	public Vector2 GetLinePos(float ypos, bool second = false)
	{
		Vector3 vector = (second ? secondline.GetPosition(Mathf.Clamp(Mathf.RoundToInt((ypos - minYLine) / segmentLength), 0, secondline.positionCount - 1)) : line.GetPosition(Mathf.Clamp(Mathf.RoundToInt((ypos - minYLine) / segmentLength), 0, line.positionCount - 1)));
		return new Vector2(vector.x, vector.z);
	}

	private void HeartBeatResult()
	{
		heartResult.localScale = new Vector3(190f, 190f, 1f);
		heartResult.DOScale(new Vector3(160f, 160f, 1f), Mathf.Clamp(50f / currentBPM - 0.04f, 0.1f, 1f)).SetDelay(0.1f).SetEase(Ease.InOutBack)
			.OnComplete(HeartBeatResult);
	}

	private void HideResult()
	{
		heartResult.DOAnchorPosY(-450f, 0f);
		beatResult.rectTransform.DOAnchorPosY(-450f, 0f);
		maxBeatResult.enabled = false;
		tutoText.enabled = false;
	}

	private void EndGame(bool won)
	{
		StartCoroutine("DoEndGame", won);
	}

	private IEnumerator DoEndGame(bool won)
	{
		isFinishing = true;
		firstStart = true;
		yield return new WaitForSeconds(0.3f);
		if (won)
		{
			DOTween.To(() => yGuitar, delegate(float x)
			{
				yGuitar = x;
			}, 0.1f, 0.6f);
			JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_end_guitar);
		}
		else
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_fail);
			DOTween.To(() => yGuitar, delegate(float x)
			{
				yGuitar = x;
			}, 1.2f, 0.6f);
		}
		lineTween.Kill();
		OnBeat = null;
		OnNote = null;
		OnEffect = null;
		yield return new WaitForSeconds(0.6f);
		ChangeLineWidth(0f, 0.4f);
		StopCoroutine("MoveGuitar");
		guitar.gameObject.SetActive(value: false);
		if (won)
		{
			heartResult.GetComponent<SVGRenderer>().vectorGraphics = wholeHeart;
			HeartBeatResult();
			heartResult.DOAnchorPosY(90f, 0.8f).SetEase(Ease.OutSine);
			beatResult.rectTransform.DOAnchorPosY(-65f, 0.9f).SetDelay(0.2f).SetEase(Ease.OutSine);
		}
		else
		{
			heartResult.GetComponent<SVGRenderer>().vectorGraphics = brokenHeart;
			heartResult.DOAnchorPosY(0f, 0.8f).SetEase(Ease.OutSine);
		}
		int beatShown = 0;
		int beatingHearts = _heartCounter;
		if (won)
		{
			beatResult.text = "0";
			float _hearttime = (float)beatingHearts * 0.012f;
			DOTween.To(() => beatShown, delegate(int x)
			{
				beatShown = x;
			}, beatingHearts, _hearttime).OnUpdate(delegate
			{
				beatResult.text = beatShown.ToString();
			});
			float t = 0f;
			while (t < _hearttime)
			{
				JukeBox.diff.PlaySound(SFXTypes.sfx_score_tally);
				t += 0.1f;
				yield return new WaitForSeconds(0.1f * Mathf.Clamp(1f - t, 0f, 1f));
			}
		}
		else
		{
			beatResult.text = "";
		}
		tutoText.enabled = true;
		bool perfect = _beatMax == beatingHearts;
		tutoText.text = (perfect ? SpeechAct.diff.GetSceneTextFinal("perfect") : SpeechAct.diff.GetSceneTextFinal("letgotouch"));
		tutoText.DOColor(Color.clear, 0.3f).From();
		yield return new WaitForSeconds(0.3f);
		if (won)
		{
			if (perfect)
			{
				HapticAct.diff.Tap(iOSHapticFeedback.iOSFeedbackType.Success);
			}
			else
			{
				HapticAct.diff.Tap(iOSHapticFeedback.iOSFeedbackType.ImpactHeavy);
			}
			maxBeatResult.enabled = true;
			maxBeatResult.text = "/" + _beatMax;
			maxBeatResult.DOColor(Color.clear, 0.3f).From();
		}
		else
		{
			maxBeatResult.text = "";
		}
		if (perfect)
		{
			DeadCloneAct.diff.AddStat("g_" + guitarObject.name);
		}
		yield return new WaitForSeconds(0.3f);
		if (GameAct.diff.card.id == TutorialConcertCardId)
		{
			GameAct.diff.scCh.ShowName(GameAct.diff.TreatText(SpeechAct.diff.GetSceneTextFinal("song_tuto")));
		}
		else
		{
			GameAct.diff.scCh.ShowName(SpeechAct.diff.GetSceneTextFinal(guitarObject.name + "_name"), GameAct.diff.TreatText(SpeechAct.diff.GetSceneText("song_credit")));
		}
		AnimBut.diff.SwitchSize(tall: true);
		AnimBut.diff.UnLock(ControlModes.next, withoutbut: true);
		while (isPlaying)
		{
			yield return 0;
		}
		AnimBut.diff.Lock();
		HideProgress(andreset: true);
		yield return new WaitForSeconds(0.5f);
		tutoText.enabled = false;
		MoveHeartsUI(480f);
		if (!simulator && won)
		{
			int val = Mathf.RoundToInt(1000f * ((float)beatingHearts / (float)_beatMax));
			GameAct.diff.AddInt(Variables.money, val);
			yield return new WaitForSeconds(0.5f);
			DOTween.To(() => beatShown, delegate(int x)
			{
				beatShown = x;
			}, 0, 0.6f).OnUpdate(delegate
			{
				beatResult.text = beatShown.ToString();
			});
		}
		yield return new WaitForSeconds(1f);
		DOTween.Kill(9);
		if (perfect)
		{
			GameAct.diff.SetInt("lastperfect", guitarObject.id);
		}
		JukeBox.diff.StopSong();
		GameAct.diff.ForceDecision(won);
		InputAct.diff.TapAction();
		foreach (MovingHeartAct heart in hearts)
		{
			heart.Stop();
		}
		if (!simulator)
		{
			JukeBox.diff.FadeStopSound(SFXTypes.sfx_minigame_pause_loop, 1.5f);
		}
		if (InputAct.diff.curInput == Inputs.mouse && !Cursor.visible)
		{
			Cursor.visible = true;
		}
	}

	private IEnumerator SuperSong()
	{
		timer = 0f;
		timetotal = profile.customChange[profile.customChange.Count - 1].timestamp;
		OnBeat = (Action<int, float>)Delegate.Combine(OnBeat, new Action<int, float>(PopHeart));
		OnNote = (Action<float, float>)Delegate.Combine(OnNote, new Action<float, float>(NoteChange));
		OnEffect = (Action<MusEffects>)Delegate.Combine(OnEffect, new Action<MusEffects>(NewEffect));
		InitLine();
		MoveHeartsUI(395f, 0.5f);
		while (!isPlaying)
		{
			yield return 0;
		}
		JukeBox.diff.PlaySong(music);
	}

	public override void Unset()
	{
		DOTween.Kill(9);
		UnityEngine.Object.Destroy(guitchild.gameObject);
		guitarObject = null;
		isInitialised = false;
		InputAct.diff.RestoreSlideFocus();
		InputAct.diff.CancelTapAction();
		StopAllCoroutines();
		ShowHide(open: true);
		isSuspended = false;
		InputAct diff = InputAct.diff;
		diff.OnSwitchMenu = (Action<bool>)Delegate.Remove(diff.OnSwitchMenu, new Action<bool>(ShowHide));
		JukeBox.diff.PlayMusic();
	}

	private void ConfigureStyle(BackgroundStyles style)
	{
		if (_style == style)
		{
			return;
		}
		_style = style;
		if (style == guitarObject.styleVariation)
		{
			JukeBox.diff.TransitionToSnapshot("Concert");
			StartCoroutine(WaitAndPop(backwards: false));
			backColorCur = guitarObject.mainColor;
			frontColor = guitarObject.mainBackColor;
			return;
		}
		switch (style)
		{
		case BackgroundStyles.crowd:
			JukeBox.diff.TransitionToSnapshot("Default");
			StartCoroutine(WaitAndPop(backwards: false));
			backColorCur = guitarObject.mainColor;
			frontColor = guitarObject.mainBackColor;
			break;
		case BackgroundStyles.mechanic:
			JukeBox.diff.TransitionToSnapshot("Concert");
			StartCoroutine(WaitAndPop(backwards: false));
			backColorCur = guitarObject.mainColor;
			frontColor = guitarObject.mainBackColor;
			break;
		default:
			JukeBox.diff.TransitionToSnapshot("Concert");
			PopTunnelLight(backward: true);
			backColorCur = guitarObject.complementaryColor;
			frontColor = guitarObject.complementaryBackColor;
			break;
		}
	}

	private IEnumerator WaitAndPop(bool backwards)
	{
		yield return new WaitForSeconds(4f);
		while (!isPlaying)
		{
			yield return 0;
		}
		if (_style == BackgroundStyles.crowd)
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_crowd_cheer);
		}
		else
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_fx_starstopeople);
		}
		PopTunnelLight(backwards);
	}

	private void Congrat()
	{
		StartCoroutine(YieldCongrats());
	}

	private IEnumerator YieldCongrats()
	{
		yield return new WaitForSeconds(2f);
		if (curStyle == BackgroundStyles.crowd)
		{
			congratHeart = true;
		}
	}

	private bool SwitchBool(bool variable)
	{
		if (variable)
		{
			variable = false;
			Congrat();
			SetStyle(def: true);
		}
		else
		{
			variable = true;
			SetStyle();
		}
		return variable;
	}

	private void NewEffect(MusEffects effect)
	{
		switch (effect)
		{
		case MusEffects.cancel:
			currentSin = 0f;
			currentDouble = 0f;
			ChangeLineWidth(1f);
			heartfalling = false;
			badhearts = false;
			speedheart = false;
			SetStyle(def: true);
			DOTween.To(() => steepCurve, delegate(float x)
			{
				steepCurve = x;
			}, 1f, 2f).SetDelay(0.5f).SetId(9);
			break;
		case MusEffects.barrage:
			StartCoroutine("PopBarrage");
			break;
		case MusEffects.speedhearts:
			speedheart = SwitchBool(speedheart);
			break;
		case MusEffects.badhearts:
			badhearts = SwitchBool(badhearts);
			break;
		case MusEffects.steep:
		{
			float endValue2 = 1f;
			if (steepCurve.Equals(1f))
			{
				endValue2 = 0.1f;
				SetStyle();
			}
			else
			{
				SetStyle(def: true);
			}
			DOTween.To(() => steepCurve, delegate(float x)
			{
				steepCurve = x;
			}, endValue2, 2f).SetDelay(0.5f).SetId(9);
			break;
		}
		case MusEffects.doubleline:
			if (currentDouble > 0f)
			{
				ChangeLineWidth(1f);
				currentDouble = 0f;
				SetStyle(def: true);
			}
			else
			{
				ChangeLineWidth(0.7f);
				currentDouble = 1f;
				SetStyle();
			}
			break;
		case MusEffects.inverse:
		{
			float endValue = 0f - Mathf.Sign(currentSide);
			DOTween.To(() => currentSide, delegate(float x)
			{
				currentSide = x;
			}, endValue, 1.5f).SetDelay(1f).SetEase(Ease.OutBack)
				.SetId(9);
			PopTunnelLight();
			Congrat();
			break;
		}
		case MusEffects.falling:
			heartfalling = SwitchBool(heartfalling);
			break;
		case MusEffects.noline:
			if (lineWidth > 0f)
			{
				ChangeLineWidth(0f);
				SetStyle();
			}
			else
			{
				SetStyle(def: true);
				ChangeLineWidth(1f);
			}
			break;
		case MusEffects.backchange:
			if (curStyle == BackgroundStyles.crowd || curStyle == BackgroundStyles.mechanic)
			{
				SetStyle();
			}
			else
			{
				alternateStyle = true;
			}
			break;
		case MusEffects.sinwave:
			if (currentSin > 0f)
			{
				currentSin = 0f;
				SetStyle(def: true);
			}
			else
			{
				SetStyle();
				currentSin = 1f;
			}
			break;
		case MusEffects.endgame:
			EndGame(MeetsAccuracyRequirement());
			break;
		}
	}

	private IEnumerator PopBarrage()
	{
		PopTunnelLight();
		barrageOn = true;
		yield return new WaitForSeconds(1.5f);
		for (int i = 0; i < 8; i++)
		{
			float num = Util.GetFloat(timer.ToString() + i, -16f, 16f);
			PopHeart(-1, currentBPM, num);
			yield return new WaitForSeconds(Mathf.Clamp(num * 0.001f, 0f, 0.2f));
		}
		yield return new WaitForSeconds(4f);
		barrageOn = false;
	}

	private void StartMoveGuitar()
	{
		if (!isSuspended && !isFinishing)
		{
			CameffectAct.diff.StopEffect();
			CameffectAct.diff.SetConcertVolume(ison: true);
			isPlaying = true;
			if (InputAct.diff.curInput == Inputs.mouse && Cursor.visible)
			{
				Cursor.visible = false;
			}
			if (firstStart)
			{
				JukeBox.diff.StopAllSoundAndMusic();
				firstStart = false;
				StartCoroutine("SuperSong");
			}
			else
			{
				JukeBox.diff.Restart();
				DOTween.Play(9);
				JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_resume);
			}
			if (!simulator)
			{
				JukeBox.diff.StopSound(SFXTypes.sfx_minigame_pause_loop);
				JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_crowd_loop, fadeIn: true);
			}
			mytrans.DOKill();
			mytrans.DOSizeDelta(new Vector2(2000f, 1000f), 0.5f).SetEase(Ease.InQuart);
			StartCoroutine("MoveGuitar");
		}
	}

	private IEnumerator MoveGuitar()
	{
		yield return new WaitForSeconds(0.5f);
		GameAct.diff.scCh.ShowName();
		BackgroundAct.diff.Deactivate();
		NavigationAct.diff.Deactivate();
		MoneyUI.diff.Deactivate();
		MetersAct.diff.Deactivate();
		lineWidth = 0f;
		line.enabled = true;
		secondline.enabled = true;
		ChangeLineWidth(1f);
		ShowProgress();
		foreach (SVGRenderer rend in peopleImg)
		{
			rend.enabled = true;
			DOTween.To(() => rend.color, delegate(Color color)
			{
				rend.color = color;
			}, backColorCur, 0.3f);
		}
		yGuitar = 1f;
		bool haspunch = false;
		Vector2 lastpos = Vector2.zero;
		float currentZrot = 0f;
		float lastpunch = 0f;
		while (true)
		{
			Vector2 pointerVirt = InputAct.diff.GetPointerVirt();
			float num = -380f * yGuitar;
			Vector2 vector = GetLinePos(num);
			float x = vector.x;
			float y = vector.y;
			Vector3 b = Vector3.Lerp(b: new Vector3(x, num, -4f), a: new Vector3(pointerVirt.x * 150f, num, y - 6f), t: 0f);
			decalGuitar = Mathf.Clamp(Vector2.SqrMagnitude(lastpos - pointerVirt) * 5f / Time.deltaTime, 0f, 1f);
			guitar.localRotation = Quaternion.Slerp(guitar.localRotation, Quaternion.Euler(new Vector3(0f, Mathf.Clamp((0f - b.x) * 3f, -45f, 45f), (0f - b.x) * 0.6f)), Time.deltaTime * 4f);
			guitar.localPosition = Vector3.Lerp(guitar.localPosition, b, Time.deltaTime * 24f);
			mytrans.localPosition = Vector3.Lerp(mytrans.localPosition, new Vector3((0f - b.x) * 0.7f, mytrans.localPosition.y, mytrans.localPosition.z), Time.deltaTime);
			float num2 = Mathf.Clamp(Mathf.Abs(guitar.localPosition.x - x) / 80f - 0.3f, 0f, 1f);
			if (num2 < lineLimit && !isOnline)
			{
				isOnline = true;
			}
			else if (num2 > lineLimit && isOnline)
			{
				isOnline = false;
			}
			if (decalGuitar > 0.07f)
			{
				if (!haspunch)
				{
					haspunch = true;
					currentZrot = ((!(pointerVirt.x > lastpos.x)) ? ((!currentZrot.Equals(-360f)) ? (-360) : 0) : ((!currentZrot.Equals(360f)) ? 360 : 0));
					lastpunch = timer;
					superTween.Kill();
					superTween = guitchild.DOLocalRotate(new Vector3(0f, 180f, currentZrot), 2.5f - decalGuitar, RotateMode.FastBeyond360).SetEase(Ease.OutBack);
				}
			}
			else if (decalGuitar < 0.02f && haspunch && timer - lastpunch > 0.4f)
			{
				haspunch = false;
			}
			lastpos = pointerVirt;
			yield return 0;
		}
	}

	private bool SwitchMoveGuitar(bool justpause)
	{
		if (isSuspended)
		{
			return false;
		}
		if (isPlaying)
		{
			if ((bool)AnimBut.diff)
			{
				AnimBut.diff.SwitchSize(tall: true);
				AnimBut.diff.UnLock(ControlModes.next, withoutbut: true);
			}
			StopMoveGuitar(justpause);
		}
		else if (!isFinishing)
		{
			if ((bool)AnimBut.diff)
			{
				AnimBut.diff.Lock();
			}
			StartMoveGuitar();
		}
		return false;
	}

	private bool StopMoveGuitar(bool justpause)
	{
		if (isSuspended)
		{
			return false;
		}
		BackgroundAct.diff.Activate();
		NavigationAct.diff.Activate();
		MoneyUI.diff.Activate();
		MetersAct.diff.Activate();
		if (!simulator)
		{
			JukeBox.diff.FadeStopSound(SFXTypes.sfx_minigame_crowd_loop, 1.5f);
		}
		if (isFinishing)
		{
			GameAct.diff.DeleteQuestion();
		}
		else
		{
			if (!simulator)
			{
				JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_pause_loop, fadeIn: true);
			}
			JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_pause);
			Card card = GameAct.diff.SelectCard("_duringconcert");
			if (card != null)
			{
				GameAct.diff.ChangeQuestion(card.question);
			}
			string text = (GameAct.diff.GetBool("mobile_keep") ? SpeechAct.diff.GetSceneTextFinal("concertpause") : SpeechAct.diff.GetSceneTextFinal("concertpausemouse"));
			GameAct.diff.scCh.ShowName(text);
			HideProgress();
		}
		if (justpause)
		{
			guitar.DOLocalMove(new Vector3(0f, guitar.localPosition.y, -4f), 0.5f).SetEase(Ease.OutBack);
		}
		isPlaying = false;
		new Color2(backColorCur, backColorCur);
		foreach (SVGRenderer rend in peopleImg)
		{
			DOTween.To(() => rend.color, delegate(Color x)
			{
				rend.color = x;
			}, backColorCur, 0.3f);
		}
		foreach (MovingHeartAct heart in hearts)
		{
			heart.HideText();
		}
		ChangeLineWidth(0f);
		mytrans.DOKill();
		mytrans.DOSizeDelta(new Vector2(360f, 360f), 0.5f).SetEase(Ease.InQuart).SetDelay(0.3f);
		CameffectAct.diff.SetConcertVolume(ison: false);
		JukeBox.diff.Pause();
		DOTween.Pause(9);
		StopCoroutine("MoveGuitar");
		StartCoroutine("YieldHide");
		if (InputAct.diff.curInput == Inputs.mouse && !Cursor.visible)
		{
			Cursor.visible = true;
		}
		return false;
	}

	private IEnumerator YieldHide()
	{
		yield return new WaitForSeconds(0.3f);
		HidePeople();
		Vector3 localPosition = mytrans.localPosition;
		localPosition.x = 0f;
		mytrans.localPosition = localPosition;
	}

	public override void InitCard(string yesText = "", string noText = "", string otherText = "", int decision = 0, bool withanim = true)
	{
		if (isSuspended || isInitialised)
		{
			return;
		}
		isInitialised = true;
		JukeBox.diff.StopMusic();
		InputAct diff = InputAct.diff;
		diff.OnSwitchMenu = (Action<bool>)Delegate.Combine(diff.OnSwitchMenu, new Action<bool>(ShowHide));
		simulator = GameAct.diff.GetBool("simulator");
		if (!simulator)
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_pause_loop, fadeIn: true);
		}
		int idGuitar = GameAct.diff.GetInt("idguitar");
		if (idGuitar < 1)
		{
			idGuitar = 1;
		}
		mytrans.sizeDelta = new Vector2(360f, 360f);
		guitarObject = guitars.Find((Guitar it) => it.id == idGuitar);
		if (GameAct.diff.card.id == TutorialConcertCardId)
		{
			GameAct.diff.scCh.ShowName(SpeechAct.diff.GetSceneTextFinal("song_tuto"));
		}
		else
		{
			GameAct.diff.scCh.ShowName(SpeechAct.diff.GetSceneTextFinal(guitarObject.name + "_name"), GameAct.diff.TreatText(SpeechAct.diff.GetSceneText("song_credit")));
		}
		backColorCur = guitarObject.mainColor;
		peopleBack.color = guitarObject.mainColor;
		curStyle = (simulator ? BackgroundStyles.mechanic : BackgroundStyles.crowd);
		frontColorCur = guitarObject.mainColor;
		effectId = 0;
		noteId = 0;
		beatId = 0;
		profile = guitarObject.profile;
		_heartCounter = 0;
		_speedseen = false;
		lastbeat = 0f;
		beatspeed = 0;
		note = 4.5f;
		_oldtimestamp = 0f;
		spotlight.intensity = 0.9f;
		music = guitarObject.clip;
		verticalCurve = guitarObject.verticalCurve;
		totalTime = profile.customChange.Find((MusEffect it) => it.effect == MusEffects.endgame).timestamp;
		guitar.gameObject.SetActive(value: true);
		guitar.localPosition = new Vector3(0f, -380f, -4f);
		GameObject gameObject = UnityEngine.Object.Instantiate(guitarObject.prefab, guitar);
		guitchild = gameObject.transform;
		guitchild.localScale = new Vector3(450f, 450f, 450f);
		if (InputAct.diff.longPortrait)
		{
			guitchild.localPosition = new Vector3(0f, -10f, 0f);
			guitchild.localScale = new Vector3(650f, 650f, 650f);
		}
		currentSide = 1f;
		currentDouble = 0f;
		currentSin = 0f;
		doubleline = 0f;
		modifier = 0f;
		side = 1f;
		xanchor = 0f;
		currentDouble = 0f;
		_lineWidth = 0f;
		heartfalling = false;
		badhearts = false;
		speedheart = false;
		alternateStyle = false;
		tunnelColors = new List<TunnelColor>();
		DOTween.To(() => steepCurve, delegate(float x)
		{
			steepCurve = x;
		}, 1f, 2f).SetDelay(0.5f).SetId(9);
		woawId = 0;
		woawText = SpeechAct.diff.GetSceneTexts("woaw");
		foreach (MovingHeartAct heart in hearts)
		{
			heart.guitar = guitchild;
		}
		isFinishing = false;
		heartNb = 2;
		MoveHeartsUI(480f, 0f);
		HideResult();
		HidePeople();
		GameAct.diff.ChangeQuestion(otherText);
		base.gameObject.SetActive(value: true);
		CaptureInput();
		beats = new List<MusEvent>();
		float num = profile.customChange.Find((MusEffect it) => it.effect == MusEffects.endgame).timestamp - 5f;
		for (int num2 = 0; num2 < profile.beatChange.Count; num2++)
		{
			MusEvent musEvent = profile.beatChange[num2];
			float num3 = ((num2 + 1 < profile.beatChange.Count) ? (profile.beatChange[num2 + 1].timestamp - 0.1f) : num);
			bool flag = true;
			float timestamp = musEvent.timestamp;
			float value = musEvent.value;
			float num4 = 60f / value;
			float num5 = 0f;
			while (flag)
			{
				float num6 = timestamp + num4 * num5;
				if (num6 > num3)
				{
					flag = false;
					continue;
				}
				beats.Add(new MusEvent(num6, value));
				num5 += 1f;
			}
		}
		_beatMax = beats.Count;
		progressText.text = _heartCounter + "/" + _beatMax;
	}

	private void CaptureInput()
	{
		InputAct.diff.SuspendSlideFocus();
		if (InputAct.diff.curInput == Inputs.touch)
		{
			InputAct.diff.GetActionFocus(StopMoveGuitar, suspendSlide: true, StartMoveGuitar, tapaction: true);
			return;
		}
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.SwitchSize(tall: true);
			AnimBut.diff.UnLock(ControlModes.next, withoutbut: true);
		}
		InputAct.diff.GetActionFocus(SwitchMoveGuitar, suspendSlide: true, null, tapaction: true);
	}

	private void ShowHide(bool open)
	{
		if (open)
		{
			guitchild.gameObject.SetActive(value: false);
			StopMoveGuitar(justpause: true);
			isSuspended = true;
		}
		else
		{
			isSuspended = false;
			guitchild.gameObject.SetActive(value: true);
			CaptureInput();
		}
	}

	public override void UpdateCard(string yesText, string noText, string question = "")
	{
	}

	public override void DefaultImage()
	{
	}

	public override void ShowDecision(int dec)
	{
	}

	public override void LerpToPos(Vector2 target, float amount)
	{
	}

	public override void SlerpToPos(float xp, float yp)
	{
	}

	public override void RotateTo(float ang)
	{
	}

	public override void Disappear(Vector2 vec, bool nodecision)
	{
	}
}
