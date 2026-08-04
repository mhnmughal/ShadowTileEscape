using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowTileEscape.Editor
{
    public static class VerticalSliceBuilder
    {
        const string LevelAssetPath = "Assets/Data/Levels/Level_01.asset";
        const string FontPath = "Assets/Fonts/NunitoSans/NunitoSans-Variable SDF.asset";
        const string MixerPath = "Assets/Audio/ShadowTileEscape.mixer";
        const string AppIconPath = "Assets/Art/AppIcon/ShadowTileEscape_AppIcon.png";

        static readonly Color Navy = new Color32(9, 13, 29, 255);
        static readonly Color Indigo = new Color32(21, 26, 58, 255);
        static readonly Color Palace = new Color32(37, 40, 90, 255);
        static readonly Color Cyan = new Color32(99, 217, 230, 255);
        static readonly Color Gold = new Color32(242, 184, 75, 255);
        static readonly Color Violet = new Color32(154, 120, 212, 255);
        static readonly Color Ivory = new Color32(244, 238, 221, 255);
        static readonly Color Orange = new Color32(233, 106, 71, 255);

        static TMP_FontAsset font;

        [MenuItem("Shadow Tile Escape/Build/Build Full Game")]
        [MenuItem("Shadow Tile Escape/Build/Build Vertical Slice")]
        public static void Build()
        {
            EnsureFolders();
            EnsureAudioAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureAudioMixer();
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) throw new InvalidOperationException($"Nunito Sans TMP asset missing at {FontPath}");

            var definitions = BuildLevelDefinitions();
            BuildBootScene();
            BuildMenuScene();
            BuildIntroScene();
            BuildHowToPlayScene();
            BuildLevelSelectScene();
            for (var i = 0; i < definitions.Length; i++) BuildLevelScene(definitions[i]);
            BuildCreditsScene();
            BuildCompletionScene();
            ConfigureProject();
            ConfigureBranding();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene("Assets/Scenes/Levels/Level_01.unity");
            Debug.Log("[ShadowTileEscape] Core level pack built: Boot, MainMenu, 15 serialized levels/definitions, build settings, audio, and mobile settings.");
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets", "Audio");
            EnsureFolder("Assets/Audio", "Generated");
            EnsureFolder("Assets/Data", "Levels");
            EnsureFolder("Assets/Scenes", "Levels");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        static LevelDefinition BuildLevelDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelAssetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(definition, LevelAssetPath);
            }

            definition.levelNumber = 1;
            definition.displayName = "The Quiet Lantern";
            definition.chapterName = "Silent Halls";
            definition.objectiveText = "Move the box, turn the lamp and mirror, recover the shard, then escape.";
            definition.hintText = "Push the box beneath the lamp before turning its beam south.";
            definition.width = 7;
            definition.height = 5;
            definition.par = 15;
            definition.requiredShards = 1;
            definition.requiredLampRotations = 1;
            definition.requiredMirrorRotations = 1;
            definition.requiredBoxPushes = 1;
            definition.requiredCurtainToggles = 0;
            definition.verifiedSolution = "N,E,I,N,E,I,N,N,E,E,E,S,I,N,E";
            definition.cells = new CellFlags[35];
            definition.playerStart = new GridCoord(0, 0);
            definition.playerFacing = Direction.East;
            definition.exit = new GridCoord(6, 4);
            definition.lights = new[]
            {
                new LightSourceState { position = new GridCoord(3, 2), direction = Direction.East, range = 4, active = true }
            };
            definition.shards = new[] { new GridCoord(5, 4) };
            definition.mirrors = new[] { new MirrorState { position = new GridCoord(5, 2), kind = MirrorKind.Slash, rotatable = true } };
            definition.boxes = new[] { new GridCoord(2, 1) };
            definition.curtains = Array.Empty<CurtainState>();
            definition.guards = Array.Empty<GuardState>();
            definition.movingLights = Array.Empty<MovingLightState>();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        static LevelDefinition[] BuildLevelDefinitions()
        {
            var levels = new LevelDefinition[15];
            levels[0] = BuildLevelDefinition();
            for (var number = 2; number <= 15; number++) levels[number - 1] = CreateLevel(number);
            return levels;
        }

        static LevelDefinition CreateLevel(int number)
        {
            var path = $"Assets/Data/Levels/Level_{number:00}.asset";
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(level, path);
            }
            level.levelNumber = number;
            level.width = 7;
            level.height = 5;
            level.cells = new CellFlags[35];
            level.playerStart = new GridCoord(0, 0);
            level.playerFacing = Direction.East;
            level.exit = new GridCoord(6, 4);
            level.requiredShards = 0;
            level.requiredLampRotations = 0;
            level.requiredMirrorRotations = 0;
            level.requiredBoxPushes = 0;
            level.requiredCurtainToggles = 0;
            level.lights = Array.Empty<LightSourceState>();
            level.mirrors = Array.Empty<MirrorState>();
            level.boxes = Array.Empty<GridCoord>();
            level.curtains = Array.Empty<CurtainState>();
            level.guards = Array.Empty<GuardState>();
            level.movingLights = Array.Empty<MovingLightState>();
            level.shards = Array.Empty<GridCoord>();
            level.objectiveText = "Reach the exit through shadow.";
            level.hintText = "Read the gold danger cells before every turn.";

            switch (number)
            {
                case 2:
                    Configure(level, "Golden Threshold", "Silent Halls", 10, "N,N,N,N,E,E,E,E,E,E");
                    level.lights = new[] { Lamp(3, 1, Direction.East, 3, true) };
                    level.shards = new[] { new GridCoord(4, 4) }; level.requiredShards = 1;
                    break;
                case 3:
                    Configure(level, "Turn the Night", "Silent Halls", 11, "N,N,N,E,E,I,N,E,E,E,E");
                    level.lights = new[] { Lamp(3, 3, Direction.East, 3) }; level.requiredLampRotations = 1;
                    break;
                case 4:
                    Configure(level, "Lantern Order", "Silent Halls", 16, "N,N,N,N,E,E,S,I,N,E,E,S,I,N,E,E");
                    level.lights = new[]
                    {
                        Lamp(2, 2, Direction.East, 3),
                        Lamp(4, 2, Direction.West, 3, false, (1 << (int)Direction.West) | (1 << (int)Direction.South))
                    };
                    level.requiredLampRotations = 2;
                    break;
                case 5:
                    Configure(level, "The Patient Blocker", "Silent Halls", 12, "N,E,I,N,E,I,N,N,E,E,E,E");
                    level.boxes = new[] { new GridCoord(2, 1) }; level.requiredBoxPushes = 1;
                    level.lights = new[] { Lamp(3, 2, Direction.East, 3) }; level.requiredLampRotations = 1;
                    break;
                case 6:
                    Configure(level, "First Reflection", "Reflections", 13, "E,E,E,E,E,N,N,N,W,I,N,E,E");
                    level.lights = new[] { Lamp(0, 3, Direction.East, 6, true) };
                    level.mirrors = new[] { Mirror(3, 3, MirrorKind.Slash) }; level.requiredMirrorRotations = 1;
                    break;
                case 7:
                    Configure(level, "Mosaic Misdirection", "Reflections", 16, "E,E,E,E,N,N,W,I,S,E,N,I,E,N,N,E");
                    level.lights = new[] { Lamp(0, 2, Direction.East, 7, true) };
                    level.mirrors = new[] { Mirror(2, 2, MirrorKind.Slash), Mirror(4, 3, MirrorKind.Backslash) }; level.requiredMirrorRotations = 2;
                    break;
                case 8:
                    Configure(level, "Silken Eclipse", "Reflections", 14, "E,E,E,N,I,I,S,E,E,E,N,N,N,N");
                    level.lights = new[] { Lamp(0, 2, Direction.East, 7, true) };
                    level.curtains = new[] { new CurtainState { position = new GridCoord(3, 2), open = false } }; level.requiredCurtainToggles = 2;
                    break;
                case 9:
                    Configure(level, "Glass and Velvet", "Reflections", 20, "N,E,I,N,E,I,S,S,E,E,N,I,S,E,N,I,E,N,N,N");
                    level.boxes = new[] { new GridCoord(2, 1) }; level.requiredBoxPushes = 1;
                    level.lights = new[] { Lamp(3, 2, Direction.East, 4) }; level.requiredLampRotations = 1;
                    level.curtains = new[] { new CurtainState { position = new GridCoord(4, 2), open = false } }; level.requiredCurtainToggles = 1;
                    level.mirrors = new[] { Mirror(5, 2, MirrorKind.Slash) }; level.requiredMirrorRotations = 1;
                    level.shards = new[] { new GridCoord(6, 3) }; level.requiredShards = 1;
                    break;
                case 10:
                    Configure(level, "The Lantern Guard", "Reflections", 10, "N,N,N,N,E,E,E,E,E,E");
                    level.guards = new[] { Guard(6, 0, 1, Direction.West, new GridCoord(6, 0), new GridCoord(5, 0)) };
                    break;
                case 11:
                    Configure(level, "Crossing Patrols", "Living Light", 10, "N,N,N,N,E,E,E,E,E,E");
                    level.guards = new[]
                    {
                        Guard(6, 0, 1, Direction.West, new GridCoord(6, 0), new GridCoord(5, 0)),
                        Guard(3, 1, 1, Direction.East, new GridCoord(3, 1), new GridCoord(4, 1))
                    };
                    break;
                case 12:
                    Configure(level, "Moon Window", "Living Light", 10, "N,N,N,N,E,E,E,E,E,E");
                    level.movingLights = new[] { Moonlight(new GridCoord(0, 0), new GridCoord(1, 0)) };
                    break;
                case 13:
                    Configure(level, "Guarded Geometry", "Living Light", 14, "N,E,I,N,N,N,E,E,E,E,S,I,N,E");
                    level.boxes = new[] { new GridCoord(2, 1) }; level.requiredBoxPushes = 1;
                    level.mirrors = new[] { Mirror(5, 2, MirrorKind.Slash) }; level.requiredMirrorRotations = 1;
                    level.guards = new[] { Guard(6, 0, 1, Direction.West, new GridCoord(6, 0), new GridCoord(5, 0)) };
                    break;
                case 14:
                    Configure(level, "Three Moon Shards", "Living Light", 10, "N,N,N,N,E,E,E,E,E,E");
                    level.shards = new[] { new GridCoord(2, 4), new GridCoord(4, 4), new GridCoord(5, 4) }; level.requiredShards = 3;
                    break;
                case 15:
                    Configure(level, "The Last Shadow", "Living Light", 20, "N,E,I,N,E,I,S,S,E,E,N,I,S,E,N,I,E,N,N,N");
                    level.boxes = new[] { new GridCoord(2, 1) }; level.requiredBoxPushes = 1;
                    level.lights = new[] { Lamp(3, 2, Direction.East, 4) }; level.requiredLampRotations = 1;
                    level.curtains = new[] { new CurtainState { position = new GridCoord(4, 2), open = false } }; level.requiredCurtainToggles = 1;
                    level.mirrors = new[] { Mirror(5, 2, MirrorKind.Slash) }; level.requiredMirrorRotations = 1;
                    level.shards = new[] { new GridCoord(6, 2), new GridCoord(6, 3) }; level.requiredShards = 2;
                    level.guards = new[] { Guard(1, 4, 0, Direction.South, new GridCoord(1, 4), new GridCoord(0, 4)) };
                    level.movingLights = new[] { Moonlight(Direction.West, new GridCoord(0, 0), new GridCoord(1, 0)) };
                    break;
            }
            EditorUtility.SetDirty(level);
            return level;
        }

        static void Configure(LevelDefinition level, string name, string chapter, int par, string solution)
        {
            level.displayName = name;
            level.chapterName = chapter;
            level.par = par;
            level.verifiedSolution = solution;
            level.objectiveText = $"{chapter}: solve {name} and reach the exit.";
            level.hintText = "Use Interact on the object in front of Noor; Undo preserves experimentation.";
        }

        static LightSourceState Lamp(int x, int y, Direction direction, int range, bool fixedDirection = false, int allowedMask = 0)
            => new LightSourceState { position = new GridCoord(x, y), direction = direction, range = range, active = true, fixedDirection = fixedDirection, allowedDirectionMask = allowedMask };

        static MirrorState Mirror(int x, int y, MirrorKind kind)
            => new MirrorState { position = new GridCoord(x, y), kind = kind, rotatable = true };

        static GuardState Guard(int x, int y, int lightRange, Direction facing, params GridCoord[] patrol)
            => new GuardState { position = new GridCoord(x, y), lightRange = lightRange, facing = facing, patrol = patrol };

        static MovingLightState Moonlight(params GridCoord[] path)
            => Moonlight(Direction.East, path);

        static MovingLightState Moonlight(Direction direction, params GridCoord[] path)
        {
            var directions = new Direction[path.Length];
            for (var i = 0; i < directions.Length; i++) directions[i] = direction;
            return new MovingLightState { path = path, directions = directions, range = 3, active = true };
        }

        static void EnsureAudioAssets()
        {
            WriteTone("Assets/Audio/Generated/ambience.wav", 110, 0.8f, 0.08f);
            WriteTone("Assets/Audio/Generated/move.wav", 330, 0.08f, 0.16f);
            WriteTone("Assets/Audio/Generated/interact.wav", 520, 0.1f, 0.18f);
            WriteTone("Assets/Audio/Generated/undo.wav", 240, 0.12f, 0.16f);
            WriteTone("Assets/Audio/Generated/failure.wav", 145, 0.22f, 0.2f);
            WriteTone("Assets/Audio/Generated/victory.wav", 660, 0.3f, 0.16f);
        }

        static void EnsureAudioMixer()
        {
            var type = typeof(AssetDatabase).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
            var mixer = AssetDatabase.LoadAssetAtPath(MixerPath, type);
            if (mixer == null)
            {
                var create = type.GetMethod("CreateMixerControllerAtPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                mixer = (UnityEngine.Object)create.Invoke(null, new object[] { MixerPath });
            }
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var groups = (System.Collections.IEnumerable)type.GetMethod("GetAllAudioGroupsSlow", flags).Invoke(mixer, null);
            var names = new HashSet<string>();
            foreach (UnityEngine.Object group in groups) names.Add(group.name);
            var add = type.GetMethod("CreateNewGroup", flags);
            var attach = type.GetMethod("AddChildToParent", flags);
            var master = type.GetProperty("masterGroup", flags).GetValue(mixer);
            if (!names.Contains("Music")) attach.Invoke(mixer, new[] { add.Invoke(mixer, new object[] { "Music", false }), master });
            if (!names.Contains("SFX")) attach.Invoke(mixer, new[] { add.Invoke(mixer, new object[] { "SFX", false }), master });
            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssetIfDirty(mixer);
            AssetDatabase.ImportAsset(MixerPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        static AudioMixerGroup MixerGroup(string name)
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            var groups = mixer.FindMatchingGroups(name);
            if (groups.Length == 0) throw new InvalidOperationException($"Audio mixer group '{name}' is missing.");
            return groups[0];
        }

        static void WriteTone(string path, double frequency, float duration, float volume)
        {
            if (File.Exists(path)) return;
            const int sampleRate = 22050;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + sampleCount * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                writer.Write(sampleRate); writer.Write(sampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(sampleCount * 2);
                for (var i = 0; i < sampleCount; i++)
                {
                    var envelope = Mathf.Min(1, i / (sampleRate * 0.01f)) * Mathf.Min(1, (sampleCount - i) / (sampleRate * 0.02f));
                    writer.Write((short)(Math.Sin(2 * Math.PI * frequency * i / sampleRate) * short.MaxValue * volume * envelope));
                }
            }
        }

        static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Boot";
            CreateCamera();
            CreateEventSystem();
            AddAmbientAudio("BootAmbience");
            var safe = CreateCanvasHierarchy("BootCanvas");
            var flow = new GameObject("SceneFlow").AddComponent<SceneFlowController>();
            SetPrivate(flow, "destination", "MainMenu");

            CreateText(safe, "Kicker", "A MOONLIT PALACE STORY", 30, Cyan, new Vector2(0.5f, 0.66f), new Vector2(900, 60));
            CreateText(safe, "Title", "SHADOW\nTILE ESCAPE", 92, Ivory, new Vector2(0.5f, 0.5f), new Vector2(1100, 240), FontStyles.Bold);
            var begin = CreateButton(safe, "BeginButton", "ENTER THE PALACE", new Vector2(0.5f, 0.27f), new Vector2(520, 104), Violet);
            UnityEventTools.AddPersistentListener(begin.onClick, flow.LoadDestination);
            CreateText(safe, "Attribution", "Made with AnkleBreaker MCP", 22, new Color(1, 1, 1, 0.55f), new Vector2(0.5f, 0.08f), new Vector2(700, 40));
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
        }

        static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";
            CreateCamera();
            CreateEventSystem();
            var safe = CreateCanvasHierarchy("MenuCanvas");
            var menuMusic = AddAmbientAudio("MenuAmbience");
            var menuController = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            var settingsController = new GameObject("SettingsController").AddComponent<SettingsController>();

            var ornament = CreateImage(safe, "PalaceArch", new Vector2(0.5f, 0.54f), new Vector2(1500, 760), Palace);
            ornament.color = new Color(Palace.r, Palace.g, Palace.b, 0.72f);
            CreateText(safe, "Chapter", "A MOONLIT PALACE STORY", 27, Gold, new Vector2(0.32f, 0.78f), new Vector2(720, 48), FontStyles.Bold);
            CreateText(safe, "Title", "SHADOW TILE\nESCAPE", 78, Ivory, new Vector2(0.32f, 0.59f), new Vector2(760, 230), FontStyles.Bold);
            CreateText(safe, "Tagline", "Light is danger. Shadow is the path.", 30, Cyan, new Vector2(0.32f, 0.42f), new Vector2(720, 60));

            var continueButton = CreateButton(safe, "ContinueButton", "CONTINUE", new Vector2(0.69f, 0.71f), new Vector2(520, 96), Violet, 27);
            var newGame = CreateButton(safe, "NewGameButton", "NEW GAME", new Vector2(0.69f, 0.59f), new Vector2(520, 96), Palace, 27);
            var levelSelect = CreateButton(safe, "LevelSelectButton", "LEVEL SELECT", new Vector2(0.69f, 0.47f), new Vector2(520, 96), Palace, 27);
            var howTo = CreateButton(safe, "HowToPlayButton", "HOW TO PLAY", new Vector2(0.69f, 0.35f), new Vector2(520, 96), Palace, 27);
            var settings = CreateButton(safe, "SettingsButton", "SETTINGS", new Vector2(0.69f, 0.23f), new Vector2(250, 96), Palace, 24);
            var credits = CreateButton(safe, "CreditsButton", "CREDITS", new Vector2(0.83f, 0.23f), new Vector2(250, 96), Palace, 24);
            UnityEventTools.AddPersistentListener(continueButton.onClick, menuController.Continue);
            UnityEventTools.AddPersistentListener(newGame.onClick, menuController.NewGame);
            UnityEventTools.AddPersistentListener(levelSelect.onClick, menuController.LevelSelect);
            UnityEventTools.AddPersistentListener(howTo.onClick, menuController.HowToPlay);
            UnityEventTools.AddPersistentListener(settings.onClick, settingsController.Show);
            UnityEventTools.AddPersistentListener(credits.onClick, menuController.Credits);

            var newGameConfirm = CreateModal(safe, "NewGameConfirmation", "BEGIN A NEW STORY?", "Existing progress will be reset. This cannot be undone.", Orange);
            var confirmNew = CreateButton(newGameConfirm.transform, "ConfirmNewGame", "RESET & BEGIN", new Vector2(0.42f, 0.30f), new Vector2(300, 96), Orange, 24);
            var cancelNew = CreateButton(newGameConfirm.transform, "CancelNewGame", "CANCEL", new Vector2(0.60f, 0.30f), new Vector2(260, 96), Palace, 24);
            UnityEventTools.AddPersistentListener(confirmNew.onClick, menuController.ConfirmNewGame);
            UnityEventTools.AddPersistentListener(cancelNew.onClick, menuController.CancelNewGame);
            newGameConfirm.SetActive(false);

            var settingsPanel = CreateModal(safe, "SettingsPanel", "SETTINGS", "Audio, comfort, and progress", Cyan);
            var musicSlider = CreateSlider(settingsPanel.transform, "MusicSlider", new Vector2(0.5f, 0.58f));
            CreateText(settingsPanel.transform, "MusicLabel", "MUSIC", 24, Ivory, new Vector2(0.31f, 0.58f), new Vector2(230, 45), FontStyles.Bold);
            var sfxSlider = CreateSlider(settingsPanel.transform, "SfxSlider", new Vector2(0.5f, 0.49f));
            CreateText(settingsPanel.transform, "SfxLabel", "SFX", 24, Ivory, new Vector2(0.31f, 0.49f), new Vector2(230, 45), FontStyles.Bold);
            var haptics = CreateToggle(settingsPanel.transform, "HapticsToggle", "HAPTICS", new Vector2(0.41f, 0.39f));
            var reduced = CreateToggle(settingsPanel.transform, "ReducedFlashingToggle", "REDUCED FLASHING", new Vector2(0.61f, 0.39f));
            var reset = CreateButton(settingsPanel.transform, "ResetProgress", "RESET PROGRESS", new Vector2(0.40f, 0.25f), new Vector2(300, 88), Orange, 22);
            var close = CreateButton(settingsPanel.transform, "CloseSettings", "DONE", new Vector2(0.62f, 0.25f), new Vector2(260, 88), Violet, 24);
            var settingsStatus = CreateText(settingsPanel.transform, "SettingsStatus", "Settings save automatically.", 21, Cyan, new Vector2(0.5f, 0.17f), new Vector2(700, 40));
            var resetConfirm = CreateModal(settingsPanel.transform, "ResetConfirmation", "RESET ALL PROGRESS?", "This action cannot be undone.", Orange);
            var confirmReset = CreateButton(resetConfirm.transform, "ConfirmReset", "RESET", new Vector2(0.43f, 0.30f), new Vector2(240, 90), Orange, 23);
            var cancelReset = CreateButton(resetConfirm.transform, "CancelReset", "CANCEL", new Vector2(0.58f, 0.30f), new Vector2(240, 90), Palace, 23);
            resetConfirm.SetActive(false);
            UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, settingsController.SetMusic);
            UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, settingsController.SetSfx);
            UnityEventTools.AddPersistentListener(haptics.onValueChanged, settingsController.SetHaptics);
            UnityEventTools.AddPersistentListener(reduced.onValueChanged, settingsController.SetReducedFlashing);
            UnityEventTools.AddPersistentListener(reset.onClick, settingsController.AskReset);
            UnityEventTools.AddPersistentListener(close.onClick, settingsController.Hide);
            UnityEventTools.AddPersistentListener(confirmReset.onClick, settingsController.ConfirmReset);
            UnityEventTools.AddPersistentListener(cancelReset.onClick, settingsController.CancelReset);
            settingsPanel.SetActive(false);

            SetPrivate(menuController, "continueButton", continueButton);
            SetPrivate(menuController, "continueLabel", continueButton.GetComponentInChildren<TMP_Text>());
            SetPrivate(menuController, "newGameConfirmation", newGameConfirm);
            SetPrivate(settingsController, "panel", settingsPanel);
            SetPrivate(settingsController, "resetConfirmation", resetConfirm);
            SetPrivate(settingsController, "musicSlider", musicSlider);
            SetPrivate(settingsController, "sfxSlider", sfxSlider);
            SetPrivate(settingsController, "hapticsToggle", haptics);
            SetPrivate(settingsController, "reducedFlashingToggle", reduced);
            SetPrivate(settingsController, "status", settingsStatus);
            SetPrivate(settingsController, "musicSource", menuMusic);

            CreateText(safe, "Version", "v0.9.0", 20, new Color(1, 1, 1, 0.45f), new Vector2(0.10f, 0.07f), new Vector2(240, 38));
            CreateText(safe, "Attribution", "Made with AnkleBreaker MCP  ·  Nunito Sans / SIL Open Font License", 20, new Color(1, 1, 1, 0.48f), new Vector2(0.5f, 0.07f), new Vector2(1050, 38));
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        static void BuildIntroScene()
        {
            BuildStoryScene("Intro", "NOOR AND THE CURSED PALACE", "When the palace woke, every lantern became an eye. Noor must cross fifteen halls by shaping shadow itself.", "BEGIN THE ESCAPE", "Level_01");
        }

        static void BuildHowToPlayScene()
        {
            BuildStoryScene("HowToPlay", "HOW TO PLAY", "MOVE  ·  WASD / ARROWS / D-PAD\nINTERACT  ·  E / SPACE / INTERACT\nUNDO  ·  Z / BACKSPACE\nRESTART  ·  R\nPAUSE  ·  ESCAPE\n\nGold cells are dangerous. Cyan marks Noor, shards, and safe guidance. Guard previews show their next destination.", "BACK TO MENU", "MainMenu");
        }

        static void BuildCreditsScene()
        {
            BuildStoryScene("Credits", "CREDITS & LICENSES", "SHADOW TILE ESCAPE\nDesign, code, UI, generated tones, and geometric art: Moonlit Loom Games\n\nNunito Sans — SIL Open Font License 1.1\nMade with AnkleBreaker MCP — AnkleBreaker Open License v1.0\nUnity and package notices: see ThirdPartyNotices.md", "BACK TO MENU", "MainMenu");
        }

        static void BuildCompletionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Completion";
            CreateCamera(); CreateEventSystem(); AddAmbientAudio("CompletionAmbience");
            var safe = CreateCanvasHierarchy("CompletionCanvas");
            var controller = new GameObject("CompletionController").AddComponent<StaticScreenController>();
            CreateText(safe, "Kicker", "DAWN BEYOND THE PALACE", 28, Gold, new Vector2(0.5f, 0.78f), new Vector2(900, 50), FontStyles.Bold);
            CreateText(safe, "Title", "NOOR IS FREE", 84, Ivory, new Vector2(0.5f, 0.61f), new Vector2(1100, 140), FontStyles.Bold);
            var body = CreateText(safe, "Body", "The living light falls quiet. Every completed hall remains open for replay, but beyond the final gate, morning waits.", 32, Cyan, new Vector2(0.5f, 0.45f), new Vector2(1120, 150));
            body.textWrappingMode = TextWrappingModes.Normal;
            var replay = CreateButton(safe, "ReplayFinale", "REPLAY FINALE", new Vector2(0.38f, 0.25f), new Vector2(360, 100), Palace, 25);
            var select = CreateButton(safe, "CompletionLevelSelect", "LEVEL SELECT", new Vector2(0.62f, 0.25f), new Vector2(360, 100), Violet, 25);
            UnityEventTools.AddPersistentListener(replay.onClick, controller.ReplayFinale);
            UnityEventTools.AddPersistentListener(select.onClick, controller.LevelSelect);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Completion.unity");
        }

        static void BuildStoryScene(string sceneName, string title, string bodyText, string buttonText, string destination)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = sceneName;
            CreateCamera(); CreateEventSystem(); AddAmbientAudio(sceneName + "Ambience");
            var safe = CreateCanvasHierarchy(sceneName + "Canvas");
            var controller = new GameObject(sceneName + "Controller").AddComponent<StaticScreenController>();
            controller.Destination = destination;
            CreateText(safe, "Kicker", "SHADOW TILE ESCAPE", 27, Gold, new Vector2(0.5f, 0.79f), new Vector2(800, 45), FontStyles.Bold);
            CreateText(safe, "Title", title, 66, Ivory, new Vector2(0.5f, 0.64f), new Vector2(1300, 110), FontStyles.Bold);
            var body = CreateText(safe, "Body", bodyText, 30, Cyan, new Vector2(0.5f, 0.46f), new Vector2(1280, 300));
            body.textWrappingMode = TextWrappingModes.Normal;
            var next = CreateButton(safe, "ContinueButton", buttonText, new Vector2(0.5f, 0.20f), new Vector2(480, 100), Violet, 25);
            UnityEventTools.AddPersistentListener(next.onClick, controller.Continue);
            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{sceneName}.unity");
        }

        static void BuildLevelSelectScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LevelSelect";
            CreateCamera(); CreateEventSystem(); AddAmbientAudio("LevelSelectAmbience");
            var safe = CreateCanvasHierarchy("LevelSelectCanvas");
            var controller = new GameObject("LevelSelectController").AddComponent<LevelSelectController>();
            CreateText(safe, "Title", "CHOOSE A SHADOW PATH", 56, Ivory, new Vector2(0.5f, 0.88f), new Vector2(1100, 90), FontStyles.Bold);
            CreateText(safe, "Chapters", "SILENT HALLS  ·  REFLECTIONS  ·  LIVING LIGHT", 25, Gold, new Vector2(0.5f, 0.80f), new Vector2(1000, 45), FontStyles.Bold);
            var levelButtons = new LevelButtonController[15];
            for (var i = 0; i < 15; i++)
            {
                var row = i / 5;
                var column = i % 5;
                var anchor = new Vector2(0.24f + column * 0.13f, 0.66f - row * 0.20f);
                var button = CreateButton(safe, $"LevelButton_{i + 1:00}", $"{i + 1:00}\nOPEN", anchor, new Vector2(210, 150), i < 5 ? Palace : i < 10 ? Violet : new Color32(64, 50, 100, 255), 25);
                var item = button.gameObject.AddComponent<LevelButtonController>();
                item.Configure(i + 1, button, button.GetComponentInChildren<TMP_Text>());
                UnityEventTools.AddPersistentListener(button.onClick, item.Open);
                levelButtons[i] = item;
            }
            controller.LevelButtons = levelButtons;
            EditorUtility.SetDirty(controller);
            var back = CreateButton(safe, "BackButton", "BACK", new Vector2(0.5f, 0.08f), new Vector2(280, 92), Palace, 24);
            UnityEventTools.AddPersistentListener(back.onClick, controller.Back);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/LevelSelect.unity");
        }

        static void BuildLevelScene(LevelDefinition definition)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = $"Level_{definition.levelNumber:00}";
            CreateCamera();
            CreateEventSystem();
            var safe = CreateCanvasHierarchy("GameplayCanvas");

            var controllerObject = new GameObject("GameplayController");
            var controller = controllerObject.AddComponent<GameplayController>();
            var ambienceSource = controllerObject.AddComponent<AudioSource>();
            ambienceSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/ambience.wav");
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = true;
            ambienceSource.volume = 0.18f;
            ambienceSource.outputAudioMixerGroup = MixerGroup("Music");
            var sfxSource = controllerObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.outputAudioMixerGroup = MixerGroup("SFX");

            var topBand = CreateImage(safe, "TopBand", new Vector2(0.5f, 0.93f), new Vector2(1720, 124), Indigo);
            var levelLabel = CreateText(topBand.transform, "LevelLabel", $"LEVEL {definition.levelNumber:00}", 32, Ivory, new Vector2(0.30f, 0.5f), new Vector2(700, 55), FontStyles.Bold, TextAlignmentOptions.Left);
            var moveLabel = CreateText(topBand.transform, "MoveLabel", $"MOVES 0 / PAR {definition.par}", 22, Cyan, new Vector2(0.59f, 0.5f), new Vector2(620, 50));

            var boardFrame = CreateImage(safe, "BoardFrame", new Vector2(0.5f, 0.52f), new Vector2(736, 544), new Color32(53, 50, 108, 255));
            var boardRoot = CreateRect(boardFrame.transform, "BoardRoot", new Vector2(0.5f, 0.5f), new Vector2(672, 480));
            var cells = new Image[35];
            var lights = new Image[35];
            for (var y = 0; y < 5; y++)
            for (var x = 0; x < 7; x++)
            {
                var index = y * 7 + x;
                var pos = new Vector2((x - 3) * 96, (y - 2) * 96);
                cells[index] = CreateImage(boardRoot, $"Cell_{x}_{y}", new Vector2(0.5f, 0.5f), new Vector2(90, 90), Indigo);
                cells[index].rectTransform.anchoredPosition = pos;
                lights[index] = CreateImage(boardRoot, $"Light_{x}_{y}", new Vector2(0.5f, 0.5f), new Vector2(80, 80), Gold);
                lights[index].rectTransform.anchoredPosition = pos;
                lights[index].gameObject.SetActive(false);
            }

            var exit = CreateImage(boardRoot, "Exit", new Vector2(0.5f, 0.5f), new Vector2(68, 68), Violet).rectTransform;
            CreateText(exit, "ExitMark", "EXIT", 18, Ivory, new Vector2(0.5f, 0.5f), new Vector2(72, 32), FontStyles.Bold);
            var shardViews = new RectTransform[3];
            for (var i = 0; i < shardViews.Length; i++)
            {
                shardViews[i] = CreateImage(boardRoot, $"MoonShard_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(34, 34), Cyan).rectTransform;
                shardViews[i].localEulerAngles = new Vector3(0, 0, 45);
            }
            var lampViews = new RectTransform[4];
            for (var i = 0; i < lampViews.Length; i++)
            {
                lampViews[i] = CreateImage(boardRoot, $"Lamp_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(62, 44), Gold).rectTransform;
                var tip = CreateImage(lampViews[i], "FacingTip", new Vector2(1, 0.5f), new Vector2(18, 28), Ivory);
                tip.rectTransform.anchoredPosition = new Vector2(8, 0);
            }
            var mirrorViews = new RectTransform[4];
            for (var i = 0; i < mirrorViews.Length; i++)
                mirrorViews[i] = CreateImage(boardRoot, $"Mirror_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(16, 72), Cyan).rectTransform;
            var boxViews = new RectTransform[4];
            for (var i = 0; i < boxViews.Length; i++)
                boxViews[i] = CreateImage(boardRoot, $"Box_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(64, 64), new Color32(101, 76, 128, 255)).rectTransform;
            var curtainViews = new RectTransform[3];
            for (var i = 0; i < curtainViews.Length; i++)
                curtainViews[i] = CreateImage(boardRoot, $"Curtain_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(72, 18), Violet).rectTransform;
            var guardPreviewViews = new RectTransform[3];
            for (var i = 0; i < guardPreviewViews.Length; i++)
                guardPreviewViews[i] = CreateImage(boardRoot, $"GuardPreview_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(42, 42), new Color(0.91f, 0.42f, 0.28f, 0.35f)).rectTransform;
            var guardViews = new RectTransform[3];
            for (var i = 0; i < guardViews.Length; i++)
            {
                guardViews[i] = CreateImage(boardRoot, $"Guard_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(64, 50), Orange).rectTransform;
                var tip = CreateImage(guardViews[i], "FacingTip", new Vector2(1, 0.5f), new Vector2(18, 24), Gold);
                tip.rectTransform.anchoredPosition = new Vector2(8, 0);
            }
            var movingLightViews = new RectTransform[2];
            for (var i = 0; i < movingLightViews.Length; i++)
            {
                movingLightViews[i] = CreateImage(boardRoot, $"Moonlight_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(52, 52), new Color32(127, 158, 230, 255)).rectTransform;
                movingLightViews[i].localEulerAngles = new Vector3(0, 0, 45);
            }

            var player = CreateImage(boardRoot, "Noor", new Vector2(0.5f, 0.5f), new Vector2(58, 42), Cyan).rectTransform;
            var cloak = CreateImage(player, "Cloak", new Vector2(0.35f, 0.5f), new Vector2(34, 34), Navy);
            cloak.rectTransform.anchoredPosition = Vector2.zero;
            var facing = CreateImage(player, "Facing", new Vector2(1, 0.5f), new Vector2(16, 18), Cyan);
            facing.rectTransform.anchoredPosition = new Vector2(5, 0);
            var pulse = CreateImage(boardRoot, "TurnPulse", new Vector2(0.5f, 0.5f), new Vector2(82, 62), new Color(0.39f, 0.85f, 0.9f, 0.24f)).rectTransform;
            pulse.gameObject.SetActive(false);

            var status = CreateText(safe, "Status", "Stay in shadow. Gold tiles are danger.", 27, Ivory, new Vector2(0.5f, 0.19f), new Vector2(1000, 52));
            var lampLabel = CreateText(safe, "LampDirection", "LAMP  EAST", 23, Gold, new Vector2(0.5f, 0.84f), new Vector2(480, 44), FontStyles.Bold);

            var north = CreateButton(safe, "MoveNorth", "N", new Vector2(0.11f, 0.29f), new Vector2(104, 104), Palace, 48);
            var west = CreateButton(safe, "MoveWest", "W", new Vector2(0.055f, 0.18f), new Vector2(104, 104), Palace, 48);
            var south = CreateButton(safe, "MoveSouth", "S", new Vector2(0.11f, 0.07f), new Vector2(104, 104), Palace, 48);
            var east = CreateButton(safe, "MoveEast", "E", new Vector2(0.165f, 0.18f), new Vector2(104, 104), Palace, 48);
            var interact = CreateButton(safe, "Interact", "INTERACT", new Vector2(0.89f, 0.17f), new Vector2(190, 116), Violet, 25);
            var undo = CreateButton(topBand.transform, "Undo", "UNDO", new Vector2(0.75f, 0.5f), new Vector2(140, 96), Palace, 23);
            var restart = CreateButton(topBand.transform, "Restart", "RESTART", new Vector2(0.85f, 0.5f), new Vector2(150, 96), Palace, 22);
            var pause = CreateButton(topBand.transform, "Pause", "PAUSE", new Vector2(0.95f, 0.5f), new Vector2(140, 96), Palace, 22);
            var menu = CreateButton(topBand.transform, "Menu", "MENU", new Vector2(0.05f, 0.5f), new Vector2(130, 96), Palace, 22);

            UnityEventTools.AddPersistentListener(north.onClick, controller.MoveNorth);
            UnityEventTools.AddPersistentListener(east.onClick, controller.MoveEast);
            UnityEventTools.AddPersistentListener(south.onClick, controller.MoveSouth);
            UnityEventTools.AddPersistentListener(west.onClick, controller.MoveWest);
            UnityEventTools.AddPersistentListener(interact.onClick, controller.Interact);
            UnityEventTools.AddPersistentListener(undo.onClick, controller.Undo);
            UnityEventTools.AddPersistentListener(restart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(pause.onClick, controller.TogglePause);
            UnityEventTools.AddPersistentListener(menu.onClick, controller.BackToMenu);

            var failure = CreateModal(safe, "FailurePanel", "CAUGHT IN THE LIGHT", "Undo the last turn or restart the puzzle.", Orange);
            var failureUndo = CreateButton(failure.transform, "UndoFailure", "UNDO", new Vector2(0.30f, 0.31f), new Vector2(220, 96), Violet, 24);
            var failureRestart = CreateButton(failure.transform, "RestartFailure", "RESTART", new Vector2(0.43f, 0.31f), new Vector2(220, 96), Palace, 23);
            var failureSelect = CreateButton(failure.transform, "FailureLevelSelect", "LEVEL SELECT", new Vector2(0.58f, 0.31f), new Vector2(260, 96), Palace, 21);
            var failureMenu = CreateButton(failure.transform, "FailureMenu", "MAIN MENU", new Vector2(0.73f, 0.31f), new Vector2(230, 96), Palace, 22);
            UnityEventTools.AddPersistentListener(failureUndo.onClick, controller.Undo);
            UnityEventTools.AddPersistentListener(failureRestart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(failureSelect.onClick, controller.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(failureMenu.onClick, controller.BackToMenu);
            failure.SetActive(false);

            var victory = CreateModal(safe, "VictoryPanel", "THE SHADOW PATH OPENS", $"Level {definition.levelNumber:00} complete", Cyan);
            var victoryReplay = CreateButton(victory.transform, "Replay", "REPLAY", new Vector2(0.28f, 0.31f), new Vector2(220, 96), Palace, 23);
            var victoryNext = CreateButton(victory.transform, "NextLevel", definition.levelNumber == 15 ? "FINALE" : "NEXT LEVEL", new Vector2(0.43f, 0.31f), new Vector2(260, 96), Violet, 22);
            var victorySelect = CreateButton(victory.transform, "VictoryLevelSelect", "LEVEL SELECT", new Vector2(0.59f, 0.31f), new Vector2(260, 96), Palace, 21);
            var victoryMenu = CreateButton(victory.transform, "VictoryMenu", "MAIN MENU", new Vector2(0.74f, 0.31f), new Vector2(230, 96), Palace, 22);
            UnityEventTools.AddPersistentListener(victoryReplay.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(victoryNext.onClick, controller.NextLevel);
            UnityEventTools.AddPersistentListener(victorySelect.onClick, controller.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(victoryMenu.onClick, controller.BackToMenu);
            victory.SetActive(false);

            var pausePanel = CreateModal(safe, "PausePanel", "PAUSED", "The palace waits in silence.", Cyan);
            var resume = CreateButton(pausePanel.transform, "Resume", "RESUME", new Vector2(0.39f, 0.31f), new Vector2(260, 96), Violet, 25);
            var pauseRestart = CreateButton(pausePanel.transform, "PauseRestart", "RESTART", new Vector2(0.52f, 0.31f), new Vector2(220, 96), Palace, 23);
            var pauseSelect = CreateButton(pausePanel.transform, "PauseLevelSelect", "LEVEL SELECT", new Vector2(0.65f, 0.31f), new Vector2(240, 96), Palace, 21);
            var pauseMenu = CreateButton(pausePanel.transform, "PauseMenu", "MAIN MENU", new Vector2(0.78f, 0.31f), new Vector2(220, 96), Palace, 22);
            UnityEventTools.AddPersistentListener(resume.onClick, controller.Resume);
            UnityEventTools.AddPersistentListener(pauseRestart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(pauseSelect.onClick, controller.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(pauseMenu.onClick, controller.BackToMenu);
            pausePanel.SetActive(false);

            controller.Definition = definition;
            EditorUtility.SetDirty(controller);
            SetPrivate(controller, "boardRoot", boardRoot);
            SetPrivate(controller, "cellViews", cells);
            SetPrivate(controller, "lightViews", lights);
            SetPrivate(controller, "playerView", player);
            SetPrivate(controller, "exitView", exit);
            SetPrivate(controller, "lampViews", lampViews);
            SetPrivate(controller, "mirrorViews", mirrorViews);
            SetPrivate(controller, "boxViews", boxViews);
            SetPrivate(controller, "curtainViews", curtainViews);
            SetPrivate(controller, "guardViews", guardViews);
            SetPrivate(controller, "guardPreviewViews", guardPreviewViews);
            SetPrivate(controller, "movingLightViews", movingLightViews);
            SetPrivate(controller, "shardViews", shardViews);
            SetPrivate(controller, "levelLabel", levelLabel);
            SetPrivate(controller, "moveLabel", moveLabel);
            SetPrivate(controller, "statusLabel", status);
            SetPrivate(controller, "lampDirectionLabel", lampLabel);
            SetPrivate(controller, "failurePanel", failure);
            SetPrivate(controller, "victoryPanel", victory);
            SetPrivate(controller, "pausePanel", pausePanel);
            SetPrivate(controller, "undoButton", undo);
            SetPrivate(controller, "turnPulseView", pulse);
            SetPrivate(controller, "sfxSource", sfxSource);
            SetPrivate(controller, "ambienceSource", ambienceSource);
            SetPrivate(controller, "moveClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/move.wav"));
            SetPrivate(controller, "interactClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/interact.wav"));
            SetPrivate(controller, "undoClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/undo.wav"));
            SetPrivate(controller, "failureClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/failure.wav"));
            SetPrivate(controller, "victoryClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/victory.wav"));

            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Levels/Level_{definition.levelNumber:00}.unity");
        }

        static GameObject CreateModal(Transform parent, string name, string title, string body, Color accent)
        {
            var panelImage = CreateImage(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, new Color32(13, 17, 40, 245));
            Stretch(panelImage.rectTransform);
            panelImage.raycastTarget = true;
            var panel = panelImage.gameObject;
            CreateText(panel.transform, "Accent", "*", 64, accent, new Vector2(0.5f, 0.76f), new Vector2(100, 80));
            CreateText(panel.transform, "Title", title, 54, Ivory, new Vector2(0.5f, 0.61f), new Vector2(920, 80), FontStyles.Bold);
            CreateText(panel.transform, "Body", body, 30, Ivory, new Vector2(0.5f, 0.46f), new Vector2(900, 70));
            return panel;
        }

        static Transform CreateCanvasHierarchy(string name)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var background = CreateImage(canvasObject.transform, "FullBleedBackground", new Vector2(0.5f, 0.5f), Vector2.zero, Navy);
            Stretch(background.rectTransform);
            var glow = CreateImage(canvasObject.transform, "MoonGlow", new Vector2(0.5f, 0.86f), new Vector2(900, 180), new Color(0.2f, 0.3f, 0.7f, 0.12f));
            glow.raycastTarget = false;

            var safe = CreateRect(canvasObject.transform, "SafeAreaRoot", new Vector2(0.5f, 0.5f), Vector2.zero);
            Stretch(safe);
            safe.gameObject.AddComponent<SafeAreaFitter>();
            return safe;
        }

        static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Navy;
            cameraObject.transform.position = new Vector3(0, 0, -10);
        }

        static AudioSource AddAmbientAudio(string name)
        {
            var source = new GameObject(name).AddComponent<AudioSource>();
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/ambience.wav");
            source.loop = true;
            source.playOnAwake = true;
            source.volume = 0.14f;
            source.outputAudioMixerGroup = MixerGroup("Music");
            return source;
        }

        static void CreateEventSystem()
        {
            var system = new GameObject("EventSystem");
            system.SetActive(false);
            system.AddComponent<EventSystem>();
            var module = system.AddComponent<InputSystemUIInputModule>();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions == null) throw new InvalidOperationException("InputSystem_Actions.inputactions is missing.");
            module.actionsAsset = actions;
            module.point = InputActionReference.Create(actions.FindAction("UI/Point", true));
            module.move = InputActionReference.Create(actions.FindAction("UI/Navigate", true));
            module.submit = InputActionReference.Create(actions.FindAction("UI/Submit", true));
            module.cancel = InputActionReference.Create(actions.FindAction("UI/Cancel", true));
            module.leftClick = InputActionReference.Create(actions.FindAction("UI/Click", true));
            module.rightClick = InputActionReference.Create(actions.FindAction("UI/RightClick", true));
            module.middleClick = InputActionReference.Create(actions.FindAction("UI/MiddleClick", true));
            module.scrollWheel = InputActionReference.Create(actions.FindAction("UI/ScrollWheel", true));
            module.trackedDevicePosition = InputActionReference.Create(actions.FindAction("UI/TrackedDevicePosition", true));
            module.trackedDeviceOrientation = InputActionReference.Create(actions.FindAction("UI/TrackedDeviceOrientation", true));
            system.SetActive(true);
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            return rect;
        }

        static Image CreateImage(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            var rect = CreateRect(parent, name, anchor, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TMP_Text CreateText(Transform parent, string name, string value, float size, Color color, Vector2 anchor, Vector2 dimensions, FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var rect = CreateRect(parent, name, anchor, dimensions);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Color color, float fontSize = 30)
        {
            var image = CreateImage(parent, name, anchor, size, color);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1);
            colors.pressedColor = new Color(0.68f, 0.68f, 0.78f, 1);
            colors.disabledColor = new Color(0.38f, 0.38f, 0.46f, 0.7f);
            button.colors = colors;
            CreateText(image.transform, "Label", label, fontSize, Ivory, new Vector2(0.5f, 0.5f), size - new Vector2(16, 12), FontStyles.Bold);
            return button;
        }

        static Slider CreateSlider(Transform parent, string name, Vector2 anchor)
        {
            var root = CreateImage(parent, name, anchor, new Vector2(480, 48), new Color32(42, 46, 92, 255));
            root.raycastTarget = true;
            var fillArea = CreateRect(root.transform, "FillArea", new Vector2(0.5f, 0.5f), new Vector2(440, 24));
            var fill = CreateImage(fillArea, "Fill", new Vector2(0, 0.5f), new Vector2(440, 24), Cyan).rectTransform;
            fill.anchorMin = new Vector2(0, 0.5f); fill.anchorMax = new Vector2(0, 0.5f); fill.pivot = new Vector2(0, 0.5f);
            var handleArea = CreateRect(root.transform, "HandleArea", new Vector2(0.5f, 0.5f), new Vector2(440, 48));
            var handle = CreateImage(handleArea, "Handle", new Vector2(0, 0.5f), new Vector2(36, 54), Ivory);
            handle.raycastTarget = true;
            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0; slider.maxValue = 1; slider.value = 0.8f;
            return slider;
        }

        static Toggle CreateToggle(Transform parent, string name, string label, Vector2 anchor)
        {
            var root = CreateRect(parent, name, anchor, new Vector2(330, 70));
            var box = CreateImage(root, "Box", new Vector2(0.12f, 0.5f), new Vector2(58, 58), Palace);
            box.raycastTarget = true;
            var check = CreateImage(box.transform, "Check", new Vector2(0.5f, 0.5f), new Vector2(34, 34), Cyan);
            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = true;
            CreateText(root, "Label", label, 22, Ivory, new Vector2(0.62f, 0.5f), new Vector2(245, 60), FontStyles.Bold, TextAlignmentOptions.Left);
            return toggle;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetPrivate(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetPrivate(UnityEngine.Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetPrivate<T>(UnityEngine.Object target, string propertyName, T[] values) where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(target);
            var array = serialized.FindProperty(propertyName);
            array.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ConfigureProject()
        {
            PlayerSettings.companyName = "Moonlit Loom Games";
            PlayerSettings.productName = "Shadow Tile Escape";
            PlayerSettings.bundleVersion = "0.9.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.moonlitloom.shadowtileescape");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.moonlitloom.shadowtileescape");
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.enableCrashReportAPI = false;
            Application.targetFrameRate = 60;

            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Intro.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/HowToPlay.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/LevelSelect.unity", true)
            };
            for (var level = 1; level <= 15; level++)
                scenes.Add(new EditorBuildSettingsScene($"Assets/Scenes/Levels/Level_{level:00}.unity", true));
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Credits.unity", true));
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Completion.unity", true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void ConfigureBranding()
        {
            var importer = AssetImporter.GetAtPath(AppIconPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"App icon is missing at {AppIconPath}");
            if (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled || importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            ClearLegacyIcons(NamedBuildTarget.Android);
            SetAllLegacyIcons(NamedBuildTarget.iOS, icon);
            SetAllLegacyIcons(NamedBuildTarget.Standalone, icon);
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            {
                var platformIcons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                foreach (var platformIcon in platformIcons)
                {
                    var layers = new Texture2D[platformIcon.maxLayerCount];
                    if (kind.ToString().StartsWith("Adaptive", StringComparison.Ordinal))
                        for (var layer = 0; layer < layers.Length; layer++) layers[layer] = icon;
                    platformIcon.SetTextures(layers);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, platformIcons);
            }
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;
            PlayerSettings.SplashScreen.backgroundColor = Navy;
            PlayerSettings.SplashScreen.overlayOpacity = 1f;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AppIconPath);
            PlayerSettings.SplashScreen.logos = new[]
            {
                PlayerSettings.SplashScreenLogo.CreateWithUnityLogo(2f),
                PlayerSettings.SplashScreenLogo.Create(2f, sprite)
            };
        }

        static void SetAllLegacyIcons(NamedBuildTarget target, Texture2D icon)
        {
            var sizes = PlayerSettings.GetIconSizes(target, IconKind.Any);
            var icons = new Texture2D[sizes.Length];
            for (var i = 0; i < icons.Length; i++) icons[i] = icon;
            PlayerSettings.SetIcons(target, icons, IconKind.Any);
        }

        static void ClearLegacyIcons(NamedBuildTarget target)
        {
            var sizes = PlayerSettings.GetIconSizes(target, IconKind.Any);
            PlayerSettings.SetIcons(target, new Texture2D[sizes.Length], IconKind.Any);
        }
    }
}
