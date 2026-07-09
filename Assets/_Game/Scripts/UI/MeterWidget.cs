using UnityEngine;
using UnityEngine.UI;

namespace Lionrise
{
    public sealed class MeterWidget : MonoBehaviour
    {
        private Text symbol;
        private Text label;
        private Text pips;
        private Text preview;
        private Image frame;
        private Color baseColor;
        private string shape;

        public static MeterWidget Create(Transform parent, string meterName, string shape, Color color)
        {
            var root = LionriseUI.CreateRect(meterName + " Meter", parent, new Vector2(0, 0), new Vector2(1, 1));
            var widget = root.gameObject.AddComponent<MeterWidget>();
            widget.shape = shape;
            widget.baseColor = color;
            var frameRect = LionriseUI.CreateRect("Mechanical Frame", root, new Vector2(.18f, .20f), new Vector2(.82f, .96f));
            widget.frame = frameRect.gameObject.AddComponent<Image>();
            widget.frame.color = new Color32(20, 19, 25, 245);
            var outline = frameRect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(95, 84, 73, 180);
            outline.effectDistance = new Vector2(1f, -1f);
            widget.symbol = LionriseUI.CreateText(frameRect, shape, 20, TextAnchor.MiddleCenter, color, FontStyle.Bold);
            LionriseUI.SetRect(widget.symbol.rectTransform, new Vector2(.08f, .30f), new Vector2(.92f, .96f));
            widget.pips = LionriseUI.CreateText(frameRect, "●○○○○", 7, TextAnchor.MiddleCenter, color);
            LionriseUI.SetRect(widget.pips.rectTransform, new Vector2(.02f, .04f), new Vector2(.98f, .35f));
            widget.label = LionriseUI.CreateText(root, meterName.ToUpperInvariant(), 7, TextAnchor.UpperCenter, new Color(.76f, .72f, .62f), FontStyle.Bold);
            LionriseUI.SetRect(widget.label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, .22f));
            widget.preview = LionriseUI.CreateText(root, string.Empty, 12, TextAnchor.UpperRight, color, FontStyle.Bold);
            LionriseUI.SetRect(widget.preview.rectTransform, new Vector2(.70f, .58f), new Vector2(.98f, .98f));
            return widget;
        }

        public void SetValue(int value, bool highContrast)
        {
            var filled = Mathf.Clamp(Mathf.CeilToInt(value / 20f), 0, 5);
            pips.text = new string('●', filled) + new string('○', 5 - filled);
            var danger = value <= 12 || value >= 88;
            var dangerColor = highContrast ? Color.white : new Color(1f, .33f, .3f);
            symbol.color = pips.color = danger ? dangerColor : baseColor;
            frame.color = danger ? new Color32(72, 18, 24, 255) : new Color32(20, 19, 25, 245);
            label.text = danger ? label.text.TrimEnd(' ', '!') + " !" : label.text.TrimEnd(' ', '!');
        }

        public void SetPreview(int delta)
        {
            preview.text = delta > 0 ? "▲" : delta < 0 ? "▼" : string.Empty;
            preview.color = delta >= 0 ? new Color(.42f, 1f, .77f) : new Color(1f, .48f, .45f);
        }

        public void ClearPreview() => preview.text = string.Empty;
    }
}
