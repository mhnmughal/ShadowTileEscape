using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
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

        static readonly Color Navy = new Color32(9, 13, 29, 255);
        static readonly Color Indigo = new Color32(21, 26, 58, 255);
        static readonly Color Palace = new Color32(37, 40, 90, 255);
        static readonly Color Cyan = new Color32(99, 217, 230, 255);
        static readonly Color Gold = new Color32(242, 184, 75, 255);
        static readonly Color Violet = new Color32(154, 120, 212, 255);
        static readonly Color Ivory = new Color32(244, 238, 221, 255);
        static readonly Color Orange = new Color32(233, 106, 71, 255);

        static TMP_FontAsset font;

        [MenuItem("Shadow Tile Escape/Build/Build Vertical Slice")]
        public static void Build()
        {
            EnsureFolders();
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) throw new InvalidOperationException($"Nunito Sans TMP asset missing at {FontPath}");

            var definition = BuildLevelDefinition();
            BuildBootScene();
            BuildMenuScene();
            BuildLevelScene(definition);
            ConfigureProject();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene("Assets/Scenes/Levels/Level_01.unity");
            Debug.Log("[ShadowTileEscape] Vertical slice built: Boot, MainMenu, Level_01, LevelDefinition, build settings, and mobile settings.");
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Data");
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
            definition.width = 7;
            definition.height = 5;
            definition.par = 10;
            definition.requiredShards = 1;
            definition.cells = new CellFlags[35];
            definition.playerStart = new GridCoord(0, 0);
            definition.playerFacing = Direction.East;
            definition.exit = new GridCoord(6, 4);
            definition.lights = new[]
            {
                new LightSourceState { position = new GridCoord(3, 2), direction = Direction.East, range = 3, active = true }
            };
            definition.shards = new[] { new GridCoord(5, 4) };
            definition.mirrors = Array.Empty<MirrorState>();
            definition.boxes = Array.Empty<GridCoord>();
            definition.curtains = Array.Empty<CurtainState>();
            definition.guards = Array.Empty<GuardState>();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Boot";
            CreateCamera();
            CreateEventSystem();
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
            var flow = new GameObject("SceneFlow").AddComponent<SceneFlowController>();

            var ornament = CreateImage(safe, "PalaceArch", new Vector2(0.5f, 0.54f), new Vector2(1120, 740), Palace);
            ornament.color = new Color(Palace.r, Palace.g, Palace.b, 0.72f);
            CreateText(safe, "Chapter", "CHAPTER I  ·  SILENT HALLS", 28, Gold, new Vector2(0.5f, 0.78f), new Vector2(800, 48), FontStyles.Bold);
            CreateText(safe, "Title", "SHADOW TILE\nESCAPE", 88, Ivory, new Vector2(0.5f, 0.61f), new Vector2(1000, 230), FontStyles.Bold);
            CreateText(safe, "Tagline", "Light is danger. Shadow is the path.", 34, Cyan, new Vector2(0.5f, 0.45f), new Vector2(900, 60));

            var start = CreateButton(safe, "StartButton", "PLAY LEVEL 01", new Vector2(0.5f, 0.30f), new Vector2(520, 108), Violet);
            UnityEventTools.AddPersistentListener(start.onClick, flow.LoadLevelOne);
            CreateText(safe, "Controls", "MOVE  WASD / ARROWS     INTERACT  E / SPACE     UNDO  Z / BACKSPACE", 24, new Color(1, 1, 1, 0.7f), new Vector2(0.5f, 0.17f), new Vector2(1200, 50));
            CreateText(safe, "Attribution", "Made with AnkleBreaker MCP  ·  Nunito Sans / SIL Open Font License", 20, new Color(1, 1, 1, 0.48f), new Vector2(0.5f, 0.07f), new Vector2(1050, 38));
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        static void BuildLevelScene(LevelDefinition definition)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level_01";
            CreateCamera();
            CreateEventSystem();
            var safe = CreateCanvasHierarchy("GameplayCanvas");

            var controllerObject = new GameObject("GameplayController");
            var controller = controllerObject.AddComponent<GameplayController>();

            var topBand = CreateImage(safe, "TopBand", new Vector2(0.5f, 0.94f), new Vector2(1720, 104), Indigo);
            var levelLabel = CreateText(topBand.transform, "LevelLabel", "LEVEL 01", 34, Ivory, new Vector2(0.31f, 0.5f), new Vector2(720, 55), FontStyles.Bold, TextAlignmentOptions.Left);
            var moveLabel = CreateText(topBand.transform, "MoveLabel", "MOVES  0 / PAR 10", 27, Cyan, new Vector2(0.65f, 0.5f), new Vector2(700, 50));

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
            var shard = CreateImage(boardRoot, "MoonShard", new Vector2(0.5f, 0.5f), new Vector2(34, 34), Cyan).rectTransform;
            shard.localEulerAngles = new Vector3(0, 0, 45);

            var lamp = CreateImage(boardRoot, "Lamp", new Vector2(0.5f, 0.5f), new Vector2(62, 44), Gold).rectTransform;
            var lampTip = CreateImage(lamp, "FacingTip", new Vector2(1, 0.5f), new Vector2(18, 28), Ivory);
            lampTip.rectTransform.anchoredPosition = new Vector2(8, 0);

            var player = CreateImage(boardRoot, "Noor", new Vector2(0.5f, 0.5f), new Vector2(58, 42), Cyan).rectTransform;
            var cloak = CreateImage(player, "Cloak", new Vector2(0.35f, 0.5f), new Vector2(34, 34), Navy);
            cloak.rectTransform.anchoredPosition = Vector2.zero;
            var facing = CreateImage(player, "Facing", new Vector2(1, 0.5f), new Vector2(16, 18), Cyan);
            facing.rectTransform.anchoredPosition = new Vector2(5, 0);

            var status = CreateText(safe, "Status", "Stay in shadow. Gold tiles are danger.", 27, Ivory, new Vector2(0.5f, 0.19f), new Vector2(1000, 52));
            var lampLabel = CreateText(safe, "LampDirection", "LAMP  EAST", 23, Gold, new Vector2(0.5f, 0.84f), new Vector2(480, 44), FontStyles.Bold);

            var north = CreateButton(safe, "MoveNorth", "N", new Vector2(0.11f, 0.29f), new Vector2(104, 104), Palace, 48);
            var west = CreateButton(safe, "MoveWest", "W", new Vector2(0.055f, 0.18f), new Vector2(104, 104), Palace, 48);
            var south = CreateButton(safe, "MoveSouth", "S", new Vector2(0.11f, 0.07f), new Vector2(104, 104), Palace, 48);
            var east = CreateButton(safe, "MoveEast", "E", new Vector2(0.165f, 0.18f), new Vector2(104, 104), Palace, 48);
            var interact = CreateButton(safe, "Interact", "ROTATE\nLAMP", new Vector2(0.89f, 0.17f), new Vector2(190, 116), Violet, 27);
            var undo = CreateButton(topBand.transform, "Undo", "UNDO", new Vector2(0.82f, 0.5f), new Vector2(150, 72), Palace, 24);
            var restart = CreateButton(topBand.transform, "Restart", "RESTART", new Vector2(0.92f, 0.5f), new Vector2(150, 72), Palace, 23);
            var menu = CreateButton(topBand.transform, "Menu", "MENU", new Vector2(0.06f, 0.5f), new Vector2(140, 72), Palace, 23);

            UnityEventTools.AddPersistentListener(north.onClick, controller.MoveNorth);
            UnityEventTools.AddPersistentListener(east.onClick, controller.MoveEast);
            UnityEventTools.AddPersistentListener(south.onClick, controller.MoveSouth);
            UnityEventTools.AddPersistentListener(west.onClick, controller.MoveWest);
            UnityEventTools.AddPersistentListener(interact.onClick, controller.Interact);
            UnityEventTools.AddPersistentListener(undo.onClick, controller.Undo);
            UnityEventTools.AddPersistentListener(restart.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(menu.onClick, controller.BackToMenu);

            var failure = CreateModal(safe, "FailurePanel", "CAUGHT IN THE LIGHT", "Undo the last turn or restart the puzzle.", Orange);
            var failureUndo = CreateButton(failure.transform, "UndoFailure", "UNDO LAST TURN", new Vector2(0.39f, 0.31f), new Vector2(300, 92), Violet, 25);
            var failureRestart = CreateButton(failure.transform, "RestartFailure", "RESTART", new Vector2(0.61f, 0.31f), new Vector2(260, 92), Palace, 25);
            UnityEventTools.AddPersistentListener(failureUndo.onClick, controller.Undo);
            UnityEventTools.AddPersistentListener(failureRestart.onClick, controller.Restart);
            failure.SetActive(false);

            var victory = CreateModal(safe, "VictoryPanel", "THE SHADOW PATH OPENS", "Moon shard recovered  ·  Level 01 complete", Cyan);
            var victoryReplay = CreateButton(victory.transform, "Replay", "REPLAY", new Vector2(0.39f, 0.31f), new Vector2(260, 92), Palace, 25);
            var victoryMenu = CreateButton(victory.transform, "VictoryMenu", "MAIN MENU", new Vector2(0.61f, 0.31f), new Vector2(280, 92), Violet, 25);
            UnityEventTools.AddPersistentListener(victoryReplay.onClick, controller.Restart);
            UnityEventTools.AddPersistentListener(victoryMenu.onClick, controller.BackToMenu);
            victory.SetActive(false);

            controller.Definition = definition;
            EditorUtility.SetDirty(controller);
            SetPrivate(controller, "boardRoot", boardRoot);
            SetPrivate(controller, "cellViews", cells);
            SetPrivate(controller, "lightViews", lights);
            SetPrivate(controller, "playerView", player);
            SetPrivate(controller, "exitView", exit);
            SetPrivate(controller, "lampView", lamp);
            SetPrivate(controller, "shardView", shard);
            SetPrivate(controller, "levelLabel", levelLabel);
            SetPrivate(controller, "moveLabel", moveLabel);
            SetPrivate(controller, "statusLabel", status);
            SetPrivate(controller, "lampDirectionLabel", lampLabel);
            SetPrivate(controller, "failurePanel", failure);
            SetPrivate(controller, "victoryPanel", victory);
            SetPrivate(controller, "undoButton", undo);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Levels/Level_01.unity");
        }

        static GameObject CreateModal(Transform parent, string name, string title, string body, Color accent)
        {
            var panel = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(1120, 590), new Color32(13, 17, 40, 250)).gameObject;
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
            text.enableWordWrapping = false;
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
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.moonlitloom.shadowtileescape");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.moonlitloom.shadowtileescape");
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Application.targetFrameRate = 60;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Levels/Level_01.unity", true)
            };
        }
    }
}
