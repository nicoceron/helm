using System;
using System.Collections.Generic;
using SVGImporter.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[AddComponentMenu("UI/SVG Image", 21)]
	public class SVGImage : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter, ISVGRenderer, ISVGReference
	{
		public enum Type
		{
			Simple = 0,
			Sliced = 1
		}

		[FormerlySerializedAs("vectorGraphics")]
		[SerializeField]
		protected SVGAsset _vectorGraphics;

		protected SVGAsset _lastVectorGraphics;

		[SerializeField]
		private Type m_Type;

		[SerializeField]
		private bool m_PreserveAspect;

		[SerializeField]
		private bool m_UsePivot;

		private float m_EventAlphaThreshold = 1f;

		protected Material _defaultMaterial;

		protected List<ISVGModify> _modifiers = new List<ISVGModify>();

		protected int _lastFrameChanged;

		protected Action<SVGLayer[], SVGAsset, bool> _OnPrepareForRendering;

		private const float epsilon = 1E-07f;

		private int tempVBOLength;

		private UIVertex[] vertexStream;

		private Vector3[] vertices;

		private int[] triangles;

		private Vector2[] uv;

		private Vector2[] uv2;

		private Vector2[] uv3;

		private Color32[] colors;

		private Vector3[] normals;

		public SVGAsset vectorGraphics
		{
			get
			{
				return _vectorGraphics;
			}
			set
			{
				if (SVGPropertyUtility.SetClass(ref _vectorGraphics, value))
				{
					Clear();
					UpdateMaterial();
					SetAllDirty();
				}
			}
		}

		public Type type
		{
			get
			{
				return m_Type;
			}
			set
			{
				if (SVGPropertyUtility.SetStruct(ref m_Type, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public bool preserveAspect
		{
			get
			{
				return m_PreserveAspect;
			}
			set
			{
				if (SVGPropertyUtility.SetStruct(ref m_PreserveAspect, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public bool usePivot
		{
			get
			{
				return m_UsePivot;
			}
			set
			{
				if (SVGPropertyUtility.SetStruct(ref m_UsePivot, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public float eventAlphaThreshold
		{
			get
			{
				return m_EventAlphaThreshold;
			}
			set
			{
				m_EventAlphaThreshold = value;
			}
		}

		public List<ISVGModify> modifiers => _modifiers;

		public int lastFrameChanged => _lastFrameChanged;

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

		private bool useLayers => _vectorGraphics.useLayers;

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

		public float pixelsPerUnit => 100f;

		protected Mesh sharedMesh
		{
			get
			{
				if (_vectorGraphics == null)
				{
					return null;
				}
				return _vectorGraphics.sharedMesh;
			}
		}

		public override Material defaultMaterial
		{
			get
			{
				GetDefaultMaterial();
				return _defaultMaterial;
			}
		}

		public virtual float minWidth => 0f;

		public virtual float preferredWidth
		{
			get
			{
				if (sharedMesh == null)
				{
					return 0f;
				}
				return sharedMesh.bounds.size.x / pixelsPerUnit;
			}
		}

		public virtual float flexibleWidth => -1f;

		public virtual float minHeight => 0f;

		public virtual float preferredHeight
		{
			get
			{
				if (sharedMesh == null)
				{
					return 0f;
				}
				return sharedMesh.bounds.size.y / pixelsPerUnit;
			}
		}

		public virtual float flexibleHeight => -1f;

		public virtual int layoutPriority => 0;

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

		public void UpdateRenderer()
		{
			SetAllDirty();
		}

		protected override void Awake()
		{
			Clear();
			UpdateMaterial();
			if (_vectorGraphics != null)
			{
				_vectorGraphics.AddReference(this);
			}
			base.Awake();
		}

		protected override void OnDestroy()
		{
			if (_vectorGraphics != null)
			{
				_vectorGraphics.RemoveReference(this);
			}
			base.OnDestroy();
		}

		private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
		{
			Vector2 vector = ((sharedMesh == null) ? Vector2.zero : ((Vector2)sharedMesh.bounds.size));
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			if (shouldPreserveAspect && vector.sqrMagnitude > 0f)
			{
				float num = vector.x / vector.y;
				float num2 = pixelAdjustedRect.width / pixelAdjustedRect.height;
				if (num > num2)
				{
					float height = pixelAdjustedRect.height;
					pixelAdjustedRect.height = pixelAdjustedRect.width * (1f / num);
					pixelAdjustedRect.y += (height - pixelAdjustedRect.height) * base.rectTransform.pivot.y;
				}
				else
				{
					float width = pixelAdjustedRect.width;
					pixelAdjustedRect.width = pixelAdjustedRect.height * num;
					pixelAdjustedRect.x += (width - pixelAdjustedRect.width) * base.rectTransform.pivot.x;
				}
			}
			return new Vector4(pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.width, pixelAdjustedRect.height);
		}

		public override void SetNativeSize()
		{
			if (sharedMesh != null)
			{
				Vector2 vector = sharedMesh.bounds.size * 1000f;
				float x = vector.x / pixelsPerUnit;
				float y = vector.y / pixelsPerUnit;
				base.rectTransform.anchorMax = base.rectTransform.anchorMin;
				base.rectTransform.sizeDelta = new Vector2(x, y);
				SetAllDirty();
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

		protected float Lerp(float from, float to, float value)
		{
			return from + value * (to - from);
		}

		public virtual void CalculateLayoutInputHorizontal()
		{
		}

		public virtual void CalculateLayoutInputVertical()
		{
		}

		public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
		{
			if (m_EventAlphaThreshold >= 1f)
			{
				return true;
			}
			_ = sharedMesh == null;
			return true;
		}

		private Vector2 MapCoordinate(Vector2 local, Rect rect)
		{
			Bounds bounds = sharedMesh.bounds;
			return new Vector2(local.x * bounds.size.x / rect.width, local.y * bounds.size.y / rect.height);
		}

		public override void SetMaterialDirty()
		{
			if (IsActive())
			{
				SVGAtlas.Instance.UpdateMaterialProperties(m_Material);
			}
			base.SetMaterialDirty();
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

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (sharedMesh == null)
			{
				base.OnPopulateMesh(vh);
				return;
			}
			vh.Clear();
			Mesh mesh = sharedMesh;
			tempVBOLength = mesh.vertexCount;
			vertices = mesh.vertices;
			triangles = mesh.triangles;
			uv = mesh.uv;
			uv2 = mesh.uv2;
			colors = mesh.colors32;
			normals = mesh.normals;
			if (vertexStream == null || vertexStream.Length != tempVBOLength)
			{
				vertexStream = new UIVertex[tempVBOLength];
			}
			if (_vectorGraphics.antialiasing || _vectorGraphics.generateNormals)
			{
				base.canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.Normal;
			}
			else
			{
				base.canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
			}
			Bounds bounds = sharedMesh.bounds;
			if (m_UsePivot)
			{
				bounds.center += new Vector3((-0.5f + _vectorGraphics.pivotPoint.x) * bounds.size.x, (0.5f - _vectorGraphics.pivotPoint.y) * bounds.size.y, 0f);
			}
			if (m_Type == Type.Simple)
			{
				Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
				for (int i = 0; i < tempVBOLength; i++)
				{
					vertexStream[i].position.x = drawingDimensions.x + InverseLerp(bounds.min.x, bounds.max.x, vertices[i].x) * drawingDimensions.z;
					vertexStream[i].position.y = drawingDimensions.y + InverseLerp(bounds.min.y, bounds.max.y, vertices[i].y) * drawingDimensions.w;
					vertexStream[i].color = colors[i] * color;
				}
			}
			else
			{
				Vector4 drawingDimensions2 = GetDrawingDimensions(shouldPreserveAspect: false);
				Vector4 border = _vectorGraphics.border;
				Vector4 vector = new Vector4(border.x + 1E-07f, border.y + 1E-07f, 1f - border.z - 1E-07f, 1f - border.w - 1E-07f);
				float num = base.canvas.referencePixelsPerUnit * vectorGraphics.scale * 100f;
				Vector2 vector2 = new Vector2(bounds.size.x * num, bounds.size.y * num);
				Vector4 vector3 = new Vector4(drawingDimensions2.x, drawingDimensions2.y, drawingDimensions2.x + drawingDimensions2.z, drawingDimensions2.y + drawingDimensions2.w);
				Vector4 vector4 = new Vector4(vector2.x * border.x, vector2.y * border.y, vector2.x * border.z, vector2.y * border.w);
				Vector2 vector5 = new Vector2(SafeDivide(1f, 1f - (border.x + border.z)) * (drawingDimensions2.z - (vector4.x + vector4.z)), SafeDivide(1f, 1f - (border.y + border.w)) * (drawingDimensions2.w - (vector4.w + vector4.y)));
				float num2 = vector4.x + vector4.z;
				if (num2 != 0f)
				{
					num2 = Mathf.Clamp01(drawingDimensions2.z / num2);
					if (num2 != 1f)
					{
						vector5.x = 0f;
						vector2.x *= num2;
						vector4.x *= num2;
						vector4.z *= num2;
					}
				}
				float num3 = vector4.w + vector4.y;
				if (num3 != 0f)
				{
					num3 = Mathf.Clamp01(drawingDimensions2.w / num3);
					if (num3 != 1f)
					{
						vector5.y = 0f;
						vector2.y *= num3;
						vector4.w *= num3;
						vector4.y *= num3;
					}
				}
				float num4 = vector3.w - vector4.w;
				float num5 = vector3.x + vector4.x;
				Vector2 vector6 = default(Vector2);
				for (int j = 0; j < tempVBOLength; j++)
				{
					vertexStream[j].color = colors[j] * color;
					vector6.x = InverseLerp(bounds.min.x, bounds.max.x, vertices[j].x);
					vector6.y = InverseLerp(bounds.min.y, bounds.max.y, vertices[j].y);
					if (border.x != 0f && vector6.x <= vector.x)
					{
						vertexStream[j].position.x = vector3.x + vector6.x * vector2.x;
					}
					else if (border.z != 0f && vector6.x >= vector.z)
					{
						vertexStream[j].position.x = vector3.z - (1f - vector6.x) * vector2.x;
					}
					else
					{
						vertexStream[j].position.x = num5 + (vector6.x - border.x) * vector5.x;
					}
					if (border.w != 0f && vector6.y >= vector.w)
					{
						vertexStream[j].position.y = vector3.w - (1f - vector6.y) * vector2.y;
					}
					else if (border.y != 0f && vector6.y <= vector.y)
					{
						vertexStream[j].position.y = vector3.y + vector6.y * vector2.y;
					}
					else
					{
						vertexStream[j].position.y = num4 - (1f - vector6.y - border.w) * vector5.y;
					}
				}
			}
			if ((_vectorGraphics.hasGradients || _vectorGraphics.useGradients == SVGUseGradients.Always) && uv != null && uv2 != null && tempVBOLength == uv.Length && tempVBOLength == uv2.Length)
			{
				for (int k = 0; k < tempVBOLength; k++)
				{
					vertexStream[k].uv0 = uv[k];
					vertexStream[k].uv1 = uv2[k];
				}
			}
			if (_vectorGraphics.antialiasing)
			{
				if (_vectorGraphics.antialiasing && normals != null && tempVBOLength == normals.Length)
				{
					for (int l = 0; l < tempVBOLength; l++)
					{
						vertexStream[l].normal.x = normals[l].x;
						vertexStream[l].normal.y = normals[l].y;
					}
				}
			}
			else if (_vectorGraphics.generateNormals && normals != null && normals.Length == tempVBOLength)
			{
				for (int m = 0; m < tempVBOLength; m++)
				{
					vertexStream[m].normal = normals[m];
				}
			}
			vh.AddUIVertexStream(new List<UIVertex>(vertexStream), new List<int>(triangles));
			_lastFrameChanged = Time.frameCount;
		}

		protected void GetDefaultMaterial()
		{
			if (_lastVectorGraphics != _vectorGraphics)
			{
				if (_lastVectorGraphics != null)
				{
					_lastVectorGraphics.RemoveReference(this);
				}
				if (_vectorGraphics != null)
				{
					_vectorGraphics.AddReference(this);
				}
				_lastVectorGraphics = _vectorGraphics;
				Clear();
			}
			if (_vectorGraphics != null && _defaultMaterial == null)
			{
				_defaultMaterial = _vectorGraphics.sharedUIMaterial;
			}
		}

		protected void Clear()
		{
			_defaultMaterial = null;
		}

		protected override void UpdateMaterial()
		{
			GetDefaultMaterial();
			base.UpdateMaterial();
		}
	}
}
