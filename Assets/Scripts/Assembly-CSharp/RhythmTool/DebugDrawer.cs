using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	[RequireComponent(typeof(RhythmPlayer))]
	[AddComponentMenu("RhythmTool/Debug Drawer", -2)]
	public class DebugDrawer : MonoBehaviour
	{
		private float width = 300f;

		private float height = 75f;

		private float padding = 10f;

		public RhythmPlayer rhythmPlayer { get; private set; }

		private void Awake()
		{
			rhythmPlayer = GetComponent<RhythmPlayer>();
		}

		private void OnGUI()
		{
			if (!(rhythmPlayer.rhythmData == null))
			{
				List<Track> tracks = rhythmPlayer.rhythmData.tracks;
				GUI.BeginGroup(new Rect(10f, 10f, width, (height + padding) * (float)tracks.Count));
				Rect rect = new Rect(0f, 0f, width, height);
				for (int i = 0; i < tracks.Count; i++)
				{
					GUI.BeginGroup(new Rect(0f, (float)i * (height + padding), width, height));
					TrackDrawer.Draw(tracks[i], rect, rhythmPlayer.time, rhythmPlayer.time + 6f);
					GUI.EndGroup();
				}
				GUI.EndGroup();
			}
		}
	}
}
