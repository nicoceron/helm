using SVGImporter;
using UnityEngine;

public class AudioColor : MonoBehaviour
{
	public SVGRenderer target;

	public Color velocity;

	public bool affectAlpha;

	public float velocityMultiplier = 1f;

	protected float _velocityMultiplierIntensity = 1f;

	public float speed = 1f;

	protected float _speedIntensity = 1f;

	public bool random = true;

	protected float _randomIntensity = 1f;

	private Color destination;

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

	private void Awake()
	{
		destination = target.color;
	}

	public void OnAudio(float audioVelocity)
	{
		float num = audioVelocity * velocityMultiplier * _velocityMultiplierIntensity;
		if (random && _randomIntensity >= 0.5f)
		{
			destination.r = Mathf.PerlinNoise(Time.realtimeSinceStartup * 1.5f, Time.realtimeSinceStartup * 3f) * velocity.r * num;
			destination.g = Mathf.PerlinNoise(Time.realtimeSinceStartup * 2f, Time.realtimeSinceStartup * 0.5f) * velocity.g * num;
			destination.b = Mathf.PerlinNoise(Time.realtimeSinceStartup * 0.2f, Time.realtimeSinceStartup * 0.15f) * velocity.b * num;
			if (affectAlpha)
			{
				destination.a = Mathf.PerlinNoise(Time.realtimeSinceStartup * 2.3f, Time.realtimeSinceStartup * 3.5f) * velocity.a * num;
			}
			else
			{
				destination.a = target.color.a;
			}
		}
		else
		{
			destination.r = velocity.r * num;
			destination.g = velocity.g * num;
			destination.b = velocity.b * num;
			if (affectAlpha)
			{
				destination.a = velocity.a * num;
			}
			else
			{
				destination.a = target.color.a;
			}
		}
		target.color = Color.Lerp(target.color, destination, Time.deltaTime * speed * _speedIntensity);
	}
}
