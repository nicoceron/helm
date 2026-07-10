using System;
using System.Collections.Generic;
using SVGImporter.Rendering;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SVGRenderer))]
	[RequireComponent(typeof(PolygonCollider2D))]
	[AddComponentMenu("Physics 2D/SVG Collider 2D", 20)]
	public class SVGCollider2D : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField]
		protected float _quality = 0.9f;

		[SerializeField]
		protected float _offset;

		protected SVGRenderer svgRenderer;

		protected PolygonCollider2D polygonCollider2D;

		private float precision;

		public float quality
		{
			get
			{
				return _quality;
			}
			set
			{
				if (_quality != value)
				{
					_quality = value;
					UpdateCollider();
				}
			}
		}

		public float offset
		{
			get
			{
				return _offset;
			}
			set
			{
				if (_offset != value)
				{
					_offset = value;
					UpdateCollider();
				}
			}
		}

		private void OnValidate()
		{
			UpdateCollider();
		}

		protected virtual void UpdateCollider()
		{
			if (svgRenderer == null)
			{
				svgRenderer = GetComponent<SVGRenderer>();
			}
			if (polygonCollider2D == null)
			{
				polygonCollider2D = GetComponent<PolygonCollider2D>();
			}
			if (svgRenderer.vectorGraphics == null || svgRenderer.vectorGraphics.colliderShape == null || svgRenderer.vectorGraphics.colliderShape.Length == 0)
			{
				polygonCollider2D.pathCount = 0;
				polygonCollider2D.points = null;
				return;
			}
			SVGPath[] colliderShape = svgRenderer.vectorGraphics.colliderShape;
			polygonCollider2D.pathCount = 0;
			if (_quality < 1f)
			{
				Bounds bounds = svgRenderer.vectorGraphics.bounds;
				float num = _quality;
				if (num < 0.001f)
				{
					num = 0.001f;
				}
				precision = Mathf.Max(bounds.size.x, bounds.size.y) / num;
				if (precision < 0.001f)
				{
					precision = 0.001f;
				}
				precision *= 0.05f;
			}
			List<Vector2[]> list = new List<Vector2[]>();
			for (int i = 0; i < colliderShape.Length; i++)
			{
				Vector2[] array = ((!(_quality < 1f)) ? ((Vector2[])colliderShape[i].points.Clone()) : SVGBezier.Optimise(colliderShape[i].points, precision));
				if (_offset != 0f)
				{
					array = SVGGeomUtils.OffsetVerts(array, _offset);
				}
				if (array != null && array.Length > 2)
				{
					list.Add(array);
				}
			}
			if (list.Count > 0)
			{
				polygonCollider2D.pathCount = list.Count;
				for (int j = 0; j < list.Count; j++)
				{
					polygonCollider2D.SetPath(j, list[j]);
				}
			}
		}

		private void OnEnable()
		{
			if (svgRenderer == null)
			{
				svgRenderer = GetComponent<SVGRenderer>();
			}
			SVGRenderer sVGRenderer = svgRenderer;
			sVGRenderer.onVectorGraphicsChanged = (Action<SVGAsset>)Delegate.Combine(sVGRenderer.onVectorGraphicsChanged, new Action<SVGAsset>(OnVectorGraphicsChanged));
			UpdateCollider();
		}

		private void OnDisable()
		{
			if (svgRenderer == null)
			{
				svgRenderer = GetComponent<SVGRenderer>();
			}
			SVGRenderer sVGRenderer = svgRenderer;
			sVGRenderer.onVectorGraphicsChanged = (Action<SVGAsset>)Delegate.Remove(sVGRenderer.onVectorGraphicsChanged, new Action<SVGAsset>(OnVectorGraphicsChanged));
		}

		protected virtual void OnVectorGraphicsChanged(SVGAsset svgAsset)
		{
			UpdateCollider();
		}
	}
}
