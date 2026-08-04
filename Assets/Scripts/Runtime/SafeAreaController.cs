using UnityEngine;

namespace ShadowTileEscape
{
    public sealed class SafeAreaController : MonoBehaviour
    {
        [SerializeField] RectTransform safeAreaRoot;
        Rect lastSafeArea;
        Vector2Int lastScreenSize;

        void Awake() => ApplyIfChanged(true);

        void Update() => ApplyIfChanged(false);

        void ApplyIfChanged(bool force)
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            if (!force && lastSafeArea == Screen.safeArea && lastScreenSize == size) return;
            if (safeAreaRoot == null || size.x <= 0 || size.y <= 0)
            {
                Debug.LogError("SafeAreaController requires a serialized SafeAreaRoot and a valid screen size.", this);
                enabled = false;
                return;
            }

            var safe = Screen.safeArea;
            safeAreaRoot.anchorMin = new Vector2(safe.xMin / size.x, safe.yMin / size.y);
            safeAreaRoot.anchorMax = new Vector2(safe.xMax / size.x, safe.yMax / size.y);
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
            lastSafeArea = safe;
            lastScreenSize = size;
        }
    }
}
