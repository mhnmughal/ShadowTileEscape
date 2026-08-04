# Third-Party Notices

Status: initial inventory; no new third-party content has been imported yet.

## Unity packages and template content

This project currently contains Unity-provided 2D/URP template configuration and packages. Exact package versions and applicable Unity terms will be recorded after Phase 0 package inspection.

## Nunito Sans

Official variable upright and italic fonts were acquired from Google Fonts' `google/fonts` repository at commit `2796410152d4f9524b68ed46e69c1b60f8e0f7c3` and stored under `Assets/Fonts/NunitoSans/`.

Copyright 2016 The Nunito Sans Project Authors (https://github.com/Fonthausen/NunitoSans).

Licensed under the SIL Open Font License, Version 1.1. The full unmodified license is stored at `Assets/Fonts/NunitoSans/License/OFL.txt`. The font may be commercially embedded/bundled but may not be sold by itself. The official variable files span weight 200–1000, covering the required Regular 400, SemiBold 600, Bold 700, and ExtraBold 800 design weights.

## AnkleBreaker Unity MCP

Plugin v2.39.5 (commit `9032874ff0b83a5941a01b2ab99599fcc61e90db`) and server v2.35.6 (commit `826af5ce61db7e2d2d675fe90d24c941a83f3924`) are installed from AnkleBreaker Studio's official GitHub repositories.

Copyright (c) 2024-2026 AnkleBreaker Consulting & AnkleBreaker Studio. Licensed under AnkleBreaker Open License v1.0; copies are stored in both installed repositories. The distributed game will include visible `Made with AnkleBreaker MCP` attribution and the logo when technically feasible. Resale/repackaging of the MCP software itself is prohibited; its internal use for this commercial game project is permitted by the license.

Official license SHA-256: `6a3bb42d593b6612cbc31514ca8fc13f78042d1ed0e29b781ed4d7b79780f6c5`. Official logo SHA-256: `c1955900f3c24ed654658c3e4121b680760444046a808b09fdf5ffc29f9dce3`.

## Unity packages and TextMeshPro resources

Resolved package names and versions are locked in `Packages/packages-lock.json`. Unity registry/built-in packages are governed by their included Unity Companion, Unity Package Distribution, Unity Terms, and third-party notices as applicable. TMP essential resources imported from uGUI 2.0.0 include LiberationSans under OFL and EmojiOne attribution files; these are retained as required package resources but will not be used for player-facing text. Package-cache sample fonts/art must not be reused as game assets without a separate register entry.

Notable third-party license families within current Unity packages include MIT, BSD, Boost, MPL-2.0, Apache-2.0 with LLVM exception, NCSA, zlib, Microsoft Public License, and CC BY 3.0. No Visual Scripting icon/sample art will be shipped as game artwork. Applicable notices entering a player build will be copied to the in-game Licenses view before release.

## MCP server dependencies

The development-only Node server remains outside `Assets` and is excluded from player builds. Its exact dependency inventory and integrity hashes are locked in `Tools/unity-mcp-server/package-lock.json`: 93 non-root dependency instances (83 MIT, 7 ISC, 2 BSD-3-Clause, 1 BSD-2-Clause). Installed packages have license metadata/files, and final `npm audit` reports zero vulnerabilities. If the development server is redistributed with the project, its lockfile and corresponding license texts must accompany it.

## Art and audio

No external game art or audio has been added. Planned art, icon, effects, and audio will be original project-created content unless a commercially usable source is documented here first.
