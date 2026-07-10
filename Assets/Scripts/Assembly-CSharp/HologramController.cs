using System;
using UnityEngine;

public class HologramController : MonoBehaviour
{
	[Serializable]
	public struct HologramLayer
	{
		public Transform transform;

		public Vector3 startLocalPosition;

		public float rotation;
	}

	public HologramLayer[] layers;

	public float depth;

	public float depthSpeed = 1f;

	public AnimationCurve depthAnimation;

	private float elapsedTime;

	private void Start()
	{
		for (int i = 0; i < layers.Length; i++)
		{
			if (!(layers[i].transform == null))
			{
				layers[i].startLocalPosition = layers[i].transform.localPosition;
			}
		}
	}

	private void Update()
	{
		elapsedTime += Time.deltaTime * depthSpeed;
		depth = depthAnimation.Evaluate(Mathf.PingPong(elapsedTime, 1f));
		for (int i = 0; i < layers.Length; i++)
		{
			if (!(layers[i].transform == null))
			{
				Vector3 localPosition = layers[i].transform.localPosition;
				localPosition.z = Mathf.LerpUnclamped(0f, layers[i].startLocalPosition.z, depth);
				layers[i].transform.localPosition = localPosition;
				layers[i].transform.localRotation *= Quaternion.Euler(0f, 0f, layers[i].rotation * Time.deltaTime);
			}
		}
	}
}
