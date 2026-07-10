using System;
using System.Collections.Generic;
using UnityEngine;

namespace SVGImporter
{
	[Serializable]
	public class LayerSelection
	{
		[HideInInspector]
		[SerializeField]
		protected List<int> _layers;

		protected HashSet<int> _cache;

		public List<int> layers
		{
			get
			{
				if (_layers == null)
				{
					_layers = new List<int>();
				}
				return _layers;
			}
		}

		public HashSet<int> cache
		{
			get
			{
				UpdateCache();
				return _cache;
			}
		}

		public void Add(int index)
		{
			if (!Contains(index))
			{
				layers.Add(index);
				cache.Add(index);
			}
		}

		public void Remove(int index)
		{
			if (Contains(index))
			{
				layers.Remove(index);
				cache.Remove(index);
			}
		}

		public bool Contains(int index)
		{
			return cache.Contains(index);
		}

		public void UpdateCache()
		{
			if (_cache == null)
			{
				_cache = new HashSet<int>();
			}
			for (int i = 0; i < layers.Count; i++)
			{
				_cache.Add(layers[i]);
			}
		}

		public void ClearCache()
		{
			if (_cache != null)
			{
				_cache.Clear();
			}
		}

		public void Clear()
		{
			ClearCache();
			if (_layers != null)
			{
				_layers.Clear();
			}
		}
	}
}
