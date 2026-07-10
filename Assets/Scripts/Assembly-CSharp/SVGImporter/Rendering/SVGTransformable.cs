using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGTransformable
	{
		private SVGTransformList _inheritTransformList;

		private SVGTransformList _currentTransformList;

		private SVGTransformList _summaryTransformList;

		public SVGTransformList inheritTransformList
		{
			get
			{
				return _inheritTransformList;
			}
			set
			{
				int num = 0;
				if (_inheritTransformList != null)
				{
					num += _inheritTransformList.Count;
				}
				if (_currentTransformList != null)
				{
					num += _currentTransformList.Count;
				}
				_inheritTransformList = value;
				_summaryTransformList = new SVGTransformList(num);
				if (_inheritTransformList != null)
				{
					_summaryTransformList.AppendItems(_inheritTransformList);
				}
				if (_currentTransformList != null)
				{
					_summaryTransformList.AppendItems(_currentTransformList);
				}
			}
		}

		public SVGTransformList currentTransformList
		{
			get
			{
				return _currentTransformList;
			}
			set
			{
				_currentTransformList = value;
				int num = 0;
				if (_inheritTransformList != null)
				{
					num += _inheritTransformList.Count;
				}
				if (_currentTransformList != null)
				{
					num += _currentTransformList.Count;
				}
				_summaryTransformList = new SVGTransformList(num);
				if (_inheritTransformList != null)
				{
					_summaryTransformList.AppendItems(_inheritTransformList);
				}
				if (_currentTransformList != null)
				{
					_summaryTransformList.AppendItems(_currentTransformList);
				}
			}
		}

		public SVGTransformList summaryTransformList => _summaryTransformList;

		public float transformAngle
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < _summaryTransformList.Count; i++)
				{
					SVGTransform sVGTransform = _summaryTransformList[i];
					if (sVGTransform.type == SVGTransformMode.Rotate)
					{
						num += sVGTransform.angle;
					}
				}
				return num;
			}
		}

		public SVGMatrix transformMatrix => summaryTransformList.Consolidate().matrix;

		public SVGTransformable(SVGTransformList transformList)
		{
			inheritTransformList = transformList;
		}

		public static SVGMatrix GetRootViewBoxTransform(AttributeList attributeList, ref Rect viewport)
		{
			SVGMatrix identity = SVGMatrix.identity;
			string value = attributeList.GetValue("x");
			string value2 = attributeList.GetValue("y");
			string value3 = attributeList.GetValue("width");
			string value4 = attributeList.GetValue("height");
			SVGLength sVGLength = new SVGLength(SVGLengthType.PX, 0f);
			SVGLength sVGLength2 = new SVGLength(SVGLengthType.PX, 0f);
			SVGLength sVGLength3 = new SVGLength(SVGLengthType.PX, 1f);
			SVGLength sVGLength4 = new SVGLength(SVGLengthType.PX, 1f);
			if (!string.IsNullOrEmpty(value))
			{
				sVGLength = new SVGLength(value);
			}
			if (!string.IsNullOrEmpty(value2))
			{
				sVGLength2 = new SVGLength(value2);
			}
			if (!string.IsNullOrEmpty(value3))
			{
				sVGLength3 = new SVGLength(value3);
			}
			if (!string.IsNullOrEmpty(value4))
			{
				sVGLength4 = new SVGLength(value4);
			}
			string value5 = attributeList.GetValue("viewBox");
			if (!string.IsNullOrEmpty(value5))
			{
				string[] array = SVGStringExtractor.ExtractTransformValue(value5);
				if (array.Length != 0 && string.IsNullOrEmpty(value))
				{
					sVGLength = new SVGLength(array[0]);
				}
				if (array.Length > 1 && string.IsNullOrEmpty(value2))
				{
					sVGLength2 = new SVGLength(array[1]);
				}
				if (array.Length > 2 && string.IsNullOrEmpty(value3))
				{
					sVGLength3 = new SVGLength(array[2]);
				}
				if (array.Length > 3 && string.IsNullOrEmpty(value4))
				{
					sVGLength4 = new SVGLength(array[3]);
				}
				viewport = new Rect(sVGLength.value, sVGLength2.value, sVGLength3.value, sVGLength4.value);
				if (string.IsNullOrEmpty(value))
				{
					viewport.x = sVGLength.value;
				}
				if (string.IsNullOrEmpty(value2))
				{
					viewport.y = sVGLength2.value;
				}
				if (string.IsNullOrEmpty(value3))
				{
					viewport.width = sVGLength3.value;
				}
				if (string.IsNullOrEmpty(value4))
				{
					viewport.height = sVGLength4.value;
					return identity;
				}
			}
			else
			{
				viewport = new Rect(sVGLength.value, sVGLength2.value, sVGLength3.value, sVGLength4.value);
			}
			return identity;
		}

		public static SVGMatrix GetViewBoxTransform(AttributeList attributeList, ref Rect viewport, bool negotiate = false)
		{
			SVGMatrix result = SVGMatrix.identity;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			string value = attributeList.GetValue("preserveAspectRatio");
			string value2 = attributeList.GetValue("viewBox");
			if (!string.IsNullOrEmpty(value2))
			{
				string[] array = SVGStringExtractor.ExtractTransformValue(value2);
				if (array.Length == 4)
				{
					Rect content = new Rect(new SVGLength(array[0]).value, new SVGLength(array[1]).value, new SVGLength(array[2]).value, new SVGLength(array[3]).value);
					SVGViewport.Align viewportAlign = SVGViewport.Align.xMidYMid;
					SVGViewport.MeetOrSlice viewportMeetOrSlice = SVGViewport.MeetOrSlice.Meet;
					if (!string.IsNullOrEmpty(value))
					{
						string[] inputStrings = SVGStringExtractor.ExtractStringArray(value);
						viewportAlign = SVGViewport.GetAlignFromStrings(inputStrings);
						viewportMeetOrSlice = SVGViewport.GetMeetOrSliceFromStrings(inputStrings);
					}
					Rect rect = viewport;
					viewport = SVGViewport.GetViewport(viewport, content, viewportAlign, viewportMeetOrSlice);
					float scaleFactorX = 0f;
					float scaleFactorY = 0f;
					if (rect.size.x != 0f)
					{
						scaleFactorX = viewport.size.x / rect.size.x;
					}
					if (rect.size.y != 0f)
					{
						scaleFactorY = viewport.size.y / rect.size.y;
					}
					result.Scale(scaleFactorX, scaleFactorY);
					result = result.Translate(viewport.x - rect.x, viewport.y - rect.y);
				}
			}
			else if (negotiate)
			{
				string value3 = attributeList.GetValue("x");
				string value4 = attributeList.GetValue("y");
				string value5 = attributeList.GetValue("width");
				string value6 = attributeList.GetValue("height");
				SVGLength sVGLength = new SVGLength(SVGLengthType.PX, 0f);
				SVGLength sVGLength2 = new SVGLength(SVGLengthType.PX, 0f);
				SVGLength sVGLength3 = new SVGLength(SVGLengthType.PX, 1f);
				SVGLength sVGLength4 = new SVGLength(SVGLengthType.PX, 1f);
				if (!string.IsNullOrEmpty(value3))
				{
					sVGLength = new SVGLength(value3);
				}
				if (!string.IsNullOrEmpty(value4))
				{
					sVGLength2 = new SVGLength(value4);
				}
				if (!string.IsNullOrEmpty(value5))
				{
					sVGLength3 = new SVGLength(value5);
				}
				if (!string.IsNullOrEmpty(value6))
				{
					sVGLength4 = new SVGLength(value6);
				}
				num = sVGLength.value;
				num2 = sVGLength2.value;
				num3 = sVGLength3.value;
				num4 = sVGLength4.value;
				float scaleFactorX2 = 1f;
				if (num3 != 0f)
				{
					scaleFactorX2 = sVGLength3.value / num3;
				}
				float scaleFactorY2 = 1f;
				if (num4 != 0f)
				{
					scaleFactorY2 = sVGLength4.value / num4;
				}
				result = result.Scale(scaleFactorX2, scaleFactorY2).Translate(num, num2);
				viewport = new Rect(num, num2, num3, num4);
			}
			return result;
		}
	}
}
