using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class JukeBox : MonoBehaviour
{
	public List<Vox> voces;

	public static JukeBox diff;

	public AudioMixer MasterMix;

	public AudioMixerGroup VOGroup;

	public List<Music> musics = new List<Music>();

	public int curDynastyId;

	public int volCeiling = 12;

	public int volFloor = 24;

	public float attenCeilVal = 100f;

	public int filterMinFreq = 1000;

	private Music curMusic;

	private string[] s = new string[4] { "Church", "Folk", "Military", "Treasury" };

	public List<VO> allvo = new List<VO>();

	public AudioSource[] sfxSources;

	public AudioSource[] uiSources;

	public AudioSource[] ambientSources;

	public AudioSource[] songSources;

	public AudioSource[] voSources;

	public List<SFX> samples;

	private int lastclipid;

	public Func<bool, Bearers> OnSpeak;

	public AudioMixerSnapshot[] snapshots;

	public bool RandomizeClip;

	private Coroutine nextplay;

	public bool isPlayingMusic;

	private float pausetime;

	private AudioSource playingSource;

	private bool isPlayingSong;

	public bool nomusic;

	private List<Music> allMus = new List<Music>();

	private int trackid = -1;

	private List<Music> cacheMus = new List<Music>();

	private float sfxVol;

	private float musVol;

	private float voVol = 1f;

	private bool isalt;

	private IEnumerator speakCorout;

	public float SpeechCover = 0.3f;

	private void Awake()
	{
		diff = this;
	}

	private void Start()
	{
		voVol = PlayerPrefs.GetFloat("volSpeech");
		if (voVol == 0f && PlayerPrefs.HasKey("volSpeech"))
		{
			MuteVoAndSave(play: false, andsave: false);
		}
		else
		{
			MuteVoAndSave(play: true, andsave: false);
		}
		if (PlayerPrefs.HasKey("volMusic"))
		{
			float val = PlayerPrefs.GetFloat("volMusic");
			float val2 = PlayerPrefs.GetFloat("volExternal");
			MusicVolume(val, andsave: false);
			SfxVolume(val2, andsave: false);
		}
		else
		{
			MusicVolume(0.5f);
			SfxVolume(0.5f);
		}
		SaveLevels();
		if (SuperPrefs.HasKey("trackId"))
		{
			trackid = SuperPrefs.GetInt("trackId");
		}
		if ((bool)GameAct.diff)
		{
			StartCoroutine("ChronicPlay");
		}
	}

	private void InitSources()
	{
		songSources = InitSource("songs");
		sfxSources = InitSource("sfx");
		uiSources = InitSource("UI");
		ambientSources = InitSource("ambient");
		voSources = InitSource("VO");
	}

	private AudioSource[] InitSource(string child)
	{
		Transform transform = base.transform.Find(child);
		AudioSource[] array = new AudioSource[transform.childCount + 1];
		array[0] = transform.GetComponent<AudioSource>();
		int num = 1;
		foreach (Transform item in transform)
		{
			array[num] = item.GetComponent<AudioSource>();
			num++;
		}
		return array;
	}

	private void StartDrone()
	{
	}

	public void PlayMusicDelayed(string back, float t)
	{
		StartCoroutine(DoStartDelayed(back, t));
	}

	private IEnumerator DoStartDelayed(string back, float t)
	{
		yield return new WaitForSeconds(t);
		PlayMusic(back);
	}

	public void StopMusic(bool nofade = false, float lerptime = 2f)
	{
		AudioSource audioSource = (songSources[0].isPlaying ? songSources[0] : songSources[1]);
		if (!(audioSource.clip == null) && isPlayingMusic)
		{
			pausetime = audioSource.time;
			if (nofade)
			{
				audioSource.Stop();
			}
			else
			{
				Fade(audioSource, lerptime, fadeIn: false);
			}
			isPlayingMusic = false;
		}
	}

	public void RestartMusic()
	{
		PlayMusic("defaut", restart: true);
	}

	public void PlayImportantMusic(string musicGroup = "defaut")
	{
		if (isPlayingMusic && !isPlayingSong && curMusic != null && curMusic.command == musicGroup)
		{
			return;
		}
		PlayMusic(musicGroup);
	}

	public void PlayMusic(string musicGroup = "defaut", bool restart = false)
	{
		if (musicGroup.Equals("defaut"))
		{
			musicGroup = ((CameffectAct.diff.isInDanger || GameAct.diff.GetInt("danger") > 6) ? "dangerous" : "peaceful");
		}
		if (musicGroup == "stop")
		{
			StopMusic();
			return;
		}
		if (nextplay != null)
		{
			StopCoroutine(nextplay);
		}
		if (!restart)
		{
			curMusic = SelectMusic(musicGroup);
		}
		if (curMusic != null)
		{
			isPlayingSong = false;
			PlayMusic(curMusic.sample, curMusic.loop, restart, curMusic.command);
		}
	}

	public void StopSong()
	{
		isPlayingSong = false;
		StopMusic(nofade: false, 6f);
	}

	public AudioSource PlaySong(AudioClip clip)
	{
		if (nextplay != null)
		{
			StopCoroutine(nextplay);
		}
		StopAllCoroutines();
		isPlayingSong = true;
		return PlayMusic(clip, loop: false);
	}

	private IEnumerator ChronicPlay()
	{
		while (true)
		{
			yield return new WaitForSeconds(10f);
			if (!songSources[0].isPlaying && !songSources[1].isPlaying && GameAct.diff.card != null && GameAct.diff.card.bearer != Bearers.concert)
			{
				PlayMusic();
			}
			if (curMusic != null)
			{
				bool flag = CameffectAct.diff.isInDanger || GameAct.diff.GetInt("danger") > 6;
				if ((curMusic.command == "dangerous" && !flag) || (curMusic.command == "peaceful" && flag))
				{
					PlayMusic();
				}
			}
		}
	}

	public AudioSource PlayMusic(AudioClip clip, bool loop, bool restart = false, string next = "defaut")
	{
		if (clip == null)
		{
			return null;
		}
		if (nomusic)
		{
			return null;
		}
		AudioSource[] array = songSources;
		AudioSource source;
		AudioSource audioSource;
		if (array[0].isPlaying)
		{
			source = array[0];
			audioSource = array[1];
		}
		else
		{
			audioSource = array[0];
			source = array[1];
		}
		audioSource.clip = clip;
		audioSource.loop = loop;
		if (restart)
		{
			audioSource.time = pausetime;
			pausetime = 0f;
		}
		else if (loop)
		{
			audioSource.time = Util.Rand(0f, audioSource.clip.length * 0.7f);
		}
		else
		{
			audioSource.time = 0f;
		}
		audioSource.Play();
		playingSource = audioSource;
		Fade(source, 3f, fadeIn: false);
		Fade(audioSource, 3f);
		isPlayingMusic = true;
		float time = clip.length - audioSource.time;
		if (nextplay != null)
		{
			StopCoroutine(nextplay);
		}
		nextplay = (loop ? StartCoroutine(NextPlay(180f, next)) : StartCoroutine(NextPlay(time, next)));
		return audioSource;
	}

	public void Pause()
	{
		if (playingSource != null)
		{
			playingSource.Pause();
		}
	}

	public void Restart()
	{
		if (playingSource != null)
		{
			playingSource.UnPause();
		}
	}

	private IEnumerator NextPlay(float time, string thisGroup)
	{
		yield return new WaitForSeconds(time - 2f);
		if (!isPlayingSong)
		{
			StopMusic();
			yield return new WaitForSeconds(3f);
			if (!isPlayingMusic && !isPlayingSong)
			{
				PlayMusic(thisGroup);
			}
		}
	}

	private Music SelectMusic(string musicGroup)
	{
		List<Music> list = musics.FindAll((Music it) => it.command == musicGroup);
		if (list.Count == 0)
		{
			list = musics.FindAll((Music it) => it.command == "defaut");
		}
		List<Music> list2 = new List<Music>(list);
		list2.RemoveAll((Music it) => cacheMus.Contains(it));
		if (list2.Count > 0)
		{
			list = list2;
		}
		Music music = list.PickRandom();
		cacheMus.Add(music);
		if (cacheMus.Count > 16)
		{
			cacheMus.RemoveAt(0);
		}
		return music;
	}

	private void Fade(AudioSource source, float lerpTime, bool fadeIn = true)
	{
		StartCoroutine(DoFadeSource(source, lerpTime, fadeIn));
	}

	private IEnumerator DoFadeSource(AudioSource source, float lerpTime, bool fadeIn)
	{
		float currentLerpTime = 0f;
		float start = (fadeIn ? 0f : 1f);
		float end = (fadeIn ? 1f : 0f);
		while (currentLerpTime < lerpTime)
		{
			currentLerpTime += Time.deltaTime;
			float t = currentLerpTime / lerpTime;
			source.volume = Mathf.Lerp(start, end, t);
			yield return null;
		}
		if (!fadeIn)
		{
			source.Stop();
		}
		source.volume = 1f;
	}

	public void Remove()
	{
		StopAllCoroutines();
		UnityEngine.Object.DestroyImmediate(base.gameObject);
	}

	private float LinearToDecibel(float linear)
	{
		if (linear != 0f)
		{
			return 20f * Mathf.Log10(linear);
		}
		return -144f;
	}

	public void MusicVolumeAndSave(float val)
	{
		MusicVolume(val);
	}

	public void MusicVolume(float val, bool andsave = true)
	{
		musVol = val;
		MasterMix.SetFloat("volMusic", LinearToDecibel(val));
		if (andsave)
		{
			SaveLevels();
		}
	}

	public void MuteVo(bool play = true)
	{
		MuteVoAndSave(play);
	}

	private void MuteVoAndSave(bool play = true, bool andsave = true)
	{
		float num = (play ? 1 : 0);
		voVol = num;
		if (andsave)
		{
			SaveLevels();
		}
	}

	public void SfxVolumeAndSave(float val)
	{
		SfxVolume(val);
	}

	public void SfxVolume(float val, bool andsave = true)
	{
		sfxVol = val;
		MasterMix.SetFloat("externalVolume", LinearToDecibel(val));
		if (andsave)
		{
			SaveLevels();
		}
	}

	private void SaveLevels()
	{
		PlayerPrefs.SetFloat("volMusic", musVol);
		PlayerPrefs.SetFloat("volExternal", sfxVol);
		PlayerPrefs.SetFloat("volSpeech", voVol);
	}

	public void FadeOutMusic(float length = 1f)
	{
		StartCoroutine(DoFadeOutMusic(isalt, length));
	}

	private IEnumerator DoFadeOutMusic(bool alt, float length)
	{
		float t = 0f;
		foreach (Vox voce in voces)
		{
			(alt ? voce.speakers[1] : voce.speakers[0]).Stop();
		}
		while (t < 1f)
		{
			foreach (Vox voce2 in voces)
			{
				(alt ? voce2.speakers[0] : voce2.speakers[1]).volume = 1f - t;
			}
			t += Time.deltaTime / length;
			yield return null;
		}
		foreach (Vox voce3 in voces)
		{
			AudioSource obj = (alt ? voce3.speakers[0] : voce3.speakers[1]);
			obj.Stop();
			obj.volume = 1f;
		}
	}

	private IEnumerator DoFadeOutSource(AudioSource[] sources, float length)
	{
		float t = 0f;
		AudioSource source = ((sources[0].isPlaying && !sources[1].isPlaying) ? sources[1] : sources[0]);
		while (t < 1f)
		{
			source.volume = 1f - t;
			t += Time.deltaTime / length;
			yield return null;
		}
		source.Stop();
		source.volume = 1f;
	}

	public void FadeOutVO()
	{
		FadeOut(voSources);
	}

	private void FadeOut(AudioSource[] sources)
	{
		foreach (AudioSource audioSource in sources)
		{
			if (audioSource.isPlaying)
			{
				StartCoroutine(DoFadeOutSource(audioSource, 1f));
			}
		}
	}

	private IEnumerator DoFadeOutSource(AudioSource source, float length)
	{
		float t = 0f;
		while (t < 1f)
		{
			source.volume = 1f - t;
			t += Time.deltaTime / length;
			yield return null;
		}
		source.Stop();
		source.volume = 1f;
	}

	private AudioClip SetSequ(List<AudioClip> list, AudioClip cur, int id)
	{
		List<AudioClip> list2 = new List<AudioClip>(list);
		if (cur != null)
		{
			list2.Remove(cur);
		}
		return list2[id];
	}

	private void PlaySequ(AudioSource source, AudioClip clip, double start, Vox vox, float totalVal)
	{
		source.clip = clip;
		float num = 0f;
		switch (vox.type)
		{
		case VoxTypes.soprano:
			num = -1f;
			break;
		case VoxTypes.alto:
			num = -1f;
			break;
		case VoxTypes.tenor:
			num = 1f;
			break;
		case VoxTypes.baryton:
			num = 1f;
			break;
		}
		float num2 = ((GameAct.diff.state == GameStates.interreign) ? 1f : ((float)vox.dataRef.val / 100f));
		source.panStereo = num * num2;
		source.PlayScheduled(start);
	}

	public List<Music> GetMusics()
	{
		return musics;
	}

	public void UpdateVocalMix()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (Vox voce in voces)
		{
			num2 += (float)voce.dataRef.val;
			if ((float)voce.dataRef.val > num)
			{
				num = voce.dataRef.val;
			}
		}
		foreach (Vox voce2 in voces)
		{
			string text = "";
			switch (voce2.type)
			{
			case VoxTypes.soprano:
				text = s[0];
				break;
			case VoxTypes.alto:
				text = s[1];
				break;
			case VoxTypes.tenor:
				text = s[2];
				break;
			case VoxTypes.baryton:
				text = s[3];
				break;
			}
			float num3 = Mathf.Min(voce2.dataRef.val, attenCeilVal);
			MasterMix.GetFloat("vol" + text, out var value);
			float newVol = ((num3 < 1f) ? (-60f) : (-1f * (Mathf.Pow(1f - num3 / attenCeilVal, 3f) * (float)(volFloor - volCeiling)) - (float)volCeiling));
			MasterMix.GetFloat("cutoff" + text, out var value2);
			float newFreq = ((num3 < 1f) ? ((float)filterMinFreq) : (Mathf.Pow(num3 / attenCeilVal, 3f) * (float)(22000 - filterMinFreq) + (float)filterMinFreq));
			StartCoroutine(LerpAttenuate(text, value, newVol, value2, newFreq, 2f));
		}
	}

	private IEnumerator LerpPitch(string param, float prevPitch, float newPitch, float lerpTime = 1f)
	{
		float currentLerpTime = 0f;
		while (currentLerpTime < lerpTime)
		{
			currentLerpTime += Time.deltaTime;
			float num = currentLerpTime / lerpTime;
			num = num * num * num * (num * (6f * num - 15f) + 10f);
			MasterMix.SetFloat(param, Mathf.Lerp(prevPitch, newPitch, num));
			yield return null;
		}
	}

	public void DuckMusic(bool duck, float length = 1.5f)
	{
		string param = "volMusic";
		MasterMix.GetFloat(param, out var value);
		float num = (duck ? (-80f) : LinearToDecibel(musVol));
		if (length < 0.001f)
		{
			MasterMix.SetFloat(param, num);
		}
		else
		{
			StartCoroutine(LerpParam(param, value, num, length, andback: true));
		}
	}

	private IEnumerator LerpParam(string param, float prevVal, float newVal, float lerpTime = 2f, bool andback = false)
	{
		float currentLerpTime = 0f;
		while (currentLerpTime < lerpTime)
		{
			currentLerpTime += Time.deltaTime;
			float num = currentLerpTime / lerpTime;
			num = num * num * num * (num * (6f * num - 15f) + 10f);
			MasterMix.SetFloat(param, Mathf.Lerp(prevVal, newVal, num));
			yield return null;
		}
		if (andback)
		{
			yield return new WaitForSeconds(lerpTime);
			StartCoroutine(LerpParam(param, newVal, prevVal, lerpTime));
		}
	}

	private IEnumerator LerpAttenuate(string chan, float prevVol, float newVol, float prevFreq, float newFreq, float lerpTime)
	{
		float currentLerpTime = 0f;
		while (currentLerpTime < lerpTime)
		{
			currentLerpTime += Time.deltaTime;
			float num = currentLerpTime / lerpTime;
			num = num * num * num * (num * (6f * num - 15f) + 10f);
			MasterMix.SetFloat("vol" + chan, Mathf.Lerp(prevVol, newVol, num));
			MasterMix.SetFloat("cutoff" + chan, Mathf.Lerp(prevFreq, newFreq, num));
			yield return null;
		}
	}

	public void BalanceSnapShots(string from, string to, float amo, float delay = 0.3f)
	{
		float num = 1f - amo;
		float[] array = new float[snapshots.Length];
		for (int i = 0; i < snapshots.Length; i++)
		{
			if (snapshots[i].name == from)
			{
				array[i] = num;
			}
			else if (snapshots[i].name == to)
			{
				array[i] = amo;
			}
			else
			{
				array[i] = 0f;
			}
		}
		MasterMix.TransitionToSnapshots(snapshots, array, delay);
	}

	public void DefaultSnapshot(float fadeLength = 2.5f)
	{
		TransitionToSnapshot("Default", fadeLength);
	}

	public void TransitionToSnapshot(string id, float t = 2.5f)
	{
		for (int i = 0; i < snapshots.Length; i++)
		{
			if (snapshots[i].name == id)
			{
				float[] array = new float[snapshots.Length];
				for (int j = 0; j < snapshots.Length; j++)
				{
					array[j] = ((i == j) ? 1 : 0);
				}
				MasterMix.TransitionToSnapshots(snapshots, array, t);
				break;
			}
		}
	}

	public void Speak(string text, Bearers type, float voPitch, float voCenterFrequ, float voFrequGain, float voGain)
	{
		if (voVol != 0f && sfxVol != 0f)
		{
			if (speakCorout != null)
			{
				StopCoroutine(speakCorout);
			}
			StartCoroutine(FadeSound(voSources[1], fadeIn: false, 0.25f));
			StartCoroutine(FadeSound(voSources[0], fadeIn: false, 0.25f));
			if (OnSpeak != null)
			{
				type = OnSpeak(arg: false);
				voPitch = 0.96f;
				voCenterFrequ = 2900f;
				voFrequGain = 1f;
				voGain = 1f;
			}
			speakCorout = DoSpeak(text, type, voPitch, voCenterFrequ, voFrequGain, voGain);
			StartCoroutine(speakCorout);
		}
	}

	private IEnumerator DoSpeak(string text, Bearers type, float voPitch, float voCenterFrequ, float voFrequGain, float voGain)
	{
		yield return new WaitForSeconds(0.5f);
		MasterMix.SetFloat("voPitch", voPitch);
		MasterMix.SetFloat("voCenterFrequ", voCenterFrequ);
		MasterMix.SetFloat("voFrequGain", voFrequGain);
		MasterMix.SetFloat("voGain", voGain);
		float textLength = Mathf.Clamp((float)text.Length / 75f, 0f, 1f);
		float soundLength = 0f;
		VO vO = allvo.Find((VO it) => it.type == type);
		if (vO == null)
		{
			yield break;
		}
		string[] collection = vO.samples;
		List<string> tempClip = new List<string>(collection);
		List<AudioClip> clipsToPlay = new List<AudioClip>();
		float i = 1f;
		bool alt = false;
		int num = 0;
		while (soundLength < textLength && tempClip.Count > 0)
		{
			int index = Util.RandInt(0, tempClip.Count);
			AudioClip audioClip = (AudioClip)Resources.Load("VO/" + type.ToString() + "/" + tempClip[index], typeof(AudioClip));
			tempClip.Remove(tempClip[index]);
			if (audioClip != null)
			{
				clipsToPlay.Add(audioClip);
				soundLength += GetLength(audioClip);
			}
			else
			{
				num++;
			}
			if (num > 10)
			{
				yield break;
			}
		}
		AudioClip longestClip = clipsToPlay.Aggregate((AudioClip seed, AudioClip c) => (!(c.length > seed.length)) ? seed : c);
		int index2 = clipsToPlay.FindIndex((AudioClip c) => c.length == longestClip.length);
		clipsToPlay.RemoveAt(index2);
		clipsToPlay.Add(longestClip);
		for (int j = 0; j < clipsToPlay.Count; j++)
		{
			AudioSource audioSource = (alt ? voSources[1] : voSources[0]);
			alt = !alt;
			AudioClip oldclip = audioSource.clip;
			audioSource.clip = clipsToPlay[j];
			audioSource.Play();
			yield return null;
			float length = GetLength(clipsToPlay[j]);
			yield return new WaitForSeconds(length);
			Resources.UnloadAsset(oldclip);
		}
		while (soundLength < textLength && tempClip.Count > 0)
		{
			int index3 = Util.RandInt(0, tempClip.Count);
			AudioClip oldclip = (AudioClip)Resources.Load("VO/" + type.ToString() + "/" + tempClip[index3], typeof(AudioClip));
			tempClip.Remove(tempClip[index3]);
			soundLength += GetLength(oldclip);
			i += 1f;
			AudioSource audioSource2 = (alt ? voSources[1] : voSources[0]);
			alt = !alt;
			AudioClip oldclip2 = audioSource2.clip;
			audioSource2.clip = oldclip;
			audioSource2.Play();
			yield return null;
			Resources.UnloadAsset(oldclip2);
			float length2 = GetLength(oldclip);
			yield return new WaitForSeconds(length2);
		}
	}

	private float GetLength(AudioClip clip)
	{
		if (clip == null)
		{
			return 0f;
		}
		return clip.length - Mathf.Clamp(clip.length * SpeechCover - 0.02f, -0.01f, 0.3f);
	}

	public void PlayValues(Dictionary<Variables, int> SFXvalues)
	{
		if (SFXvalues.Count != 0)
		{
			StopCoroutine("DoPlayValues");
			StartCoroutine("DoPlayValues", SFXvalues);
		}
	}

	private void PlayValue(SFXTypes up, SFXTypes down, int val)
	{
		if (val != 0 && val <= 15)
		{
			if (val < 0)
			{
				PlaySound(down);
			}
			else
			{
				PlaySound(up);
			}
		}
	}

	private IEnumerator DoPlayValues(Dictionary<Variables, int> SFXvalues)
	{
		foreach (KeyValuePair<Variables, int> item in SFXvalues.OrderBy((KeyValuePair<Variables, int> it) => it.Value))
		{
			switch (item.Key)
			{
			case Variables.power:
				PlayValue(SFXTypes.ui_score_power_decrease, SFXTypes.ui_score_power_increase, item.Value);
				break;
			case Variables.oxygen:
				PlayValue(SFXTypes.ui_score_oxygen_decrease, SFXTypes.ui_score_oxygen_increase, item.Value);
				break;
			case Variables.people:
				PlayValue(SFXTypes.ui_score_people_decrease, SFXTypes.ui_score_people_increase, item.Value);
				break;
			case Variables.hull:
				PlayValue(SFXTypes.ui_score_hull_decrease, SFXTypes.ui_score_hull_increase, item.Value);
				break;
			}
			yield return new WaitForSeconds(0.15f);
		}
	}

	private AudioSource[] GetSoundSource(SFXSources type)
	{
		return type switch
		{
			SFXSources.sfx => sfxSources, 
			SFXSources.ui => uiSources, 
			SFXSources.ambient => ambientSources, 
			SFXSources.songs => songSources, 
			_ => null, 
		};
	}

	private AudioSource GetFreeSource(AudioSource[] sources)
	{
		foreach (AudioSource audioSource in sources)
		{
			if (!audioSource.isPlaying)
			{
				return audioSource;
			}
		}
		return sources[0];
	}

	public void FadeOutAmbient()
	{
		FadeOut(ambientSources);
	}

	public void PlaySound(AudioClip type, bool fadeIn = false, bool duckMusic = false, float duckLength = 2.5f)
	{
		if (sfxVol != 0f)
		{
			AudioSource[] soundSource = GetSoundSource(SFXSources.sfx);
			AudioSource freeSource = GetFreeSource(soundSource);
			freeSource.clip = type;
			if (duckMusic)
			{
				DuckMusic(duck: true, duckLength);
			}
			freeSource.Stop();
			if (fadeIn)
			{
				freeSource.volume = 0f;
				freeSource.Play();
				StartCoroutine(FadeSound(freeSource, fadeIn: true, 1.5f));
			}
			else
			{
				freeSource.Play();
			}
		}
	}

	public void PlayAttenuatedSound(SFXTypes name, float volume = 1f)
	{
		PlaySound(name, fadeIn: false, duckMusic: false, 2.5f, -1, 1.5f, volume);
	}

	public void PlaySound(SFXTypes name, bool fadeIn = false, bool duckMusic = false, float duckLength = 2.5f, int id = -1, float fadeInLength = 1.5f, float volume = 1f)
	{
		if ((bool)HapticAct.diff)
		{
			SFXTypes sFXTypes = name;
			if (sFXTypes == SFXTypes.ui_button_next || sFXTypes == SFXTypes.ui_menu_close || sFXTypes == SFXTypes.ui_menu_open)
			{
				HapticAct.diff.Tap();
			}
		}
		if (sfxVol == 0f)
		{
			return;
		}
		SFX sFX = samples.Find((SFX it) => it.type == name);
		if (sFX == null)
		{
			return;
		}
		if (sFX.source == SFXSources.ambient)
		{
			fadeIn = true;
			FadeOutAmbient();
		}
		AudioSource[] soundSource = GetSoundSource(sFX.source);
		AudioSource freeSource = GetFreeSource(soundSource);
		AudioClip audioClip = null;
		if (sFX.clips.Count > 1)
		{
			List<AudioClip> list = new List<AudioClip>(sFX.clips);
			if (id == -1 && list.Contains(sFX.lastclip))
			{
				list.Remove(sFX.lastclip);
			}
			audioClip = (sFX.lastclip = ((id == -1) ? list[Util.RandInt(0, list.Count)] : list[id]));
		}
		else
		{
			audioClip = sFX.clips[0];
		}
		freeSource.clip = audioClip;
		freeSource.loop = sFX.loop;
		if (sFX.loop)
		{
			freeSource.time = Util.Rand(0f, audioClip.length) * 0.5f;
		}
		else
		{
			freeSource.time = 0f;
		}
		sFX.lastsource = freeSource;
		if (duckMusic)
		{
			DuckMusic(duck: true, duckLength);
		}
		freeSource.Stop();
		if (fadeIn)
		{
			freeSource.volume = 0f;
			freeSource.Play();
			StartCoroutine(FadeSound(freeSource, fadeIn: true, fadeInLength));
		}
		else
		{
			freeSource.volume = volume;
			freeSource.Play();
		}
	}

	public void FadeStopSound(SFXTypes name, float fadeLength, bool unduckMusic = false, float duckLength = 2.5f)
	{
		SFX sFX = samples.Find((SFX it) => it.type == name);
		if (sFX != null)
		{
			if (unduckMusic)
			{
				DuckMusic(duck: false, duckLength);
			}
			if ((bool)sFX.lastsource)
			{
				StartCoroutine(FadeSound(sFX.lastsource, fadeIn: false, fadeLength));
			}
		}
	}

	public void StopAllSoundAndMusic()
	{
		AudioSource[] array = sfxSources;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource.isPlaying && audioSource.loop)
			{
				audioSource.Stop();
			}
		}
		array = ambientSources;
		foreach (AudioSource audioSource2 in array)
		{
			if (audioSource2.isPlaying && audioSource2.loop)
			{
				audioSource2.Stop();
			}
		}
		array = uiSources;
		foreach (AudioSource audioSource3 in array)
		{
			if (audioSource3.isPlaying && audioSource3.loop)
			{
				audioSource3.Stop();
			}
		}
		StopMusic();
	}

	public void StopSound(SFXTypes name, bool unduckMusic = false, float duckLength = 2.5f)
	{
		SFX sFX = samples.Find((SFX it) => it.type == name);
		if (sFX != null)
		{
			if (unduckMusic)
			{
				DuckMusic(duck: false, duckLength);
			}
			if ((bool)sFX.lastsource)
			{
				sFX.lastsource.Stop();
			}
		}
	}

	private IEnumerator FadeSound(AudioSource source, bool fadeIn, float lerpTime)
	{
		if (fadeIn || source.isPlaying)
		{
			float currentLerpTime = 0f;
			float start = (fadeIn ? 0f : 1f);
			float end = (fadeIn ? 1f : 0f);
			while (currentLerpTime < lerpTime)
			{
				currentLerpTime += Time.deltaTime;
				float t = currentLerpTime / lerpTime;
				source.volume = Mathf.Lerp(start, end, t);
				yield return new WaitForEndOfFrame();
			}
			if (!fadeIn)
			{
				source.Stop();
			}
			source.volume = 1f;
		}
	}

	public void LinkVar(List<DataVariable> vars)
	{
		foreach (Vox voce in voces)
		{
			_ = voce;
		}
	}

	private void AddSample(List<AudioClip> list, string nam, string name, AudioClip sample)
	{
		if (name.Contains(nam) && !list.Contains(sample))
		{
			list.Add(sample);
		}
	}
}
