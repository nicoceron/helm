using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SVGImporter
{
	[RequireComponent(typeof(SVGRenderer))]
	public class SVGLegacyAnimator : MonoBehaviour
	{
		[Serializable]
		public class OnCompleteEvent : UnityEvent<SVGLegacyAnimator>
		{
		}

		public enum WrapMode
		{
			ONCE = 0,
			LOOP = 1,
			PING_PONG = 2
		}

		public SVGAsset[] frames;

		public WrapMode wrapMode;

		public bool playOnAwake = true;

		public float duration = 1f;

		public float timeScale = 1f;

		public bool direction = true;

		public int loops = -1;

		public int currentLoop;

		public bool rewind;

		public float progress;

		[FormerlySerializedAs("onComplete")]
		[SerializeField]
		protected OnCompleteEvent m_onComplete = new OnCompleteEvent();

		protected bool _isPlaying;

		protected SVGRenderer svgRenderer;

		public OnCompleteEvent onComplete
		{
			get
			{
				return m_onComplete;
			}
			set
			{
				m_onComplete = value;
			}
		}

		public bool isPlaying => _isPlaying;

		public float normalizedProgress
		{
			get
			{
				if (duration == 0f)
				{
					return 0f;
				}
				return Mathf.Clamp01(progress / duration);
			}
		}

		public void Play()
		{
			_isPlaying = true;
		}

		public void Stop()
		{
			currentLoop = 0;
			progress = 0f;
			_isPlaying = false;
		}

		public void Pause()
		{
			_isPlaying = false;
		}

		public void Restart()
		{
			Stop();
			Play();
		}

		protected virtual void Awake()
		{
			svgRenderer = GetComponent<SVGRenderer>();
		}

		protected virtual void Start()
		{
			if (playOnAwake)
			{
				Play();
			}
		}

		protected virtual void LateUpdate()
		{
			if (!_isPlaying)
			{
				return;
			}
			if (progress >= 0f && direction)
			{
				progress += Time.deltaTime * timeScale;
				if (progress >= duration)
				{
					AnimationEnded();
				}
			}
			else if (progress <= duration && !direction)
			{
				progress -= Time.deltaTime * timeScale;
				if (progress <= 0f)
				{
					AnimationEnded();
				}
			}
			switch (wrapMode)
			{
			case WrapMode.ONCE:
				progress = Mathf.Clamp(progress, 0f, duration);
				break;
			case WrapMode.LOOP:
				progress = Mathf.Repeat(progress, duration);
				break;
			case WrapMode.PING_PONG:
				progress = Mathf.Clamp(progress, 0f, duration);
				break;
			}
			UpdateMesh();
		}

		public void UpdateMesh()
		{
			int num = Mathf.Clamp(Mathf.RoundToInt(normalizedProgress * (float)frames.Length - 0.5f), 0, frames.Length - 1);
			if (svgRenderer.vectorGraphics != frames[num])
			{
				svgRenderer.vectorGraphics = frames[num];
			}
		}

		private void AnimationEnded()
		{
			switch (wrapMode)
			{
			case WrapMode.ONCE:
				if (rewind)
				{
					Stop();
				}
				else
				{
					_isPlaying = false;
				}
				m_onComplete.Invoke(this);
				break;
			case WrapMode.LOOP:
				if (loops >= 0 && currentLoop >= loops)
				{
					if (rewind)
					{
						Stop();
					}
					else
					{
						currentLoop = loops;
						_isPlaying = false;
					}
					m_onComplete.Invoke(this);
				}
				else
				{
					currentLoop++;
				}
				break;
			case WrapMode.PING_PONG:
				if (loops >= 0 && currentLoop >= loops)
				{
					if (rewind)
					{
						Stop();
					}
					else
					{
						currentLoop = loops;
						_isPlaying = false;
					}
					m_onComplete.Invoke(this);
				}
				else
				{
					direction = !direction;
					currentLoop++;
				}
				break;
			}
		}
	}
}
