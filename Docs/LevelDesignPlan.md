# Level Design Plan

Status: All 15 serialized level definitions and scenes are built. Every exact solution below completed through the deterministic engine in the 43/43 EditMode run.

All levels use a 7×5 grid, Noor starts at `(0,0)`, the exit is `(6,4)`, and coordinates are zero-based. `N/E/S/W` move; `I` interacts with the object in front of Noor.

| Level | Chapter | Title / focus | Par | Required objectives | Verified solution |
|---:|---|---|---:|---|---|
| 1 | Silent Halls | The Quiet Lantern — box, lamp, mirror, shard | 15 | box 1, lamp 1, mirror 1, shard 1 | `N,E,I,N,E,I,N,N,E,E,E,S,I,N,E` |
| 2 | Silent Halls | Golden Threshold — fixed beam and shard | 10 | shard 1 | `N,N,N,N,E,E,E,E,E,E` |
| 3 | Silent Halls | Turn the Night — first lamp | 11 | lamp 1 | `N,N,N,E,E,I,N,E,E,E,E` |
| 4 | Silent Halls | Lantern Order — constrained two-lamp order | 16 | lamp 2 | `N,N,N,N,E,E,S,I,N,E,E,S,I,N,E,E` |
| 5 | Silent Halls | The Patient Blocker — box plus lamp | 12 | box 1, lamp 1 | `N,E,I,N,E,I,N,N,E,E,E,E` |
| 6 | Reflections | First Reflection — reflected beam | 13 | mirror 1 | `E,E,E,E,E,N,N,N,W,I,N,E,E` |
| 7 | Reflections | Mosaic Misdirection — two mirrors | 16 | mirror 2 | `E,E,E,E,N,N,W,I,S,E,N,I,E,N,N,E` |
| 8 | Reflections | Silken Eclipse — curtain timing | 14 | curtain 2 | `E,E,E,N,I,I,S,E,E,E,N,N,N,N` |
| 9 | Reflections | Glass and Velvet — combined static mechanics | 20 | box/lamp/curtain/mirror/shard 1 each | `N,E,I,N,E,I,S,S,E,E,N,I,S,E,N,I,E,N,N,N` |
| 10 | Reflections | The Lantern Guard — patrol timing | 10 | reach exit | `N,N,N,N,E,E,E,E,E,E` |
| 11 | Living Light | Crossing Patrols — multiple guards | 10 | reach exit | `N,N,N,N,E,E,E,E,E,E` |
| 12 | Living Light | Moon Window — moving light cycle | 10 | reach exit | `N,N,N,N,E,E,E,E,E,E` |
| 13 | Living Light | Guarded Geometry — guard, box, mirror | 14 | box 1, mirror 1 | `N,E,I,N,N,N,E,E,E,E,S,I,N,E` |
| 14 | Living Light | Three Moon Shards — multi-stage objective | 10 | shard 3 | `N,N,N,N,E,E,E,E,E,E` |
| 15 | Living Light | The Last Shadow — all systems finale | 20 | box/lamp/curtain/mirror 1 each, shard 2 | `N,E,I,N,E,I,S,S,E,E,N,I,S,E,N,I,E,N,N,N` |

## Level 1 vertical-slice gate

- Lamp `(3,2)` initially east, mirror `(5,2)`, box `(2,1)`, shard `(5,4)`.
- Exact 15-turn solution above completes with all four requirements and three stars.
- Verified exposure route: `N,N,N,E,E,E,E,S` fails at `(4,2)`; Undo restores `(4,3)` at move 7.
- Pause blocks commands and resumes cleanly. Failure retains Undo/Restart/Level Select/Main Menu; victory exposes Replay/Next/Level Select/Main Menu.
- Evidence: `TestResults/playmode-results.xml`, `Assets/Screenshots/VerticalSlice_Level01_Failure.png`, `Assets/Screenshots/VerticalSlice_Level01_Completed.png`, and device captures under `Assets/Screenshots/Device_*`.

## Validation policy

- `LevelSolutionTests.SerializedSolutionCompletesLevel` loads each `LevelDefinition` through `AssetDatabase`, parses its exact sequence, and requires a completed terminal state.
- Invalid input must not increment moves or history. Accepted failure remains undoable.
- Logical light, guard intentions, moonlight movement, objectives, and terminal safety are deterministic and independent of scene presentation.
