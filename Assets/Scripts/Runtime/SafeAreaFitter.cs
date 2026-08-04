using UnityEngine;

namespace ShadowTileEscape
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rect;
        Rect lastSafeArea;
        Vector2Int lastSize;

        void Awake()
        {
            rect = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            if (lastSafeArea != Screen.safeArea || lastSize != size) Apply();
        }

        void Apply()
        {
            var safe = Screen.safeArea;
            var size = new Vector2(Screen.width, Screen.height);
            rect.anchorMin = new Vector2(safe.xMin / size.x, safe.yMin / size.y);
            rect.anchorMax = new Vector2(safe.xMax / size.x, safe.yMax / size.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            lastSafeArea = safe;
            lastSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
