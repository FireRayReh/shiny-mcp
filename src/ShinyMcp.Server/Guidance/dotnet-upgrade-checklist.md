# .NET Upgrade Checklist

Use when upgrading a project between .NET versions (e.g. .NET 6/7/8 -> .NET 8/9/10).

1. **Inventory** — list target frameworks across all projects in the solution (`dotnet_sln_list` + inspect each `.csproj`'s `<TargetFramework>`), and enumerate NuGet dependencies (`dotnet_list_packages`) for version compatibility with the target TFM.
2. **Update TFMs** one project at a time, respecting dependency order (libraries before the apps that consume them).
3. **Update packages** — bump package references to versions supporting the new TFM; re-run `dotnet_list_packages` with `outdated`/`vulnerable` to confirm.
4. **Build & fix breaks** — run `dotnet_build`; address analyzer/compiler errors introduced by removed or changed APIs.
5. **Re-run tests** — `dotnet_test` after each project's upgrade, not just at the end.
6. **Adopt new defaults deliberately** — nullable reference types, implicit usings, new analyzer rules; opt in incrementally rather than suppressing warnings wholesale.
7. **Check runtime-specific config** — `global.json` SDK pin, Docker base images, CI pipeline SDK versions, and `RuntimeIdentifiers` for platform-specific builds.
8. **Validate hosting/deployment** — Azure App Service / Functions / Container Apps runtime stack settings must match the new TFM before deploying.

Do a solution-wide `dotnet_build` + `dotnet_test` pass as the final gate before calling the upgrade complete.

Source: adapted from `github/awesome-copilot` (`agents/dotnet-upgrade.agent.md`, `instructions/dotnet-upgrade.instructions.md`), MIT licensed.
