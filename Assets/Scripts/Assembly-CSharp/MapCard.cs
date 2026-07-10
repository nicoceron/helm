using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class MapCard : CardAct
{
	public Text tuto;

	private string routeDescription;

	public SVGImage place_img;

	public Text place_text;

	public List<SVGImage> rectangles;

	public Color UnactiveColor;

	public Color ActiveColor;

	public RectTransform circle;

	public List<RectTransform> spots;

	public RectTransform signal;

	public LineRenderer line;

	public List<RectTransform> distances;

	public List<MapSpot> MapSpots = new List<MapSpot>();

	private string nameini;

	public RectTransform launchBut;

	private int launchIn = -311;

	private int launchOut = -420;

	private int lineId;

	public SVGAsset[] iconFacility;

	private bool paused;

	private int totaldistance;

	private int navcounter;

	private Card routeCard;

	public List<MapSpot> activeSpots = new List<MapSpot>();

	private float cz;

	private Tweener tutotween;

	private Tweener linetween;

	private Vector3 middlepos = new Vector3(0f, 326f, -2f);

	private Vector3 sidepos = new Vector3(143f, 343f, -2f);

	private Vector2 initpos;

	private Vector3 curang;

	private bool isDown;

	private Tweener maptween;

	private float diffAng;

	public bool routeshown;

	private bool routebuilding;

	private void InitMap()
	{
		MapSpots.Clear();
		foreach (RectTransform spot in spots)
		{
			if (spot.name.StartsWith("spot"))
			{
				MapSpots.Add(new MapSpot(spot));
			}
		}
	}

	private void SetMap()
	{
		navcounter = 0;
		totaldistance = 0;
		BackgroundAct.diff.HideBack();
		nameini = BackgroundAct.diff.nameBack;
		place_text.text = SpeechAct.diff.GetSceneTextFinal(nameini);
		place_img.vectorGraphics = (SVGAsset)Resources.Load("icons/" + BackgroundAct.diff.curBack.type, typeof(SVGAsset));
		int num = 0;
		foreach (MapSpot mapSpot in MapSpots)
		{
			num++;
			mapSpot.type = NavigationAct.diff.GetSpotType(nameini, num);
			if (mapSpot.type == Backgrounds.none)
			{
				mapSpot.img.vectorGraphics = null;
				mapSpot.img.enabled = false;
				continue;
			}
			mapSpot.trans.DOLocalRotate(new Vector3(0f, 0f, Util.Rand(-180f, 180f)), Util.Rand(1.5f, 3f));
			mapSpot.name = NavigationAct.diff.GetSpotName(nameini, num);
			SetIcon(mapSpot);
			mapSpot.trans.anchoredPosition = mapSpot.position + new Vector2(Util.GetFloat(nameini + num + "posx", -10f, 10f), Util.GetFloat(nameini + num + "posy", -10f, 10f));
		}
		if (nameini == "Sidoma" && tutotween == null)
		{
			ActiveTuto();
		}
	}

	private void SetIcon(MapSpot s)
	{
		if (s.type == Backgrounds.defaut)
		{
			s.isSignal = true;
			s.type = Backgrounds.defaut;
			s.img.vectorGraphics = null;
			s.img.enabled = false;
		}
		else
		{
			s.img.enabled = true;
			s.isSignal = false;
			s.img.vectorGraphics = NavigationAct.diff.GetIconPlace(s.type, s.name);
		}
	}

	private bool ExistingRoad()
	{
		if (NavigationAct.diff.navigation.Count > 0)
		{
			return true;
		}
		return false;
	}

	private void AlterRoad(MapSpot s)
	{
		if (!NavigationAct.diff.HasGoal(s.name))
		{
			List<NavPoint> navigation = NavigationAct.diff.navigation;
			if (navcounter < navigation.Count)
			{
				NavPoint navPoint = navigation[navcounter];
				s.distance = navPoint.distance - totaldistance;
				s.name = navPoint.name;
				s.type = navPoint.type;
				SetIcon(s);
				totaldistance += navPoint.distance;
				navcounter++;
			}
		}
	}

	private void SelectSpotsAndShowMap()
	{
		JukeBox.diff.PlaySound(SFXTypes.sfx_map_launch_button_up);
		routeshown = true;
		routebuilding = false;
		cz = Mathf.Repeat(curang.z, 360f);
		bool flag = cz.Equals(0f) && ExistingRoad();
		if (InputAct.diff.NavigationMode())
		{
			AnimBut.diff.SwitchSize(tall: true);
			AnimBut.diff.UnLock(ControlModes.next, withoutbut: true);
		}
		activeSpots = new List<MapSpot>();
		if (tutotween != null)
		{
			return;
		}
		foreach (MapSpot mapSpot in MapSpots)
		{
			mapSpot.distance = -1;
			mapSpot.realposition = Quaternion.AngleAxis(cz, Vector3.forward) * mapSpot.trans.anchoredPosition;
			if (mapSpot.realposition.y > 0f && Mathf.Abs(mapSpot.realposition.x) < 130f && mapSpot.type != Backgrounds.none)
			{
				activeSpots.Add(mapSpot);
			}
		}
		activeSpots.Sort((MapSpot p1, MapSpot p2) => p1.rank.CompareTo(p2.rank));
		int num = -1;
		List<MapSpot> list = new List<MapSpot>();
		MapSpot item = activeSpots[0];
		foreach (MapSpot activeSpot in activeSpots)
		{
			if (activeSpot.rank > num)
			{
				num = activeSpot.rank;
				item = activeSpot;
				if (flag)
				{
					AlterRoad(activeSpot);
				}
			}
			else if (activeSpot.rank == num)
			{
				if (NavigationAct.diff.HasGoal(activeSpot.name))
				{
					list.Add(item);
				}
				else
				{
					list.Add(activeSpot);
				}
			}
		}
		foreach (MapSpot item2 in list)
		{
			activeSpots.Remove(item2);
		}
		GameAct.diff.SetBool("unknown", boo: false);
		string text = nameini + (int)cz;
		if (flag)
		{
			NavigationAct.diff.SetRouteValue(text, activeSpots.Count, full: false);
		}
		else if (GameAct.diff.GetBool("transponder") && Util.GetFloat(text + "unknown") > 0.9f)
		{
			UnselectSpots();
			activeSpots = new List<MapSpot>();
			GameAct.diff.SetBool("unknown", boo: true);
		}
		else
		{
			NavigationAct.diff.SetRouteValue(text, activeSpots.Count);
		}
		string lastPointName = ((activeSpots.Count == 0) ? SpeechAct.diff.GenerateName(text) : activeSpots[Util.GetInt(text, 0, activeSpots.Count)].name);
		NavigationAct.diff.SetLastPointName(lastPointName);
		GameAct.diff.SetRandomiserSuffix(text);
		List<Card> hiddenCards = GameAct.diff.GetHiddenCards("_route");
		routeCard = GameAct.diff.ProcessCards(hiddenCards, smallbatch: true);
		routeDescription = GameAct.diff.TreatText(routeCard.question);
		ShowLine();
	}

	private void ActiveTuto(bool left = false)
	{
		int num = ((!left) ? 1 : (-1));
		curang = new Vector3(0f, 0f, 20 * num);
		tutotween = circle.DOLocalRotate(curang, 1.7f).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			ActiveTuto(!left);
		});
	}

	private void ResetSpots()
	{
		if (InputAct.diff.NavigationMode())
		{
			AnimBut.diff.Lock();
		}
		tuto.text = "";
		routeshown = false;
		routebuilding = false;
		UnselectSpots();
	}

	private void UnselectSpots()
	{
		maptween.Kill();
		linetween.Kill();
		ResetLine();
		foreach (MapSpot activeSpot in activeSpots)
		{
			activeSpot.img.DOKill();
			activeSpot.trans.DOKill();
			activeSpot.img.color = UnactiveColor;
			activeSpot.trans.DOSizeDelta(new Vector2(4f, 4f), 0.2f).SetEase(Ease.InSine);
		}
		foreach (SVGImage rectangle in rectangles)
		{
			rectangle.enabled = false;
			for (int i = 0; i < 2; i++)
			{
				Transform child = rectangle.transform.GetChild(i);
				child.GetComponent<Text>().text = "";
				foreach (Transform item in child)
				{
					item.GetComponent<SVGImage>().vectorGraphics = null;
				}
			}
			rectangle.transform.GetChild(2).gameObject.SetActive(value: false);
		}
		foreach (RectTransform distance in distances)
		{
			distance.gameObject.SetActive(value: false);
		}
		launchBut.DOComplete();
		launchBut.DOAnchorPosY(launchOut, 0.5f).SetEase(Ease.InBack);
	}

	private void ResetLine()
	{
		linetween.Kill();
		lineId = 0;
		line.positionCount = 1;
	}

	private void ShowLine()
	{
		if (isDown)
		{
			return;
		}
		lineId++;
		Vector3 pos = line.GetPosition(line.positionCount - 1);
		line.positionCount = lineId + 1;
		line.SetPosition(line.positionCount - 1, pos);
		JukeBox.diff.PlaySound(SFXTypes.sfx_map_destination);
		if (lineId > 1)
		{
			int num = lineId - 2;
			distances[num].gameObject.SetActive(value: true);
			Vector3 position = line.GetPosition(num + 1);
			Vector3 position2 = line.GetPosition(num);
			distances[num].anchoredPosition = Quaternion.AngleAxis(cz, Vector3.back) * ((position - position2) / 2f + position2 + Vector3.up * 283f);
			distances[num].rotation = Quaternion.identity;
			int distance = (int)((position2 - position).sqrMagnitude / 1300f);
			activeSpots[num].distance = distance;
			distances[num].GetChild(0).GetComponent<Text>().text = distance.ToString();
			launchBut.DOComplete();
			launchBut.DOAnchorPosY(launchIn, 0.5f).SetEase(Ease.InOutBack);
			JukeBox.diff.PlaySound(SFXTypes.sfx_map_direction_line);
		}
		MapSpot mapSpot;
		if (lineId > activeSpots.Count)
		{
			Vector3 zero = Vector3.zero;
			if (activeSpots.Count == 0)
			{
				zero = middlepos;
				launchBut.DOComplete();
				launchBut.DOAnchorPosY(launchIn, 0.5f).SetEase(Ease.InOutBack);
				JukeBox.diff.PlaySound(SFXTypes.sfx_map_direction_line);
			}
			else
			{
				mapSpot = activeSpots[activeSpots.Count - 1];
				zero = ((mapSpot.realposition.x > 0f) ? sidepos : new Vector3(0f - sidepos.x, sidepos.y, sidepos.z));
			}
			if (linetween != null)
			{
				linetween.Kill();
			}
			linetween = DOTween.To(() => pos, delegate(Vector3 x)
			{
				pos = x;
			}, zero, 0.2f).OnUpdate(delegate
			{
				UpdateLastLine(pos);
			});
			tuto.text = routeDescription;
			return;
		}
		mapSpot = activeSpots[lineId - 1];
		Vector2 realposition = mapSpot.realposition;
		Vector3 endValue = new Vector3(realposition.x, realposition.y - 283f, -2f);
		SVGImage sVGImage = rectangles[lineId - 1];
		sVGImage.enabled = true;
		sVGImage.rectTransform.anchoredPosition = mapSpot.trans.anchoredPosition;
		sVGImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
		int index = ((realposition.x > 0f) ? 1 : 0);
		if (mapSpot.tween != null)
		{
			mapSpot.tween.Complete();
		}
		if (mapSpot.isSignal)
		{
			Transform child = sVGImage.transform.GetChild(2);
			child.gameObject.SetActive(value: true);
			child.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(32f, 32f), 0.3f)
				.From(new Vector2(14f, 14f))
				.SetLoops(-1, LoopType.Restart)
				.SetEase(Ease.OutBack);
			sVGImage.transform.GetChild(index).GetComponent<Text>().text = "? ? ?";
		}
		else
		{
			mapSpot.tween = mapSpot.img.DOColor(ActiveColor, 0.4f);
			mapSpot.trans.DOSizeDelta(new Vector2(16f, 16f), 0.3f).SetEase(Ease.OutBack);
			Transform child2 = sVGImage.transform.GetChild(index);
			child2.GetComponent<Text>().text = SpeechAct.diff.GetSceneTextFinal(mapSpot.name);
			int num2 = 0;
			for (int num3 = 0; num3 < 3; num3++)
			{
				if (NavigationAct.diff.HasFacility(mapSpot.name, num3 switch
				{
					1 => "bar", 
					0 => "concert", 
					_ => "shop", 
				}))
				{
					child2.GetChild(num2).GetComponent<SVGImage>().vectorGraphics = iconFacility[num3];
					num2++;
				}
			}
		}
		linetween = DOTween.To(() => pos, delegate(Vector3 x)
		{
			pos = x;
		}, endValue, 0.2f).OnUpdate(delegate
		{
			UpdateLastLine(pos);
		}).OnComplete(ShowLine);
	}

	private void UpdateLastLine(Vector3 newpos)
	{
		line.SetPosition(line.positionCount - 1, newpos);
	}

	private void Awake()
	{
		InitMap();
	}

	public override void InitCard(string yesText = "", string noText = "", string otherText = "", int decision = 0, bool withanim = true)
	{
		// AssetRipper preserves this card as inactive in the scene. Unity therefore
		// may not invoke Awake until SetActive below, while GameAct calls InitCard
		// directly on the inactive component. Build the runtime spot model here as
		// well so the recovered map works on its very first opening.
		if (MapSpots.Count == 0)
		{
			InitMap();
		}
		BackgroundAct.diff.HideBottom();
		foreach (MapSpot mapSpot in MapSpots)
		{
			mapSpot.distance = -1;
			mapSpot.img.enabled = false;
			mapSpot.trans.sizeDelta = new Vector2(4f, 4f);
		}
		curang = Vector3.zero;
		initpos = Vector2.zero;
		routeshown = false;
		routebuilding = false;
		paused = false;
		isDown = false;
		base.gameObject.SetActive(value: true);
		line.positionCount = 1;
		launchBut.DOComplete();
		launchBut.DOAnchorPosY(launchOut, 0.3f);
		StopCoroutine("CheckMap");
		SetMap();
		InputAct.diff.SuspendSlideFocus();
		StartCoroutine("CheckMap");
		curang = Vector3.zero;
		Mask mask = mytrans.GetComponent<Mask>();
		mask.enabled = true;
		mytrans.DOSizeDelta(new Vector2(2000f, 1000f), 0.7f).SetEase(Ease.InQuart).OnComplete(delegate
		{
			InitRoad(mask);
		});
		InputAct diff = InputAct.diff;
		diff.OnSwitchMenu = (Action<bool>)Delegate.Combine(diff.OnSwitchMenu, new Action<bool>(ShowHide));
	}

	private void ShowHide(bool open)
	{
		if (tutotween != null)
		{
			tutotween.Kill();
		}
		if (open)
		{
			UnselectSpots();
			paused = true;
		}
		else
		{
			paused = false;
			routebuilding = true;
			curang = new Vector3(0f, 0f, 45f * Mathf.Round(curang.z / 45f));
			maptween = circle.DOLocalRotate(curang, 0.7f).SetEase(Ease.OutBack).OnComplete(SelectSpotsAndShowMap);
		}
		launchBut.GetComponent<Button>().interactable = !paused;
	}

	private void StartMoveMap()
	{
		if (!isDown && !InputAct.diff.NavigationMode())
		{
			isDown = true;
			initpos = -InputAct.diff.GetPointerVirt(noagro: true);
		}
	}

	private bool TapMap(bool arg)
	{
		if (tutotween != null)
		{
			tutotween.Kill();
			tutotween = null;
			AnimBut.diff.Lock();
			routeshown = false;
			return false;
		}
		bool flag = InputAct.diff.NavigationMode();
		if (!flag && isDown)
		{
			isDown = false;
		}
		if (routeshown && flag)
		{
			Launch();
		}
		return false;
	}

	private void InitRoad(Mask mask)
	{
		BackgroundAct.diff.Activate();
		NavigationAct.diff.Activate();
		MoneyUI.diff.Activate();
		MetersAct.diff.Activate();
		initpos = -InputAct.diff.GetPointerVirt(noagro: true);
		InputAct.diff.GetActionFocus(TapMap, suspendSlide: true, StartMoveMap, tapaction: true);
		mask.enabled = false;
	}

	public void Launch()
	{
		InputAct diff = InputAct.diff;
		diff.OnSwitchMenu = (Action<bool>)Delegate.Remove(diff.OnSwitchMenu, new Action<bool>(ShowHide));
		JukeBox.diff.PlaySound(SFXTypes.sfx_map_launch_button_press);
		JukeBox.diff.PlaySound(SFXTypes.sfx_map_button_launch);
		BackgroundAct.diff.Activate();
		NavigationAct.diff.Activate();
		MoneyUI.diff.Activate();
		MetersAct.diff.Activate();
		MoneyUI.diff.HideMoney();
		ResetSpots();
		GameAct.diff.SetInt(Variables.hide, -1);
		GameAct.diff.SetNextCard("_launch");
		BackgroundAct.diff.lastBack = BackgroundAct.diff.nameBack;
		GameAct.diff.LoadCard(routeCard);
		NavigationAct.diff.SetRoute(activeSpots);
		StopCoroutine("CheckMap");
		InputAct.diff.TapAction();
		GameAct.diff.ForceDecision();
		BackgroundAct.diff.ShowBack(andreset: true);
	}

	private IEnumerator CheckMap()
	{
		while (true)
		{
			if (paused)
			{
				yield return 0;
				continue;
			}
			Vector2 pointerVirt = InputAct.diff.GetPointerVirt(noagro: true);
			if (!InputAct.diff.NavigationMode())
			{
				pointerVirt += initpos;
			}
			if ((InputAct.diff.NavigationMode() || isDown) && Mathf.Abs(pointerVirt.x) > 0.01f)
			{
				if (routeshown || routebuilding)
				{
					JukeBox.diff.PlaySound(SFXTypes.sfx_map_launch_button_down);
					ResetSpots();
					if (tutotween != null)
					{
						tutotween.Kill();
						tutotween = null;
					}
				}
				diffAng = pointerVirt.x * Time.deltaTime * 400f;
				curang = new Vector3(0f, 0f, Mathf.Repeat(curang.z + diffAng, 360f));
				circle.localRotation = Quaternion.Euler(curang);
			}
			else if (!routebuilding && !routeshown)
			{
				routebuilding = true;
				curang = new Vector3(0f, 0f, 45f * Mathf.Round(curang.z / 45f));
				maptween = circle.DOLocalRotate(curang, 0.7f).SetEase(Ease.OutBack).OnComplete(SelectSpotsAndShowMap);
			}
			yield return 0;
		}
	}

	public override void Unset()
	{
		InputAct.diff.RestoreSlideFocus();
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
