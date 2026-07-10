using UnityEngine;

namespace SVGImporter
{
	public class SVGRenderTexture
	{
		private const int EMPTY_LAYER = 31;

		protected static Camera _camera;

		protected static SVGRenderer _renderer;

		protected static Camera camera
		{
			get
			{
				if (_camera == null)
				{
					_camera = new GameObject("SVG Camera").AddComponent<Camera>();
					_camera.cullingMask = int.MinValue;
					_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
					_camera.clearFlags = CameraClearFlags.Color;
					_camera.orthographic = true;
					_camera.enabled = false;
				}
				return _camera;
			}
		}

		protected static SVGRenderer renderer
		{
			get
			{
				if (_renderer == null)
				{
					_renderer = new GameObject("editor SVG Renderer")
					{
						layer = 31
					}.AddComponent<SVGRenderer>();
				}
				return _renderer;
			}
		}

		protected static void RemoveCamera()
		{
			if (_camera != null)
			{
				_camera.targetTexture = null;
				Object.Destroy(_camera.gameObject);
				_camera = null;
			}
		}

		protected static void RemoveSVGRenderer()
		{
			if (_renderer != null)
			{
				_renderer.vectorGraphics = null;
				Object.Destroy(_renderer.gameObject);
				_renderer = null;
			}
		}

		protected static RenderTexture GetRenderTexture(SVGAsset svgAsset, Rect textureSize)
		{
			float num = 1f;
			if (svgAsset != null)
			{
				num = svgAsset.bounds.size.x / svgAsset.bounds.size.y;
			}
			int num2 = Mathf.CeilToInt(textureSize.width);
			RenderTexture renderTexture = new RenderTexture(num2, Mathf.CeilToInt((float)num2 / num), 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
			renderTexture.antiAliasing = 8;
			renderTexture.Create();
			return renderTexture;
		}

		public static RenderTexture RenderSVG(SVGAsset svgAsset, Rect textureSize)
		{
			Bounds bounds = svgAsset.bounds;
			renderer.transform.position = camera.transform.forward * (camera.nearClipPlane + svgAsset.bounds.size.z + 1f) - svgAsset.bounds.center;
			renderer.vectorGraphics = svgAsset;
			if (bounds.size.x > bounds.size.y)
			{
				camera.orthographicSize = Mathf.Min(bounds.size.x, bounds.size.y) * 0.5f;
			}
			else
			{
				camera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) * 0.5f;
			}
			RenderTexture renderTexture = GetRenderTexture(svgAsset, textureSize);
			camera.targetTexture = renderTexture;
			camera.Render();
			camera.targetTexture = null;
			RemoveSVGRenderer();
			RemoveCamera();
			return renderTexture;
		}
	}
}
