using System.Collections;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class AnimBut : MonoBehaviour
{
	public SVGAsset psBut;

	public SVGAsset psBackBut;

	public SVGAsset psMenuBut;

	public SVGAsset psInventoryBut;

	public SVGAsset xboxBut;

	public SVGAsset xboxBackBut;

	public SVGAsset xboxMenuBut;

	public SVGAsset xboxInventoryBut;

	public SVGAsset SwitchBut;

	public SVGAsset SwitchBackBut;

	public SVGAsset SwitchMenuBut;

	public SVGAsset SwitchInventoryBut;

	public SVGAsset tvMenuBut;

	public SVGAsset tvInventoryBut;

	public SVGAsset defaultBut;

	private float amo = 1.1f;

	private RectTransform trans;

	private Vector2 osize = new Vector2(40f, 40f);

	public static AnimBut diff;

	public bool startLocked;

	private Button but;

	public Text txt;

	private AutoText scTxt;

	private AutoText scImg;

	public Text txt2;

	public AutoText sc2Txt;

	public AutoText sc2Img;

	private SVGImage img;

	public SVGImage img2;

	public ControlModes mode = ControlModes.none;

	private ControlModes lastmode = ControlModes.none;

	private Inputs typeCont = Inputs.none;

	private RectTransform patrans;

	private bool lastlock;

	private void Awake()
	{
		but = base.transform.parent.GetComponentInChildren<Button>();
		trans = GetComponent<RectTransform>();
		patrans = base.transform.parent.GetComponent<RectTransform>();
		img = GetComponent<SVGImage>();
		scImg = GetComponent<AutoText>();
		scTxt = txt.GetComponent<AutoText>();
	}

	public void ResetLock(bool withlastmode = true)
	{
		if (!withlastmode)
		{
			lastmode = mode;
		}
		if (lastmode == ControlModes.none)
		{
			Lock();
		}
		else
		{
			UnLock(lastmode, lastlock, withlastmode);
		}
	}

	public void UnLock(ControlModes nmode, bool withoutbut = false, bool withlastmode = true)
	{
		if (InputAct.diff.isInMenu && nmode == ControlModes.next)
		{
			lastmode = nmode;
			return;
		}
		but.gameObject.SetActive(!withoutbut);
		lastlock = withoutbut;
		scTxt.AnteMove();
		scImg.AnteMove();
		sc2Img.AnteMove();
		sc2Txt.AnteMove();
		if (nmode == mode && withlastmode)
		{
			return;
		}
		Inputs curInput = InputAct.diff.curInput;
		if ((nmode == ControlModes.multichoice || nmode == ControlModes.selectback) && !InputAct.diff.NavigationMode())
		{
			Lock();
			return;
		}
		if (withlastmode)
		{
			lastmode = mode;
		}
		mode = nmode;
		UpdateControl(curInput);
	}

	public void Lock(bool direct = false)
	{
		but.gameObject.SetActive(value: false);
		if (direct)
		{
			scTxt.MoveDirect();
			scImg.MoveDirect();
			sc2Txt.MoveDirect();
			sc2Img.MoveDirect();
			sc2Txt.StopAllCoroutines();
			sc2Img.StopAllCoroutines();
		}
		else
		{
			scTxt.Move();
			scImg.Move();
			sc2Txt.Move();
			sc2Img.Move();
		}
		lastmode = mode;
		mode = ControlModes.none;
	}

	private void OnEnable()
	{
		diff = this;
	}

	public void SwitchSize(bool tall)
	{
		but.GetComponent<RectTransform>().sizeDelta = (tall ? new Vector2(10000f, 950f) : new Vector2(10000f, 140f));
	}

	private void SetGraphics(SVGAsset sel, SVGAsset back, SVGAsset menu, SVGAsset inventory)
	{
		string text = ((typeCont == Inputs.leap) ? "action_leap" : "action_next");
		switch (mode)
		{
		case ControlModes.next:
			ApplySingle(sel, text);
			break;
		case ControlModes.nextmenu:
			ApplyBoth(sel, menu, text, "action_menu");
			break;
		case ControlModes.resetmenu:
			ApplyBoth(sel, menu, "action_reset", "action_menu");
			break;
		case ControlModes.selectback:
			ApplyBoth(sel, back, "action_select", "action_back");
			break;
		case ControlModes.multichoice:
			ApplySingle(sel, "action_select");
			break;
		}
	}

	private void ApplySingle(SVGAsset but1, string txt1)
	{
		sc2Txt.Move();
		sc2Img.Move();
		SVGImage sVGImage = img2;
		bool flag = (txt2.enabled = false);
		sVGImage.enabled = flag;
		SVGImage sVGImage2 = img;
		flag = (txt.enabled = true);
		sVGImage2.enabled = flag;
		img.vectorGraphics = but1;
		txt.text = ((typeCont == Inputs.touch) ? "" : SpeechAct.diff.GetSceneTextFinal(txt1));
	}

	private void ApplyBoth(SVGAsset but1, SVGAsset but2, string text1, string text2)
	{
		if (but2 == null)
		{
			ApplySingle(but1, text1);
			return;
		}
		SVGImage sVGImage = img2;
		Text text3 = txt2;
		SVGImage sVGImage2 = img;
		bool flag = (txt.enabled = true);
		bool flag3 = (sVGImage2.enabled = flag);
		bool flag5 = (text3.enabled = flag3);
		sVGImage.enabled = flag5;
		img.vectorGraphics = but1;
		img2.vectorGraphics = but2;
		txt.text = SpeechAct.diff.GetSceneTextFinal(text1);
		txt2.text = SpeechAct.diff.GetSceneTextFinal(text2);
	}

	public void UpdateControl(Inputs type)
	{
		typeCont = type;
		if (!InputAct.diff.isLandscape())
		{
			patrans.anchoredPosition = new Vector2(184f, 0f);
		}
		else
		{
			patrans.anchoredPosition = new Vector2(0f, 0f);
		}
		switch (type)
		{
		case Inputs.ps:
			SetGraphics(psBut, psBackBut, psMenuBut, psInventoryBut);
			break;
		case Inputs.xbox:
			SetGraphics(xboxBut, xboxBackBut, xboxMenuBut, xboxInventoryBut);
			break;
		case Inputs.ninSwitch:
			SetGraphics(SwitchBut, SwitchBackBut, SwitchMenuBut, SwitchInventoryBut);
			break;
		case Inputs.tv:
			SetGraphics(defaultBut, tvInventoryBut, tvMenuBut, tvInventoryBut);
			break;
		default:
			SetGraphics(defaultBut, null, null, null);
			break;
		}
	}

	private void OnDisable()
	{
		diff = null;
	}

	private IEnumerator DoAnimate()
	{
		while (true)
		{
			float t = 0f;
			while (t < 1f)
			{
				amo = Mathf.Lerp(amo, 1.1f, Time.deltaTime);
				trans.sizeDelta = Vector2.LerpUnclamped(osize, osize * amo, Easing.ElasticEaseInOut(Mathf.PingPong(t * 0.8f * amo, 1f), 0f, 1f, 1f));
				t += Time.deltaTime * 3f;
				yield return null;
			}
			t = 0f;
			while (t < 1f)
			{
				amo = Mathf.Lerp(amo, 1.1f, Time.deltaTime);
				trans.sizeDelta = Vector2.Lerp(osize * amo, osize, t);
				t += Time.deltaTime * 4f;
				yield return null;
			}
			yield return null;
		}
	}

	public void Tap()
	{
		amo = 1.5f;
		if ((bool)JukeBox.diff)
		{
			JukeBox.diff.PlaySound(SFXTypes.ui_button_next);
		}
	}
}
