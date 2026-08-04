# QA Report

Status: Full deterministic and scene-flow suites pass; mobile build/profile work is in progress.

| Area | Status | Evidence |
|---|---|---|
| Baseline compilation | PASS | MCP compilation buffer: count 0, `isCompiling:false` after plugin import |
| Unity Console | PASS | After clear: only bridge registration/start logs, no errors or warnings in MCP buffer |
| Edit Mode tests | PASS | 43/43, `TestResults/editmode-results.xml`, 1.295 s |
| Play Mode tests | PASS | 5/5, `TestResults/playmode-results.xml`, 31.247 s |
| Scene/manual flows | PASS | Boot → Main Menu → Level Select → Level 1; touch input, 15-turn solution, failure/undo, pause/resume, victory/save |
| 15 level completions | PASS (automated) | Every serialized `verifiedSolution` executed through `TurnEngine` and completed |
| Aspect ratios/safe areas | PASS | Exact 1334×750 and 1024×768 captures inspected; `SafeAreaFitter` serialized in all canvases |
| Runtime-creation audit | PASS | Gameplay content, visual pools, UI, effects, audio sources, and scenes are editor-baked; no runtime level/UI generation |
| Performance | PASS (Editor profile) | 16.512–16.927 ms, 59.1–60.6 FPS, 7 draws, 465 triangles, 0 B sampled GC; physical-device soak remains release check |
| App icon/splash | PASS | 1024px project icon imported and assigned; branded splash configured |

## Phase 1 MCP validation

- Exact project ping: PASS.
- Scene/GameObject CRUD: PASS.
- Screenshot capture and visual inspection: PASS (`Assets/Screenshots/MCP_Phase1_Validation.png`).
- Temporary content cleanup: PASS; scene moved to OS Trash (recoverable), zero matching residue.
- Auxiliary upstream Node tests: PASS — initial sandbox run was blocked from loopback binding; approved rerun completed 60/60 with zero failures/cancellations. Final npm audit: 0 vulnerabilities.

Failures, screenshot paths, Console evidence, per-scene results, per-level results, reviewed forbidden-call occurrences, and remaining issues will be recorded here.

## Full-game evidence

- Compilation: PASS, MCP compilation buffer count 0 after the final imports.
- EditMode: PASS 43/43 — 15 serialized level solutions plus range/source/blocker/overlap rules, all eight mirror cases, invalid no-op, failed-turn undo, lamp/box/curtain interactions, two- and three-guard contention, head-on swap, shared preview resolution, moving moonlight, guard-facing safety, objective gates, save round-trip/backup/default/future-version safety, monotonic progression, and Level 15 cap.
- PlayMode: PASS 5/5 — real Boot/Menu/Level Select buttons, locked/open level state, serialized mobile movement, exact Level 1 solution, pause gate, failure, retained snapshot undo, victory/save, all 15 serialized scene/controller solutions, and Level 15 Finale → Completion. Saves use isolated temporary directories.
- Initial board: `Assets/Screenshots/VerticalSlice_Level01_Initial.png`.
- Main menu: `Assets/Screenshots/VerticalSlice_MainMenu.png`.
- Failure modal before undo: `Assets/Screenshots/VerticalSlice_Level01_Failure.png`.
- Victory modal: `Assets/Screenshots/VerticalSlice_Level01_Completed.png`.
- Level Select: `Assets/Screenshots/FullGame_LevelSelect.png`.
- Intro: `Assets/Screenshots/FullGame_Intro.png`.
- iPhone 8 landscape active gameplay: `Assets/Screenshots/Device_iPhone8_Landscape_Level01_Active.png` (1334×750).
- Tablet 4:3 active gameplay: `Assets/Screenshots/Device_Tablet_4x3_Level01_Active.png` (1024×768).
- Captures were visually inspected. No exception/error remained after the final full-game bake.

## Release verification

- Build Settings: PASS — 22 enabled scenes, Boot first, Completion last, zero missing scene assets.
- Missing references: PASS — zero in current scene and zero across assets.
- Android: PASS — release APK, 0 errors/0 warnings.
- iOS Unity export: PASS — 0 errors/0 warnings.
- iOS unsigned Xcode device build: PASS — `** BUILD SUCCEEDED **`; generated Unity/Xcode deprecation/linker warnings only.
- Privacy defaults: PASS — analytics submission, engine diagnostics, cloud diagnostics reporting, and crash-report API disabled.
