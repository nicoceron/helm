using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;

public class FightCard : CardAct
{
	public AnimationCurve spawnCurve;

	public Transform spawner;

	public RectTransform spawnerTarget;

	private Tweener rotateHull;

	public RectTransform reticule;

	public Transform ship;

	public Transform shipHull;

	private float timer;

	public List<RectTransform> stars;

	private List<SVGImage> starImages = new List<SVGImage>();

	private List<float> starTime = new List<float>();

	private Vector2 starFocus = Vector2.zero;

	private void Awake()
	{
		foreach (RectTransform star in stars)
		{
			starImages.Add(star.GetComponent<SVGImage>());
			starTime.Add(0f);
		}
	}

	private void Start()
	{
		InitStars();
		StartCoroutine("MoveShip");
		StartCoroutine("PlayTargets");
		StartCoroutine("MoveTarget");
	}

	private void FixedUpdate()
	{
		timer += Time.fixedDeltaTime * 1f;
		UpdateStars();
	}

	private IEnumerator PlayTargets()
	{
		yield return new WaitForSeconds(2f);
		while (true)
		{
			yield return new WaitForSeconds(4f);
		}
	}

	private IEnumerator MoveTarget()
	{
		float speed = 1f;
		float maxtime = 4f;
		float t = 0f;
		float speedSens = 1f;
		while (true)
		{
			spawner.Rotate(0f, 0f, Time.fixedDeltaTime * 100f * speed * speedSens);
			t += Time.fixedDeltaTime;
			spawnerTarget.anchoredPosition = new Vector2(0f, spawnCurve.Evaluate(Time.fixedTime * 0.08f) * 110f + 20f);
			if (t > maxtime)
			{
				speedSens = ((!(Util.Rand() > 0.5f)) ? 1 : (-1));
				speed = Util.Rand(0.7f, 1.3f);
				t = 0f;
				maxtime = Util.Rand(3f, 6f);
			}
			yield return 0;
		}
	}

	private IEnumerator MoveShip()
	{
		float radius = 150f;
		rotateHull = shipHull.DOLocalRotate(new Vector3(360f, 0f, 0f), 1.8f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutBack);
		Vector2 lastPos = Vector2.zero;
		bool isRotating = false;
		while (true)
		{
			Vector2 vector = -InputAct.diff.GetPointerVirt();
			Vector3 b = new Vector3(vector.x * 1500f, (vector.y - 0.3f) * 1500f, 0f);
			float magnitude = b.magnitude;
			if (magnitude > radius)
			{
				b *= radius / magnitude;
			}
			ship.localPosition = Vector3.Lerp(ship.localPosition, b, Time.deltaTime * 10f);
			reticule.anchoredPosition = -(Vector2)ship.localPosition;
			if (Vector2.SqrMagnitude(reticule.anchoredPosition - lastPos) > 2f && !isRotating)
			{
				isRotating = true;
				rotateHull = shipHull.DOLocalRotate(new Vector3((0f - b.x) * b.y * 0.02f, 0f, 0f), 1.8f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutBack).OnComplete(delegate
				{
					isRotating = false;
				});
			}
			lastPos = reticule.anchoredPosition;
			yield return 0;
		}
	}

	private void InitStars()
	{
		for (int i = 0; i < stars.Count; i++)
		{
			starTime[i] = (float)i / (float)stars.Count;
			stars[i].localRotation = Quaternion.Euler(0f, 0f, Util.Rand(-180f, 180f));
		}
	}

	private void UpdateStars()
	{
		float num = 2f;
		int index = 0;
		starFocus = Vector2.Lerp(starFocus, new Vector2((0f - ship.localPosition.x) * 1.5f, (0f - ship.localPosition.y) * 1.5f), Time.deltaTime * 32f);
		for (int i = 0; i < stars.Count; i++)
		{
			float num2 = Mathf.Repeat(starTime[i] + timer, 1f);
			if (num2 < Time.fixedDeltaTime)
			{
				stars[i].localRotation = Quaternion.Euler(0f, 0f, Util.Rand(-180f, 180f));
			}
			if (num2 < num)
			{
				index = i;
				num = num2;
			}
			float a = Mathf.Clamp(Mathf.PingPong(num2 * 4f, 2f), 0f, 1f);
			Color color = starImages[i].color;
			color.a = a;
			starImages[i].color = color;
			stars[i].localScale = new Vector3(1f + num2 * 3f, 1f + num2 * 3f, 1f);
			stars[i].anchoredPosition = starFocus * (1f - num2);
			stars[i].Rotate(new Vector3(0f, 0f, Mathf.Clamp(starFocus.x * starFocus.y, -10f, 10f) * Time.fixedDeltaTime));
		}
		stars[index].SetAsFirstSibling();
	}
}
