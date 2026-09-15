# shiny-mcp

A C# (.NET 8) MCP server for .NET / Azure engineering work in VS Code, built around the
kind of operations covered by the [`github/awesome-copilot`](https://github.com/github/awesome-copilot)
agents, skills, and instructions for `.NET`/`C#` and `Azure` (MIT licensed). It runs unmodified on
Windows and macOS as a local stdio process — no cloud hosting required.

## What it gives an agent

**`dotnet_*` tools** — shell out to the `dotnet` CLI:
`dotnet_restore`, `dotnet_build`, `dotnet_test`, `dotnet_run`, `dotnet_format`, `dotnet_clean`,
`dotnet_new`, `dotnet_sln_list`, `dotnet_list_packages` (all/outdated/vulnerable/deprecated),
`dotnet_add_package`.

**`azure_*` / `azd_*` / `bicep_*` tools** — shell out to `az` and `azd`:
- Read-only: `azure_whoami`, `azure_list_subscriptions`, `azure_list_resource_groups`,
  `azure_list_resources`, `bicep_build`, `bicep_what_if`, `azd_env_list`, `azd_env_values`, `azd_package`.
- Mutating (require `confirm: true`, matching the safety guardrails in the awesome-copilot
  `azure-developer-cli` skill): `azd_provision`, `azd_deploy`, `azd_down`.

**Guidance tools** — return condensed, no-network reference material distilled from the
awesome-copilot .NET/Azure agents and instructions, so an editor agent can ground its
recommendations without needing that repo checked out:
`get_dotnet_best_practices`, `get_dotnet_janitor_checklist`, `get_dotnet_upgrade_checklist`,
`get_azure_naming_conventions`, `get_azd_guardrails`, `get_azure_waf_pillars`.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later, on PATH.
- For the `azure_*`/`azd_*` tools: [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
  and/or [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
  on PATH, and `az login` already run. Tools work fine without them if you only need the
  `dotnet_*`/guidance tools — each Azure tool just reports the CLI is missing if it isn't installed.
- VS Code with the GitHub Copilot / Copilot Chat extension (MCP support).

Both `dotnet`, `az`, and `azd` are cross-platform CLIs, and the server only ever invokes them by
name via `PATH` resolution, so the exact same code and config work on Windows and macOS.

## Build

```bash
cd src/ShinyMcp.Server
dotnet build -c Release
```

## Connect it to VS Code

This repo already includes `.vscode/mcp.json`:

```json
{
  "servers": {
    "shiny-mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "${workspaceFolder}/src/ShinyMcp.Server/ShinyMcp.Server.csproj", "-c", "Release"]
    }
  }
}
```

Open this folder in VS Code, open the Copilot Chat panel, switch to **Agent** mode, and click
**Tools** — `shiny-mcp` should appear with its tool list. VS Code starts/stops the server process
for you; no manual `dotnet run` needed.

If you'd rather register it globally (available in every workspace) instead of per-repo, run
**MCP: Add Server** from the Command Palette and point it at the same `dotnet run ...` command,
or add the equivalent block to your user `mcp.json` (Windows: `%APPDATA%\Code\User\mcp.json`,
macOS: `~/Library/Application Support/Code/User/mcp.json`).

## Publishing a standalone binary (optional)

If you don't want VS Code to depend on the .NET SDK being present, publish a self-contained
executable and point `command` at it directly instead of `dotnet run`:

```bash
# Windows
dotnet publish src/ShinyMcp.Server -c Release -r win-x64 --self-contained -o publish/win-x64

# macOS (Apple Silicon)
dotnet publish src/ShinyMcp.Server -c Release -r osx-arm64 --self-contained -o publish/osx-arm64

# macOS (Intel)
dotnet publish src/ShinyMcp.Server -c Release -r osx-x64 --self-contained -o publish/osx-x64
```

Then set `"command"` to the full path of `publish/<rid>/shiny-mcp` (or `shiny-mcp.exe` on Windows).

## Safety notes

- All logging goes to **stderr**; stdout is reserved for the MCP JSON-RPC stream (mixing the two
  breaks the protocol).
- `azd_provision`, `azd_deploy`, and `azd_down` refuse to run unless called with `confirm: true` —
  the agent should only set that after you've explicitly approved the target environment.
- Long-running or hung processes are killed after a per-tool timeout (longer for `dotnet_test`,
  `azd provision/deploy/down`).

## Attribution

The guidance content under `src/ShinyMcp.Server/Guidance/` is condensed and adapted from the
MIT-licensed [`github/awesome-copilot`](https://github.com/github/awesome-copilot) repository's
`.NET`/`C#`/`Azure` agents, instructions, and skills — see the `Source:` line at the bottom of
each file for the specific upstream document.
