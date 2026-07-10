using DG.Tweening;
using SVGImporter;
using UnityEngine;

public class AutoMove : MonoBehaviour
{
	private Tweener move;

	private Tweener size;

	private RectTransform rect;

	private SVGImage img;

	private int rank;

	private float timeTodestination = 1f;

	public Vector2 anchorTarg;

	public Vector2 sizeTarg;

	public Vector2 anchorRankAdd;

	private Vector2 anchorOrigin;

	public Vector2 anchorRandom;

	public Vector2 sizeRandom;

	private Vector2 sizeOrigin;

	public float delayRank = 1f;

	private void Awake()
	{
		img = GetComponent<SVGImage>();
		rect = GetComponent<RectTransform>();
		rank = base.transform.GetSiblingIndex();
		rect.anchoredPosition += anchorRankAdd;
		anchorOrigin = rect.anchoredPosition;
		sizeOrigin = rect.sizeDelta;
		SetMove((float)rank * delayRank);
		timeTodestination = delayRank * (float)base.transform.parent.childCount;
	}

	private void SetMove(float delay = 0f)
	{
		rect.anchoredPosition = anchorOrigin + Util.Rand(-1f) * anchorRandom;
		rect.sizeDelta = sizeOrigin + Util.Rand(-1f) * sizeRandom;
		move = rect.DOAnchorPos(anchorTarg + Util.Rand(-1f) * anchorRandom, timeTodestination).SetDelay(delay).OnComplete(ReinitMove);
		size = rect.DOSizeDelta(sizeTarg + Util.Rand(-1f) * sizeRandom, timeTodestination).SetDelay(delay);
	}

	private void ReinitMove()
	{
		move.Kill();
		size.Kill();
		SetMove();
	}

	private void OnEnable()
	{
		img.enabled = true;
		SetMove((float)rank * delayRank);
	}

	private void OnDisable()
	{
		move.Kill();
		size.Kill();
		img.enabled = false;
	}
}
