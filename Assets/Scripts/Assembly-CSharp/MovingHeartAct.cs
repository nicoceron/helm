using System.Collections;
using DG.Tweening;
using SVGImporter;
using TMPro;
using UnityEngine;

public class MovingHeartAct : MonoBehaviour
{
	private TMP_Text woawtext;

	public ConcertCard scCon;

	private SVGRenderer img;

	public SVGAsset brokenHeart;

	public SVGAsset goodHeart;

	public Transform guitar;

	public Transform meTrans;

	public bool isMoving;

	private float start_pos = 160f;

	private float end_pos = -405f;

	private Tweener domove;

	private Tweener rotateHeart;

	private bool isBroken;

	public Color redColor;

	private float ypo;

	private float speed = 1f;

	private float remedySpeed = 1f;

	public float decal;

	private float decalFix;

	private float lifetime;

	private Tween _tween;

	private bool isBad;

	private int beatId;

	private bool use2ndline;

	private float acc;

	private Sequence endSequence;

	private bool isDying;

	private bool isMissed;

	private void Awake()
	{
		woawtext = GetComponentInChildren<TMP_Text>();
		endSequence = null;
		base.gameObject.SetActive(value: false);
		img = GetComponent<SVGRenderer>();
	}

	private void FixedUpdate()
	{
		if (!guitar || !isMoving)
		{
			return;
		}
		if (!scCon.isPlaying)
		{
			if (img.enabled)
			{
				remedySpeed = -0.2f;
				img.enabled = false;
			}
			return;
		}
		if (!img.enabled)
		{
			DOTween.To(() => remedySpeed, delegate(float x)
			{
				remedySpeed = x;
			}, 1f, 5f);
			img.enabled = true;
		}
		UpdateMove();
	}

	private void UpdateMove()
	{
		acc += Time.fixedDeltaTime;
		if (isMissed)
		{
			return;
		}
		ypo -= Time.fixedDeltaTime * 350f * acc * speed * remedySpeed;
		Vector2 linePos = scCon.GetLinePos(ypo, use2ndline);
		float x = linePos.x - decal * 8f + decalFix;
		meTrans.localPosition = Vector3.Lerp(meTrans.localPosition, new Vector3(x, ypo, -5f + linePos.y), Time.fixedDeltaTime * 24f);
		meTrans.localRotation = Quaternion.Euler(-90f, 0f, 360f * decal);
		if (ypo < end_pos && !isMissed)
		{
			if (isBad)
			{
				StopAndBonus();
			}
			else
			{
				StopAndDie();
			}
		}
		else
		{
			CheckDistGuitar();
		}
	}

	private void Move(float bpm, float timestamp, bool anddecal)
	{
		lifetime = Time.realtimeSinceStartup;
		base.gameObject.SetActive(value: true);
		use2ndline = Util.GetInt(timestamp.ToString(), 0, 2) == 0;
		speed = 1f;
		acc = 0f;
		isMoving = true;
		ypo = start_pos;
		Vector2 linePos = scCon.GetLinePos(start_pos - 50f, use2ndline);
		meTrans.localPosition = new Vector3(linePos.x, start_pos - 50f, -5f + linePos.y);
		decal = 0f;
		if (!anddecal)
		{
			UpdateMove();
			return;
		}
		float endValue = ((Util.GetInt(timestamp + "zou", 0, 2) != 0) ? 1 : (-1));
		if (_tween != null)
		{
			_tween.Kill();
		}
		_tween = DOTween.To(() => decal, delegate(float x)
		{
			decal = x;
		}, endValue, 5f).SetId(8);
		meTrans.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		UpdateMove();
	}

	public void ShowText(string t)
	{
		speed = 0.2f;
		woawtext.text = t;
		woawtext.enabled = true;
	}

	public void HideText()
	{
		woawtext.enabled = false;
	}

	public bool PopAndMove(float bpm, float timestamp, bool withPop, bool anddecal, bool badhearts, float xpo = 0f, int id = -1)
	{
		if (!isMoving)
		{
			img.color = redColor;
			isBad = false;
			if (badhearts)
			{
				isBad = Util.GetInt(timestamp + "bad", 0, 2) == 0;
				if (isBad)
				{
					SwitchBroken();
					img.color = Color.black;
				}
			}
			decalFix = xpo;
			beatId = id;
			Move(bpm, timestamp, anddecal);
			if (withPop)
			{
				Pop(bpm);
			}
			return true;
		}
		if (withPop)
		{
			Pop(bpm);
		}
		return false;
	}

	public void Pop(float bpm, bool withPop = true)
	{
		if (!isBad && isMoving && !isMissed)
		{
			if (!withPop)
			{
				meTrans.localScale = new Vector3(6f, 6f, 1f);
				return;
			}
			meTrans.localScale = new Vector3(6f + speed * 6f, 6f + speed * 6f, 1f);
			domove = meTrans.DOScale(new Vector3(6f, 6f, 1f), Mathf.Clamp(50f / bpm - 0.04f, 0.1f, 1f)).SetEase(Ease.InOutBack);
		}
	}

	private void CheckDistGuitar()
	{
		float num = (isBad ? 0.8f : 1.4f);
		if (scCon.superTween != null && scCon.superTween.active)
		{
			num += Mathf.Clamp(2f - Mathf.Abs(-0.7f + scCon.superTween.position), 0f, 1.8f);
		}
		if (isMoving && meTrans.localPosition.y < -350f && Mathf.Abs(guitar.position.x - base.transform.position.x) < num)
		{
			if (isBad)
			{
				StopAndDie();
			}
			else
			{
				StopAndBonus();
			}
		}
	}

	private void StopAndDie()
	{
		JukeBox.diff.PlaySound(SFXTypes.sfx_minigame_miss);
		isMissed = true;
		scCon.RemoveHeart(beatId == -1);
		endSequence.Kill();
		endSequence = DOTween.Sequence();
		float endValue = ((meTrans.localPosition.x < 0f) ? (-5) : 5);
		endSequence.AppendCallback(SwitchBroken).Append(meTrans.DOLocalMoveX(endValue, 1f).SetEase(Ease.InOutBack)).Join(meTrans.DOLocalMoveZ(-70f, 1f).SetEase(Ease.InSine))
			.Join(DOTween.To(() => img.color, delegate(Color x)
			{
				img.color = x;
			}, Color.black, 1f))
			.SetEase(Ease.InSine)
			.AppendInterval(3f)
			.AppendCallback(delegate
			{
				Stop();
			});
	}

	private void SwitchBroken()
	{
		img.vectorGraphics = brokenHeart;
	}

	private void StopAndBonus(bool silent = false)
	{
		scCon.AddHeart(silent, beatId == -1);
		isMissed = true;
		Stop();
	}

	private IEnumerator WaitAndStop()
	{
		yield return new WaitForSeconds(2f);
		Stop();
	}

	public void Stop()
	{
		woawtext.enabled = false;
		isMoving = false;
		isMissed = false;
		img.vectorGraphics = goodHeart;
		endSequence.Kill();
		domove.Kill();
		meTrans.DOKill();
		img.DOKill();
		img.color = redColor;
		base.gameObject.SetActive(value: false);
	}
}
