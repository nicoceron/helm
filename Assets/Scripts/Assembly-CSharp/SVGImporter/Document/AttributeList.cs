using System.Collections.Generic;
using System.Text;

namespace SVGImporter.Document
{
	public struct AttributeList
	{
		private Dictionary<string, string> attrs;

		public int Count => attrs.Count;

		public Dictionary<string, string> Get => attrs;

		public Dictionary<string, string> Set
		{
			set
			{
				attrs = value;
			}
		}

		public AttributeList(AttributeList a)
		{
			if (a.attrs != null)
			{
				attrs = new Dictionary<string, string>(a.attrs);
			}
			else
			{
				attrs = null;
			}
		}

		public void Clear()
		{
			if (attrs != null)
			{
				attrs.Clear();
			}
		}

		public void Add(string name, string value)
		{
			if (attrs == null)
			{
				attrs = new Dictionary<string, string>();
			}
			attrs[name] = value;
		}

		public string GetValue(string name)
		{
			if (attrs != null && attrs.TryGetValue(name, out var value))
			{
				return value;
			}
			return "";
		}

		public new string ToString()
		{
			if (attrs == null)
			{
				return "null";
			}
			bool flag = true;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> attr in attrs)
			{
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(attr.Key).Append("=").Append(attr.Value);
				flag = false;
			}
			return stringBuilder.ToString();
		}
	}
}
