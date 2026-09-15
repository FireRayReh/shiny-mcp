using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ShinyMcp.Server.Tools;

/// <summary>
/// Knowledge tools that return .NET/Azure engineering guidance condensed from the
/// `github/awesome-copilot` agents/instructions/skills. These don't touch the filesystem
/// or run anything — they let an editor agent ground its recommendations without
/// needing that repository checked out locally.
/// </summary>
[McpServerToolType]
public static class GuidanceTools
{
    [McpServerTool(Name = "get_dotnet_best_practices"), Description("Returns a condensed .NET/C# best-practices checklist covering architecture, DI, async, testing, and security.")]
    public static string DotNetBestPractices() => Load("dotnet-best-practices.md");

    [McpServerTool(Name = "get_dotnet_janitor_checklist"), Description("Returns an ordered checklist for a .NET code-cleanup / modernization / tech-debt pass.")]
    public static string DotNetJanitorChecklist() => Load("dotnet-janitor-checklist.md");

    [McpServerTool(Name = "get_dotnet_upgrade_checklist"), Description("Returns a step-by-step checklist for upgrading a .NET solution to a newer target framework.")]
    public static string DotNetUpgradeChecklist() => Load("dotnet-upgrade-checklist.md");

    [McpServerTool(Name = "get_azure_naming_conventions"), Description("Returns Azure resource naming conventions based on the Cloud Adoption Framework (CAF), including abbreviations and resources that disallow hyphens.")]
    public static string AzureNamingConventions() => Load("azure-naming-conventions.md");

    [McpServerTool(Name = "get_azd_guardrails"), Description("Returns Azure Developer CLI (azd) safety guardrails and project defaults (secrets handling, environment layout, IaC conventions, CI/CD).")]
    public static string AzdGuardrails() => Load("azd-guardrails.md");

    [McpServerTool(Name = "get_azure_waf_pillars"), Description("Returns a quick-reference for the Azure Well-Architected Framework's five pillars and how to structure an architecture recommendation.")]
    public static string AzureWafPillars() => Load("azure-waf-pillars.md");

    private static string Load(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Guidance", fileName);
        return File.Exists(path)
            ? File.ReadAllText(path)
            : $"Guidance file '{fileName}' was not found alongside the server binary.";
    }
}
