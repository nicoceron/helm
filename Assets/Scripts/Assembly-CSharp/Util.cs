using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Util
{
	public static Vector2 nullpos = new Vector2(-1f, -1f);

	public static void Write(string text)
	{
	}

	public static string GetTextFile(string source)
	{
		return GetDecode(((TextAsset)Resources.Load(source, typeof(TextAsset))).text);
	}

	public static string GetDecode(string txt)
	{
		byte[] bytes = Convert.FromBase64String(txt);
		return Encoding.UTF8.GetString(bytes);
	}

	public static int[] RandIntArray(int[] arr)
	{
		for (int num = arr.Length - 1; num > 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			int num3 = arr[num];
			arr[num] = arr[num2];
			arr[num2] = num3;
		}
		return arr;
	}

	public static float RandSign()
	{
		return (!((float)UnityEngine.Random.Range(0, 1) > 0.5f)) ? 1 : (-1);
	}

	public static float Rand(float min = 0f, float max = 1f)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static int RandInt(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static T PickRandom<T>(this List<T> list) where T : new()
	{
		if (list.Count == 0)
		{
			return new T();
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		return list[RandInt(0, list.Count)];
	}

	public static void Shuffle<T>(this List<T> list, string name)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list.Swap(i, GetInt(name + i, i, list.Count - 1));
		}
	}

	public static void Shuffle<T>(this T[] list)
	{
		List<T> list2 = new List<T>(list);
		list2.Shuffle();
		list2.CopyTo(list);
	}

	public static void Shuffle<T>(this List<T> list)
	{
		System.Random random = new System.Random();
		for (int i = 0; i < list.Count; i++)
		{
			list.Swap(i, random.Next(i, list.Count - 1));
		}
	}

	public static void Swap<T>(this List<T> list, int i, int j)
	{
		T value = list[i];
		list[i] = list[j];
		list[j] = value;
	}

	public static float GetValueX(string name)
	{
		return GetSeed(name, 231.1232f);
	}

	public static float GetValueY(string name)
	{
		return GetSeed(name, 145.32176f);
	}

	private static float GetSeed(string name, float constant)
	{
		name += name;
		float num = constant * (float)Math.PI;
		string text = name;
		foreach (char value in text)
		{
			num += (float)Convert.ToInt32(value) * 0.0123f;
		}
		return Mathf.PingPong(num * constant, 1f);
	}

	public static float GetSqr(string name)
	{
		return Mathf.Pow(GetFloat(name), 2f);
	}

	public static float GetFloat(string name, float min = 0f, float max = 1f)
	{
		return min + GetValuePP(GetValueX(name), GetValueY(name)) * (max - min);
	}

	private static float GetValueSerie(float posx, float posy)
	{
		return GetValuePP(posx, posy);
	}

	private static float GetValuePP(float posx, float posy)
	{
		return Mathf.PingPong(Mathf.PerlinNoise(posx, posy) * 12331.342f, 1f);
	}

	public static int GetInt(string name, int min = 0, int max = 1)
	{
		return Mathf.FloorToInt(GetFloat(name, min, (float)max - 0.5f));
	}

	public static int[] ShuffleToArray(int numbers, int lowestInt = 0)
	{
		int[] array = new int[numbers];
		for (int i = 0; i < numbers; i++)
		{
			array[i] = i + lowestInt;
		}
		return ShuffleIntArray(array);
	}

	public static int[] ShuffleIntArray(int[] array)
	{
		for (int num = array.Length; num > 1; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			int num3 = array[num2];
			array[num2] = array[num - 1];
			array[num - 1] = num3;
		}
		return array;
	}
}
