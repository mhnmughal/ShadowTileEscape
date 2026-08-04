using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowTileEscape
{
    public sealed class LevelButtonController : MonoBehaviour
    {
        [SerializeField] int levelNumber;
        [SerializeField] Button button;
        [SerializeField] TMP_Text label;

        public void Configure(int number, Button targetButton, TMP_Text targetLabel)
        {
            levelNumber = number;
            button = targetButton;
            label = targetLabel;
        }

        public void Refresh(SaveData save)
        {
            button.interactable = levelNumber <= save.unlockedLevel;
            var progress = save.levels[levelNumber - 1];
            label.text = button.interactable
                ? $"{levelNumber:00}\n{(progress.completed ? new string('*', Mathf.Max(1, progress.stars)) : "OPEN")}"
                : $"{levelNumber:00}\nLOCKED";
        }

        public void Open() => SceneManager.LoadScene($"Level_{levelNumber:00}");
    }
}
