using UnityEngine;

namespace RhythmTool.Examples
{
	public abstract class SongSelector : MonoBehaviour
	{
		public RhythmAnalyzer analyzer;

		public RhythmPlayer player;

		public virtual void NextSong()
		{
			player.Stop();
		}
	}
}
