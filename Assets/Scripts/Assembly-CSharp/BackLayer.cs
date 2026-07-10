using System;
using DG.Tweening;
using SVGImporter;
using UnityEngine;

[Serializable]
public class BackLayer
{
	public RectTransform layer;

	public Vector2 startPos;

	public Color startColor;

	public Vector2 endPos;

	public Color endColor;

	public Ease easetype;

	public bool nomove;

	public BackVariation variator;

	public SVGImage img;

	public BackLayer(RectTransform _layer, Vector2 _initpos, Color _initcolor, Ease _ease)
	{
		easetype = _ease;
		startPos = Vector2.zero;
		endPos = _initpos;
		layer = _layer;
		startColor = Color.Lerp(Color.white, Color.black, Util.Rand(0.8f));
		endColor = _initcolor;
		BackVariation component = layer.GetComponent<BackVariation>();
		SVGImage component2 = layer.GetComponent<SVGImage>();
		if (component2 != null)
		{
			img = component2;
		}
		if (component != null)
		{
			variator = component;
		}
	}
}
