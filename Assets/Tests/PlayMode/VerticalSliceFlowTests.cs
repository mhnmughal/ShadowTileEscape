using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ShadowTileEscape.Tests
{
    public sealed class VerticalSliceFlowTests
    {
        [UnityTest]
        public IEnumerator BootAndMenuButtonsReachLevelOne()
        {
            SceneManager.LoadScene("Boot");
            yield return null;
            GameObject.Find("BeginButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/VerticalSlice_MainMenu.png", 2);
            yield return new WaitForSeconds(0.2f);
            GameObject.Find("StartButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Level_01"));
            Assert.That(Object.FindFirstObjectByType<GameplayController>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SerializedTouchButtonAndSolutionCompleteLevel()
        {
            SceneManager.LoadScene("Level_01");
            yield return null;
            var controller = Object.FindFirstObjectByType<GameplayController>();
            Assert.That(controller, Is.Not.Null);

            GameObject.Find("MoveNorth").GetComponent<Button>().onClick.Invoke();
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(0, 1)));
            controller.MoveNorth();
            controller.MoveNorth();
            controller.MoveNorth();
            controller.MoveEast();
            controller.MoveEast();
            controller.MoveEast();
            controller.MoveEast();
            controller.MoveEast();
            controller.MoveEast();
            yield return null;

            Assert.That(controller.CurrentState.completed, Is.True);
            Assert.That(controller.CurrentState.moveCount, Is.EqualTo(10));
            Assert.That(controller.CurrentState.ShardsCollected, Is.EqualTo(1));
            Assert.That(GameObject.Find("VictoryPanel"), Is.Not.Null);
            var persisted = SaveGameService.ForCurrentUser().Load();
            Assert.That(persisted.levels[0].completed, Is.True);
            Assert.That(persisted.levels[0].stars, Is.EqualTo(3));
            Assert.That(persisted.unlockedLevel, Is.GreaterThanOrEqualTo(2));
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/VerticalSlice_Level01_Completed.png", 2);
            yield return new WaitForEndOfFrame();
        }

        [UnityTest]
        public IEnumerator ExposureFailsAndUndoRestoresAcceptedPreTurnState()
        {
            SceneManager.LoadScene("Level_01");
            yield return null;
            var controller = Object.FindFirstObjectByType<GameplayController>();
            for (var i = 0; i < 6; i++) controller.MoveEast();
            controller.MoveNorth();
            controller.MoveNorth();
            Assert.That(controller.CurrentState.failed, Is.True);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(6, 2)));
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/VerticalSlice_Level01_Failure.png", 2);
            yield return new WaitForSeconds(0.2f);

            controller.Undo();
            yield return null;
            Assert.That(controller.CurrentState.failed, Is.False);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(6, 1)));
            Assert.That(controller.CurrentState.moveCount, Is.EqualTo(7));
        }
    }
}
