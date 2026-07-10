using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace RhythmTool
{
	[ExecuteInEditMode]
	[AddComponentMenu("RhythmTool/Analyzer", -1)]
	public class RhythmAnalyzer : MonoBehaviour
	{
		private AudioClip audioClip;

		private int hopSize = 1024;

		private int frameSize = 2048;

		private int bufferCount = 128;

		private int channels;

		private int sampleRate;

		private int totalFrames;

		private int lastFrame;

		private float[] buffer;

		private float[] window;

		private float[] samples;

		private float[] monoSamples;

		private float[] spectrum;

		private float[] magnitude;

		private Thread analyze;

		private AutoResetEvent waitForMainThread;

		private bool getData;

		private bool abort;

		private int initialLength = 5;

		private List<Analysis> analyses = new List<Analysis>();

		public RhythmData rhythmData { get; private set; }

		public float progress { get; private set; }

		public bool isDone { get; private set; }

		public bool initialized { get; private set; }

		public event Action<RhythmData> Initialized;

		public RhythmData Analyze(AudioClip audioClip, int initialLength = 5)
		{
			Abort();
			this.audioClip = audioClip;
			this.initialLength = initialLength;
			Initialize();
			return rhythmData;
		}

		public void Abort()
		{
			if (!abort && analyze != null && analyze.IsAlive)
			{
				getData = false;
				abort = true;
				waitForMainThread.Set();
				analyze.Join();
			}
		}

		private void Initialize()
		{
			abort = false;
			isDone = false;
			initialized = false;
			progress = 0f;
			lastFrame = 0;
			totalFrames = audioClip.samples / hopSize;
			channels = audioClip.channels;
			sampleRate = audioClip.frequency;
			initialLength *= sampleRate / hopSize;
			GetComponents(analyses);
			analyses.RemoveAll((Analysis a) => !a.enabled);
			foreach (Analysis analysis in analyses)
			{
				analysis.Initialize(sampleRate, frameSize, hopSize);
			}
			rhythmData = RhythmData.Create(audioClip.name, analyses.Select((Analysis a) => a.track));
			StartAnalyze();
		}

		private void StartAnalyze()
		{
			int num = hopSize * bufferCount + (frameSize - hopSize);
			buffer = new float[num * channels];
			window = Util.HannWindow(frameSize);
			samples = new float[frameSize * channels];
			monoSamples = new float[frameSize];
			spectrum = new float[frameSize];
			magnitude = new float[frameSize / 2];
			waitForMainThread = new AutoResetEvent(initialState: false);
			analyze = new Thread(Analyze);
			analyze.Start();
		}

		private void Analyze()
		{
			while (lastFrame < totalFrames && !abort)
			{
				int num = lastFrame % bufferCount;
				if (num == 0)
				{
					FillBuffer();
				}
				Array.Copy(buffer, num * hopSize * channels, samples, 0, samples.Length);
				ProcessFrame(samples);
				lastFrame++;
				progress = (float)lastFrame / (float)totalFrames;
			}
			OnAnalysisDone();
		}

		private void OnAnalysisDone()
		{
			isDone = true;
		}

		private void ProcessFrame(float[] samples)
		{
			Util.GetMono(samples, monoSamples, channels);
			Array.Copy(monoSamples, spectrum, frameSize);
			Util.ApplyWindow(spectrum, window);
			Util.GetSpectrum(spectrum);
			Util.GetSpectrumMagnitude(spectrum, magnitude);
			foreach (Analysis analysis in analyses)
			{
				analysis.Process(monoSamples, magnitude, lastFrame);
			}
		}

		private void FillBuffer()
		{
			getData = true;
			waitForMainThread.WaitOne();
		}

		private void GetData()
		{
			if (audioClip == null)
			{
				Abort();
				return;
			}
			getData = false;
			audioClip.GetData(buffer, lastFrame * hopSize);
			waitForMainThread.Set();
		}

		private void Update()
		{
			if (getData)
			{
				GetData();
			}
			if (!initialized && lastFrame > initialLength)
			{
				initialized = true;
				if (this.Initialized != null)
				{
					this.Initialized(rhythmData);
				}
			}
		}
	}
}
