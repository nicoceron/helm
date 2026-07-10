using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmTool.Examples
{
	public class Visualizer : MonoBehaviour
	{
		public bool recordBeat = true;

		public bool recordCustom = true;

		public bool recordNote = true;

		public SongProfile profile;

		public RhythmAnalyzer analyzer;

		public RhythmPlayer player;

		public RhythmEventProvider eventProvider;

		public Text textBPM;

		public Text textTime;

		public Line linePrefab;

		private List<Line> lines;

		private List<Chroma> chromaFeatures;

		private KeyCode[] keys = new KeyCode[11]
		{
			KeyCode.A,
			KeyCode.Z,
			KeyCode.E,
			KeyCode.R,
			KeyCode.T,
			KeyCode.Y,
			KeyCode.U,
			KeyCode.I,
			KeyCode.O,
			KeyCode.W,
			KeyCode.P
		};

		private KeyCode[] notes = new KeyCode[10]
		{
			KeyCode.Q,
			KeyCode.S,
			KeyCode.D,
			KeyCode.F,
			KeyCode.G,
			KeyCode.H,
			KeyCode.J,
			KeyCode.K,
			KeyCode.L,
			KeyCode.M
		};

		private float lastbpm;

		private float lastNote;

		private float lastIntensity;

		public int onsetBeat = 108;

		private void Awake()
		{
			if (recordBeat)
			{
				profile.beatChange = new List<MusEvent>();
			}
			if (recordNote)
			{
				profile.noteChange = new List<MusEvent>
				{
					new MusEvent(0f, 0f)
				};
			}
			analyzer.Initialized += OnInitialized;
			player.Reset += OnReset;
			eventProvider.Register<Beat>(OnBeat);
			eventProvider.Register<Onset>(OnOnset);
			eventProvider.Register<Value>(OnSegment, "Volume");
			lines = new List<Line>();
			chromaFeatures = new List<Chroma>();
		}

		private void OnDestroy()
		{
			if (profile.customChange.Count > 1)
			{
				profile.customChange.Sort((MusEffect a, MusEffect b) => a.timestamp.CompareTo(b.timestamp));
			}
			if (profile.noteChange.Count > 1)
			{
				profile.noteChange.Sort((MusEvent a, MusEvent b) => a.timestamp.CompareTo(b.timestamp));
			}
		}

		private void Update()
		{
			if (!player.isPlaying)
			{
				return;
			}
			textTime.text = (Mathf.Round(player.time * 10f) * 0.1f).ToString();
			if (recordCustom)
			{
				for (int i = 0; i < keys.Length; i++)
				{
					if (Input.GetKeyDown(keys[i]))
					{
						int v = ((i == 9) ? 11 : i);
						profile.customChange.Add(new MusEffect(player.time, v));
					}
				}
			}
			if (recordNote)
			{
				for (int j = 0; j < notes.Length; j++)
				{
					if (Input.GetKeyDown(notes[j]))
					{
						float v2 = j;
						profile.noteChange.Add(new MusEvent(player.time, v2));
					}
				}
			}
			UpdateLines();
		}

		private void UpdateLines()
		{
			float time = player.time;
			List<Line> list = new List<Line>();
			foreach (Line line in lines)
			{
				if (line.timestamp < time || line.timestamp > time + eventProvider.offset)
				{
					Object.Destroy(line.gameObject);
					list.Add(line);
				}
			}
			foreach (Line item in list)
			{
				lines.Remove(item);
			}
			foreach (Line line2 in lines)
			{
				Vector3 position = line2.transform.position;
				position.x = line2.timestamp - time;
				line2.transform.position = position;
			}
		}

		private void OnInitialized(RhythmData rhythmData)
		{
			player.Play();
		}

		private void OnReset()
		{
			foreach (Line line in lines)
			{
				Object.Destroy(line.gameObject);
			}
			lines.Clear();
		}

		private void OnBeat(Beat beat)
		{
			if (onsetBeat <= 0)
			{
				CreateLine(beat.timestamp, 0f, 1f, Color.black, 1f);
				if (recordBeat)
				{
					float v = Mathf.Round(beat.bpm * 10f) / 10f;
					profile.beatChange.Add(new MusEvent(beat.timestamp, v));
				}
			}
		}

		private void OnOnset(Onset onset)
		{
			if (onsetBeat > 0)
			{
				profile.beatChange.Add(new MusEvent(onset.timestamp, onsetBeat));
			}
			if (!recordNote)
			{
				return;
			}
			chromaFeatures.Clear();
			player.rhythmData.GetIntersectingFeatures(chromaFeatures, onset.timestamp, onset.timestamp);
			float num = 0f;
			foreach (Chroma chromaFeature in chromaFeatures)
			{
				num += (float)chromaFeature.note / 10f;
				CreateLine(onset.timestamp, -2f + (float)chromaFeature.note * 0.1f, 0.2f, Color.blue, onset.strength / 10f);
			}
			if (chromaFeatures.Count > 0)
			{
				num /= (float)chromaFeatures.Count;
			}
			if (chromaFeatures.Count == 0)
			{
				CreateLine(onset.timestamp, -2f + lastNote * 0.1f, 0.2f, Color.blue, onset.strength / 10f);
			}
		}

		private void OnSegment(Value segment)
		{
			CreateLine(segment.timestamp, -3f, 1f, Color.green, segment.value / 10f);
		}

		private void CreateLine(float timestamp, float position, float scale, Color color, float opacity)
		{
			Line line = Object.Instantiate(linePrefab);
			line.transform.position = new Vector3(0f, position, 0f);
			line.transform.localScale = new Vector3(0.1f, scale, 0.01f);
			line.Init(color, opacity, timestamp);
			lines.Add(line);
		}
	}
}
