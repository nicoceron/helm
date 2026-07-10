using UnityEngine;

namespace RhythmTool
{
	[AddComponentMenu("RhythmTool/Segmenter")]
	public class Segmenter : Analysis<Value>
	{
		[Range(0f, 64f)]
		[Tooltip("The threshold for detecting large differences in volume.")]
		public float threshold = 22f;

		[Range(1f, 16f)]
		[Tooltip("How much smoothing is applied to the audio signal.")]
		public int smoothing = 8;

		private Vector2 changeWeight = new Vector2(0.1f, 10f);

		private float changeStartSlope = 0.005f;

		private float changeEndSlope = 0.002f;

		private int iterations = 4;

		private int bufferSize;

		private float[][] buffer;

		private float[] kernel;

		private float w;

		private float current;

		private float next;

		private bool change;

		private float changeSign;

		private Vector2 changeStart;

		private float maxSlope;

		private int maxSlopeIndex;

		public override string name => "Segments";

		public override void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			base.Initialize(sampleRate, frameSize, hopSize);
			bufferSize = smoothing * 16;
			buffer = new float[iterations][];
			for (int i = 0; i < iterations; i++)
			{
				buffer[i] = new float[bufferSize];
			}
			kernel = Util.HannWindow(bufferSize);
			w = 0f;
			for (int j = 0; j < bufferSize; j++)
			{
				w += kernel[j];
			}
			maxSlope = 0f;
			maxSlopeIndex = 0;
		}

		public override void Process(float[] samples, float[] magnitude, int frameIndex)
		{
			base.Process(samples, magnitude, frameIndex);
			float num = Util.Mean(magnitude, 0, 350);
			for (int i = 0; i < iterations; i++)
			{
				for (int j = 0; j < bufferSize - 1; j++)
				{
					buffer[i][j] = buffer[i][j + 1];
				}
				if (i == 0)
				{
					buffer[i][bufferSize - 1] = num;
				}
				else
				{
					buffer[i][bufferSize - 1] = Util.WeightedSum(buffer[i - 1], kernel, bufferSize / 2) / w;
				}
			}
			num = Util.WeightedSum(buffer[iterations - 1], kernel, bufferSize / 2) / w;
			current = next;
			next = num;
			FindSegments();
		}

		private void FindSegments()
		{
			float num = Mathf.Abs(next - current);
			if (num > maxSlope)
			{
				maxSlope = num;
				maxSlopeIndex = base.frameIndex - bufferSize / 2 * iterations;
			}
			FindChangeEnd(num);
			FindChangeStart(num);
		}

		private void FindChangeEnd(float slope)
		{
			if (change && slope * changeSign < changeEndSlope)
			{
				float num = threshold;
				if (Mathf.Abs(slope) < changeStartSlope)
				{
					num *= 0.75f;
				}
				Vector2 a = new Vector2(base.frameIndex - bufferSize / 2 * iterations, current) - changeStart;
				if (Vector2.Scale(a, changeWeight).magnitude > num)
				{
					Value feature = new Value
					{
						timestamp = FrameIndexToSeconds(maxSlopeIndex),
						value = current
					};
					AddFeature(feature);
				}
				change = false;
			}
		}

		private void FindChangeStart(float slope)
		{
			if (!change && Mathf.Abs(slope) > changeStartSlope)
			{
				maxSlope = slope;
				maxSlopeIndex = base.frameIndex - bufferSize / 2 * iterations;
				changeStart = new Vector2(maxSlopeIndex, current);
				change = true;
				changeSign = Mathf.Sign(slope);
			}
		}
	}
}
