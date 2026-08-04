# AnkleBreaker Unity MCP Setup Log

Status: **PASS** — exact-project health gate completed on 2026-08-04.

| Check | Status | Evidence |
|---|---|---|
| Prerequisites | PASS | macOS 26.6 arm64; Git 2.50.1; bundled Node 24.14.0; npm 11.9.0; Unity Hub installed; Unity 6000.3.19f1; Codex CLI 0.146.0-alpha.9.2 |
| Unity plugin | PASS | Embedded UPM package `com.anklebreaker.unity-mcp` v2.39.5, commit `9032874ff0b83a5941a01b2ab99599fcc61e90db` |
| Node server | PASS | `Tools/unity-mcp-server`, v2.35.6, entry `src/index.js`, commit `826af5ce61db7e2d2d675fe90d24c941a83f3924`, 92 packages installed |
| Codex configuration | PASS | `.codex/config.toml`; `codex mcp get/list` resolve the project-local Node, entry point, cwd, and environment |
| Bridge port/ping | PASS | MCP `unity_editor_ping`: connected/status ok on 127.0.0.1:7890 |
| Exact project identity | PASS | `/Users/mehranmughal/Documents/Game Development/ShadowTileEscape`; Unity 6000.3.19f1; PID 3569 |
| MCP read tools | PASS | Project info, editor state, active scene, hierarchy, compilation errors, Console logs, packages retrieved |
| Temporary scene/object CRUD | PASS | `MCP_Validation_Temporary`; cube created at `(0,0,0)`, inspected, changed to `(2,1,0)`/45°/1.5 scale, re-inspected, deleted |
| Screenshot | PASS | `Assets/Screenshots/MCP_Phase1_Validation.png`, 1334×750, 15,632 bytes; visually inspected |
| Cleanup | PASS | Original clean `SampleScene` reopened; temporary scene moved to OS Trash; residue search returned 0 assets |

## Installation notes and issues

- Directly adding the Git URL to `Packages/manifest.json` did not trigger resolution while the editor remained open. The same official repository was cloned as Unity's supported embedded UPM package at `Packages/com.anklebreaker.unity-mcp`; Unity then registered it as `source: Embedded` and wrote `file:com.anklebreaker.unity-mcp` to the lock file.
- Initial `npm install` failed because the sandbox blocked the default home cache and DNS. Retrying with `/private/tmp/shadow-tile-escape-npm-cache` and approved network access installed successfully.
- Unity compiled the plugin with zero errors and one third-party `CS0618` warning in `MCPObjectId.cs`; project code had zero warnings/errors.
- Initial npm audit reported 8 advisories. A reviewed dry run showed compatible transitive updates; the lockfile was updated, `npm ci` synchronized generated dependencies, and the final audit reports 0 vulnerabilities.
- The first upstream protocol run hung because the sandbox denied its mock loopback server (`EPERM listen 127.0.0.1`). Rerunning with approved loopback access passed 60/60 tests with zero failures/cancellations.

## License

Both repositories use AnkleBreaker Open License v1.0. The distributed game must visibly display `Made with AnkleBreaker MCP` or `Powered by AnkleBreaker MCP`, with the logo when technically feasible, and retain copyright/license notices.
