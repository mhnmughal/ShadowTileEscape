using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowTileEscape
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] Button continueButton;
        [SerializeField] Button newGameButton;
        [SerializeField] TMP_Text continueLabel;
        [SerializeField] TMP_Text saveSummaryLabel;
        [SerializeField] GameObject newGameConfirmation;
        [SerializeField] CanvasGroup menuContent;
        SaveGameService service;
        SaveData save;

        void Awake()
        {
            service = SaveGameService.ForCurrentUser();
            newGameConfirmation.SetActive(false);
            RefreshFromSave();
        }

        public void RefreshFromSave()
        {
            save = service.Load();
            var hasProgress = save.unlockedLevel > 1 || save.levels[0].completed;
            continueButton.interactable = hasProgress;
            continueLabel.text = hasProgress ? $"Continue  ·  Level {save.lastPlayedLevel:00}" : "Continue  ·  No journey yet";
            if (newGameButton != null) newGameButton.image.color = hasProgress
                ? new Color32(31, 37, 78, 255)
                : new Color32(177, 126, 45, 255);
            if (saveSummaryLabel != null)
            {
                var completed = 0;
                var stars = 0;
                foreach (var progress in save.levels) { if (progress.completed) completed++; stars += progress.stars; }
                saveSummaryLabel.text = hasProgress
                    ? $"{completed}/15 halls cleared  ·  {stars}/45 stars"
                    : "Begin Noor's first crossing";
            }
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject((hasProgress ? continueButton : newGameButton).gameObject);
        }

        public void Continue()
        {
            if (!continueButton.interactable) { NewGame(); return; }
            SceneManager.LoadScene($"Level_{Mathf.Clamp(save.lastPlayedLevel, 1, save.unlockedLevel):00}");
        }
        public void NewGame()
        {
            if (save.unlockedLevel > 1 || save.levels[0].completed)
            {
                newGameConfirmation.SetActive(true);
                SetMenuInteractive(false);
            }
            else SceneManager.LoadScene("Intro");
        }
        public void ConfirmNewGame()
        {
            var retainedSettings = save.settings;
            service.ResetConfirmed();
            service.Save(new SaveData { settings = retainedSettings });
            SceneManager.LoadScene("Intro");
        }
        public void CancelNewGame() { newGameConfirmation.SetActive(false); SetMenuInteractive(true); }
        public void LevelSelect() => SceneManager.LoadScene("LevelSelect");
        public void HowToPlay() => SceneManager.LoadScene("HowToPlay");

        void Update()
        {
            if (newGameConfirmation.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CancelNewGame();
        }

        void SetMenuInteractive(bool value)
        {
            if (menuContent == null) return;
            menuContent.interactable = value;
            menuContent.blocksRaycasts = value;
        }
    }
}
