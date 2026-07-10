using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[AddComponentMenu("Rendering/SVG Sorter", 20)]
	public class SVGSorter : MonoBehaviour
	{
		public float depthOffset = 0.01f;

		public int layerIndex;

		public bool sort = true;

		private float zOffsetStart;

		private int layerIndexStart;

		public void Sort()
		{
			zOffsetStart = base.transform.position.z;
			SortRecursive(base.transform, ref zOffsetStart, ref layerIndexStart);
		}

		private void SortRecursive(Transform transform, ref float zOffset, ref int layerIndex)
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				SVGRenderer component = child.GetComponent<SVGRenderer>();
				if (component != null)
				{
					if (!component.overrideSorter)
					{
						SVGAsset vectorGraphics = component.vectorGraphics;
						if (vectorGraphics != null)
						{
							Bounds bounds = vectorGraphics.bounds;
							Vector3 position = component.transform.position;
							zOffset += bounds.size.z * Mathf.Sign(depthOffset);
							position.z = zOffset;
							component.transform.position = position;
							zOffset += depthOffset;
							component.sortingOrder = layerIndex++;
						}
					}
					else if (component.overrideSorterChildren)
					{
						continue;
					}
				}
				SortRecursive(child, ref zOffset, ref layerIndex);
			}
		}
	}
}
