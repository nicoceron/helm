using System;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class StrokeRendererLegacy : MonoBehaviour
	{
		[Serializable]
		public struct StrokePoint
		{
			public Vector2 position;

			public Transform transform;

			public Vector2 GetPosition()
			{
				if (transform == null)
				{
					return position;
				}
				return transform.position;
			}
		}

		public StrokePoint[] points;

		[Header("Line Style")]
		public StrokeLineJoin lineJoin;

		public StrokeLineCap lineCap;

		public Color32 color = Color.white;

		public float width = 1f;

		public float mitterLimit = 4f;

		public float roundQuality = 10f;

		public float[] dashArray;

		public float dashOffset;

		public ClosePathRule closeLine;

		protected MeshFilter _meshFilter;

		protected MeshRenderer _meshRenderer;

		public MeshFilter meshFilter
		{
			get
			{
				if (_meshFilter == null)
				{
					_meshFilter = GetComponent<MeshFilter>();
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
					_meshRenderer = GetComponent<MeshRenderer>();
				}
				return _meshRenderer;
			}
		}

		private void LateUpdate()
		{
			if (points != null && points.Length > 1)
			{
				RenderStroke();
			}
		}

		protected virtual void RenderStroke()
		{
			int num = points.Length - 1;
			StrokeSegment[] array = new StrokeSegment[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new StrokeSegment(points[i].GetPosition(), points[i + 1].GetPosition());
			}
			meshFilter.sharedMesh = SVGLineUtils.StrokeMesh(array, width, color, lineJoin, lineCap, mitterLimit, dashArray, dashOffset, closeLine, roundQuality);
		}
	}
}
