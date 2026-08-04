using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShadowTileEscape.Tests
{
    public sealed class UiRedesignFlowTests
    {
        string saveDirectory;

        [SetUp]
        public void SetUp()
        {
            saveDirectory = Path.Combine(Application.temporaryCachePath, $"shadow-ui-tests-{Guid.NewGuid():N}");
            SaveGameService.CurrentDirectoryProvider = () => saveDirectory;
        }

        [TearDown]
        public void TearDown()
        {
            SaveGameService.CurrentDirectoryProvider = () => Application.persistentDataPath;
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        [UnityTest]
        public IEnumerator MainMenuPrimaryStateNewGameConfirmationAndCreditsRelocationWork()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;
            Assert.That(Find("ContinueButton").GetComponent<Button>().interactable, Is.False);
            Assert.That(TryFind("CreditsButton"), Is.Null);

            var service = SaveGameService.ForCurrentUser();
            var save = service.Load();
            ProgressionRules.Complete(save, 1, 12, 15, 1);
            save.settings.musicVolume = 0.31f;
            service.Save(save);
            SceneManager.LoadScene("MainMenu");
            yield return null;
            Assert.That(Find("ContinueButton").GetComponent<Button>().interactable, Is.True);

            Click("NewGameButton");
            Assert.That(Find("NewGameConfirmation").activeSelf, Is.True);
            Assert.That(Find("MenuContent").GetComponent<CanvasGroup>().interactable, Is.False);
            Click("CancelNewGame");
            Assert.That(Find("NewGameConfirmation").activeSelf, Is.False);

            Click("SettingsButton");
            Assert.That(Find("SettingsPanel").activeSelf, Is.True);
            Click("CreditsAndLicenses");
            Assert.That(Find("CreditsPanel").activeSelf, Is.True);
            Assert.That(Find("CreditsViewport").GetComponent<ScrollRect>(), Is.Not.Null);
            Click("CloseCredits");
            Click("CloseSettings");
        }

        [UnityTest]
        public IEnumerator SettingsPersistMixerComfortTutorialAndSettingsPreservingProgressReset()
        {
            var service = SaveGameService.ForCurrentUser();
            var seeded = service.Load();
            ProgressionRules.Complete(seeded, 1, 13, 15, 1);
            seeded.tutorialViewed = true;
            service.Save(seeded);
            SceneManager.LoadScene("MainMenu");
            yield return null;
            Click("SettingsButton");

            var settings = Object.FindFirstObjectByType<SettingsController>();
            var music = Find("MusicSlider").GetComponent<Slider>();
            var sfx = Find("SfxSlider").GetComponent<Slider>();
            music.value = 0.25f;
            sfx.value = 0.5f;
            Find("HapticsToggle").GetComponent<Toggle>().isOn = false;
            Find("ReducedFlashingToggle").GetComponent<Toggle>().isOn = true;
            var mixer = (AudioMixer)typeof(SettingsController).GetField("mixer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(settings);
            Assert.That(mixer.GetFloat("MusicVolume", out var musicDb), Is.True);
            Assert.That(musicDb, Is.EqualTo(Mathf.Log10(0.25f) * 20f).Within(0.01f));
            Assert.That(mixer.GetFloat("SFXVolume", out var sfxDb), Is.True);
            Assert.That(sfxDb, Is.EqualTo(Mathf.Log10(0.5f) * 20f).Within(0.01f));

            Click("ResetTutorial");
            Click("ConfirmTutorialReset");
            Assert.That(service.Load().tutorialViewed, Is.False);

            Click("ResetProgress");
            Click("ConfirmReset");
            var reset = service.Load();
            Assert.That(reset.unlockedLevel, Is.EqualTo(1));
            Assert.That(reset.levels[0].completed, Is.False);
            Assert.That(reset.settings.musicVolume, Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(reset.settings.sfxVolume, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(reset.settings.haptics, Is.False);
            Assert.That(reset.settings.reducedFlashing, Is.True);
            Assert.That(Find("ContinueButton").GetComponent<Button>().interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator LevelSelectRendersChapterAndPerLevelProgress()
        {
            var service = SaveGameService.ForCurrentUser();
            var save = service.Load();
            ProgressionRules.Complete(save, 1, 15, 15, 1);
            ProgressionRules.Complete(save, 2, 8, 10, 1);
            save.lastPlayedLevel = 3;
            service.Save(save);
            SceneManager.LoadScene("LevelSelect");
            yield return null;

            Assert.That(Find("OverallProgress").GetComponent<TMP_Text>().text, Does.Contain("2/15"));
            Assert.That(Find("Chapter_01/ChapterProgress").GetComponent<TMP_Text>().text, Does.Contain("2/5"));
            Assert.That(Find("LevelButton_01/Progress").GetComponent<TMP_Text>().text, Does.Contain("BEST 15"));
            Assert.That(Find("LevelButton_02/Progress").GetComponent<TMP_Text>().text, Does.Contain("STARS 3/3"));
            Assert.That(Find("LevelButton_03/Progress").GetComponent<TMP_Text>().text, Is.EqualTo("CURRENT PATH"));
            Assert.That(Find("LevelButton_04").GetComponent<Button>().interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator GameplayHintPauseSettingsAndModalCommandBlockingWork()
        {
            SceneManager.LoadScene("Level_01");
            yield return null;
            var controller = Object.FindFirstObjectByType<GameplayController>();
            controller.ShowHint();
            Assert.That(Find("HintPanel").activeSelf, Is.True);
            controller.MoveNorth();
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(0, 0)));
            controller.HideHint();
            controller.MoveNorth();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(controller.CurrentState.player, Is.EqualTo(new GridCoord(0, 1)));

            controller.TogglePause();
            Click("PauseSettings");
            Assert.That(Find("SettingsPanel").activeSelf, Is.True);
            Assert.That(Find("PausePanel").GetComponent<CanvasGroup>().interactable, Is.False);
            Click("CloseSettings");
            Assert.That(Find("PausePanel").GetComponent<CanvasGroup>().interactable, Is.True);
            controller.Resume();
        }

        [UnityTest]
        public IEnumerator CompletionShowsAggregateResultsAndEveryRequiredDestination()
        {
            var service = SaveGameService.ForCurrentUser();
            var save = service.Load();
            for (var level = 1; level <= 15; level++) ProgressionRules.Complete(save, level, 10, 10, 1);
            service.Save(save);
            SceneManager.LoadScene("Completion");
            yield return null;
            Assert.That(Find("CompletionTotals").GetComponent<TMP_Text>().text, Does.Contain("15/15 HALLS"));
            Assert.That(Find("CompletionTotals").GetComponent<TMP_Text>().text, Does.Contain("45/45 STARS"));
            Assert.That(Find("ReplayFinale").GetComponent<Button>(), Is.Not.Null);
            Assert.That(Find("CompletionLevelSelect").GetComponent<Button>(), Is.Not.Null);
            Assert.That(Find("CompletionMainMenu").GetComponent<Button>(), Is.Not.Null);
            Assert.That(Find("CompletionCredits").GetComponent<Button>(), Is.Not.Null);
        }

        static void Click(string path) => Find(path).GetComponent<Button>().onClick.Invoke();

        static GameObject Find(string path)
        {
            var segments = path.Split('/');
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var current = root.name == segments[0] ? root.transform : FindChildRecursive(root.transform, segments[0]);
                if (current == null) continue;
                var index = 1;
                for (; index < segments.Length && current != null; index++) current = FindChild(current, segments[index]);
                if (current != null && index == segments.Length) return current.gameObject;
            }
            Assert.Fail($"GameObject '{path}' not found in {SceneManager.GetActiveScene().name}.");
            return null;
        }

        static GameObject TryFind(string name)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var result = FindChildRecursive(root.transform, name);
                if (result != null) return result.gameObject;
            }
            return null;
        }

        static Transform FindChild(Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++) if (parent.GetChild(i).name == name) return parent.GetChild(i);
            return null;
        }

        static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
