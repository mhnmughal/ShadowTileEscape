# Shadow Tile Escape

A deterministic, turn-based mobile puzzle game set in a moonlit palace. Move Noor through shadow, redirect lanterns with mirrors, push boxes, work curtains, read guard previews, collect shards, and escape fifteen handcrafted rooms.

## Open and run

- Unity: `6000.3.19f1`
- Open this directory in Unity Hub.
- Start from `Assets/Scenes/Boot.unity` and enter Play Mode.
- Production scenes are already serialized; runtime level/UI generation is not required.

## Controls

- Move: WASD, arrow keys, or on-screen direction buttons
- Interact: E, Space, or on-screen Interact
- Undo: Z or Backspace
- Restart: R
- Pause: Escape or the Pause button

## Verification

- EditMode: `Shadow Tile Escape > QA > Run EditMode Tests`
- PlayMode: `Shadow Tile Escape > QA > Run PlayMode Tests`
- Rebuild serialized content: `Shadow Tile Escape > Build > Build Full Game`
- Checked-in result evidence: `TestResults/editmode-results.xml` and `TestResults/playmode-results.xml`
- Current totals: 43/43 Edit Mode and 10/10 Play Mode.
- Final UI evidence: `Assets/Screenshots/UIRedesign_*.png`.
- Detailed QA, level solutions, build evidence, architecture, and MCP setup are under `Docs/`.

## Platform outputs

- Android release APK: `Builds/Android/ShadowTileEscape.apk`
- iOS Unity/Xcode export: `Builds/iOS/Unity-iPhone.xcodeproj`

The iOS export still requires an Apple signing team/profile to produce a distributable IPA. Android store release signing likewise requires a private release keystore; neither credential is committed.

## Notices

Nunito Sans is licensed under SIL OFL 1.1. AnkleBreaker MCP attribution and complete third-party notices are in `ThirdPartyNotices.md`. The app also displays “Made with AnkleBreaker MCP” in product UI.
