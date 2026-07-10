using UnityEngine;

public class AudioRotate : MonoBehaviour
{
	public Transform target;

	public Vector3 velocity;

	public float velocityMultiplier = 1f;

	protected float _velocityMultiplierIntensity = 1f;

	public float speed = 1f;

	protected float _speedIntensity = 1f;

	public bool random = true;

	protected float _randomIntensity = 1f;

	private Vector3 destination;

	public void VelocityMultiplierIntensity(float value)
	{
		_velocityMultiplierIntensity = value;
	}

	public void SpeedIntensity(float value)
	{
		_speedIntensity = value;
	}

	public void RandomIntensity(float value)
	{
		_randomIntensity = value;
	}

	public void OnAudio(float audioVelocity)
	{
		float num = audioVelocity * velocityMultiplier * _velocityMultiplierIntensity;
		if (random && _randomIntensity >= 0.5f)
		{
			destination.x = Mathf.PerlinNoise(Time.realtimeSinceStartup * 1.5f, Time.realtimeSinceStartup * 3f) * velocity.x * num;
			destination.y = Mathf.PerlinNoise(Time.realtimeSinceStartup * 0.5f, Time.realtimeSinceStartup) * velocity.y * num;
			destination.z = Mathf.PerlinNoise(Time.realtimeSinceStartup * 0.3f, Time.realtimeSinceStartup * 2f) * velocity.z * num;
		}
		else
		{
			destination.x = velocity.x * num;
			destination.y = velocity.y * num;
			destination.z = velocity.z * num;
		}
		target.localRotation = Quaternion.Lerp(target.localRotation, Quaternion.Euler(destination), Time.deltaTime * speed * _speedIntensity);
	}
}
