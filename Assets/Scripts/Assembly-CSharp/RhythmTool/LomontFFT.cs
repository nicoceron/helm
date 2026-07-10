using System;
using UnityEngine;

namespace RhythmTool
{
	public class LomontFFT
	{
		private float[] cosTable;

		private float[] sinTable;

		public void FFT(float[] data, bool forward)
		{
			int num = data.Length;
			if ((num & (num - 1)) != 0)
			{
				throw new ArgumentException("data length " + num + " in FFT is not a power of 2");
			}
			num /= 2;
			BitReverse(data);
			if (cosTable == null || cosTable.Length != num)
			{
				InitializeTables(num);
			}
			float num2 = (forward ? 1 : (-1));
			int num3 = 0;
			for (int num4 = 2; num4 <= num; num4 *= 2)
			{
				for (int i = 0; i < num4; i += 2)
				{
					float num5 = cosTable[num3];
					float num6 = num2 * sinTable[num3];
					num3++;
					for (int j = i; j < 2 * num; j += 2 * num4)
					{
						int num7 = j + num4;
						float num8 = num5 * data[num7] - num6 * data[num7 + 1];
						float num9 = num6 * data[num7] + num5 * data[num7 + 1];
						data[num7] = data[j] - num8;
						data[num7 + 1] = data[j + 1] - num9;
						data[j] += num8;
						data[j + 1] += num9;
					}
				}
			}
		}

		public void RealFFT(float[] data, bool forward)
		{
			if (forward)
			{
				FFT(data, forward: true);
			}
			Reconstruct(data, forward);
			if (forward)
			{
				float num = data[0];
				data[0] += data[1];
				data[1] = num - data[1];
			}
			else
			{
				float num2 = data[0];
				data[0] = 0.5f * (num2 + data[1]);
				data[1] = 0.5f * (num2 - data[1]);
				FFT(data, forward: false);
			}
		}

		private void Reconstruct(float[] data, bool forward)
		{
			int num = data.Length;
			float num2 = (forward ? 1 : (-1));
			float num3 = 0.5f;
			float num4 = (float)Math.PI / (float)(num / 2) * num2;
			float num5 = Mathf.Sin(0.5f * num4);
			float num6 = -2f * num5 * num5;
			float num7 = Mathf.Sin(num4);
			float num8 = 1f + num6;
			float num9 = num7;
			for (int i = 1; i < num / 4; i++)
			{
				int num10 = 2 * i;
				int num11 = num - 2 * i;
				float num12 = num3 * (data[num10] + data[num11]);
				float num13 = num3 * (data[num10 + 1] - data[num11 + 1]);
				float num14 = num3 * num2 * (data[num10 + 1] + data[num11 + 1]);
				float num15 = num3 * (0f - num2) * (data[num10] - data[num11]);
				data[num10] = num12 + num8 * num14 - num9 * num15;
				data[num10 + 1] = num13 + num8 * num15 + num9 * num14;
				data[num11] = num12 - num8 * num14 + num9 * num15;
				data[num11 + 1] = 0f - num13 + num8 * num15 + num9 * num14;
				num8 = (num5 = num8) * num6 - num9 * num7 + num8;
				num9 = num9 * num6 + num5 * num7 + num9;
			}
		}

		private void InitializeTables(int length)
		{
			cosTable = new float[length];
			sinTable = new float[length];
			int num = 0;
			for (int num2 = 2; num2 <= length; num2 *= 2)
			{
				float num3 = (float)Math.PI / (float)(num2 / 2);
				float num4 = 1f;
				float num5 = 0f;
				float num6 = Mathf.Sin(num3);
				float num7 = Mathf.Sin(num3 / 2f);
				num7 = -2f * num7 * num7;
				for (int i = 0; i < num2; i += 2)
				{
					cosTable[num] = num4;
					sinTable[num++] = num5;
					float num8 = num4;
					num4 = num4 * num7 - num5 * num6 + num4;
					num5 = num5 * num7 + num8 * num6 + num5;
				}
			}
		}

		private static void BitReverse(float[] data)
		{
			int num = data.Length;
			int num2 = num >> 1;
			int num3 = 0;
			for (int i = 0; i < num - 1; i += 2)
			{
				if (i < num3)
				{
					Swap(data, i, num3);
					Swap(data, i + 1, num3 + 1);
				}
				int num4;
				for (num4 = num2; num4 <= num3; num4 >>= 1)
				{
					num3 -= num4;
				}
				num3 += num4;
			}
		}

		private static void Swap(float[] data, int a, int b)
		{
			float num = data[a];
			data[a] = data[b];
			data[b] = num;
		}
	}
}
