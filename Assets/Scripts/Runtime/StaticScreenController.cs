using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowTileEscape
{
    public sealed class StaticScreenController : MonoBehaviour
    {
        [SerializeField] string destination = "MainMenu";
        public string Destination { set => destination = value; }
        public void Continue() => SceneManager.LoadScene(destination);
        public void MainMenu() => SceneManager.LoadScene("MainMenu");
        public void LevelSelect() => SceneManager.LoadScene("LevelSelect");
        public void ReplayFinale() => SceneManager.LoadScene("Level_15");
    }
}
