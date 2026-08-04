# Shadow Tile Escape — Governing Rules

Every Codex agent must read this file before working in this repository.

## Complete-product standard

- The deliverable is a complete mobile game, not a prototype.
- Every requested gameplay mechanic must be implemented, connected, and tested.
- Every visible button must perform its intended action.
- Every required scene must be saved and included in Build Settings.
- No placeholder screens, placeholder buttons, or unfinished gameplay may remain.
- Compilation alone does not prove completion; actual menu and gameplay flows must be tested.
- All 15 levels must be manually playable and solvable.
- A feature is complete only when its logic, visuals, audio, UI, reset, undo, and save behavior work where applicable.

## Manual Unity construction

All required game content must exist as serialized Unity content before Play Mode, including scenes, cameras, canvases, EventSystems, UI panels, buttons, text, managers, grid cells, lights, lamps, mirrors, boxes, curtains, guards, Noor, collectibles, exits, AudioSources, particles, transitions, tutorials, and required effects.

Allowed: Unity MCP and Editor scripts during development; editor-time SVG, PNG, audio, and configuration generation; prefab creation/editing before Play Mode; runtime activation, movement, animation, and component-property changes on existing objects; pre-created object pools.

Required production content must not use these during Play Mode:

```csharp
new GameObject(...)
Object.Instantiate(...)
gameObject.AddComponent(...)
Resources.Load(...)
Addressables.InstantiateAsync(...)
```

Do not dynamically generate UI, cameras, managers, levels, grid geometry, environments, core actors, or required effects. Do not use `GameObject.Find`, `FindObjectOfType`, `FindFirstObjectByType`, or `FindAnyObjectByType` as the normal dependency system. Use serialized Inspector, scene, or explicitly assigned prefab references.

## Coding standard

- Use namespace `ShadowTileEscape` and clear, single-responsibility classes.
- Separate deterministic logical state from visual animation.
- Use serialized references and validate serialized configuration.
- Avoid hidden dependencies, giant managers, scattered `PlayerPrefs`, unnecessary static state, unnecessary `Update`, per-frame grid/light solves, and avoidable allocations.
- Use events or commands where appropriate.
- Report errors clearly; never silently swallow exceptions.
- Do not modify third-party package code unless unavoidable.
- Resolve all project-code compilation errors and meaningful warnings.

## Art and licensing standard

- Use only original or verified commercial-use assets; never use paid or copied content.
- Use TextMeshPro and legitimately sourced Nunito Sans for all player-facing text; retain its license.
- Record every external package, font, sound, and asset in `ThirdPartyNotices.md`.
- Include all AnkleBreaker license notices and visible Credits attribution when required.

## Git standard

- Inspect Git status before editing; preserve unrelated work and never revert pre-existing changes.
- Make focused local commits after stable phase gates when Git is available; never push or rewrite history.
- Record commit hashes in the development log.

## Testing standard

- Check compilation after each script batch and clear the Console before major validation.
- Run relevant Edit Mode and Play Mode tests plus real user flows.
- Inspect Game View screenshots at required aspect ratios and safe areas.
- Test touch and desktop controls, pause, restart, undo, failure, victory, save/load, unlocks, and progress reset.
- Audit forbidden runtime-content creation before completion.
- Use `PASS`, `FAIL`, `BLOCKED`, and `NOT RUN` honestly; never report an unrun check as passed.

## Parallel-agent standard

- Only one write owner may edit shared Unity scenes, prefabs, and `ProjectSettings` at a time.
- Parallel agents primarily perform read-only audits, research, review, test analysis, documentation review, and screenshot review.
- Agents must avoid conflicting edits and report scope, files inspected, findings, severity, and recommended fixes.
- The primary agent owns consolidation and final decisions.

## Phase gates

Follow phases 0–9 in dependency order. At each phase end: compile, inspect Console, run applicable tests, inspect scenes, update documentation, fix failures, commit when Git is available, record evidence, and proceed only on `PASS` or an explicitly documented blocker. Do not begin gameplay until the AnkleBreaker Unity MCP health gate passes for this exact project.
