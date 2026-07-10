using UnityEngine;

public class AudioSpectrum : MonoBehaviour
{
	protected static AudioSpectrum _Instance;

	public AudioSource audioSource;

	public int resolution = 32;

	[HideInInspector]
	public float[] leftChannel;

	[HideInInspector]
	public float[] rightChannel;

	public static AudioSpectrum Instance => _Instance;

	private void Awake()
	{
		_Instance = this;
		leftChannel = new float[resolution];
		rightChannel = new float[resolution];
	}

	private void Update()
	{
		if (leftChannel == null || leftChannel.Length != resolution)
		{
			leftChannel = new float[resolution];
		}
		if (rightChannel == null || rightChannel.Length != resolution)
		{
			rightChannel = new float[resolution];
		}
		audioSource.GetSpectrumData(leftChannel, 0, FFTWindow.BlackmanHarris);
		audioSource.GetSpectrumData(rightChannel, 1, FFTWindow.BlackmanHarris);
	}
}
