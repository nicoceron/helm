using System;
using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	[Serializable]
	public class SVGPaintable
	{
		private Rect _viewport;

		private SVGVisibility _visibility;

		private SVGDisplay _display;

		private SVGOverflow _overflow;

		private SVGClipPathUnits _clipPathUnits;

		private SVGClipRule _clipRule;

		private float _opacity;

		private float _fillOpacity;

		private float _strokeOpacity;

		private SVGColor? _fillColor;

		private SVGColor? _strokeColor;

		private SVGLength _strokeWidth;

		private SVGLength _miterLimit;

		private float[] _dashArray;

		private SVGLength _dashOfset;

		private bool isStrokeWidth;

		private SVGStrokeLineCapMethod _strokeLineCap;

		private SVGStrokeLineJoinMethod _strokeLineJoin;

		private SVGFillRule _fillRule;

		private Dictionary<string, Dictionary<string, string>> _cssStyle;

		private List<List<Vector2>> _clipPathList;

		private Dictionary<string, SVGLinearGradientElement> _linearGradList;

		private Dictionary<string, SVGRadialGradientElement> _radialGradList;

		private Dictionary<string, SVGConicalGradientElement> _conicalGradList;

		private string _gradientID = "";

		public SVGFill svgFill;

		public Rect viewport => _viewport;

		public SVGVisibility visibility => _visibility;

		public SVGDisplay display => _display;

		public SVGOverflow overflow => _overflow;

		public SVGClipPathUnits clipPathUnits => _clipPathUnits;

		public SVGClipRule clipRule => _clipRule;

		public SVGColor? fillColor => _fillColor;

		public SVGColor? strokeColor
		{
			get
			{
				if (IsStroke())
				{
					return _strokeColor;
				}
				return null;
			}
		}

		public float opacity => _opacity;

		public float fillOpacity => _fillOpacity;

		public float strokeOpacity => _strokeOpacity;

		public float strokeWidth => _strokeWidth.value;

		public float miterLimit => _miterLimit.value;

		public float[] dashArray => _dashArray;

		public float dashOffset => _dashOfset.value;

		public SVGStrokeLineCapMethod strokeLineCap => _strokeLineCap;

		public SVGStrokeLineJoinMethod strokeLineJoin => _strokeLineJoin;

		public SVGFillRule fillRule => _fillRule;

		public Dictionary<string, Dictionary<string, string>> cssStyle => _cssStyle;

		public List<List<Vector2>> clipPathList => _clipPathList;

		public Dictionary<string, SVGLinearGradientElement> linearGradList => _linearGradList;

		public Dictionary<string, SVGRadialGradientElement> radialGradList => _radialGradList;

		public Dictionary<string, SVGConicalGradientElement> conicalGradList => _conicalGradList;

		public string gradientID => _gradientID;

		private void InitDefaults()
		{
			isStrokeWidth = false;
			_visibility = SVGVisibility.Visible;
			_display = SVGDisplay.Inline;
			_overflow = SVGOverflow.visible;
			_clipPathUnits = SVGClipPathUnits.UserSpaceOnUse;
			_clipRule = SVGClipRule.nonzero;
			_opacity = 1f;
			_fillOpacity = 1f;
			_strokeOpacity = 1f;
			_fillColor = default(SVGColor);
			_strokeColor = default(SVGColor);
			_strokeWidth = new SVGLength(1f);
			_strokeLineJoin = SVGStrokeLineJoinMethod.Miter;
			_strokeLineCap = SVGStrokeLineCapMethod.Butt;
			_fillRule = SVGFillRule.NonZero;
			_miterLimit = new SVGLength(4f);
			_dashArray = null;
			_dashOfset = new SVGLength(0f);
			_cssStyle = new Dictionary<string, Dictionary<string, string>>();
			_clipPathList = new List<List<Vector2>>();
			_linearGradList = new Dictionary<string, SVGLinearGradientElement>();
			_radialGradList = new Dictionary<string, SVGRadialGradientElement>();
			_conicalGradList = new Dictionary<string, SVGConicalGradientElement>();
		}

		public SVGPaintable()
		{
			InitDefaults();
		}

		public SVGPaintable(Node node)
		{
			InitDefaults();
			Initialize(node.attributes);
			ReadCSS(node);
		}

		public void AddCSS(string cssString)
		{
			if (string.IsNullOrEmpty(cssString))
			{
				return;
			}
			Dictionary<string, Dictionary<string, string>> dictionary = CSSParser.Parse(cssString);
			if (dictionary == null || dictionary.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<string, Dictionary<string, string>> item in dictionary)
			{
				if (_cssStyle.ContainsKey(item.Key))
				{
					_cssStyle[item.Key] = item.Value;
				}
				else
				{
					_cssStyle.Add(item.Key, item.Value);
				}
			}
		}

		private List<List<Vector2>> CloneClipPathList(List<List<Vector2>> input)
		{
			if (input != null)
			{
				List<List<Vector2>> list = new List<List<Vector2>>();
				for (int i = 0; i < input.Count; i++)
				{
					if (input[i] != null && input[i].Count != 0)
					{
						list.Add(new List<Vector2>(input[i].ToArray()));
					}
				}
				return list;
			}
			return null;
		}

		public SVGPaintable(SVGPaintable inheritPaintable, Node node)
		{
			InitDefaults();
			if (inheritPaintable != null)
			{
				_visibility = inheritPaintable.visibility;
				_display = inheritPaintable.display;
				_clipRule = inheritPaintable.clipRule;
				_viewport = inheritPaintable._viewport;
				_fillRule = inheritPaintable._fillRule;
				_cssStyle = inheritPaintable._cssStyle;
				_clipPathList = CloneClipPathList(inheritPaintable._clipPathList);
				_linearGradList = inheritPaintable._linearGradList;
				_radialGradList = inheritPaintable._radialGradList;
				_conicalGradList = inheritPaintable._conicalGradList;
			}
			if (inheritPaintable != null)
			{
				if (!IsFillX())
				{
					if (inheritPaintable.IsLinearGradiantFill())
					{
						_gradientID = inheritPaintable.gradientID;
					}
					else if (inheritPaintable.IsRadialGradiantFill())
					{
						_gradientID = inheritPaintable.gradientID;
					}
					else
					{
						_fillColor = inheritPaintable.fillColor;
					}
				}
				if (!IsStroke() && inheritPaintable.IsStroke())
				{
					_strokeColor = inheritPaintable.strokeColor;
				}
				if (_strokeLineCap == SVGStrokeLineCapMethod.Unknown)
				{
					_strokeLineCap = inheritPaintable.strokeLineCap;
				}
				if (_strokeLineJoin == SVGStrokeLineJoinMethod.Unknown)
				{
					_strokeLineJoin = inheritPaintable.strokeLineJoin;
				}
				if (!isStrokeWidth)
				{
					_strokeWidth.NewValueSpecifiedUnits(inheritPaintable.strokeWidth);
				}
			}
			Initialize(node.attributes);
			ReadCSS(node);
			if (inheritPaintable != null)
			{
				_opacity *= inheritPaintable._opacity;
				_fillOpacity *= inheritPaintable._fillOpacity;
				_strokeOpacity *= inheritPaintable._strokeOpacity;
			}
		}

		private void Initialize(AttributeList attrList)
		{
			ReadStyle(attrList.Get);
			ReadStyle(attrList.GetValue("style"));
		}

		public void ReadCSS(Node node)
		{
			if (_cssStyle == null || _cssStyle.Count == 0)
			{
				return;
			}
			AttributeList attributes = node.attributes;
			string value = attributes.GetValue("class");
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			string[] array = value.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				string key = "." + array[i];
				if (_cssStyle.ContainsKey(key))
				{
					ReadCSSElement(_cssStyle[key]);
				}
			}
		}

		public void ReadCSSElement(Dictionary<string, string> element)
		{
			if (element != null && element.Count != 0)
			{
				ReadStyle(element);
			}
		}

		public void SetViewport(Rect viewport)
		{
			_viewport = viewport;
		}

		private void ReadStyle(string styleString)
		{
			if (!string.IsNullOrEmpty(styleString))
			{
				Dictionary<string, string> dic = new Dictionary<string, string>();
				SVGStringExtractor.ExtractStyleValue(styleString, ref dic);
				ReadStyle(dic);
			}
		}

		private void ReadClipPath(string clipPathValue)
		{
			if (clipPathValue.IndexOf("url") < 0)
			{
				return;
			}
			string text = SVGStringExtractor.ExtractUrl(clipPathValue);
			if (string.IsNullOrEmpty(text) || !SVGParser._defs.ContainsKey(text))
			{
				return;
			}
			Node node = SVGParser._defs[text];
			if (node == null)
			{
				return;
			}
			SVGMatrix identity = SVGMatrix.identity;
			switch (node.attributes.GetValue("clipPathUnits").ToLower())
			{
			case "userSpaceOnUse":
				_clipPathUnits = SVGClipPathUnits.UserSpaceOnUse;
				break;
			case "objectBoundingBox":
				_clipPathUnits = SVGClipPathUnits.ObjectBoundingBox;
				break;
			}
			List<Node> nodes = node.GetNodes();
			List<List<Vector2>> list = new List<List<Vector2>>();
			if (nodes != null && nodes.Count > 0)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					List<List<Vector2>> clipPath = GetClipPath(nodes[i], identity);
					if (clipPath != null)
					{
						list.AddRange(clipPath);
					}
				}
			}
			if (list.Count > 0)
			{
				list = SVGGeom.MergePolygon(list);
			}
			if (_clipPathList != null && _clipPathList.Count > 0)
			{
				_clipPathList = SVGGeom.ClipPolygon(_clipPathList, list);
			}
			else
			{
				_clipPathList = list;
			}
		}

		private List<List<Vector2>> GetClipPath(Node node, SVGMatrix svgMatrix)
		{
			SVGTransformList sVGTransformList = new SVGTransformList();
			sVGTransformList.AppendItem(new SVGTransform(svgMatrix));
			switch (node.name)
			{
			case SVGNodeName.Rect:
				return new SVGRectElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.Line:
				return new SVGLineElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.Circle:
				return new SVGCircleElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.Ellipse:
				return new SVGEllipseElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.PolyLine:
				return new SVGPolylineElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.Polygon:
				return new SVGPolygonElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.Path:
				return new SVGPathElement(node, sVGTransformList).GetClipPath();
			case SVGNodeName.Use:
			{
				string text = node.attributes.GetValue("xlink:href");
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				if (text[0] == '#')
				{
					text = text.Remove(0, 1);
				}
				if (SVGParser._defs.ContainsKey(text))
				{
					Node node2 = SVGParser._defs[text];
					if (node2 != null && node2 != node)
					{
						return GetClipPath(node2, svgMatrix);
					}
				}
				break;
			}
			}
			return null;
		}

		private void ReadStyle(Dictionary<string, string> _dictionary)
		{
			if (_dictionary == null || _dictionary.Count == 0)
			{
				return;
			}
			if (_dictionary.ContainsKey("visibility"))
			{
				SetVisibility(_dictionary["visibility"]);
			}
			if (_dictionary.ContainsKey("display"))
			{
				SetDisplay(_dictionary["display"]);
			}
			if (_dictionary.ContainsKey("overflow"))
			{
				SetOverflow(_dictionary["overflow"]);
			}
			if (_dictionary.ContainsKey("clip-rule"))
			{
				SetClipRule(_dictionary["clip-rule"]);
			}
			if (_dictionary.ContainsKey("clip-path"))
			{
				ReadClipPath(_dictionary["clip-path"]);
			}
			if (_dictionary.ContainsKey("fill"))
			{
				string text = _dictionary["fill"];
				if (text.IndexOf("url") >= 0)
				{
					_gradientID = SVGStringExtractor.ExtractUrl(text);
				}
				else
				{
					_fillColor = new SVGColor(_dictionary["fill"]);
				}
			}
			if (_dictionary.ContainsKey("opacity"))
			{
				_opacity *= new SVGLength(_dictionary["opacity"]).value;
			}
			if (_dictionary.ContainsKey("fill-opacity"))
			{
				_fillOpacity *= new SVGLength(_dictionary["fill-opacity"]).value;
			}
			if (_dictionary.ContainsKey("stroke-opacity"))
			{
				_strokeOpacity *= new SVGLength(_dictionary["stroke-opacity"]).value;
			}
			if (_dictionary.ContainsKey("fill-rule"))
			{
				SetFillRule(_dictionary["fill-rule"]);
			}
			if (_dictionary.ContainsKey("stroke"))
			{
				_strokeColor = new SVGColor(_dictionary["stroke"]);
			}
			if (_dictionary.ContainsKey("stroke-width"))
			{
				isStrokeWidth = true;
				_strokeWidth = new SVGLength(_dictionary["stroke-width"]);
			}
			if (_dictionary.ContainsKey("stroke-linecap"))
			{
				SetStrokeLineCap(_dictionary["stroke-linecap"]);
			}
			if (_dictionary.ContainsKey("stroke-linejoin"))
			{
				SetStrokeLineJoin(_dictionary["stroke-linejoin"]);
			}
			if (_dictionary.ContainsKey("stroke-miterlimit"))
			{
				_miterLimit = new SVGLength(_dictionary["stroke-miterlimit"]);
			}
			if (_dictionary.ContainsKey("stroke-dasharray"))
			{
				SetDashArray(_dictionary["stroke-dasharray"].Split(','));
			}
			if (_dictionary.ContainsKey("stroke-dashoffset"))
			{
				_dashOfset = new SVGLength(_dictionary["stroke-dashoffset"]);
			}
		}

		private void SetVisibility(string visibilityType)
		{
			switch (visibilityType)
			{
			case "visible":
				_visibility = SVGVisibility.Visible;
				break;
			case "hidden":
				_visibility = SVGVisibility.Hidden;
				break;
			case "collapse":
				_visibility = SVGVisibility.Collapse;
				break;
			}
		}

		private void SetOverflow(string overflowType)
		{
			switch (overflowType)
			{
			case "visible":
				_overflow = SVGOverflow.visible;
				break;
			case "auto":
				_overflow = SVGOverflow.auto;
				break;
			case "hidden":
				_overflow = SVGOverflow.hidden;
				break;
			case "scroll":
				_overflow = SVGOverflow.scroll;
				break;
			}
		}

		private void SetClipRule(string clipRuleType)
		{
			switch (clipRuleType)
			{
			case "nonzero":
				_clipRule = SVGClipRule.nonzero;
				break;
			case "evenodd":
				_clipRule = SVGClipRule.evenodd;
				break;
			}
		}

		private void SetDisplay(string displayType)
		{
			if (_display != SVGDisplay.None && displayType != null)
			{
				switch (displayType)
				{
				case "inline":
					_display = SVGDisplay.Inline;
					break;
				case "block":
					_display = SVGDisplay.Block;
					break;
				case "flex":
					_display = SVGDisplay.Flex;
					break;
				case "inline-block":
					_display = SVGDisplay.InlineBlock;
					break;
				case "inline-flex":
					_display = SVGDisplay.InlineFlex;
					break;
				case "inline-table":
					_display = SVGDisplay.InlineTable;
					break;
				case "list-item":
					_display = SVGDisplay.ListItem;
					break;
				case "run-in":
					_display = SVGDisplay.RunIn;
					break;
				case "table":
					_display = SVGDisplay.Table;
					break;
				case "table-caption":
					_display = SVGDisplay.TableCaption;
					break;
				case "table-column-group":
					_display = SVGDisplay.TableColumnGroup;
					break;
				case "table-header-group":
					_display = SVGDisplay.TableHeaderGroup;
					break;
				case "table-footer-group":
					_display = SVGDisplay.TableFooterGroup;
					break;
				case "table-row-group":
					_display = SVGDisplay.TableRowGroup;
					break;
				case "table-cell":
					_display = SVGDisplay.TableCell;
					break;
				case "table-column":
					_display = SVGDisplay.TableColumn;
					break;
				case "table-row":
					_display = SVGDisplay.TableRow;
					break;
				case "none":
					_display = SVGDisplay.None;
					break;
				}
			}
		}

		private void SetDashArray(string[] dashArrayType)
		{
			if (dashArrayType != null && dashArrayType.Length != 0)
			{
				_dashArray = new float[dashArrayType.Length];
				for (int i = 0; i < _dashArray.Length; i++)
				{
					_dashArray[i] = new SVGLength(dashArrayType[i]).value;
				}
			}
		}

		private void SetFillRule(string fillRuleType)
		{
			switch (fillRuleType)
			{
			case "nonzero":
				_fillRule = SVGFillRule.NonZero;
				break;
			case "evenodd":
				_fillRule = SVGFillRule.EvenOdd;
				break;
			}
		}

		private void SetStrokeLineCap(string lineCapType)
		{
			switch (lineCapType)
			{
			case "butt":
				_strokeLineCap = SVGStrokeLineCapMethod.Butt;
				break;
			case "round":
				_strokeLineCap = SVGStrokeLineCapMethod.Round;
				break;
			case "square":
				_strokeLineCap = SVGStrokeLineCapMethod.Square;
				break;
			}
		}

		private void SetStrokeLineJoin(string lineCapType)
		{
			switch (lineCapType)
			{
			case "miter":
				_strokeLineJoin = SVGStrokeLineJoinMethod.Miter;
				break;
			case "miter-clip":
				_strokeLineJoin = SVGStrokeLineJoinMethod.MiterClip;
				break;
			case "round":
				_strokeLineJoin = SVGStrokeLineJoinMethod.Round;
				break;
			case "bevel":
				_strokeLineJoin = SVGStrokeLineJoinMethod.Bevel;
				break;
			}
		}

		public bool IsLinearGradiantFill()
		{
			if (string.IsNullOrEmpty(_gradientID))
			{
				return false;
			}
			return _linearGradList.ContainsKey(_gradientID);
		}

		public bool IsRadialGradiantFill()
		{
			if (string.IsNullOrEmpty(_gradientID))
			{
				return false;
			}
			return _radialGradList.ContainsKey(_gradientID);
		}

		public bool IsConicalGradiantFill()
		{
			if (string.IsNullOrEmpty(_gradientID))
			{
				return false;
			}
			return _conicalGradList.ContainsKey(_gradientID);
		}

		public bool IsSolidFill()
		{
			if (!_fillColor.HasValue)
			{
				return false;
			}
			return _fillColor.Value.colorType != SVGColorType.None;
		}

		public bool IsFill()
		{
			if (!_fillColor.HasValue)
			{
				if (!IsLinearGradiantFill())
				{
					return IsRadialGradiantFill();
				}
				return true;
			}
			return _fillColor.Value.colorType != SVGColorType.None;
		}

		public bool IsFillX()
		{
			if (!_fillColor.HasValue)
			{
				if (!IsLinearGradiantFill())
				{
					return IsRadialGradiantFill();
				}
				return true;
			}
			return _fillColor.Value.colorType != SVGColorType.Unknown;
		}

		public bool IsStroke()
		{
			if (!_strokeColor.HasValue)
			{
				return false;
			}
			if (_strokeColor.Value.colorType == SVGColorType.Unknown || _strokeColor.Value.colorType == SVGColorType.None)
			{
				return false;
			}
			return true;
		}

		public SVGPaintMethod GetPaintType()
		{
			if (IsLinearGradiantFill())
			{
				return SVGPaintMethod.LinearGradientFill;
			}
			if (IsRadialGradiantFill())
			{
				return SVGPaintMethod.RadialGradientFill;
			}
			if (IsConicalGradiantFill())
			{
				return SVGPaintMethod.ConicalGradientFill;
			}
			if (IsSolidFill())
			{
				return SVGPaintMethod.SolidFill;
			}
			if (IsStroke())
			{
				return SVGPaintMethod.PathDraw;
			}
			return SVGPaintMethod.NoDraw;
		}

		public void AppendLinearGradient(SVGLinearGradientElement linearGradElement)
		{
			if (_linearGradList.ContainsKey(linearGradElement.id))
			{
				_linearGradList[linearGradElement.id] = linearGradElement;
			}
			else
			{
				_linearGradList.Add(linearGradElement.id, linearGradElement);
			}
		}

		public void AppendRadialGradient(SVGRadialGradientElement radialGradElement)
		{
			if (_radialGradList.ContainsKey(radialGradElement.id))
			{
				_radialGradList[radialGradElement.id] = radialGradElement;
			}
			else
			{
				_radialGradList.Add(radialGradElement.id, radialGradElement);
			}
		}

		public void AppendConicalGradient(SVGConicalGradientElement conicalGradElement)
		{
			if (_conicalGradList.ContainsKey(conicalGradElement.id))
			{
				_conicalGradList[conicalGradElement.id] = conicalGradElement;
			}
			else
			{
				_conicalGradList.Add(conicalGradElement.id, conicalGradElement);
			}
		}

		public SVGLinearGradientBrush GetLinearGradientBrush(Rect bounds, SVGMatrix matrix, Rect viewport)
		{
			if (!_linearGradList.ContainsKey(_gradientID))
			{
				return null;
			}
			return new SVGLinearGradientBrush(_linearGradList[_gradientID], bounds, matrix, viewport);
		}

		public SVGRadialGradientBrush GetRadialGradientBrush(Rect bounds, SVGMatrix matrix, Rect viewport)
		{
			if (!_radialGradList.ContainsKey(_gradientID))
			{
				return null;
			}
			return new SVGRadialGradientBrush(_radialGradList[_gradientID], bounds, matrix, viewport);
		}

		public SVGConicalGradientBrush GetConicalGradientBrush(Rect bounds, SVGMatrix matrix, Rect viewport)
		{
			if (!_conicalGradList.ContainsKey(_gradientID))
			{
				return null;
			}
			return new SVGConicalGradientBrush(_conicalGradList[_gradientID], bounds, matrix, viewport);
		}
	}
}
