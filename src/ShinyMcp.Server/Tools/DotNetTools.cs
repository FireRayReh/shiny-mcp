using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ShinyMcp.Server.Tools;

/// <summary>
/// .NET/C# engineering operations: build, test, restore, run, format, packages, and scaffolding.
/// Shells out to the `dotnet` CLI that must already be installed and on PATH.
/// </summary>
[McpServerToolType]
public static class DotNetTools
{
    [McpServerTool(Name = "dotnet_restore"), Description("Restores NuGet dependencies for a project or solution.")]
    public static async Task<string> Restore(
        [Description("Path to a .sln, .csproj, or directory. Defaults to the current directory.")] string? path = null)
        => await Run(["restore", .. PathArg(path)], path);

    [McpServerTool(Name = "dotnet_build"), Description("Builds a .NET project or solution.")]
    public static async Task<string> Build(
        [Description("Path to a .sln, .csproj, or directory. Defaults to the current directory.")] string? path = null,
        [Description("Build configuration, e.g. Debug or Release.")] string configuration = "Debug",
        [Description("If true, does not run an implicit restore before building.")] bool noRestore = false)
    {
        List<string> args = ["build", .. PathArg(path), "-c", configuration];
        if (noRestore) args.Add("--no-restore");
        return await Run(args, path);
    }

    [McpServerTool(Name = "dotnet_test"), Description("Runs unit tests for a project or solution using the configured test framework (e.g. xUnit, MSTest, NUnit).")]
    public static async Task<string> Test(
        [Description("Path to a .sln, .csproj, or directory. Defaults to the current directory.")] string? path = null,
        [Description("Optional filter expression passed to --filter, e.g. 'FullyQualifiedName~MyNamespace'.")] string? filter = null,
        [Description("Build configuration, e.g. Debug or Release.")] string configuration = "Debug")
    {
        List<string> args = ["test", .. PathArg(path), "-c", configuration];
        if (!string.IsNullOrWhiteSpace(filter)) args.AddRange(["--filter", filter]);
        return await Run(args, path, timeoutSeconds: 600);
    }

    [McpServerTool(Name = "dotnet_run"), Description("Runs a .NET project. Intended for quick smoke checks, not long-lived servers.")]
    public static async Task<string> RunProject(
        [Description("Path to a .csproj or directory. Defaults to the current directory.")] string? path = null,
        [Description("Arguments to pass to the application, after '--'.")] string[]? appArguments = null,
        [Description("Timeout in seconds before the process is killed.")] int timeoutSeconds = 60)
    {
        List<string> args = ["run", .. PathArg(path)];
        if (appArguments is { Length: > 0 })
        {
            args.Add("--");
            args.AddRange(appArguments);
        }
        return await Run(args, path, timeoutSeconds);
    }

    [McpServerTool(Name = "dotnet_format"), Description("Applies C# code style and analyzer fixes with `dotnet format` (whitespace, usings, analyzer diagnostics).")]
    public static async Task<string> Format(
        [Description("Path to a .sln or .csproj. Defaults to the current directory.")] string? path = null,
        [Description("If true, only reports issues without modifying files (--verify-no-changes).")] bool verifyOnly = false)
    {
        List<string> args = ["format", .. PathArg(path)];
        if (verifyOnly) args.Add("--verify-no-changes");
        return await Run(args, path);
    }

    [McpServerTool(Name = "dotnet_clean"), Description("Cleans build outputs (bin/obj) for a project or solution.")]
    public static async Task<string> Clean(
        [Description("Path to a .sln, .csproj, or directory. Defaults to the current directory.")] string? path = null)
        => await Run(["clean", .. PathArg(path)], path);

    [McpServerTool(Name = "dotnet_new"), Description("Scaffolds a new .NET project, solution, or file from a template (e.g. webapi, console, classlib, worker, azurefunctions).")]
    public static async Task<string> New(
        [Description("Template short name, e.g. 'webapi', 'console', 'classlib', 'worker', 'sln', 'azurefunctions'.")] string template,
        [Description("Project/output name.")] string? name = null,
        [Description("Directory to create the project in. Created if missing.")] string? outputDirectory = null,
        [Description("Target framework moniker, e.g. net8.0. Omit to use the template default.")] string? framework = null)
    {
        List<string> args = ["new", template];
        if (!string.IsNullOrWhiteSpace(name)) args.AddRange(["-n", name]);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            args.AddRange(["-o", outputDirectory]);
        }
        if (!string.IsNullOrWhiteSpace(framework)) args.AddRange(["-f", framework]);
        return await Run(args, path: null);
    }

    [McpServerTool(Name = "dotnet_sln_list"), Description("Lists the projects referenced by a solution file.")]
    public static async Task<string> SolutionList(
        [Description("Path to a .sln file. Defaults to the solution found in the current directory.")] string? solutionPath = null)
        => await Run(["sln", .. PathArg(solutionPath), "list"], solutionPath);

    [McpServerTool(Name = "dotnet_list_packages"), Description("Lists NuGet package references for a project or solution, optionally filtered to outdated or vulnerable packages.")]
    public static async Task<string> ListPackages(
        [Description("Path to a .sln or .csproj. Defaults to the current directory.")] string? path = null,
        [Description("One of: 'all' (default), 'outdated', 'vulnerable', 'deprecated'.")] string mode = "all")
    {
        List<string> args = ["list", .. PathArg(path), "package"];
        switch (mode.ToLowerInvariant())
        {
            case "outdated": args.Add("--outdated"); break;
            case "vulnerable": args.Add("--vulnerable"); break;
            case "deprecated": args.Add("--deprecated"); break;
        }
        return await Run(args, path);
    }

    [McpServerTool(Name = "dotnet_add_package"), Description("Adds a NuGet package reference to a project.")]
    public static async Task<string> AddPackage(
        [Description("Path to the .csproj to modify.")] string projectPath,
        [Description("NuGet package id, e.g. 'Azure.Identity'.")] string packageId,
        [Description("Specific package version. Omit to use the latest stable version.")] string? version = null)
    {
        List<string> args = ["add", projectPath, "package", packageId];
        if (!string.IsNullOrWhiteSpace(version)) args.AddRange(["-v", version]);
        return await Run(args, path: null);
    }

    private static IEnumerable<string> PathArg(string? path)
        => string.IsNullOrWhiteSpace(path) ? [] : [path];

    private static async Task<string> Run(List<string> args, string? path, int timeoutSeconds = 180)
    {
        var workingDirectory = path is not null && Directory.Exists(path) ? path : null;
        var result = await ProcessRunner.RunAsync("dotnet", args, workingDirectory, timeoutSeconds);
        return result.Format("dotnet " + string.Join(' ', args));
    }
}
