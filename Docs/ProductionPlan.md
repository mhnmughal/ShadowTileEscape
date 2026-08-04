# Production Plan

Current phase: **Phase 9 — Final evidence and documentation**

## Discovery findings

- Unity project root: `/Users/mehranmughal/Documents/Game Development/ShadowTileEscape`
- Git: root repository initialized; baseline commit `832ceaf` preserves Phases 0–2.
- Starting point: URP 2D starter with one `Assets/Scenes/SampleScene.unity`; the production build now uses 22 serialized scenes.
- Unity 6000.3.19f1 is open on the exact project; MCP reports PID 3569, active clean `SampleScene`, URP, Linear color, and StandaloneOSX target.
- AndroidPlayer/SDK/NDK/OpenJDK and iOSSupport are installed; Android platforms 34–36 and Xcode 26.6 are available.

## Phase checklist

- [x] Phase 0: discovery, baseline open/compile/Console evidence
- [x] Phase 1: AnkleBreaker plugin/server/configuration and exact-project MCP health gate
- [x] Phase 2: architecture, GDD, visual direction, and parallel audits
- [x] Phase 3: validated vertical slice
- [x] Phase 4: complete gameplay systems
- [x] Phase 5: complete UI, save, audio, and presentation
- [x] Phase 6: 15 handcrafted validated levels
- [x] Phase 7: automated tests and visual QA evidence
- [x] Phase 8: mobile builds, profiling, and release audit
- [x] Phase 9: final evidence and documentation

## Dependencies

- Compatible installed Unity Editor and platform modules
- Node.js/npm meeting AnkleBreaker server requirements
- AnkleBreaker Unity plugin and Node server
- Legitimately sourced Nunito Sans and license

## Risks and blockers

- Root Git is healthy; vendored upstream metadata remains recoverable at `/private/tmp/shadow-tile-escape-upstream-git-20260804/` for this session.
- Physical-device thermal/haptic validation and store signing require release hardware/credentials and are documented release-operator checks.
- Android and iOS modules are installed; release signing credentials remain external release inputs.

## Acceptance gates

- Phase 0: exact Unity version/environment documented; project opens; baseline compilation/Console recorded.
- Phase 1: ping identifies this project; Codex tools can inspect and mutate/delete a temporary scene object; screenshot captured; no residue.
- Phase 3: complete vertical slice passes compile, tests, playthrough, undo/restart/save, and aspect checks.
- Final: all Definition of Done checks have evidence; anything unavailable remains honestly `BLOCKED` or `NOT RUN`.

## Completed milestones

- Master prompt read in full.
- Initial repository root and Git check completed.
- Governing and evidence documents created.
- Phase 0 baseline: project open, exact Unity/project identity verified, zero compilation errors, clean plugin startup Console.
- Phase 1 MCP gate: plugin/server/configuration/read/CRUD/screenshot/cleanup all verified.
- Phase 1 server dependencies remediated to zero `npm audit` findings; upstream suite passes 60/60 with approved loopback access.
- Phase 2 read-only architecture, UI/visual, gameplay/QA, and licensing audits completed and consolidated.
- Official Nunito Sans sources/OFL imported; TMP essential resources and Nunito SDF assets created; TMP default verified.
- Phase 2 gate: four audits consolidated, licensing blockers remediated, zero compilation errors and empty Console after clear.
- Phase 3 gate was reopened after audit, then closed with box, lamp, mirror, pause, audio, Level Select, save, failure/undo, a 15-turn exact solution, and required iPhone 8 landscape evidence.
- Phase 4: deterministic lamps, mirrors, curtains, boxes, patrol guards, moving lights, simultaneous guard resolution/preview, overlap-safe lighting, objective counters, failure, undo, and completion implemented.
- Phase 5: complete screen flow, settings/reset confirmation, versioned safe save, Music/SFX mixer, generated feedback tones, safe-area UI, app icon, splash branding, and terminal/pause navigation implemented.
- Phase 6: 15 serialized definitions/scenes, each with an exact solution executed by automated tests.
- Phase 7 evidence: 43/43 EditMode and 10/10 PlayMode tests pass; every solution completes in both the model and its serialized scene. The commercial UI pass covers every screen plus settings/credits, all modal states, and a seven-resolution landscape matrix from 1334×750 through 2732×2048.
- Phase 8 final rebuild: Android IL2CPP/ARM64 and iOS Unity export both pass with 0 errors/0 warnings; the refreshed unsigned iOS Xcode device build ends `** BUILD SUCCEEDED **`.

## Remaining work

No implementation blocker remains. Optional next release action: provide store signing credentials and run physical-device soak/accessibility review.
