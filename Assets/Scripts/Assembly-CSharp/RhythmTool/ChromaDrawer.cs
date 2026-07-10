using UnityEngine;

namespace RhythmTool
{
	public class ChromaDrawer : TrackDrawer<Chroma>
	{
		protected override void DrawFeature(Chroma feature, Rect rect, float start, float end)
		{
			float featurePosition = TrackDrawer.GetFeaturePosition(feature, rect, start, end);
			float y = rect.height - 1f - rect.height / 14f * (float)feature.note;
			float b = feature.length / (end - start) * rect.width;
			b = Mathf.Max(1f, b);
			TrackDrawer.DrawRect(new Rect(featurePosition, y, b, -1f));
		}
	}
}
