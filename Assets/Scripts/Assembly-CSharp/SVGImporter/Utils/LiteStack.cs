using System.Collections.Generic;

namespace SVGImporter.Utils
{
	public class LiteStack<T>
	{
		private int idx;

		private List<T> stack = new List<T>();

		public int Count => idx;

		public void Push(T obj)
		{
			idx++;
			if (idx > stack.Count)
			{
				stack.Add(obj);
			}
			else
			{
				stack[idx - 1] = obj;
			}
		}

		public T Pop()
		{
			T result = Peek();
			if (idx > 0)
			{
				idx--;
				stack[idx] = default(T);
			}
			return result;
		}

		public T Peek()
		{
			if (idx > 0)
			{
				return stack[idx - 1];
			}
			return default(T);
		}

		public void Clear()
		{
			stack.Clear();
			idx = 0;
		}
	}
	public class LiteStack : LiteStack<object>
	{
	}
}
