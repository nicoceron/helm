using System.Collections.Generic;
using UnityEngine;

namespace SVGImporter.Geometry
{
	public class SVGMeshCutter
	{
		private struct MeshBuilder
		{
			private struct IntPair
			{
				public int first;

				public int second;

				public IntPair(int first, int second)
				{
					if (first < second)
					{
						this.first = first;
						this.second = second;
					}
					else
					{
						this.first = second;
						this.second = first;
					}
				}
			}

			public List<Vector3> pos;

			public List<Color32> col;

			public List<Vector2> uv;

			public List<Vector2> uv2;

			public List<int> tri;

			private Dictionary<IntPair, int> map;

			private Mesh mesh;

			private Vector3[] origVertices;

			public MeshBuilder(Mesh m, Vector3[] vertices)
			{
				pos = new List<Vector3>();
				col = new List<Color32>();
				uv = new List<Vector2>();
				uv2 = new List<Vector2>();
				tri = new List<int>();
				map = new Dictionary<IntPair, int>();
				mesh = m;
				origVertices = vertices;
			}

			private int MergeVertex(int i)
			{
				IntPair key = new IntPair(i, i);
				if (!map.TryGetValue(key, out var value))
				{
					map.Add(key, value = pos.Count);
					pos.Add(origVertices[i]);
					col.Add(mesh.colors32[i]);
					uv.Add(mesh.uv[i]);
					uv2.Add(mesh.uv2[i]);
				}
				return value;
			}

			private static void MergeCutVertex(MeshBuilder leftSide, MeshBuilder rightSide, int i1, int i2, Vector2 origin, Vector2 direction, out int jl, out int jr)
			{
				IntPair key = new IntPair(i1, i2);
				if (!leftSide.map.TryGetValue(key, out jl))
				{
					jl = leftSide.pos.Count;
					jr = rightSide.pos.Count;
					leftSide.map.Add(key, jl);
					rightSide.map.Add(key, jr);
					float num = CutEdge(leftSide.origVertices[i1], leftSide.origVertices[i2], origin, direction);
					Vector3 item = leftSide.origVertices[i1] + (leftSide.origVertices[i2] - leftSide.origVertices[i1]) * num;
					leftSide.pos.Add(item);
					rightSide.pos.Add(item);
					Color32 item2 = Color32.Lerp(leftSide.mesh.colors32[i1], leftSide.mesh.colors32[i2], num);
					leftSide.col.Add(item2);
					rightSide.col.Add(item2);
					Vector2 item3 = leftSide.mesh.uv[i1] + (leftSide.mesh.uv[i2] - leftSide.mesh.uv[i1]) * num;
					leftSide.uv.Add(item3);
					rightSide.uv.Add(item3);
					Vector2 item4 = leftSide.mesh.uv2[i1] + (leftSide.mesh.uv2[i2] - leftSide.mesh.uv2[i1]) * num;
					leftSide.uv2.Add(item4);
					rightSide.uv2.Add(item4);
				}
				else
				{
					jr = rightSide.map[key];
				}
			}

			public void AddTri(int i1, int i2, int i3)
			{
				tri.Add(MergeVertex(i1));
				tri.Add(MergeVertex(i2));
				tri.Add(MergeVertex(i3));
			}

			public static void AddCutTri(MeshBuilder leftSide, MeshBuilder rightSide, int i1, int i2, int i3, Vector2 origin, Vector2 direction)
			{
				int item = leftSide.MergeVertex(i1);
				int item2 = rightSide.MergeVertex(i2);
				int item3 = rightSide.MergeVertex(i3);
				MergeCutVertex(leftSide, rightSide, i1, i2, origin, direction, out var jl, out var jr);
				MergeCutVertex(leftSide, rightSide, i1, i3, origin, direction, out var jl2, out var jr2);
				leftSide.tri.Add(item);
				leftSide.tri.Add(jl);
				leftSide.tri.Add(jl2);
				rightSide.tri.Add(jr);
				rightSide.tri.Add(item2);
				rightSide.tri.Add(item3);
				rightSide.tri.Add(jr);
				rightSide.tri.Add(item3);
				rightSide.tri.Add(jr2);
			}

			public Mesh ToMesh()
			{
				Mesh obj = new Mesh();
				obj.vertices = pos.ToArray();
				obj.colors32 = col.ToArray();
				obj.uv = uv.ToArray();
				obj.uv2 = uv2.ToArray();
				obj.triangles = tri.ToArray();
				obj.RecalculateBounds();
				return obj;
			}

			private static float CutEdge(Vector3 v1, Vector3 v2, Vector2 origin, Vector2 direction)
			{
				return Mathf.Clamp01((direction.y * v1.x - direction.x * v1.y + direction.x * origin.y - direction.y * origin.x) / (direction.x * (v2.y - v1.y) - direction.y * (v2.x - v1.x)));
			}

			public bool IsDegenerate(Vector2 origin, Vector2 direction)
			{
				float num = (float)pos.Count * ((0f - direction.x) * origin.y + direction.y * origin.x);
				for (int i = 0; i < pos.Count; i++)
				{
					num += direction.x * pos[i].y - direction.y * pos[i].x;
				}
				return (double)Mathf.Abs(num) < 0.01 * (double)direction.magnitude;
			}
		}
	}
}
