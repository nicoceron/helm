using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : CardAct
{
	[Range(10f, 300f)]
	public float voPitch = 100f;

	[Range(20f, 20000f)]
	public float voCenterFrequ = 2900f;

	[Range(0f, 3f)]
	public float voFrequGain = 1.5f;

	[Range(0f, 1f)]
	public float voVolume = 1f;

	private bool voForceVO;

	private bool mute;

	public SVGImage blinkIm;

	public bool hasEyes;

	public bool keepEyes;

	public SVGAsset death;

	public RectTransform eyes;

	private Bearer bear;

	public float eyesUpMove = 0.5f;

	public float eyesLatMove = 0.5f;

	public Action OnInit;

	public Action OnHideFond;

	public Action<string> OnChoice;

	public Action<int> OnDecisionMade;

	public SVGImage[] layersImg;

	public SVGImage[] layersImg2;

	private int destroyEffect = -10;

	private Card curCard;

	public SVGImage sprite2;

	public GameObject lockedCard;

	public SVGImage lockedGraphic;

	private SVGAsset blueim;

	public GameObject choiceSign;

	public Text choiceText;

	public GameObject choiceBut;

	private bool isHiddenChoice;

	private int meliId;

	private bool isWinterDone;

	private Selectable selectable;

	private Vector2 priceObjectStartingScale = Vector2.zero;

	private SVGImage frontLayer;

	private SVGImage frontLayer2;

	private SVGImage bodyLayer;

	private BearerGen _model;

	private bool noblink;

	private bool hasmaskdefault;

	private bool yesnodeactivate;

	private bool isShown;

	private Sequence[] computerSequence = new Sequence[5];

	private int price;

	private bool hasEvil;

	private Vector2 epos;

	private SVGAsset curEye;

	public SVGImage eyesIm;

	public SVGImage eyesIm2;

	private bool isSelecting;

	public Card CurrentCard => curCard;

	private void SpecialDecision(int decision)
	{
		string curCardName = GameAct.diff.GetCurCardName();
		if (curCardName != null && curCardName == "ratereigns")
		{
			_ = 1;
		}
	}

	public override void Init(Bearer newbear)
	{
		bear = newbear;
		frontLayer = layersImg[layersImg.Length - 1];
		frontLayer2 = layersImg2[layersImg2.Length - 1];
		frontLayer.enabled = false;
		bodyLayer = layersImg[2];
		selectable = choiceBut.GetComponent<Selectable>();
		if (newbear.type == BearerTypes.generated)
		{
			noblink = true;
			_model = CardReader.diff.bearerGenModels.Find((BearerGen it) => it.bearer == newbear.bearer);
			SetGenerated();
		}
		else
		{
			sprite.vectorGraphics = bear.sprite;
			SVGAsset sVGAsset = (SVGAsset)Resources.Load("masks/" + bear.bearer);
			if (sVGAsset != null)
			{
				frontLayer.vectorGraphics = sVGAsset;
				frontLayer.enabled = true;
				hasmaskdefault = true;
			}
			if (bear.hasEyes)
			{
				eyesIm.vectorGraphics = bear.eyes;
			}
			else
			{
				eyesIm.enabled = false;
			}
			epos = eyes.anchoredPosition;
			SVGAsset sVGAsset2 = (SVGAsset)Resources.Load("blink/" + bear.bearer);
			if (sVGAsset2 != null)
			{
				blinkIm.vectorGraphics = sVGAsset2;
			}
			curEye = eyesIm.vectorGraphics;
		}
		hasEyes = bear.hasEyes;
		_Awake();
		blueim = (SVGAsset)Resources.Load("bearers/" + bear.bearer.ToString() + "-locked");
		voForceVO = false;
	}

	private IEnumerator DelayUnlock(float delay)
	{
		yield return new WaitForSeconds(delay);
		base.transform.SetAsLastSibling();
		if (curCard.bearerVariation != "")
		{
			CustomImage(curCard.bearerVariation);
		}
		else
		{
			DefaultImage();
		}
		lockedGraphic.vectorGraphics = blueim;
		lockedCard.SetActive(value: true);
		lockedCard.GetComponent<Animator>().Play("glass_break");
		if (hasEyes)
		{
			eyes.gameObject.SetActive(value: true);
			StartCoroutine("MoveEyes");
		}
		ActivateButton(first: false);
		yield return new WaitForSeconds(0.6f);
		choiceSign.SetActive(value: true);
	}

	public void UnlockBlue()
	{
		StopCoroutine("DelayUnlock");
		StartCoroutine("DelayUnlock", 0);
	}

	public void UpdateChoiceCard(Card card, bool isHidden, bool showSubtitle, DataDisplay display, bool first = false, float subtitlesize = 1f)
	{
		isShown = true;
		if (card == null || fond == null)
		{
			return;
		}
		curCard = card;
		SetupPrice(card, single: false);
		string text = card.question.Get();
		text = ((!text.Contains("€") && !text.Contains("£")) ? bear.generated.Get() : text.Split('€', '£')[0]);
		if (display != DataDisplay.fullamount && display != DataDisplay.moving)
		{
			CustomImage("locked");
			base.InitCard();
			eyes.gameObject.SetActive(value: false);
			return;
		}
		isHiddenChoice = isHidden;
		ActivateButton(first);
		if (isHidden)
		{
			base.InitCard("", "", "", 0, withanim: false);
			fond.enabled = true;
		}
		else
		{
			if (showSubtitle)
			{
				SetSign(text, subtitlesize);
			}
			base.InitCard();
		}
		if (hasEyes)
		{
			eyes.gameObject.SetActive(value: true);
			StartCoroutine("MoveEyes");
		}
		else if ((bool)eyes)
		{
			eyes.gameObject.SetActive(value: false);
		}
		if (card.bearerVariation != "")
		{
			CustomImage(card.bearerVariation);
		}
		else
		{
			DefaultImage();
		}
		if (bear.bearer == Bearers.merchant || bear.bearer == Bearers.mutant)
		{
			SetGenerated(BackgroundAct.diff.nameBack);
		}
	}

	private void SetSign(string txt, float subtitlesize)
	{
		choiceSign.SetActive(value: true);
		choiceText.rectTransform.localScale = new Vector3(subtitlesize, subtitlesize, 1f);
		choiceText.text = SpeechAct.diff.FinalFormat(txt);
	}

	public void ActivateButton(bool first)
	{
		StopCoroutine("DoActivateButton");
		StartCoroutine("DoActivateButton", first);
	}

	private IEnumerator DoActivateButton(bool first)
	{
		while (GameAct.diff.state != GameStates.interaction)
		{
			yield return 0;
		}
		selectable.enabled = true;
		choiceBut.SetActive(value: true);
		if (first && (!InputAct.diff || InputAct.diff.NavigationMode()))
		{
			choiceBut.GetComponent<AutoSelectMe>().enabled = true;
		}
	}

	private void DeactivateButton()
	{
		StopCoroutine("DoActivateButton");
		choiceBut.SetActive(value: false);
		choiceBut.GetComponent<AutoSelectMe>().enabled = false;
	}

	public void UpdateCharacCard(Card card, int decision, bool withanim = true)
	{
		if (!hasmaskdefault)
		{
			frontLayer.enabled = false;
		}
		if (bear.type == BearerTypes.generated && string.IsNullOrEmpty(card.bearerVariation))
		{
			if (!card.name.StartsWith("_"))
			{
				SetGenerated();
			}
			else if (bear.bearer == Bearers.merchant || bear.bearer == Bearers.mutant)
			{
				SetGenerated(BackgroundAct.diff.nameBack);
			}
		}
		if (price != 0)
		{
			GameAct.diff.SetInt(Variables.price, price);
		}
		isShown = true;
		string question = card.question.Get();
		string text = ((!card.override_yes.isEmpty) ? GameAct.diff.TreatText(card.override_yes) : SpeechAct.diff.GetSceneTextFinal("yes"));
		string text2 = ((!card.override_no.isEmpty) ? GameAct.diff.TreatText(card.override_no) : SpeechAct.diff.GetSceneTextFinal("no"));
		if (!yesnodeactivate && (text == "..." || text2 == "..."))
		{
			fondyesno.gameObject.SetActive(value: false);
			fondyesno2.gameObject.SetActive(value: false);
			yesnodeactivate = true;
		}
		else if (yesnodeactivate && text != "..." && text2 != "...")
		{
			fondyesno.gameObject.SetActive(value: true);
			fondyesno2.gameObject.SetActive(value: true);
			yesnodeactivate = false;
		}
		if (OnInit != null)
		{
			OnInit();
		}
		destroyEffect = -10;
		Outcome outcome = card.yes_outcomes.Find((Outcome it) => it.variable == Variables.destroy && (it.bearer == Bearers.anyone || it.bearer == card.bearer));
		Outcome outcome2 = card.no_outcomes.Find((Outcome it) => it.variable == Variables.destroy && (it.bearer == Bearers.anyone || it.bearer == card.bearer));
		if (outcome != null && outcome2 != null)
		{
			destroyEffect = 2;
			PrepareDeath();
		}
		else if (outcome != null)
		{
			destroyEffect = 1;
			PrepareDeath();
		}
		else if (outcome2 != null)
		{
			destroyEffect = -1;
			PrepareDeath();
		}
		else if (hasEyes)
		{
			eyes.gameObject.SetActive(value: true);
			StartCoroutine("MoveEyes");
			eyes.localScale = new Vector3(1f, 1f);
		}
		else if ((bool)eyes)
		{
			eyes.gameObject.SetActive(value: false);
		}
		bodyLayer.rectTransform.localScale = new Vector3(1f, 1f);
		base.InitCard(text, text2, "", decision, withanim);
		if (card.bearer == Bearers.computer)
		{
			ComputerMove();
		}
		if (bear.bearer == Bearers.phone)
		{
			JukeBox.diff.TransitionToSnapshot("Down", 0.2f);
			try
			{
				Bearers overridebear = (Bearers)Enum.Parse(typeof(Bearers), card.bearerVariation);
				Speak(question, overridebear);
			}
			catch
			{
				Speak(question);
			}
		}
		else
		{
			Speak(question);
		}
		if (withanim && bear.type != BearerTypes.generated)
		{
			if (card.bearerVariation != "")
			{
				CustomImage(card.bearerVariation);
			}
			else if (card.bearer == Bearers.spaceship)
			{
				CustomImage(GameAct.diff.GetInt("enemy").ToString());
			}
			else
			{
				DefaultImage();
			}
		}
	}

	private void Speak(string question, Bearers overridebear = Bearers.none)
	{
		if (!mute && (voForceVO || question.Length <= 3 || !(question.Substring(0, 2) == "((") || !(question.Substring(question.Length - 2, 2) == "))")))
		{
			Bearers bearers = ((overridebear != Bearers.none) ? overridebear : bear.bearer);
			if (bearers == Bearers.merchant)
			{
				bearers = Bearers.yoyu;
			}
			if (bearers == Bearers.mutant)
			{
				bearers = Bearers.yoyu;
			}
			JukeBox.diff.Speak(question, bearers, voPitch * 0.01f, voCenterFrequ, voFrequGain, voVolume);
		}
	}

	public override void UpdateCard(string yesText, string noText, string question = "")
	{
		isShown = true;
		if (!string.IsNullOrEmpty(question))
		{
			Speak(question);
		}
		base.UpdateCard(yesText, noText, question);
	}

	private void NoGravity()
	{
	}

	private void Gravity()
	{
	}

	public string SelfGenerate()
	{
		int num = GameAct.diff.GetInt("nb_human");
		int num2 = GameAct.diff.GetInt("nb_animal");
		int num3 = GameAct.diff.GetInt("nb_alien");
		bool flag = GameAct.diff.GetBool("blue");
		bool flag2 = GameAct.diff.GetBool("excited");
		bool flag3 = num > num2 && num > num3;
		bool flag4 = num3 > num2 && num3 > num;
		bool flag5 = num2 > num && num2 > num3;
		if (!flag3 && !flag4 && !flag4)
		{
			float num4 = Util.Rand();
			if (num4 < 0.33f)
			{
				flag3 = true;
			}
			else if (num4 < 0.66f)
			{
				flag5 = true;
			}
			else
			{
				flag4 = true;
			}
		}
		GameAct.diff.SetBool("snout", boo: false);
		if (flag4)
		{
			if (flag)
			{
				if (flag2)
				{
					return "squid";
				}
				return "blueye";
			}
			if (num > num2)
			{
				return "redeye";
			}
			return "insect";
		}
		if (flag5)
		{
			if (num3 <= num)
			{
				GameAct.diff.SetBool("snout", boo: true);
				return "pig";
			}
			if (flag2)
			{
				return "bear";
			}
			return "raccoon";
		}
		if (flag3)
		{
			if (num3 > num2)
			{
				if (flag)
				{
					return "black";
				}
				return "hair";
			}
			if (flag2)
			{
				return "woman";
			}
			return "white";
		}
		return "beard";
	}

	private void ComputerMove()
	{
		GameAct.diff.GetInt("danger");
		bool isInDanger = CameffectAct.diff.isInDanger;
		if (!isShown)
		{
			return;
		}
		Util.RandInt(0, computerSequence.Length);
		bool flag = (isInDanger ? (Util.Rand() > 0.3f) : (Util.Rand() > 0.15f));
		Vector2 vector = (flag ? Vector2.zero : ((Vector2)(Quaternion.AngleAxis(Util.Rand(-180f, 180f), Vector3.forward) * Vector2.up)));
		float num = (isInDanger ? Util.Rand(15f, 28f) : Util.Rand(10f, 25f));
		float interval = (isInDanger ? Util.Rand(0f, 0.3f) : (Util.Rand() * 2f));
		float duration = (isInDanger ? Util.Rand(0.2f, 0.8f) : Util.Rand(0.3f, 2f));
		Ease ease = (isInDanger ? Ease.OutBounce : Ease.InOutBack);
		for (int i = 0; i < computerSequence.Length; i++)
		{
			Sequence sequence = computerSequence[i];
			SVGImage obj = layersImg[i + 1];
			obj.enabled = true;
			sequence?.Kill();
			float f = i;
			RectTransform rectTransform = obj.rectTransform;
			sequence = DOTween.Sequence().SetId(666);
			sequence.AppendInterval(interval);
			if (i < 4)
			{
				sequence.Append(rectTransform.DOAnchorPos(vector * num * (Mathf.Pow(f, 1.2f) * 0.1f), duration).SetEase(Ease.OutBack));
			}
			else
			{
				sequence.Append(rectTransform.DOAnchorPos(vector * num * (Mathf.Pow(2.5f, 1.2f) * 0.1f), duration).SetEase(Ease.OutBack));
			}
			int num2 = Util.RandInt(0, 4);
			for (int j = 0; j < num2; j++)
			{
				if (i < 2)
				{
					float interval2 = (isInDanger ? Util.Rand(0.3f, 0.8f) : Util.Rand(0.8f, 2f));
					float num3 = (isInDanger ? Util.Rand(0.3f, 0.9f) : Util.Rand(0.5f, 2f));
					sequence.AppendInterval(interval2).AppendCallback(delegate
					{
						JukeBox.diff.PlaySound(SFXTypes.sfx_computer_move_ring1);
					}).Append(rectTransform.DOLocalRotate(new Vector3(0f, 0f, Util.RandInt(-10, 10) * 30), num3 * (1f - (float)i * 0.4f)).SetEase(ease));
				}
				else if (flag)
				{
					float interval3 = (isInDanger ? Util.Rand(0.4f, 0.9f) : Util.Rand(0.9f, 2f));
					float duration2 = (isInDanger ? Util.Rand(0.3f, 0.9f) : Util.Rand(0.3f, 8f));
					float num4 = Util.Rand(0.6f);
					sequence.AppendInterval(interval3).AppendCallback(delegate
					{
						JukeBox.diff.PlaySound(SFXTypes.sfx_computer_move_ring2);
					}).Append(rectTransform.DOScale(new Vector3(num4, num4), duration2));
				}
				if (i == 0 && isInDanger)
				{
					sequence.AppendInterval(0.7f);
				}
				if (i == 0 && !isInDanger)
				{
					sequence.AppendInterval(1.3f);
				}
			}
			if (i == 0)
			{
				sequence.AppendCallback(ComputerMove);
			}
			computerSequence[i] = sequence;
			computerSequence[i].Play();
		}
	}

	public void SetupPrice(Card card, bool single)
	{
		if (card == null || card.yes_outcomes.Count == 0)
		{
			return;
		}
		Outcome outcome = card.yes_outcomes.Find((Outcome it) => it.variable == Variables.money);
		if (outcome == null)
		{
			return;
		}
		price = outcome.value;
		bool flag = price < 0;
		if (BackgroundAct.diff.GetNextName().Equals("Gultron"))
		{
			price = -price;
		}
		Card card2 = GameAct.diff.GetHiddenCards().Find((Card it) => (it.name.Equals("_buy9_choice") || it.name.Equals("_black9_choice")) && it.bearerVariation == card.bearerVariation);
		if (card2 == null)
		{
			if (single)
			{
				GameAct.diff.SetInt(Variables.price, price);
			}
			if (card.bearer == Bearers.merchandise)
			{
				SetPrice(price, single);
			}
			return;
		}
		List<Condition> conditions = card2.conditions;
		float num = 1f;
		foreach (Condition item in conditions)
		{
			if (!string.IsNullOrEmpty(item.custom_name) && item.condition == Conditions.above && item.condition == Conditions.below && !item.custom_name.StartsWith("nb_"))
			{
				float num2 = GameAct.diff.GetInt(item.custom_name);
				float num3 = ((item.condition == Conditions.above) ? (5f - num2) : (num2 - 5f));
				if (num3 > 0f)
				{
					num3 = Mathf.Pow(num3, 1.4f);
				}
				num = (flag ? (num - num3 * 0.2f) : (num + num3 * 0.4f));
			}
		}
		num = Mathf.Clamp(num + Util.GetFloat(card.bearerVariation + BackgroundAct.diff.GetNextName(), -0.1f, 0.1f), 0.3f, 2.9f);
		float f = num * (float)price;
		price = Mathf.RoundToInt(f);
		if (single)
		{
			GameAct.diff.SetInt(Variables.price, Mathf.RoundToInt(f));
		}
		if (card.bearer == Bearers.merchandise)
		{
			SetPrice(price, single);
		}
	}

	public override void CustomImage(string source, string folder = "bearers")
	{
		DefaultImage();
		if (bear.type == BearerTypes.generated)
		{
			SetGenerated(source);
			return;
		}
		if (source == "self")
		{
			source = SelfGenerate();
		}
		if (hasEyes && (!keepEyes || source.Contains("noeyes")))
		{
			StopEyes();
			eyes.gameObject.SetActive(value: false);
		}
		else if (hasEyes)
		{
			SVGAsset sVGAsset = (SVGAsset)Resources.Load("eyes/" + defaultImage.name + "-" + source);
			if (sVGAsset != null)
			{
				eyesIm.vectorGraphics = sVGAsset;
			}
			SVGAsset sVGAsset2 = (SVGAsset)Resources.Load("blink/" + defaultImage.name + "-" + source);
			if (sVGAsset2 != null)
			{
				blinkIm.vectorGraphics = sVGAsset2;
			}
		}
		frontLayer.enabled = !source.Equals("concert");
		if (source == "space")
		{
			SVGAsset sVGAsset3 = (SVGAsset)Resources.Load("masks/" + defaultImage.name + "-" + source);
			if (sVGAsset3 != null)
			{
				frontLayer.vectorGraphics = sVGAsset3;
				frontLayer.enabled = true;
			}
		}
		base.CustomImage(source, folder);
	}

	public override void DefaultImage()
	{
		if (hasEyes && hasCustomImage && !keepEyes)
		{
			eyesIm.vectorGraphics = bear.eyes;
			eyes.gameObject.SetActive(value: true);
			StartCoroutine("MoveEyes");
		}
		base.DefaultImage();
	}

	private void StopEyes()
	{
		StopCoroutine("MoveEyes");
		StopCoroutine("Blink");
	}

	public override void HideCard()
	{
		base.transform.DOKill();
		price = 0;
		isShown = false;
		if (OnChoice != null)
		{
			OnChoice(sprite.vectorGraphics.name);
		}
		if (OnDecisionMade != null)
		{
			OnDecisionMade(decision);
		}
		SpecialDecision(decision);
		DisableDeath();
		StopEyes();
		base.HideCard();
	}

	public override void HideFond()
	{
		if (OnHideFond != null)
		{
			OnHideFond();
		}
		base.HideFond();
	}

	public override void ShowDecision(int dec)
	{
		if (!destroy2Sides && (destroyEffect == dec || destroyEffect == 2))
		{
			if (hasEyes)
			{
				StopCoroutine("MoveEyes");
			}
			SwitchDeath(showdeath: true);
		}
		else if (destroy2Sides && dec != 0 && destroyEffect != 2 && dec != destroyEffect)
		{
			_ = hasEyes;
			SwitchDeath(showdeath: false);
		}
		base.ShowDecision(dec);
	}

	private IEnumerator MoveEyes()
	{
		if (!noblink)
		{
			StartCoroutine("Blink");
		}
		eyes.gameObject.SetActive(value: true);
		eyesIm.enabled = true;
		blinkIm.enabled = false;
		eyes.anchoredPosition = Vector2.zero + epos;
		yield return new WaitForSeconds(0.4f);
		eyes.anchoredPosition = Vector2.zero + epos;
		while (true)
		{
			float t = 0f;
			float trust = Mathf.Sign(bear.vote);
			Vector2 fpos = FaceAct.diff.GetFacePos(trust) * 40f;
			Vector2 wpos = mytrans.transform.position;
			while (t < 1f)
			{
				Vector2 b = new Vector2(Mathf.Clamp(fpos.x - wpos.x * 0.25f + epos.x, -6f, 6f), Mathf.Clamp(fpos.y - (wpos.y + 10f) * 0.25f + epos.y, -6f, 6f));
				eyes.anchoredPosition = Vector2.Lerp(eyes.anchoredPosition, b, t);
				yield return 0;
				t += Time.deltaTime * 3f;
				yield return 0;
			}
			yield return new WaitForSeconds(Util.Rand(0.5f));
		}
	}

	private IEnumerator Blink()
	{
		while (!hasEvil)
		{
			float num = ((bear.vote < 0f) ? 2 : 12);
			yield return new WaitForSeconds(num + Util.Rand(0.4f, 0.9f));
			if (blinkIm.vectorGraphics == null)
			{
				break;
			}
			eyesIm.enabled = false;
			blinkIm.enabled = true;
			yield return new WaitForSeconds(Util.Rand(0.3f, 0.5f));
			eyesIm.enabled = true;
			blinkIm.enabled = false;
		}
	}

	public void SetGenerated(string seed = "")
	{
		if (string.IsNullOrEmpty(seed))
		{
			seed = Util.Rand().ToString();
		}
		if (_model == null)
		{
			return;
		}
		BearerGen model = _model;
		bear.ResetName(seed);
		int num = Util.GetInt(seed, 1, 50000);
		bool flag = false;
		for (int i = 0; i < layersImg.Length; i++)
		{
			SVGImage sVGImage = layersImg[i];
			List<SVGAsset> layer = model.GetLayer(i);
			if (layer == null)
			{
				sVGImage.enabled = false;
				continue;
			}
			sVGImage.enabled = true;
			if (i == model.eyeLayer)
			{
				sVGImage.vectorGraphics = layer[(num + (i - 1) * 156) % layer.Count];
				eyesIm = sVGImage;
				curEye = sVGImage.vectorGraphics;
				eyes = eyesIm.rectTransform;
				eyesIm.vectorGraphics = sVGImage.vectorGraphics;
				flag = true;
			}
			else
			{
				sVGImage.vectorGraphics = layer[(num + i * 156) % layer.Count];
			}
			if (i == 0)
			{
				sprite.vectorGraphics = sVGImage.vectorGraphics;
			}
		}
		if (!flag)
		{
			eyes = null;
			eyesIm = null;
		}
	}

	public void PrepareDeath()
	{
		sprite2.vectorGraphics = sprite.vectorGraphics;
		if (bodyLayer.enabled)
		{
			for (int i = 0; i < layersImg2.Length; i++)
			{
				SVGImage obj = layersImg2[i];
				SVGImage sVGImage = layersImg[i];
				obj.enabled = sVGImage.enabled;
				obj.vectorGraphics = sVGImage.vectorGraphics;
			}
		}
		if (frontLayer.enabled)
		{
			frontLayer2.enabled = true;
			frontLayer2.vectorGraphics = frontLayer2.vectorGraphics;
		}
		sprite2.vectorGraphics = sprite.vectorGraphics;
	}

	private void SwitchDeath(bool showdeath)
	{
		if (showdeath)
		{
			mytrans.anchoredPosition = new Vector2(-70f, 90f);
			mytrans.sizeDelta = new Vector2(140f, 380f);
			sprite.rectTransform.anchoredPosition = new Vector2(70f, 0f);
			destroy2Sides = true;
			mytrans2.anchoredPosition = new Vector2(70f, 90f);
			mytrans2.rotation = Quaternion.identity;
			mytrans2.gameObject.SetActive(value: true);
		}
		else
		{
			mytrans.anchoredPosition = new Vector2(0f, 90f);
			mytrans.sizeDelta = new Vector2(280f, 380f);
			sprite.rectTransform.anchoredPosition = new Vector2(0f, 0f);
			destroy2Sides = false;
			mytrans2.gameObject.SetActive(value: false);
		}
	}

	private void DisableDeath()
	{
		SVGImage[] array = layersImg2;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		mytrans2.gameObject.SetActive(value: false);
		destroy2Sides = false;
	}

	public void SelectChoice()
	{
		if (!isSelecting)
		{
			isSelecting = true;
			StopCoroutine("LargeCard");
			StopCoroutine("NormalCard");
			StartCoroutine("LargeCard");
		}
	}

	public void UnSelectChoice()
	{
		isSelecting = false;
		StopCoroutine("LargeCard");
		StopCoroutine("NormalCard");
		StartCoroutine("NormalCard");
	}

	public override void Unset()
	{
		if (bear.bearer == Bearers.phone)
		{
			JukeBox.diff.DefaultSnapshot(0.3f);
		}
		if (bear.bearer == Bearers.computer)
		{
			DOTween.Complete(666);
		}
		base.Unset();
	}

	private IEnumerator LargeCard()
	{
		RectTransform trans = mytrans;
		Vector2 osize = trans.localScale;
		Vector2 tsize = new Vector2(1.1f, 1.1f);
		float t = 0f;
		while (t < 1f)
		{
			trans.localScale = Vector2.LerpUnclamped(osize, tsize, Easing.BackEaseIn(t, 0f, 1f, 1f));
			if (isSelecting && t > 0.6f)
			{
				isSelecting = false;
			}
			t += Time.deltaTime * 3f;
			yield return 0;
		}
		trans.localScale = tsize;
	}

	private IEnumerator NormalCard()
	{
		RectTransform trans = mytrans;
		Vector2 osize = trans.localScale;
		Vector2 tsize = new Vector2(1f, 1f);
		float t = 0f;
		while (t < 1f)
		{
			trans.localScale = Vector2.Lerp(osize, tsize, t);
			t += Time.deltaTime * 5f;
			yield return 0;
		}
		trans.localScale = tsize;
	}

	public void ValidChoice()
	{
		if (!InputAct.diff.isInMenu)
		{
			if (isSelecting)
			{
				StopCoroutine("YieldValid");
				StartCoroutine("YieldValid");
			}
			else
			{
				DoValid();
			}
		}
	}

	private IEnumerator YieldValid()
	{
		while (isSelecting)
		{
			yield return 0;
		}
		DoValid();
	}

	private void DoValid()
	{
		GameAct.diff.ValidSelection(this, bear, curCard);
		UpdateCharacCard(curCard, 0, isHiddenChoice);
		DisableChoice(andremove: false);
		StopCoroutine("LargeCard");
		StopCoroutine("NormalCard");
		StartCoroutine("NormalCard");
	}

	public void DisableChoice(bool andremove = true)
	{
		if (!(choiceBut == null))
		{
			DeactivateButton();
			choiceSign.SetActive(value: false);
			isHiddenChoice = false;
			if (andremove && !GameAct.diff.bearers.Contains(bear))
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
