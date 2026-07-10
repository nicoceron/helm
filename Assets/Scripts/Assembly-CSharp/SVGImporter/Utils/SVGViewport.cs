using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGViewport
	{
		public enum Align
		{
			None = 0,
			xMinYMin = 1,
			xMidYMin = 2,
			xMaxYMin = 3,
			xMinYMid = 4,
			xMidYMid = 5,
			xMaxYMid = 6,
			xMinYMax = 7,
			xMidYMax = 8,
			xMaxYMax = 9
		}

		public enum MeetOrSlice
		{
			Meet = 0,
			Slice = 1
		}

		private const string None = "none";

		private const string xMinYMin = "xminymin";

		private const string xMidYMin = "xmidymin";

		private const string xMaxYMin = "xmaxymin";

		private const string xMinYMid = "xminymid";

		private const string xMidYMid = "xmidymid";

		private const string xMaxYMid = "xmaxymid";

		private const string xMinYMax = "xminymax";

		private const string xMidYMax = "xmidymax";

		private const string xMaxYMax = "xmaxymax";

		private const string Meet = "meet";

		private const string Slice = "slice";

		public static MeetOrSlice GetMeetOrSliceFromStrings(string[] inputStrings)
		{
			if (inputStrings == null || inputStrings.Length == 0)
			{
				return MeetOrSlice.Meet;
			}
			for (int i = 0; i < inputStrings.Length; i++)
			{
				if (!string.IsNullOrEmpty(inputStrings[i]))
				{
					switch (inputStrings[i].ToLower())
					{
					case "meet":
						return MeetOrSlice.Meet;
					case "slice":
						return MeetOrSlice.Slice;
					}
				}
			}
			return MeetOrSlice.Meet;
		}

		public static MeetOrSlice GetMeetOrSliceFromString(string inputText)
		{
			if (string.IsNullOrEmpty(inputText))
			{
				return MeetOrSlice.Meet;
			}
			return inputText.ToLower() switch
			{
				"meet" => MeetOrSlice.Meet, 
				"slice" => MeetOrSlice.Slice, 
				_ => MeetOrSlice.Meet, 
			};
		}

		public static string GetStringFromMeetOrSlice(MeetOrSlice meetOrSlice)
		{
			return meetOrSlice switch
			{
				MeetOrSlice.Meet => "meet", 
				MeetOrSlice.Slice => "slice", 
				_ => "meet", 
			};
		}

		public static Align GetAlignFromStrings(string[] inputStrings)
		{
			if (inputStrings == null || inputStrings.Length == 0)
			{
				return Align.xMidYMid;
			}
			for (int i = 0; i < inputStrings.Length; i++)
			{
				if (!string.IsNullOrEmpty(inputStrings[i]))
				{
					switch (inputStrings[i].ToLower())
					{
					case "none":
						return Align.None;
					case "xminymin":
						return Align.xMinYMin;
					case "xmidymin":
						return Align.xMidYMin;
					case "xmaxymin":
						return Align.xMaxYMin;
					case "xminymid":
						return Align.xMinYMid;
					case "xmidymid":
						return Align.xMidYMid;
					case "xmaxymid":
						return Align.xMaxYMid;
					case "xminymax":
						return Align.xMinYMax;
					case "xmidymax":
						return Align.xMidYMax;
					case "xmaxymax":
						return Align.xMaxYMax;
					}
				}
			}
			return Align.xMidYMid;
		}

		public static Align GetAlignFromString(string inputText)
		{
			if (string.IsNullOrEmpty(inputText))
			{
				return Align.xMidYMid;
			}
			return inputText.ToLower() switch
			{
				"none" => Align.None, 
				"xminymin" => Align.xMinYMin, 
				"xmidymin" => Align.xMidYMin, 
				"xmaxymin" => Align.xMaxYMin, 
				"xminymid" => Align.xMinYMid, 
				"xmidymid" => Align.xMidYMid, 
				"xmaxymid" => Align.xMaxYMid, 
				"xminymax" => Align.xMinYMax, 
				"xmidymax" => Align.xMidYMax, 
				"xmaxymax" => Align.xMaxYMax, 
				_ => Align.xMidYMid, 
			};
		}

		public static string GetStringFromAlign(Align align)
		{
			return align switch
			{
				Align.None => "none", 
				Align.xMinYMin => "xminymin", 
				Align.xMidYMin => "xmidymin", 
				Align.xMaxYMin => "xmaxymin", 
				Align.xMinYMid => "xminymid", 
				Align.xMidYMid => "xmidymid", 
				Align.xMaxYMid => "xmaxymid", 
				Align.xMinYMax => "xminymax", 
				Align.xMidYMax => "xmidymax", 
				Align.xMaxYMax => "xmaxymax", 
				_ => null, 
			};
		}

		public static Rect GetViewport(Rect viewport, Rect content, Align viewportAlign = Align.xMidYMid, MeetOrSlice viewportMeetOrSlice = MeetOrSlice.Meet)
		{
			viewport.x -= content.x;
			viewport.y -= content.y;
			if (viewportAlign != Align.None)
			{
				Vector2 vector = new Vector2(viewport.width / content.width, viewport.height / content.height);
				Vector2 size = default(Vector2);
				switch (viewportMeetOrSlice)
				{
				case MeetOrSlice.Meet:
				{
					float num = Mathf.Min(vector.x, vector.y);
					size.x = content.width * num;
					size.y = content.height * num;
					Vector2 vector2 = Getalign(viewport, size, viewportAlign);
					return new Rect(vector2.x, vector2.y, size.x, size.y);
				}
				case MeetOrSlice.Slice:
				{
					float num = Mathf.Max(vector.x, vector.y);
					size.x = content.width * num;
					size.y = content.height * num;
					Vector2 vector2 = Getalign(viewport, size, viewportAlign);
					return new Rect(vector2.x, vector2.y, size.x, size.y);
				}
				}
			}
			return viewport;
		}

		protected static Vector2 Getalign(Rect viewport, Vector2 size, Align align)
		{
			return align switch
			{
				Align.xMinYMin => new Vector2(viewport.x, viewport.y), 
				Align.xMidYMin => new Vector2(viewport.x + (viewport.width - size.x) * 0.5f, viewport.y), 
				Align.xMaxYMin => new Vector2(viewport.x + (viewport.width - size.x), viewport.y), 
				Align.xMinYMid => new Vector2(viewport.x, viewport.y + (viewport.height - size.y) * 0.5f), 
				Align.xMidYMid => new Vector2(viewport.x + (viewport.width - size.x) * 0.5f, viewport.y + (viewport.height - size.y) * 0.5f), 
				Align.xMaxYMid => new Vector2(viewport.x + (viewport.width - size.x), viewport.y + (viewport.height - size.y) * 0.5f), 
				Align.xMinYMax => new Vector2(viewport.x, viewport.y + (viewport.height - size.y)), 
				Align.xMidYMax => new Vector2(viewport.x + (viewport.width - size.x) * 0.5f, viewport.y + (viewport.height - size.y)), 
				Align.xMaxYMax => new Vector2(viewport.x + (viewport.width - size.x), viewport.y + (viewport.height - size.y)), 
				_ => new Vector2(viewport.x, viewport.y), 
			};
		}
	}
}
