using System;
using System.Collections.Generic;
using SVGImporter.Geometry;
using SVGImporter.Rendering;
using SVGImporter.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[AddComponentMenu("Rendering/SVG Renderer", 20)]
	public class SVGRenderer : UIBehaviour, ISVGShape, ISVGRenderer, ISVGReference
	{
		public enum Type
		{
			Simple = 0,
			Sliced = 1
		}

		public Action<SVGAsset> onVectorGraphicsChanged;

		protected Action<SVGLayer[], SVGAsset, bool> _OnPrepareForRendering;

		protected Type _lastType;

		[FormerlySerializedAs("type")]
		[SerializeField]
		private Type _type;

		[FormerlySerializedAs("lastTimeModified")]
		[SerializeField]
		protected long _lastTimeModified;

		protected Rect _rectTransformRect;

		protected Rect _lastRectTransformRect;

		protected int _lastFrameChanged;

		[FormerlySerializedAs("vectorGraphics")]
		[SerializeField]
		protected SVGAsset _vectorGraphics;

		protected SVGAsset _lastVectorGraphics;

		[FormerlySerializedAs("color")]
		[SerializeField]
		protected Color _color = Color.white;

		protected Color _lastColor = Color.white;

		protected Color32[] _cachedColors;

		protected Vector3[] _cachedVertices;

		[FormerlySerializedAs("opaqueMaterial")]
		[SerializeField]
		protected Material _opaqueMaterial;

		protected Material _lastOpaqueMaterial;

		[FormerlySerializedAs("transparentMaterial")]
		[SerializeField]
		protected Material _transparentMaterial;

		protected Material _lastTransparentMaterial;

		protected MeshFilter _meshFilter;

		protected MeshRenderer _meshRenderer;

		protected SVGLayer[] _layers;

		protected Mesh _sharedMesh;

		protected Mesh _mesh;

		[FormerlySerializedAs("sortingLayerID")]
		[SerializeField]
		protected int _sortingLayerID;

		protected int _lastSortingLayerID;

		[FormerlySerializedAs("sortingLayerName")]
		[SerializeField]
		protected string _sortingLayerName;

		[FormerlySerializedAs("sortingOrder")]
		[SerializeField]
		protected int _sortingOrder;

		protected int _lastSortingOrder;

		[FormerlySerializedAs("overrideSorter")]
		[SerializeField]
		protected bool _overrideSorter;

		protected bool _lastOverrideSorter;

		[FormerlySerializedAs("overrideSorterChildren")]
		[SerializeField]
		protected bool _overrideSorterChildren;

		protected bool _lastOverrideSorterChildren;

		protected Color32[] _finalColors;

		protected Color _cashedColor;

		private const float epsilon = 1E-07f;

		protected Vector3[] _finalVertices;

		protected bool _lastUseSharedMesh;

		protected List<ISVGModify> _modifiers = new List<ISVGModify>();

		public virtual Action<SVGLayer[], SVGAsset, bool> OnPrepareForRendering
		{
			get
			{
				return _OnPrepareForRendering;
			}
			set
			{
				_OnPrepareForRendering = value;
			}
		}

		public Type type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public int lastFrameChanged => _lastFrameChanged;

		public SVGAsset vectorGraphics
		{
			get
			{
				return _vectorGraphics;
			}
			set
			{
				_vectorGraphics = value;
			}
		}

		public Color color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
			}
		}

		public Material opaqueMaterial
		{
			get
			{
				return _opaqueMaterial;
			}
			set
			{
				if (_opaqueMaterial != value)
				{
					_opaqueMaterial = value;
					UpdateMaterials();
				}
			}
		}

		public Material transparentMaterial
		{
			get
			{
				return _transparentMaterial;
			}
			set
			{
				if (_transparentMaterial != value)
				{
					_transparentMaterial = value;
					UpdateMaterials();
				}
			}
		}

		public MeshFilter meshFilter
		{
			get
			{
				if (_meshFilter == null)
				{
					GetComponent<MeshRenderer>();
				}
				return _meshFilter;
			}
		}

		public MeshRenderer meshRenderer
		{
			get
			{
				if (_meshRenderer == null)
				{
					GetComponent<MeshRenderer>();
				}
				return _meshRenderer;
			}
		}

		public RectTransform rectTransform => base.transform as RectTransform;

		public int sortingLayerID
		{
			get
			{
				return meshRenderer.sortingLayerID;
			}
			set
			{
				if (!SortingLayer.IsValid(value))
				{
					Debug.LogWarning(base.name + ": This renderer has an invalid layer-id, resetting to default.");
					_sortingLayerID = SortingLayer.NameToID("Default");
				}
				else
				{
					_sortingLayerID = value;
				}
				meshRenderer.sortingLayerID = _sortingLayerID;
				_sortingLayerName = meshRenderer.sortingLayerName;
			}
		}

		public string sortingLayerName
		{
			get
			{
				return meshRenderer.sortingLayerName;
			}
			set
			{
				meshRenderer.sortingLayerName = (_sortingLayerName = value);
				_lastSortingLayerID = (_sortingLayerID = meshRenderer.sortingLayerID);
			}
		}

		public int sortingOrder
		{
			get
			{
				return meshRenderer.sortingOrder;
			}
			set
			{
				meshRenderer.sortingOrder = (_sortingOrder = value);
			}
		}

		public bool overrideSorter
		{
			get
			{
				return _overrideSorter;
			}
			set
			{
				_overrideSorter = value;
			}
		}

		public bool overrideSorterChildren
		{
			get
			{
				return _overrideSorterChildren;
			}
			set
			{
				_overrideSorterChildren = value;
			}
		}

		public SVGPath[] shape
		{
			get
			{
				if (_vectorGraphics == null)
				{
					return null;
				}
				return _vectorGraphics.colliderShape;
			}
		}

		public bool hasBorder
		{
			get
			{
				if (_vectorGraphics != null)
				{
					return _vectorGraphics.border.sqrMagnitude > 0f;
				}
				return false;
			}
		}

		protected float pixelsPerUnit => 100f;

		private bool useLayers => _vectorGraphics.useLayers;

		private bool useSharedMesh
		{
			get
			{
				if (!useLayers && _color == Color.white)
				{
					return _type == Type.Simple;
				}
				return false;
			}
		}

		public bool isVisible => _meshRenderer.isVisible;

		public List<ISVGModify> modifiers => _modifiers;

		protected override void Awake()
		{
			base.Awake();
			CacheComponents();
			meshFilter.sharedMesh = null;
			if (_vectorGraphics != null)
			{
				_vectorGraphics.AddReference(this);
			}
			Clear();
			PrepareForRendering(force: true);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			EnableMeshRenderer(value: true);
		}

		private void OnWillRenderObject()
		{
			if (!meshRenderer.isPartOfStaticBatch)
			{
				PrepareForRendering();
			}
		}

		protected override void OnDisable()
		{
			EnableMeshRenderer(value: false);
			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			if (_vectorGraphics != null)
			{
				_vectorGraphics.RemoveReference(this);
			}
			base.OnDestroy();
		}

		private void CacheComponents()
		{
			if (_meshFilter == null)
			{
				_meshFilter = GetComponent<MeshFilter>();
				if (_meshFilter == null)
				{
					_meshFilter = base.gameObject.AddComponent<MeshFilter>();
				}
			}
			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
				if (_meshRenderer == null)
				{
					_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
				}
			}
		}

		public void UpdateRenderer()
		{
			PrepareForRendering(force: true);
		}

		protected void PrepareForRendering(bool force = false)
		{
			if (_vectorGraphics == null)
			{
				if (_lastVectorGraphics != null)
				{
					_lastVectorGraphics.RemoveReference(this);
					_lastVectorGraphics = null;
				}
				Clear();
				return;
			}
			bool flag = force || _lastType != _type || meshFilter.sharedMesh == null;
			bool flag2 = force || _lastColor != _color;
			bool flag3 = force || _lastOpaqueMaterial != _opaqueMaterial || _lastTransparentMaterial != _transparentMaterial;
			if (_lastVectorGraphics != _vectorGraphics)
			{
				flag = true;
				flag2 = true;
				if (_lastVectorGraphics != null)
				{
					_lastVectorGraphics.RemoveReference(this);
				}
				if (_vectorGraphics != null)
				{
					_vectorGraphics.AddReference(this);
				}
			}
			if (useLayers || !useSharedMesh)
			{
				if (_lastUseSharedMesh)
				{
					flag = true;
				}
				if (!flag && _type == Type.Sliced && rectTransform != null)
				{
					_rectTransformRect = rectTransform.rect;
					if (_rectTransformRect != _lastRectTransformRect)
					{
						flag = true;
						_lastRectTransformRect = _rectTransformRect;
					}
				}
			}
			if (useLayers)
			{
				if (_layers == null)
				{
					_layers = _vectorGraphics.layersClone;
				}
				if (flag || flag2)
				{
					InitMesh();
					flag3 = true;
					if (_type == Type.Sliced)
					{
						UpdateSlicedMesh();
					}
					UpdateColors(force);
					_lastFrameChanged = Time.frameCount;
					flag3 = true;
					if (_OnPrepareForRendering != null)
					{
						_OnPrepareForRendering(_layers, _vectorGraphics, force);
					}
					GenerateMesh();
					if (meshFilter.sharedMesh != _mesh)
					{
						meshFilter.sharedMesh = _mesh;
					}
				}
			}
			else if (useSharedMesh)
			{
				_sharedMesh = _vectorGraphics.sharedMesh;
				meshFilter.sharedMesh = _sharedMesh;
			}
			else
			{
				if (flag)
				{
					InitMesh();
					flag3 = true;
					if (_type == Type.Sliced)
					{
						UpdateSlicedMesh();
					}
					if (onVectorGraphicsChanged != null)
					{
						onVectorGraphicsChanged(_vectorGraphics);
					}
				}
				if (flag || flag2)
				{
					UpdateColors(force);
					_lastFrameChanged = Time.frameCount;
					flag3 = true;
				}
				if (meshFilter.sharedMesh != _mesh)
				{
					meshFilter.sharedMesh = _mesh;
				}
			}
			if (flag3)
			{
				UpdateMaterials();
			}
			_lastOpaqueMaterial = _opaqueMaterial;
			_lastTransparentMaterial = _transparentMaterial;
			_lastVectorGraphics = _vectorGraphics;
			_lastColor = _color;
			_lastType = _type;
			_lastUseSharedMesh = useSharedMesh;
		}

		protected void GenerateMesh()
		{
			SVGMesh.CombineMeshes(_layers, _mesh, out var _, _vectorGraphics.useGradients, _vectorGraphics.format, _vectorGraphics.compressDepth, _vectorGraphics.antialiasing);
		}

		protected void UpdateColors(bool force = false)
		{
			if (_color == Color.white)
			{
				return;
			}
			if (useLayers)
			{
				Color32 color = _color;
				bool flag = color.a != byte.MaxValue;
				int num = _layers.Length;
				for (int i = 0; i < num; i++)
				{
					int num2 = _layers[i].shapes.Length;
					for (int j = 0; j < num2; j++)
					{
						if (_layers[i].shapes[j].fill != null)
						{
							Color32 color2 = _layers[i].shapes[j].fill.color;
							color2.r = (byte)(color2.r * color.r / 255);
							color2.g = (byte)(color2.g * color.g / 255);
							color2.b = (byte)(color2.b * color.b / 255);
							color2.a = (byte)(color2.a * color.a / 255);
							_layers[i].shapes[j].fill.color = color2;
							if (flag)
							{
								_layers[i].shapes[j].fill.blend = FILL_BLEND.ALPHA_BLENDED;
							}
						}
					}
				}
			}
			else
			{
				if (!(_sharedMesh != null))
				{
					return;
				}
				if (_cachedColors == null || _cachedColors.Length != _sharedMesh.vertexCount)
				{
					Color32[] colors = _sharedMesh.colors32;
					if (colors == null || colors.Length == 0)
					{
						return;
					}
					_finalColors = new Color32[colors.Length];
					_cachedColors = (Color32[])colors.Clone();
				}
				int num3 = _cachedColors.Length;
				Color32 color3 = _color;
				if (!(opaqueMaterial.name == "AlphaBlend_SVGColorApply"))
				{
					for (int k = 0; k < num3; k++)
					{
						_finalColors[k].r = (byte)(_cachedColors[k].r * color3.r / 255);
						_finalColors[k].g = (byte)(_cachedColors[k].g * color3.g / 255);
						_finalColors[k].b = (byte)(_cachedColors[k].b * color3.b / 255);
						_finalColors[k].a = (byte)(_cachedColors[k].a * color3.a / 255);
					}
					_mesh.colors32 = _finalColors;
					meshFilter.sharedMesh = _mesh;
				}
			}
		}

		protected float InverseLerp(float from, float to, float value)
		{
			if (from < to)
			{
				value -= from;
				value /= to - from;
				return value;
			}
			return 1f - (value - to) / (from - to);
		}

		protected float SafeDivide(float a, float b)
		{
			if (b == 0f)
			{
				return 0f;
			}
			return a / b;
		}

		protected string BorderToString(Vector4 border)
		{
			return $"left: {border.x}, bottom: {border.y}, right: {border.z}, top: {border.w}";
		}

		protected void UpdateSlicedMesh()
		{
			if (!hasBorder || !(rectTransform != null))
			{
				return;
			}
			Bounds bounds = _vectorGraphics.bounds;
			Vector4 vector = new Vector4(_rectTransformRect.x, _rectTransformRect.y, _rectTransformRect.width, _rectTransformRect.height);
			Vector4 border = _vectorGraphics.border;
			Vector4 vector2 = new Vector4(border.x + 1E-07f, border.y + 1E-07f, 1f - border.z - 1E-07f, 1f - border.w - 1E-07f);
			float num = vectorGraphics.scale * 100f;
			Vector2 vector3 = new Vector2(bounds.size.x * num, bounds.size.y * num);
			Vector4 vector4 = new Vector4(vector.x, vector.y, vector.x + vector.z, vector.y + vector.w);
			Vector4 vector5 = new Vector4(vector3.x * border.x, vector3.y * border.y, vector3.x * border.z, vector3.y * border.w);
			Vector2 vector6 = new Vector2(SafeDivide(1f, 1f - (border.x + border.z)) * (vector.z - (vector5.x + vector5.z)), SafeDivide(1f, 1f - (border.y + border.w)) * (vector.w - (vector5.w + vector5.y)));
			float num2 = vector5.x + vector5.z;
			if (num2 != 0f)
			{
				num2 = Mathf.Clamp01(vector.z / num2);
				if (num2 != 1f)
				{
					vector6.x = 0f;
					vector3.x *= num2;
					vector5.x *= num2;
					vector5.z *= num2;
				}
			}
			float num3 = vector5.w + vector5.y;
			if (num3 != 0f)
			{
				num3 = Mathf.Clamp01(vector.w / num3);
				if (num3 != 1f)
				{
					vector6.y = 0f;
					vector3.y *= num3;
					vector5.w *= num3;
					vector5.y *= num3;
				}
			}
			float num4 = vector4.w - vector5.w;
			float num5 = vector4.x + vector5.x;
			Vector2 vector7 = default(Vector2);
			if (useLayers)
			{
				int num6 = _layers.Length;
				for (int i = 0; i < num6; i++)
				{
					int num7 = _layers[i].shapes.Length;
					for (int j = 0; j < num7; j++)
					{
						int num8 = _layers[i].shapes[j].vertices.Length;
						for (int k = 0; k < num8; k++)
						{
							vector7.x = InverseLerp(bounds.min.x, bounds.max.x, _layers[i].shapes[j].vertices[k].x);
							vector7.y = InverseLerp(bounds.min.y, bounds.max.y, _layers[i].shapes[j].vertices[k].y);
							if (border.x != 0f && vector7.x <= vector2.x)
							{
								_layers[i].shapes[j].vertices[k].x = vector4.x + vector7.x * vector3.x;
							}
							else if (border.z != 0f && vector7.x >= vector2.z)
							{
								_layers[i].shapes[j].vertices[k].x = vector4.z - (1f - vector7.x) * vector3.x;
							}
							else
							{
								_layers[i].shapes[j].vertices[k].x = num5 + (vector7.x - border.x) * vector6.x;
							}
							if (border.w != 0f && vector7.y >= vector2.w)
							{
								_layers[i].shapes[j].vertices[k].y = vector4.w - (1f - vector7.y) * vector3.y;
							}
							else if (border.y != 0f && vector7.y <= vector2.y)
							{
								_layers[i].shapes[j].vertices[k].y = vector4.y + vector7.y * vector3.y;
							}
							else
							{
								_layers[i].shapes[j].vertices[k].y = num4 - (1f - vector7.y - border.w) * vector6.y;
							}
						}
					}
				}
				return;
			}
			if (_cachedVertices == null)
			{
				if (_sharedMesh == null)
				{
					_sharedMesh = _vectorGraphics.sharedMesh;
				}
				Vector3[] vertices = _sharedMesh.vertices;
				if (vertices == null || vertices.Length == 0)
				{
					return;
				}
				_finalVertices = new Vector3[vertices.Length];
				_cachedVertices = (Vector3[])vertices.Clone();
			}
			int num9 = _cachedVertices.Length;
			for (int l = 0; l < num9; l++)
			{
				vector7.x = InverseLerp(bounds.min.x, bounds.max.x, _cachedVertices[l].x);
				vector7.y = InverseLerp(bounds.min.y, bounds.max.y, _cachedVertices[l].y);
				if (border.x != 0f && vector7.x <= vector2.x)
				{
					_finalVertices[l].x = vector4.x + vector7.x * vector3.x;
				}
				else if (border.z != 0f && vector7.x >= vector2.z)
				{
					_finalVertices[l].x = vector4.z - (1f - vector7.x) * vector3.x;
				}
				else
				{
					_finalVertices[l].x = num5 + (vector7.x - border.x) * vector6.x;
				}
				if (border.w != 0f && vector7.y >= vector2.w)
				{
					_finalVertices[l].y = vector4.w - (1f - vector7.y) * vector3.y;
				}
				else if (border.y != 0f && vector7.y <= vector2.y)
				{
					_finalVertices[l].y = vector4.y + vector7.y * vector3.y;
				}
				else
				{
					_finalVertices[l].y = num4 - (1f - vector7.y - border.w) * vector6.y;
				}
			}
			_mesh.vertices = _finalVertices;
			meshFilter.sharedMesh = _mesh;
		}

		internal bool AtlasContainsMaterial(Material material)
		{
			return SVGAtlas.Instance.ContainsMaterial(material);
		}

		protected void SwapMaterials(bool transparent = true)
		{
			if (_vectorGraphics == null)
			{
				CleanMaterials();
				return;
			}
			bool hasGradients = _vectorGraphics.hasGradients || _vectorGraphics.useGradients == SVGUseGradients.Always;
			Material firstMaterial = SVGAtlas.Instance.GetOpaqueMaterial(hasGradients);
			Material material = SVGAtlas.Instance.GetTransparentMaterial(_vectorGraphics.antialiasing, hasGradients);
			int subMeshCount = 0;
			if (useLayers)
			{
				subMeshCount = _mesh.subMeshCount;
			}
			else if (_sharedMesh != null)
			{
				subMeshCount = _sharedMesh.subMeshCount;
			}
			if (_vectorGraphics.isOpaque)
			{
				if (transparent)
				{
					if (_transparentMaterial != null)
					{
						SetSharedMaterials(subMeshCount, _transparentMaterial, _transparentMaterial);
					}
					else
					{
						SetSharedMaterials(subMeshCount, material, material);
					}
				}
				else if (_opaqueMaterial == null && _transparentMaterial == null)
				{
					SetSharedMaterials(subMeshCount, firstMaterial, material);
				}
				else if (_opaqueMaterial != null && _transparentMaterial != null)
				{
					SetSharedMaterials(subMeshCount, _opaqueMaterial, _transparentMaterial);
				}
				else if (_transparentMaterial != null)
				{
					SetSharedMaterials(subMeshCount, firstMaterial, _transparentMaterial);
				}
				else if (_opaqueMaterial != null)
				{
					SetSharedMaterials(subMeshCount, _opaqueMaterial, material);
				}
			}
			else if (_transparentMaterial == null)
			{
				SetSharedMaterials(subMeshCount, material, material);
			}
			else
			{
				SetSharedMaterials(subMeshCount, _transparentMaterial, _transparentMaterial);
			}
		}

		private void SetSharedMaterials(int subMeshCount, Material firstMaterial, Material secondMaterial)
		{
			if (subMeshCount < 2)
			{
				meshRenderer.sharedMaterials = new Material[1] { firstMaterial };
			}
			else
			{
				meshRenderer.sharedMaterials = new Material[2] { firstMaterial, secondMaterial };
			}
		}

		public void UpdateMaterials()
		{
			if (_opaqueMaterial != null)
			{
				SVGAtlas.Instance.UpdateMaterialProperties(_opaqueMaterial);
			}
			if (_transparentMaterial != null)
			{
				SVGAtlas.Instance.UpdateMaterialProperties(_transparentMaterial);
			}
			SwapMaterials(_color.a != 1f);
			if (_opaqueMaterial.name == "AlphaBlend_SVGColorApply" && meshRenderer.material != null)
			{
				meshRenderer.material.SetColor("_Color", _color);
			}
		}

		public void SetAllDirty()
		{
			if (!meshRenderer.isPartOfStaticBatch)
			{
				PrepareForRendering(force: true);
			}
		}

		private void EnableMeshRenderer(bool value)
		{
			if (!meshRenderer.isPartOfStaticBatch)
			{
				meshRenderer.enabled = value;
			}
		}

		private void InitMesh()
		{
			if (_vectorGraphics == null)
			{
				_lastVectorGraphics = null;
				Clear();
				return;
			}
			if (useLayers)
			{
				_layers = _vectorGraphics.layersClone;
				if (_mesh == null)
				{
					_mesh = new Mesh();
					_mesh.hideFlags = HideFlags.DontSave;
				}
				else
				{
					_mesh.Clear();
				}
				_mesh.name = _vectorGraphics.name + " Instance " + _mesh.GetInstanceID();
				meshFilter.sharedMesh = _mesh;
				return;
			}
			CleanMesh();
			if (_sharedMesh != _vectorGraphics.sharedMesh)
			{
				_sharedMesh = _vectorGraphics.sharedMesh;
			}
			if (useSharedMesh)
			{
				if (meshFilter.sharedMesh != _sharedMesh)
				{
					meshFilter.sharedMesh = _sharedMesh;
				}
				return;
			}
			if (_mesh == null)
			{
				_mesh = new Mesh();
				_mesh.hideFlags = HideFlags.DontSave;
			}
			else
			{
				_mesh.Clear();
			}
			SVGMeshUtils.Fill(_vectorGraphics.sharedMesh, _mesh);
			Mesh mesh = _mesh;
			mesh.name = mesh.name + " Instance " + _mesh.GetInstanceID();
			if (meshFilter.sharedMesh != _mesh)
			{
				meshFilter.sharedMesh = _mesh;
			}
		}

		public void AddModifier(ISVGModify modifier)
		{
			if (!_modifiers.Contains(modifier))
			{
				_modifiers.Add(modifier);
			}
		}

		public void RemoveModifier(ISVGModify modifier)
		{
			if (_modifiers.Contains(modifier))
			{
				_modifiers.Remove(modifier);
			}
		}

		protected void Clear()
		{
			CleanMaterials();
			CleanMesh();
			CleanLayers();
			CleanCache();
		}

		private void CleanMaterials()
		{
			meshRenderer.sharedMaterials = new Material[0];
		}

		private void CleanMesh()
		{
			if (_mesh != null)
			{
				_mesh.Clear();
			}
		}

		private void CleanLayers()
		{
			if (_layers != null)
			{
				_layers = null;
			}
		}

		private void CleanCache()
		{
			if (_cachedColors != null)
			{
				_cachedColors = null;
			}
			if (_finalColors != null)
			{
				_finalColors = null;
			}
			if (_cachedVertices != null)
			{
				_cachedVertices = null;
			}
			if (_finalVertices != null)
			{
				_finalVertices = null;
			}
		}

		private void DestroyArray<T>(T[] array) where T : UnityEngine.Object
		{
			if (array == null)
			{
				return;
			}
			foreach (T val in array)
			{
				if (!(val == null))
				{
					DestroyObjectInternal(val);
				}
			}
		}

		private void DestroyObjectInternal(UnityEngine.Object obj)
		{
			if (!(obj == null))
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
	}
}
