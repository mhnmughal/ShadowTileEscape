using System.Collections;
using UnityEngine;

namespace ShadowTileEscape
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UiEntranceMotion : MonoBehaviour
    {
        [SerializeField] RectTransform visual;
        [SerializeField] Vector2 offset = new Vector2(0f, -22f);
        [SerializeField] float duration = 0.2f;
        CanvasGroup group;
        Vector2 destination;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            if (visual == null) visual = transform as RectTransform;
            destination = visual.anchoredPosition;
        }

        void OnEnable()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            var reduceMotion = SaveGameService.ForCurrentUser().Load().settings.reducedFlashing;
            if (reduceMotion)
            {
                group.alpha = 1f;
                visual.anchoredPosition = destination;
                return;
            }

            StopAllCoroutines();
            StartCoroutine(Enter());
        }

        IEnumerator Enter()
        {
            group.alpha = 0f;
            visual.anchoredPosition = destination + offset;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                group.alpha = t;
                visual.anchoredPosition = Vector2.Lerp(destination + offset, destination, t);
                yield return null;
            }
            group.alpha = 1f;
            visual.anchoredPosition = destination;
        }
    }
}
