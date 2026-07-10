using SVGImporter.Rendering;
using UnityEngine;

namespace SVGImporter
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ISVGRenderer))]
	[AddComponentMenu("Rendering/SVG Modifiers/Blur Modifier", 22)]
	public class SVGBlurModifier : SVGModifier
	{
		public Camera camera;

		public bool useCameraVelocity;

		public float radius = 20f;

		public bool motionBlur;

		public bool manualMotionBlur = true;

		public float direction;

		protected Vector3 lastPosition;

		protected Vector2 transformVelocity;

		protected Camera mainCamera
		{
			get
			{
				if (camera == null)
				{
					if (Camera.current != null)
					{
						return Camera.current;
					}
					return Camera.main;
				}
				return camera;
			}
		}

		private void LateUpdate()
		{
			transformVelocity = base.transform.position - lastPosition;
			if (Time.deltaTime > 0f)
			{
				transformVelocity.x /= Time.deltaTime;
				transformVelocity.y /= Time.deltaTime;
			}
			lastPosition = base.transform.position;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			lastPosition = base.transform.position;
		}

		protected override void PrepareForRendering(SVGLayer[] layers, SVGAsset svgAsset, bool force)
		{
			if (layers == null)
			{
				return;
			}
			Camera camera = mainCamera;
			SVGMatrix identity = SVGMatrix.identity;
			SVGMatrix identity2 = SVGMatrix.identity;
			Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
			Matrix4x4 matrix4x = camera.projectionMatrix * worldToCameraMatrix;
			float num = radius;
			float magnitude = ((Vector2)matrix4x.MultiplyVector(Vector2.one * radius)).magnitude;
			if (camera.orthographic)
			{
				num *= magnitude;
			}
			else
			{
				float num2 = Vector3.Distance(base.transform.position, camera.transform.position);
				num = ((!(num2 > 0f)) ? (num * magnitude) : (num * (magnitude / num2)));
			}
			if (!motionBlur)
			{
				identity = identity.Scale(num);
			}
			else
			{
				float num3 = num;
				if (!manualMotionBlur)
				{
					Vector2 vector = transformVelocity;
					if (useCameraVelocity)
					{
						vector += (Vector2)base.transform.InverseTransformVector(camera.velocity);
					}
					float num4 = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
					Vector2 zero = Vector2.zero;
					if (num4 > 0f)
					{
						zero.x = vector.x / num4;
						zero.y = vector.y / num4;
					}
					direction = Mathf.Atan2(zero.y, zero.x) * 57.29578f;
					num3 = num4 * num;
				}
				identity = identity.Scale(1f + num3, 1f);
			}
			identity2 = identity2.Rotate(0f - direction);
			SVGMatrix sVGMatrix = SVGMatrix.identity.Rotate(direction);
			int num5 = layers.Length;
			if (!useSelection)
			{
				for (int i = 0; i < num5; i++)
				{
					if (layers[i].shapes == null)
					{
						continue;
					}
					int num6 = layers[i].shapes.Length;
					for (int j = 0; j < num6; j++)
					{
						if (layers[i].shapes[j].type == SVGShapeType.ANTIALIASING && layers[i].shapes[j].angles != null)
						{
							int vertexCount = layers[i].shapes[j].vertexCount;
							for (int k = 0; k < vertexCount; k++)
							{
								Vector2 point = layers[i].shapes[j].angles[k];
								point = identity2.Transform(point);
								point = identity.Transform(point);
								point = sVGMatrix.Transform(point);
								layers[i].shapes[j].angles[k] = point;
							}
						}
					}
				}
				return;
			}
			for (int l = 0; l < num5; l++)
			{
				if (layers[l].shapes == null || !layerSelection.Contains(l))
				{
					continue;
				}
				int num7 = layers[l].shapes.Length;
				for (int m = 0; m < num7; m++)
				{
					if (layers[l].shapes[m].type == SVGShapeType.ANTIALIASING && layers[l].shapes[m].angles != null)
					{
						int vertexCount2 = layers[l].shapes[m].vertexCount;
						for (int n = 0; n < vertexCount2; n++)
						{
							Vector2 point2 = layers[l].shapes[m].angles[n];
							point2 = identity2.Transform(point2);
							point2 = identity.Transform(point2);
							point2 = sVGMatrix.Transform(point2);
							layers[l].shapes[m].angles[n] = point2;
						}
					}
				}
			}
		}
	}
}
