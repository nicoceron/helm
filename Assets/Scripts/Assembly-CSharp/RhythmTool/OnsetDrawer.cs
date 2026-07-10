using UnityEngine;

namespace RhythmTool
{
	public class OnsetDrawer : TrackDrawer<Onset>
	{
		protected override void DrawFeature(Onset feature, Rect rect, float start, float end)
		{
			float featurePosition = TrackDrawer.GetFeaturePosition(feature, rect, start, end);
			TrackDrawer.DrawRect(new Rect(featurePosition, rect.height, 1f, (0f - feature.strength) * 10f));
		}
	}
}
