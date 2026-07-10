using UnityEngine;

public class Instancer : MonoBehaviour
{
	public Instanced[] prefabs;

	public int count = 10;

	public bool pickRandom;

	public AnimationCurve updateFallof = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	[HideInInspector]
	public Instanced[] instances;

	private int lastInstanceIndex;

	private void OnEnable()
	{
		instances = new Instanced[count];
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = Object.Instantiate(GetInstance(), position, rotation);
			gameObject.transform.SetParent(base.transform);
			instances[i] = gameObject.GetComponent<Instanced>();
		}
		lastInstanceIndex = 0;
	}

	private GameObject GetInstance()
	{
		if (prefabs == null || prefabs.Length == 0)
		{
			return null;
		}
		if (prefabs.Length == 1)
		{
			return prefabs[0].gameObject;
		}
		Instanced instanced = null;
		if (pickRandom)
		{
			instanced = prefabs[Mathf.RoundToInt(Random.value * (float)(prefabs.Length - 1))];
		}
		else
		{
			instanced = prefabs[lastInstanceIndex];
			lastInstanceIndex = Mathf.RoundToInt(Mathf.Repeat(lastInstanceIndex + 1, prefabs.Length - 1));
		}
		return instanced.gameObject;
	}

	private void OnDisable()
	{
		if (instances != null)
		{
			for (int i = 0; i < instances.Length; i++)
			{
				if (!(instances[i] == null))
				{
					Object.Destroy(instances[i].gameObject);
				}
			}
		}
		instances = null;
	}

	public void OnUpdate(float value)
	{
		if (instances != null)
		{
			float num = instances.Length;
			for (int i = 0; (float)i < num; i++)
			{
				float time = (float)i / num;
				instances[i].onUpdate.Invoke(value * updateFallof.Evaluate(time));
			}
		}
	}
}
