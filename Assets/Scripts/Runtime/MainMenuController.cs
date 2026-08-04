using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowTileEscape
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] Button continueButton;
        [SerializeField] TMP_Text continueLabel;
        [SerializeField] GameObject newGameConfirmation;
        SaveGameService service;
        SaveData save;

        void Awake()
        {
            service = SaveGameService.ForCurrentUser();
            save = service.Load();
            continueButton.interactable = save.unlockedLevel > 1 || save.levels[0].completed;
            continueLabel.text = continueButton.interactable ? $"CONTINUE  ·  LEVEL {save.lastPlayedLevel:00}" : "CONTINUE  ·  NO SAVE";
            newGameConfirmation.SetActive(false);
        }

        public void Continue() => SceneManager.LoadScene($"Level_{Mathf.Clamp(save.lastPlayedLevel, 1, 15):00}");
        public void NewGame()
        {
            if (save.unlockedLevel > 1 || save.levels[0].completed) newGameConfirmation.SetActive(true);
            else SceneManager.LoadScene("Intro");
        }
        public void ConfirmNewGame() { service.ResetConfirmed(); SceneManager.LoadScene("Intro"); }
        public void CancelNewGame() => newGameConfirmation.SetActive(false);
        public void LevelSelect() => SceneManager.LoadScene("LevelSelect");
        public void HowToPlay() => SceneManager.LoadScene("HowToPlay");
        public void Credits() => SceneManager.LoadScene("Credits");
    }
}
