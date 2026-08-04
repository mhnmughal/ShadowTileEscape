using UnityEngine;
using UnityEngine.EventSystems;

namespace ShadowTileEscape
{
    public sealed class UiButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] RectTransform visual;
        [SerializeField] float pressedScale = 0.965f;
        [SerializeField] float selectedScale = 1.025f;

        void Awake()
        {
            if (visual == null) visual = transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData) => SetScale(pressedScale);
        public void OnPointerUp(PointerEventData eventData) => SetScale(eventData.pointerEnter == gameObject ? selectedScale : 1f);
        public void OnPointerExit(PointerEventData eventData) => SetScale(1f);
        public void OnSelect(BaseEventData eventData) => SetScale(selectedScale);
        public void OnDeselect(BaseEventData eventData) => SetScale(1f);

        void OnDisable() => SetScale(1f);
        void SetScale(float value)
        {
            if (visual != null) visual.localScale = Vector3.one * value;
        }
    }
}
