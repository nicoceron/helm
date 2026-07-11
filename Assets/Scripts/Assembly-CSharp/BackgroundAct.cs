using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundAct : MonoBehaviour
{
	public bool isiPad;

	public List<Background> backgrounds;

	public SVGImage[] backCards;

	public Text question;

	public Text who;

	public Color mainColor;

	public Color secondColor;

	public Color questionColor;

	public Color secondQuestionColor;

	public Image backUI;

	public RectTransform bottom;

	public RectTransform top;

	public List<string> placeCache = new List<string>();

	public RectTransform opki;

	public static BackgroundAct diff;

	public SVGImage ipadBack;

	private Background defautBack;

	private Action OnGlitch;

	private bool isTransitionning;

	private bool doTrans;

	public GameObject deadKingGroup;

	private Vector2 oriPos;

	public Transform allObjInGame;

	private Backgrounds tarBack = Backgrounds.none;

	public string nameBack = "SectorAlpha";

	public string lastBack = "SectorAlpha";

	public Background curBack;

	private BackAct scGenerated;

	private GameObject curGenerated;

	private int curSpot;

	private GameObject[] backUIs;

	private float lastResolutionRatio;

	public string forcename = "";

	public Backgrounds forceback = Backgrounds.none;

	private IEnumerator topcorout;

	private IEnumerator botcorout;

	private IEnumerator othercorout;

	private IEnumerator optioncorout;

	public float recSize = 1600f;

	private bool isQuick;

	private bool haslight;

	private BackProfile profile;

	private bool firstpass = true;

	private void Update()
	{
		float num = Screen.currentResolution.width / Screen.currentResolution.height;
		if (curGenerated != null && lastResolutionRatio != num)
		{
			lastResolutionRatio = num;
			num = (int)(num * 100f) / 100;
			if ((double)num > 1.77 && curGenerated.transform.localScale.x != 1.5f)
			{
				curGenerated.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
			}
			else if ((double)num <= 1.77 && curGenerated.transform.localScale.x != 1f)
			{
				curGenerated.transform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
	}

	private void Start()
	{
		backUIs = GameObject.FindGameObjectsWithTag("BackUI");
		curBack = (defautBack = backgrounds[0]);
		diff = this;
		GameAct gameAct = GameAct.diff;
		gameAct.OnNewCard = (Action<Card>)Delegate.Combine(gameAct.OnNewCard, new Action<Card>(SwitchBackground));
		GameAct gameAct2 = GameAct.diff;
		gameAct2.OnStart = (Action<GameStates>)Delegate.Combine(gameAct2.OnStart, new Action<GameStates>(FadeToLight));
		InputAct inputAct = InputAct.diff;
		inputAct.OnSwitchMenu = (Action<bool>)Delegate.Combine(inputAct.OnSwitchMenu, new Action<bool>(SwitchLight));
		GameAct gameAct3 = GameAct.diff;
		gameAct3.OnGameInit = (Action<GameSave>)Delegate.Combine(gameAct3.OnGameInit, new Action<GameSave>(LoadBack));
		PlayerPrefs.HasKey("forceportrait");
	}

	private void LoadBack(GameSave save)
	{
		if (save != null)
		{
			SetNextName(save.place_name, save.place, save.place_last);
			placeCache = save.place_cache;
		}
	}

	public void SetNextName(string name, Backgrounds back = Backgrounds.none, string last = "")
	{
		forcename = name;
		if (back != Backgrounds.none)
		{
			forceback = back;
		}
		if (!string.IsNullOrEmpty(last))
		{
			lastBack = last;
		}
	}

	public void ResetBack()
	{
		tarBack = Backgrounds.none;
		nameBack = "";
		curBack = defautBack;
	}

	private void SwitchWhenLanding(int decision)
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnValidateDecision = (Action<int>)Delegate.Remove(gameAct.OnValidateDecision, new Action<int>(SwitchWhenLanding));
		Card card = GameAct.diff.card;
		Outcome outcome = ((decision == 1) ? card.yes_outcomes.Find((Outcome it) => it.custom_name == "landing") : card.no_outcomes.Find((Outcome it) => it.custom_name == "landing"));
		if (outcome != null && outcome.value == 1)
		{
			CheckAndSwitch(card);
			return;
		}
		forceback = Backgrounds.none;
		forcename = "";
	}

	private void SwitchBackground(Card card)
	{
		if (card.yes_outcomes.Find((Outcome it) => it.custom_name == "landing") != null)
		{
			GameAct gameAct = GameAct.diff;
			gameAct.OnValidateDecision = (Action<int>)Delegate.Combine(gameAct.OnValidateDecision, new Action<int>(SwitchWhenLanding));
		}
		else
		{
			CheckAndSwitch(card);
		}
	}

	public bool Landing()
	{
		if (!NavigationAct.diff.placeToLand.Contains(forceback) && !NavigationAct.diff.placeToLand.Contains(tarBack))
		{
			return NavigationAct.diff.placeToLand.Contains(curBack.type);
		}
		return true;
	}

	public bool PlaceMatch(Backgrounds type)
	{
		if (type != forceback && type != tarBack)
		{
			return type == curBack.type;
		}
		return true;
	}

	public bool NameMatch(string n)
	{
		if (n.StartsWith("<"))
		{
			if (NavigationAct.diff.HasGoal(forcename))
			{
				return true;
			}
			if (NavigationAct.diff.HasGoal(nameBack))
			{
				return true;
			}
			return false;
		}
		if (!(n == forcename))
		{
			return n == nameBack;
		}
		return true;
	}

	public string GetNextName()
	{
		if (string.IsNullOrEmpty(forcename))
		{
			return nameBack;
		}
		return forcename;
	}

	public new Backgrounds GetType()
	{
		if (curBack != null)
		{
			return curBack.type;
		}
		return tarBack;
	}

	private void CheckAndSwitch(Card card)
	{
		bool flag = string.IsNullOrEmpty(card.place_name);
		string text = ((!string.IsNullOrEmpty(forcename)) ? forcename : ((!flag) ? (card.place_name.Equals("new") ? NavigationAct.diff.GetName(card.place) : card.place_name) : (string.IsNullOrEmpty(nameBack) ? NavigationAct.diff.GetName(card.place) : nameBack)));
		forcename = "";
		Backgrounds backgrounds = ((forceback != Backgrounds.none) ? forceback : card.place);
		if ((!NavigationAct.diff.placeToLand.Contains(tarBack) || backgrounds != Backgrounds.defaut) && (tarBack != backgrounds || !(text == nameBack)))
		{
			nameBack = text;
			tarBack = backgrounds;
			forceback = Backgrounds.none;
			CameffectAct.diff.StopEffect();
			GameAct gameAct = GameAct.diff;
			gameAct.OnNewCardSuspend = (Func<CardTypes, bool>)Delegate.Combine(gameAct.OnNewCardSuspend, new Func<CardTypes, bool>(TransitBack));
		}
	}

	private bool TransitBack(CardTypes lastcard)
	{
		if (!isTransitionning)
		{
			isTransitionning = true;
			StopCoroutine("DoTransit");
			StartCoroutine("DoTransit", backCards[0].enabled);
			return true;
		}
		if (doTrans)
		{
			return true;
		}
		GameAct gameAct = GameAct.diff;
		gameAct.OnNewCardSuspend = (Func<CardTypes, bool>)Delegate.Remove(gameAct.OnNewCardSuspend, new Func<CardTypes, bool>(TransitBack));
		isTransitionning = false;
		return false;
	}

	public void ShowTop(bool direct = false)
	{
		top.gameObject.SetActive(value: true);
		if (MetersAct.diff != null)
		{
			MetersAct.diff.Activate();
			MetersAct.diff.ShowAllData(yes: true);
		}
		if (MoneyUI.diff != null)
		{
			MoneyUI.diff.HideMoney();
		}
		DoMove(top, new Vector2(0f, -50f), 3.5f, direct, topcorout);
	}

	public void HideTop(bool direct = false)
	{
		DoMove(top, new Vector2(0f, 70f), 3f, direct, topcorout);
	}

	private void DoMove(RectTransform targ, Vector2 diff, float time, bool direct, IEnumerator corout, float delay = 0f)
	{
		if (direct)
		{
			targ.anchoredPosition = diff;
		}
		else
		{
			targ.DOAnchorPos(diff, time).SetEase(Ease.OutQuart);
		}
	}

	public void ShowBottom(bool direct = false)
	{
		NavigationAct.diff.ShowUI(curBack.type, GetDisplayPlaceName(nameBack));
		bool showNavigation = GameAct.diff != null && GameAct.diff.cardType == CardTypes.custom;
		bottom.gameObject.SetActive(showNavigation);
		if (showNavigation)
		{
			DoMove(bottom, new Vector2(0f, 50f), 3.5f, direct, botcorout);
		}
	}

	public void HideBottom(bool direct = false)
	{
		DoMove(bottom, new Vector2(0f, -50f), 3f, direct, botcorout);
	}

	public void ShowOptions(bool direct = false)
	{
		DoMove(opki, new Vector2(0f, -300f), 4f, direct, optioncorout, 3f);
	}

	private void HideOptions(bool direct = false)
	{
		DoMove(opki, new Vector2(0f, -200f), 3.5f, direct, optioncorout);
	}

	public void FadeToBlack()
	{
		SVGImage[] array = backCards;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		SetColor();
		backUI.DOColor(mainColor, 0.5f);
		Enlarge(1600f);
	}

	public void FadeToLight(GameStates state = GameStates.none)
	{
		if (state == GameStates.restart)
		{
			StartCoroutine("DelayedFade", false);
		}
		else
		{
			StartCoroutine("DelayedFade", true);
		}
	}

	private IEnumerator DelayedFade(bool withdelay)
	{
		if (withdelay)
		{
			backUI.DOFade(0f, 0.6f);
		}
		HideTop(direct: true);
		HideBottom(direct: true);
		float seconds = (withdelay ? 0.8f : 0f);
		yield return new WaitForSeconds(seconds);
		seconds = (withdelay ? 0.5f : 0f);
		yield return new WaitForSeconds(seconds);
		if (withdelay)
		{
			backUI.DOFade(1f, 0.5f);
			ShowBacks();
		}
		question.DOColor(questionColor, 0.5f);
		backUI.DOColor(mainColor, 0.5f);
		SwitchSize();
	}

	public void SwitchSize()
	{
		if (InputAct.diff.isLandscape())
		{
			Enlarge(350f);
		}
		else
		{
			Enlarge(430f);
		}
	}

	private void Enlarge(float targ)
	{
		recSize = targ;
		GameObject[] array = backUIs;
		for (int i = 0; i < array.Length; i++)
		{
			RectTransform component = array[i].GetComponent<RectTransform>();
			Vector2 endValue = new Vector2(targ, component.sizeDelta.y);
			component.DOSizeDelta(endValue, 0.4f).SetEase(Ease.OutQuad);
		}
	}

	private IEnumerator DoTransit(bool quick)
	{
		isQuick = quick;
		doTrans = true;
		HideTop();
		HideBottom();
		if (quick)
		{
			backCards[0].enabled = false;
		}
		GameAct.diff.DeleteQuestion();
		SetBack(direct: false, tarBack);
		backUI.DOFade(0f, 0.5f);
		RectTransform back = ipadBack.rectTransform;
		float speed = (quick ? 0.5f : 0.25f);
		float t = 0f;
		Vector2 opos = back.anchoredPosition;
		Vector3 oScale = back.localScale;
		Vector3 vector = new Vector3(0f, 0f, 1f);
		Vector2 tpos = new Vector2(vector.x, vector.y);
		Vector3 tscale = new Vector3(vector.z, vector.z, 1f);
		while (t < 1f)
		{
			float t2 = Easing.CubicEaseInOut(t, 0f, 1f, 1f);
			back.anchoredPosition = Vector2.LerpUnclamped(opos, tpos, t2);
			back.localScale = Vector3.LerpUnclamped(oScale, tscale, t2);
			t += Time.deltaTime * speed;
			yield return 0;
		}
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
		question.gameObject.SetActive(value: false);
		who.gameObject.SetActive(value: false);
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
		question.gameObject.SetActive(value: true);
		who.gameObject.SetActive(value: true);
	}

	private void EndTransition()
	{
		if (GameAct.diff.cardType != CardTypes.end)
		{
			backCards[0].enabled = true;
		}
		ShowTop();
		ShowBottom();
		// The landscape canvas already matches the source game's height-based layout.
		// The landing palette turns the entire question surface tan, which reads as
		// one oversized box. Scenario cards use the original dark game chrome.
		SetColor(second: false);
		doTrans = false;
	}

	public void SetColor(bool second = false)
	{
		Color endValue = (second ? secondColor : mainColor);
		Color endValue2 = (second ? secondQuestionColor : questionColor);
		backUI.DOColor(endValue, 0.4f);
		question.DOColor(endValue2, 0.3f);
		who.DOColor(endValue2, 0.3f);
	}

	private bool GoNext(bool n = false)
	{
		if (doTrans)
		{
			doTrans = false;
			AnimBut.diff.Lock();
		}
		return true;
	}

	private IEnumerator DoTransitEnd()
	{
		doTrans = true;
		HideTop();
		HideBottom();
		ShowOptions();
		GameAct.diff.DeleteQuestion();
		if (allObjInGame != null)
		{
			foreach (Transform item in allObjInGame)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		deadKingGroup.SetActive(value: true);
		if (GameAct.diff.GetInt(Variables.journey) == 0)
		{
			yield return new WaitForSeconds(1f);
		}
		else
		{
			yield return new WaitForSeconds(2.5f);
		}
		AnimBut.diff.UnLock(ControlModes.nextmenu);
		InputAct.diff.GetActionFocus(GoNext, suspendSlide: true);
		HideTop(direct: true);
	}

	private bool TransitBackEnd(CardTypes lastcard)
	{
		if (!isTransitionning)
		{
			isTransitionning = true;
			StopCoroutine("DoTransitEnd");
			StartCoroutine("DoTransitEnd");
			return false;
		}
		if (doTrans)
		{
			return false;
		}
		TurnLight(on: true);
		backCards[0].enabled = true;
		deadKingGroup.SetActive(value: false);
		GameAct gameAct = GameAct.diff;
		gameAct.OnCardHiding = (Func<CardTypes, bool>)Delegate.Remove(gameAct.OnCardHiding, new Func<CardTypes, bool>(TransitBackEnd));
		isTransitionning = false;
		return true;
	}

	public void SwitchWithTop(RectTransform target, bool hidetop)
	{
		if (othercorout != null)
		{
			StopCoroutine(othercorout);
			othercorout = null;
		}
		othercorout = DoSwitchTop(target, hidetop);
		StartCoroutine(othercorout);
	}

	private IEnumerator DoSwitchTop(RectTransform target, bool hidetop)
	{
		Vector2 targ = new Vector2(0f, 0f);
		Vector2 opo = target.anchoredPosition;
		if (hidetop)
		{
			HideTop();
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			targ = new Vector2(0f, 120f);
		}
		float t = 0f;
		while (t < 1f)
		{
			target.anchoredPosition = Vector2.Lerp(opo, targ, t);
			t += Time.deltaTime * 3f;
			yield return 0;
		}
		target.anchoredPosition = targ;
		if (!hidetop)
		{
			ShowTop();
		}
	}

	private void SwitchLight(bool on)
	{
	}

	public void TurnLight(bool on)
	{
	}

	public bool isInside()
	{
		_ = curBack;
		return false;
	}

	public void SetBackDirect(int spot = 0, Backgrounds back = Backgrounds.defaut, bool becomesDef = false)
	{
		SetBack(direct: true, back, becomesDef);
		SetBackDirect(spot);
	}

	private void SetBackDirect(int spot = 0)
	{
		Vector3 vector = new Vector3(0f, 0f, 1f);
		ipadBack.rectTransform.anchoredPosition = new Vector2(vector.x, vector.y);
		ipadBack.rectTransform.localScale = new Vector3(vector.z, vector.z, 1f);
	}

	public void SetBack(bool direct, Backgrounds back = Backgrounds.defaut, bool becomesDef = false)
	{
		_ = curBack;
		if (becomesDef)
		{
			curBack = null;
		}
		if (back == Backgrounds.defaut && defautBack != null)
		{
			curBack = defautBack;
		}
		else
		{
			curBack = backgrounds.Find((Background it) => it.type == back);
			if (becomesDef)
			{
				defautBack = curBack;
			}
		}
		if (direct)
		{
			SwitchGraphics();
		}
		else if ((bool)scGenerated)
		{
			if (curBack.type == Backgrounds.vrplanet)
			{
				CameffectAct.diff.PlayEffect(EffectStyles.vr);
			}
			scGenerated.Disappear(SwitchGraphics);
		}
		else
		{
			ipadBack.DOColor(Color.black, 0.5f).OnComplete(SwitchGraphics);
		}
	}

	private void SwitchGraphics()
	{
		CameffectAct.diff.StopEffect(ifloop: true);
		if ((bool)curGenerated)
		{
			UnityEngine.Object.Destroy(curGenerated);
			scGenerated = null;
		}
		if (curBack.generated.Count > 0)
		{
			GameObject gameObject = SelectGenerated(curBack.generated, nameBack);
			if (gameObject != null)
			{
				curGenerated = UnityEngine.Object.Instantiate(gameObject, base.transform);
				curGenerated.transform.SetAsFirstSibling();
				scGenerated = curGenerated.GetComponent<BackAct>();
				scGenerated.Appear(GetDisplayPlaceName(nameBack), EndTransition);
				ipadBack.enabled = false;
				if (profile != null && profile.appearSFX != SFXTypes.none)
				{
					JukeBox.diff.PlaySound(profile.appearSFX);
				}
				return;
			}
		}
		ipadBack.enabled = true;
		ipadBack.vectorGraphics = curBack.image;
		ipadBack.color = Color.black;
		ipadBack.DOColor(Color.white, 0.4f).OnComplete(EndTransition);
	}

	private GameObject SelectGenerated(List<BackProfile> profiles, string name)
	{
		string visualKey = GetVisualKey(nameBack);
		if (profile != null)
		{
			placeCache.Add(profile.name);
			if (placeCache.Count > 6)
			{
				placeCache.RemoveAt(0);
			}
		}
		List<BackProfile> list = new List<BackProfile>();
		List<BackProfile> list2 = new List<BackProfile>();
		foreach (BackProfile profile in profiles)
		{
			if (!string.IsNullOrEmpty(profile.name) && profile.name == visualKey)
			{
				this.profile = profile;
				Debug.Log($"HELM_BACKGROUND_RESOLVED place='{GetDisplayPlaceName(name)}' key='{visualKey}' profile='{profile.name}' prefab='{profile.prefab?.name}'");
				return profile.prefab;
			}
			if (GameAct.diff.TestCond(profile.treatedConditions))
			{
				list.Add(profile);
				if (placeCache.Contains(profile.name))
				{
					list2.Add(profile);
				}
			}
		}
		if (list.Count > list2.Count)
		{
			foreach (BackProfile item in list2)
			{
				list.Remove(item);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		BackProfile backProfile = list[Util.GetInt(visualKey, 0, list.Count)];
		if (backProfile.alternative != null && Util.GetFloat(name + "alternative") > 0.5f)
		{
			return backProfile.alternative;
		}
		this.profile = backProfile;
		Debug.Log($"HELM_BACKGROUND_RESOLVED place='{GetDisplayPlaceName(name)}' key='{visualKey}' profile='{backProfile.name}' prefab='{backProfile.prefab?.name}'");
		return backProfile.prefab;
	}

	public static string GetDisplayPlaceName(string placeName)
	{
		if (string.IsNullOrEmpty(placeName))
		{
			return placeName;
		}
		int separator = placeName.IndexOf('|');
		return separator < 0 ? placeName : placeName.Substring(0, separator);
	}

	private static string GetVisualKey(string placeName)
	{
		if (string.IsNullOrEmpty(placeName))
		{
			return placeName;
		}
		int separator = placeName.IndexOf('|');
		return separator < 0 || separator == placeName.Length - 1
			? placeName
			: placeName.Substring(separator + 1);
	}

	public void HideBack()
	{
		backCards[0].enabled = false;
	}

	public void ShowBack(bool andreset = false)
	{
		backCards[0].enabled = true;
		if (andreset)
		{
			ResetBack();
		}
	}

	public void ShowBacks(bool keepone = false)
	{
		MonoBehaviour.print("showing back and keepone " + keepone);
		StartCoroutine("DoShowBack", keepone);
	}

	private IEnumerator DoShowBack(bool keepone)
	{
		yield return null;
		yield return null;
		JukeBox.diff.PlaySound(SFXTypes.card_stacking);
		float t = 0.2f;
		int num = (keepone ? 1 : 0);
		for (int i = num; i < backCards.Length; i++)
		{
			yield return new WaitForSeconds(t);
			StartCoroutine(ShowBack(backCards[i], i));
			t -= 0.02f;
			HapticAct.diff.Tap();
		}
		yield return new WaitForSeconds(0.1f);
		HapticAct.diff.BigChange();
	}

	private IEnumerator ShowBack(SVGImage ba, int id)
	{
		ba.enabled = true;
		float t = 0f;
		Vector2 opos = new Vector2(-300f, 100f);
		Vector2 tpos = Vector2.zero;
		RectTransform trans = ba.GetComponent<RectTransform>();
		while (t < 1f)
		{
			trans.anchoredPosition = Vector2.Lerp(opos, tpos, Easing.QuintEaseOut(t, 0f, 1f, 1f));
			t += Time.deltaTime;
			yield return null;
		}
		trans.anchoredPosition = tpos;
		if (id > 0)
		{
			ba.enabled = false;
		}
	}
}
