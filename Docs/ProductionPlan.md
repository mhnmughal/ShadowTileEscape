# Production Plan

Current phase: **Phase 3 — Playable vertical slice**

## Discovery findings

- Unity project root: `/Users/mehranmughal/Documents/Game Development/ShadowTileEscape`
- Git: `BLOCKED` — no `.git` worktree metadata exists at or above the project root.
- Existing project: URP 2D starter with one `Assets/Scenes/SampleScene.unity`.
- Unity 6000.3.19f1 is open on the exact project; MCP reports PID 3569, active clean `SampleScene`, URP, Linear color, and StandaloneOSX target.
- AndroidPlayer/SDK/NDK/OpenJDK and iOSSupport are installed; Android platforms 34–36 and Xcode 26.6 are available.

## Phase checklist

- [x] Phase 0: discovery, baseline open/compile/Console evidence
- [x] Phase 1: AnkleBreaker plugin/server/configuration and exact-project MCP health gate
- [x] Phase 2: architecture, GDD, visual direction, and parallel audits
- [ ] Phase 3: validated vertical slice
- [ ] Phase 4: complete gameplay systems
- [ ] Phase 5: complete UI, save, audio, and presentation
- [ ] Phase 6: 15 handcrafted validated levels
- [ ] Phase 7: automated tests and full manual QA
- [ ] Phase 8: mobile builds, profiling, and release audit
- [ ] Phase 9: final evidence and documentation

## Dependencies

- Compatible installed Unity Editor and platform modules
- Node.js/npm meeting AnkleBreaker server requirements
- AnkleBreaker Unity plugin and Node server
- Legitimately sourced Nunito Sans and license

## Risks and blockers

- Root Git was initialized after preserving the cloned dependencies' nested metadata outside the project; the first phase commit is pending final status review.
- Root Git is absent; the two cloned upstream tool/package repositories contain nested Git metadata that must be handled before a safe first root commit.
- Mobile orientation, identifiers, safe-area behavior, icons, scene order, input actions, and quality remain starter defaults.
- Mobile build-module availability is unknown.

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

## Remaining work

All implementation and validation work from Phase 0 onward remains.
