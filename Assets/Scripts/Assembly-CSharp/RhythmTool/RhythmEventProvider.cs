using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	[CreateAssetMenu(fileName = "Event Provider.asset", menuName = "RhythmTool/Event Provider")]
	public class RhythmEventProvider : RhythmTarget
	{
		private abstract class RhythmEvent : IDisposable
		{
			public abstract void Process(RhythmData rhythmData, float start, float end);

			public abstract void Dispose();
		}

		private class RhythmEvent<T> : RhythmEvent where T : IFeature
		{
			private Action<T> _action;

			private List<T> _features = new List<T>();

			private string trackName;

			public RhythmEvent(string trackName)
			{
				this.trackName = trackName;
			}

			public override void Process(RhythmData rhythmData, float start, float end)
			{
				if (_action == null)
				{
					return;
				}
				if (string.IsNullOrEmpty(trackName))
				{
					rhythmData.GetFeatures(_features, start, end);
				}
				else
				{
					rhythmData.GetFeatures(_features, start, end, trackName);
				}
				foreach (T feature in _features)
				{
					_action(feature);
				}
				_features.Clear();
			}

			public void Register(Action<T> action)
			{
				_action = Delegate.Combine(_action, action) as Action<T>;
			}

			public void Unregister(Action<T> action)
			{
				_action = Delegate.Remove(_action, action) as Action<T>;
			}

			public override void Dispose()
			{
				_action = null;
			}
		}

		[Range(-10f, 10f)]
		[Tooltip("The offset in seconds. E.g. an offset of 5 will trigger events 5 seconds in advance.")]
		public float offset;

		private Dictionary<int, RhythmEvent> _events = new Dictionary<int, RhythmEvent>();

		public override void Process(RhythmData rhythmData, float start, float end)
		{
			foreach (KeyValuePair<int, RhythmEvent> @event in _events)
			{
				@event.Value.Process(rhythmData, start + offset, end + offset);
			}
		}

		public override void Reset(RhythmData rhythmData, float time)
		{
			if (offset > 0f)
			{
				Process(rhythmData, time - offset, time);
			}
		}

		public void Register<T>(Action<T> action) where T : IFeature
		{
			Register(action, null);
		}

		public void Unregister<T>(Action<T> action) where T : IFeature
		{
			Unregister(action, null);
		}

		public void Register<T>(Action<T> action, string trackName) where T : IFeature
		{
			int hashCode = GetHashCode(typeof(T), trackName);
			RhythmEvent<T> rhythmEvent;
			if (_events.ContainsKey(hashCode))
			{
				rhythmEvent = _events[hashCode] as RhythmEvent<T>;
			}
			else
			{
				rhythmEvent = new RhythmEvent<T>(trackName);
				_events.Add(hashCode, rhythmEvent);
			}
			rhythmEvent.Register(action);
		}

		public void Unregister<T>(Action<T> action, string trackName) where T : IFeature
		{
			int hashCode = GetHashCode(typeof(T), trackName);
			if (_events.ContainsKey(hashCode))
			{
				RhythmEvent<T> rhythmEvent = _events[hashCode] as RhythmEvent<T>;
				rhythmEvent.Unregister(action);
			}
		}

		private void OnDestroy()
		{
			foreach (RhythmEvent value in _events.Values)
			{
				value.Dispose();
			}
			_events.Clear();
		}

		private static int GetHashCode(Type type, string trackName)
		{
			int num = type.GetHashCode();
			if (!string.IsNullOrEmpty(trackName))
			{
				num = CombineHashCodes(num, trackName.GetHashCode());
			}
			return num;
		}

		private static int CombineHashCodes(int h1, int h2)
		{
			return ((h1 << 5) + h1) ^ h2;
		}
	}
}
