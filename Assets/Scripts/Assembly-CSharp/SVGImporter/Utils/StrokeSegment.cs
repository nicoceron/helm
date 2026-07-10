using UnityEngine;

namespace SVGImporter.Utils
{
	public struct StrokeSegment
	{
		public Vector2 startPoint;

		public Vector2 endPoint;

		public Vector2 direction;

		public Vector2 directionNormalized;

		public Vector2 directionNormalizedRotated;

		public float length;

		public StrokeSegment(Vector2 startPoint, Vector2 endPoint)
		{
			this.startPoint = startPoint;
			this.endPoint = endPoint;
			direction = endPoint - startPoint;
			length = direction.magnitude;
			if (length != 0f)
			{
				directionNormalized.x = direction.x / length;
				directionNormalized.y = direction.y / length;
				directionNormalizedRotated = Quaternion.Euler(0f, 0f, 90f) * directionNormalized;
			}
			else
			{
				directionNormalized = (directionNormalizedRotated = Vector2.zero);
			}
		}
	}
}
