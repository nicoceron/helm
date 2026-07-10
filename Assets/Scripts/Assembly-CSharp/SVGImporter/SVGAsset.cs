using System;
using System.Collections.Generic;
using SVGImporter.Document;
using SVGImporter.Geometry;
using SVGImporter.Rendering;
using SVGImporter.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SVGImporter
{
	[Serializable]
	public class SVGAsset : ScriptableObject
	{
		[FormerlySerializedAs("lastTimeModified")]
		[SerializeField]
		protected long _lastTimeModified;

		[FormerlySerializedAs("documentAsset")]
		[SerializeField]
		protected SVGDocumentAsset _documentAsset;

		[FormerlySerializedAs("sharedMesh")]
		[SerializeField]
		protected Mesh _sharedMesh;

		protected Mesh _runtimeMesh;

		protected Mesh _runtimeLegacyUIMesh;

		protected Material[] _runtimeMaterials;

		[FormerlySerializedAs("antialiasing")]
		[SerializeField]
		protected bool _antialiasing;

		[FormerlySerializedAs("generateCollider")]
		[SerializeField]
		protected bool _generateCollider;

		[FormerlySerializedAs("keepSVGFile")]
		[SerializeField]
		protected bool _keepSVGFile = true;

		[FormerlySerializedAs("ignoreSVGCanvas")]
		[SerializeField]
		protected bool _ignoreSVGCanvas = true;

		[FormerlySerializedAs("colliderShape")]
		[SerializeField]
		protected SVGPath[] _colliderShape;

		[FormerlySerializedAs("format")]
		[SerializeField]
		protected SVGAssetFormat _format = SVGAssetFormat.Transparent;

		[FormerlySerializedAs("useGradients")]
		[SerializeField]
		protected SVGUseGradients _useGradients;

		[FormerlySerializedAs("meshCompression")]
		[SerializeField]
		protected SVGMeshCompression _meshCompression;

		[FormerlySerializedAs("optimizeMesh")]
		[SerializeField]
		protected bool _optimizeMesh = true;

		[FormerlySerializedAs("generateNormals")]
		[SerializeField]
		protected bool _generateNormals;

		[FormerlySerializedAs("generateTangents")]
		[SerializeField]
		protected bool _generateTangents;

		[FormerlySerializedAs("scale")]
		[SerializeField]
		protected float _scale = 0.01f;

		[FormerlySerializedAs("vpm")]
		[SerializeField]
		protected float _vpm = 1000f;

		[FormerlySerializedAs("depthOffset")]
		[SerializeField]
		protected float _depthOffset = 0.01f;

		[FormerlySerializedAs("compressDepth")]
		[SerializeField]
		protected bool _compressDepth = true;

		[FormerlySerializedAs("pivotPoint")]
		[SerializeField]
		protected Vector2 _pivotPoint = new Vector2(0.5f, 0.5f);

		[FormerlySerializedAs("customPivotPoint")]
		[SerializeField]
		protected bool _customPivotPoint;

		[FormerlySerializedAs("border")]
		[SerializeField]
		protected Vector4 _border = new Vector4(0f, 0f, 0f, 0f);

		[FormerlySerializedAs("sliceMesh")]
		[SerializeField]
		protected bool _sliceMesh;

		protected string _svgFile;

		[FormerlySerializedAs("sharedGradients")]
		[SerializeField]
		protected CCGradient[] _sharedGradients;

		[FormerlySerializedAs("sharedShaders")]
		[SerializeField]
		protected string[] _sharedShaders;

		[FormerlySerializedAs("canvasRectangle")]
		[SerializeField]
		protected Rect _canvasRectangle;

		[FormerlySerializedAs("useLayers")]
		[SerializeField]
		protected bool _useLayers;

		[FormerlySerializedAs("layers")]
		[SerializeField]
		protected SVGLayer[] _layers;

		public Mesh sharedMesh => runtimeMesh;

		public bool isOpaque
		{
			get
			{
				if (_format == SVGAssetFormat.Transparent || _format == SVGAssetFormat.uGUI)
				{
					return false;
				}
				if (_sharedShaders == null || _sharedShaders.Length == 0)
				{
					return true;
				}
				for (int i = 0; i < _sharedShaders.Length; i++)
				{
					if (!string.IsNullOrEmpty(_sharedShaders[i]) && _sharedShaders[i].ToLower().Contains("opaque"))
					{
						return true;
					}
				}
				return false;
			}
		}

		public Mesh mesh
		{
			get
			{
				Mesh mesh = sharedMesh;
				if (mesh == null)
				{
					return null;
				}
				Mesh mesh2 = SVGMeshUtils.Clone(mesh);
				if (mesh2 != null)
				{
					mesh2.name = mesh2.name + " Instance " + mesh2.GetInstanceID();
				}
				return mesh2;
			}
		}

		protected Mesh runtimeMesh
		{
			get
			{
				if (!hasGradients)
				{
					return _sharedMesh;
				}
				if (_runtimeMesh == null && _sharedMesh != null)
				{
					Dictionary<int, int> dictionary = new Dictionary<int, int>();
					CCGradient[] array = new CCGradient[_sharedGradients.Length];
					for (int i = 0; i < _sharedGradients.Length; i++)
					{
						if (_sharedGradients[i] != null)
						{
							CCGradient gradient = SVGAtlas.Instance.GetGradient(_sharedGradients[i]);
							if (gradient != null)
							{
								array[i] = gradient;
							}
							else
							{
								array[i] = SVGAtlas.Instance.AddGradient(_sharedGradients[i].Clone());
							}
							dictionary.Add(_sharedGradients[i].index, array[i].index);
						}
					}
					_runtimeMesh = SVGMeshUtils.Clone(_sharedMesh);
					_runtimeMesh.hideFlags = HideFlags.DontSave;
					if (_runtimeMesh.uv2 != null && _runtimeMesh.uv2.Length != 0)
					{
						Vector2[] uv = _runtimeMesh.uv2;
						for (int j = 0; j < uv.Length; j++)
						{
							int key = Mathf.FloorToInt(Mathf.Abs(uv[j].x));
							try
							{
								uv[j].x = dictionary[key];
							}
							catch
							{
							}
						}
						_runtimeMesh.uv2 = uv;
					}
				}
				return _runtimeMesh;
			}
		}

		protected Mesh runtimeLegacyUIMesh
		{
			get
			{
				if (_runtimeLegacyUIMesh == null)
				{
					_runtimeLegacyUIMesh = CreateLegacyUIMesh(sharedMesh);
				}
				return _runtimeLegacyUIMesh;
			}
		}

		public Mesh sharedLegacyUIMesh => runtimeLegacyUIMesh;

		public Material sharedUIMaterial
		{
			get
			{
				if (_antialiasing)
				{
					return SVGAtlas.Instance.uiAntialiased;
				}
				return SVGAtlas.Instance.ui;
			}
		}

		public Material uiMaterial
		{
			get
			{
				if (_antialiasing)
				{
					return CloneMaterial(SVGAtlas.Instance.uiAntialiased);
				}
				return CloneMaterial(SVGAtlas.Instance.ui);
			}
		}

		public Material[] sharedMaterials => runtimeMaterials;

		public Material[] materials
		{
			get
			{
				if (sharedMaterials == null)
				{
					return null;
				}
				int num = sharedMaterials.Length;
				Material[] array = new Material[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = CloneMaterial(sharedMaterials[i]);
				}
				return array;
			}
		}

		public Material[] runtimeMaterials
		{
			get
			{
				bool flag = false;
				if (_runtimeMaterials != null && _runtimeMaterials.Length != 0)
				{
					for (int i = 0; i < _runtimeMaterials.Length; i++)
					{
						if (!(_runtimeMaterials[i] != null))
						{
							flag = true;
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					if (_sharedShaders != null && _sharedShaders.Length != 0)
					{
						_runtimeMaterials = new Material[_sharedShaders.Length];
						for (int j = 0; j < _sharedShaders.Length; j++)
						{
							if (_sharedShaders[j] != null)
							{
								string text = _sharedShaders[j];
								if (text == SVGShader.SolidColorOpaque.name)
								{
									_runtimeMaterials[j] = SVGAtlas.Instance.opaqueSolid;
								}
								else if (text == SVGShader.SolidColorAlphaBlended.name)
								{
									_runtimeMaterials[j] = SVGAtlas.Instance.transparentSolid;
								}
								else if (text == SVGShader.SolidColorAlphaBlendedAntialiased.name)
								{
									_runtimeMaterials[j] = SVGAtlas.Instance.transparentSolidAntialiased;
								}
								else if (text == SVGShader.GradientColorOpaque.name)
								{
									_runtimeMaterials[j] = SVGAtlas.Instance.opaqueGradient;
								}
								else if (text == SVGShader.GradientColorAlphaBlended.name)
								{
									_runtimeMaterials[j] = SVGAtlas.Instance.transparentGradient;
								}
								else if (text == SVGShader.GradientColorAlphaBlendedAntialiased.name)
								{
									_runtimeMaterials[j] = SVGAtlas.Instance.transparentGradientAntialiased;
								}
							}
						}
					}
					else
					{
						_runtimeMaterials = new Material[0];
					}
				}
				return _runtimeMaterials;
			}
		}

		public bool antialiasing => _antialiasing;

		public bool generateCollider => _generateCollider;

		public bool keepSVGFile => _keepSVGFile;

		public bool ignoreSVGCanvas => _ignoreSVGCanvas;

		public SVGPath[] colliderShape => _colliderShape;

		public SVGAssetFormat format => _format;

		public SVGUseGradients useGradients => _useGradients;

		public SVGMeshCompression meshCompression => _meshCompression;

		public bool optimizeMesh => _optimizeMesh;

		public bool generateNormals => _generateNormals;

		public bool generateTangents => _generateTangents;

		public float scale => _scale;

		public float vpm => _vpm;

		public float depthOffset => _depthOffset;

		public bool compressDepth => _compressDepth;

		public Vector2 pivotPoint => _pivotPoint;

		public bool customPivotPoint => _customPivotPoint;

		public Vector4 border => _border;

		public bool sliceMesh => _sliceMesh;

		public string svgFile
		{
			get
			{
				if (!string.IsNullOrEmpty(_svgFile))
				{
					return _svgFile;
				}
				if (_documentAsset != null)
				{
					return _documentAsset.svgFile;
				}
				return null;
			}
		}

		public CCGradient[] sharedGradients => _sharedGradients;

		public string[] sharedShaders => _sharedShaders;

		public Bounds bounds
		{
			get
			{
				if (_sharedMesh == null)
				{
					return default(Bounds);
				}
				return _sharedMesh.bounds;
			}
		}

		public Rect canvasRectangle => _canvasRectangle;

		public bool useLayers => _useLayers;

		public SVGLayer[] layers => _layers;

		public SVGLayer[] layersClone
		{
			get
			{
				if (_layers == null)
				{
					return null;
				}
				int num = _layers.Length;
				SVGLayer[] array = new SVGLayer[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = _layers[i].Clone();
				}
				return array;
			}
		}

		public bool hasGradients
		{
			get
			{
				if (_sharedGradients == null || _sharedGradients.Length == 0)
				{
					return false;
				}
				if (_sharedGradients.Length == 1 && _sharedGradients[0].hash == "GC999FFFFFFC000FFFFFFA999999A000999")
				{
					return false;
				}
				return true;
			}
		}

		public int uiVertexCount
		{
			get
			{
				if (_sharedMesh == null || _sharedMesh.triangles == null)
				{
					return 0;
				}
				int num = _sharedMesh.triangles.Length;
				return num + num / 3;
			}
		}

		public void AddReference(ISVGReference reference)
		{
			if (!hasGradients || SVGAtlas.beingDestroyed)
			{
				return;
			}
			for (int i = 0; i < _sharedGradients.Length; i++)
			{
				if (_sharedGradients[i] != null)
				{
					CCGradient gradient = SVGAtlas.Instance.GetGradient(_sharedGradients[i]);
					if (gradient != null)
					{
						gradient.AddReference(reference);
						continue;
					}
					gradient = SVGAtlas.Instance.AddGradient(_sharedGradients[i].Clone());
					gradient.AddReference(reference);
				}
			}
		}

		public void RemoveReference(ISVGReference reference)
		{
			if (!hasGradients)
			{
				return;
			}
			int num = 0;
			if (SVGAtlas.beingDestroyed)
			{
				return;
			}
			for (int i = 0; i < _sharedGradients.Length; i++)
			{
				if (_sharedGradients[i] == null)
				{
					continue;
				}
				CCGradient gradient = SVGAtlas.Instance.GetGradient(_sharedGradients[i]);
				if (gradient != null)
				{
					gradient.RemoveReference(reference);
					if (gradient.referenceCount == 0)
					{
						SVGAtlas.Instance.RemoveGradient(gradient);
					}
					num += gradient.CountReferences(reference);
				}
			}
			if (num == 0)
			{
				if (_runtimeMesh != null)
				{
					_runtimeMesh.Clear();
					_runtimeMesh = null;
				}
				if (_runtimeMaterials != null)
				{
					_runtimeMaterials = null;
				}
			}
		}

		protected Material CloneMaterial(Material original)
		{
			if (original == null)
			{
				return null;
			}
			Material material = new Material(original.shader);
			material.CopyPropertiesFromMaterial(original);
			return material;
		}

		protected static Mesh CreateLegacyUIMesh(Mesh inputMesh)
		{
			if (inputMesh == null)
			{
				return null;
			}
			Mesh mesh = new Mesh();
			Vector3[] vertices = inputMesh.vertices;
			Color32[] colors = inputMesh.colors32;
			Vector2[] uv = inputMesh.uv;
			Vector2[] uv2 = inputMesh.uv2;
			Vector3[] normals = inputMesh.normals;
			Vector4[] tangents = inputMesh.tangents;
			int[] triangles = inputMesh.triangles;
			int num = triangles.Length;
			int num2 = num + num / 3;
			Vector3[] array = new Vector3[num2];
			Color32[] array2 = new Color32[num2];
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < num; i += 3)
			{
				num4 = triangles[i];
				array[num3] = vertices[num4];
				array2[num3] = colors[num4];
				num3++;
				num4 = triangles[i + 1];
				array[num3] = vertices[num4];
				array2[num3] = colors[num4];
				num3++;
				num4 = triangles[i + 2];
				array[num3] = vertices[num4];
				array2[num3] = colors[num4];
				num3++;
			}
			mesh.vertices = array;
			mesh.colors32 = array2;
			if (uv != null && uv.Length != 0 && uv2 != null && uv2.Length != 0)
			{
				num3 = 0;
				num4 = 0;
				Vector2[] array3 = new Vector2[num2];
				Vector2[] array4 = new Vector2[num2];
				for (int j = 0; j < num; j += 3)
				{
					num4 = triangles[j];
					array3[num3] = uv[num4];
					array4[num3] = uv2[num4];
					num3++;
					num4 = triangles[j + 1];
					array3[num3] = uv[num4];
					array4[num3] = uv2[num4];
					num3++;
					num4 = triangles[j + 2];
					array3[num3] = uv[num4];
					array4[num3] = uv2[num4];
					num3++;
				}
				mesh.uv = array3;
				mesh.uv2 = array4;
			}
			if (normals != null && normals.Length != 0)
			{
				num3 = 0;
				num4 = 0;
				Vector3[] array5 = new Vector3[num2];
				for (int k = 0; k < num; k += 3)
				{
					num4 = triangles[k];
					array5[num3] = normals[num4];
					num3++;
					num4 = triangles[k + 1];
					array5[num3] = normals[num4];
					num3++;
					num4 = triangles[k + 2];
					array5[num3] = normals[num4];
					num3++;
				}
				mesh.normals = array5;
			}
			if (tangents != null && tangents.Length != 0)
			{
				num3 = 0;
				num4 = 0;
				Vector4[] array6 = new Vector4[num2];
				for (int l = 0; l < num; l += 3)
				{
					num4 = triangles[l];
					array6[num3] = tangents[num4];
					num3++;
					num4 = triangles[l + 1];
					array6[num3] = tangents[num4];
					num3++;
					num4 = triangles[l + 2];
					array6[num3] = tangents[num4];
					num3++;
				}
				mesh.tangents = array6;
			}
			return mesh;
		}

		public static SVGAsset Load(string svgText, SVGImporterSettings settings = null)
		{
			if (string.IsNullOrEmpty(svgText))
			{
				return null;
			}
			if (settings == null)
			{
				SVGAssetImport.format = SVGAssetFormat.Transparent;
				SVGAssetImport.pivotPoint = new Vector2(0.5f, 0.5f);
				SVGAssetImport.meshScale = 0.01f;
				SVGAssetImport.border = new Vector4(0f, 0f, 0f, 0f);
				SVGAssetImport.sliceMesh = false;
				SVGAssetImport.minDepthOffset = 0.01f;
				SVGAssetImport.compressDepth = true;
				SVGAssetImport.ignoreSVGCanvas = true;
				SVGAssetImport.useGradients = SVGUseGradients.Always;
			}
			else
			{
				SVGAssetImport.format = settings.defaultSVGFormat;
				SVGAssetImport.pivotPoint = settings.defaultPivotPoint;
				SVGAssetImport.meshScale = settings.defaultScale;
				SVGAssetImport.border = new Vector4(0f, 0f, 0f, 0f);
				SVGAssetImport.sliceMesh = false;
				SVGAssetImport.minDepthOffset = settings.defaultDepthOffset;
				SVGAssetImport.compressDepth = settings.defaultCompressDepth;
				SVGAssetImport.ignoreSVGCanvas = settings.defaultIgnoreSVGCanvas;
				SVGAssetImport.useGradients = settings.defaultUseGradients;
				SVGAssetImport.antialiasing = settings.defaultAntialiasing;
			}
			SVGGraphics r = new SVGGraphics(1000f, SVGAssetImport.antialiasing);
			SVGDocument sVGDocument = null;
			SVGAssetImport.Clear();
			SVGAssetImport.atlasData = new SVGAtlasData();
			SVGAssetImport.atlasData.Init(262144);
			SVGAssetImport.atlasData.AddGradient(SVGAtlasData.GetDefaultGradient());
			SVGParser.Init();
			SVGGraphics.Init();
			SVGElement sVGElement = null;
			List<SVGError> list = new List<SVGError>();
			sVGDocument = new SVGDocument(svgText, r);
			sVGElement = sVGDocument.rootElement;
			if (sVGElement == null)
			{
				Debug.LogError("SVG Document is corrupted!");
				return null;
			}
			SVGAsset sVGAsset = ScriptableObject.CreateInstance<SVGAsset>();
			sVGAsset._antialiasing = SVGAssetImport.antialiasing;
			sVGAsset._border = SVGAssetImport.border;
			sVGAsset._compressDepth = SVGAssetImport.compressDepth;
			sVGAsset._depthOffset = SVGAssetImport.minDepthOffset;
			sVGAsset._ignoreSVGCanvas = SVGAssetImport.ignoreSVGCanvas;
			sVGAsset._meshCompression = SVGMeshCompression.Off;
			sVGAsset._scale = SVGAssetImport.meshScale;
			sVGAsset._format = SVGAssetImport.format;
			sVGAsset._useGradients = SVGAssetImport.useGradients;
			sVGAsset._pivotPoint = SVGAssetImport.pivotPoint;
			sVGAsset._vpm = SVGAssetImport.vpm;
			sVGAsset._sharedGradients = null;
			if (settings != null)
			{
				sVGAsset._generateCollider = settings.defaultGenerateCollider;
				sVGAsset._generateNormals = settings.defaultGenerateNormals;
				sVGAsset._generateTangents = settings.defaultGenerateTangents;
				sVGAsset._sliceMesh = false;
				sVGAsset._optimizeMesh = settings.defaultOptimizeMesh;
				sVGAsset._keepSVGFile = settings.defaultKeepSVGFile;
			}
			else
			{
				sVGAsset._generateCollider = false;
				sVGAsset._generateNormals = false;
				sVGAsset._generateTangents = false;
				sVGAsset._sliceMesh = false;
				sVGAsset._optimizeMesh = true;
				sVGAsset._keepSVGFile = false;
			}
			try
			{
				sVGElement.Render();
				Rect viewport = sVGElement.paintable.viewport;
				viewport.x *= SVGAssetImport.meshScale;
				viewport.y *= SVGAssetImport.meshScale;
				viewport.size *= SVGAssetImport.meshScale;
				SVGGraphics.CorrectSVGLayers(SVGGraphics.layers, viewport, sVGAsset, out var offset);
				bool flag = sVGAsset.useGradients == SVGUseGradients.Always;
				Mesh mesh = new Mesh();
				SVGMesh.CombineMeshes(SVGGraphics.layers.ToArray(), mesh, out var shaders, sVGAsset._useGradients, sVGAsset._format, sVGAsset._compressDepth, sVGAsset._antialiasing);
				if (mesh == null)
				{
					return null;
				}
				if (sVGAsset._useGradients == SVGUseGradients.Always)
				{
					if (shaders != null)
					{
						for (int i = 0; i < shaders.Length; i++)
						{
							if (!(shaders[i] == null))
							{
								if (shaders[i].name == SVGShader.SolidColorOpaque.name)
								{
									shaders[i] = SVGShader.GradientColorOpaque;
								}
								else if (shaders[i].name == SVGShader.SolidColorAlphaBlended.name)
								{
									shaders[i] = SVGShader.GradientColorAlphaBlended;
								}
								else if (shaders[i].name == SVGShader.SolidColorAlphaBlendedAntialiased.name)
								{
									shaders[i] = SVGShader.GradientColorAlphaBlendedAntialiased;
								}
							}
						}
					}
					flag = true;
				}
				else if (shaders != null)
				{
					for (int j = 0; j < shaders.Length; j++)
					{
						if (!(shaders[j] == null) && (shaders[j].name == SVGShader.GradientColorOpaque.name || shaders[j].name == SVGShader.GradientColorAlphaBlended.name || shaders[j].name == SVGShader.GradientColorAlphaBlendedAntialiased.name))
						{
							flag = true;
							break;
						}
					}
				}
				if (!sVGAsset.useLayers)
				{
					sVGAsset._sharedMesh = mesh;
				}
				if (shaders != null && shaders.Length != 0)
				{
					sVGAsset._sharedShaders = new string[shaders.Length];
					if (flag)
					{
						for (int k = 0; k < shaders.Length; k++)
						{
							sVGAsset._sharedShaders[k] = shaders[k].name;
						}
					}
					else
					{
						for (int l = 0; l < shaders.Length; l++)
						{
							if (shaders[l].name == SVGShader.GradientColorAlphaBlended.name)
							{
								shaders[l] = SVGShader.SolidColorAlphaBlended;
							}
							else if (shaders[l].name == SVGShader.GradientColorAlphaBlendedAntialiased.name)
							{
								shaders[l] = SVGShader.SolidColorAlphaBlendedAntialiased;
							}
							else if (shaders[l].name == SVGShader.GradientColorOpaque.name)
							{
								shaders[l] = SVGShader.SolidColorOpaque;
							}
							sVGAsset._sharedShaders[l] = shaders[l].name;
						}
					}
				}
				sVGAsset._canvasRectangle = new Rect(viewport.x, viewport.y, viewport.size.x, viewport.size.y);
				if (sVGAsset.generateCollider && SVGGraphics.paths != null && SVGGraphics.paths.Count > 0)
				{
					List<List<Vector2>> list2 = new List<List<Vector2>>();
					for (int m = 0; m < SVGGraphics.paths.Count; m++)
					{
						Vector2[] points = SVGGraphics.paths[m].points;
						for (int n = 0; n < points.Length; n++)
						{
							points[n].x = points[n].x * SVGAssetImport.meshScale - offset.x;
							points[n].y = (points[n].y * SVGAssetImport.meshScale + offset.y) * -1f;
						}
						list2.Add(new List<Vector2>(points));
					}
					list2 = SVGGeom.MergePolygon(list2);
					SVGPath[] array = new SVGPath[list2.Count];
					for (int num = 0; num < list2.Count; num++)
					{
						array[num] = new SVGPath(list2[num].ToArray());
					}
					if (array != null && array.Length != 0)
					{
						sVGAsset._colliderShape = array;
					}
				}
				if (flag && SVGAssetImport.atlasData.gradientCache != null && SVGAssetImport.atlasData.gradientCache.Count > 0)
				{
					CCGradient[] array2 = new CCGradient[SVGAssetImport.atlasData.gradientCache.Count];
					int num2 = 0;
					foreach (KeyValuePair<string, CCGradient> item in SVGAssetImport.atlasData.gradientCache)
					{
						array2[num2++] = item.Value;
					}
					sVGAsset._sharedGradients = array2;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Asset Failed to import\n" + ex.Message);
				list.Add(SVGError.CorruptedFile);
			}
			sVGAsset._documentAsset = SVGDocumentAsset.CreateInstance(svgText, list.ToArray());
			sVGDocument?.Clear();
			SVGAssetImport.Clear();
			return sVGAsset;
		}
	}
}
