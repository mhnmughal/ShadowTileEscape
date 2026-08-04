using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShadowTileEscape.Tests
{
    public sealed class VerticalSliceFlowTests
    {
        string saveDirectory;

        [SetUp]
        public void SetUp()
        {
            saveDirectory = Path.Combine(Application.temporaryCachePath, $"shadow-tile-escape-tests-{Guid.NewGuid():N}");
            SaveGameService.CurrentDirectoryProvider = () => saveDirectory;
        }

        [TearDown]
        public void TearDown()
        {
            SaveGameService.CurrentDirectoryProvider = () => Application.persistentDataPath;
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        [UnityTest]
        public IEnumerator BootMenuAndLevelSelectReachLevelOne()
        {
            SceneManager.LoadScene("Boot");
            yield return null;
            GameObject.Find("BeginButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/VerticalSlice_MainMenu.png", 2);
            yield return new WaitForSeconds(0.2f);
            GameObject.Find("LevelSelectButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("LevelSelect"));
            var levelOne = GameObject.Find("LevelButton_01").GetComponent<Button>();
            Assert.That(levelOne.interactable, Is.True);
            Assert.That(GameObject.Find("LevelButton_02").GetComponent<Button>().interactable, Is.False);
            levelOne.onClick.Invoke();
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
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(0, 1)));
            yield return Execute(controller.MoveEast, controller.Interact, controller.MoveNorth, controller.MoveEast,
                controller.Interact, controller.MoveNorth, controller.MoveNorth, controller.MoveEast,
                controller.MoveEast, controller.MoveEast, controller.MoveSouth, controller.Interact,
                controller.MoveNorth, controller.MoveEast);
            yield return null;

            Assert.That(controller.CurrentState.completed, Is.True);
            Assert.That(controller.CurrentState.moveCount, Is.EqualTo(15));
            Assert.That(controller.CurrentState.ShardsCollected, Is.EqualTo(1));
            Assert.That(controller.CurrentState.boxPushes, Is.EqualTo(1));
            Assert.That(controller.CurrentState.lampRotations, Is.EqualTo(1));
            Assert.That(controller.CurrentState.mirrorRotations, Is.EqualTo(1));
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
            yield return Execute(controller.MoveNorth, controller.MoveNorth, controller.MoveNorth,
                controller.MoveEast, controller.MoveEast, controller.MoveEast, controller.MoveEast, controller.MoveSouth);
            Assert.That(controller.CurrentState.failed, Is.True);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(4, 2)));
            ScreenCapture.CaptureScreenshot("Assets/Screenshots/VerticalSlice_Level01_Failure.png", 2);
            yield return new WaitForSeconds(0.2f);

            controller.Undo();
            yield return null;
            Assert.That(controller.CurrentState.failed, Is.False);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(4, 3)));
            Assert.That(controller.CurrentState.moveCount, Is.EqualTo(7));
        }

        [UnityTest]
        public IEnumerator PausePanelBlocksCommandsAndResumeRestoresInput()
        {
            SceneManager.LoadScene("Level_01");
            yield return null;
            var controller = Object.FindFirstObjectByType<GameplayController>();
            controller.TogglePause();
            Assert.That(GameObject.Find("PausePanel"), Is.Not.Null);
            controller.MoveNorth();
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(0, 0)));
            controller.Resume();
            controller.MoveNorth();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(0, 1)));
        }

        [UnityTest]
        public IEnumerator EverySerializedLevelSceneCompletesAndFinaleOpensCompletion()
        {
            var definitionField = typeof(GameplayController).GetField("definition", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(definitionField, Is.Not.Null);
            for (var level = 1; level <= 15; level++)
            {
                SceneManager.LoadScene($"Level_{level:00}");
                yield return null;
                var controller = Object.FindFirstObjectByType<GameplayController>();
                Assert.That(controller, Is.Not.Null, $"Level {level:00} controller");
                var definition = (LevelDefinition)definitionField.GetValue(controller);
                foreach (var token in definition.verifiedSolution.Split(','))
                {
                    switch (token)
                    {
                        case "N": controller.MoveNorth(); break;
                        case "E": controller.MoveEast(); break;
                        case "S": controller.MoveSouth(); break;
                        case "W": controller.MoveWest(); break;
                        case "I": controller.Interact(); break;
                        default: Assert.Fail($"Unknown solution token '{token}' in Level {level:00}"); break;
                    }
                    yield return new WaitForSecondsRealtime(0.09f);
                }
                Assert.That(controller.CurrentState.completed, Is.True, $"Level {level:00} serialized solution");
            }

            GameObject.Find("NextLevel").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Completion"));
        }

        static IEnumerator Execute(params Action[] commands)
        {
            foreach (var command in commands)
            {
                command();
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
}
