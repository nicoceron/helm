using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[AddComponentMenu("Miscellaneous/SVG Frame Animator", 20)]
	public class SVGFrameAnimator : MonoBehaviour
	{
		public SVGAsset[] frames;

		public float frameIndex;

		private float lastFrameIndex;

		protected SVGRenderer _svgRenderer;

		protected SVGImage _svgImage;

		public SVGRenderer svgRenderer
		{
			get
			{
				if (_svgRenderer == null)
				{
					_svgRenderer = GetComponent<SVGRenderer>();
				}
				return _svgRenderer;
			}
		}

		public SVGImage svgImage
		{
			get
			{
				if (_svgImage == null)
				{
					_svgImage = GetComponent<SVGImage>();
				}
				return _svgImage;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
		}

		protected virtual void UpdateMesh()
		{
			if (frames != null && frames.Length != 0)
			{
				int num = (int)Mathf.Repeat(frameIndex, frames.Length);
				if (svgRenderer != null && svgRenderer.vectorGraphics != frames[num])
				{
					svgRenderer.vectorGraphics = frames[num];
				}
				if (svgImage != null && svgImage.vectorGraphics != frames[num])
				{
					svgImage.vectorGraphics = frames[num];
				}
			}
		}

		private void LateUpdate()
		{
			if (frameIndex != lastFrameIndex)
			{
				UpdateMesh();
				lastFrameIndex = frameIndex;
			}
		}
	}
}
