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
        const string PanelSpritePath = "Assets/Art/UI/ui-panel.png";
        const string ButtonSpritePath = "Assets/Art/UI/ui-button.png";
        const string CircleSpritePath = "Assets/Art/UI/ui-circle.png";

        static readonly Color Navy = new Color32(9, 13, 29, 255);
        static readonly Color Indigo = new Color32(21, 26, 58, 255);
        static readonly Color Palace = new Color32(37, 40, 90, 255);
        static readonly Color Cyan = new Color32(99, 217, 230, 255);
        static readonly Color Gold = new Color32(242, 184, 75, 255);
        static readonly Color Violet = new Color32(154, 120, 212, 255);
        static readonly Color Ivory = new Color32(244, 238, 221, 255);
        static readonly Color Orange = new Color32(233, 106, 71, 255);
        static readonly Color Surface = new Color32(18, 23, 52, 246);
        static readonly Color SurfaceRaised = new Color32(29, 35, 76, 250);
        static readonly Color Muted = new Color32(142, 154, 184, 255);
        static readonly Color Disabled = new Color32(73, 79, 104, 210);

        static TMP_FontAsset font;
        static Sprite panelSprite;
        static Sprite buttonSprite;
        static Sprite circleSprite;

        enum UiIcon { Continue, NewGame, Levels, Help, Settings, Back, Pause, Restart, Undo, Hint, Interact, Credits, Close }

        [MenuItem("Shadow Tile Escape/Build/Build Full Game")]
        [MenuItem("Shadow Tile Escape/Build/Build Vertical Slice")]
        public static void Build()
        {
            EnsureFolders();
            EnsureUiSprites();
            EnsureAudioAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureAudioMixer();
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PanelSpritePath);
            buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            if (font == null) throw new InvalidOperationException($"Nunito Sans TMP asset missing at {FontPath}");
            if (panelSprite == null || buttonSprite == null || circleSprite == null) throw new InvalidOperationException("Generated UI sprites failed to import.");

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
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "UI");
            EnsureFolder("Assets/Audio", "Generated");
            EnsureFolder("Assets/Data", "Levels");
            EnsureFolder("Assets/Scenes", "Levels");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        static void EnsureUiSprites()
        {
            WriteRoundedSprite(PanelSpritePath, 18);
            WriteRoundedSprite(ButtonSpritePath, 14);
            WriteRoundedSprite(CircleSpritePath, 32);
        }

        static void WriteRoundedSprite(string path, int radius)
        {
            if (!File.Exists(path))
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var px = Mathf.Clamp(x, radius, size - 1 - radius);
                    var py = Mathf.Clamp(y, radius, size - 1 - radius);
                    var inside = (new Vector2(x - px, y - py)).sqrMagnitude <= radius * radius;
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.spriteBorder = new Vector4(radius, radius, radius, radius);
            importer.SaveAndReimport();
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

            var card = CreatePanel(safe, "LoadingCard", new Vector2(0.5f, 0.52f), new Vector2(920, 720), Surface);
            card.gameObject.AddComponent<UiEntranceMotion>();
            var mark = CreateImage(card.transform, "GameMark", new Vector2(0.5f, 0.73f), new Vector2(210, 210), Color.white);
            mark.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AppIconPath); mark.preserveAspect = true;
            CreateText(card.transform, "Kicker", "A MOONLIT PALACE STORY", 22, Gold, new Vector2(0.5f, 0.53f), new Vector2(650, 42), FontStyles.Bold);
            CreateText(card.transform, "Title", "SHADOW TILE ESCAPE", 58, Ivory, new Vector2(0.5f, 0.42f), new Vector2(780, 90), FontStyles.Bold);
            CreateText(card.transform, "LoadingLabel", "THE PALACE IS READY", 18, Muted, new Vector2(0.5f, 0.31f), new Vector2(500, 36), FontStyles.Bold);
            var indicator = CreateImage(card.transform, "LoadingIndicator", new Vector2(0.5f, 0.25f), new Vector2(220, 5), Gold);
            indicator.sprite = buttonSprite; indicator.type = Image.Type.Sliced;
            var begin = CreateButton(card.transform, "BeginButton", "Enter the Palace", new Vector2(0.5f, 0.12f), new Vector2(500, 90), Violet, 25);
            CreateButtonIcon(begin.transform, UiIcon.Continue);
            UnityEventTools.AddPersistentListener(begin.onClick, flow.LoadDestination);
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
            var content = CreateRect(safe, "MenuContent", new Vector2(0.5f, 0.53f), new Vector2(1580, 820));
            var contentGroup = content.gameObject.AddComponent<CanvasGroup>();
            content.gameObject.AddComponent<UiEntranceMotion>();

            var brand = CreatePanel(content, "BrandingPanel", new Vector2(0.28f, 0.5f), new Vector2(850, 760), Surface);
            var moon = CreateImage(brand.transform, "CrescentMoon", new Vector2(0.16f, 0.79f), new Vector2(126, 126), Gold);
            moon.sprite = circleSprite; moon.preserveAspect = true; moon.color = new Color(Gold.r, Gold.g, Gold.b, 0.94f);
            var moonCutout = CreateImage(moon.transform, "MoonCutout", new Vector2(0.66f, 0.62f), new Vector2(105, 105), Surface);
            moonCutout.sprite = circleSprite; moonCutout.preserveAspect = true;
            var beam = CreateImage(brand.transform, "MoonlightBeam", new Vector2(0.23f, 0.67f), new Vector2(220, 26), new Color(Gold.r, Gold.g, Gold.b, 0.15f));
            beam.rectTransform.localEulerAngles = new Vector3(0, 0, -24);
            CreatePalaceSilhouette(brand.transform);
            var icon = CreateImage(brand.transform, "GameMark", new Vector2(0.78f, 0.74f), new Vector2(190, 190), Color.white);
            icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AppIconPath); icon.preserveAspect = true;
            CreateText(brand.transform, "StorySubtitle", "NOOR AND THE LIVING LIGHT", 22, Gold, new Vector2(0.47f, 0.78f), new Vector2(560, 38), FontStyles.Bold, TextAlignmentOptions.Left);
            var titleOne = CreateText(brand.transform, "TitleLineOne", "SHADOW TILE", 72, Ivory, new Vector2(0.47f, 0.60f), new Vector2(680, 105), FontStyles.Bold, TextAlignmentOptions.Left);
            var titleTwo = CreateText(brand.transform, "TitleLineTwo", "ESCAPE", 94, Ivory, new Vector2(0.47f, 0.47f), new Vector2(680, 130), FontStyles.Bold, TextAlignmentOptions.Left);
            titleOne.characterSpacing = -1.5f; titleTwo.characterSpacing = -2.5f;
            CreateText(brand.transform, "Hook", "Light is danger. Shadow is the path.", 29, Cyan, new Vector2(0.47f, 0.34f), new Vector2(700, 52), FontStyles.Normal, TextAlignmentOptions.Left);
            var story = CreateText(brand.transform, "StoryHint", "Guide Noor through a moonlit palace where every lantern watches.", 21, Muted, new Vector2(0.47f, 0.25f), new Vector2(700, 62), FontStyles.Normal, TextAlignmentOptions.Left);
            story.textWrappingMode = TextWrappingModes.Normal;

            var navigation = CreatePanel(content, "NavigationPanel", new Vector2(0.80f, 0.5f), new Vector2(540, 760), SurfaceRaised);
            CreateText(navigation.transform, "NavigationKicker", "THE SHADOW PATH", 19, Gold, new Vector2(0.5f, 0.91f), new Vector2(470, 34), FontStyles.Bold);
            CreateText(navigation.transform, "NavigationTitle", "Continue Noor's journey", 32, Ivory, new Vector2(0.5f, 0.84f), new Vector2(500, 48), FontStyles.Bold);
            var saveSummary = CreateText(navigation.transform, "SaveSummary", "Begin Noor's first crossing", 19, Muted, new Vector2(0.5f, 0.78f), new Vector2(500, 36));

            var continueButton = CreateButton(navigation.transform, "ContinueButton", "Continue", new Vector2(0.5f, 0.66f), new Vector2(500, 86), Violet, 25);
            var newGame = CreateButton(navigation.transform, "NewGameButton", "New Game", new Vector2(0.5f, 0.52f), new Vector2(500, 86), Palace, 25);
            var levelSelect = CreateButton(navigation.transform, "LevelSelectButton", "Level Select", new Vector2(0.5f, 0.38f), new Vector2(500, 86), Palace, 25);
            var howTo = CreateButton(navigation.transform, "HowToPlayButton", "How to Play", new Vector2(0.5f, 0.24f), new Vector2(500, 86), Palace, 25);
            var settings = CreateButton(navigation.transform, "SettingsButton", "Settings", new Vector2(0.5f, 0.10f), new Vector2(500, 86), Palace, 25);
            CreateButtonIcon(continueButton.transform, UiIcon.Continue);
            CreateButtonIcon(newGame.transform, UiIcon.NewGame);
            CreateButtonIcon(levelSelect.transform, UiIcon.Levels);
            CreateButtonIcon(howTo.transform, UiIcon.Help);
            CreateButtonIcon(settings.transform, UiIcon.Settings);
            UnityEventTools.AddPersistentListener(continueButton.onClick, menuController.Continue);
            UnityEventTools.AddPersistentListener(newGame.onClick, menuController.NewGame);
            UnityEventTools.AddPersistentListener(levelSelect.onClick, menuController.LevelSelect);
            UnityEventTools.AddPersistentListener(howTo.onClick, menuController.HowToPlay);

            var newGameConfirm = CreateModal(safe, "NewGameConfirmation", "Begin a new story?", "Existing progress will be reset. Audio and comfort settings will be kept.", Orange);
            var confirmNew = CreateButton(newGameConfirm.transform, "ConfirmNewGame", "Reset & Begin", new Vector2(0.42f, 0.30f), new Vector2(300, 92), Orange, 23);
            var cancelNew = CreateButton(newGameConfirm.transform, "CancelNewGame", "Cancel", new Vector2(0.60f, 0.30f), new Vector2(260, 92), Palace, 23);
            UnityEventTools.AddPersistentListener(confirmNew.onClick, menuController.ConfirmNewGame);
            UnityEventTools.AddPersistentListener(cancelNew.onClick, menuController.CancelNewGame);
            newGameConfirm.SetActive(false);

            var settingsController = CreateSettingsOverlay(safe, menuMusic, menuController, contentGroup);
            UnityEventTools.AddPersistentListener(settings.onClick, settingsController.Show);

            SetPrivate(menuController, "continueButton", continueButton);
            SetPrivate(menuController, "newGameButton", newGame);
            SetPrivate(menuController, "continueLabel", continueButton.GetComponentInChildren<TMP_Text>());
            SetPrivate(menuController, "saveSummaryLabel", saveSummary);
            SetPrivate(menuController, "newGameConfirmation", newGameConfirm);
            SetPrivate(menuController, "menuContent", contentGroup);

            CreateText(safe, "Version", "v0.9.0", 18, Muted, new Vector2(0.10f, 0.045f), new Vector2(250, 34), FontStyles.Normal, TextAlignmentOptions.Left);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        static void BuildIntroScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Intro";
            CreateCamera(); CreateEventSystem(); AddAmbientAudio("IntroAmbience");
            var safe = CreateCanvasHierarchy("IntroCanvas");
            var controller = new GameObject("IntroController").AddComponent<StaticScreenController>();
            controller.Destination = "Level_01";
            var content = CreateRect(safe, "IntroContent", new Vector2(0.5f, 0.52f), new Vector2(1660, 800));
            content.gameObject.AddComponent<CanvasGroup>(); content.gameObject.AddComponent<UiEntranceMotion>();
            var art = CreatePanel(content, "StoryArtwork", new Vector2(0.28f, 0.5f), new Vector2(760, 730), Surface);
            CreatePalaceSilhouette(art.transform);
            var moon = CreateImage(art.transform, "Moon", new Vector2(0.28f, 0.76f), new Vector2(150, 150), Gold);
            moon.sprite = circleSprite; moon.preserveAspect = true;
            var noor = CreateImage(art.transform, "NoorSilhouette", new Vector2(0.60f, 0.29f), new Vector2(90, 170), Cyan);
            noor.sprite = panelSprite; noor.type = Image.Type.Sliced;
            var beam = CreateImage(art.transform, "LanternBeam", new Vector2(0.52f, 0.50f), new Vector2(560, 46), new Color(Gold.r, Gold.g, Gold.b, 0.24f));
            beam.rectTransform.localEulerAngles = new Vector3(0, 0, -18);
            var story = CreatePanel(content, "StoryPanel", new Vector2(0.77f, 0.5f), new Vector2(770, 730), SurfaceRaised);
            CreateText(story.transform, "Kicker", "PROLOGUE  ·  THE LIVING LIGHT", 20, Gold, new Vector2(0.5f, 0.87f), new Vector2(610, 38), FontStyles.Bold, TextAlignmentOptions.Left);
            CreateText(story.transform, "Title", "Noor and the cursed palace", 46, Ivory, new Vector2(0.5f, 0.72f), new Vector2(630, 112), FontStyles.Bold, TextAlignmentOptions.Left);
            var body = CreateText(story.transform, "Body", "When the palace woke, every lantern became an eye. Noor must cross fifteen halls by shaping shadow itself—turning light, reading patrols, and trusting the quiet path between them.", 27, Muted, new Vector2(0.5f, 0.49f), new Vector2(630, 210), FontStyles.Normal, TextAlignmentOptions.Left);
            body.textWrappingMode = TextWrappingModes.Normal;
            CreateText(story.transform, "PageIndicator", "01  /  01", 18, Cyan, new Vector2(0.5f, 0.29f), new Vector2(240, 34), FontStyles.Bold);
            var begin = CreateButton(story.transform, "ContinueButton", "Begin the Escape", new Vector2(0.5f, 0.16f), new Vector2(500, 88), Violet, 24);
            var skip = CreateButton(story.transform, "SkipButton", "Skip Story", new Vector2(0.5f, 0.045f), new Vector2(300, 64), Palace, 19);
            UnityEventTools.AddPersistentListener(begin.onClick, controller.Continue);
            UnityEventTools.AddPersistentListener(skip.onClick, controller.Skip);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Intro.unity");
        }

        static void BuildHowToPlayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HowToPlay";
            CreateCamera(); CreateEventSystem(); AddAmbientAudio("HowToPlayAmbience");
            var safe = CreateCanvasHierarchy("HowToPlayCanvas");
            var controller = new GameObject("HowToPlayController").AddComponent<StaticScreenController>();
            controller.Destination = "MainMenu";
            CreateText(safe, "Kicker", "PALACE FIELD GUIDE", 19, Gold, new Vector2(0.5f, 0.94f), new Vector2(600, 34), FontStyles.Bold);
            CreateText(safe, "Title", "How to Walk the Shadow Path", 48, Ivory, new Vector2(0.5f, 0.88f), new Vector2(1100, 70), FontStyles.Bold);
            var cards = new[]
            {
                ("Movement", "Use the D-pad, arrows, or WASD. Each accepted action advances the palace.", UiIcon.Continue),
                ("Stay in shadow", "Gold-lit tiles are danger. Plan a complete turn before moving.", UiIcon.Hint),
                ("Rotate lamps", "Face a lamp and Interact to turn its beam through allowed directions.", UiIcon.Interact),
                ("Move boxes", "Push a box into a beam to make a temporary island of safety.", UiIcon.Levels),
                ("Reflect light", "Mirrors bend lantern rays. Rotate marked mirrors to redirect danger.", UiIcon.Settings),
                ("Close curtains", "Curtains can block a beam. Open them only when the route demands it.", UiIcon.Close),
                ("Read guards", "Ghost markers preview patrol movement. Their lanterns move with them.", UiIcon.Pause),
                ("Undo", "Rewind one accepted turn—including objects, guards, moonlight, and shards.", UiIcon.Undo),
                ("Restart", "Reset the entire puzzle whenever the route no longer feels recoverable.", UiIcon.Restart)
            };
            for (var i = 0; i < cards.Length; i++)
            {
                var row = i / 3;
                var column = i % 3;
                var card = CreatePanel(safe, $"GuideCard_{i + 1:00}", new Vector2(0.22f + column * 0.28f, 0.69f - row * 0.22f), new Vector2(480, 200), SurfaceRaised);
                CreateIconBadge(card.transform, cards[i].Item3, new Vector2(0.13f, 0.63f), 64);
                CreateText(card.transform, "Heading", cards[i].Item1, 25, Ivory, new Vector2(0.60f, 0.70f), new Vector2(330, 42), FontStyles.Bold, TextAlignmentOptions.Left);
                var copy = CreateText(card.transform, "Body", cards[i].Item2, 19, Muted, new Vector2(0.58f, 0.38f), new Vector2(340, 92), FontStyles.Normal, TextAlignmentOptions.Left);
                copy.textWrappingMode = TextWrappingModes.Normal;
            }
            var back = CreateButton(safe, "ContinueButton", "Back to Menu", new Vector2(0.5f, 0.07f), new Vector2(360, 78), Palace, 22);
            CreateButtonIcon(back.transform, UiIcon.Back);
            UnityEventTools.AddPersistentListener(back.onClick, controller.Continue);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/HowToPlay.unity");
        }

        static void BuildCreditsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Credits";
            CreateCamera(); CreateEventSystem(); AddAmbientAudio("CreditsAmbience");
            var safe = CreateCanvasHierarchy("CreditsCanvas");
            var controller = new GameObject("CreditsController").AddComponent<StaticScreenController>();
            controller.Destination = "MainMenu";
            CreateText(safe, "Kicker", "SETTINGS  ·  LEGAL", 19, Gold, new Vector2(0.5f, 0.93f), new Vector2(520, 34), FontStyles.Bold);
            CreateText(safe, "Title", "Credits & Licenses", 50, Ivory, new Vector2(0.5f, 0.86f), new Vector2(900, 74), FontStyles.Bold);
            CreateCreditsCard(safe, "CreditsCard", new Vector2(0.5f, 0.49f), new Vector2(1260, 650));
            var back = CreateButton(safe, "ContinueButton", "Back to Menu", new Vector2(0.5f, 0.075f), new Vector2(360, 78), Palace, 22);
            UnityEventTools.AddPersistentListener(back.onClick, controller.Continue);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Credits.unity");
        }

        static void BuildCompletionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Completion";
            CreateCamera(); CreateEventSystem(); AddAmbientAudio("CompletionAmbience");
            var safe = CreateCanvasHierarchy("CompletionCanvas");
            var controller = new GameObject("CompletionController").AddComponent<StaticScreenController>();
            var card = CreatePanel(safe, "CompletionCard", new Vector2(0.5f, 0.52f), new Vector2(1360, 760), Surface);
            card.gameObject.AddComponent<UiEntranceMotion>();
            var mark = CreateImage(card.transform, "GameMark", new Vector2(0.17f, 0.69f), new Vector2(240, 240), Color.white);
            mark.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AppIconPath); mark.preserveAspect = true;
            CreateText(card.transform, "Kicker", "DAWN BEYOND THE PALACE", 22, Gold, new Vector2(0.58f, 0.83f), new Vector2(740, 42), FontStyles.Bold);
            CreateText(card.transform, "Title", "Noor is Free", 72, Ivory, new Vector2(0.58f, 0.69f), new Vector2(780, 110), FontStyles.Bold);
            var body = CreateText(card.transform, "Body", "The living light falls quiet. Every cleared hall remains open for replay—but beyond the final gate, morning waits.", 27, Muted, new Vector2(0.58f, 0.53f), new Vector2(800, 120));
            body.textWrappingMode = TextWrappingModes.Normal;
            var totals = CreateText(card.transform, "CompletionTotals", "15/15 HALLS  ·  45/45 STARS", 22, Cyan, new Vector2(0.58f, 0.40f), new Vector2(800, 46), FontStyles.Bold);
            var replay = CreateButton(card.transform, "ReplayFinale", "Replay Finale", new Vector2(0.27f, 0.21f), new Vector2(300, 86), Palace, 22);
            var select = CreateButton(card.transform, "CompletionLevelSelect", "Level Select", new Vector2(0.50f, 0.21f), new Vector2(300, 86), Violet, 22);
            var menu = CreateButton(card.transform, "CompletionMainMenu", "Main Menu", new Vector2(0.73f, 0.21f), new Vector2(300, 86), Palace, 22);
            var credits = CreateButton(card.transform, "CompletionCredits", "Credits & Licenses", new Vector2(0.5f, 0.07f), new Vector2(360, 68), Palace, 19);
            UnityEventTools.AddPersistentListener(replay.onClick, controller.ReplayFinale);
            UnityEventTools.AddPersistentListener(select.onClick, controller.LevelSelect);
            UnityEventTools.AddPersistentListener(menu.onClick, controller.MainMenu);
            UnityEventTools.AddPersistentListener(credits.onClick, controller.Credits);
            SetPrivate(controller, "completionTotals", totals);
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
            CreateText(safe, "Kicker", "PALACE MAP", 18, Gold, new Vector2(0.5f, 0.95f), new Vector2(400, 32), FontStyles.Bold);
            CreateText(safe, "Title", "Choose a Shadow Path", 48, Ivory, new Vector2(0.5f, 0.90f), new Vector2(1000, 70), FontStyles.Bold);
            var overall = CreateText(safe, "OverallProgress", "PALACE JOURNEY  0/15", 19, Cyan, new Vector2(0.5f, 0.85f), new Vector2(900, 34), FontStyles.Bold);
            var content = CreateRect(safe, "ChapterContainer", new Vector2(0.5f, 0.48f), new Vector2(1600, 690));
            content.gameObject.AddComponent<CanvasGroup>(); content.gameObject.AddComponent<UiEntranceMotion>();
            var levelButtons = new LevelButtonController[15];
            var chapterProgress = new TMP_Text[3];
            var chapterNames = new[] { "Silent Halls", "Reflections", "Living Light" };
            var chapterAccents = new[] { Cyan, Violet, Gold };
            for (var chapter = 0; chapter < 3; chapter++)
            {
                var card = CreatePanel(content, $"Chapter_{chapter + 1:00}", new Vector2(0.18f + chapter * 0.32f, 0.5f), new Vector2(480, 670), SurfaceRaised);
                CreateText(card.transform, "ChapterNumber", $"CHAPTER {chapter + 1:00}", 17, chapterAccents[chapter], new Vector2(0.5f, 0.94f), new Vector2(390, 30), FontStyles.Bold);
                CreateText(card.transform, "ChapterTitle", chapterNames[chapter], 30, Ivory, new Vector2(0.5f, 0.88f), new Vector2(400, 46), FontStyles.Bold);
                chapterProgress[chapter] = CreateText(card.transform, "ChapterProgress", "0/5 COMPLETE", 14, Muted, new Vector2(0.5f, 0.82f), new Vector2(410, 30), FontStyles.Bold);
                for (var row = 0; row < 5; row++)
                {
                    var i = chapter * 5 + row;
                    var number = i + 1;
                    var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"Assets/Data/Levels/Level_{number:00}.asset");
                    var button = CreateButton(card.transform, $"LevelButton_{number:00}", definition.displayName,
                        new Vector2(0.5f, 0.71f - row * 0.145f), new Vector2(420, 82), Surface, 19);
                    var title = button.GetComponentInChildren<TMP_Text>();
                    title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.56f, 0.66f);
                    title.rectTransform.sizeDelta = new Vector2(300, 30);
                    title.alignment = TextAlignmentOptions.Left;
                    var numberLabel = CreateText(button.transform, "LevelNumber", $"{number:00}", 24, Ivory, new Vector2(0.12f, 0.56f), new Vector2(70, 36), FontStyles.Bold);
                    var progress = CreateText(button.transform, "Progress", "OPEN", 12, Muted, new Vector2(0.56f, 0.25f), new Vector2(300, 22), FontStyles.Bold, TextAlignmentOptions.Left);
                    var rail = CreateImage(button.transform, "StateRail", new Vector2(0.015f, 0.5f), new Vector2(6, 58), chapterAccents[chapter]);
                    rail.sprite = buttonSprite; rail.type = Image.Type.Sliced;
                    var item = button.gameObject.AddComponent<LevelButtonController>();
                    item.Configure(number, definition.displayName, button, numberLabel, title, progress, rail);
                    UnityEventTools.AddPersistentListener(button.onClick, item.Open);
                    levelButtons[i] = item;
                }
            }
            controller.LevelButtons = levelButtons;
            controller.ChapterProgressLabels = chapterProgress;
            controller.OverallProgressLabel = overall;
            EditorUtility.SetDirty(controller);
            var back = CreateButton(safe, "BackButton", "Back", new Vector2(0.095f, 0.06f), new Vector2(230, 72), Palace, 21);
            CreateButtonIcon(back.transform, UiIcon.Back);
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
            var content = CreateRect(safe, "GameplayContent", new Vector2(0.5f, 0.5f), new Vector2(1700, 980));
            var contentGroup = content.gameObject.AddComponent<CanvasGroup>();

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

            var topBand = CreatePanel(content, "TopBand", new Vector2(0.5f, 0.92f), new Vector2(1580, 112), SurfaceRaised);
            var levelLabel = CreateText(topBand.transform, "LevelLabel", $"LEVEL {definition.levelNumber:00}", 27, Ivory, new Vector2(0.17f, 0.62f), new Vector2(450, 38), FontStyles.Bold, TextAlignmentOptions.Left);
            var objectiveLabel = CreateText(topBand.transform, "ObjectiveLabel", definition.objectiveText, 17, Muted, new Vector2(0.29f, 0.28f), new Vector2(820, 36), FontStyles.Normal, TextAlignmentOptions.Left);
            var moveLabel = CreateText(topBand.transform, "MoveLabel", $"MOVES 0  ·  PAR {definition.par}", 17, Cyan, new Vector2(0.56f, 0.63f), new Vector2(400, 36), FontStyles.Bold);

            var boardFrame = CreatePanel(content, "BoardFrame", new Vector2(0.5f, 0.52f), new Vector2(736, 544), new Color32(34, 39, 82, 255));
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

            var statusPanel = CreatePanel(content, "StatusPanel", new Vector2(0.5f, 0.16f), new Vector2(880, 62), Surface);
            var status = CreateText(statusPanel.transform, "Status", "Stay in shadow. Gold tiles are danger.", 19, Ivory, new Vector2(0.5f, 0.5f), new Vector2(820, 38));
            var lampLabel = CreateText(content, "LampDirection", "LAMP  EAST", 18, Gold, new Vector2(0.5f, 0.81f), new Vector2(440, 34), FontStyles.Bold);

            var north = CreateButton(content, "MoveNorth", string.Empty, new Vector2(0.10f, 0.31f), new Vector2(104, 104), Palace, 40);
            var west = CreateButton(content, "MoveWest", string.Empty, new Vector2(0.045f, 0.20f), new Vector2(104, 104), Palace, 40);
            var south = CreateButton(content, "MoveSouth", string.Empty, new Vector2(0.10f, 0.09f), new Vector2(104, 104), Palace, 40);
            var east = CreateButton(content, "MoveEast", string.Empty, new Vector2(0.155f, 0.20f), new Vector2(104, 104), Palace, 40);
            CreateDirectionIcon(north.transform, 0);
            CreateDirectionIcon(east.transform, -90);
            CreateDirectionIcon(south.transform, 180);
            CreateDirectionIcon(west.transform, 90);
            var interact = CreateButton(content, "Interact", "Interact", new Vector2(0.90f, 0.19f), new Vector2(210, 116), Violet, 23);
            CreateIconBadge(content, UiIcon.Interact, new Vector2(0.90f, 0.32f), 66);
            var undo = CreateButton(topBand.transform, "Undo", "Undo", new Vector2(0.745f, 0.5f), new Vector2(118, 76), Palace, 18);
            var restart = CreateButton(topBand.transform, "Restart", "Restart", new Vector2(0.825f, 0.5f), new Vector2(114, 76), Palace, 18);
            var hint = CreateButton(topBand.transform, "Hint", "Hint", new Vector2(0.90f, 0.5f), new Vector2(100, 76), Palace, 18);
            var pause = CreateButton(topBand.transform, "Pause", "Pause", new Vector2(0.968f, 0.5f), new Vector2(100, 76), Palace, 17);

            UnityEventTools.AddPersistentListener(north.onClick, controller.MoveNorth);
            UnityEventTools.AddPersistentListener(east.onClick, controller.MoveEast);
            UnityEventTools.AddPersistentListener(south.onClick, controller.MoveSouth);
            UnityEventTools.AddPersistentListener(west.onClick, controller.MoveWest);
            UnityEventTools.AddPersistentListener(interact.onClick, controller.Interact);
            UnityEventTools.AddPersistentListener(undo.onClick, controller.Undo);
            UnityEventTools.AddPersistentListener(restart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(hint.onClick, controller.ShowHint);
            UnityEventTools.AddPersistentListener(pause.onClick, controller.TogglePause);

            var hintPanel = CreateModal(safe, "HintPanel", "A whisper in the dark", definition.hintText, Cyan);
            var closeHint = CreateButton(hintPanel.transform, "CloseHint", "Return to Puzzle", new Vector2(0.5f, 0.30f), new Vector2(340, 88), Violet, 22);
            UnityEventTools.AddPersistentListener(closeHint.onClick, controller.HideHint);
            hintPanel.SetActive(false);

            var failure = CreateModal(safe, "FailurePanel", "Caught in the light", "Undo the last turn or restart the puzzle.", Orange);
            var failureTitle = failure.transform.Find("ModalCard/Title").GetComponent<TMP_Text>();
            var failureReason = failure.transform.Find("ModalCard/Body").GetComponent<TMP_Text>();
            var failureUndo = CreateButton(failure.transform, "UndoFailure", "Undo Last Turn", new Vector2(0.30f, 0.30f), new Vector2(260, 90), Violet, 21);
            var failureRestart = CreateButton(failure.transform, "RestartFailure", "Retry", new Vector2(0.44f, 0.30f), new Vector2(210, 90), Palace, 22);
            var failureSelect = CreateButton(failure.transform, "FailureLevelSelect", "Level Select", new Vector2(0.58f, 0.30f), new Vector2(230, 90), Palace, 20);
            var failureMenu = CreateButton(failure.transform, "FailureMenu", "Main Menu", new Vector2(0.72f, 0.30f), new Vector2(220, 90), Palace, 20);
            UnityEventTools.AddPersistentListener(failureUndo.onClick, controller.Undo);
            UnityEventTools.AddPersistentListener(failureRestart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(failureSelect.onClick, controller.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(failureMenu.onClick, controller.BackToMenu);
            failure.SetActive(false);

            var victory = CreateModal(safe, "VictoryPanel", "The shadow path opens", $"Level {definition.levelNumber:00} complete", Cyan);
            var victoryStats = CreateText(victory.transform.Find("ModalCard"), "VictoryStats", "STARS  3/3", 23, Cyan, new Vector2(0.5f, 0.42f), new Vector2(620, 100), FontStyles.Bold);
            victoryStats.textWrappingMode = TextWrappingModes.Normal;
            var victoryReplay = CreateButton(victory.transform, "Replay", "Replay", new Vector2(0.28f, 0.25f), new Vector2(210, 88), Palace, 21);
            var victoryNext = CreateButton(victory.transform, "NextLevel", definition.levelNumber == 15 ? "Finale" : "Next Level", new Vector2(0.43f, 0.25f), new Vector2(240, 88), Violet, 21);
            var victorySelect = CreateButton(victory.transform, "VictoryLevelSelect", "Level Select", new Vector2(0.59f, 0.25f), new Vector2(240, 88), Palace, 20);
            var victoryMenu = CreateButton(victory.transform, "VictoryMenu", "Main Menu", new Vector2(0.74f, 0.25f), new Vector2(220, 88), Palace, 20);
            UnityEventTools.AddPersistentListener(victoryReplay.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(victoryNext.onClick, controller.NextLevel);
            UnityEventTools.AddPersistentListener(victorySelect.onClick, controller.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(victoryMenu.onClick, controller.BackToMenu);
            victory.SetActive(false);

            var pausePanel = CreateModal(safe, "PausePanel", "Paused", "The palace waits in silence.", Cyan);
            var resume = CreateButton(pausePanel.transform, "Resume", "Resume", new Vector2(0.28f, 0.30f), new Vector2(220, 90), Violet, 23);
            var pauseRestart = CreateButton(pausePanel.transform, "PauseRestart", "Restart", new Vector2(0.39f, 0.30f), new Vector2(200, 90), Palace, 21);
            var pauseSettings = CreateButton(pausePanel.transform, "PauseSettings", "Settings", new Vector2(0.50f, 0.30f), new Vector2(200, 90), Palace, 21);
            var pauseSelect = CreateButton(pausePanel.transform, "PauseLevelSelect", "Level Select", new Vector2(0.61f, 0.30f), new Vector2(220, 90), Palace, 20);
            var pauseMenu = CreateButton(pausePanel.transform, "PauseMenu", "Main Menu", new Vector2(0.72f, 0.30f), new Vector2(210, 90), Palace, 20);
            UnityEventTools.AddPersistentListener(resume.onClick, controller.Resume);
            UnityEventTools.AddPersistentListener(pauseRestart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(pauseSelect.onClick, controller.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(pauseMenu.onClick, controller.BackToMenu);
            pausePanel.SetActive(false);

            var settingsController = CreateSettingsOverlay(safe, ambienceSource, null, pausePanel.GetComponent<CanvasGroup>());
            UnityEventTools.AddPersistentListener(pauseSettings.onClick, settingsController.Show);
            SetPrivate(controller, "settingsController", settingsController);

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
            SetPrivate(controller, "objectiveLabel", objectiveLabel);
            SetPrivate(controller, "moveLabel", moveLabel);
            SetPrivate(controller, "statusLabel", status);
            SetPrivate(controller, "lampDirectionLabel", lampLabel);
            SetPrivate(controller, "failureTitleLabel", failureTitle);
            SetPrivate(controller, "failureReasonLabel", failureReason);
            SetPrivate(controller, "victoryStatsLabel", victoryStats);
            SetPrivate(controller, "failurePanel", failure);
            SetPrivate(controller, "victoryPanel", victory);
            SetPrivate(controller, "pausePanel", pausePanel);
            SetPrivate(controller, "hintPanel", hintPanel);
            SetPrivate(controller, "undoButton", undo);
            SetPrivate(controller, "turnPulseView", pulse);
            SetPrivate(controller, "sfxSource", sfxSource);
            SetPrivate(controller, "ambienceSource", ambienceSource);
            SetPrivate(controller, "audioMixer", AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath));
            SetPrivate(controller, "gameplayContent", contentGroup);
            SetPrivate(controller, "moveClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/move.wav"));
            SetPrivate(controller, "interactClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/interact.wav"));
            SetPrivate(controller, "undoClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/undo.wav"));
            SetPrivate(controller, "failureClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/failure.wav"));
            SetPrivate(controller, "victoryClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Generated/victory.wav"));

            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Levels/Level_{definition.levelNumber:00}.unity");
        }

        static SettingsController CreateSettingsOverlay(Transform safe, AudioSource musicSource,
            MainMenuController menuController, CanvasGroup backgroundControls)
        {
            var controller = new GameObject(menuController == null ? "GameplaySettingsController" : "SettingsController").AddComponent<SettingsController>();
            var panel = CreateModal(safe, "SettingsPanel", "Settings", "Audio, comfort, and saved progress", Cyan);
            var card = panel.transform.Find("ModalCard");
            var cardGroup = card.GetComponent<CanvasGroup>();
            card.Find("Body").gameObject.SetActive(false);

            CreateText(card, "MusicLabel", "Music", 21, Ivory, new Vector2(0.25f, 0.61f), new Vector2(180, 38), FontStyles.Bold, TextAlignmentOptions.Left);
            var musicValue = CreateText(card, "MusicValue", "80%", 18, Cyan, new Vector2(0.78f, 0.61f), new Vector2(100, 32), FontStyles.Bold, TextAlignmentOptions.Right);
            var musicSlider = CreateSlider(card, "MusicSlider", new Vector2(0.53f, 0.61f));
            CreateText(card, "SfxLabel", "Sound effects", 21, Ivory, new Vector2(0.25f, 0.49f), new Vector2(180, 38), FontStyles.Bold, TextAlignmentOptions.Left);
            var sfxValue = CreateText(card, "SfxValue", "90%", 18, Cyan, new Vector2(0.78f, 0.49f), new Vector2(100, 32), FontStyles.Bold, TextAlignmentOptions.Right);
            var sfxSlider = CreateSlider(card, "SfxSlider", new Vector2(0.53f, 0.49f));
            var haptics = CreateToggle(card, "HapticsToggle", "Haptics", new Vector2(0.34f, 0.37f));
            var reduced = CreateToggle(card, "ReducedFlashingToggle", "Reduced flashing", new Vector2(0.68f, 0.37f));
            var tutorialReset = CreateButton(card, "ResetTutorial", "Reset Tutorial", new Vector2(0.28f, 0.22f), new Vector2(260, 72), Palace, 19);
            var reset = CreateButton(card, "ResetProgress", "Reset Progress", new Vector2(0.50f, 0.22f), new Vector2(260, 72), Orange, 19);
            var credits = CreateButton(card, "CreditsAndLicenses", "Credits & Licenses", new Vector2(0.72f, 0.22f), new Vector2(280, 72), Palace, 19);
            var close = CreateButton(card, "CloseSettings", "Done", new Vector2(0.5f, 0.055f), new Vector2(280, 76), Violet, 22);
            var settingsStatus = CreateText(card, "SettingsStatus", "Settings save automatically.", 16, Muted, new Vector2(0.5f, 0.15f), new Vector2(680, 28));

            var resetConfirm = CreateModal(panel.transform, "ResetConfirmation", "Reset all progress?", "Stars, shards, best moves, story progress, and unlocked levels will be cleared. Audio and comfort settings stay.", Orange);
            var confirmReset = CreateButton(resetConfirm.transform, "ConfirmReset", "Reset Progress", new Vector2(0.43f, 0.29f), new Vector2(270, 88), Orange, 21);
            var cancelReset = CreateButton(resetConfirm.transform, "CancelReset", "Cancel", new Vector2(0.59f, 0.29f), new Vector2(220, 88), Palace, 21);
            resetConfirm.SetActive(false);

            var tutorialConfirm = CreateModal(panel.transform, "TutorialResetConfirmation", "Reset tutorial tips?", "The next visit will show the tutorial guidance again. Game progress will not change.", Gold);
            var confirmTutorial = CreateButton(tutorialConfirm.transform, "ConfirmTutorialReset", "Reset Tutorial", new Vector2(0.43f, 0.29f), new Vector2(270, 88), Violet, 21);
            var cancelTutorial = CreateButton(tutorialConfirm.transform, "CancelTutorialReset", "Cancel", new Vector2(0.59f, 0.29f), new Vector2(220, 88), Palace, 21);
            tutorialConfirm.SetActive(false);

            var creditsPanel = CreateModal(panel.transform, "CreditsPanel", "Credits & Licenses", "Full in-game notices", Gold);
            var creditsCard = creditsPanel.transform.Find("ModalCard");
            creditsCard.Find("Body").gameObject.SetActive(false);
            CreateCreditsScroll(creditsCard);
            var closeCredits = CreateButton(creditsPanel.transform, "CloseCredits", "Back to Settings", new Vector2(0.5f, 0.22f), new Vector2(320, 76), Violet, 20);
            creditsPanel.SetActive(false);

            UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, controller.SetMusic);
            UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, controller.SetSfx);
            UnityEventTools.AddPersistentListener(haptics.onValueChanged, controller.SetHaptics);
            UnityEventTools.AddPersistentListener(reduced.onValueChanged, controller.SetReducedFlashing);
            UnityEventTools.AddPersistentListener(tutorialReset.onClick, controller.AskTutorialReset);
            UnityEventTools.AddPersistentListener(reset.onClick, controller.AskReset);
            UnityEventTools.AddPersistentListener(credits.onClick, controller.ShowCredits);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(confirmReset.onClick, controller.ConfirmReset);
            UnityEventTools.AddPersistentListener(cancelReset.onClick, controller.CancelReset);
            UnityEventTools.AddPersistentListener(confirmTutorial.onClick, controller.ConfirmTutorialReset);
            UnityEventTools.AddPersistentListener(cancelTutorial.onClick, controller.CancelTutorialReset);
            UnityEventTools.AddPersistentListener(closeCredits.onClick, controller.HideCredits);

            SetPrivate(controller, "panel", panel);
            SetPrivate(controller, "resetConfirmation", resetConfirm);
            SetPrivate(controller, "tutorialResetConfirmation", tutorialConfirm);
            SetPrivate(controller, "creditsPanel", creditsPanel);
            SetPrivate(controller, "musicSlider", musicSlider);
            SetPrivate(controller, "sfxSlider", sfxSlider);
            SetPrivate(controller, "hapticsToggle", haptics);
            SetPrivate(controller, "reducedFlashingToggle", reduced);
            SetPrivate(controller, "musicValueLabel", musicValue);
            SetPrivate(controller, "sfxValueLabel", sfxValue);
            SetPrivate(controller, "status", settingsStatus);
            SetPrivate(controller, "mixer", AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath));
            SetPrivate(controller, "musicSource", musicSource);
            SetPrivate(controller, "mainMenuController", menuController);
            SetPrivate(controller, "backgroundControls", backgroundControls);
            SetPrivate(controller, "settingsControls", cardGroup);
            panel.SetActive(false);
            return controller;
        }

        static GameObject CreateModal(Transform parent, string name, string title, string body, Color accent)
        {
            var panelImage = CreateImage(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, new Color32(13, 17, 40, 245));
            Stretch(panelImage.rectTransform);
            panelImage.raycastTarget = true;
            var panel = panelImage.gameObject;
            panel.AddComponent<CanvasGroup>();
            var card = CreatePanel(panel.transform, "ModalCard", new Vector2(0.5f, 0.52f), new Vector2(1160, 690), SurfaceRaised);
            card.raycastTarget = true;
            var group = card.gameObject.AddComponent<CanvasGroup>();
            card.gameObject.AddComponent<UiEntranceMotion>();
            var ornament = CreateImage(card.transform, "Accent", new Vector2(0.5f, 0.84f), new Vector2(48, 48), accent);
            ornament.sprite = circleSprite; ornament.preserveAspect = true;
            CreateText(card.transform, "Title", title, 44, Ivory, new Vector2(0.5f, 0.73f), new Vector2(920, 70), FontStyles.Bold);
            var bodyText = CreateText(card.transform, "Body", body, 24, Muted, new Vector2(0.5f, 0.59f), new Vector2(900, 96));
            bodyText.textWrappingMode = TextWrappingModes.Normal;
            return panel;
        }

        static Image CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            var panel = CreateImage(parent, name, anchor, size, color);
            panel.sprite = panelSprite;
            panel.type = Image.Type.Sliced;
            var shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.34f);
            shadow.effectDistance = new Vector2(0, -10);
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.18f);
            outline.effectDistance = new Vector2(2, -2);
            return panel;
        }

        static void CreatePalaceSilhouette(Transform parent)
        {
            var baseLine = CreateImage(parent, "PalaceBase", new Vector2(0.5f, 0.055f), new Vector2(700, 58), Palace);
            baseLine.sprite = panelSprite; baseLine.type = Image.Type.Sliced;
            for (var i = 0; i < 5; i++)
            {
                var tower = CreateImage(parent, $"Arch_{i + 1}", new Vector2(0.18f + i * 0.16f, 0.12f), new Vector2(88, 130 + (i % 2) * 30), Palace);
                tower.sprite = panelSprite; tower.type = Image.Type.Sliced;
                var cutout = CreateImage(tower.transform, "Doorway", new Vector2(0.5f, 0.18f), new Vector2(34, 62), Navy);
                cutout.sprite = panelSprite; cutout.type = Image.Type.Sliced;
            }
        }

        static void CreateAmbientDust(Transform parent)
        {
            var root = CreateRect(parent, "AmbientDust", new Vector2(0.5f, 0.5f), Vector2.zero);
            Stretch(root);
            var particles = new RectTransform[12];
            for (var i = 0; i < particles.Length; i++)
            {
                var dust = CreateImage(root, $"Dust_{i + 1:00}", new Vector2(0.08f + (i * 0.083f) % 0.84f, 0.12f + (i * 0.137f) % 0.74f), new Vector2(5 + i % 3 * 2, 5 + i % 3 * 2), new Color(0.72f, 0.82f, 1f, 0.18f));
                dust.sprite = circleSprite; dust.preserveAspect = true;
                particles[i] = dust.rectTransform;
            }
            var motion = root.gameObject.AddComponent<UiAmbientMotion>();
            SetPrivate(motion, "particles", particles);
        }

        static void CreateButtonIcon(Transform button, UiIcon kind)
        {
            var label = button.GetComponentInChildren<TMP_Text>();
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.58f, 0.5f);
            label.rectTransform.sizeDelta = new Vector2(Mathf.Max(80, ((RectTransform)button).sizeDelta.x - 130), Mathf.Max(34, ((RectTransform)button).sizeDelta.y - 18));
            label.alignment = TextAlignmentOptions.Left;
            CreateIconBadge(button, kind, new Vector2(0.11f, 0.5f), 50);
        }

        static void CreateIconBadge(Transform parent, UiIcon kind, Vector2 anchor, float size)
        {
            var badge = CreateImage(parent, "Icon", anchor, new Vector2(size, size), new Color(Cyan.r, Cyan.g, Cyan.b, 0.13f));
            badge.sprite = circleSprite; badge.preserveAspect = true;
            CreateIconGeometry(badge.transform, kind, Mathf.Max(2, size * 0.075f));
        }

        static void CreateIconGeometry(Transform parent, UiIcon kind, float stroke)
        {
            void Line(string name, Vector2 anchor, Vector2 size, float angle = 0)
            {
                var line = CreateImage(parent, name, anchor, size, Cyan);
                line.sprite = buttonSprite; line.type = Image.Type.Sliced;
                line.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            }

            switch (kind)
            {
                case UiIcon.Continue: Line("Arrow", new Vector2(0.5f, 0.5f), new Vector2(stroke * 2, stroke * 6), -45); Line("ArrowHead", new Vector2(0.58f, 0.5f), new Vector2(stroke * 2, stroke * 4), 45); break;
                case UiIcon.NewGame: Line("Horizontal", new Vector2(0.5f, 0.5f), new Vector2(stroke * 6, stroke)); Line("Vertical", new Vector2(0.5f, 0.5f), new Vector2(stroke, stroke * 6)); break;
                case UiIcon.Levels:
                    for (var i = 0; i < 4; i++) { var block = CreateImage(parent, $"Tile_{i}", new Vector2(0.36f + i % 2 * 0.28f, 0.36f + i / 2 * 0.28f), new Vector2(stroke * 2.2f, stroke * 2.2f), Cyan); block.sprite = buttonSprite; block.type = Image.Type.Sliced; }
                    break;
                case UiIcon.Pause: Line("Left", new Vector2(0.39f, 0.5f), new Vector2(stroke * 1.4f, stroke * 6)); Line("Right", new Vector2(0.61f, 0.5f), new Vector2(stroke * 1.4f, stroke * 6)); break;
                case UiIcon.Back:
                case UiIcon.Undo: Line("Stem", new Vector2(0.54f, 0.5f), new Vector2(stroke * 6, stroke)); Line("Upper", new Vector2(0.34f, 0.58f), new Vector2(stroke * 3, stroke), 45); Line("Lower", new Vector2(0.34f, 0.42f), new Vector2(stroke * 3, stroke), -45); break;
                case UiIcon.Close: Line("Slash", new Vector2(0.5f, 0.5f), new Vector2(stroke, stroke * 7), 45); Line("Backslash", new Vector2(0.5f, 0.5f), new Vector2(stroke, stroke * 7), -45); break;
                case UiIcon.Restart: Line("Top", new Vector2(0.5f, 0.67f), new Vector2(stroke * 5, stroke)); Line("Right", new Vector2(0.68f, 0.5f), new Vector2(stroke, stroke * 4)); Line("Arrow", new Vector2(0.30f, 0.67f), new Vector2(stroke * 3, stroke), -45); break;
                case UiIcon.Settings: Line("Diamond", new Vector2(0.5f, 0.5f), new Vector2(stroke * 5, stroke * 5), 45); var core = CreateImage(parent, "Core", new Vector2(0.5f, 0.5f), new Vector2(stroke * 2, stroke * 2), Surface); core.sprite = circleSprite; core.preserveAspect = true; break;
                case UiIcon.Hint: var bulb = CreateImage(parent, "Bulb", new Vector2(0.5f, 0.58f), new Vector2(stroke * 5, stroke * 5), Cyan); bulb.sprite = circleSprite; bulb.preserveAspect = true; Line("Stem", new Vector2(0.5f, 0.30f), new Vector2(stroke * 3, stroke)); break;
                case UiIcon.Interact: Line("Diamond", new Vector2(0.5f, 0.5f), new Vector2(stroke * 4, stroke * 4), 45); break;
                case UiIcon.Credits: var head = CreateImage(parent, "Head", new Vector2(0.5f, 0.62f), new Vector2(stroke * 3, stroke * 3), Cyan); head.sprite = circleSprite; head.preserveAspect = true; Line("Body", new Vector2(0.5f, 0.36f), new Vector2(stroke * 5, stroke * 2)); break;
                default: Line("Top", new Vector2(0.5f, 0.62f), new Vector2(stroke * 5, stroke)); Line("Bottom", new Vector2(0.5f, 0.38f), new Vector2(stroke * 5, stroke)); break;
            }
        }

        static void CreateDirectionIcon(Transform parent, float angle)
        {
            var root = CreateRect(parent, "DirectionIcon", new Vector2(0.5f, 0.5f), new Vector2(46, 46));
            root.localEulerAngles = new Vector3(0, 0, angle);
            var stem = CreateImage(root, "Stem", new Vector2(0.5f, 0.46f), new Vector2(6, 30), Cyan);
            stem.sprite = buttonSprite; stem.type = Image.Type.Sliced;
            var left = CreateImage(root, "ArrowLeft", new Vector2(0.39f, 0.72f), new Vector2(6, 20), Cyan);
            left.sprite = buttonSprite; left.type = Image.Type.Sliced; left.rectTransform.localEulerAngles = new Vector3(0, 0, -45);
            var right = CreateImage(root, "ArrowRight", new Vector2(0.61f, 0.72f), new Vector2(6, 20), Cyan);
            right.sprite = buttonSprite; right.type = Image.Type.Sliced; right.rectTransform.localEulerAngles = new Vector3(0, 0, 45);
        }

        static readonly string CreditsText =
            "SHADOW TILE ESCAPE\nOriginal design, programming, level design, UI, geometric game art, and generated feedback tones.\n\n" +
            "NUNITO SANS\nCopyright The Nunito Sans Project Authors. Licensed under the SIL Open Font License 1.1. The font and license are bundled with the game project.\n\n" +
            "DEVELOPMENT TOOLING\nMade with AnkleBreaker MCP.\n\n" +
            "OPENAI IMAGE GENERATION\nOriginal Shadow Tile Escape app icon generated for this project with OpenAI built-in ImageGen.\n\n" +
            "UNITY PACKAGES\nUnity Engine, Universal Render Pipeline, Input System, TextMesh Pro, uGUI, Test Framework, Performance Testing, Mathematics, and their transitive Unity packages are used under their applicable Unity package terms.\n\n" +
            "AUDIO & ART\nAmbient loop and interface cues are original editor-generated waveforms. All remaining UI geometry and icons are original project-authored assets. No paid or copied icon packs are used.";

        static void CreateCreditsCard(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            var card = CreatePanel(parent, name, anchor, size, SurfaceRaised);
            CreateCreditsScroll(card.transform);
        }

        static void CreateCreditsScroll(Transform card)
        {
            var viewport = CreateImage(card, "CreditsViewport", new Vector2(0.5f, 0.48f), new Vector2(980, 390), new Color(0, 0, 0, 0.12f));
            viewport.sprite = panelSprite; viewport.type = Image.Type.Sliced; viewport.raycastTarget = true;
            var mask = viewport.gameObject.AddComponent<RectMask2D>();
            var content = CreateRect(viewport.transform, "CreditsContent", new Vector2(0.5f, 1f), new Vector2(900, 760));
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1); content.anchoredPosition = new Vector2(0, -24);
            var text = CreateText(content, "CreditsText", CreditsText, 19, Muted, new Vector2(0.5f, 1f), new Vector2(860, 720), FontStyles.Normal, TextAlignmentOptions.TopLeft);
            text.textWrappingMode = TextWrappingModes.Normal;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform; scroll.content = content; scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 24;
            scroll.verticalNormalizedPosition = 1f;
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
            var glow = CreateImage(canvasObject.transform, "MoonGlow", new Vector2(0.5f, 1.06f), new Vector2(520, 520), new Color(0.24f, 0.34f, 0.78f, 0.08f));
            glow.sprite = circleSprite; glow.preserveAspect = true;
            glow.raycastTarget = false;
            glow.gameObject.SetActive(false);
            CreateAmbientDust(canvasObject.transform);

            var safe = CreateRect(canvasObject.transform, "SafeAreaRoot", new Vector2(0.5f, 0.5f), Vector2.zero);
            Stretch(safe);
            var safeArea = canvasObject.AddComponent<SafeAreaController>();
            SetPrivate(safeArea, "safeAreaRoot", safe);
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
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            var shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.32f);
            shadow.effectDistance = new Vector2(0, -5);
            var outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.22f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.88f);
            colors.selectedColor = new Color(1f, 0.96f, 0.82f, 1f);
            colors.pressedColor = new Color(0.72f, 0.76f, 0.92f, 1);
            colors.disabledColor = new Color(0.40f, 0.43f, 0.56f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            image.gameObject.AddComponent<UiButtonFeedback>();
            CreateText(image.transform, "Label", label, fontSize, Ivory, new Vector2(0.5f, 0.5f), size - new Vector2(16, 12), FontStyles.Bold);
            return button;
        }

        static Slider CreateSlider(Transform parent, string name, Vector2 anchor)
        {
            var root = CreateImage(parent, name, anchor, new Vector2(420, 48), new Color32(42, 46, 92, 255));
            root.sprite = buttonSprite; root.type = Image.Type.Sliced;
            root.raycastTarget = true;
            var fillArea = CreateRect(root.transform, "FillArea", new Vector2(0.5f, 0.5f), new Vector2(380, 24));
            var fill = CreateImage(fillArea, "Fill", new Vector2(0.5f, 0.5f), Vector2.zero, Cyan).rectTransform;
            Stretch(fill);
            var handleArea = CreateRect(root.transform, "HandleArea", new Vector2(0.5f, 0.5f), new Vector2(380, 48));
            var handle = CreateImage(handleArea, "Handle", new Vector2(0, 0.5f), new Vector2(36, 54), Ivory);
            handle.sprite = circleSprite; handle.preserveAspect = true;
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
            box.sprite = buttonSprite; box.type = Image.Type.Sliced;
            box.raycastTarget = true;
            var check = CreateImage(box.transform, "Check", new Vector2(0.5f, 0.5f), new Vector2(34, 34), Cyan);
            check.sprite = circleSprite; check.preserveAspect = true;
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
            PlayerSettings.companyName = "Moonlit Sicku Games";
            PlayerSettings.productName = "Shadow Tile Escape";
            PlayerSettings.bundleVersion = "0.9.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.moonlitloom.shadowtileescape");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.moonlitsicku.shadowtileescape");
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
