using System;
using UnityEngine;

namespace RhythmTool
{
	[AddComponentMenu("RhythmTool/Chromagram")]
	public class Chromagram : Analysis<Chroma>
	{
		private int startNote = 21;

		private int endNote = 89;

		private int bufferSize = 2048;

		private int downsampleFactor = 16;

		private int chromaInterval = 4;

		private int[] noteIndices;

		private float[] downsampled;

		private float[] spectrum;

		private float[] magnitude;

		private float[] window;

		private float[] pitchWindow;

		private float[] pitch;

		private float[] chroma;

		private int offset;

		private int[] chromaHistory;

		public override string name => "Chroma";

		public override void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			base.Initialize(sampleRate, frameSize, hopSize);
			noteIndices = new int[endNote - startNote];
			for (int i = 0; i < noteIndices.Length; i++)
			{
				float midiFrequency = GetMidiFrequency(i + startNote);
				noteIndices[i] = FrequencyToIndex(midiFrequency, bufferSize, sampleRate / downsampleFactor) - 1;
			}
			downsampled = new float[bufferSize];
			spectrum = new float[bufferSize];
			magnitude = new float[bufferSize / 2];
			window = Util.HannWindow(bufferSize);
			pitchWindow = new float[noteIndices.Length];
			for (int j = 0; j < noteIndices.Length; j++)
			{
				pitchWindow[j] = Util.HannWindow(j, noteIndices.Length * 2) + 0.1f;
			}
			pitch = new float[noteIndices.Length];
			chroma = new float[12];
			offset = bufferSize * downsampleFactor / hopSize / 2;
			chromaHistory = new int[12];
		}

		public override void Process(float[] samples, float[] magnitude, int frameIndex)
		{
			base.Process(samples, magnitude, frameIndex);
			Downsample(samples);
			if (frameIndex % chromaInterval == 0)
			{
				UpdateChroma();
			}
		}

		private void Downsample(float[] samples)
		{
			int num = base.hopSize / downsampleFactor;
			int num2 = base.frameSize - base.hopSize;
			for (int i = 0; i < bufferSize - num; i++)
			{
				downsampled[i] = downsampled[i + num];
			}
			for (int j = 0; j < num; j++)
			{
				float num3 = 0f;
				for (int k = 0; k < downsampleFactor; k++)
				{
					num3 += samples[num2 + j * downsampleFactor + k];
				}
				downsampled[bufferSize - num + j] = num3 / (float)downsampleFactor;
			}
		}

		private void UpdateChroma()
		{
			Array.Copy(downsampled, spectrum, bufferSize);
			Util.ApplyWindow(spectrum, window);
			Util.GetSpectrum(spectrum);
			Util.GetSpectrumMagnitude(spectrum, magnitude);
			for (int i = 0; i < pitch.Length; i++)
			{
				int num = noteIndices[i];
				int num2 = Mathf.FloorToInt((float)num * 0.015f);
				int start = Mathf.Max(num - num2, 0);
				int end = Mathf.Min(num + num2, magnitude.Length);
				float num3 = Util.Max(magnitude, start, end);
				pitch[i] = num3 * num3 * pitchWindow[i];
			}
			Array.Clear(chroma, 0, chroma.Length);
			for (int j = 0; j < pitch.Length; j++)
			{
				chroma[j % 12] += pitch[j];
			}
			float num4 = Util.Max(chroma);
			float num5 = Util.Mean(chroma);
			for (int k = 0; k < chroma.Length; k++)
			{
				chroma[k] = (chroma[k] - num5) / (num4 - num5);
			}
			for (int l = 0; l < chroma.Length; l++)
			{
				if (chroma[l] >= 0.9f && chromaHistory[l] == 0)
				{
					chromaHistory[l] = base.frameIndex;
				}
				if (chroma[l] < 0.8f && chromaHistory[l] != 0)
				{
					int num6 = chromaHistory[l];
					if (base.frameIndex - num6 > 5)
					{
						Chroma feature = new Chroma
						{
							timestamp = FrameIndexToSeconds(num6 - offset),
							length = FrameIndexToSeconds(base.frameIndex - num6),
							note = (Note)l
						};
						AddFeature(feature);
					}
					chromaHistory[l] = 0;
				}
			}
		}

		private static int FrequencyToIndex(float frequency, int length, int samplerate)
		{
			return Mathf.RoundToInt((float)length * frequency / (float)samplerate);
		}

		private static float GetMidiFrequency(int index)
		{
			return Mathf.Pow(2f, (float)(index - 69) / 12f) * 440f;
		}
	}
}
