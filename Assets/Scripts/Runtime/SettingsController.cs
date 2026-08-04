using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowTileEscape
{
    public sealed class SettingsController : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] GameObject resetConfirmation;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Toggle hapticsToggle;
        [SerializeField] Toggle reducedFlashingToggle;
        [SerializeField] TMP_Text status;
        [SerializeField] AudioSource musicSource;
        SaveGameService service;
        SaveData save;
        bool loading;

        void Awake()
        {
            service = SaveGameService.ForCurrentUser();
            save = service.Load();
            loading = true;
            musicSlider.value = save.settings.musicVolume;
            sfxSlider.value = save.settings.sfxVolume;
            hapticsToggle.isOn = save.settings.haptics;
            reducedFlashingToggle.isOn = save.settings.reducedFlashing;
            loading = false;
            panel.SetActive(false);
            resetConfirmation.SetActive(false);
        }

        public void Show() => panel.SetActive(true);
        public void Hide() => panel.SetActive(false);
        public void SetMusic(float value) { save.settings.musicVolume = value; if (musicSource != null) musicSource.volume = 0.14f * value; Persist(); }
        public void SetSfx(float value) { save.settings.sfxVolume = value; Persist(); }
        public void SetHaptics(bool value) { save.settings.haptics = value; Persist(); }
        public void SetReducedFlashing(bool value) { save.settings.reducedFlashing = value; Persist(); }
        public void AskReset() => resetConfirmation.SetActive(true);
        public void CancelReset() => resetConfirmation.SetActive(false);
        public void ConfirmReset()
        {
            service.ResetConfirmed();
            save = new SaveData();
            resetConfirmation.SetActive(false);
            status.text = "Progress reset.";
        }

        void Persist()
        {
            if (!loading && !service.HasUnsupportedSave) service.Save(save);
        }
    }
}
