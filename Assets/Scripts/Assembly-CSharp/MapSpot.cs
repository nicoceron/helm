using System;
using DG.Tweening;
using SVGImporter;
using UnityEngine;

[Serializable]
public class MapSpot
{
	public string name;

	public RectTransform trans;

	public SVGImage img;

	public int rank;

	public Vector2 position;

	public Vector2 realposition;

	public Backgrounds type;

	public int distance;

	public Tweener tween;

	public bool isSignal;

	public MapSpot(RectTransform obj)
	{
		name = obj.name;
		position = obj.anchoredPosition;
		trans = obj;
		trans.DOSizeDelta(new Vector2(4f, 4f), 0.3f).SetEase(Ease.OutSine);
		img = obj.GetComponent<SVGImage>();
		switch (name.Substring(name.Length - 1))
		{
		case "0":
			rank = 0;
			break;
		case "1":
			rank = 1;
			break;
		case "2":
			rank = 2;
			break;
		case "3":
			rank = 3;
			break;
		}
	}
}
