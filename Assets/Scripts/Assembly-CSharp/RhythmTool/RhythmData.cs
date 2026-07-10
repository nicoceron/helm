using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	public class RhythmData : ScriptableObject, IEnumerable<Track>, IEnumerable
	{
		[SerializeField]
		private string _name;

		[SerializeField]
		private List<Track> _tracks = new List<Track>();

		public new string name => _name;

		public List<Track> tracks => _tracks;

		public Track<T> GetTrack<T>() where T : IFeature
		{
			foreach (Track track in _tracks)
			{
				if (track is Track<T>)
				{
					return track as Track<T>;
				}
			}
			return null;
		}

		public Track<T> GetTrack<T>(string trackName) where T : IFeature
		{
			foreach (Track track in _tracks)
			{
				if (track is Track<T> && track.name == trackName)
				{
					return track as Track<T>;
				}
			}
			return null;
		}

		public void GetTracks<T>(List<Track<T>> tracks) where T : IFeature
		{
			foreach (Track<T> track in tracks)
			{
				if (track is Track<T>)
				{
					tracks.Add(track as Track<T>);
				}
			}
		}

		public void GetTracks<T>(List<Track<T>> tracks, string trackName) where T : IFeature
		{
			foreach (Track<T> track in tracks)
			{
				if (track is Track<T> && track.name == trackName)
				{
					tracks.Add(track as Track<T>);
				}
			}
		}

		public void GetFeatures<T>(List<T> features, float start, float end) where T : IFeature
		{
			foreach (Track track in _tracks)
			{
				if (track is Track<T>)
				{
					(track as Track<T>).GetFeatures(features, start, end);
				}
			}
		}

		public void GetFeatures<T>(List<T> features, float start, float end, string trackName) where T : IFeature
		{
			foreach (Track track in _tracks)
			{
				if (track.name == trackName && track is Track<T>)
				{
					(track as Track<T>).GetFeatures(features, start, end);
				}
			}
		}

		public void GetIntersectingFeatures<T>(List<T> features, float start, float end) where T : IFeature
		{
			foreach (Track track in _tracks)
			{
				if (track is Track<T>)
				{
					(track as Track<T>).GetIntersectingFeatures(features, start, end);
				}
			}
		}

		public void GetIntersectingFeatures<T>(List<T> features, float start, float end, string trackName) where T : IFeature
		{
			foreach (Track track in _tracks)
			{
				if (track.name == trackName && track is Track<T>)
				{
					(track as Track<T>).GetIntersectingFeatures(features, start, end);
				}
			}
		}

		public IEnumerator<Track> GetEnumerator()
		{
			foreach (Track track in _tracks)
			{
				yield return track;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _tracks.GetEnumerator();
		}

		private void OnDestroy()
		{
			foreach (Track track in tracks)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(track);
				}
				else
				{
					Object.DestroyImmediate(track);
				}
			}
		}

		public static RhythmData Create(string name, IEnumerable<Track> tracks)
		{
			RhythmData rhythmData = ScriptableObject.CreateInstance<RhythmData>();
			rhythmData._name = name;
			rhythmData._tracks = new List<Track>(tracks);
			return rhythmData;
		}
	}
}
