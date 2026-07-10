using System;
using System.Collections.Generic;
using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	public class SVGAtlas : MonoBehaviour
	{
		protected bool _atlasHasChanged;

		protected static bool _beingDestroyed;

		protected static Texture2D _whiteTexture;

		protected static Texture2D _gradientShapeTexture;

		protected static int _gradientShapeTextureSize = 512;

		protected SVGAtlasData _atlasData;

		protected Material _ui;

		protected Material _uiAntialiased;

		protected Material _opaqueSolid;

		protected Material _transparentSolid;

		protected Material _transparentSolidAntialiased;

		protected Material _opaqueGradient;

		protected Material _transparentGradient;

		protected Material _transparentGradientAntialiased;

		public List<Texture2D> atlasTextures;

		public List<Material> materials;

		public const int defaultGradientWidth = 128;

		public const int defaultGradientHeight = 4;

		public const int defaultAtlasTextureWidth = 512;

		public const int defaultAtlasTextureHeight = 512;

		private const int atlasIndex = 0;

		public int gradientWidth = 128;

		public int gradientHeight = 4;

		public int atlasTextureWidth = 512;

		public int atlasTextureHeight = 512;

		protected static SVGAtlas _Instance;

		private const int pixelOffset = 1;

		private const float PI2 = (float)Math.PI * 2f;

		public const string _GradientColorKey = "_GradientColor";

		public const string _GradientShapeKey = "_GradientShape";

		public const string _ParamsKey = "_Params";

		public bool atlasHasChanged => _atlasHasChanged;

		public static bool beingDestroyed => _beingDestroyed;

		public static Texture2D whiteTexture
		{
			get
			{
				if (_whiteTexture == null)
				{
					_whiteTexture = GenerateWhiteTexture();
				}
				return _whiteTexture;
			}
		}

		public static Texture2D gradientShapeTexture
		{
			get
			{
				if (_gradientShapeTexture == null)
				{
					_gradientShapeTexture = GenerateGradientShapeTexture(_gradientShapeTextureSize);
				}
				return _gradientShapeTexture;
			}
		}

		public static int gradientShapeTextureSize
		{
			get
			{
				return _gradientShapeTextureSize;
			}
			set
			{
				if (_gradientShapeTextureSize != value)
				{
					if (_gradientShapeTexture != null)
					{
						UnityEngine.Object.DestroyImmediate(_gradientShapeTexture);
					}
					_gradientShapeTexture = GenerateGradientShapeTexture(_gradientShapeTextureSize);
				}
			}
		}

		public SVGAtlasData atlasData => _atlasData;

		public Material ui
		{
			get
			{
				if (_ui == null)
				{
					_ui = new Material(SVGShader.UI);
					_ui.hideFlags = HideFlags.DontSave;
					UpdateMaterialProperties(_ui);
				}
				return _ui;
			}
		}

		public Material uiAntialiased
		{
			get
			{
				if (_uiAntialiased == null)
				{
					_uiAntialiased = new Material(SVGShader.UIAntialiased);
					_uiAntialiased.hideFlags = HideFlags.DontSave;
					UpdateMaterialProperties(_uiAntialiased);
				}
				return _uiAntialiased;
			}
		}

		public Material opaqueSolid
		{
			get
			{
				if (_opaqueSolid == null)
				{
					_opaqueSolid = new Material(SVGShader.SolidColorOpaque);
					_opaqueSolid.hideFlags = HideFlags.DontSave;
				}
				return _opaqueSolid;
			}
		}

		public Material transparentSolid
		{
			get
			{
				if (_transparentSolid == null)
				{
					_transparentSolid = new Material(SVGShader.SolidColorAlphaBlended);
					_transparentSolid.hideFlags = HideFlags.DontSave;
				}
				return _transparentSolid;
			}
		}

		public Material transparentSolidAntialiased
		{
			get
			{
				if (_transparentSolidAntialiased == null)
				{
					_transparentSolidAntialiased = new Material(SVGShader.SolidColorAlphaBlendedAntialiased);
					_transparentSolidAntialiased.hideFlags = HideFlags.DontSave;
				}
				return _transparentSolidAntialiased;
			}
		}

		public Material opaqueGradient
		{
			get
			{
				if (_opaqueGradient == null)
				{
					_opaqueGradient = new Material(SVGShader.GradientColorOpaque);
					_opaqueGradient.hideFlags = HideFlags.DontSave;
					UpdateMaterialProperties(_opaqueGradient);
				}
				return _opaqueGradient;
			}
		}

		public Material transparentGradient
		{
			get
			{
				if (_transparentGradient == null)
				{
					_transparentGradient = new Material(SVGShader.GradientColorAlphaBlended);
					_transparentGradient.hideFlags = HideFlags.DontSave;
					UpdateMaterialProperties(_transparentGradient);
				}
				return _transparentGradient;
			}
		}

		public Material transparentGradientAntialiased
		{
			get
			{
				if (_transparentGradientAntialiased == null)
				{
					_transparentGradientAntialiased = new Material(SVGShader.GradientColorAlphaBlendedAntialiased);
					_transparentGradientAntialiased.hideFlags = HideFlags.DontSave;
					UpdateMaterialProperties(_transparentGradientAntialiased);
				}
				return _transparentGradientAntialiased;
			}
		}

		public static SVGAtlas Instance
		{
			get
			{
				if (_Instance == null)
				{
					SVGAtlas[] array = Resources.FindObjectsOfTypeAll<SVGAtlas>();
					if (array != null && array.Length != 0)
					{
						_Instance = array[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = new GameObject("SVGAtlas", typeof(SVGAtlas))
					{
						hideFlags = HideFlags.HideAndDontSave
					}.GetComponent<SVGAtlas>();
					_Instance.hideFlags = HideFlags.DontSave;
					_Instance.Init();
				}
				return _Instance;
			}
		}

		public int imagePerRow => atlasTextureWidth / gradientWidth;

		public Vector4 textureParams => new Vector4(atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight);

		public static void ClearGradientShapeTexture()
		{
			if (!(gradientShapeTexture == null))
			{
				UnityEngine.Object.DestroyImmediate(_gradientShapeTexture);
				_gradientShapeTexture = null;
			}
		}

		public void UpdateMaterialProperties(Material material)
		{
			if (!(material == null))
			{
				if (atlasTextures != null && atlasTextures.Count > 0 && material.HasProperty("_GradientColor"))
				{
					material.SetTexture("_GradientColor", atlasTextures[0]);
				}
				if (material.HasProperty("_GradientShape"))
				{
					material.SetTexture("_GradientShape", gradientShapeTexture);
				}
				if (material.HasProperty("_Params"))
				{
					material.SetVector("_Params", new Vector4(atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
				}
			}
		}

		protected void Awake()
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			_atlasHasChanged = false;
			_beingDestroyed = false;
			AddFakeCamera();
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnAtlasPreRender));
		}

		public void OnPreRender()
		{
			OnAtlasPreRender();
		}

		protected void OnDestroy()
		{
			_beingDestroyed = true;
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnAtlasPreRender));
		}

		protected void AddFakeCamera()
		{
			Camera camera = base.gameObject.AddComponent<Camera>();
			camera.hideFlags = HideFlags.DontSave;
			camera.clearFlags = CameraClearFlags.Nothing;
			camera.orthographic = true;
			camera.depth = float.MinValue;
			camera.cullingMask = 0;
			camera.useOcclusionCulling = false;
		}

		public void OnAtlasPreRender(Camera camera = null)
		{
			SVGImporterSettings.UpdateAntialiasing();
			if (_atlasHasChanged)
			{
				RebuildAtlas();
				_atlasHasChanged = false;
			}
		}

		public bool ContainsMaterial(Material material)
		{
			if (material == _ui)
			{
				return true;
			}
			if (material == _uiAntialiased)
			{
				return true;
			}
			if (material == _opaqueSolid)
			{
				return true;
			}
			if (material == _transparentSolid)
			{
				return true;
			}
			if (material == _transparentSolidAntialiased)
			{
				return true;
			}
			if (material == _opaqueGradient)
			{
				return true;
			}
			if (material == _transparentGradient)
			{
				return true;
			}
			if (material == _transparentGradientAntialiased)
			{
				return true;
			}
			if (materials != null && materials.Contains(material))
			{
				return true;
			}
			return false;
		}

		public void UpdateMaterialList()
		{
			if (materials == null)
			{
				materials = new List<Material>();
			}
			materials.Clear();
			if (_ui != null)
			{
				materials.Add(_ui);
			}
			if (_uiAntialiased != null)
			{
				materials.Add(_uiAntialiased);
			}
			if (_opaqueSolid != null)
			{
				materials.Add(_opaqueSolid);
			}
			if (_transparentSolid != null)
			{
				materials.Add(_transparentSolid);
			}
			if (_opaqueGradient != null)
			{
				materials.Add(_opaqueGradient);
			}
			if (_transparentGradient != null)
			{
				materials.Add(_transparentGradient);
			}
			if (_transparentGradientAntialiased != null)
			{
				materials.Add(_transparentGradientAntialiased);
			}
		}

		public void UpdateGradientList()
		{
		}

		public void ClearAll()
		{
			Debug.Log("Cleared SVG Atlas: " + Time.frameCount + ", playmode: " + Application.isPlaying);
			if (_ui != null)
			{
				DestroyObjectInternal(_ui);
				_ui = null;
			}
			if (_uiAntialiased != null)
			{
				DestroyObjectInternal(_uiAntialiased);
				_uiAntialiased = null;
			}
			if (_opaqueSolid != null)
			{
				DestroyObjectInternal(_opaqueSolid);
				_opaqueSolid = null;
			}
			if (_transparentSolid != null)
			{
				DestroyObjectInternal(_transparentSolid);
				_transparentSolid = null;
			}
			if (_transparentSolidAntialiased != null)
			{
				DestroyObjectInternal(_transparentSolidAntialiased);
				_transparentSolidAntialiased = null;
			}
			if (_opaqueGradient != null)
			{
				DestroyObjectInternal(_opaqueGradient);
				_opaqueGradient = null;
			}
			if (_transparentGradient != null)
			{
				DestroyObjectInternal(_transparentGradient);
				_transparentGradient = null;
			}
			if (_transparentGradientAntialiased != null)
			{
				DestroyObjectInternal(_transparentGradientAntialiased);
				_transparentGradientAntialiased = null;
			}
			ClearAllData();
			ClearMaterials();
			ClearAtlasTextures();
		}

		protected void Init()
		{
			if (materials == null)
			{
				materials = new List<Material>();
			}
			if (_atlasData == null)
			{
				_atlasData = new SVGAtlasData();
				_atlasData.Init(atlasTextureWidth * atlasTextureHeight);
				AddGradient(SVGAtlasData.GetDefaultGradient());
			}
		}

		public static void RenderGradient(Texture2D texture, CCGradient gradient, int x, int y, int gradientWidth, int gradientHeight)
		{
			if (texture == null || gradient == null || !gradient.initialised)
			{
				return;
			}
			float num = gradientWidth - 1 - 2;
			Color[] array = new Color[gradientWidth * gradientHeight];
			for (int i = 0; i < gradientWidth; i++)
			{
				Color color = gradient.Evaluate((float)(i - 1) / num);
				for (int j = 0; j < gradientHeight; j++)
				{
					array[gradientWidth * j + i] = color;
				}
			}
			texture.SetPixels(x, y, gradientWidth, gradientHeight, array);
		}

		public bool GetCoords(out int x, out int y, int imageIndex)
		{
			bool result = atlasTextures == null || atlasTextures.Count == 0;
			GetCoords(out x, out y, imageIndex, gradientWidth, gradientHeight, atlasTextureWidth, atlasTextureHeight);
			return result;
		}

		public static void GetCoords(out int x, out int y, int imageIndex, int gradientWidth, int gradientHeight, int atlasTextureWidth, int atlasTextureHeight)
		{
			int num = imageIndex * gradientWidth;
			x = num % atlasTextureWidth;
			y = Mathf.FloorToInt(num / atlasTextureWidth) * gradientHeight;
		}

		public Texture CreateAtlasTexture(int index, int width, int height)
		{
			if (atlasTextures == null)
			{
				atlasTextures = new List<Texture2D>();
			}
			Texture2D texture2D = CreateTexture(width, height);
			texture2D.hideFlags = HideFlags.DontSave;
			texture2D.name = "Atlas " + index;
			AssignMaterialGradients(_opaqueGradient, texture2D, gradientShapeTexture, gradientWidth, gradientHeight);
			AssignMaterialGradients(_transparentGradient, texture2D, gradientShapeTexture, gradientWidth, gradientHeight);
			AssignMaterialGradients(_transparentGradientAntialiased, texture2D, gradientShapeTexture, gradientWidth, gradientHeight);
			AssignMaterialGradients(_ui, texture2D, gradientShapeTexture, gradientWidth, gradientHeight);
			AssignMaterialGradients(_uiAntialiased, texture2D, gradientShapeTexture, gradientWidth, gradientHeight);
			if (index >= atlasTextures.Count - 1)
			{
				atlasTextures.Add(texture2D);
			}
			else if (index >= 0)
			{
				atlasTextures[index] = texture2D;
			}
			return texture2D;
		}

		public static Texture2D CreateTexture(int width, int height)
		{
			return new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				anisoLevel = 0
			};
		}

		public CCGradient AddGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised)
			{
				return null;
			}
			if (_atlasData == null)
			{
				_atlasData = new SVGAtlasData();
				_atlasData.Init(atlasTextureWidth * atlasTextureHeight);
			}
			gradient = _atlasData.AddGradient(gradient, out var gradientExist);
			if (gradientExist)
			{
				return gradient;
			}
			int x = 0;
			int y = 0;
			GetCoords(out x, out y, gradient.index);
			gradient.atlasIndex = 0;
			_atlasHasChanged = true;
			return gradient;
		}

		public bool RemoveGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised)
			{
				return false;
			}
			if (_atlasData == null)
			{
				return false;
			}
			if (!_atlasData.RemoveGradient(gradient))
			{
				return false;
			}
			return true;
		}

		public CCGradient GetGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised)
			{
				return null;
			}
			if (_atlasData == null)
			{
				return null;
			}
			return _atlasData.GetGradient(gradient);
		}

		public bool HasGradient(CCGradient gradient)
		{
			if (gradient == null || !gradient.initialised)
			{
				return false;
			}
			if (_atlasData == null)
			{
				return false;
			}
			return _atlasData.HasGradient(gradient);
		}

		public void RebuildAtlas()
		{
			int index = 0;
			if (_atlasData == null)
			{
				Debug.LogWarning("atlasData is null! " + GetInstanceID());
				return;
			}
			CCGradient[] gradients = _atlasData.gradients;
			if (gradients == null)
			{
				return;
			}
			int num = gradients.Length;
			for (int i = 0; i < num; i++)
			{
				if (gradients[i] != null)
				{
					if (GetCoords(out var x, out var y, gradients[i].index))
					{
						CreateAtlasTexture(index, atlasTextureWidth, atlasTextureHeight);
					}
					RenderGradient(atlasTextures[index], gradients[i], x, y, gradientWidth, gradientHeight);
				}
			}
			for (int j = 0; j < atlasTextures.Count; j++)
			{
				atlasTextures[j].Apply(updateMipmaps: false);
			}
		}

		public static Texture2D GenerateGradientAtlasTexture(CCGradient[] gradients, int gradientWidth, int gradientHeight)
		{
			if (gradients == null || gradients.Length == 0)
			{
				return null;
			}
			int num = gradients.Length;
			int num2 = gradientWidth * 2;
			int height = Mathf.CeilToInt(num * gradientWidth / num2) * gradientHeight + gradientHeight;
			Texture2D texture2D = CreateTexture(num2, height);
			for (int i = 0; i < gradients.Length; i++)
			{
				GetCoords(out var x, out var y, i, gradientWidth, gradientHeight, num2, height);
				RenderGradient(texture2D, gradients[i], x, y, gradientWidth, gradientHeight);
			}
			texture2D.Apply(updateMipmaps: false);
			return texture2D;
		}

		public static Texture2D GenerateGradientShapeTexture(int textureSize)
		{
			Texture2D texture2D = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, mipChain: false);
			texture2D.hideFlags = HideFlags.DontSave;
			texture2D.name = "Gradient Shape Texture";
			texture2D.anisoLevel = 0;
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			int num = gradientShapeTextureSize * gradientShapeTextureSize;
			Color32[] array = new Color32[num];
			float num2 = 0f;
			float num3 = 0f;
			float num4 = (float)gradientShapeTextureSize * 0.5f;
			float num5 = gradientShapeTextureSize - 1;
			for (int i = 0; i < num; i++)
			{
				num2 = i % gradientShapeTextureSize;
				num3 = Mathf.Floor((float)i / (float)gradientShapeTextureSize);
				array[i].r = (byte)Mathf.RoundToInt(num2 / num5 * 255f);
				array[i].g = (byte)Mathf.RoundToInt(Mathf.Clamp01(Mathf.Sqrt(Mathf.Pow(num4 - num2, 2f) + Mathf.Pow(num4 - num3, 2f)) / (num4 - 1f)) * 255f);
				float num6 = Mathf.Atan2(0f - num4 + num3, 0f - num4 + num2);
				if (num6 < 0f)
				{
					num6 = (float)Math.PI * 2f + num6;
				}
				array[i].b = (byte)Mathf.RoundToInt(Mathf.Clamp01(num6 / ((float)Math.PI * 2f)) * 255f);
				array[i].a = byte.MaxValue;
			}
			texture2D.SetPixels32(array);
			texture2D.Apply(updateMipmaps: true);
			return texture2D;
		}

		public static Texture2D GenerateWhiteTexture()
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			texture2D.hideFlags = HideFlags.DontSave;
			texture2D.name = "White Texture";
			texture2D.anisoLevel = 0;
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.SetPixel(0, 0, Color.white);
			texture2D.Apply(updateMipmaps: false);
			return texture2D;
		}

		public Material GetMaterial(SVGFill fill)
		{
			Material result = null;
			switch (fill.fillType)
			{
			case FILL_TYPE.SOLID:
				result = GetColorMaterial(fill);
				break;
			case FILL_TYPE.GRADIENT:
				result = GetGradientMaterial(fill);
				break;
			}
			return result;
		}

		protected Material GetGradientMaterial(SVGFill fill)
		{
			Material material = null;
			Shader shader = null;
			shader = fill.blend switch
			{
				FILL_BLEND.OPAQUE => SVGShader.GradientColorOpaque, 
				FILL_BLEND.ALPHA_BLENDED => SVGShader.GradientColorAlphaBlended, 
				_ => SVGShader.GradientColorOpaque, 
			};
			for (int i = 0; i < materials.Count; i++)
			{
				if (!(materials[i] == null) && !(materials[i].shader != shader))
				{
					if (fill.gradientColors.atlasIndex < 0 || fill.gradientColors.atlasIndex >= atlasTextures.Count)
					{
						throw new IndexOutOfRangeException();
					}
					Texture texture = atlasTextures[fill.gradientColors.atlasIndex];
					if (!(texture == null) && !(materials[i].GetTexture("_GradientColor") != texture))
					{
						material = materials[i];
						material.SetTexture("_GradientShape", gradientShapeTexture);
						material.SetVector("_Params", new Vector4(atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
					}
				}
			}
			if (material == null)
			{
				material = new Material(shader);
				Texture2D value = atlasTextures[fill.gradientColors.atlasIndex];
				material.SetTexture("_GradientColor", value);
				material.SetTexture("_GradientShape", gradientShapeTexture);
				material.SetVector("_Params", new Vector4(atlasTextureWidth, atlasTextureHeight, gradientWidth, gradientHeight));
				materials.Add(material);
			}
			return material;
		}

		protected Material GetColorMaterial(SVGFill fill)
		{
			Material material = null;
			Shader shader = null;
			shader = fill.blend switch
			{
				FILL_BLEND.OPAQUE => SVGShader.SolidColorOpaque, 
				FILL_BLEND.ALPHA_BLENDED => SVGShader.SolidColorAlphaBlended, 
				_ => SVGShader.SolidColorOpaque, 
			};
			for (int i = 0; i < materials.Count; i++)
			{
				if (!(materials[i] == null) && !(materials[i].shader != shader))
				{
					material = materials[i];
				}
			}
			if (material == null)
			{
				material = new Material(shader);
				materials.Add(material);
			}
			return material;
		}

		protected string GetMegaBytes(int bits)
		{
			float num = bits / 1024 / 1024 / 8;
			if (num < 1f)
			{
				return Mathf.FloorToInt(bits / 1024 / 8) + " KB";
			}
			return num.ToString(".0") + " MB";
		}

		public void ClearAllData()
		{
			Debug.Log("Clear Atlas Data");
			if (_atlasData != null)
			{
				_atlasData.Clear();
			}
		}

		public void ClearMaterials()
		{
			if (materials == null)
			{
				return;
			}
			for (int i = 0; i < materials.Count; i++)
			{
				if (!(materials[i] == null))
				{
					DestroyObjectInternal(materials[i]);
				}
			}
			materials.Clear();
			materials = null;
		}

		public void ClearAtlasTextures()
		{
			if (atlasTextures == null || atlasTextures.Count == 0)
			{
				return;
			}
			for (int i = 0; i < atlasTextures.Count; i++)
			{
				if (!(atlasTextures[i] == null))
				{
					DestroyObjectInternal(atlasTextures[i]);
					atlasTextures[i] = null;
				}
			}
			atlasTextures.Clear();
		}

		private static void DestroyObjectInternal(UnityEngine.Object target)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(target);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(target, allowDestroyingAssets: true);
			}
		}

		internal static Camera[] GetAllCameras()
		{
			return Camera.allCameras;
		}

		internal static void AddComponent<T>(Component component) where T : MonoBehaviour
		{
			if (!(component == null))
			{
				GameObject gameObject = component.gameObject;
				if (!(gameObject == null) && !(gameObject.GetComponent<T>() != null))
				{
					gameObject.AddComponent<T>();
				}
			}
		}

		public static void AssignMaterialGradients(Material material, Texture2D gradientAtlas, Texture2D gradientShape, int gradientWidth, int gradientHeight)
		{
			if (!(material == null))
			{
				if (material.HasProperty("_GradientColor"))
				{
					material.SetTexture("_GradientColor", gradientAtlas);
				}
				if (material.HasProperty("_GradientShape"))
				{
					material.SetTexture("_GradientShape", gradientShape);
				}
				if (material.HasProperty("_Params") && gradientAtlas != null)
				{
					Vector4 value = new Vector4(gradientAtlas.width, gradientAtlas.height, gradientWidth, gradientHeight);
					material.SetVector("_Params", value);
				}
			}
		}

		public static void AssignMaterialGradients(Material[] materials, Texture2D gradientAtlas, Texture2D gradientShape, int gradientWidth, int gradientHeight)
		{
			if (materials != null && materials.Length != 0)
			{
				for (int i = 0; i < materials.Length; i++)
				{
					AssignMaterialGradients(materials[i], gradientAtlas, gradientShape, gradientWidth, gradientHeight);
				}
			}
		}

		public Material GetTransparentMaterial(bool antialiasing, bool hasGradients)
		{
			if (antialiasing)
			{
				if (hasGradients)
				{
					return transparentGradientAntialiased;
				}
				return transparentSolidAntialiased;
			}
			if (hasGradients)
			{
				return transparentGradient;
			}
			return transparentSolid;
		}

		public Material GetOpaqueMaterial(bool hasGradients)
		{
			if (hasGradients)
			{
				return opaqueGradient;
			}
			return opaqueSolid;
		}
	}
}
