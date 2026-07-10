using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGConicalGradientBrush
	{
		private SVGConicalGradientElement _conicalGradElement;

		private SVGLength _cx;

		private SVGLength _cy;

		private SVGLength _r;

		private List<Color> _stopColorList;

		private List<float> _stopOffsetList;

		protected bool _alphaBlended;

		protected SVGFill _fill;

		protected SVGMatrix _gradientTransform;

		protected SVGMatrix _transform;

		protected Rect _viewport;

		public bool alphaBlended => _alphaBlended;

		public SVGFill fill => _fill;

		public SVGConicalGradientBrush(SVGConicalGradientElement conicalGradElement)
		{
			_transform = SVGMatrix.identity;
			_conicalGradElement = conicalGradElement;
			Initialize();
			CreateFill();
		}

		public SVGConicalGradientBrush(SVGConicalGradientElement conicalGradElement, Rect bounds, SVGMatrix matrix, Rect viewport)
		{
			_viewport = viewport;
			_transform = matrix;
			_conicalGradElement = conicalGradElement;
			Initialize();
			CreateFill(bounds);
		}

		protected Color GetColor(SVGColor svgColor)
		{
			if (svgColor.color.a != 1f)
			{
				_alphaBlended = true;
			}
			return svgColor.color;
		}

		private void Initialize()
		{
			_cx = _conicalGradElement.cx;
			_cy = _conicalGradElement.cy;
			_r = _conicalGradElement.r;
			_stopColorList = new List<Color>();
			_stopOffsetList = new List<float>();
			GetStopList();
		}

		private void CreateFill()
		{
			if (_alphaBlended)
			{
				_fill = new SVGFill(Color.white, FILL_BLEND.ALPHA_BLENDED, FILL_TYPE.GRADIENT, GRADIENT_TYPE.CONICAL);
			}
			else
			{
				_fill = new SVGFill(Color.white, FILL_BLEND.OPAQUE, FILL_TYPE.GRADIENT, GRADIENT_TYPE.CONICAL);
			}
			_gradientTransform = _conicalGradElement.gradientTransform.Consolidate().matrix;
			_fill.gradientColors = SVGAssetImport.atlasData.AddGradient(ParseGradientColors());
			_fill.viewport = _viewport;
		}

		private void CreateFill(Rect bounds)
		{
			CreateFill();
			_fill.transform = SVGSimplePath.GetFillTransform(_fill, bounds, new SVGLength[2] { _cx, _cy }, new SVGLength[2] { _r, _r }, _transform, _gradientTransform);
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
			List<SVGStopElement> stopList = _conicalGradElement.stopList;
			int count = stopList.Count;
			if (count == 0)
			{
				return;
			}
			_stopColorList.Add(GetColor(stopList[0].stopColor));
			_stopOffsetList.Add(0f);
			int num = 0;
			for (num = 0; num < count; num++)
			{
				float offset = stopList[num].offset;
				if (offset > _stopOffsetList[_stopOffsetList.Count - 1] && offset <= 100f)
				{
					_stopColorList.Add(GetColor(stopList[num].stopColor));
					_stopOffsetList.Add(offset);
				}
				else if (offset == _stopOffsetList[_stopOffsetList.Count - 1])
				{
					_stopColorList[_stopOffsetList.Count - 1] = GetColor(stopList[num].stopColor);
				}
			}
			if (_stopOffsetList[_stopOffsetList.Count - 1] != 100f)
			{
				_stopColorList.Add(_stopColorList[_stopOffsetList.Count - 1]);
				_stopOffsetList.Add(100f);
			}
		}
	}
}
