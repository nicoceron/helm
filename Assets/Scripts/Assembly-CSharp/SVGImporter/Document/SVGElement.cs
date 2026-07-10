using System.Collections.Generic;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter.Document
{
	public class SVGElement : SVGParentable, ISVGDrawable
	{
		protected string _name;

		private AttributeList _attrList;

		private List<object> _elementList;

		private SVGParser _xmlImp;

		private SVGPaintable _paintable;

		protected bool _rootElement;

		private SVGMatrix _cachedViewBoxTransform = SVGMatrix.identity;

		private bool cachedViewBox;

		public string name => _name;

		public AttributeList attributeList => _attrList;

		public List<object> elementList => _elementList;

		public SVGPaintable paintable => _paintable;

		public bool rootElement => _rootElement;

		public SVGElement(SVGParser xmlImp, SVGTransformList inheritTransformList, SVGPaintable inheritPaintable, bool root = false)
			: base(inheritTransformList)
		{
			_rootElement = root;
			_name = _attrList.GetValue("id");
			_xmlImp = xmlImp;
			_attrList = _xmlImp.node.attributes;
			if (inheritPaintable != null)
			{
				_paintable = new SVGPaintable(inheritPaintable, _xmlImp.node);
			}
			else
			{
				_paintable = new SVGPaintable(_xmlImp.node);
			}
			Init();
		}

		protected void Init()
		{
			_elementList = new List<object>();
			ViewBoxTransform();
			SVGTransform newItem = new SVGTransform(_cachedViewBoxTransform);
			SVGTransformList sVGTransformList = new SVGTransformList(_attrList.GetValue("transform"));
			sVGTransformList.AppendItem(newItem);
			base.currentTransformList = sVGTransformList;
			_ = _rootElement;
			GetElementList();
		}

		private void GetElementList()
		{
			_xmlImp.GetElementList(_elementList, _paintable, base.summaryTransformList);
		}

		public void BeforeRender(SVGTransformList transformList)
		{
			base.inheritTransformList = transformList;
			for (int i = 0; i < _elementList.Count; i++)
			{
				if (_elementList[i] is ISVGDrawable iSVGDrawable)
				{
					iSVGDrawable.BeforeRender(base.summaryTransformList);
				}
			}
		}

		public void Render()
		{
			for (int i = 0; i < _elementList.Count; i++)
			{
				if (_elementList[i] is ISVGDrawable iSVGDrawable)
				{
					iSVGDrawable.Render();
				}
			}
		}

		public SVGMatrix ViewBoxTransform()
		{
			if (!cachedViewBox)
			{
				cachedViewBox = true;
				Rect viewport = _paintable.viewport;
				if (_rootElement)
				{
					_cachedViewBoxTransform = SVGTransformable.GetRootViewBoxTransform(_attrList, ref viewport);
				}
				else
				{
					_cachedViewBoxTransform = SVGTransformable.GetViewBoxTransform(_attrList, ref viewport, negotiate: true);
				}
				paintable.SetViewport(viewport);
			}
			return _cachedViewBoxTransform;
		}
	}
}
