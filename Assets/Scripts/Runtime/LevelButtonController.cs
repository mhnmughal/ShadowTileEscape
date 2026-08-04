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
        [SerializeField] TMP_Text numberLabel;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text progressLabel;
        [SerializeField] Image stateRail;
        [SerializeField] string levelTitle;

        public void Configure(int number, string title, Button targetButton, TMP_Text targetNumber,
            TMP_Text targetTitle, TMP_Text targetProgress, Image targetRail)
        {
            levelNumber = number;
            levelTitle = title;
            button = targetButton;
            numberLabel = targetNumber;
            titleLabel = targetTitle;
            progressLabel = targetProgress;
            stateRail = targetRail;
        }

        public void Refresh(SaveData save)
        {
            button.interactable = levelNumber <= save.unlockedLevel;
            var progress = save.levels[levelNumber - 1];
            numberLabel.text = $"{levelNumber:00}";
            titleLabel.text = levelTitle;
            if (!button.interactable)
            {
                progressLabel.text = "LOCKED";
                stateRail.color = new Color32(73, 79, 104, 255);
            }
            else if (progress.completed)
            {
                progressLabel.text = $"STARS {progress.stars}/3  ·  SHARD {progress.shards}  ·  BEST {progress.bestMoves}";
                stateRail.color = new Color32(99, 217, 230, 255);
            }
            else if (save.lastPlayedLevel == levelNumber)
            {
                progressLabel.text = "CURRENT PATH";
                stateRail.color = new Color32(242, 184, 75, 255);
            }
            else
            {
                progressLabel.text = "OPEN";
                stateRail.color = new Color32(154, 120, 212, 255);
            }
        }

        public void Open() => SceneManager.LoadScene($"Level_{levelNumber:00}");
    }
}
