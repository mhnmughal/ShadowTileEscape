using System;
using System.IO;
using UnityEngine;

namespace ShadowTileEscape
{
    [Serializable]
    public sealed class LevelProgress
    {
        public bool completed;
        public int stars;
        public int shards;
        public int bestMoves;
    }

    [Serializable]
    public sealed class GameSettings
    {
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.9f;
        public bool haptics = true;
        public bool reducedFlashing;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = SaveGameService.CurrentVersion;
        public int unlockedLevel = 1;
        public int lastPlayedLevel = 1;
        public bool introViewed;
        public bool tutorialViewed;
        public LevelProgress[] levels = CreateLevels();
        public GameSettings settings = new GameSettings();

        static LevelProgress[] CreateLevels()
        {
            var levels = new LevelProgress[15];
            for (var i = 0; i < levels.Length; i++) levels[i] = new LevelProgress();
            return levels;
        }
    }

    public static class ProgressionRules
    {
        public static int StarsFor(int moves, int par) => moves <= par ? 3 : moves <= par + 3 ? 2 : 1;

        public static void Complete(SaveData save, int levelNumber, int moves, int par, int shards)
        {
            if (levelNumber < 1 || levelNumber > 15) throw new ArgumentOutOfRangeException(nameof(levelNumber));
            var progress = save.levels[levelNumber - 1];
            progress.completed = true;
            progress.stars = Math.Max(progress.stars, StarsFor(moves, par));
            progress.shards = Math.Max(progress.shards, Math.Max(0, shards));
            progress.bestMoves = progress.bestMoves == 0 ? moves : Math.Min(progress.bestMoves, moves);
            save.unlockedLevel = Math.Max(save.unlockedLevel, Math.Min(15, levelNumber + 1));
        }
    }

    public sealed class SaveGameService
    {
        public const int CurrentVersion = 1;
        public static Func<string> CurrentDirectoryProvider { get; set; } = () => Application.persistentDataPath;
        readonly string primaryPath;
        readonly string backupPath;
        readonly string temporaryPath;
        public bool HasUnsupportedSave { get; private set; }

        public SaveGameService(string directory)
        {
            primaryPath = Path.Combine(directory, "shadow-tile-escape.json");
            backupPath = primaryPath + ".bak";
            temporaryPath = primaryPath + ".tmp";
        }

        public static SaveGameService ForCurrentUser() => new SaveGameService(CurrentDirectoryProvider());

        public SaveData Load()
        {
            HasUnsupportedSave = false;
            if (TryLoad(primaryPath, out var primary)) return primary;
            if (HasUnsupportedSave) return new SaveData();
            if (TryLoad(backupPath, out var backup)) return backup;
            return new SaveData();
        }

        public void Save(SaveData data)
        {
            if (HasUnsupportedSave) throw new InvalidOperationException("A newer save version exists and will not be overwritten.");
            if (!IsValid(data)) throw new InvalidDataException("Save data is outside supported bounds.");
            Directory.CreateDirectory(Path.GetDirectoryName(primaryPath));
            WriteFlushed(temporaryPath, JsonUtility.ToJson(data, true));
            if (!File.Exists(primaryPath))
            {
                File.Move(temporaryPath, primaryPath);
                return;
            }

            try
            {
                File.Replace(temporaryPath, primaryPath, backupPath);
            }
            catch (PlatformNotSupportedException)
            {
                File.Copy(primaryPath, backupPath, true);
                File.Delete(primaryPath);
                File.Move(temporaryPath, primaryPath);
            }
        }

        bool TryLoad(string path, out SaveData data)
        {
            data = null;
            if (!File.Exists(path)) return false;
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data != null && data.version > CurrentVersion)
                {
                    HasUnsupportedSave = true;
                    data = null;
                    return false;
                }
                return IsValid(data);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                return false;
            }
        }

        static bool IsValid(SaveData data)
        {
            if (data == null || data.version != CurrentVersion || data.unlockedLevel < 1 || data.unlockedLevel > 15) return false;
            if (data.lastPlayedLevel < 1 || data.lastPlayedLevel > 15) return false;
            if (data.levels == null || data.levels.Length != 15 || data.settings == null) return false;
            for (var i = 0; i < data.levels.Length; i++)
            {
                var level = data.levels[i];
                if (level == null || level.stars < 0 || level.stars > 3 || level.shards < 0 || level.bestMoves < 0) return false;
            }
            return data.settings.musicVolume >= 0 && data.settings.musicVolume <= 1
                && data.settings.sfxVolume >= 0 && data.settings.sfxVolume <= 1;
        }

        public void ResetConfirmed()
        {
            if (File.Exists(primaryPath)) File.Delete(primaryPath);
            if (File.Exists(backupPath)) File.Delete(backupPath);
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            HasUnsupportedSave = false;
        }

        static void WriteFlushed(string path, string json)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(true);
            }
        }
    }
}
