using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace ShadowTileEscape
{
    public sealed class StaticScreenController : MonoBehaviour
    {
        [SerializeField] string destination = "MainMenu";
        [SerializeField] TMP_Text completionTotals;
        public string Destination { set => destination = value; }

        void Awake()
        {
            if (completionTotals == null) return;
            var save = SaveGameService.ForCurrentUser().Load();
            var stars = 0;
            var shards = 0;
            var completed = 0;
            foreach (var progress in save.levels)
            {
                stars += progress.stars;
                shards += progress.shards;
                if (progress.completed) completed++;
            }
            completionTotals.text = $"{completed}/15 HALLS  ·  {stars}/45 STARS  ·  {shards} SHARDS";
        }

        void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            if (SceneManager.GetActiveScene().name == "Completion") MainMenu();
            else if (SceneManager.GetActiveScene().name != "Intro") Continue();
        }

        public void Continue()
        {
            MarkViewed();
            SceneManager.LoadScene(destination);
        }
        public void Skip() => Continue();
        public void MainMenu() => SceneManager.LoadScene("MainMenu");
        public void LevelSelect() => SceneManager.LoadScene("LevelSelect");
        public void ReplayFinale() => SceneManager.LoadScene("Level_15");
        public void Credits() => SceneManager.LoadScene("Credits");

        void MarkViewed()
        {
            var scene = SceneManager.GetActiveScene().name;
            if (scene != "Intro" && scene != "HowToPlay") return;
            var service = SaveGameService.ForCurrentUser();
            var save = service.Load();
            if (scene == "Intro") save.introViewed = true;
            else save.tutorialViewed = true;
            if (!service.HasUnsupportedSave) service.Save(save);
        }
    }
}
