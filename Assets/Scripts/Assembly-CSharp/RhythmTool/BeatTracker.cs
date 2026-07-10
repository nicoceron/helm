using System;
using UnityEngine;

namespace RhythmTool
{
	[DisallowMultipleComponent]
	[AddComponentMenu("RhythmTool/Beat Tracker")]
	public class BeatTracker : Analysis<Beat>
	{
		private float[] signalBuffer;

		private float[] signal;

		private float[] smoothedSignal;

		private float[] autoCorrelation;

		private float[] combFilter;

		private float[] lengthScore;

		private float[] offsetScore;

		private float[] signalWindow;

		private float[] offsetWindow;

		private float[] kernel;

		private float[] prevMagnitude;

		private float prevSpectralFlux;

		private int maxBeatLength;

		private int minBeatLength;

		private int beatLength;

		private int prevBeatLength;

		private int beatOffset;

		private int updateOffset;

		private int bufferSize;

		private int resolution = 10;

		private int combElements = 8;

		public override string name => "Beats";

		public override void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			base.Initialize(sampleRate, frameSize, hopSize);
			float num = (float)sampleRate * 60f / (float)hopSize;
			maxBeatLength = Mathf.RoundToInt(num / 80f);
			minBeatLength = Mathf.RoundToInt(num / 160f);
			bufferSize = maxBeatLength * combElements * 2;
			signalBuffer = new float[bufferSize];
			signal = new float[bufferSize];
			smoothedSignal = new float[bufferSize];
			autoCorrelation = new float[bufferSize];
			combFilter = new float[maxBeatLength * 2 * resolution];
			lengthScore = new float[maxBeatLength * resolution];
			offsetScore = new float[maxBeatLength * resolution];
			signalWindow = new float[bufferSize / 2];
			for (int i = 0; i < bufferSize / 2; i++)
			{
				signalWindow[i] = Util.HannWindow(i, bufferSize);
			}
			kernel = new float[8];
			for (int j = 0; j < kernel.Length; j++)
			{
				kernel[j] = Util.HannWindow(j, kernel.Length);
			}
			offsetWindow = new float[maxBeatLength * resolution];
			prevMagnitude = new float[frameSize / 2];
			prevSpectralFlux = 0f;
			prevBeatLength = 0;
			beatLength = (minBeatLength + minBeatLength / 2) * resolution;
			updateOffset = maxBeatLength;
			beatOffset = -1;
		}

		public override void Process(float[] samples, float[] magnitude, int frameIndex)
		{
			base.Process(samples, magnitude, frameIndex);
			float sample = GetSample(magnitude);
			signalBuffer[frameIndex % bufferSize] = sample;
			beatOffset--;
			updateOffset--;
			if (updateOffset == 0)
			{
				UpdateSignal();
				UpdateLength();
				UpdateOffset();
			}
			if (beatOffset == 0)
			{
				Beat feature = new Beat
				{
					timestamp = FrameIndexToSeconds(frameIndex),
					bpm = 60f / FrameIndexToSeconds((float)beatLength / (float)resolution)
				};
				AddFeature(feature);
			}
		}

		private float GetSample(float[] magnitude)
		{
			float num = 0f;
			for (int i = 0; i < magnitude.Length; i++)
			{
				num += Mathf.Max(magnitude[i] - prevMagnitude[i], 0f);
			}
			Array.Copy(magnitude, prevMagnitude, magnitude.Length);
			float result = num - prevSpectralFlux;
			prevSpectralFlux = num;
			return result;
		}

		private void UpdateSignal()
		{
			for (int i = 0; i < bufferSize; i++)
			{
				signal[i] = signalBuffer[(i + base.frameIndex + 1) % bufferSize];
			}
			Array.Clear(signal, 0, 4);
			Array.Clear(signal, signal.Length - 4, 4);
			Util.Smooth(signal, smoothedSignal, kernel);
			for (int j = 0; j < signalWindow.Length; j++)
			{
				smoothedSignal[j] *= signalWindow[j];
			}
		}

		private void UpdateOffset()
		{
			if (beatLength != prevBeatLength)
			{
				for (int i = 0; i < beatLength; i++)
				{
					offsetWindow[i] = 0.75f + Util.HannWindow(i, beatLength) * 0.25f;
				}
				Array.Clear(offsetScore, beatLength, offsetScore.Length - beatLength);
				if ((float)Mathf.Abs(beatLength - prevBeatLength) / (float)(minBeatLength * resolution) > 0.1f)
				{
					Array.Clear(offsetScore, 0, offsetScore.Length);
				}
			}
			float num = (float)beatLength / (float)resolution;
			for (int j = 0; j < beatLength; j++)
			{
				float num2 = 0f;
				float num3 = (float)j / (float)resolution;
				num3 = (float)(bufferSize - 1) - (num - num3);
				int num4 = Mathf.RoundToInt(num3 / num);
				for (int k = 0; k < num4; k++)
				{
					num2 += Util.Interpolate(smoothedSignal, num3 - (float)k * num);
				}
				float b = num2 / (float)num4 * offsetWindow[j];
				offsetScore[j] = Mathf.Lerp(offsetScore[j], b, 0.1f);
			}
			int num5 = Util.MaxIndex(offsetScore, 0, beatLength);
			beatOffset = Mathf.RoundToInt((float)num5 / (float)resolution);
			updateOffset = beatOffset + Mathf.RoundToInt(beatLength / 2 / resolution);
			if (offsetScore[num5] < 0.15f)
			{
				beatOffset = -1;
			}
		}

		private void UpdateLength()
		{
			UpdateAutoCorrelation();
			UpdateLengthScore();
			prevBeatLength = beatLength;
			beatLength = Util.MaxIndex(lengthScore, minBeatLength * resolution);
		}

		private void UpdateAutoCorrelation()
		{
			for (int i = minBeatLength / 2; i < autoCorrelation.Length; i++)
			{
				float num = 0f;
				for (int j = 0; j < smoothedSignal.Length - i; j++)
				{
					num += smoothedSignal[j] * smoothedSignal[j + i];
				}
				autoCorrelation[i] = num / (float)(smoothedSignal.Length - i);
			}
			float num2 = Util.Max(autoCorrelation, minBeatLength / 2);
			if (!(num2 < 1f))
			{
				for (int k = 0; k < autoCorrelation.Length; k++)
				{
					autoCorrelation[k] /= num2;
				}
			}
		}

		private void UpdateLengthScore()
		{
			for (int i = minBeatLength * resolution / 2; i < combFilter.Length - 1; i++)
			{
				float num = (float)i / (float)resolution;
				float num2 = 0f;
				for (int j = 0; j < combElements; j++)
				{
					num2 += Util.Interpolate(autoCorrelation, (float)(j + 1) * num);
				}
				combFilter[i] = num2 / (float)combElements;
			}
			for (int k = minBeatLength * resolution; k < lengthScore.Length; k++)
			{
				float b = combFilter[k] + combFilter[k / 2] + combFilter[k * 2];
				lengthScore[k] = Mathf.Lerp(lengthScore[k], b, 0.1f);
			}
		}
	}
}
