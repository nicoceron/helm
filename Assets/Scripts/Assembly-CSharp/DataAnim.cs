using System;
using System.Collections;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class DataAnim : MonoBehaviour
{
	public GameObject haloScrew;

	public SVGImage[] screws;

	public GameObject smoke;

	public Variables variable;

	public bool isShown;

	public SVGImage mask;

	public SVGImage skull;

	public RectTransform gauge;

	private Graphic gaugeGra;

	public SVGImage arrow;

	public SVGAsset upArrow;

	public SVGAsset downArrow;

	public SVGAsset unknownArrow;

	public bool isLock;

	private bool isMoving;

	public Text add;

	public Color dangerCol;

	public Color lockCol;

	public Color normCol;

	public Color normMaskCol;

	public Color screwNormalCol;

	public Color screwLitSmallCol;

	public Color screwLitBigCol;

	private Color jaugeCol;

	private Color maskCol;

	private RectTransform addTrans;

	public Text amount;

	public SVGImage lockIcon;

	public SVGImage moveIcon;

	private int dataReal;

	private int dataShown;

	private int addReal;

	private int addShown;

	private int hiddenAdd = 116;

	private int shownAdd = 44;

	private SVGImage image;

	private bool isGauge;

	private bool stayHidden;

	private bool winter;

	private RectTransform trans;

	private Action<Outcome> Moveco;

	private Outcome moveco;

	private bool animated;

	private bool indanger;

	private bool inperfection;

	private Tweener gaugeTween;

	private Tweener maskTween;

	private bool isVisible;

	private void Awake()
	{
		addTrans = add.rectTransform;
		SwitchGauge();
		SetData(50);
		trans = GetComponent<RectTransform>();
	}

	private void Start()
	{
		GameAct diff = GameAct.diff;
		diff.OnStart = (Action<GameStates>)Delegate.Combine(diff.OnStart, new Action<GameStates>(HideDanger));
	}

	public void Init(Variables va, int amount = 50)
	{
		variable = va;
		SetData(amount);
	}

	private void DataInit(GameStates state)
	{
		Init(variable, GameAct.diff.GetInt(variable));
	}

	public void ShowDataCol(bool yes)
	{
		isShown = yes;
		float t = (yes ? 0.3f : 0.01f);
		jaugeCol = (yes ? normCol : lockCol);
		maskCol = (yes ? normMaskCol : lockCol);
		gaugeGra.enabled = (yes ? true : false);
		CrossFadeColor(mask, maskCol, t);
		CrossFadeColor(gaugeGra, jaugeCol, t);
	}

	private void CrossFadeColor(Graphic target, Color tcol, float t, bool accentonorigin = true)
	{
		StartCoroutine(DoCrossFade(target, tcol, t, accentonorigin));
	}

	private IEnumerator DoCrossFade(Graphic target, Color tcol, float totaltime, bool intro)
	{
		float t = 0f;
		Color ocol = target.color;
		while (t < 1f)
		{
			float t2 = (intro ? Easing.CubicEaseIn(t, 0f, 1f, 1f) : Easing.CubicEaseOut(t, 0f, 1f, 1f));
			target.color = Color.Lerp(ocol, tcol, t2);
			t += Time.deltaTime / totaltime;
			yield return 0;
		}
		target.color = tcol;
	}

	public void SwitchGauge(bool doit = true)
	{
		if (!doit)
		{
			isGauge = false;
			Text text = amount;
			bool flag = (add.enabled = true);
			text.enabled = flag;
		}
		else
		{
			isGauge = true;
			Text text2 = amount;
			bool flag = (add.enabled = false);
			text2.enabled = flag;
			gaugeGra = gauge.GetComponent<Graphic>();
		}
	}

	public void SetData(int value)
	{
		dataReal = (dataShown = value);
		UpdateAmount();
	}

	public void UnLock()
	{
		isLock = false;
		mask.gameObject.SetActive(value: true);
		UpdateAmount();
		lockIcon.enabled = false;
	}

	public void Lock()
	{
		if (!addTrans)
		{
			Awake();
		}
		isLock = true;
		mask.gameObject.SetActive(value: false);
		lockIcon.enabled = true;
		if (isMoving)
		{
			moveIcon.enabled = false;
		}
	}

	public void SetAdd(int value, DataDisplay effect = DataDisplay.none, float strength = 0f)
	{
		haloScrew.SetActive(value: false);
		MoveScrewColor(screwNormalCol, 0f);
		SetData(dataReal);
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		float num = (float)GameAct.diff.GetInt(Variables.overall) * 0.002f;
		int num2 = Mathf.RoundToInt(((value < 0) ? Mathf.Clamp((float)dataReal / 70f + num, 0.6f, 1.2f) : Mathf.Clamp(1f - (float)dataReal / 180f - num, 0.6f, 1.2f)) * (float)value * strength);
		value = ((effect == DataDisplay.fullamount) ? (value * (int)strength - dataReal) : ((effect == DataDisplay.towards && dataReal > 50) ? SumUp(dataReal, -num2) : SumUp(dataReal, num2)));
		if (value == 0 || isLock)
		{
			addReal = 0;
			HideAdd();
			return;
		}
		ShowAdd(Mathf.Abs(num2) > 14);
		switch (effect)
		{
		default:
			return;
		case DataDisplay.none:
		case DataDisplay.moving:
		case DataDisplay.fullamount:
		case DataDisplay.towards:
			if (!stayHidden)
			{
				StopAmountCo();
				addShown = (addReal = value);
				UpdateAdd();
				return;
			}
			break;
		case DataDisplay.hidden:
			break;
		case DataDisplay.locked:
			return;
		}
		ScrambleAmount(value);
	}

	private int SumUp(int all, int addata)
	{
		int num = all + addata;
		if (num >= 1)
		{
			if (num <= 99)
			{
				return addata;
			}
			return 100 - all;
		}
		return -all;
	}

	private void OnEnable()
	{
		jaugeCol = normCol;
		maskCol = normMaskCol;
		CrossFadeColor(gaugeGra, jaugeCol, 0.3f);
		CrossFadeColor(mask, maskCol, 0.3f);
		if (Moveco != null)
		{
			Moveco(moveco);
			Moveco = (Action<Outcome>)Delegate.Remove(Moveco, new Action<Outcome>(Move));
		}
		DirectHide();
	}

	private void OnDisable()
	{
		if (Moveco == null && isMoving && moveco != null)
		{
			Moveco = (Action<Outcome>)Delegate.Combine(Moveco, new Action<Outcome>(Move));
		}
	}

	public void Move(Outcome outco)
	{
		moveco = outco;
		if (!base.gameObject.activeInHierarchy)
		{
			Moveco = (Action<Outcome>)Delegate.Combine(Moveco, new Action<Outcome>(Move));
			return;
		}
		isMoving = true;
		StopCoroutine("DoMove");
		StartCoroutine("DoMove", outco);
	}

	public void Stop()
	{
		isMoving = false;
		moveIcon.enabled = false;
		StopCoroutine("DoMove");
	}

	public void StayHidden()
	{
		stayHidden = true;
	}

	public void DontStayHidden()
	{
		stayHidden = false;
	}

	private void ShowMoveIcon(float val)
	{
		moveIcon.enabled = true;
		moveIcon.vectorGraphics = ((Mathf.Sign(val) > 0f) ? upArrow : downArrow);
	}

	private IEnumerator DoMove(Outcome outco)
	{
		yield return 0;
		if (!isLock)
		{
			ShowMoveIcon(outco.value);
		}
		while (true)
		{
			if (GameAct.diff.state == GameStates.interreign)
			{
				yield return new WaitForSeconds(1f);
				continue;
			}
			while (InputAct.diff.isInMenu)
			{
				yield return new WaitForSeconds(1f);
			}
			while (isLock)
			{
				yield return new WaitForSeconds(1f);
				if (!isLock)
				{
					ShowMoveIcon(outco.value);
				}
			}
			if (outco == null)
			{
				Stop();
				yield break;
			}
			if (outco.value == 0)
			{
				break;
			}
			float abs = Mathf.Abs(outco.value);
			yield return new WaitForSeconds(1f / abs);
			if ((dataReal > 0 && outco.value < 0) || (dataReal < 100 && outco.value > 0))
			{
				int num = (int)Mathf.Sign(outco.value);
				dataReal += num;
				dataShown += num;
				UpdateAmount();
				StartCoroutine(MoveMoveIcon(num, 1f / abs));
				if (isVisible)
				{
					addShown = SumUp(dataReal, addShown);
					addReal = SumUp(dataReal, addReal);
					UpdateAdd();
				}
			}
		}
		Stop();
	}

	private IEnumerator MoveMoveIcon(float sign, float time)
	{
		float t = 0f;
		while (t < 1f)
		{
			moveIcon.rectTransform.anchoredPosition = new Vector2(18f, 10f - sign * 6f + 6f * t * sign);
			t += Time.deltaTime * time;
			yield return 0;
		}
	}

	public int ResolveAddition()
	{
		dataReal = GameAct.diff.GetInt(variable);
		StopAmountCo();
		if (addReal == 0)
		{
			return dataReal;
		}
		UpdateAdd();
		if (!isLock)
		{
			dataReal += addReal;
		}
		StartCoroutine("Exchange");
		return dataReal;
	}

	private IEnumerator Exchange()
	{
		addReal = 0;
		yield return StartCoroutine("ReachData");
		addTrans.anchoredPosition = new Vector2(addTrans.anchoredPosition.x, hiddenAdd);
	}

	private IEnumerator ReachData()
	{
		if (dataReal > 90 && !inperfection)
		{
			inperfection = true;
			gaugeTween.Kill();
			maskTween.Kill();
			gaugeGra.color = normCol;
			mask.color = normMaskCol;
			gaugeTween = gaugeGra.DOColor(Color.white, 0.6f).SetEase(Ease.InOutFlash).SetLoops(-1)
				.SetDelay(0.2f);
			maskTween = mask.DOColor(Color.white, 0.6f).SetEase(Ease.InOutFlash).SetLoops(-1)
				.SetDelay(0.2f);
		}
		if (dataReal < 20 && !indanger)
		{
			switch (variable)
			{
			case Variables.hull:
				JukeBox.diff.PlaySound(SFXTypes.sfx_class_empty_hull);
				break;
			case Variables.oxygen:
				JukeBox.diff.PlaySound(SFXTypes.sfx_class_empty_oxygen);
				break;
			case Variables.people:
				JukeBox.diff.PlaySound(SFXTypes.sfx_class_empty_people);
				break;
			case Variables.power:
				JukeBox.diff.PlaySound(SFXTypes.sfx_class_empty_power);
				break;
			}
			smoke.SetActive(value: true);
			float z = ((Util.Rand() > 0.5f) ? Util.Rand(8f, 22f) : Util.Rand(-22f, -8f));
			indanger = true;
			trans.DOAnchorPosY(190f + Util.Rand(0f, 5f), 0.4f).SetEase(Ease.OutBounce, 10f).SetDelay(0.2f);
			trans.DORotate(new Vector3(0f, 0f, z), 1f).SetEase(Ease.OutBounce, 4f).SetDelay(0.2f);
			gaugeTween.Kill();
			maskTween.Kill();
			gaugeGra.color = normCol;
			mask.color = normMaskCol;
			gaugeTween = gaugeGra.DOColor(dangerCol, 0.6f).SetEase(Ease.InOutFlash).SetLoops(-1)
				.SetDelay(0.2f);
			maskTween = mask.DOColor(dangerCol, 0.6f).SetEase(Ease.InOutFlash).SetLoops(-1)
				.SetDelay(0.2f);
			CameffectAct.diff.NewDanger(base.name);
		}
		if ((dataReal > 20 && indanger) || (dataReal < 90 && inperfection))
		{
			if (indanger)
			{
				switch (variable)
				{
				case Variables.hull:
					JukeBox.diff.PlaySound(SFXTypes.sfx_class_repair_hull);
					break;
				case Variables.oxygen:
					JukeBox.diff.PlaySound(SFXTypes.sfx_class_repair_oxygen);
					break;
				case Variables.people:
					JukeBox.diff.PlaySound(SFXTypes.sfx_class_repair_people);
					break;
				case Variables.power:
					JukeBox.diff.PlaySound(SFXTypes.sfx_class_repair_power);
					break;
				}
				smoke.SetActive(value: false);
				trans.DOAnchorPosY(200f, 0.4f);
				trans.DORotate(new Vector3(0f, 0f, 0f), 0.4f).SetEase(Ease.OutBack);
				CameffectAct.diff.RemoveDanger(base.name);
			}
			gaugeTween.Kill();
			maskTween.Kill();
			gaugeGra.color = normCol;
			mask.color = normMaskCol;
			indanger = false;
			inperfection = false;
		}
		animated = true;
		UpdateAdd();
		if (addShown != addReal)
		{
			yield return 0;
			if (!indanger && !inperfection)
			{
				Color color = ((dataReal > dataShown) ? Color.white : Color.red);
				gaugeGra.color = color;
				mask.color = color;
				gaugeGra.DOColor(normCol, 0.5f).SetEase(Ease.InCirc);
				mask.DOColor(normMaskCol, 0.5f).SetEase(Ease.InCirc);
			}
			yield return new WaitForSeconds(0.3f);
			while (dataShown != dataReal)
			{
				dataShown = ((dataReal < dataShown) ? (dataShown - 1) : (dataShown + 1));
				addShown = ((addReal < addShown) ? (addShown - 1) : (addShown + 1));
				UpdateAdd(nocol: true);
				UpdateAmount();
				float f = dataShown - dataReal;
				yield return new WaitForSeconds(Time.deltaTime * 6f / (1f + Mathf.Abs(f)));
			}
			DirectHide();
			animated = false;
		}
	}

	public void OpenEffect(Effect effect)
	{
		StopCoroutine("ApplyEffect");
		StartCoroutine("ApplyEffect", effect);
	}

	private IEnumerator ApplyEffect(Effect effect)
	{
		while (animated)
		{
			yield return 0;
		}
		CrossFadeColor(mask, lockCol, 0.1f, accentonorigin: false);
		CrossFadeColor(gaugeGra, lockCol, 0.1f, accentonorigin: false);
		lockIcon.vectorGraphics = (SVGAsset)Resources.Load("effects/" + effect.tag, typeof(SVGAsset));
		lockIcon.enabled = true;
		lockIcon.color = lockCol;
		yield return new WaitForSeconds(0.12f);
		float t = 0f;
		while (t < 1f)
		{
			lockIcon.color = Color.Lerp(lockCol, Color.white, Easing.QuadEaseOut(t, 0f, 1f, 1f));
			t += Time.deltaTime * 7f;
			yield return 0;
		}
		t = 0f;
		while (t < 1f)
		{
			lockIcon.color = Color.Lerp(Color.white, lockCol, Easing.QuadEaseIn(t, 0f, 1f, 1f));
			t += Time.deltaTime * 6f;
			yield return 0;
		}
		CrossFadeColor(mask, normMaskCol, 0.1f);
		CrossFadeColor(gaugeGra, jaugeCol, 0.1f);
		lockIcon.enabled = false;
		yield return new WaitForSeconds(0.12f);
	}

	private void StopAmountCo()
	{
		StopCoroutine("Exchange");
		StopCoroutine("ReachData");
		StopCoroutine("Scramble");
		StopCoroutine("Rotate");
	}

	private void DirectHide()
	{
		MoveScrewColor(screwNormalCol, 0.01f);
		haloScrew.SetActive(value: false);
	}

	private void UpdateAdd(bool nocol = false)
	{
		if (addShown == 0)
		{
			DirectHide();
		}
	}

	private void UpdateAmount()
	{
		if (!isGauge)
		{
			amount.text = dataShown.ToString();
		}
		gauge.anchoredPosition = new Vector2(0f, -40f + (float)dataShown * 0.4f);
	}

	private void ScrambleAmount(int value)
	{
		StopAmountCo();
		addReal = (addShown = value);
		StartCoroutine("Scramble");
	}

	private IEnumerator Scramble()
	{
		CrossFadeColor(add, Color.grey, 0.3f);
		add.text = "?";
		yield return 0;
	}

	private void HideAdd()
	{
		isVisible = false;
		MoveScrewColor(screwNormalCol);
		haloScrew.SetActive(value: false);
	}

	private void ShowAdd(bool bigimpact)
	{
		isVisible = true;
		if (bigimpact)
		{
			MoveScrewColor(screwLitBigCol);
			haloScrew.SetActive(value: true);
		}
		else
		{
			MoveScrewColor(screwLitSmallCol);
			haloScrew.SetActive(value: false);
		}
	}

	private void MoveScrewColor(Color targ, float duration = 0.3f)
	{
		SVGImage[] array = screws;
		foreach (SVGImage target in array)
		{
			target.DOKill();
			target.DOColor(targ, duration);
		}
	}

	private IEnumerator MoveAdd(int yPos)
	{
		float t = 0f;
		Vector2 tpos = new Vector2(addTrans.anchoredPosition.x, yPos);
		while (t < 1f)
		{
			addTrans.anchoredPosition = Vector2.Lerp(addTrans.anchoredPosition, tpos, 0.3f);
			t += Time.deltaTime * 4f;
			yield return 0;
		}
		addTrans.anchoredPosition = tpos;
	}

	private void HideDanger(GameStates state)
	{
		CrossFadeColor(skull, Color.clear, 0.3f);
	}

	private void ShowDanger(float amount)
	{
		CrossFadeColor(skull, Color.white, 1f);
	}

	public void UpdateDanger()
	{
		if (!isLock)
		{
			if (dataReal < 10)
			{
				ShowDanger((float)(10 - dataReal) / 10f);
			}
			else if (dataReal > 90)
			{
				ShowDanger((float)(dataReal - 90) / 10f);
			}
		}
	}
}
