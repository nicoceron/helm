using UnityEngine;

namespace Lionrise
{
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            var target = (RectTransform)transform;
            var safe = Screen.safeArea;
            var min = safe.position;
            var max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            target.anchorMin = min;
            target.anchorMax = max;
            target.offsetMin = target.offsetMax = Vector2.zero;
            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}

