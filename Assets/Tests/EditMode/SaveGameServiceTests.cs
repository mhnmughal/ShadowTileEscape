using System;
using System.IO;
using NUnit.Framework;

namespace ShadowTileEscape.Tests
{
    public sealed class SaveGameServiceTests
    {
        string directory;
        SaveGameService service;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "shadow-tile-escape-tests", Guid.NewGuid().ToString("N"));
            service = new SaveGameService(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void RoundTripPreservesProgressAndSettings()
        {
            var save = new SaveData();
            ProgressionRules.Complete(save, 1, 10, 10, 1);
            save.settings.reducedFlashing = true;
            service.Save(save);
            var loaded = service.Load();
            Assert.That(loaded.unlockedLevel, Is.EqualTo(2));
            Assert.That(loaded.levels[0].stars, Is.EqualTo(3));
            Assert.That(loaded.levels[0].shards, Is.EqualTo(1));
            Assert.That(loaded.settings.reducedFlashing, Is.True);
        }

        [Test]
        public void CompletionNeverRegressesAndCapsUnlockAtFifteen()
        {
            var save = new SaveData();
            ProgressionRules.Complete(save, 15, 20, 12, 2);
            ProgressionRules.Complete(save, 15, 10, 12, 1);
            Assert.That(save.unlockedLevel, Is.EqualTo(15));
            Assert.That(save.levels[14].bestMoves, Is.EqualTo(10));
            Assert.That(save.levels[14].stars, Is.EqualTo(3));
            Assert.That(save.levels[14].shards, Is.EqualTo(2));
        }

        [Test]
        public void CorruptPrimaryFallsBackToLastKnownGoodBackup()
        {
            var first = new SaveData();
            ProgressionRules.Complete(first, 1, 10, 10, 1);
            service.Save(first);
            var second = service.Load();
            ProgressionRules.Complete(second, 2, 12, 12, 0);
            service.Save(second);
            File.WriteAllText(Path.Combine(directory, "shadow-tile-escape.json"), "{not valid json");
            var recovered = service.Load();
            Assert.That(recovered.levels[0].completed, Is.True);
            Assert.That(recovered.levels[1].completed, Is.False);
        }

        [Test]
        public void DefaultsAreReturnedWhenNoValidSaveExists()
        {
            var loaded = service.Load();
            Assert.That(loaded.version, Is.EqualTo(SaveGameService.CurrentVersion));
            Assert.That(loaded.unlockedLevel, Is.EqualTo(1));
            Assert.That(loaded.levels, Has.Length.EqualTo(15));
        }

        [Test]
        public void NewerSaveVersionIsNeverOverwritten()
        {
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "shadow-tile-escape.json");
            var future = new SaveData { version = SaveGameService.CurrentVersion + 1 };
            File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(future));
            var original = File.ReadAllText(path);
            service.Load();
            Assert.That(service.HasUnsupportedSave, Is.True);
            Assert.Throws<InvalidOperationException>(() => service.Save(new SaveData()));
            Assert.That(File.ReadAllText(path), Is.EqualTo(original));
        }
    }
}
