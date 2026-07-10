using UnityEngine;

namespace RhythmTool
{
	public class ValueDrawer : TrackDrawer<Value>
	{
		protected override void DrawFeature(Value feature, Rect rect, float start, float end)
		{
			float featurePosition = TrackDrawer.GetFeaturePosition(feature, rect, start, end);
			TrackDrawer.DrawRect(new Rect(featurePosition, rect.height, 1f, (0f - feature.value) * 10f));
		}
	}
}
