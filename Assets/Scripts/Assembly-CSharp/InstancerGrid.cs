using UnityEngine;

public class InstancerGrid : MonoBehaviour
{
	public Instancer instancer;

	public int grid = 3;

	protected float _gridIntensity = 1f;

	public float space = 1f;

	protected float _spaceIntensity = 1f;

	public float speed = 1f;

	protected float _speedIntensity = 1f;

	public bool horizontal = true;

	protected float _horizontalIntensity = 1f;

	public bool square;

	protected float _squareIntensity = 1f;

	private Vector3 destination;

	public void GridIntensity(float value)
	{
		_gridIntensity = value;
	}

	public void SpaceIntensity(float value)
	{
		_spaceIntensity = value;
	}

	public void SpeedIntensity(float value)
	{
		_speedIntensity = value;
	}

	public void HorizontalIntensity(float value)
	{
		_horizontalIntensity = value;
	}

	public void SquareIntensity(float value)
	{
		_squareIntensity = value;
	}

	private void Update()
	{
		float num = instancer.instances.Length;
		int num2 = Mathf.RoundToInt((float)grid * _gridIntensity);
		if (square && _squareIntensity >= 0.5f)
		{
			num2 = Mathf.RoundToInt(Mathf.Sqrt(num));
		}
		if (num2 < 1)
		{
			num2 = 1;
		}
		float num3 = space * _spaceIntensity;
		float t = Time.deltaTime * speed * _speedIntensity;
		float num4 = (float)(num2 - 1) * 0.5f * num3;
		bool flag = horizontal && _horizontalIntensity >= 0.5f;
		for (int i = 0; (float)i < num; i++)
		{
			float num5;
			float num6;
			if (flag)
			{
				num5 = i % num2;
				num6 = Mathf.Floor(i / num2);
			}
			else
			{
				num6 = i % num2;
				num5 = Mathf.Floor(i / num2);
			}
			destination.x = 0f - num4 + num6 * num3;
			destination.y = 0f - num4 + num5 * num3;
			instancer.instances[i].transform.localPosition = Vector3.Lerp(instancer.instances[i].transform.localPosition, destination, t);
		}
	}
}
