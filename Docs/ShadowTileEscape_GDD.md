# Shadow Tile Escape — Game Design Document

Status: Phase 2 design baseline approved; implementation and play validation remain `NOT RUN`.

## Product

`Light is danger. Shadow is the path.`

Shadow Tile Escape is a landscape 2D, top-down, deterministic turn-based stealth puzzle for iOS and Android, with desktop Editor controls for testing. A complete playthrough spans three chapters and 15 handcrafted levels.

## Story

Noor, a child trapped inside a cursed palace, is hunted by guardians made of living light. Noor survives only in shadow. The player rotates lamps, redirects beams, moves blockers, opens curtains, predicts lantern guards, and times shifting moonlight to build a safe route to the exit. The final level resolves Noor's escape and leaves every completed level replayable.

## Design pillars

1. **Readable danger:** exact cell-aligned gold beams and redundant direction/pattern cues always agree with deterministic logic.
2. **Deliberate turns:** one command produces one fully resolved state; no reflex timing or hidden randomness.
3. **Meaningful manipulation:** light sources and blockers reshape safe routes, rather than acting as decorative switches.
4. **Forgiving experimentation:** multi-step undo, restart, clear preview, and concise tutorials make complex puzzles approachable.
5. **Storybook atmosphere:** original moonlit-palace art, restrained motion, and warm danger against cool shadow create identity without obscuring the board.

## Core loop

1. Read current light, guard direction, next-turn previews, objective, and optional shards.
2. Submit one movement or interaction command.
3. Resolve player action, logical light, safety, guards/moving lights, final safety, collectibles, and exit deterministically.
4. Present the committed result with short animation/audio while input is locked.
5. Undo/restart when the plan fails, or complete the exit to earn stars, record shards/best moves, and unlock progression.

## Rules and mechanics

- Noor moves one orthogonal cell per accepted turn and may only enter valid, empty, logically safe cells.
- Invalid commands are pure no-ops: no snapshot, animation, move count, guard advance, audio, or save write.
- Adjacent interaction supports configured lamps, mirrors, boxes, and curtains.
- Lamps rotate through serialized 90-degree orientations. Mirrors use fixed slash/backslash reflection tables. Boxes block movement and light. Closed curtains block light; open curtains pass it.
- Rays include traversed cells, stop before opaque blockers, never mark the blocker cell as lit, and terminate at the grid boundary or inclusive range limit. Source cells are not danger cells unless another ray crosses them. Mirror cells are lit because the ray reaches them before reflection.
- Loop detection is per source/ray traversal and keys `(cell, incoming direction)` with a `width × height × 4` hard bound.
- Guards compute intentions simultaneously from the same pre-guard state. Contested destinations and head-on swaps make all involved guards wait. Preview uses the same resolver as execution.
- Immediate player exposure ends the turn before guards advance. Final exposure/contact is checked after guards/moving lights advance. Both failure paths retain the accepted command's pre-turn snapshot for Undo Last Turn.
- Shards are optional per-level collectibles and restore through undo. The exit completes only after its serialized objective conditions are met.

## Turn sequence

Receive and gate command; validate; capture the pre-turn snapshot; apply player movement/interaction; solve light; check immediate safety; advance guards and moving lights; solve light; check final safety/contact; process shards; process exit; update move/HUD state; commit the history result; unlock input after presentation. The authoritative state changes synchronously; animations never drive logic.

## Progression and ratings

- Chapter 1: movement, safety, lamps, ordering, undo, and boxes.
- Chapter 2: mirrors, curtains, combined blockers, and the first lantern guard.
- Chapter 3: multiple guards, moving moonlight, advanced combinations, shard objectives, and final escape.
- Completion unlocks exactly the next level; Level 15 opens Completion and never creates an invalid Level 16.
- Rating baseline: 3 stars at or below par, 2 stars up to `par + 3`, otherwise 1 star for completion. This rule is centralized and may be tuned only before level pars are locked.
- Stars, shard totals, and best moves never regress. Continue appears only when meaningful progress exists.

## Content and screens

Serialized scenes: Boot, Main Menu, Intro, How to Play, Level Select, 15 level scenes, Credits, and Completion. Gameplay includes a HUD, mobile D-pad/Interact controls, tutorial and hint layers, pause, failure, victory, confirmations, and a full-screen transition/input blocker. Every visible action must be functional; no runtime-created required UI/content.

## UI and accessibility

- Canvas reference: 1920×1080. Full-bleed backgrounds sit outside a `SafeAreaRoot`; all interactive content is inside it.
- Minimum reference touch target: 96×96 px with visible normal, pressed, focused, and disabled states.
- Nunito Sans TMP: title 72–96, screen header 44–56, body/button 30–36, metadata 24–28 reference px; allow 30% text expansion.
- Danger uses gold plus chevrons/hatching/source direction; safe shadow retains cyan edge/texture. Guards, lamps, locks, and unfamiliar actions use icon plus text.
- Reduced Flashing removes rapid pulses, white flashes, dense sparkle, and camera shake without changing logic. Haptics and audio remain redundant feedback only.
- Back behavior and modal input blocking are deterministic; pausing/focus loss stops new command submission.

## Art direction

**Moonlit Palace Storybook:** top-down geometric palace shapes, clean inked silhouettes, restrained arch/eight-point lattice motifs, subtle stone/rug pattern, controlled glow, and small magical details.

Palette tokens: blue-black `#090D1D`, deep indigo `#151A3A`, palace indigo `#25285A`, cool cyan `#63D9E6`, danger gold `#F2B84B`, violet `#9A78D4`, ivory `#F4EEDD`, danger orange `#E96A47`.

Gold means active light/danger; cyan means Noor/safe/selection; violet is navigation/magic; orange-red is failure/destructive action. Noor is a compact dark-cloak silhouette with cyan rim/scarf and clear facing. Guards are larger angular gold silhouettes with luminous centers and obvious facing cones. Every interactable has a unique readable silhouette.

Chapter decoration shifts from quiet indigo stone/rugs, to violet mirror mosaics/glass, to dark living-light halls and moon windows, while gameplay semantics remain consistent. Exact cell/beam sprites are authoritative presentation; URP Light2D/glow is secondary atmosphere.

## Audio

One persistent mixer exposes Music and SFX groups. Required original/editor-generated or verified-commercial-use sounds cover ambience, UI, movement, each interaction, shards, guards/detection, failure, victory, exit, undo, and restart. One AudioListener is active. Sources are serialized; settings persist through the central save.

## Save and privacy

A single versioned JSON save stores progression and settings under `Application.persistentDataPath`. Writes use temp file, atomic replacement, and backup; corrupt primary data falls back to backup then defaults. No personal data, network dependency, ads, analytics, tracking, purchases, microphone, camera, or location.

## Acceptance

No design item is `PASS` until its logic, serialized content, visuals, audio, reset/undo/save behavior, tests, manual flow, and applicable screenshot evidence are executed. All 15 documented solution sequences must pass the pure model and be manually completed in their scenes.
