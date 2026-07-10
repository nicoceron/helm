using System;
using UnityEngine;

namespace RhythmTool
{
	public class Util
	{
		private static LomontFFT fft = new LomontFFT();

		public static void GetMono(float[] samples, float[] monoSamples, int channels = 0)
		{
			if (channels == 0)
			{
				channels = samples.Length / monoSamples.Length;
			}
			if (samples.Length % monoSamples.Length != 0)
			{
				throw new ArgumentException("samples length is not a multiple of monoSamples length.");
			}
			if (monoSamples.Length * channels != samples.Length)
			{
				throw new ArgumentException("monoSamples length does not match samples length for " + channels + " channels");
			}
			for (int i = 0; i < monoSamples.Length; i++)
			{
				float num = 0f;
				for (int j = 0; j < channels; j++)
				{
					num += samples[i * channels + j];
				}
				num /= (float)channels;
				monoSamples[i] = num * 1.4f;
			}
		}

		public static void GetSpectrum(float[] samples)
		{
			fft.RealFFT(samples, forward: true);
		}

		public static void GetSpectrumMagnitude(float[] spectrum, float[] magnitude)
		{
			if (magnitude.Length != spectrum.Length / 2)
			{
				throw new Exception("magnitude length has to be half of spectrum length.");
			}
			for (int i = 0; i < magnitude.Length - 2; i++)
			{
				int num = i * 2 + 2;
				float num2 = spectrum[num];
				float num3 = spectrum[num + 1];
				magnitude[i] = Mathf.Sqrt(num2 * num2 + num3 * num3);
			}
			magnitude[magnitude.Length - 2] = spectrum[0];
			magnitude[magnitude.Length - 1] = spectrum[1];
		}

		public static void GetSpectrumPhase(float[] spectrum, float[] phase)
		{
			if (phase.Length != spectrum.Length / 2)
			{
				throw new Exception("phase length has to be half of spectrum length.");
			}
			for (int i = 0; i < phase.Length - 2; i++)
			{
				int num = i * 2 + 2;
				phase[i] = Mathf.Atan2(spectrum[num + 1], spectrum[num]);
			}
			phase[phase.Length - 2] = spectrum[0];
			phase[phase.Length - 1] = spectrum[1];
		}

		internal static void ApplyWindow(float[] array, float[] window)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] *= window[i];
			}
		}

		public static float Mean(float[] array, int start = 0, int end = 0)
		{
			if (end == 0)
			{
				end = array.Length;
			}
			float num = 0f;
			for (int i = start; i < end; i++)
			{
				num += array[i];
			}
			return num / (float)(end - start);
		}

		public static float WeightedSum(float[] array, float[] kernel, int index)
		{
			float num = 0f;
			int num2 = index - kernel.Length / 2;
			int num3 = index + kernel.Length / 2;
			for (int i = num2; i < num3; i++)
			{
				if (i > 0 && i < array.Length)
				{
					num += array[i] * kernel[i - num2];
				}
			}
			return num;
		}

		public static int MaxIndex(float[] array, int start = 0, int end = 0)
		{
			if (end == 0)
			{
				end = array.Length;
			}
			int num = start;
			for (int i = start; i < end; i++)
			{
				if (array[i] > array[num])
				{
					num = i;
				}
			}
			return num;
		}

		public static int MinIndex(float[] array, int start = 0, int end = 0)
		{
			if (end == 0)
			{
				end = array.Length;
			}
			int num = start;
			for (int i = start; i < end; i++)
			{
				if (array[i] < array[num])
				{
					num = i;
				}
			}
			return num;
		}

		public static float Max(float[] array, int start = 0, int end = 0)
		{
			return array[MaxIndex(array, start, end)];
		}

		public static float Min(float[] array, int start = 0, int end = 0)
		{
			return array[MinIndex(array, start, end)];
		}

		public static void Smooth(float[] array, float[] smoothedArray, float[] kernel)
		{
			for (int i = 0; i < array.Length; i++)
			{
				smoothedArray[i] = WeightedSum(array, kernel, i) / (float)kernel.Length;
			}
		}

		public static float Interpolate(float[] array, float index)
		{
			int num = (int)index;
			if (num == array.Length - 1)
			{
				return array[array.Length - 1];
			}
			return array[num] + (array[num + 1] - array[num]) * (index - (float)num);
		}

		public static void HannWindow(float[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = HannWindow(i, array.Length);
			}
		}

		public static float[] HannWindow(int length)
		{
			float[] array = new float[length];
			HannWindow(array);
			return array;
		}

		public static float HannWindow(int n, int windowSize)
		{
			return 0.5f * (1f - Mathf.Cos((float)Math.PI * 2f * (float)n / (float)(windowSize - 1)));
		}
	}
}
