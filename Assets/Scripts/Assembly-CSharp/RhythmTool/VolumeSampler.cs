using UnityEngine;

namespace RhythmTool
{
	[AddComponentMenu("RhythmTool/Volume Sampler")]
	public class VolumeSampler : Analysis<Value>
	{
		[SerializeField]
		[Range(1f, 64f)]
		[Tooltip("How often to sample volume.")]
		private int _interval = 4;

		[SerializeField]
		[Range(0f, 16f)]
		[Tooltip("How much smoothing is applied.")]
		private int _smoothing = 8;

		private int bufferSize;

		private int smoothingBufferSize;

		private float[] buffer;

		private float[] smoothingBuffer;

		private float[] smoothingKernel;

		private float w;

		public override string name => "Volume";

		public int interval
		{
			get
			{
				return _interval;
			}
			set
			{
				_interval = Mathf.Clamp(value, 1, 64);
			}
		}

		public int smoothing
		{
			get
			{
				return _smoothing;
			}
			set
			{
				_smoothing = Mathf.Clamp(value, 0, 16);
			}
		}

		public override void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			base.Initialize(sampleRate, frameSize, hopSize);
			bufferSize = _interval;
			buffer = new float[bufferSize];
			if (_smoothing == 0)
			{
				smoothingBufferSize = 0;
				return;
			}
			smoothingBufferSize = _smoothing + 2;
			smoothingKernel = Util.HannWindow(smoothingBufferSize);
			smoothingBuffer = new float[smoothingBufferSize];
			w = 0f;
			for (int i = 0; i < smoothingBufferSize; i++)
			{
				w += smoothingKernel[i];
			}
		}

		public override void Process(float[] samples, float[] magnitude, int frameIndex)
		{
			base.Process(samples, magnitude, frameIndex);
			float num = Util.Mean(magnitude, 0, magnitude.Length);
			int num2 = frameIndex % bufferSize;
			buffer[num2] = num;
			if (num2 != 0)
			{
				return;
			}
			float num3 = Util.Mean(buffer, 0, bufferSize);
			if (smoothingBufferSize > 0)
			{
				for (int i = 0; i < smoothingBufferSize - 1; i++)
				{
					smoothingBuffer[i] = smoothingBuffer[i + 1];
				}
				smoothingBuffer[smoothingBufferSize - 1] = num3;
				num3 = Util.WeightedSum(smoothingBuffer, smoothingKernel, smoothingBufferSize / 2) / w;
			}
			Value feature = new Value
			{
				timestamp = FrameIndexToSeconds(frameIndex - bufferSize * smoothingBufferSize / 2),
				value = num3
			};
			AddFeature(feature);
		}
	}
}
