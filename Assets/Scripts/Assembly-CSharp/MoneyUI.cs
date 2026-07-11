using System;
using System.Collections;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class MoneyUI : MonoBehaviour
{
	public RectTransform pad;

	public static MoneyUI diff;

	public Text place;

	public Text money;

	public Text followers;

	public Text numfollowers;

	public SVGImage placeimg;

	private int moneyAmo;

	private float hiddenPos = 320f;

	private float shownPos = 172f;

	private Tweener movePad;

	private Tweener moveBox;

	private Sequence moveTemp;

	private RectTransform trans;

	private int nb_followers;

	private int defaultposition = 50;

	private bool isShown;

	public void SetSpace()
	{
		defaultposition = 0;
	}

	public void SetDefaultPosition(string name)
	{
		switch (name)
		{
		default:
			return;
		case "concert":
			defaultposition = -50;
			break;
		case "shop":
			defaultposition = 0;
			break;
		case "shipyard":
			defaultposition = 50;
			break;
		}
		movePad.Kill();
		movePad = pad.DOAnchorPosY(defaultposition, 0.4f).SetEase(Ease.OutSine);
	}

	private void Awake()
	{
		diff = this;
		trans = GetComponent<RectTransform>();
	}

	private void Start()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnDataChange = (Action<Variables, int>)Delegate.Combine(gameAct.OnDataChange, new Action<Variables, int>(CheckMoney));
		StopCoroutine("FollowerChange");
		StartCoroutine("FollowerChange");
	}

	private IEnumerator FollowerChange()
	{
		yield return new WaitForSeconds(5f);
		nb_followers = GameAct.diff.GetInt(Variables.nb_fame);
		while (true)
		{
			float seconds = Mathf.Clamp(Util.Rand(0.5f, 1.5f) * 1000f / (float)(1 + nb_followers) + Util.Rand(-10f, 10f), 0.5f, 1000f);
			GameAct.diff.AddInt(Variables.nb_fame);
			yield return new WaitForSeconds(seconds);
		}
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	private void CheckFame(int val)
	{
		if (val >= 100)
		{
			UpdateFame(nb_followers, val);
		}
	}

	private void CheckMoney(Variables var, int val)
	{
		if (isShown && (var == Variables.oxygen || var == Variables.people || var == Variables.power || var == Variables.hull))
		{
			moveTemp.Kill();
			moveTemp = DOTween.Sequence();
			moveTemp.Append(trans.DOAnchorPosY(hiddenPos, 0.4f).SetEase(Ease.InBack)).AppendInterval(1.2f).Append(trans.DOAnchorPosY(shownPos, 0.4f).SetEase(Ease.OutBack));
		}
		if (var == Variables.nb_fame)
		{
			CheckFame(val);
		}
		if (var == Variables.money && GameAct.diff.state != GameStates.none)
		{
			UpdateMoney(moneyAmo, val);
			moneyAmo = val;
		}
	}

	private void UpdateFame(int oldfame, int newfame)
	{
		StartCoroutine(MoveFame(oldfame, newfame));
	}

	private void UpdateMoney(int oldmoney, int newmoney)
	{
		defaultposition = 0;
		SetPlace();
		movePad.Kill();
		movePad = pad.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutSine);
		if (!isShown)
		{
			moveBox.Kill();
			moveBox = trans.DOAnchorPosY(shownPos, 0.7f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				DoMoveMoney(oldmoney, newmoney, thenout: true);
			});
		}
		else
		{
			DoMoveMoney(oldmoney, newmoney);
		}
	}

	private void DoMoveMoney(int oldmoney, int newmoney, bool thenout = false)
	{
		StartCoroutine(MoveMoney(oldmoney, newmoney, thenout));
	}

	private IEnumerator MoveMoney(int oldmoney, int newmoney, bool thenout)
	{
		float o = oldmoney;
		float n = newmoney;
		float t = 0f;
		if (oldmoney > newmoney)
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_money_buy);
		}
		else if (newmoney - oldmoney > 1999)
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_money_receive_funds);
		}
		else
		{
			JukeBox.diff.PlaySound(SFXTypes.sfx_money_count_small);
		}
		while (t < 1f)
		{
			float t2 = Easing.SineEaseInOut(t, 0f, 1f, 1f);
			float f = Mathf.Lerp(o, n, t2);
			money.text = SpeechAct.diff.GetSmartText("money", 0, Mathf.RoundToInt(f));
			t += Time.deltaTime * 1.5f;
			yield return 0;
		}
		money.text = SpeechAct.diff.GetSmartText("money", 0, newmoney);
		movePad = pad.DOAnchorPosY(defaultposition, 0.4f).SetEase(Ease.OutSine).SetDelay(1.4f);
		if (thenout)
		{
			moveBox = trans.DOAnchorPosY(hiddenPos, 0.7f).SetDelay(0.3f).SetEase(Ease.InBack);
		}
	}

	private IEnumerator MoveFame(int oldfame, int newfame)
	{
		if (Mathf.Abs(oldfame - newfame) > 10)
		{
			if (movePad != null && movePad.active)
			{
				yield return movePad.WaitForCompletion();
			}
			movePad = pad.DOAnchorPosY(50f, 0.4f).SetEase(Ease.OutSine);
			yield return movePad.WaitForCompletion();
		}
		float o = oldfame;
		float n = newfame;
		float t = 0f;
		while (t < 1f)
		{
			float t2 = Easing.SineEaseInOut(t, 0f, 1f, 1f);
			float f = Mathf.Lerp(o, n, t2);
			string[] array = SpeechAct.diff.GetSmartTextFinal("followers", 0, Mathf.RoundToInt(f)).Split(' ');
			followers.text = array[1];
			numfollowers.text = array[0];
			t += Time.deltaTime * 1.5f;
			yield return 0;
		}
		string[] array2 = SpeechAct.diff.GetSmartTextFinal("followers", 0, newfame).Split(' ');
		followers.text = array2[1];
		numfollowers.text = array2[0];
		nb_followers = newfame;
		movePad = pad.DOAnchorPosY(defaultposition, 0.4f).SetEase(Ease.OutSine).SetDelay(1.4f);
	}

	public void ShowMoney()
	{
		if (!isShown)
		{
			moneyAmo = GameAct.diff.GetInt(Variables.money);
			isShown = true;
			SetPlace();
			money.text = SpeechAct.diff.GetSmartText("money");
			string[] array = SpeechAct.diff.GetSmartText("followers", 0, GameAct.diff.GetInt(Variables.nb_fame)).Split(' ');
			followers.text = SpeechAct.diff.FinalFormat(array[1]);
			numfollowers.text = SpeechAct.diff.FinalFormat(array[0]);
			moveBox.Kill();
			moveBox = trans.DOAnchorPosY(shownPos, 0.2f);
			movePad = pad.DOAnchorPosY(defaultposition, 0.4f).SetEase(Ease.OutSine).SetDelay(1.4f);
		}
	}

	public void SetPlace()
	{
		string nameBack = BackgroundAct.diff.nameBack;
		place.text = SpeechAct.diff.GetSceneTextFinal(BackgroundAct.GetDisplayPlaceName(nameBack));
		placeimg.vectorGraphics = NavigationAct.diff.GetIconPlace(BackgroundAct.diff.GetType(), nameBack);
	}

	public void HideMoney()
	{
		isShown = false;
		moveBox.Kill();
		moveBox = trans.DOAnchorPosY(hiddenPos, 0.7f).SetEase(Ease.InBack);
	}
}
