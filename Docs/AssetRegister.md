# Asset Register

Status: Full-game inventory current.

| Asset/package | Origin | License | Commercial use | Project path | Status |
|---|---|---|---|---|---|
| Unity URP/2D starter assets | Unity project template | Unity package/template terms | Yes under applicable Unity terms | `Assets/Settings`, `Assets/DefaultVolumeProfile.asset` | Present; inspect versions |
| Input System actions | Unity project template | Unity package/template terms | Yes under applicable Unity terms | `Assets/InputSystem_Actions.inputactions` | Present |
| Nunito Sans variable/italic | Google Fonts `google/fonts`, commit `2796410152d4f9524b68ed46e69c1b60f8e0f7c3` | SIL OFL 1.1 | Yes; embed/bundle with notice/license | `Assets/Fonts/NunitoSans/` | ACQUIRED/IMPORTED |
| Nunito Sans TMP SDF assets | Editor-generated from the official variable sources | SIL OFL 1.1 | Yes | `Assets/Fonts/NunitoSans/* SDF.asset` | CREATED |
| TMP Essential Resources | Unity uGUI/TMP 2.0.0 package | Unity package terms plus included notices | Yes with Unity project | `Assets/TextMesh Pro/` | IMPORTED |
| AnkleBreaker Unity MCP plugin v2.39.5 | AnkleBreaker Studio GitHub, commit `9032874` | AnkleBreaker Open License v1.0 | Internal/commercial project use permitted; attribution required | `Packages/com.anklebreaker.unity-mcp` | INSTALLED |
| AnkleBreaker Unity MCP server v2.35.6 | AnkleBreaker Studio GitHub, commit `826af5c` | AnkleBreaker Open License v1.0 | Internal/commercial project use permitted; attribution required | `Tools/unity-mcp-server` | INSTALLED |
| Full-game UI/game art | Original serialized uGUI geometry/colors | Project-owned | Yes | `Assets/Scenes/**` | CREATED |
| App icon | OpenAI built-in image generation; final prompt recorded below | Project-owned generated content subject to applicable OpenAI terms | Yes | `Assets/Art/AppIcon/ShadowTileEscape_AppIcon.png` | CREATED/IMPORTED/ASSIGNED |
| Feedback tones/ambience | Editor-generated sine tones authored by project builder | Project-owned | Yes | `Assets/Audio/Generated/*.wav` | CREATED |
| Audio mixer | Unity editor-generated mixer with Music/SFX groups | Project-owned configuration | Yes | `Assets/Audio/ShadowTileEscape.mixer` | CREATED |

Every added external or generated asset will be registered with source, license, attribution, and path.

Phase 1 evidence screenshot is project-generated at `Assets/Screenshots/MCP_Phase1_Validation.png`.

Phase 3 project-generated evidence: `VerticalSlice_MainMenu.png`, `VerticalSlice_Level01_Initial.png`, `VerticalSlice_Level01_Failure.png`, and `VerticalSlice_Level01_Completed.png` under `Assets/Screenshots/`.

Nunito SHA-256: upright `f934d7142fb4784bf828da485b7dcbd90c0c80d514e9d49a5da0ed3a1ae2491d`; italic `d9d5db18f3c11221a4fbb553cbc709391c1179964c7eaa4466ef43c78aa4492f`; OFL `efbb0c9e864cef973982d9a17567e6be5c3d1759695574586f3f18c7ecca064b`.

## App icon generation record

Mode: OpenAI built-in ImageGen.

Final prompt:

> Use case: logo-brand. Asset type: 1024x1024 mobile game app icon for Shadow Tile Escape. Primary request: a striking geometric icon showing a small cyan hooded escapee silhouette stepping across one deep-indigo palace floor tile while a single sharp golden lantern beam narrowly misses them; a crescent moon and pointed Mughal-inspired palace arch form the background silhouette. Style/medium: polished flat geometric illustration, premium indie puzzle game identity, crisp large shapes, subtle paper-grain texture only. Composition/framing: centered square icon, strong silhouette, readable at 48px, generous safe margins, no border. Lighting/mood: mysterious moonlit palace, clever rather than frightening. Color palette: near-black navy, deep indigo, vivid cyan, warm gold, small ivory accent. Constraints: no words, no letters, no numerals, no watermark, no mockup device, no rounded-corner mask, no photorealism, no tiny details, no gradients that reduce small-size clarity. Avoid: generic keyholes, skulls, weapons, horror imagery, busy scenery.
