using System;
using System.Collections;
using System.Collections.Generic;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class CardAct : MonoBehaviour
{
	public RectTransform mytrans;

	public RectTransform mytrans2;

	protected RectTransform thistrans;

	public Animator anima;

	public SVGImage sprite;

	public SVGImage fond;

	public Text yesSign;

	public Text noSign;

	public Text yesSign2;

	public Text noSign2;

	public RectTransform fondyesno;

	public RectTransform fondyesno2;

	public int decision;

	protected Color oriCol;

	protected Color clCol;

	protected AudioSource audioCard;

	protected bool hasCustomImage;

	protected SVGAsset defaultImage;

	public GameObject priceObj;

	protected Vector3 tpos = new Vector3(0f, -85f, 0f);

	protected Vector3 opos = new Vector3(0f, 40f, 0f);

	protected float timeSinceGrab;

	protected GameObject cardGraphics;

	public SVGAsset[] cardAnimation;

	private int cardAnimSet;

	public bool destroy2Sides;

	protected RectTransform lockedRect;

	private Coroutine animsprite;

	private bool shuffle;

	private Vector2 half = new Vector2(70f, 0f);

	protected void _Awake()
	{
		defaultImage = sprite.vectorGraphics;
		cardGraphics = sprite.gameObject;
		if ((bool)yesSign)
		{
			oriCol = (clCol = yesSign.color);
		}
		clCol.a = 0f;
		thistrans = GetComponent<RectTransform>();
		thistrans.localScale = new Vector3(1f, 1f, 1f);
		GoToPos(new Vector2(0f, -1000f));
		cardGraphics.SetActive(value: false);
	}

	public void SetPrice(bool small = true)
	{
		priceObj.GetComponent<RectTransform>().localScale = (small ? Vector3.one : new Vector3(1.3f, 1.3f, 1f));
	}

	public void SetPrice(int amount, bool small = true)
	{
		priceObj.SetActive(value: true);
		priceObj.GetComponentInChildren<Text>().text = SpeechAct.diff.GetSmartText("money", 0, Mathf.Abs(amount));
		SetPrice(small);
	}

	public virtual bool OverrideInteraction()
	{
		return false;
	}

	public virtual void Init(Bearer bear)
	{
	}

	public virtual void ReactToItem(Transform item)
	{
	}

	public virtual void StopReacting()
	{
	}

	public virtual void InitCard(string yesText = "", string noText = "", string otherText = "", int decision = 0, bool withanim = true)
	{
		cardGraphics.SetActive(value: true);
		UpdateCard(yesText, noText);
		timeSinceGrab = Time.realtimeSinceStartup;
		if (withanim)
		{
			fond.enabled = true;
			if (decision == -1)
			{
				anima.Play("turnright");
			}
			else
			{
				anima.Play("turnleft");
			}
		}
	}

	public virtual void UpdateCard(string yesText, string noText, string question = "")
	{
		if ((bool)fondyesno)
		{
			StopCorout();
			RectTransform rectTransform = fondyesno;
			Vector2 anchoredPosition = (fondyesno2.anchoredPosition = new Vector3(0f, 0f, 0f));
			rectTransform.anchoredPosition = anchoredPosition;
			RectTransform rectTransform2 = fondyesno;
			Quaternion rotation = (fondyesno2.rotation = Quaternion.Euler(0f, 0f, 0f));
			rectTransform2.rotation = rotation;
			Text text = yesSign;
			string text2 = (yesSign2.text = yesText);
			text.text = text2;
			Text text4 = noSign;
			text2 = (noSign2.text = noText);
			text4.text = text2;
			Text text6 = yesSign;
			Text text7 = noSign;
			Text text8 = yesSign2;
			Color color = (noSign2.color = clCol);
			Color color3 = (text8.color = color);
			Color color5 = (text7.color = color3);
			text6.color = color5;
		}
	}

	public virtual void CustomImage(string source, string folder = "bearers")
	{
		if (!base.gameObject.activeInHierarchy || string.IsNullOrEmpty(source))
		{
			return;
		}
		cardAnimSet = 0;
		if (source.Contains("&"))
		{
			shuffle = false;
			string[] array = source.Split(new char[1] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			source = array[0];
			shuffle = source.Equals("mirror");
			GetAnimation(source, folder);
			switch (array[1])
			{
			case "yes":
			{
				cardAnimSet = 1;
				SVGAsset vectorGraphics3 = cardAnimation[0];
				sprite.vectorGraphics = vectorGraphics3;
				return;
			}
			case "both":
				shuffle = true;
				cardAnimSet = 2;
				AnimSprite();
				return;
			case "swipe":
			{
				cardAnimSet = 3;
				SVGAsset vectorGraphics2 = cardAnimation[0];
				sprite.vectorGraphics = vectorGraphics2;
				return;
			}
			case "no":
			{
				cardAnimSet = -1;
				SVGAsset vectorGraphics = cardAnimation[0];
				sprite.vectorGraphics = vectorGraphics;
				return;
			}
			case "delay":
				AnimSprite(reverse: false, 1f);
				break;
			}
		}
		else
		{
			GetAnimation(source, folder);
		}
		AnimSprite(reverse: true);
		hasCustomImage = true;
	}

	private void AnimSprite(bool reverse = false, float delay = 0f)
	{
		if (animsprite != null)
		{
			StopCoroutine(animsprite);
		}
		animsprite = StartCoroutine(DoAnimSprite(cardAnimation, delay, reverse));
	}

	private IEnumerator DoAnimSprite(SVGAsset[] sprites, float delay, bool reverse)
	{
		yield return new WaitForSeconds(delay);
		if (reverse)
		{
			for (int i = sprites.Length - 1; i > -1; i--)
			{
				SVGAsset sVGAsset = sprites[i];
				if (sprite.vectorGraphics != sVGAsset)
				{
					sprite.vectorGraphics = sVGAsset;
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
		else
		{
			foreach (SVGAsset sVGAsset2 in sprites)
			{
				if (sprite.vectorGraphics != sVGAsset2)
				{
					sprite.vectorGraphics = sVGAsset2;
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
		if (shuffle)
		{
			sprites.Shuffle();
		}
	}

	private void GetAnimation(string source, string folder)
	{
		List<SVGAsset> list = new List<SVGAsset>();
		string text = ((!(defaultImage == null)) ? (folder + "/" + defaultImage.name + "-") : (folder + "/"));
		SVGAsset sVGAsset = (SVGAsset)Resources.Load(text + source, typeof(SVGAsset));
		int num = 1;
		while (sVGAsset != null)
		{
			list.Add(sVGAsset);
			sVGAsset = (SVGAsset)Resources.Load(text + source + num, typeof(SVGAsset));
			num++;
		}
		cardAnimation = list.ToArray();
	}

	public virtual void DefaultImage()
	{
		sprite.vectorGraphics = defaultImage;
		hasCustomImage = false;
	}

	public virtual void HideCard()
	{
		if ((bool)priceObj)
		{
			priceObj.SetActive(value: false);
		}
	}

	public virtual void HideFond()
	{
		fond.enabled = false;
	}

	private void StopCorout()
	{
		StopCoroutine("MoveFond");
		StopCoroutine("ChangeYes");
		StopCoroutine("ChangeNo");
		StopCoroutine("RotateFond");
	}

	public virtual void ShowDecision(int dec)
	{
		int num = decision;
		if (decision == dec)
		{
			return;
		}
		decision = dec;
		StopCorout();
		if (!fondyesno)
		{
			return;
		}
		switch (decision)
		{
		case -1:
			StartCoroutine("MoveFond", tpos);
			StartCoroutine("ChangeYes", clCol);
			StartCoroutine("ChangeNo", oriCol);
			timeSinceGrab = Time.realtimeSinceStartup;
			if (cardAnimSet == -1 || cardAnimSet == 2)
			{
				AnimSprite();
			}
			if ((cardAnimSet == 1 || cardAnimSet == 2) && num == 1)
			{
				AnimSprite(reverse: true);
			}
			if (cardAnimSet == 3)
			{
				SVGAsset sVGAsset2 = cardAnimation[2];
				if (sprite.vectorGraphics != sVGAsset2)
				{
					sprite.vectorGraphics = sVGAsset2;
				}
			}
			break;
		case 0:
			StartCoroutine("MoveFond", opos);
			StartCoroutine("ChangeYes", clCol);
			StartCoroutine("ChangeNo", clCol);
			if ((cardAnimSet == 1 || cardAnimSet == 2) && num == 1)
			{
				AnimSprite(reverse: true);
			}
			if ((cardAnimSet == -1 || cardAnimSet == 2) && num == -1)
			{
				AnimSprite(reverse: true);
			}
			if (cardAnimSet == 3 && num != 0)
			{
				SVGAsset sVGAsset3 = cardAnimation[0];
				if (sprite.vectorGraphics != sVGAsset3)
				{
					sprite.vectorGraphics = sVGAsset3;
				}
			}
			break;
		case 1:
			StartCoroutine("MoveFond", tpos);
			StartCoroutine("ChangeYes", oriCol);
			StartCoroutine("ChangeNo", clCol);
			timeSinceGrab = Time.realtimeSinceStartup;
			if (cardAnimSet == 1 || cardAnimSet == 2)
			{
				AnimSprite();
			}
			if ((cardAnimSet == -1 || cardAnimSet == 2) && num == -1)
			{
				AnimSprite(reverse: true);
			}
			if (cardAnimSet == 3)
			{
				SVGAsset sVGAsset = cardAnimation[1];
				if (sprite.vectorGraphics != sVGAsset)
				{
					sprite.vectorGraphics = sVGAsset;
				}
			}
			break;
		}
	}

	private IEnumerator MoveFond(Vector3 targ)
	{
		float t = 0f;
		while (t < 0.5f)
		{
			fondyesno.anchoredPosition = Vector3.Lerp(fondyesno.anchoredPosition, targ, Time.deltaTime * 10f);
			if (destroy2Sides)
			{
				fondyesno2.anchoredPosition = Vector3.Lerp(fondyesno2.anchoredPosition, targ, Time.deltaTime * 10f);
			}
			t += Time.deltaTime;
			yield return null;
		}
		fondyesno.anchoredPosition = targ;
		if (destroy2Sides)
		{
			fondyesno2.anchoredPosition = targ;
		}
	}

	private IEnumerator RotateFond(float dir)
	{
		float t = 0f;
		while (t < 0.5f)
		{
			fondyesno.rotation = Quaternion.Slerp(fondyesno.rotation, Quaternion.Euler(0f, 0f, dir), Time.deltaTime * 20f);
			if (destroy2Sides)
			{
				fondyesno2.rotation = Quaternion.Slerp(fondyesno2.rotation, Quaternion.Euler(0f, 0f, dir), Time.deltaTime * 20f);
			}
			t += Time.deltaTime;
			yield return null;
		}
		fondyesno.rotation = Quaternion.Euler(0f, 0f, dir);
		if (destroy2Sides)
		{
			fondyesno2.rotation = Quaternion.Euler(0f, 0f, dir);
		}
	}

	private IEnumerator ChangeYes(Color targ)
	{
		float t = 0f;
		while (t < 0.5f)
		{
			Color color = Color.Lerp(yesSign.color, targ, Time.deltaTime * 20f);
			yesSign.color = color;
			if (destroy2Sides)
			{
				yesSign2.color = color;
			}
			t += Time.deltaTime;
			yield return null;
		}
		yesSign.color = targ;
		if (destroy2Sides)
		{
			yesSign2.color = targ;
		}
	}

	private IEnumerator ChangeNo(Color targ)
	{
		float t = 0f;
		while (t < 0.5f)
		{
			Color color = Color.Lerp(noSign.color, targ, Time.deltaTime * 20f);
			noSign.color = color;
			if (destroy2Sides)
			{
				noSign2.color = color;
			}
			t += Time.deltaTime;
			yield return null;
		}
		noSign.color = targ;
		if (destroy2Sides)
		{
			noSign2.color = targ;
		}
	}

	public void GoToPos(Vector2 target)
	{
		if (!(mytrans == null))
		{
			if (destroy2Sides)
			{
				mytrans.anchoredPosition = target - half;
				mytrans2.anchoredPosition = target + half;
			}
			else
			{
				mytrans.anchoredPosition = target;
			}
			if (lockedRect != null)
			{
				lockedRect.anchoredPosition = target;
			}
		}
	}

	public virtual void LerpToPos(Vector2 target, float amount)
	{
		if (!(mytrans == null))
		{
			if (destroy2Sides)
			{
				float num = ((target.x < 0f) ? 1.2f : 1f);
				float num2 = ((target.x < 0f) ? 1f : 1.2f);
				mytrans.anchoredPosition = Vector2.Lerp(mytrans.anchoredPosition, target * num - half, amount);
				mytrans2.anchoredPosition = Vector2.Lerp(mytrans2.anchoredPosition, target * num2 + half, amount);
			}
			else
			{
				mytrans.anchoredPosition = Vector2.Lerp(mytrans.anchoredPosition, target, amount);
			}
		}
	}

	public virtual void SlerpToPos(float xp, float yp)
	{
		if (!(mytrans == null))
		{
			if ((bool)fondyesno)
			{
				fondyesno.rotation = Quaternion.Euler(0f, 0f, 0f);
			}
			if (destroy2Sides)
			{
				float num = ((xp < 0f) ? (xp * 0.0012f) : (xp * 0.003f));
				float num2 = ((xp < 0f) ? (xp * 0.003f) : (xp * 0.0012f));
				mytrans.rotation = Quaternion.Slerp(mytrans.rotation, Quaternion.AngleAxis((0f - yp) * num2, Vector3.forward), Time.deltaTime * 12f);
				mytrans2.rotation = Quaternion.Slerp(mytrans2.rotation, Quaternion.AngleAxis((0f - yp) * num, Vector3.forward), Time.deltaTime * 12f);
				fondyesno2.rotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				mytrans.rotation = Quaternion.Slerp(mytrans.rotation, Quaternion.AngleAxis((0f - yp) * xp * 0.0012f, Vector3.forward), Time.deltaTime * 12f);
			}
		}
	}

	public virtual void Disappear(Vector2 vec, bool nodecision)
	{
		if (!(mytrans == null))
		{
			float num = (nodecision ? 0.3f : (Mathf.Clamp(Time.realtimeSinceStartup - timeSinceGrab - 0.3f, 0.2f, 4f) * 10f));
			vec.y -= (nodecision ? (num * 10f * Time.deltaTime) : (num * Mathf.Pow(mytrans.anchoredPosition.x, 2f) * Time.deltaTime * 0.01f));
			vec *= 3f / num;
			AddToPos(vec);
			float num2 = vec.x * 10f / (10f + num);
			mytrans.Rotate(new Vector3(0f, 0f, (0f - num2) * 0.4f));
			if (destroy2Sides)
			{
				mytrans2.Rotate(new Vector3(0f, 0f, (0f - num2) * 0.4f));
			}
		}
	}

	private void AddToPos(Vector2 amount)
	{
		if (!(mytrans == null))
		{
			mytrans.anchoredPosition += amount;
			if (destroy2Sides)
			{
				mytrans2.anchoredPosition += amount;
			}
		}
	}

	public virtual void RotateTo(float ang)
	{
		if (!(mytrans == null))
		{
			mytrans.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
			if (destroy2Sides)
			{
				mytrans2.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
			}
		}
	}

	public virtual void Unset()
	{
		if (!(cardGraphics == null))
		{
			cardGraphics.SetActive(value: false);
		}
	}
}
