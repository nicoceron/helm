using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool.Examples
{
	public class AudioClipSelector : SongSelector
	{
		public List<AudioClip> songs;

		private int currentSong = -1;

		private void Start()
		{
			NextSong();
		}

		public override void NextSong()
		{
			base.NextSong();
			Object.Destroy(player.rhythmData);
			currentSong++;
			if (currentSong >= songs.Count)
			{
				currentSong = 0;
			}
			AudioClip audioClip = songs[currentSong];
			RhythmData rhythmData = analyzer.Analyze(audioClip);
			player.audioClip = audioClip;
			player.rhythmData = rhythmData;
		}
	}
}
