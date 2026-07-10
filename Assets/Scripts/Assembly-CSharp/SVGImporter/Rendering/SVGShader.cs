using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGShader
	{
		protected static Shader _GradientColorAlphaBlended;

		protected static Shader _GradientColorAlphaBlendedAntialiased;

		protected static Shader _GradientColorOpaque;

		protected static Shader _SolidColorAlphaBlended;

		protected static Shader _SolidColorAlphaBlendedAntialiased;

		protected static Shader _SolidColorOpaque;

		protected static Shader _UI;

		protected static Shader _UIAntialiased;

		public static Shader GradientColorAlphaBlended
		{
			get
			{
				if (_GradientColorAlphaBlended == null)
				{
					_GradientColorAlphaBlended = Shader.Find("SVG Importer/GradientColor/GradientColorAlphaBlended");
				}
				return _GradientColorAlphaBlended;
			}
		}

		public static Shader GradientColorAlphaBlendedAntialiased
		{
			get
			{
				if (_GradientColorAlphaBlendedAntialiased == null)
				{
					_GradientColorAlphaBlendedAntialiased = Shader.Find("SVG Importer/GradientColor/GradientColorAlphaBlendedAntialiased");
				}
				return _GradientColorAlphaBlendedAntialiased;
			}
		}

		public static Shader GradientColorOpaque
		{
			get
			{
				if (_GradientColorOpaque == null)
				{
					_GradientColorOpaque = Shader.Find("SVG Importer/GradientColor/GradientColorOpaque");
				}
				return _GradientColorOpaque;
			}
		}

		public static Shader SolidColorAlphaBlended
		{
			get
			{
				if (_SolidColorAlphaBlended == null)
				{
					_SolidColorAlphaBlended = Shader.Find("SVG Importer/SolidColor/SolidColorAlphaBlended");
				}
				return _SolidColorAlphaBlended;
			}
		}

		public static Shader SolidColorAlphaBlendedAntialiased
		{
			get
			{
				if (_SolidColorAlphaBlendedAntialiased == null)
				{
					_SolidColorAlphaBlendedAntialiased = Shader.Find("SVG Importer/SolidColor/SolidColorAlphaBlendedAntialiased");
				}
				return _SolidColorAlphaBlendedAntialiased;
			}
		}

		public static Shader SolidColorOpaque
		{
			get
			{
				if (_SolidColorOpaque == null)
				{
					_SolidColorOpaque = Shader.Find("SVG Importer/SolidColor/SolidColorOpaque");
				}
				return _SolidColorOpaque;
			}
		}

		public static Shader UI
		{
			get
			{
				if (_UI == null)
				{
					_UI = Shader.Find("SVG Importer/UI/UI");
				}
				return _UI;
			}
		}

		public static Shader UIAntialiased
		{
			get
			{
				if (_UIAntialiased == null)
				{
					_UI = Shader.Find("SVG Importer/UI/UIAntialiased");
				}
				return _UI;
			}
		}
	}
}
