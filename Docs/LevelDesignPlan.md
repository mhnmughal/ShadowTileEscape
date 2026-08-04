# Level Design Plan

Status: Level 1 vertical slice is automated and visually verified. Levels 2–15 remain pending.

| Level | Chapter | Mechanic focus | Scene | Par | Shards | Automated | Manual |
|---:|---|---|---|---:|---:|---|---|
| 1 | Silent Halls | Movement, exact light cells, lamp read, shard/exit, undo | `Assets/Scenes/Levels/Level_01.unity` | 10 | 1 | PASS | PASS via executed PlayMode flow + screenshots |
| 2 | Silent Halls | Lit/safe cells, restart | `Assets/Scenes/Levels/Level_02.unity` | TBD | 1 | NOT RUN | NOT RUN |
| 3 | Silent Halls | Rotatable lamp, interaction | `Assets/Scenes/Levels/Level_03.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 4 | Silent Halls | Two lamps, order dependency, undo | `Assets/Scenes/Levels/Level_04.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 5 | Silent Halls | Box plus lamp | `Assets/Scenes/Levels/Level_05.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 6 | Reflections | One mirror/reflected beam | `Assets/Scenes/Levels/Level_06.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 7 | Reflections | Multiple mirrors/misdirection | `Assets/Scenes/Levels/Level_07.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 8 | Reflections | Curtains/turn timing | `Assets/Scenes/Levels/Level_08.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 9 | Reflections | Box, mirror, curtain | `Assets/Scenes/Levels/Level_09.unity` | TBD | 2 | NOT RUN | NOT RUN |
| 10 | Reflections | Lantern guard/preview | `Assets/Scenes/Levels/Level_10.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 11 | Living Light | Multiple guards/timing | `Assets/Scenes/Levels/Level_11.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 12 | Living Light | Moving moonlight cycle | `Assets/Scenes/Levels/Level_12.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 13 | Living Light | Guard, mirror, box | `Assets/Scenes/Levels/Level_13.unity` | TBD | TBD | NOT RUN | NOT RUN |
| 14 | Living Light | Three shards/multi-stage exit | `Assets/Scenes/Levels/Level_14.unity` | TBD | 3 | NOT RUN | NOT RUN |
| 15 | Living Light | All mechanics/final escape | `Assets/Scenes/Levels/Level_15.unity` | TBD | TBD | NOT RUN | NOT RUN |

Each row will be expanded with grid size, initial configuration, exact intended input sequence, shard coordinates, failure cases, test evidence, manual completion evidence, and screenshot path after its scene exists.

## Level 1 — The Quiet Lantern

- Grid: 7×5; Noor `(0,0)` facing east; exit `(6,4)`; shard `(5,4)`; lamp `(3,2)` facing east, range 3.
- Initial danger cells: `(4,2)`, `(5,2)`, `(6,2)`. Lamp/source cell `(3,2)` is not lit by its own ray.
- Intended 3-star solution: `N,N,N,N,E,E,E,E,E,E` (10 turns), collecting the shard before entering the exit.
- Verified failure: `E,E,E,E,E,E,N,N` enters `(6,2)`, retains the accepted pre-turn snapshot, and Undo restores `(6,1)` at move 7.
- Evidence: `TestResults/playmode-results.xml`, `Assets/Screenshots/VerticalSlice_Level01_Initial.png`, `Assets/Screenshots/VerticalSlice_Level01_Failure.png`, and `Assets/Screenshots/VerticalSlice_Level01_Completed.png`.
