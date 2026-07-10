using System;
using UnityEngine;

namespace RhythmTool
{
	[AddComponentMenu("RhythmTool/Onset Detector")]
	public class OnsetDetector : Analysis<Onset>
	{
		[Range(0f, 1f)]
		[Tooltip("Normalize the song. A higher value helps find onsets for quiet songs, but can increase false positives.")]
		public float normalization = 0.2f;

		[Range(0f, 1f)]
		[Tooltip("Threshold for finding onsets. A lower value will make the onset detection more sensitive, but can increase false positives.")]
		public float threshold = 0.3f;

		[Range(2f, 32f)]
		[Tooltip("The size of the buffer determines the minimum time between detected onsets and how much of the surrounding data is used for calculating the threshold.")]
		public int bufferSize = 12;

		private int start;

		private int end = 1022;

		private float[] buffer;

		private float mean;

		private float m2;

		private float[] prevMagnitude;

		public override string name => "Onsets";

		public override void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			base.Initialize(sampleRate, frameSize, hopSize);
			buffer = new float[bufferSize];
			prevMagnitude = new float[frameSize / 2];
			mean = 1f;
			m2 = 0f;
		}

		public override void Process(float[] samples, float[] magnitude, int frameIndex)
		{
			base.Process(samples, magnitude, frameIndex);
			float sample = SpectralDifference(magnitude);
			sample = Normalize(sample);
			buffer[frameIndex % bufferSize] = sample;
			int num = Util.MaxIndex(buffer);
			if ((frameIndex - bufferSize / 2) % bufferSize == num)
			{
				float num2 = buffer[num];
				float num3 = Util.Mean(buffer, 0, bufferSize);
				if (num2 > num3 + threshold)
				{
					Onset feature = new Onset
					{
						timestamp = FrameIndexToSeconds(frameIndex - bufferSize / 2),
						strength = num2
					};
					AddFeature(feature);
				}
			}
		}

		private float SpectralDifference(float[] magnitude)
		{
			float num = 0f;
			for (int i = start; i < end; i++)
			{
				float f = Mathf.Abs(magnitude[i] * magnitude[i] - prevMagnitude[i] * prevMagnitude[i]);
				num += Mathf.Sqrt(f);
			}
			Array.Copy(magnitude, prevMagnitude, magnitude.Length);
			return num / (float)(end - start);
		}

		private float Normalize(float sample)
		{
			float num = sample - mean;
			mean += num / (float)(base.frameIndex + 1);
			m2 += num * (sample - mean);
			float num2 = Mathf.Sqrt(m2 / (float)(base.frameIndex + 1));
			return Mathf.Lerp(sample, (sample - mean) / num2, normalization);
		}
	}
}
