using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	public abstract class Track : ScriptableObject
	{
		[SerializeField]
		protected string _name;

		public new string name => _name;
	}
	public abstract class Track<T> : Track where T : IFeature
	{
		[SerializeField]
		private List<T> _features = new List<T>();

		[NonSerialized]
		private List<int> cachedTimestamps = new List<int>();

		private Dictionary<int, int> cachedIndices = new Dictionary<int, int>();

		private static Type concreteType;

		public T this[int index] => _features[index];

		public int count => _features.Count;

		static Track()
		{
			Type[] types = typeof(T).Assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.IsSubclassOf(typeof(Track<T>)) && !type.IsAbstract)
				{
					concreteType = type;
					break;
				}
			}
		}

		public void Add(T feature)
		{
			if (_features.Count == 0 || feature.timestamp > _features[_features.Count - 1].timestamp)
			{
				_features.Add(feature);
				return;
			}
			int index = GetIndex(feature.timestamp);
			_features.Insert(index, feature);
			ClearCache(feature.timestamp);
		}

		public void Remove(T feature)
		{
			_features.Remove(feature);
		}

		public void Sort()
		{
			_features.Sort((T a, T b) => a.timestamp.CompareTo(b.timestamp));
			ClearCache(0f);
		}

		public void GetFeatures(List<T> features, float start, float end)
		{
			int index = GetIndex(start);
			int index2 = GetIndex(end);
			for (int i = index; i < index2; i++)
			{
				features.Add(_features[i]);
			}
		}

		public void GetIntersectingFeatures(List<T> features, float start, float end)
		{
			int intersectingIndex = GetIntersectingIndex(start);
			int index = GetIndex(end);
			for (int i = intersectingIndex; i < index; i++)
			{
				T item = _features[i];
				if (item.timestamp + item.length > start)
				{
					features.Add(item);
				}
			}
		}

		public int GetIndex(float timestamp)
		{
			int num = BinarySearch(timestamp);
			if (num < 0)
			{
				num = ~num;
			}
			while (num > 1 && _features[num - 1].timestamp >= timestamp)
			{
				num--;
			}
			return num;
		}

		private int BinarySearch(float timestamp)
		{
			int num = 0;
			int num2 = _features.Count - 1;
			while (num <= num2)
			{
				int num3 = num + (num2 - num >> 1);
				int num4 = _features[num3].timestamp.CompareTo(timestamp);
				if (num4 == 0)
				{
					return num3;
				}
				if (num4 < 0)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3 - 1;
				}
			}
			return ~num;
		}

		private int GetIntersectingIndex(float timestamp)
		{
			int num = Mathf.RoundToInt(timestamp / 5f) * 5;
			int cacheIndex = GetCacheIndex(num);
			int num2 = 0;
			if (cacheIndex > 0)
			{
				num2 = cachedIndices[cachedTimestamps[cacheIndex - 1]];
			}
			for (int i = num2; i < _features.Count; i++)
			{
				T val = _features[i];
				if (val.timestamp + val.length > timestamp)
				{
					if (!cachedIndices.ContainsKey(num))
					{
						cachedTimestamps.Insert(cacheIndex, num);
						cachedIndices.Add(num, i);
					}
					return i;
				}
			}
			return _features.Count;
		}

		private int GetCacheIndex(int timestamp)
		{
			int num = cachedTimestamps.BinarySearch(timestamp);
			if (num < 0)
			{
				num = Mathf.Max(~num - 1, 0);
			}
			return num;
		}

		private void ClearCache(float timestamp)
		{
			int cacheIndex = GetCacheIndex((int)timestamp);
			for (int i = cacheIndex; i < cachedTimestamps.Count; i++)
			{
				cachedIndices.Remove(cachedTimestamps[i]);
			}
			cachedTimestamps.RemoveRange(cacheIndex, cachedTimestamps.Count - cacheIndex);
		}

		public static Track<T> Create(string name)
		{
			if (concreteType == null)
			{
				Debug.LogWarning("No Track found for " + typeof(T).Name);
				return null;
			}
			Track<T> obj = ScriptableObject.CreateInstance(concreteType) as Track<T>;
			((UnityEngine.Object)obj).name = name;
			obj._name = name;
			return obj;
		}
	}
}
