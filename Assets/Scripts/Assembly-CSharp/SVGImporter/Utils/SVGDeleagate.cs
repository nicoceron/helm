using System;

namespace SVGImporter.Utils
{
	public static class SVGDeleagate
	{
		public static bool IsRegistered(Delegate source, Action compare)
		{
			if ((object)source == null || compare == null)
			{
				return false;
			}
			Delegate[] invocationList = source.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Equals(compare))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsRegistered<T>(Delegate source, Action<T> compare)
		{
			if ((object)source == null || compare == null)
			{
				return false;
			}
			Delegate[] invocationList = source.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Equals(compare))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsRegistered<T1, T2>(Delegate source, Action<T1, T2> compare)
		{
			if ((object)source == null || compare == null)
			{
				return false;
			}
			Delegate[] invocationList = source.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Equals(compare))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsRegistered<T1, T2, T3>(Delegate source, Action<T1, T2, T3> compare)
		{
			if ((object)source == null || compare == null)
			{
				return false;
			}
			Delegate[] invocationList = source.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Equals(compare))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsRegistered<T1, T2, T3, T4>(Delegate source, Action<T1, T2, T3, T4> compare)
		{
			if ((object)source == null || compare == null)
			{
				return false;
			}
			Delegate[] invocationList = source.GetInvocationList();
			if (invocationList == null || invocationList.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Equals(compare))
				{
					return true;
				}
			}
			return false;
		}
	}
}
