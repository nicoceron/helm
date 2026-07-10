using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AudioEvent : MonoBehaviour
{
	[Serializable]
	public class TriggerEvent : UnityEvent<float>
	{
	}

	[Range(0f, 1f)]
	public float spectrumStart = 0.5f;

	[Range(0f, 1f)]
	public float spectrumLength = 0.25f;

	[Range(0f, 1f)]
	public float stereoPan = 0.5f;

	public AnimationCurve spectrumFalloff = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	public float amplifier = 1f;

	[FormerlySerializedAs("onAudio")]
	[SerializeField]
	protected TriggerEvent m_onAudio = new TriggerEvent();

	public TriggerEvent onAudio
	{
		get
		{
			return m_onAudio;
		}
		set
		{
			m_onAudio = value;
		}
	}

	private void Update()
	{
		int resolution = AudioSpectrum.Instance.resolution;
		int num = Mathf.Clamp(Mathf.RoundToInt(spectrumStart * (float)resolution) - Mathf.RoundToInt((float)resolution * 0.5f), 0, resolution - 1);
		int end = Mathf.Clamp(num + Mathf.RoundToInt((float)resolution * spectrumLength), 0, resolution - 1);
		float num2 = ((stereoPan == 0f) ? GetVelocity(AudioSpectrum.Instance.leftChannel, num, end, spectrumFalloff) : ((stereoPan != 1f) ? Mathf.Lerp(GetVelocity(AudioSpectrum.Instance.leftChannel, num, end, spectrumFalloff), GetVelocity(AudioSpectrum.Instance.leftChannel, num, end, spectrumFalloff), stereoPan) : GetVelocity(AudioSpectrum.Instance.rightChannel, num, end, spectrumFalloff)));
		onAudio.Invoke(num2 * amplifier);
	}

	private float GetVelocity(float[] channel, int start, int end, AnimationCurve falloff)
	{
		if (start == end)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = end - start;
		float num4 = 0f;
		for (int i = start; i < end; i++)
		{
			num2 = num4 / num3;
			num += channel[i] * falloff.Evaluate(num2);
			num4 += 1f;
		}
		return num / num3;
	}
}
