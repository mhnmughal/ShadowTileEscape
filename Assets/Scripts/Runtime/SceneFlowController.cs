using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowTileEscape
{
    public sealed class SceneFlowController : MonoBehaviour
    {
        [SerializeField] string destination = "MainMenu";
        public void LoadDestination() => SceneManager.LoadScene(destination);
        public void LoadLevelOne() => SceneManager.LoadScene("Level_01");
        public void LoadMainMenu() => SceneManager.LoadScene("MainMenu");
        public void Quit() => Application.Quit();
    }
}
