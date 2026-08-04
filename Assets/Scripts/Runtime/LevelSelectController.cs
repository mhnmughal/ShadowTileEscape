using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace ShadowTileEscape
{
    public sealed class LevelSelectController : MonoBehaviour
    {
        [SerializeField] LevelButtonController[] levelButtons;
        [SerializeField] TMP_Text[] chapterProgressLabels;
        [SerializeField] TMP_Text overallProgressLabel;
        public LevelButtonController[] LevelButtons { set => levelButtons = value; }
        public TMP_Text[] ChapterProgressLabels { set => chapterProgressLabels = value; }
        public TMP_Text OverallProgressLabel { set => overallProgressLabel = value; }

        void Awake()
        {
            var save = SaveGameService.ForCurrentUser().Load();
            for (var i = 0; i < levelButtons.Length; i++) levelButtons[i].Refresh(save);
            var totalCompleted = 0;
            var totalStars = 0;
            var totalShards = 0;
            for (var chapter = 0; chapter < 3; chapter++)
            {
                var completed = 0;
                var stars = 0;
                var shards = 0;
                for (var i = chapter * 5; i < chapter * 5 + 5; i++)
                {
                    if (save.levels[i].completed) completed++;
                    stars += save.levels[i].stars;
                    shards += save.levels[i].shards;
                }
                chapterProgressLabels[chapter].text = $"{completed}/5 COMPLETE  ·  {stars}/15 STARS  ·  {shards} SHARDS";
                totalCompleted += completed;
                totalStars += stars;
                totalShards += shards;
            }
            overallProgressLabel.text = $"PALACE JOURNEY  {totalCompleted}/15  ·  {totalStars}/45 STARS  ·  {totalShards} SHARDS";
        }

        public void Back() => SceneManager.LoadScene("MainMenu");

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Back();
        }
    }
}
