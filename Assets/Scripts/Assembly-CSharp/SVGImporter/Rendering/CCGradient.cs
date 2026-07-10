using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SVGImporter.Rendering
{
	[Serializable]
	public class CCGradient
	{
		public const string DEFAULT_GRADIENT_HASH = "GC999FFFFFFC000FFFFFFA999999A000999";

		public CCGradientColorKey[] colorKeys;

		public CCGradientAlphaKey[] alphaKeys;

		private static string currentHash = "";

		public int index;

		[NonSerialized]
		[HideInInspector]
		public int atlasIndex;

		[NonSerialized]
		[HideInInspector]
		protected List<ISVGReference> _references;

		[NonSerialized]
		public Action<ISVGReference> onReferenceAdded;

		[NonSerialized]
		public Action<ISVGReference> onReferenceRemoved;

		public string hash
		{
			get
			{
				if (string.IsNullOrEmpty(currentHash) || currentHash != "GC999FFFFFFC000FFFFFFA999999A000999")
				{
					string text = "G";
					if (colorKeys != null && colorKeys.Length != 0)
					{
						for (int i = 0; i < colorKeys.Length; i++)
						{
							text = text + "C" + Mathf.RoundToInt(colorKeys[i].time * 999f).ToString("000") + ColorToHex(colorKeys[i].color);
						}
					}
					if (alphaKeys != null && alphaKeys.Length != 0)
					{
						for (int j = 0; j < alphaKeys.Length; j++)
						{
							text = text + "A" + Mathf.RoundToInt(alphaKeys[j].time * 999f).ToString("000") + Mathf.RoundToInt(alphaKeys[j].alpha * 999f).ToString("000");
						}
					}
					currentHash = text;
				}
				return currentHash;
			}
		}

		public List<ISVGReference> references => _references;

		public int referenceCount
		{
			get
			{
				if (_references == null)
				{
					return 0;
				}
				return _references.Count;
			}
		}

		public bool initialised
		{
			get
			{
				if (colorKeys != null && alphaKeys != null && colorKeys.Length != 0)
				{
					return alphaKeys.Length != 0;
				}
				return false;
			}
		}

		public bool AddReference(ISVGReference reference)
		{
			if (_references == null)
			{
				_references = new List<ISVGReference> { reference };
				if (onReferenceAdded != null)
				{
					onReferenceAdded(reference);
				}
				return true;
			}
			if (!_references.Contains(reference))
			{
				_references.Add(reference);
				if (onReferenceAdded != null)
				{
					onReferenceAdded(reference);
				}
				return true;
			}
			return false;
		}

		public bool RemoveReference(ISVGReference reference)
		{
			if (_references != null)
			{
				bool num = _references.Remove(reference);
				if (num && onReferenceRemoved != null)
				{
					onReferenceRemoved(reference);
				}
				return num;
			}
			return false;
		}

		public int CountReferences(ISVGReference reference)
		{
			int num = 0;
			if (_references != null)
			{
				for (int i = 0; i < _references.Count; i++)
				{
					if (_references[i] == reference)
					{
						num++;
					}
				}
			}
			return num;
		}

		public CCGradient(CCGradientColorKey[] colorKeys, CCGradientAlphaKey[] alphaKeys, bool sort = true)
		{
			SetKeys(colorKeys, alphaKeys, sort);
		}

		public void SetKeys(CCGradientColorKey[] colorKeys, CCGradientAlphaKey[] alphaKeys, bool sort = true)
		{
			this.colorKeys = (CCGradientColorKey[])colorKeys.Clone();
			this.alphaKeys = (CCGradientAlphaKey[])alphaKeys.Clone();
			if (sort)
			{
				Array.Sort(this.colorKeys, (CCGradientColorKey x, CCGradientColorKey y) => y.time.CompareTo(x.time));
				Array.Sort(this.alphaKeys, (CCGradientAlphaKey x, CCGradientAlphaKey y) => y.time.CompareTo(x.time));
			}
			if (this.alphaKeys == null || this.alphaKeys.Length == 0)
			{
				this.alphaKeys = new CCGradientAlphaKey[2]
				{
					new CCGradientAlphaKey(1f, 0f),
					new CCGradientAlphaKey(1f, 1f)
				};
			}
		}

		public Color32 Evaluate(float time)
		{
			time = Mathf.Clamp01(time);
			Color32 result;
			if (colorKeys == null || colorKeys.Length == 0)
			{
				result = new Color32(0, 0, 0, byte.MaxValue);
			}
			else if (colorKeys.Length == 1)
			{
				result = colorKeys[0].color;
			}
			else
			{
				int num = colorKeys.Length;
				float num2 = float.MaxValue;
				float num3 = 0f;
				int num4 = 0;
				for (int i = 0; i < num; i++)
				{
					num3 = Mathf.Abs(colorKeys[i].time - time);
					if (num3 < num2)
					{
						num2 = num3;
						num4 = i;
						continue;
					}
					if (!(num3 > num2))
					{
						num2 = num3;
						num4 = i;
					}
					break;
				}
				if (colorKeys[num4].time > time)
				{
					int num5 = num4;
					int num6 = Mathf.Clamp(num4 + 1, 0, num - 1);
					result = Color32.Lerp(colorKeys[num5].color, colorKeys[num6].color, Mathf.InverseLerp(colorKeys[num5].time, colorKeys[num6].time, time));
				}
				else if (colorKeys[num4].time < time)
				{
					int num7 = Mathf.Clamp(num4 - 1, 0, num - 1);
					int num8 = num4;
					result = Color32.Lerp(colorKeys[num7].color, colorKeys[num8].color, Mathf.InverseLerp(colorKeys[num7].time, colorKeys[num8].time, time));
				}
				else
				{
					result = colorKeys[num4].color;
				}
			}
			if (alphaKeys == null || alphaKeys.Length == 0)
			{
				result.a = byte.MaxValue;
			}
			else if (alphaKeys.Length == 1)
			{
				result.a = (byte)Mathf.RoundToInt(alphaKeys[0].alpha * 255f);
			}
			else
			{
				int num9 = alphaKeys.Length;
				float num10 = float.MaxValue;
				float num11 = 0f;
				int num12 = 0;
				for (int j = 0; j < num9; j++)
				{
					num11 = Mathf.Abs(alphaKeys[j].time - time);
					if (num11 < num10)
					{
						num10 = num11;
						num12 = j;
						continue;
					}
					if (!(num11 > num10))
					{
						num10 = num11;
						num12 = j;
					}
					break;
				}
				if (alphaKeys[num12].time > time)
				{
					int num13 = num12;
					int num14 = Mathf.Clamp(num12 + 1, 0, num9 - 1);
					result.a = (byte)Mathf.RoundToInt(Mathf.Lerp(alphaKeys[num13].alpha, alphaKeys[num14].alpha, Mathf.InverseLerp(alphaKeys[num13].time, alphaKeys[num14].time, time)) * 255f);
				}
				else if (alphaKeys[num12].time < time)
				{
					int num15 = Mathf.Clamp(num12 - 1, 0, num9 - 1);
					int num16 = num12;
					result.a = (byte)Mathf.RoundToInt(Mathf.Lerp(alphaKeys[num15].alpha, alphaKeys[num16].alpha, Mathf.InverseLerp(alphaKeys[num15].time, alphaKeys[num16].time, time)) * 255f);
				}
				else
				{
					result.a = (byte)Mathf.RoundToInt(alphaKeys[num12].alpha * 255f);
				}
			}
			return result;
		}

		public Color32 ApproximateColor(int samples)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			float num5 = samples - 1;
			for (float num6 = 0f; num6 < (float)samples; num6 += 1f)
			{
				Color32 color = Evaluate(num6 / num5);
				num += color.r;
				num2 += color.g;
				num3 += color.b;
				num4 += color.a;
			}
			num = Mathf.Clamp(Mathf.RoundToInt((float)num / (float)samples), 0, 255);
			num2 = Mathf.Clamp(Mathf.RoundToInt((float)num2 / (float)samples), 0, 255);
			num3 = Mathf.Clamp(Mathf.RoundToInt((float)num3 / (float)samples), 0, 255);
			num4 = Mathf.Clamp(Mathf.RoundToInt((float)num4 / (float)samples), 0, 255);
			return new Color32((byte)num, (byte)num2, (byte)num3, (byte)num4);
		}

		public CCGradient Clone()
		{
			if (colorKeys == null || alphaKeys == null)
			{
				return null;
			}
			return new CCGradient((CCGradientColorKey[])colorKeys.Clone(), (CCGradientAlphaKey[])alphaKeys.Clone(), sort: false)
			{
				index = index,
				atlasIndex = atlasIndex
			};
		}

		public override string ToString()
		{
			string text = string.Format("[CCGradient: initialised={0}, index={1}, atlasIndex={2}]", hash, initialised, index, atlasIndex);
			if (colorKeys != null && colorKeys.Length != 0)
			{
				text += "\nColorKeys:\n";
				for (int i = 0; i < colorKeys.Length; i++)
				{
					text = text + colorKeys[i].ToString() + "\n";
				}
			}
			if (alphaKeys != null && alphaKeys.Length != 0)
			{
				text += "\nAlphaKeys:\n";
				for (int j = 0; j < alphaKeys.Length; j++)
				{
					text = text + alphaKeys[j].ToString() + "\n";
				}
			}
			return text;
		}

		public static string ColorToHex(Color32 color)
		{
			return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		}

		public static Color HexToColor(string hex)
		{
			byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, null, out var result);
			byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, null, out var result2);
			byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, null, out var result3);
			return new Color32(result, result2, result3, byte.MaxValue);
		}
	}
}
