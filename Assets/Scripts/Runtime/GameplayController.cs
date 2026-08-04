using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowTileEscape
{
    public sealed class GameplayController : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] LevelDefinition definition;

        [Header("Board")]
        [SerializeField] RectTransform boardRoot;
        [SerializeField] Image[] cellViews;
        [SerializeField] Image[] lightViews;
        [SerializeField] RectTransform playerView;
        [SerializeField] RectTransform exitView;
        [SerializeField] RectTransform lampView;
        [SerializeField] RectTransform shardView;
        [SerializeField] float cellSize = 96f;

        [Header("HUD")]
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text moveLabel;
        [SerializeField] TMP_Text statusLabel;
        [SerializeField] TMP_Text lampDirectionLabel;
        [SerializeField] GameObject failurePanel;
        [SerializeField] GameObject victoryPanel;
        [SerializeField] Button undoButton;

        readonly LightSolver lightSolver = new LightSolver();
        TurnEngine engine;
        LevelState initialState;
        SaveGameService saveService;
        bool completionSaved;

        public LevelDefinition Definition
        {
            set => definition = value;
        }

        public LevelState CurrentState => engine?.State;

        static readonly Color ShadowA = new Color32(21, 26, 58, 255);
        static readonly Color ShadowB = new Color32(31, 37, 78, 255);
        static readonly Color LitGold = new Color32(242, 184, 75, 155);

        void Awake()
        {
            initialState = definition.CreateState();
            engine = new TurnEngine(initialState);
            saveService = SaveGameService.ForCurrentUser();
            levelLabel.text = $"LEVEL {definition.levelNumber:00}  ·  {definition.displayName}";
            Refresh();
        }

        void Update()
        {
            if (engine == null || engine.State.failed || engine.State.completed || Keyboard.current == null) return;
            var keyboard = Keyboard.current;
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) MoveNorth();
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) MoveEast();
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) MoveSouth();
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) MoveWest();
            else if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) Interact();
            else if (keyboard.zKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame) Undo();
            else if (keyboard.rKey.wasPressedThisFrame) Restart();
            else if (keyboard.escapeKey.wasPressedThisFrame) BackToMenu();
        }

        public void MoveNorth() => Submit(PlayerCommand.Move(Direction.North));
        public void MoveEast() => Submit(PlayerCommand.Move(Direction.East));
        public void MoveSouth() => Submit(PlayerCommand.Move(Direction.South));
        public void MoveWest() => Submit(PlayerCommand.Move(Direction.West));
        public void Interact() => Submit(PlayerCommand.Interact());

        public void Undo()
        {
            if (!engine.Undo()) return;
            statusLabel.text = "Turn rewound.";
            Refresh();
        }

        public void Restart()
        {
            engine.Restart(initialState);
            completionSaved = false;
            statusLabel.text = "Level restarted.";
            Refresh();
        }

        public void BackToMenu() => SceneManager.LoadScene("MainMenu");

        void Submit(PlayerCommand command)
        {
            var result = engine.TryExecute(command);
            if (!result.Accepted)
            {
                statusLabel.text = "That path is blocked.";
                return;
            }
            statusLabel.text = result.outcome == TurnOutcome.Failed
                ? result.reason + "  Undo or restart."
                : result.outcome == TurnOutcome.Completed
                    ? "The shadow path is open. Level complete!"
                    : "Stay in shadow. Gold tiles are danger.";
            if (result.outcome == TurnOutcome.Completed && !completionSaved)
            {
                var save = saveService.Load();
                ProgressionRules.Complete(save, definition.levelNumber, engine.State.moveCount, definition.par, engine.State.ShardsCollected);
                saveService.Save(save);
                completionSaved = true;
            }
            Refresh();
        }

        void Refresh()
        {
            var state = engine.State;
            var lit = lightSolver.Solve(state);
            for (var y = 0; y < state.height; y++)
            for (var x = 0; x < state.width; x++)
            {
                var index = y * state.width + x;
                cellViews[index].color = ((x + y) & 1) == 0 ? ShadowA : ShadowB;
                lightViews[index].gameObject.SetActive(lit[index] > 0);
                lightViews[index].color = LitGold;
            }

            Position(playerView, state.player);
            Position(exitView, state.exit);
            playerView.localEulerAngles = new Vector3(0, 0, FacingAngle(state.playerFacing));

            if (state.lights.Length > 0)
            {
                lampView.gameObject.SetActive(true);
                Position(lampView, state.lights[0].position);
                lampView.localEulerAngles = new Vector3(0, 0, FacingAngle(state.lights[0].direction));
                lampDirectionLabel.text = $"LAMP  {DirectionGlyph(state.lights[0].direction)}";
            }

            if (state.shards.Length > 0)
            {
                shardView.gameObject.SetActive(!state.collectedShards[0]);
                Position(shardView, state.shards[0]);
            }

            moveLabel.text = $"MOVES  {state.moveCount} / PAR {definition.par}   ·   SHARD {state.ShardsCollected}/{definition.requiredShards}";
            failurePanel.SetActive(state.failed);
            victoryPanel.SetActive(state.completed);
            undoButton.interactable = engine.HistoryCount > 0;
        }

        void Position(RectTransform view, GridCoord coord)
        {
            var left = -(definition.width - 1) * cellSize * 0.5f;
            var bottom = -(definition.height - 1) * cellSize * 0.5f;
            view.anchoredPosition = new Vector2(left + coord.x * cellSize, bottom + coord.y * cellSize);
        }

        static float FacingAngle(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return 90;
                case Direction.West: return 180;
                case Direction.South: return 270;
                default: return 0;
            }
        }

        static string DirectionGlyph(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return "NORTH";
                case Direction.East: return "EAST";
                case Direction.South: return "SOUTH";
                default: return "WEST";
            }
        }
    }
}
