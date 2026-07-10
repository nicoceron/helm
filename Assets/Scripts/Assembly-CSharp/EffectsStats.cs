using System.Collections.Generic;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class EffectsStats : MonoBehaviour
{
	public GameObject titlePrefab;

	public GameObject portraitPrefab;

	public SVGAsset imgLocked;

	public GameObject slide;

	public GameObject noeffect;

	public EffectAct scEf;

	public Transform content;

	[SerializeField]
	private Scrollbar _scrollbar;

	private List<GameObject> objs = new List<GameObject>();

	private int r;

	private int c;

	private int min = -50;

	private int height = -150;

	private int lastpo;

	private List<string> nameList = new List<string>();

	private List<SVGAsset> imgList = new List<SVGAsset>();

	private List<SVGAsset> eyesList = new List<SVGAsset>();

	private List<string> txtList = new List<string>();

	private List<bool> seenList = new List<bool>();

	private int totalNb;

	private List<string> stats = new List<string>();

	public Guitar[] guitars;

	public int scrollBarSteps = 30;

	public bool dataLoaded;

	private void Awake()
	{
		DataLoad();
	}

	public void DataLoad()
	{
		if (dataLoaded)
		{
			return;
		}
		InitSection();
		r = 0;
		c = 0;
		lastpo = 0;
		stats = DeadCloneAct.diff.overallStats;
		foreach (Bearer regularBearer in GameAct.diff.GetRegularBearers())
		{
			nameList.Add(regularBearer.bearer.ToString());
			imgList.Add((SVGAsset)Resources.Load("bearers/" + regularBearer.bearer, typeof(SVGAsset)));
			string text = regularBearer.title.Get();
			if (string.IsNullOrEmpty(text) || text == " ")
			{
				text = regularBearer.generated.Get();
			}
			txtList.Add(SpeechAct.diff.FinalFormat(text));
			if (regularBearer.hasEyes)
			{
				eyesList.Add((SVGAsset)Resources.Load("eyes/" + regularBearer.bearer, typeof(SVGAsset)));
			}
			else
			{
				eyesList.Add(null);
			}
			totalNb++;
		}
		AddSection("character_stats", totalNb, "b_");
		Guitar[] array = guitars;
		foreach (Guitar guitar in array)
		{
			nameList.Add(guitar.name);
			imgList.Add((SVGAsset)Resources.Load("bearers/guitar-" + guitar.name, typeof(SVGAsset)));
			txtList.Add(SpeechAct.diff.GetSceneTextFinal(guitar.name + "_name"));
			totalNb++;
		}
		AddSection("guitar_stats", totalNb, "g_");
		foreach (Card item in GameAct.diff.GetHiddenCards().FindAll((Card it) => it.bearer == Bearers.end))
		{
			nameList.Add(item.bearerVariation);
			imgList.Add((SVGAsset)Resources.Load("deaths/" + item.bearerVariation, typeof(SVGAsset)));
			txtList.Add(SpeechAct.diff.GetSceneTextFinal("end_" + item.bearerVariation));
			totalNb++;
		}
		AddSection("death_stats", totalNb, "e_");
		foreach (Card item2 in GameAct.diff.GetHiddenCards().FindAll((Card it) => it.bearer == Bearers.gameover))
		{
			nameList.Add(item2.bearerVariation);
			imgList.Add((SVGAsset)Resources.Load("bearers/gameover-" + item2.bearerVariation, typeof(SVGAsset)));
			txtList.Add(SpeechAct.diff.GetSceneTextFinal("gameover_" + item2.bearerVariation));
			totalNb++;
		}
		AddSection("gameover_stats", totalNb, "o_");
		content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, -lastpo - height);
		_scrollbar.value = 0f;
		dataLoaded = true;
	}

	private void OnEnable()
	{
		if (InputAct.diff.curInput == Inputs.keyboard || InputAct.diff.curInput == Inputs.ninSwitch || InputAct.diff.curInput == Inputs.ps || InputAct.diff.curInput == Inputs.xbox)
		{
			if (_scrollbar.numberOfSteps != scrollBarSteps)
			{
				_scrollbar.numberOfSteps = scrollBarSteps;
			}
		}
		else
		{
			_scrollbar.numberOfSteps = 0;
		}
	}

	private void InitSection()
	{
		nameList = new List<string>();
		imgList = new List<SVGAsset>();
		txtList = new List<string>();
		seenList = new List<bool>();
		eyesList = new List<SVGAsset>();
		totalNb = 0;
	}

	private void AddSection(string statTitle, int totalNb, string suffix)
	{
		GameObject gameObject = Object.Instantiate(titlePrefab);
		objs.Add(gameObject);
		gameObject.transform.SetParent(content, worldPositionStays: false);
		gameObject.transform.GetChild(0).GetComponent<Text>().text = SpeechAct.diff.GetSceneTextFinal(statTitle);
		Text component = gameObject.transform.GetChild(1).GetComponent<Text>();
		Slider component2 = gameObject.transform.GetChild(2).GetComponent<Slider>();
		string sceneText = SpeechAct.diff.GetSceneText(statTitle, 1);
		lastpo = min + height * r;
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-66f, lastpo);
		r++;
		int num = 0;
		bool flag = false;
		for (int i = 0; i < imgList.Count; i++)
		{
			bool num2 = stats.Contains(suffix + nameList[i]);
			GameObject gameObject2 = Object.Instantiate(portraitPrefab);
			objs.Add(gameObject2);
			gameObject2.transform.SetParent(content, worldPositionStays: false);
			SVGImage component3 = gameObject2.GetComponent<SVGImage>();
			Text componentInChildren = gameObject2.GetComponentInChildren<Text>();
			if (num2)
			{
				num++;
				component3.vectorGraphics = imgList[i];
				if (eyesList.Count > 0 && eyesList[i] != null)
				{
					SVGImage component4 = gameObject2.transform.Find("eyes").GetComponent<SVGImage>();
					component4.vectorGraphics = eyesList[i];
					component4.enabled = true;
				}
			}
			else
			{
				component3.vectorGraphics = imgLocked;
			}
			componentInChildren.text = txtList[i];
			lastpo = min + height * r;
			gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-66 + c * 136, lastpo);
			c++;
			flag = false;
			if (c > 1)
			{
				r++;
				c = 0;
				flag = true;
			}
		}
		sceneText = sceneText.Replace("<number>", num.ToString());
		sceneText = sceneText.Replace("<total>", totalNb.ToString());
		component.text = SpeechAct.diff.FinalFormat(sceneText);
		float value = (float)num / (float)totalNb;
		component2.value = value;
		c = 0;
		if (!flag)
		{
			r++;
		}
		InitSection();
	}
}
