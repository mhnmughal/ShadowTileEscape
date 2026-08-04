# Technical Architecture

Status: Phase 3 core implementation verified; Phases 4–9 remain.

## Verified baseline

- Unity 6000.3.19f1, URP 17.3.0 with Renderer2D, Linear color, Input System 1.19.0, uGUI/TMP 2.0.0, Test Framework 1.6.0.
- Exact-project MCP is healthy on port 7890 with plugin 2.39.5 and server 2.35.6.
- AndroidPlayer/SDK/NDK/OpenJDK and iOSSupport are installed; Android platforms 34–36 and Xcode 26.6 are available.
- Build Settings contain serialized Boot, MainMenu, and Level_01 scenes; deterministic runtime/editor/test assemblies are present.
- Root Git baseline is commit `832ceaf`; upstream dependencies are vendored ordinary files.

## Responsibilities and boundaries

Use the smallest useful assembly split:

- `ShadowTileEscape.Runtime`: pure deterministic model, Unity adapters, flow/save/input/presentation.
- `ShadowTileEscape.Editor`: scene baking, serialized validation, preflight/build utilities.
- `ShadowTileEscape.EditModeTests`: pure model, serialization, validation.
- `ShadowTileEscape.PlayModeTests`: real scene/menu/input/save flows.

Avoid one-interface/one-implementation abstractions and manager-per-mechanic sprawl. Core classes with clear value are `LevelState`, `LightSolver`, `TurnEngine`, `ProgressionRules`, `SaveGameService`, `LevelController`, `LevelBindings`, `LevelPresenter`, `GameplayInputController`, `SceneFlowController`, `AudioController`, and focused view/UI components.

## Serialized authoring and scenes

One `LevelDefinition` ScriptableObject per handcrafted level stores dimensions, cell flags, metadata/par/objective, initial Noor state, and stable indexed configurations for lamps, mirrors, boxes, curtains, guards, moonlight, shards, and exit.

Every level scene still contains all required grid geometry, actors, interactables, beam segments, effects, managers/adapters, camera, AudioSources, Canvas/UI/modal roots, transition overlay, and EventSystem before Play Mode. `LevelBindings` maps stable definition indices to scene views. Editor validation rejects missing, duplicate, out-of-grid, and mismatched bindings.

An Editor-only scene baker may place common hierarchy/prefab instances before Play Mode and save them as normal serialized scenes. It is never included in player builds and never generates gameplay content at runtime.

Boot contains the pre-created persistent application services. A serialized `AppContext` ScriptableObject provides explicit cross-scene references; scenes bind to it through the Inspector. No normal dependency path uses scene-wide searches.

## Deterministic domain

Value types: `GridCoord`, `Direction`, `PlayerCommand`, `LevelState`, `TurnSnapshot`, and `TurnResult`. Flat cell arrays use `index = y * width + x`; indexed mechanic arrays preserve stable serialized identity. Physics, transforms, Tilemaps, renderers, colliders, hierarchy order, and animation state are never gameplay authority.

`TurnEngine.TryExecute(command)` is the single keyboard/mobile entry path. It synchronously validates and applies the required turn sequence. Invalid commands return without mutation. Accepted commands capture one pre-turn snapshot before mutation and return a result/delta for presentation. The scene input lock remains active until short animations/audio finish.

Guards resolve simultaneously: compute all intentions from the same pre-guard state, then reject contested destinations and swaps consistently. Preview calls the identical resolver. Serialized list or hierarchy order cannot change outcomes.

## Grid and occupancy

One authoritative occupancy map combines immutable cell flags with current indexed actor/interactable positions. Grid/world conversion uses a serialized origin and cell size, orthogonal integer coordinates, and editor validation. Derived occupancy is rebuilt after reset/undo and updated only on accepted state changes.

## Logical light

`LightSolver` preallocates `litCount[cell]` and `visited[cell * 4 + direction]`. Each active source walks cell-by-cell to the inclusive range limit or grid edge; walls, boxes, closed curtains, and opaque blockers stop propagation. Fixed slash/backslash tables reflect mirrors. Per-source traversal is bounded by `width * height * 4` visited states.

Lighting recalculates only after accepted state-changing commands, guard/moonlight advancement, reset, and undo. Overlap uses counts so removing one source cannot clear cells lit by another.

Presentation reads the committed result and activates/moves pre-created cell-aligned beam segments. Exact logical cells and beam endpoints must match. URP Light2D, bloom, particles, and fades are decorative only.

## Undo and reset

Snapshots contain all mutable values: Noor position/facing; lamp/mirror states; box positions; curtain states; guard positions/patrol indices; moonlight indices; shard flags; exit/objective/terminal state; and move count. Derived occupancy/light/visuals are rebuilt after restore.

An accepted failed turn retains its pre-turn snapshot for Undo Last Turn. Invalid commands add no history. Default history limit is 64 and drops only the oldest snapshot. Restart and transitions clear history. Scene reload is permitted for restart after flow tests prove it.

## Save

One versioned JSON document stores progress and settings. Validate version/ranges, write a temporary file, flush, atomically replace primary, and retain a last-known-good backup. Read order: valid primary, valid backup, defaults. Unsupported newer versions fail safely without overwriting them.

Completion merges stars/shards monotonically, improves best moves only, clamps unlocks to 1–15, and treats Level 15 as final completion. New Game and Reset Progress require confirmation. No `PlayerPrefs`, cloud, encryption, or scattered keys.

## Input, UI, and safe area

Replace starter actions with discrete Up/Down/Left/Right/Interact/Undo/Restart/Pause buttons. Required keyboard bindings: WASD/arrows, E/Space, Z/Backspace, R, Escape. Serialized mobile buttons invoke the same command methods. Held/simultaneous/diagonal inputs submit at most one accepted command.

CanvasScaler uses 1920×1080. `FullBleedRoot` owns background/dimmers; `SafeAreaRoot` anchors to `Screen.safeArea` and updates only on resolution/safe-area change. All interactive UI lives below it. Modals include blockers; turn, modal, transition, focus loss, and suspension gates reject input.

TMP essential resources are installed. The official Nunito Sans variable and italic sources and SDF assets live under `Assets/Fonts/NunitoSans`; TMP's project default is Nunito Sans.

## Performance

- No continuous grid/light solve, open-ended pathfinding, or avoidable idle `Update`.
- Preallocated flat arrays and serialized/pre-created visual pools avoid turn allocations and runtime content creation.
- Limit transparent overdraw/particles; use mobile texture sizes/compression and one deliberate mobile quality profile.
- Keep exact beam presentation independent of half-resolution 2D light textures.
- Profile GC, Canvas rebuilds, light recalculation, scene load, texture/audio memory, and stable 60 FPS on iPhone-8-class hardware.

## Build/release configuration pending

Before vertical-slice mobile checks: landscape-only, safe-area behavior, development identifiers, scene order, frame-rate/quality, privacy submission, and input actions. Android IL2CPP/ARM64 and iOS minimum 15 are acceptable baselines. Signing team and release keystore remain documented release-only placeholders.

## Prohibited production runtime patterns

No required runtime `new GameObject`, `Instantiate`, `AddComponent`, `Resources.Load`, Addressables instantiation, normal scene-wide searches, runtime level generation, or dynamically built UI/effects. Editor-only and test-only occurrences must be excluded and recorded in QA.
