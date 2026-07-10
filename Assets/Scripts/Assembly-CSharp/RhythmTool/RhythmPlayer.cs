using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("RhythmTool/Rhythm Player", -3)]
	public class RhythmPlayer : MonoBehaviour
	{
		public RhythmData rhythmData;

		public List<RhythmTarget> targets;

		private AudioClip _audioClip;

		private float _time;

		public AudioSource audioSource { get; private set; }

		public AudioClip audioClip
		{
			get
			{
				return _audioClip;
			}
			set
			{
				audioSource.clip = value;
			}
		}

		public float time
		{
			get
			{
				return _time;
			}
			set
			{
				_time = value;
				audioSource.time = _time;
			}
		}

		public float volume
		{
			get
			{
				return audioSource.volume;
			}
			set
			{
				audioSource.volume = value;
			}
		}

		public float pitch
		{
			get
			{
				return audioSource.pitch;
			}
			set
			{
				audioSource.pitch = value;
			}
		}

		public bool isPlaying => audioSource.isPlaying;

		public float prevTime { get; private set; }

		public event Action SongLoaded;

		public event Action SongEnded;

		public event Action Reset;

		public void Play()
		{
			if (!(audioClip == null))
			{
				if (audioSource.time == 0f)
				{
					OnReset();
				}
				audioSource.Play();
			}
		}

		public void Stop()
		{
			audioSource.Stop();
		}

		public void Pause()
		{
			audioSource.Pause();
		}

		public void UnPause()
		{
			audioSource.UnPause();
		}

		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
		}

		private void Update()
		{
			prevTime = _time;
			if (audioSource.isPlaying)
			{
				_time = Mathf.Clamp(_time + Time.unscaledDeltaTime * audioSource.pitch, audioSource.time - 0.02f, audioSource.time + 0.02f);
			}
			if (audioSource.clip == null)
			{
				return;
			}
			if (audioSource.clip != _audioClip)
			{
				OnSongLoaded();
			}
			if (audioSource.timeSamples == audioSource.clip.samples)
			{
				OnSongEnded();
			}
			if (Mathf.Abs(_time - prevTime) > 0.5f + Time.unscaledDeltaTime)
			{
				OnReset();
			}
			_audioClip = audioSource.clip;
			if (rhythmData == null)
			{
				return;
			}
			foreach (RhythmTarget target in targets)
			{
				if (!(target == null))
				{
					target.Process(rhythmData, prevTime, _time);
				}
			}
		}

		private void OnSongLoaded()
		{
			if (audioClip == null)
			{
				_time = 0f;
			}
			prevTime = _time;
			if (this.SongLoaded != null)
			{
				this.SongLoaded();
			}
		}

		private void OnSongEnded()
		{
			if (this.SongEnded != null)
			{
				this.SongEnded();
			}
		}

		private void OnReset()
		{
			_time = audioSource.time;
			prevTime = _time;
			if (this.Reset != null)
			{
				this.Reset();
			}
			foreach (RhythmTarget target in targets)
			{
				if (!(target == null))
				{
					target.Reset(rhythmData, time);
				}
			}
		}
	}
}
