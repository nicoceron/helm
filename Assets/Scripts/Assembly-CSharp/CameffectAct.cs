using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameffectAct : MonoBehaviour
{
	public RectTransform GameScreen;

	public static CameffectAct diff;

	public Volume volume;

	public Volume concertVolume;

	public List<EffectProfile> profiles;

	private List<string> valueDanger = new List<string>();

	private bool isPlaying;

	public bool isInDanger;

	private EffectStyles curEffect = EffectStyles.none;

	private void Awake()
	{
		diff = this;
	}

	public void SetConcertVolume(bool ison)
	{
		concertVolume.enabled = true;
		float endValue = (ison ? 0.2f : 0f);
		if (ison)
		{
			concertVolume.weight = 0f;
		}
		DOTween.Kill(999);
		DOTween.To(() => concertVolume.weight, delegate(float x)
		{
			concertVolume.weight = x;
		}, endValue, 0.3f).OnComplete(delegate
		{
			concertVolume.enabled = ison;
		});
	}

	public void NewDanger(string v)
	{
		if (!valueDanger.Contains(v))
		{
			valueDanger.Add(v);
		}
		if (!isPlaying && valueDanger.Count > 1)
		{
			isInDanger = true;
			PlayEffect(EffectStyles.alert);
			NormalScreenShake();
		}
	}

	public void SmoothScreenShake()
	{
		GameScreen.DOComplete();
		GameScreen.DOPunchPosition(new Vector3(0f, Util.Rand(-0.02f), 0f), 0.3f);
		DOTween.Kill(999);
		DOTween.To(() => concertVolume.weight, delegate(float x)
		{
			concertVolume.weight = x;
		}, 1f, 0.06f).SetEase(Ease.OutCubic).SetId(999)
			.OnComplete(delegate
			{
				DOTween.To(() => concertVolume.weight, delegate(float y)
				{
					concertVolume.weight = y;
				}, 0.2f, 0.3f).SetEase(Ease.InCubic).SetId(999);
			});
	}

	public void NormalScreenShake()
	{
		GameScreen.DOComplete();
		GameScreen.DOPunchPosition(new Vector3(Util.Rand(-5f, 5f), Util.Rand(-1f), 0f), 0.5f).SetDelay(0.2f);
	}

	public void RemoveDanger(string v)
	{
		if (valueDanger.Contains(v))
		{
			valueDanger.Remove(v);
		}
		if (valueDanger.Count < 2)
		{
			isInDanger = false;
			StopEffect(ifloop: false, EffectStyles.alert);
		}
	}

	public void PlayEffect(EffectStyles style)
	{
		StopEffect();
		EffectProfile effectProfile = profiles.Find((EffectProfile it) => it.style == style);
		isPlaying = true;
		curEffect = effectProfile.style;
		if (curEffect == EffectStyles.concert)
		{
			effectProfile.volumeprofile.TryGet<ColorAdjustments>(typeof(ColorAdjustments), out var component);
			component.hueShift = new ClampedFloatParameter(Util.Rand(-100f, 100f), -180f, 180f);
		}
		StartCoroutine("DoPlayEffect", effectProfile);
	}

	public void StopEffect(bool ifloop = false, EffectStyles style = EffectStyles.none)
	{
		if (style == EffectStyles.none || curEffect == style)
		{
			EffectProfile effectProfile = profiles.Find((EffectProfile it) => it.style == curEffect);
			if ((!(effectProfile != null) || !(!effectProfile.loop && ifloop)) && isPlaying)
			{
				isPlaying = false;
				StopCoroutine("DoPlayEffect");
				volume.weight = 0f;
				volume.profile = null;
			}
		}
	}

	private IEnumerator DoPlayEffect(EffectProfile profile)
	{
		volume.profile = profile.volumeprofile;
		volume.weight = 0f;
		bool loop = profile.loop;
		float time = profile.time;
		float t = 0f;
		if (profile.gotone > 0f)
		{
			while (t < 1f)
			{
				t += Time.deltaTime / profile.gotone;
				volume.weight = t * t;
				yield return 0;
			}
		}
		if (profile.sound != SFXTypes.none)
		{
			JukeBox.diff.PlaySound(profile.sound);
		}
		t = 0f;
		while (t < time)
		{
			float weight = profile.intensity.Evaluate(t);
			volume.weight = weight;
			t += Time.deltaTime;
			if (t > time && loop)
			{
				t = 0f;
				if (profile.sound != SFXTypes.none)
				{
					JukeBox.diff.PlaySound(profile.sound);
				}
			}
			yield return 0;
		}
		StopEffect();
	}
}
