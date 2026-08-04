# QA Report

Status: **PASS** for the serialized game, commercial UI regression, automated suites, screenshot matrix, and unsigned mobile builds. Store signing and physical-device thermal/haptic testing remain release-operator checks.

## Final gate — 2026-08-04

| Area | Status | Evidence |
|---|---|---|
| Exact Unity project/MCP | PASS | Unity 6000.3.19f1, project path exact, AnkleBreaker port 7890, plugin 2.39.5 |
| Compilation | PASS | CompilationPipeline returned 0 errors and 0 warnings after final bake and tests |
| Unity Console | PASS | 0 errors and 0 warnings after final test run; final Android/iOS Unity builds each reported 0/0 |
| Edit Mode tests | PASS | 43/43, 0 failed/skipped, `TestResults/editmode-results.xml`, end `2026-08-04 13:10:48Z` |
| Play Mode tests | PASS | 10/10, 0 failed/skipped, `TestResults/playmode-results.xml`, end `2026-08-04 13:12:16Z` |
| Missing references | PASS | 0 in active scene; 0 across assets |
| Serialized scenes | PASS | 22 enabled scenes; Boot first, Completion last; all 15 level scenes load and solve |
| UI flows | PASS | Main Menu, Intro, How to Play, Level Select, Settings, Credits, gameplay, pause, hint, failure, victory, Completion |
| Settings/persistence | PASS | Music/SFX mixer values and save persistence, haptics, reduced flashing, tutorial reset, confirmed progress reset preserving settings |
| Modal/input gates | PASS | New Game, reset, hint, pause/settings, failure, and victory block base interaction and return one layer at a time |
| Responsive/safe area | PASS | Seven exact landscape screenshots inspected; no clipping or panel overlap at phone, wide-phone, or tablet ratios |
| Runtime-content audit | PASS | No production-runtime matches for forbidden GameObject creation, instantiation, AddComponent, Resources/Addressables load, or scene-wide Find APIs |
| Android build | PASS | Release IL2CPP/ARM64 APK, 24.01 s cached rebuild, 0 errors/0 warnings |
| iOS Unity export | PASS | Release ARM64/iOS 15 Xcode export, 45.15 s, 0 errors/0 warnings |
| iOS native compile | PASS | Signing disabled, iphoneos 26.5; `** BUILD SUCCEEDED **` |

## Automated coverage

Edit Mode covers all 15 verified solutions; inclusive light range and source semantics; blockers, overlapping rays, and all mirror directions; invalid no-op behavior; failed-turn undo; lamp, mirror, box, and curtain interactions; two- and three-guard contention and swaps; shared guard preview; moving moonlight; objective gates; save round-trip, backup/default/future-version safety; monotonic progression; and the Level 15 cap.

Play Mode covers the real serialized Boot/Menu/Level Select flow, Continue/New Game state and confirmation, Credits relocation, all 15 scene/controller solutions, mobile controls, hint and pause/settings gates, failure/undo, victory/save, mixer-backed sliders, haptics/reduced-flashing state, tutorial/progress resets, rich level metadata, Completion totals, and Finale → Completion. Test saves use isolated temporary directories and are deleted afterward.

## Screenshot-based visual QA

Exact final responsive Main Menu captures:

- `Assets/Screenshots/UIRedesign_MainMenu_1334x750.png`
- `Assets/Screenshots/UIRedesign_MainMenu_1920x1080.png`
- `Assets/Screenshots/UIRedesign_MainMenu_2436x1125.png`
- `Assets/Screenshots/UIRedesign_MainMenu_2340x1080.png`
- `Assets/Screenshots/UIRedesign_MainMenu_2532x1170.png`
- `Assets/Screenshots/UIRedesign_MainMenu_2796x1290.png`
- `Assets/Screenshots/UIRedesign_MainMenu_2732x2048.png`

Final screen/state captures:

- `Assets/Screenshots/UIRedesign_Boot.png`
- `Assets/Screenshots/UIRedesign_Settings.png`
- `Assets/Screenshots/UIRedesign_CreditsAndLicenses.png`
- `Assets/Screenshots/UIRedesign_Intro.png`
- `Assets/Screenshots/UIRedesign_HowToPlay.png`
- `Assets/Screenshots/UIRedesign_LevelSelect.png`
- `Assets/Screenshots/UIRedesign_GameplayHUD.png`
- `Assets/Screenshots/UIRedesign_Pause.png`
- `Assets/Screenshots/UIRedesign_Failure.png`
- `Assets/Screenshots/UIRedesign_Victory.png`
- `Assets/Screenshots/UIRedesign_Completion.png`

Each capture was inspected for hierarchy, typography, alignment, spacing, contrast, touch-target sizing, safe edges, clipping, stretching, empty space, background balance, and default-Unity appearance. Defects found and fixed during the loop included tablet menu clipping, a low D-pad edge, crowded gameplay actions, absent directional glyphs, credits scroll position/body overlap, settings slider fill sizing, and stale screenshot Sprite importer rectangles.

## Functional UI audit

- Main Menu: dynamic Continue and summary, confirmed New Game, Level Select, How to Play, Settings; no standalone Credits button.
- Settings: Music/SFX percentages and exposed mixer parameters, haptics, reduced flashing, tutorial reset, confirmed progress reset, nested Credits & Licenses, Done/back.
- Level Select: three chapter groups, 15 serialized buttons, locked/current/open/completed presentation, stars/shard/best-move metadata.
- Gameplay: persistent level objective, move/par/shard/action counters, D-pad and Interact, Undo/Restart/Hint/Pause, nested pause settings, dynamic failure reason, victory statistics and navigation.
- Completion: aggregate halls/stars/shards plus Replay Finale, Level Select, Main Menu, and Credits & Licenses.

## Phase 1 MCP validation

Exact-project ping, scene/object CRUD, screenshot capture, and recoverable cleanup passed. `Assets/Screenshots/MCP_Phase1_Validation.png` was inspected. The development-only server suite passed 60/60 with approved loopback access and `npm audit` reported zero vulnerabilities.

## Remaining limitations

- No physical iPhone/Android thermal soak, haptic feel review, VoiceOver/TalkBack session, or store-signing/archive submission was run because hardware and private signing credentials are external.
- The unsigned iOS compile emits only generated Unity/Xcode script/deprecation/linker warnings; Unity project-code compilation and Unity builds are warning-free.
- The prior Editor performance sample remains 16.512–16.927 ms (59.1–60.6 FPS), 7 draws, 465 triangles, and 0 B sampled GC; it is not a substitute for device profiling.
