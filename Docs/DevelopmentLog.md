# Development Log

## 2026-08-04 — Phase 0 start

- Work completed: read the 2,316-line production prompt; read governing coding skill; identified the Unity root; attempted Git status; inventoried top-level project content; created governing and required evidence documents.
- Files changed: `AGENTS.md`, required `Docs/*.md`, and `ThirdPartyNotices.md`.
- Tests run: Git worktree check (`BLOCKED`: not a Git repository); gameplay/Unity tests `NOT RUN`.
- Screenshots: none.
- Bugs discovered: none yet; source project is an unconfigured 2D URP starter.
- Bugs fixed: none.
- Commit hash: unavailable because the project has no Git worktree.
- Remaining issues: finish Phase 0 discovery and baseline; install/verify MCP; all production phases.

## 2026-08-04 — Phases 0 and 1

- Work completed: inventoried Unity/packages/settings/platform modules; installed embedded AnkleBreaker UPM plugin; cloned/installed project-local Node server; created verified project-scoped Codex MCP configuration; completed exact-project read and temporary CRUD/screenshot/cleanup health checks.
- Files changed: `Packages/manifest.json`, `Packages/packages-lock.json`, `Packages/com.anklebreaker.unity-mcp/**`, `Tools/unity-mcp-server/**`, `.codex/config.toml`, `.gitignore`, documentation, `Assets/Screenshots/MCP_Phase1_Validation.png` and its Unity metadata.
- Tests run: Unity compilation PASS (0 errors); MCP health matrix PASS; server unit subset 18 PASS; upstream protocol file FAIL/cancelled after hang.
- Screenshot: `Assets/Screenshots/MCP_Phase1_Validation.png`, visually inspected.
- Bugs discovered/fixed: manifest did not hot-resolve; fixed using embedded UPM clone. npm home-cache/DNS install failures; fixed using task cache and approved network. Auxiliary protocol test hang remains documented.
- Commit hash: unavailable because the project has no Git worktree.
- Remaining issues: Phase 2 audits/architecture; vertical slice; all production/QA/build phases.

## 2026-08-04 — Phase 2 audits and licensing gate

- Work completed: ran four read-only parallel audits; consolidated deterministic gameplay, UI/safe-area, art/accessibility, save/guard/undo, architecture, licensing, and release decisions. Imported official Nunito Sans/OFL/source metadata, TMP essentials, upright/italic SDF assets, and set Nunito as TMP default. Aligned embedded UPM manifest. Remediated MCP dependencies to zero audit findings.
- Tests run: AnkleBreaker server 60/60 PASS with loopback permission; npm audit 0 vulnerabilities; Unity font/TMP imports and default font verified through MCP.
- Screenshots: no new product screenshot; UI/art still `NOT RUN`.
- Bugs fixed: TMP asset generation null due missing TMP Settings; imported essential resources and retried successfully. Sandbox protocol test EPERM diagnosed and rerun with appropriate permission.
- Source control: root Git initialized; upstream dependency Git metadata preserved at `/private/tmp/shadow-tile-escape-upstream-git-20260804/` so pinned contents are vendored as ordinary files. Phase commit pending.
- Remaining issues: establish safe root Git handling; configure input/mobile settings; implement and validate vertical slice.

## 2026-08-04 — Phase 3 vertical slice

- Work completed: implemented deterministic grid/light/turn/undo/guard/shard/exit model; versioned atomic save with backup recovery and monotonic progression; serialized Boot, Main Menu, and Level 1; landscape IDs/orientation/iOS 15/Android IL2CPP ARM64/build scene order; safe-area Canvas; keyboard and mobile controls; failure/victory overlays; editor baker and QA runner.
- Tests run: compilation PASS (0); EditMode 12/12 PASS; PlayMode 3/3 PASS. The 10-turn `N,N,N,N,E,E,E,E,E,E` solution, exposure route `E×6,N,N`, undo restoration, serialized UI navigation, and save persistence were executed.
- Screenshots: `VerticalSlice_MainMenu.png`, `VerticalSlice_Level01_Initial.png`, `VerticalSlice_Level01_Failure.png`, `VerticalSlice_Level01_Completed.png`; all visually inspected.
- Bugs discovered/fixed: programmatic InputSystem UI default assignment threw during scene bake; fixed by inactive construction and explicit project action references. `LevelDefinition` script/filename mismatch produced an invalid asset reference; moved to `LevelDefinition.cs` and directly assigned/serialized. Missing Nunito arrow glyphs replaced with readable ASCII direction labels. Async failure capture initially landed after Undo; added an explicit pre-undo render delay.
- Commit hash: pending Phase 3 checkpoint.
- Remaining issues: complete mechanics/screens/audio/presentation, levels 2–15, aspect/device matrix, builds/profiling, final documentation.

## 2026-08-04 — Phases 3 gate repair and 4–7 full game

- Work completed: reopened the vertical-slice gate after audit and added box/mirror requirements, pause/focus gating, Level Select, audio/animation feedback, and exact iPhone 8 evidence. Extended the deterministic model with constrained/fixed lamps, mirrors, curtains, boxes, shards, patrol guards/lantern facing, moving moonlight, multi-part objectives, simultaneous collision resolution, shared guard preview, and terminal undo/restart/navigation.
- Content/UI: baked Intro, How To Play, Level Select, Credits, Completion, Settings/reset confirmations, and Levels 01–15. Added safe versioned save behavior, last-played/intro/tutorial fields, future-version overwrite protection, Music/SFX mixer routing, generated ambience/SFX, and app icon/splash branding.
- Tests run: compilation PASS (0); EditMode 43/43 PASS; final PlayMode 5/5 PASS. All 15 exact solutions complete in both the deterministic model and their serialized scenes. PlayMode save data uses a unique temporary directory and never touches player progress.
- Visual evidence: Main Menu, Intro, Level Select, failure, victory, exact 1334×750 active/pause, and 1024×768 active/pause captures inspected.
- Bugs fixed: three-guard contention previously allowed one guard through; collision rejection is now independent and stable. Combined MonoBehaviour source filenames prevented reliable serialization; controllers were split into matching files. Audio mixer groups were initially orphaned; builder now attaches Music/SFX to Master before routing sources. Platform icon assignment now fills every expected legacy slot.
- Source control: full-game checkpoint pending successful platform build evidence.
- Remaining issues: complete Android build, export iOS, profile/release audit, final documentation and commit.

## 2026-08-04 — Phases 8 and 9 release evidence

- Builds: Android Release IL2CPP/ARM64 PASS at `Builds/Android/ShadowTileEscape.apk` with 0 errors/0 warnings. iOS Unity export PASS with 0/0, followed by an unsigned Release iphoneos Xcode build ending `** BUILD SUCCEEDED **`.
- Release fixes: changed icon import to uncompressed 1024px, configured Android adaptive icons, cleared deprecated legacy/round icon slots, disabled analytics submission and engine diagnostics, and retained crash/cloud diagnostics disabled.
- Profile: Level 1 Play Mode measured 7 draws, 465 triangles, 16.512–16.927 ms frames (59.1–60.6 FPS), and 0 B sampled GC; analyzer suggestions: none.
- Final regression: compilation 0 errors; EditMode 43/43 PASS; PlayMode 5/5 PASS; 22 enabled build scenes with no missing assets; missing-reference scan 0 scene/0 assets; Console clear confirmed.
- External release inputs: Apple/Android store signing credentials and physical-device thermal/haptic soak only.
