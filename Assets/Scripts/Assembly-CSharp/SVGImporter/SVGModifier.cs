using System;
using UnityEngine;

namespace SVGImporter
{
	public abstract class SVGModifier : MonoBehaviour, ISVGModify
	{
		[HideInInspector]
		public bool manualUpdate;

		[HideInInspector]
		public bool useSelection;

		[HideInInspector]
		public LayerSelection layerSelection;

		protected ISVGRenderer _svgRenderer;

		public bool hasSelection
		{
			get
			{
				if (!useSelection)
				{
					return false;
				}
				if (layerSelection == null || layerSelection.layers.Count == 0)
				{
					return false;
				}
				return true;
			}
		}

		public ISVGRenderer svgRenderer
		{
			get
			{
				if (_svgRenderer == null)
				{
					_svgRenderer = GetComponent<ISVGRenderer>();
				}
				return _svgRenderer;
			}
		}

		protected virtual void Init()
		{
			if (svgRenderer != null)
			{
				svgRenderer.AddModifier(this);
				ISVGRenderer iSVGRenderer = svgRenderer;
				iSVGRenderer.OnPrepareForRendering = (Action<SVGLayer[], SVGAsset, bool>)Delegate.Combine(iSVGRenderer.OnPrepareForRendering, new Action<SVGLayer[], SVGAsset, bool>(PrepareForRendering));
			}
		}

		protected virtual void Clear()
		{
			if (svgRenderer != null)
			{
				ISVGRenderer iSVGRenderer = svgRenderer;
				iSVGRenderer.OnPrepareForRendering = (Action<SVGLayer[], SVGAsset, bool>)Delegate.Remove(iSVGRenderer.OnPrepareForRendering, new Action<SVGLayer[], SVGAsset, bool>(PrepareForRendering));
				svgRenderer.RemoveModifier(this);
				_svgRenderer = null;
			}
		}

		protected virtual void OnEnable()
		{
			Init();
		}

		protected virtual void OnDisable()
		{
			Clear();
			if (svgRenderer != null)
			{
				svgRenderer.UpdateRenderer();
			}
		}

		protected virtual void OnWillRenderObject()
		{
			if (svgRenderer != null && !manualUpdate && (!Application.isPlaying || svgRenderer.lastFrameChanged != Time.frameCount))
			{
				svgRenderer.UpdateRenderer();
			}
		}

		protected abstract void PrepareForRendering(SVGLayer[] layers, SVGAsset svgAsset, bool force);
	}
}
