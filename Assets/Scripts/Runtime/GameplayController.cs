using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
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
        [SerializeField] RectTransform[] lampViews;
        [SerializeField] RectTransform[] mirrorViews;
        [SerializeField] RectTransform[] boxViews;
        [SerializeField] RectTransform[] curtainViews;
        [SerializeField] RectTransform[] guardViews;
        [SerializeField] RectTransform[] guardPreviewViews;
        [SerializeField] RectTransform[] movingLightViews;
        [SerializeField] RectTransform[] shardViews;
        [SerializeField] float cellSize = 96f;

        [Header("HUD")]
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text objectiveLabel;
        [SerializeField] TMP_Text moveLabel;
        [SerializeField] TMP_Text statusLabel;
        [SerializeField] TMP_Text lampDirectionLabel;
        [SerializeField] TMP_Text failureTitleLabel;
        [SerializeField] TMP_Text failureReasonLabel;
        [SerializeField] TMP_Text victoryStatsLabel;
        [SerializeField] GameObject failurePanel;
        [SerializeField] GameObject victoryPanel;
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject hintPanel;
        [SerializeField] Button undoButton;
        [SerializeField] CanvasGroup gameplayContent;
        [SerializeField] SettingsController settingsController;

        [Header("Feedback")]
        [SerializeField] RectTransform turnPulseView;
        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioSource ambienceSource;
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] AudioClip moveClip;
        [SerializeField] AudioClip interactClip;
        [SerializeField] AudioClip undoClip;
        [SerializeField] AudioClip failureClip;
        [SerializeField] AudioClip victoryClip;
        [SerializeField] float presentationDuration = 0.08f;

        readonly LightSolver lightSolver = new LightSolver();
        TurnEngine engine;
        LevelState initialState;
        SaveGameService saveService;
        bool completionSaved;
        bool paused;
        bool inputLocked;
        bool hapticsEnabled;
        bool reducedFlashing;

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
            var sessionSave = saveService.Load();
            hapticsEnabled = sessionSave.settings.haptics;
            reducedFlashing = sessionSave.settings.reducedFlashing;
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MusicVolume", LinearToDecibels(sessionSave.settings.musicVolume));
                audioMixer.SetFloat("SFXVolume", LinearToDecibels(sessionSave.settings.sfxVolume));
            }
            else
            {
                if (ambienceSource != null) ambienceSource.volume = 0.18f * sessionSave.settings.musicVolume;
                if (sfxSource != null) sfxSource.volume = sessionSave.settings.sfxVolume;
            }
            sessionSave.lastPlayedLevel = definition.levelNumber;
            if (!saveService.HasUnsupportedSave) saveService.Save(sessionSave);
            levelLabel.text = $"LEVEL {definition.levelNumber:00}  ·  {definition.displayName}";
            objectiveLabel.text = definition.objectiveText;
            statusLabel.text = definition.hintText;
            hintPanel.SetActive(false);
            Refresh();
        }

        void Update()
        {
            if (engine == null || Keyboard.current == null) return;
            var keyboard = Keyboard.current;
            if (settingsController != null && settingsController.IsOpen) return;
            if (hintPanel.activeSelf)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) HideHint();
                return;
            }
            if (keyboard.escapeKey.wasPressedThisFrame) { TogglePause(); return; }
            if (keyboard.zKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame) { Undo(); return; }
            if (keyboard.rKey.wasPressedThisFrame) { Restart(); return; }
            if (paused || engine.State.failed || engine.State.completed) return;
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) MoveNorth();
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) MoveEast();
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) MoveSouth();
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) MoveWest();
            else if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) Interact();
        }

        public void MoveNorth() => Submit(PlayerCommand.Move(Direction.North));
        public void MoveEast() => Submit(PlayerCommand.Move(Direction.East));
        public void MoveSouth() => Submit(PlayerCommand.Move(Direction.South));
        public void MoveWest() => Submit(PlayerCommand.Move(Direction.West));
        public void Interact() => Submit(PlayerCommand.Interact());

        public void Undo()
        {
            if (paused || inputLocked || hintPanel.activeSelf) return;
            if (!engine.Undo()) return;
            statusLabel.text = "Turn rewound.";
            Refresh();
            PlayFeedback(undoClip);
            StartCoroutine(PresentTurn());
        }

        public void Restart()
        {
            if (inputLocked) return;
            engine.Restart(initialState);
            completionSaved = false;
            SetPaused(false);
            statusLabel.text = "Level restarted.";
            hintPanel.SetActive(false);
            Refresh();
            PlayFeedback(undoClip);
        }

        public void BackToMenu() => SceneManager.LoadScene("MainMenu");
        public void OpenLevelSelect() => SceneManager.LoadScene("LevelSelect");
        public void NextLevel()
        {
            if (definition.levelNumber >= 15) SceneManager.LoadScene("Completion");
            else SceneManager.LoadScene($"Level_{definition.levelNumber + 1:00}");
        }

        public void TogglePause()
        {
            if (engine == null || engine.State.failed || engine.State.completed) return;
            SetPaused(!paused);
        }

        public void Resume() => SetPaused(false);
        public void ShowHint()
        {
            if (paused || engine.State.failed || engine.State.completed) return;
            hintPanel.SetActive(true);
            SetContentInteractive(false);
        }
        public void HideHint() { hintPanel.SetActive(false); SetContentInteractive(true); }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && engine != null && !engine.State.failed && !engine.State.completed) SetPaused(true);
        }

        void OnApplicationPause(bool isPaused)
        {
            if (isPaused && engine != null && !engine.State.failed && !engine.State.completed) SetPaused(true);
        }

        void Submit(PlayerCommand command)
        {
            if (paused || inputLocked || hintPanel.activeSelf) return;
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
                if (!saveService.HasUnsupportedSave) saveService.Save(save);
                completionSaved = true;
            }
            if (result.outcome == TurnOutcome.Failed)
            {
                failureTitleLabel.text = result.reason.IndexOf("guard", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "THE WATCH CLOSES IN"
                    : "CAUGHT IN THE LIGHT";
                failureReasonLabel.text = result.reason + "  Rewind the last turn or try the route again.";
            }
            else if (result.outcome == TurnOutcome.Completed)
            {
                var stars = ProgressionRules.StarsFor(engine.State.moveCount, definition.par);
                victoryStatsLabel.text = $"STARS  {stars}/3\nMOVES  {engine.State.moveCount}  ·  PAR  {definition.par}\nSHARDS  {engine.State.ShardsCollected}/{definition.requiredShards}";
            }
            Refresh();
            PlayFeedback(result.outcome == TurnOutcome.Failed ? failureClip
                : result.outcome == TurnOutcome.Completed ? victoryClip
                : command.type == CommandType.Interact ? interactClip : moveClip);
            if (result.outcome == TurnOutcome.Failed || result.outcome == TurnOutcome.Completed) TryHaptic();
            StartCoroutine(PresentTurn());
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
            if (turnPulseView != null) Position(turnPulseView, state.player);
            Position(exitView, state.exit);
            playerView.localEulerAngles = new Vector3(0, 0, FacingAngle(state.playerFacing));

            for (var i = 0; i < lampViews.Length; i++)
            {
                var active = i < state.lights.Length;
                lampViews[i].gameObject.SetActive(active);
                if (!active) continue;
                Position(lampViews[i], state.lights[i].position);
                lampViews[i].localEulerAngles = new Vector3(0, 0, FacingAngle(state.lights[i].direction));
            }
            for (var i = 0; i < mirrorViews.Length; i++)
            {
                var active = i < state.mirrors.Length;
                mirrorViews[i].gameObject.SetActive(active);
                if (!active) continue;
                Position(mirrorViews[i], state.mirrors[i].position);
                mirrorViews[i].localEulerAngles = new Vector3(0, 0, state.mirrors[i].kind == MirrorKind.Slash ? 45 : -45);
            }
            RefreshPositionPool(boxViews, state.boxes);
            for (var i = 0; i < curtainViews.Length; i++)
            {
                var active = i < state.curtains.Length;
                curtainViews[i].gameObject.SetActive(active);
                if (!active) continue;
                Position(curtainViews[i], state.curtains[i].position);
                var image = curtainViews[i].GetComponent<Image>();
                image.color = state.curtains[i].open ? new Color32(154, 120, 212, 80) : new Color32(154, 120, 212, 255);
            }
            var preview = engine.PreviewGuardPositions();
            for (var i = 0; i < guardViews.Length; i++)
            {
                var active = i < state.guards.Length;
                guardViews[i].gameObject.SetActive(active);
                guardPreviewViews[i].gameObject.SetActive(active && preview[i] != state.guards[i].position);
                if (!active) continue;
                Position(guardViews[i], state.guards[i].position);
                guardViews[i].localEulerAngles = new Vector3(0, 0, FacingAngle(state.guards[i].facing));
                if (guardPreviewViews[i].gameObject.activeSelf) Position(guardPreviewViews[i], preview[i]);
            }
            for (var i = 0; i < movingLightViews.Length; i++)
            {
                var active = i < state.movingLights.Length && state.movingLights[i].active && state.movingLights[i].path.Length > 0;
                movingLightViews[i].gameObject.SetActive(active);
                if (active) Position(movingLightViews[i], state.movingLights[i].Position);
            }
            for (var i = 0; i < shardViews.Length; i++)
            {
                var active = i < state.shards.Length && !state.collectedShards[i];
                shardViews[i].gameObject.SetActive(active);
                if (active) Position(shardViews[i], state.shards[i]);
            }

            lampDirectionLabel.text = state.lights.Length > 0
                ? $"LAMP  {DirectionGlyph(state.lights[0].direction)}"
                : definition.chapterName.ToUpperInvariant();

            var shardText = definition.requiredShards > 0 ? $"  ·  SHARDS {state.ShardsCollected}/{definition.requiredShards}" : string.Empty;
            var actionTotal = definition.requiredLampRotations + definition.requiredMirrorRotations
                + definition.requiredBoxPushes + definition.requiredCurtainToggles;
            var actionText = actionTotal > 0 ? $"  ·  ACTIONS {ObjectiveActions(state)}" : string.Empty;
            moveLabel.text = $"MOVES {state.moveCount}  ·  PAR {definition.par}{shardText}{actionText}";
            failurePanel.SetActive(state.failed);
            victoryPanel.SetActive(state.completed);
            undoButton.interactable = engine.HistoryCount > 0;
            SetContentInteractive(!state.failed && !state.completed && !paused && !hintPanel.activeSelf);
        }

        void SetPaused(bool value)
        {
            paused = value;
            if (pausePanel != null) pausePanel.SetActive(value);
            SetContentInteractive(!value);
            if (value && statusLabel != null) statusLabel.text = "Paused. The palace waits.";
            else if (!value && statusLabel != null && engine != null && !engine.State.failed && !engine.State.completed)
                statusLabel.text = definition.hintText;
        }

        void PlayFeedback(AudioClip clip)
        {
            if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
        }

        IEnumerator PresentTurn()
        {
            inputLocked = true;
            if (reducedFlashing)
            {
                yield return null;
                inputLocked = false;
                yield break;
            }
            if (turnPulseView != null) turnPulseView.gameObject.SetActive(true);
            var elapsed = 0f;
            while (elapsed < presentationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var phase = presentationDuration <= 0 ? 1 : elapsed / presentationDuration;
                if (turnPulseView != null) turnPulseView.localScale = Vector3.one * Mathf.Lerp(0.75f, 1.35f, phase);
                yield return null;
            }
            if (turnPulseView != null)
            {
                turnPulseView.localScale = Vector3.one;
                turnPulseView.gameObject.SetActive(false);
            }
            inputLocked = false;
        }

        void TryHaptic()
        {
            if (hapticsEnabled && Application.isMobilePlatform) Handheld.Vibrate();
        }

        static float LinearToDecibels(float value) => value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;

        void SetContentInteractive(bool value)
        {
            if (gameplayContent == null) return;
            gameplayContent.interactable = value;
            gameplayContent.blocksRaycasts = value;
        }

        void RefreshPositionPool(RectTransform[] views, GridCoord[] positions)
        {
            for (var i = 0; i < views.Length; i++)
            {
                var active = i < positions.Length;
                views[i].gameObject.SetActive(active);
                if (active) Position(views[i], positions[i]);
            }
        }

        string ObjectiveActions(LevelState state)
        {
            var done = Math.Min(state.lampRotations, definition.requiredLampRotations)
                + Math.Min(state.mirrorRotations, definition.requiredMirrorRotations)
                + Math.Min(state.boxPushes, definition.requiredBoxPushes)
                + Math.Min(state.curtainToggles, definition.requiredCurtainToggles);
            var total = definition.requiredLampRotations + definition.requiredMirrorRotations
                + definition.requiredBoxPushes + definition.requiredCurtainToggles;
            return $"{done}/{total}";
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
