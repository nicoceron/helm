using System;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class SpaceUI : UIAct
{
	[SerializeField]
	private SVGAsset bunnyBomb;

	[SerializeField]
	private SVGAsset normalBomb;

	public GameObject signal;

	public RectTransform[] missiles;

	public RectTransform starParent;

	public RectTransform lockedUI;

	public RectTransform planetUI;

	public SVGImage ship;

	public SVGImage enemy;

	private RectTransform shipTrans;

	private ParticleSystem shipExhaust;

	private RectTransform shipPar;

	private RectTransform enemyTrans;

	private RectTransform enemyPar;

	private ParticleSystem enemyExhaust;

	public List<RectTransform> stars;

	private List<Sequence> starSequences;

	private float lengthxpos;

	private Color textcol;

	public Text distance;

	private int distanceAmount;

	public SVGImage destination;

	public Text todestination;

	private bool shipRunning;

	private bool hasDestination;

	private bool hasLanded;

	public SVGAsset[] enemysprites;

	private Vector2 shipCenter = new Vector2(0f, 18.4f);

	private Vector2 shipSide = new Vector2(-82f, 18.4f);

	private Color destinationColor;

	private bool _isFighting;

	private int leftVariation;

	private int rightVariation;

	private int heatVariation;

	private float curHeat;

	private float effectTimer;

	public bool isLocked = true;

	private bool isSignal;

	private Tween signalTween;

	private Tween blink;

	private Vector2 initsize = new Vector2(2f, 2f);

	private Sequence shipSequence;

	private bool isFighting
	{
		get
		{
			return _isFighting;
		}
		set
		{
			SetFight(value);
		}
	}

	private void SetEnShip(Card card)
	{
		int num = Mathf.Clamp(GameAct.diff.GetInt("enemy"), 0, 5);
		enemy.vectorGraphics = enemysprites[num];
		RectTransform[] array = missiles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<SVGImage>().vectorGraphics = ((num == 4) ? bunnyBomb : normalBomb);
		}
		GameAct diff = GameAct.diff;
		diff.OnRefresh = (Action<Card>)Delegate.Remove(diff.OnRefresh, new Action<Card>(SetEnShip));
	}

	private void SetFight(bool value)
	{
		if (value == _isFighting)
		{
			return;
		}
		_isFighting = value;
		if (_isFighting)
		{
			GameAct diff = GameAct.diff;
			diff.OnValidateDecision = (Action<int>)Delegate.Combine(diff.OnValidateDecision, new Action<int>(SetFightName));
			GameAct diff2 = GameAct.diff;
			diff2.OnDataChange = (Action<Variables, int>)Delegate.Combine(diff2.OnDataChange, new Action<Variables, int>(TreatHeat));
			GameAct diff3 = GameAct.diff;
			diff3.OnChoice = (Action<int>)Delegate.Combine(diff3.OnChoice, new Action<int>(VariationHeat));
			enemyPar.gameObject.SetActive(value: true);
			enemyExhaust.Play();
			enemyTrans.anchoredPosition = new Vector2(-200f, 0f);
			enemyTrans.DOKill();
			enemyTrans.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				ShipSequence(enemyTrans);
			});
			shipPar.DOKill();
			shipPar.DOAnchorPos(shipSide, 0.5f);
			destination.enabled = false;
			todestination.enabled = false;
			distance.enabled = false;
			GameAct.diff.SetInt(Variables.stop, 1);
			JukeBox.diff.PlayMusic("fight");
			UnSetSignal();
			GameAct diff4 = GameAct.diff;
			diff4.OnRefresh = (Action<Card>)Delegate.Combine(diff4.OnRefresh, new Action<Card>(SetEnShip));
		}
		else
		{
			GameAct diff5 = GameAct.diff;
			diff5.OnValidateDecision = (Action<int>)Delegate.Remove(diff5.OnValidateDecision, new Action<int>(SetFightName));
			GameAct diff6 = GameAct.diff;
			diff6.OnDataChange = (Action<Variables, int>)Delegate.Remove(diff6.OnDataChange, new Action<Variables, int>(TreatHeat));
			GameAct diff7 = GameAct.diff;
			diff7.OnChoice = (Action<int>)Delegate.Remove(diff7.OnChoice, new Action<int>(VariationHeat));
			enemyExhaust.Stop();
			JukeBox.diff.PlayMusic();
			enemyTrans.parent.DOKill();
			shipTrans.DOKill();
			enemyTrans.DOKill();
			enemyTrans.DOAnchorPosX(-200f, 0.5f).OnComplete(delegate
			{
				enemyPar.gameObject.SetActive(value: false);
			});
			shipPar.DOKill();
			new Vector3(0f, 0f, 0f);
			shipPar.DOAnchorPos(shipCenter, 0.5f);
			RotateScene(0f);
			destination.enabled = true;
			todestination.enabled = true;
			distance.enabled = true;
			SetSignal();
		}
	}

	private void SetFightName(int decision)
	{
		if (heatVariation != 0)
		{
			GameAct.diff.AddInt(Variables.heat, heatVariation, -10, 10);
		}
		bool flag = Util.Rand() > 0.5f;
		leftVariation = (flag ? Util.RandInt(-1, -3) : Util.RandInt(1, 3));
		rightVariation = ((!flag) ? Util.RandInt(-1, -3) : Util.RandInt(1, 3));
		GameAct.diff.OpenCard("_fight");
	}

	private void VariationHeat(int decision)
	{
		if (decision != 0)
		{
			heatVariation = ((decision == 1) ? rightVariation : leftVariation);
			RotateScene(curHeat + (float)heatVariation);
		}
	}

	private void TreatHeat(Variables var, int val = 0)
	{
		if (var == Variables.heat)
		{
			curHeat = val;
			RotateScene(curHeat, withrand: true);
		}
	}

	private void CheckFight(Card card)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		bool flag = card.name.Contains("_fight");
		if (isFighting && !flag)
		{
			isFighting = false;
		}
		else if (!isFighting && flag)
		{
			isFighting = true;
		}
		if (!isFighting)
		{
			return;
		}
		int num = GameAct.diff.GetInt(Variables.heat);
		TreatHeat(Variables.heat, num);
		if (card.name.Equals("_fight"))
		{
			if (num > 8)
			{
				Attack(enemyPar, shipPar);
			}
			else if (num < 2)
			{
				Attack(shipPar, enemyPar);
			}
		}
	}

	private void Attack(RectTransform from, RectTransform to)
	{
		effectTimer = 0f;
		GameAct diff = GameAct.diff;
		diff.OnNewCardSuspend = (Func<CardTypes, bool>)Delegate.Combine(diff.OnNewCardSuspend, new Func<CardTypes, bool>(AttackEffect));
		RectTransform[] array = missiles;
		foreach (RectTransform missile in array)
		{
			Sequence s = DOTween.Sequence();
			missile.rotation = from.rotation;
			missile.anchoredPosition = from.anchoredPosition;
			float num = Util.Rand();
			float num2 = Util.RandSign();
			float num3 = Util.RandSign();
			s.AppendInterval(Util.Rand(0.1f, 0.3f)).AppendCallback(delegate
			{
				missile.gameObject.SetActive(value: true);
			}).Append(missile.DOAnchorPos(new Vector2(num * 25f * num2, (1f - num) * 25f * num3), 0.8f).SetEase(Ease.OutBack))
				.Append(missile.DOAnchorPos(to.anchoredPosition, 0.3f).SetEase(Ease.InBack))
				.AppendCallback(delegate
				{
					Boum(missile, to == shipPar);
				});
		}
	}

	private void Boum(RectTransform trans, bool sandman)
	{
		trans.gameObject.SetActive(value: false);
		CameffectAct.diff.PlayEffect(EffectStyles.boum);
		int num = Util.RandInt(0, 4);
		if (!sandman)
		{
			return;
		}
		if ((float)GameAct.diff.GetInt(Variables.hull) > 1f)
		{
			GameAct.diff.AddInt(Variables.hull, -5);
			return;
		}
		switch (num)
		{
		case 0:
			GameAct.diff.AddInt(Variables.power, Util.RandInt(-5, -15));
			break;
		case 1:
			GameAct.diff.AddInt(Variables.people, Util.RandInt(-5, -15));
			break;
		case 2:
			GameAct.diff.AddInt(Variables.oxygen, Util.RandInt(-5, -15));
			break;
		case 3:
			GameAct.diff.AddInt(Variables.power, Util.RandInt(-4, -8));
			GameAct.diff.AddInt(Variables.hull, Util.RandInt(-4, -8));
			break;
		}
	}

	private bool AttackEffect(CardTypes type)
	{
		if (effectTimer < 1.5f)
		{
			effectTimer += Time.deltaTime;
			return true;
		}
		GameAct diff = GameAct.diff;
		diff.OnNewCardSuspend = (Func<CardTypes, bool>)Delegate.Remove(diff.OnNewCardSuspend, new Func<CardTypes, bool>(AttackEffect));
		return false;
	}

	public void ShowShip()
	{
		planetUI.gameObject.SetActive(value: false);
	}

	public void ShowPlace(Backgrounds type)
	{
		StopShip();
		GameAct.diff.SetInt(Variables.stop, 1);
		planetUI.gameObject.SetActive(value: true);
		int num = 0;
		foreach (Transform item in planetUI)
		{
			if (num > 0)
			{
				item.GetComponent<PlanetButton>().alreadyseen = false;
			}
			num++;
		}
		planetUI.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(BackgroundAct.diff.recSize, planetUI.sizeDelta.y), 0.3f);
		MoneyUI.diff.ShowMoney();
		if (isLocked)
		{
			isLocked = false;
			lockedUI.DOAnchorPosY(-365f, 1.5f).SetEase(Ease.InBack);
		}
	}

	private void SetSignal()
	{
		if (isSignal)
		{
			signal.SetActive(value: true);
			signalTween = signal.transform.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(32f, 32f), 0.3f)
				.From(new Vector2(14f, 14f))
				.SetLoops(-1, LoopType.Restart)
				.SetEase(Ease.OutBack);
		}
		else
		{
			UnSetSignal();
		}
	}

	private void UnSetSignal()
	{
		isSignal = false;
		if (signalTween != null)
		{
			signalTween.Kill();
		}
		signal.SetActive(value: false);
	}

	private void SetDestination(SVGAsset sprite, int length, NavPoint point)
	{
		if (blink != null)
		{
			blink.Kill();
		}
		isSignal = !NavigationAct.diff.placeToLand.Contains(point.type);
		SetSignal();
		Color color = destination.color;
		color.a = 1f;
		destination.color = color;
		destination.vectorGraphics = (isSignal ? null : sprite);
		todestination.text = length.ToString();
		hasDestination = true;
		if (isLocked)
		{
			isLocked = false;
			lockedUI.DOAnchorPosY(-365f, 1.5f).SetEase(Ease.InBack);
		}
	}

	public void UnsetDestination()
	{
		todestination.text = "";
		destination.vectorGraphics = null;
		hasDestination = false;
		UnSetSignal();
	}

	public void UpdateDestinationSignal(NavPoint point)
	{
		if (point.distance == 0 && hasDestination)
		{
			todestination.text = "? ? ?";
			return;
		}
		if (!hasDestination)
		{
			SetDestination(null, point.distance, point);
		}
		todestination.text = point.distance.ToString();
	}

	public void UpdateDestination(SVGAsset sprite, NavPoint point)
	{
		if (point.distance == 0 && hasDestination)
		{
			todestination.text = SpeechAct.diff.GetSceneTextFinal(point.name);
			return;
		}
		if (!hasDestination)
		{
			SetDestination(sprite, point.distance, point);
		}
		todestination.text = point.distance.ToString();
	}

	public void UpdateDistance(int amount)
	{
		distance.text = amount.ToString();
	}

	private void Awake()
	{
		destinationColor = destination.color;
		shipTrans = ship.rectTransform;
		enemyTrans = enemy.rectTransform;
		shipPar = shipTrans.parent.GetComponent<RectTransform>();
		enemyPar = enemyTrans.parent.GetComponent<RectTransform>();
		shipExhaust = shipTrans.GetComponentInChildren<ParticleSystem>();
		enemyExhaust = enemyTrans.GetComponentInChildren<ParticleSystem>();
		textcol = distance.color;
	}

	private void CheckStop(Variables var, int stop)
	{
		if (var == Variables.stop && base.gameObject.activeSelf)
		{
			if (stop != 1)
			{
				StartShip();
			}
			else if (stop == 1 && !isFighting)
			{
				StopShip();
			}
		}
	}

	private Vector2 shipRandPos()
	{
		return new Vector2(Util.Rand(-5f, 5f), Util.Rand(-1f));
	}

	private float shipRandTime()
	{
		return 2f + Util.Rand(0f, 2f);
	}

	private void StartShip()
	{
		if (!shipRunning)
		{
			shipRunning = true;
			shipExhaust.Play();
			ShipSequence(shipTrans);
			for (int i = 0; i < stars.Count; i++)
			{
				RectTransform rectTransform = stars[i];
				DOTween.Kill(rectTransform);
				float speed = ((float)i + 1f) * 0.2f + 0.5f + Util.Rand(-0.4f, 0.4f);
				StarSequence(rectTransform, speed);
			}
		}
	}

	private void ShipSequence(RectTransform _trans)
	{
		if (!shipRunning)
		{
			return;
		}
		_trans.DOKill();
		switch (Util.RandInt(0, 4))
		{
		case 0:
			_trans.DOPunchAnchorPos(shipRandPos(), shipRandTime(), 4).OnComplete(delegate
			{
				ShipSequence(_trans);
			});
			break;
		case 1:
			_trans.DOShakeAnchorPos(shipRandTime(), 1f).OnComplete(delegate
			{
				ShipSequence(_trans);
			});
			break;
		case 2:
			_trans.DOAnchorPos(shipRandPos() * 2f, shipRandTime()).SetLoops(3).OnComplete(delegate
			{
				ShipSequence(_trans);
			});
			break;
		case 3:
			_trans.DOAnchorPos(shipRandPos(), shipRandTime()).OnComplete(delegate
			{
				ShipSequence(_trans);
			});
			break;
		}
	}

	private void StarSequence(RectTransform star, float speed)
	{
		if (shipRunning)
		{
			star.anchoredPosition = GenerateInitPos();
			star.sizeDelta = initsize;
			DOTween.Kill(star);
			DOTween.Sequence().AppendInterval(Util.Rand(0f, 0.8f)).Append(star.DOAnchorPosX(Util.Rand(-240f, -260f), speed).SetEase(Ease.InCirc))
				.Join(star.DOSizeDelta(new Vector2(Util.Rand(20f, 25f) - speed * 3f, 1.5f), speed))
				.AppendCallback(delegate
				{
					StarSequence(star, speed);
				});
		}
	}

	private void RotateScene(float deg, bool withrand = false)
	{
		float z = (withrand ? (Mathf.PingPong(deg, 10f) * 18f + Util.Rand(-4f, 4f)) : (Mathf.PingPong(deg, 10f) * 18f));
		float duration = Util.Rand(0.5f, 1.2f);
		Vector3 endValue = new Vector3(0f, 0f, z);
		DOTween.Kill(101);
		shipPar.DOLocalRotate(endValue, duration).SetEase(Ease.OutBack).SetId(101);
		starParent.DOLocalRotate(endValue, duration).SetEase(Ease.OutBack).SetId(101);
		enemyPar.DOLocalRotate(endValue, duration).SetEase(Ease.OutBack).SetId(101);
	}

	private Vector2 GenerateInitPos()
	{
		return new Vector2(250f, Util.Rand(-50f, 100f));
	}

	private void StopShip()
	{
		shipRunning = false;
		shipExhaust.Stop();
		shipTrans.anchoredPosition = Vector2.zero;
	}

	private void OnDisable()
	{
		StopShip();
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		GameAct diff = GameAct.diff;
		diff.OnNewCard = (Action<Card>)Delegate.Combine(diff.OnNewCard, new Action<Card>(CheckFight));
		GameAct diff2 = GameAct.diff;
		diff2.OnDataChange = (Action<Variables, int>)Delegate.Combine(diff2.OnDataChange, new Action<Variables, int>(CheckStop));
		StartShip();
		if (SpeechAct.diff.asiaLayout)
		{
			if (SpeechAct.diff.lang == "jp")
			{
				distance.fontSize = 21;
			}
			else
			{
				distance.fontSize = 28;
			}
		}
	}
}
