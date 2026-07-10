using System;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ISVGShape), typeof(ISVGRenderer))]
	[AddComponentMenu("Rendering/SVG Stroke Renderer", 21)]
	public class SVGStrokeRenderer : MonoBehaviour, ISVGModify
	{
		public StrokeLineJoin lineJoin;

		public StrokeLineCap lineCap;

		public Color32 color = Color.white;

		public float width = 1f;

		public float mitterLimit = 4f;

		public float roundQuality = 10f;

		public float[] dashArray;

		public float dashOffset;

		public ClosePathRule closeLine;

		protected ISVGShape svgShape;

		protected ISVGRenderer svgRenderer;

		private void OnWillRenderObject()
		{
			if (svgRenderer != null && svgRenderer.lastFrameChanged != Time.frameCount)
			{
				svgRenderer.UpdateRenderer();
			}
		}

		protected virtual void PrepareForRendering(SVGLayer[] layers, SVGAsset svgAsset, bool force)
		{
		}

		private void Init()
		{
			svgShape = GetComponent(typeof(ISVGShape)) as ISVGShape;
			svgRenderer = GetComponent(typeof(ISVGRenderer)) as ISVGRenderer;
			if (svgRenderer != null)
			{
				svgRenderer.AddModifier(this);
				ISVGRenderer iSVGRenderer = svgRenderer;
				iSVGRenderer.OnPrepareForRendering = (Action<SVGLayer[], SVGAsset, bool>)Delegate.Combine(iSVGRenderer.OnPrepareForRendering, new Action<SVGLayer[], SVGAsset, bool>(PrepareForRendering));
			}
		}

		private void Clear()
		{
			if (svgRenderer != null)
			{
				ISVGRenderer iSVGRenderer = svgRenderer;
				iSVGRenderer.OnPrepareForRendering = (Action<SVGLayer[], SVGAsset, bool>)Delegate.Remove(iSVGRenderer.OnPrepareForRendering, new Action<SVGLayer[], SVGAsset, bool>(PrepareForRendering));
				svgRenderer.RemoveModifier(this);
				svgRenderer = null;
			}
			svgShape = null;
		}

		private void OnEnable()
		{
			Init();
		}

		private void OnDisable()
		{
			Clear();
		}
	}
}
