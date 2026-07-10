using System;
using UnityEngine;

public class InstancerSpiral : MonoBehaviour
{
	public Instancer instancer;

	public float outerRadius = 1f;

	protected float _outerRadiusIntensity = 1f;

	public float innerRadius;

	protected float _innerRadiusIntensity = 1f;

	public float space = 30f;

	protected float _spaceIntensity = 1f;

	public float speed = 1f;

	protected float _speedIntensity = 1f;

	private Vector3 destination;

	public void OuterRadiusIntensity(float value)
	{
		_outerRadiusIntensity = value;
	}

	public void InnerRadiusIntensity(float value)
	{
		_innerRadiusIntensity = value;
	}

	public void SpaceIntensity(float value)
	{
		_spaceIntensity = value;
	}

	public void SpeedIntensity(float value)
	{
		_speedIntensity = value;
	}

	private void Update()
	{
		float t = Time.deltaTime * speed * _speedIntensity;
		float num = space * ((float)Math.PI / 180f) * _spaceIntensity;
		float num2 = instancer.instances.Length;
		float a = outerRadius * _outerRadiusIntensity;
		float b = innerRadius * _innerRadiusIntensity;
		for (int i = 0; i < instancer.instances.Length; i++)
		{
			float t2 = (float)i / num2;
			float f = (float)i * num;
			float num3 = Mathf.Lerp(a, b, t2);
			destination.x = Mathf.Cos(f) * num3;
			destination.y = Mathf.Sin(f) * num3;
			instancer.instances[i].transform.localPosition = Vector3.Lerp(instancer.instances[i].transform.localPosition, destination, t);
		}
	}
}
