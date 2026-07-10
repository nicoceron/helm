using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGLinearGradientBrush
	{
		private SVGLinearGradientElement _linearGradElement;

		private SVGLength _x1;

		private SVGLength _y1;

		private SVGLength _x2;

		private SVGLength _y2;

		private List<Color> _stopColorList;

		private List<float> _stopOffsetList;

		protected bool _alphaBlended;

		protected SVGFill _fill;

		protected SVGMatrix _gradientTransform;

		protected SVGMatrix _transform;

		protected Rect _viewport;

		public bool alphaBlended => _alphaBlended;

		public SVGFill fill => _fill;

		public SVGLinearGradientBrush(SVGLinearGradientElement linearGradElement)
		{
			_transform = SVGMatrix.identity;
			_linearGradElement = linearGradElement;
			Initialize();
			CreateFill();
		}

		public SVGLinearGradientBrush(SVGLinearGradientElement linearGradElement, Rect bounds, SVGMatrix matrix, Rect viewport)
		{
			_viewport = viewport;
			_transform = matrix;
			_linearGradElement = linearGradElement;
			Initialize();
			CreateFill(bounds);
		}

		private void Initialize()
		{
			_x1 = _linearGradElement.x1;
			_y1 = _linearGradElement.y1;
			_x2 = _linearGradElement.x2;
			_y2 = _linearGradElement.y2;
			_stopColorList = new List<Color>();
			_stopOffsetList = new List<float>();
			GetStopList();
		}

		private void CreateFill()
		{
			if (_alphaBlended)
			{
				_fill = new SVGFill(Color.white, FILL_BLEND.ALPHA_BLENDED, FILL_TYPE.GRADIENT, GRADIENT_TYPE.LINEAR);
			}
			else
			{
				_fill = new SVGFill(Color.white, FILL_BLEND.OPAQUE, FILL_TYPE.GRADIENT, GRADIENT_TYPE.LINEAR);
			}
			_gradientTransform = _linearGradElement.gradientTransform.Consolidate().matrix;
			_fill.gradientColors = SVGAssetImport.atlasData.AddGradient(ParseGradientColors());
			_fill.viewport = _viewport;
		}

		private void CreateFill(Rect bounds)
		{
			CreateFill();
			_fill.transform = SVGSimplePath.GetFillTransform(_fill, bounds, new SVGLength[2] { _x1, _y1 }, new SVGLength[2] { _x2, _y2 }, _transform, _gradientTransform);
		}

		public CCGradient ParseGradientColors()
		{
			int count = _stopColorList.Count;
			CCGradientColorKey[] array = new CCGradientColorKey[count];
			CCGradientAlphaKey[] array2 = new CCGradientAlphaKey[count];
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				num = Mathf.Clamp01(_stopOffsetList[i] * 0.01f);
				array[i] = new CCGradientColorKey(_stopColorList[i], num);
				array2[i] = new CCGradientAlphaKey(_stopColorList[i].a, num);
			}
			return new CCGradient(array, array2);
		}

		private void GetStopList()
		{
			List<SVGStopElement> stopList = _linearGradElement.stopList;
			int count = stopList.Count;
			if (count == 0)
			{
				return;
			}
			_stopColorList.Add(GetColor(stopList[0].stopColor));
			_stopOffsetList.Add(0f);
			for (int i = 0; i < count; i++)
			{
				float offset = stopList[i].offset;
				if (offset > _stopOffsetList[_stopOffsetList.Count - 1] && offset <= 100f)
				{
					_stopColorList.Add(GetColor(stopList[i].stopColor));
					_stopOffsetList.Add(offset);
				}
				else if (offset == _stopOffsetList[_stopOffsetList.Count - 1])
				{
					_stopColorList[_stopOffsetList.Count - 1] = GetColor(stopList[i].stopColor);
				}
			}
			if (_stopOffsetList[_stopOffsetList.Count - 1] != 100f)
			{
				_stopColorList.Add(_stopColorList[_stopOffsetList.Count - 1]);
				_stopOffsetList.Add(100f);
			}
		}

		protected Color GetColor(SVGColor svgColor)
		{
			if (svgColor.color.a != 1f)
			{
				_alphaBlended = true;
			}
			return svgColor.color;
		}
	}
}
