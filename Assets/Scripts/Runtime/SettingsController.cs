using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ShadowTileEscape
{
    public sealed class SettingsController : MonoBehaviour
    {
        const string MusicParameter = "MusicVolume";
        const string SfxParameter = "SFXVolume";

        [SerializeField] GameObject panel;
        [SerializeField] GameObject resetConfirmation;
        [SerializeField] GameObject tutorialResetConfirmation;
        [SerializeField] GameObject creditsPanel;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Toggle hapticsToggle;
        [SerializeField] Toggle reducedFlashingToggle;
        [SerializeField] TMP_Text musicValueLabel;
        [SerializeField] TMP_Text sfxValueLabel;
        [SerializeField] TMP_Text status;
        [SerializeField] AudioMixer mixer;
        [SerializeField] AudioSource musicSource;
        [SerializeField] MainMenuController mainMenuController;
        [SerializeField] CanvasGroup backgroundControls;
        [SerializeField] CanvasGroup settingsControls;
        SaveGameService service;
        SaveData save;
        bool loading;

        public bool IsOpen => panel != null && panel.activeSelf;

        void Awake()
        {
            service = SaveGameService.ForCurrentUser();
            LoadControls();
            panel.SetActive(false);
            resetConfirmation.SetActive(false);
            tutorialResetConfirmation.SetActive(false);
            creditsPanel.SetActive(false);
        }

        void Update()
        {
            if (!IsOpen || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            if (creditsPanel.activeSelf) HideCredits();
            else if (resetConfirmation.activeSelf) CancelReset();
            else if (tutorialResetConfirmation.activeSelf) CancelTutorialReset();
            else Hide();
        }

        public void Show()
        {
            LoadControls();
            status.text = "Settings save automatically.";
            resetConfirmation.SetActive(false);
            tutorialResetConfirmation.SetActive(false);
            creditsPanel.SetActive(false);
            panel.SetActive(true);
            SetSettingsInteractive(true);
            SetBackgroundInteractive(false);
        }

        public void Hide()
        {
            resetConfirmation.SetActive(false);
            tutorialResetConfirmation.SetActive(false);
            creditsPanel.SetActive(false);
            panel.SetActive(false);
            SetSettingsInteractive(true);
            SetBackgroundInteractive(true);
        }

        public void SetMusic(float value)
        {
            save.settings.musicVolume = Mathf.Clamp01(value);
            ApplyMixer(MusicParameter, save.settings.musicVolume);
            if (mixer == null && musicSource != null) musicSource.volume = 0.14f * save.settings.musicVolume;
            musicValueLabel.text = $"{Mathf.RoundToInt(save.settings.musicVolume * 100f)}%";
            Persist("Music level saved.");
        }

        public void SetSfx(float value)
        {
            save.settings.sfxVolume = Mathf.Clamp01(value);
            ApplyMixer(SfxParameter, save.settings.sfxVolume);
            sfxValueLabel.text = $"{Mathf.RoundToInt(save.settings.sfxVolume * 100f)}%";
            Persist("Sound level saved.");
        }

        public void SetHaptics(bool value)
        {
            save.settings.haptics = value;
            Persist(value ? "Haptics on." : "Haptics off.");
        }

        public void SetReducedFlashing(bool value)
        {
            save.settings.reducedFlashing = value;
            Persist(value ? "Reduced flashing on." : "Reduced flashing off.");
        }

        public void AskTutorialReset() { tutorialResetConfirmation.SetActive(true); SetSettingsInteractive(false); }
        public void CancelTutorialReset() { tutorialResetConfirmation.SetActive(false); SetSettingsInteractive(true); }
        public void ConfirmTutorialReset()
        {
            save.tutorialViewed = false;
            Persist("Tutorial tips will appear again.");
            tutorialResetConfirmation.SetActive(false);
            SetSettingsInteractive(true);
        }

        public void AskReset() { resetConfirmation.SetActive(true); SetSettingsInteractive(false); }
        public void CancelReset() { resetConfirmation.SetActive(false); SetSettingsInteractive(true); }
        public void ConfirmReset()
        {
            var retainedSettings = save.settings;
            service.ResetConfirmed();
            save = new SaveData { settings = retainedSettings };
            service.Save(save);
            resetConfirmation.SetActive(false);
            SetSettingsInteractive(true);
            status.text = "Progress reset. Audio and comfort settings kept.";
            mainMenuController?.RefreshFromSave();
            if (SceneManager.GetActiveScene().name != "MainMenu") SceneManager.LoadScene("MainMenu");
        }

        public void ShowCredits()
        {
            creditsPanel.SetActive(true);
            SetSettingsInteractive(false);
            resetConfirmation.SetActive(false);
            tutorialResetConfirmation.SetActive(false);
        }

        public void HideCredits() { creditsPanel.SetActive(false); SetSettingsInteractive(true); }

        void LoadControls()
        {
            save = service.Load();
            loading = true;
            musicSlider.SetValueWithoutNotify(save.settings.musicVolume);
            sfxSlider.SetValueWithoutNotify(save.settings.sfxVolume);
            hapticsToggle.SetIsOnWithoutNotify(save.settings.haptics);
            reducedFlashingToggle.SetIsOnWithoutNotify(save.settings.reducedFlashing);
            musicValueLabel.text = $"{Mathf.RoundToInt(save.settings.musicVolume * 100f)}%";
            sfxValueLabel.text = $"{Mathf.RoundToInt(save.settings.sfxVolume * 100f)}%";
            ApplyMixer(MusicParameter, save.settings.musicVolume);
            ApplyMixer(SfxParameter, save.settings.sfxVolume);
            loading = false;
        }

        void ApplyMixer(string parameter, float linearValue)
        {
            if (mixer != null) mixer.SetFloat(parameter, LinearToDecibels(linearValue));
        }

        static float LinearToDecibels(float value) => value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;

        void Persist(string message)
        {
            if (loading || service.HasUnsupportedSave) return;
            service.Save(save);
            if (status != null) status.text = message;
        }

        void SetBackgroundInteractive(bool value)
        {
            if (backgroundControls == null) return;
            backgroundControls.interactable = value;
            backgroundControls.blocksRaycasts = value;
        }

        void SetSettingsInteractive(bool value)
        {
            if (settingsControls == null) return;
            settingsControls.interactable = value;
            settingsControls.blocksRaycasts = value;
        }
    }
}
