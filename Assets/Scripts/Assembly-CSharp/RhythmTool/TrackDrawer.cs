using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
	public abstract class TrackDrawer
	{
		private static Dictionary<Type, TrackDrawer> trackDrawers;

		private static Texture2D texture;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			trackDrawers = new Dictionary<Type, TrackDrawer>();
			texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
		}

		public static void Draw(Track track, Rect rect, float start, float end)
		{
			TrackDrawer trackDrawer = GetTrackDrawer(track);
			GUIStyle box = GUI.skin.box;
			box.alignment = TextAnchor.UpperLeft;
			GUI.Box(new Rect(0f, 0f, rect.width, rect.height), track.name, box);
			Rect rect2 = new Rect(5f, 5f, rect.width - 10f, rect.height - 10f);
			GUI.BeginGroup(rect2);
			trackDrawer.DrawTrack(track, rect2, start, end);
			GUI.EndGroup();
		}

		protected abstract void DrawTrack(Track track, Rect rect, float start, float end);

		public static TrackDrawer GetTrackDrawer(Track track)
		{
			Type type = track.GetType();
			if (trackDrawers.TryGetValue(type, out var value))
			{
				return value;
			}
			value = Activator.CreateInstance(GetTrackDrawerType(type.BaseType.GetGenericArguments()[0])) as TrackDrawer;
			trackDrawers.Add(type, value);
			return value;
		}

		protected static float GetFeaturePosition(IFeature feature, Rect rect, float start, float end)
		{
			return (feature.timestamp - start) / (end - start) * rect.width;
		}

		protected static void DrawRect(Rect position)
		{
			GUI.DrawTexture(position, texture);
		}

		private static Type GetTrackDrawerType(Type featureType)
		{
			Type type = typeof(TrackDrawer<>).MakeGenericType(featureType);
			Type[] types = featureType.Assembly.GetTypes();
			foreach (Type type2 in types)
			{
				if (type2.IsSubclassOf(type) && !type2.IsAbstract)
				{
					return type2;
				}
			}
			return type;
		}
	}
	public class TrackDrawer<T> : TrackDrawer where T : IFeature
	{
		private List<T> features;

		public TrackDrawer()
		{
			features = new List<T>();
		}

		protected override void DrawTrack(Track track, Rect rect, float start, float end)
		{
			DrawTrack(track as Track<T>, rect, start, end);
		}

		protected virtual void DrawTrack(Track<T> track, Rect rect, float start, float end)
		{
			features.Clear();
			track.GetIntersectingFeatures(features, start, end);
			foreach (T feature in features)
			{
				DrawFeature(feature, rect, start, end);
			}
		}

		protected virtual void DrawFeature(T feature, Rect rect, float start, float end)
		{
			float featurePosition = TrackDrawer.GetFeaturePosition(feature, rect, start, end);
			TrackDrawer.DrawRect(new Rect(featurePosition, rect.height, 1f, -10f));
		}
	}
}
