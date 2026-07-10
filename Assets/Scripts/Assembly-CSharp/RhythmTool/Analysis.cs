using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	public abstract class Analysis : MonoBehaviour
	{
		public Track track { get; protected set; }

		public int sampleRate { get; private set; }

		public int frameSize { get; private set; }

		public int hopSize { get; private set; }

		public new abstract string name { get; }

		protected int frameIndex { get; private set; }

		public virtual void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			this.sampleRate = sampleRate;
			this.frameSize = frameSize;
			this.hopSize = hopSize;
		}

		public virtual void Process(float[] samples, float[] magnitude, int frameIndex)
		{
			this.frameIndex = frameIndex;
		}

		protected float FrameIndexToSeconds(float frameIndex)
		{
			return frameIndex / ((float)sampleRate / (float)hopSize);
		}
	}
	[ExecuteInEditMode]
	public abstract class Analysis<T> : Analysis where T : IFeature
	{
		private Queue<T> toAdd = new Queue<T>();

		private Queue<T> toRemove = new Queue<T>();

		public new Track<T> track { get; private set; }

		public override void Initialize(int sampleRate, int frameSize, int hopSize)
		{
			base.Initialize(sampleRate, frameSize, hopSize);
			track = Track<T>.Create(name);
			lock (toAdd)
			{
				toAdd.Clear();
			}
			lock (toRemove)
			{
				toRemove.Clear();
			}
			base.track = track;
		}

		protected void AddFeature(T feature)
		{
			lock (toAdd)
			{
				toAdd.Enqueue(feature);
			}
		}

		protected void RemoveFeature(T feature)
		{
			lock (toRemove)
			{
				toRemove.Enqueue(feature);
			}
		}

		private void Update()
		{
			lock (toAdd)
			{
				while (toAdd.Count > 0)
				{
					track.Add(toAdd.Dequeue());
				}
			}
			lock (toRemove)
			{
				while (toRemove.Count > 0)
				{
					track.Remove(toRemove.Dequeue());
				}
			}
		}
	}
}
