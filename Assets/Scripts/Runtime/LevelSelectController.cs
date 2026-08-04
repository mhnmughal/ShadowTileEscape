using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowTileEscape
{
    public sealed class LevelSelectController : MonoBehaviour
    {
        [SerializeField] LevelButtonController[] levelButtons;
        public LevelButtonController[] LevelButtons { set => levelButtons = value; }

        void Awake()
        {
            var save = SaveGameService.ForCurrentUser().Load();
            for (var i = 0; i < levelButtons.Length; i++) levelButtons[i].Refresh(save);
        }

        public void Back() => SceneManager.LoadScene("MainMenu");
    }
}
