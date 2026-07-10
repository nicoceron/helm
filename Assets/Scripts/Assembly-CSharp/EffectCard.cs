using System.Collections;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class EffectCard : CardAct
{
	public SVGImage icon;

	public Text title;

	public Text description;

	public EffectAct scEf;

	public Text question;

	private void Awake()
	{
		_Awake();
	}

	private void Start()
	{
		if (SpeechAct.diff.asiaLayout)
		{
			description.fontSize = 13;
			description.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
		else
		{
			description.fontSize = 15;
			description.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
	}

	public void InitEffect(Effect effect, int dec)
	{
		icon.vectorGraphics = (SVGAsset)Resources.Load("effects/" + effect.tag, typeof(SVGAsset));
		title.text = effect.title;
		description.text = effect.description;
		question.text = SpeechAct.diff.GetSceneText("effects");
		base.InitCard(effect.tag, effect.title, effect.description, dec);
		JukeBox.diff.PlaySound(SFXTypes.ui_effect_received);
		StartCoroutine("LateAudio", effect.tag);
	}

	private IEnumerator LateAudio(string tag)
	{
		yield return new WaitForSeconds(0.4f);
		AudioClip type = (AudioClip)Resources.Load("effects_sfx/" + tag, typeof(AudioClip));
		JukeBox.diff.PlaySound(type);
	}

	public override void HideCard()
	{
		GameAct.diff.HideOutcome();
		base.HideCard();
	}

	public override void HideFond()
	{
		base.HideFond();
	}

	public override void ShowDecision(int dec)
	{
	}
}
