using System.Collections.Generic;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	public class SVGAtlasData
	{
		public CCGradient[] gradients;

		public Dictionary<string, CCGradient> gradientCache;

		public void Init(int length)
		{
			gradients = new CCGradient[length];
			gradientCache = new Dictionary<string, CCGradient>();
		}

		public void ClearGradientCache()
		{
			if (gradientCache != null)
			{
				gradientCache.Clear();
			}
			gradientCache = null;
		}

		public void InitGradientCache()
		{
			if (gradientCache != null)
			{
				return;
			}
			gradientCache = new Dictionary<string, CCGradient>();
			int num = gradients.Length;
			for (int i = 0; i < num; i++)
			{
				if (gradients[i] != null)
				{
					string hash = gradients[i].hash;
					if (!gradientCache.ContainsKey(hash))
					{
						gradientCache.Add(hash, gradients[i]);
					}
				}
			}
		}

		public void RebuildGradientCache()
		{
			ClearGradientCache();
			InitGradientCache();
		}

		public static CCGradient GetDefaultGradient()
		{
			CCGradientColorKey[] colorKeys = new CCGradientColorKey[2]
			{
				new CCGradientColorKey(Color.white, 0f),
				new CCGradientColorKey(Color.white, 1f)
			};
			CCGradientAlphaKey[] alphaKeys = new CCGradientAlphaKey[2]
			{
				new CCGradientAlphaKey(1f, 0f),
				new CCGradientAlphaKey(1f, 1f)
			};
			return new CCGradient(colorKeys, alphaKeys);
		}

		public CCGradient AddGradient(CCGradient gradient)
		{
			bool gradientExist;
			return AddGradient(gradient, out gradientExist);
		}

		public CCGradient AddGradient(CCGradient gradient, out bool gradientExist)
		{
			gradientExist = false;
			if (gradient == null || !gradient.initialised)
			{
				return null;
			}
			if (gradientCache == null || gradientCache.Count == 0)
			{
				RebuildGradientCache();
			}
			string hash = gradient.hash;
			if (gradientCache.ContainsKey(hash))
			{
				gradient = gradientCache[hash];
				gradientExist = true;
			}
			else
			{
				int num = gradients.Length;
				for (int i = 0; i < num; i++)
				{
					if (gradients[i] == null)
					{
						gradient.index = i;
						gradients[i] = gradient;
						gradientCache.Add(hash, gradient);
						break;
					}
				}
				gradientExist = false;
			}
			return gradient;
		}

		public bool RemoveGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised)
			{
				return false;
			}
			if (gradientCache == null || gradientCache.Count == 0)
			{
				return false;
			}
			string hash = gradient.hash;
			if (gradientCache.ContainsKey(hash))
			{
				gradientCache.Remove(hash);
				gradients[gradient.index] = null;
				return true;
			}
			return false;
		}

		public CCGradient GetGradient(int index)
		{
			index = Mathf.Clamp(index, 0, gradients.Length - 1);
			return gradients[index];
		}

		public SVGFill GetGradient(SVGFill gradient)
		{
			gradient.gradientColors = GetGradient(gradient.gradientColors);
			return gradient;
		}

		public CCGradient GetGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised || gradientCache == null)
			{
				return null;
			}
			string hash = gradient.hash;
			if (gradientCache.ContainsKey(hash))
			{
				return gradientCache[hash];
			}
			return null;
		}

		public bool HasGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised || gradientCache == null)
			{
				return false;
			}
			string hash = gradient.hash;
			if (gradientCache.ContainsKey(hash))
			{
				gradient = gradientCache[hash];
				return true;
			}
			return false;
		}

		public void Clear()
		{
			if (gradients != null)
			{
				gradients = null;
			}
			if (gradientCache != null)
			{
				gradientCache.Clear();
				gradientCache = null;
			}
		}
	}
}
