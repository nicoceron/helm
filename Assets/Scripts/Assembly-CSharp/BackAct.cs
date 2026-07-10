using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BackAct : MonoBehaviour
{
	public string backName;

	public string conditionString;

	private List<Condition> conditions;

	public List<Color> colorScheme;

	public List<BackLayer> layers;

	[HideInInspector]
	public Color mainColor;

	public Ease mainEase = Ease.OutBack;

	[HideInInspector]
	public float _tweenamount = 1f;

	public void Generate(string name)
	{
		bool flag = colorScheme.Count > 0;
		Color color = (flag ? colorScheme[Util.GetInt(name, 0, colorScheme.Count)] : Color.white);
		int num = 0;
		foreach (BackLayer layer in layers)
		{
			string n = name + " " + num;
			if ((bool)layer.variator)
			{
				Vector2 endPos = layer.variator.Generate(n);
				layer.endPos = endPos;
			}
			if (flag)
			{
				layer.endColor = color;
				if ((bool)layer.img && !layer.layer.CompareTag("IgnoreColor"))
				{
					layer.img.color = color;
				}
			}
			layer.startColor = Color.Lerp(Color.white, Color.black, Util.GetFloat(n, 0.6f));
			num++;
		}
	}

	public void Appear(string name, TweenCallback openmethod)
	{
		Generate(name);
		for (int i = 0; i < layers.Count; i++)
		{
			BackLayer backLayer = layers[i];
			RectTransform layer = backLayer.layer;
			layer.anchoredPosition = backLayer.endPos - backLayer.startPos;
			if (i == 0)
			{
				layer.DOAnchorPos(backLayer.endPos, 3f).SetEase(backLayer.easetype).OnComplete(openmethod);
			}
			else
			{
				layer.DOAnchorPos(backLayer.endPos, 3f).SetEase(backLayer.easetype);
			}
			StartCoroutine(BackEffect());
			if (!backLayer.layer.gameObject.CompareTag("IgnoreColor") && (bool)backLayer.img)
			{
				backLayer.img.color = backLayer.startColor;
				backLayer.img.DOColor(backLayer.endColor, 3f).SetEase(backLayer.easetype);
			}
		}
	}

	private IEnumerator BackEffect()
	{
		yield return 0;
		if (GameAct.diff.GetBool("crash"))
		{
			JukeBox.diff.StopAllSoundAndMusic();
			GameAct.diff.SetBool("crash", boo: false);
			CameffectAct.diff.PlayEffect(EffectStyles.crash);
		}
	}

	public void Disappear(TweenCallback closemethod)
	{
		for (int i = 0; i < layers.Count; i++)
		{
			BackLayer backLayer = layers[i];
			RectTransform layer = backLayer.layer;
			layer.anchoredPosition = backLayer.endPos;
			if (i == 0)
			{
				layer.DOAnchorPos(backLayer.endPos - backLayer.startPos, 1f).SetEase(backLayer.easetype).OnComplete(closemethod);
			}
			else
			{
				layer.DOAnchorPos(backLayer.endPos - backLayer.startPos, 1f).SetEase(backLayer.easetype);
			}
			if (!backLayer.layer.gameObject.CompareTag("IgnoreColor") && (bool)backLayer.img)
			{
				backLayer.img.color = backLayer.endColor;
				backLayer.img.DOColor(backLayer.startColor, 1f).SetEase(backLayer.easetype);
			}
		}
	}
}
