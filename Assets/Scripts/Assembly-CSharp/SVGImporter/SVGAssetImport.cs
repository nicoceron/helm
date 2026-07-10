using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	public class SVGAssetImport
	{
		public static SVGAtlasData atlasData;

		public static List<SVGError> errors;

		protected static bool _importingSVG = false;

		public static SVGUseGradients useGradients;

		public static bool antialiasing;

		public static float vpm;

		public static Vector2 pivotPoint;

		public static bool ignoreSVGCanvas;

		public static float meshScale = 1f;

		public static Vector4 border;

		public static bool sliceMesh = false;

		public static float minDepthOffset = 0.001f;

		public static SVGAssetFormat format = SVGAssetFormat.Opaque;

		public static bool compressDepth = true;

		private string _SVGFile;

		private Texture2D _texture;

		private SVGGraphics _graphics;

		private SVGDocument _svgDocument;

		public static bool importingSVG => _importingSVG;

		public SVGAssetImport(string svgFile, float vertexPerMeter = 1000f)
		{
			vpm = vertexPerMeter;
			_SVGFile = svgFile;
			_graphics = new SVGGraphics(vertexPerMeter, antialiasing);
		}

		private void CreateEmptySVGDocument()
		{
			_svgDocument = new SVGDocument(_SVGFile, _graphics);
		}

		public static void Clear()
		{
			if (atlasData != null)
			{
				atlasData.Clear();
				atlasData = null;
			}
			SVGParser.Clear();
			SVGGraphics.Clear();
		}

		public void NewSVGFile(string svgFile)
		{
			_SVGFile = svgFile;
		}

		public Texture2D GetTexture()
		{
			if (_texture == null)
			{
				return new Texture2D(0, 0, TextureFormat.ARGB32, mipChain: false);
			}
			return _texture;
		}

		public Texture2D CloneTexture(Texture2D texture)
		{
			if (texture == null)
			{
				return null;
			}
			Texture2D texture2D = new Texture2D(texture.width, texture.height, texture.format, mipChain: false);
			texture2D.name = texture.name;
			texture2D.SetPixels32(texture.GetPixels32());
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.anisoLevel = 0;
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.Apply();
			return texture2D;
		}
	}
}
