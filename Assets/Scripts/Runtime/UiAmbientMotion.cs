using UnityEngine;

namespace ShadowTileEscape
{
    public sealed class UiAmbientMotion : MonoBehaviour
    {
        [SerializeField] RectTransform[] particles;
        Vector2[] origins;
        float motionScale = 1f;

        void Awake()
        {
            origins = new Vector2[particles.Length];
            for (var i = 0; i < particles.Length; i++) origins[i] = particles[i].anchoredPosition;
            if (SaveGameService.ForCurrentUser().Load().settings.reducedFlashing) motionScale = 0.3f;
        }

        void Update()
        {
            var time = Time.unscaledTime;
            for (var i = 0; i < particles.Length; i++)
            {
                var phase = time * (0.18f + i * 0.013f) + i * 1.7f;
                particles[i].anchoredPosition = origins[i] + new Vector2(Mathf.Sin(phase) * 8f, Mathf.Cos(phase * 0.7f) * 12f) * motionScale;
            }
        }
    }
}
