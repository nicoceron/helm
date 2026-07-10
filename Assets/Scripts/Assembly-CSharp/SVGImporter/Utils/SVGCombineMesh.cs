using UnityEngine;

namespace SVGImporter.Utils
{
	public class SVGCombineMesh
	{
		public static Mesh Combine(Mesh[] meshes)
		{
			CombineInstance[] array = new CombineInstance[meshes.Length];
			for (int i = 0; i < meshes.Length; i++)
			{
				array[i].mesh = meshes[i];
			}
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(array, mergeSubMeshes: false, useMatrices: false);
			return mesh;
		}
	}
}
