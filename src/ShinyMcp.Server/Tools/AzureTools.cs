using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ShinyMcp.Server.Tools;

/// <summary>
/// Azure engineering operations via the `az` and `azd` CLIs. Read-only operations run freely;
/// operations that create, modify, or delete cloud resources require <c>confirm: true</c>,
/// mirroring the safety guardrails in the awesome-copilot azure-developer-cli skill
/// (https://github.com/github/awesome-copilot/blob/main/skills/azure-developer-cli/SKILL.md).
/// </summary>
[McpServerToolType]
public static class AzureTools
{
    private const string ConfirmRequiredMessage =
        "This operation can create, modify, or delete Azure resources. Re-run with confirm: true " +
        "only after the user has explicitly approved the target environment, subscription, and scope.";

    [McpServerTool(Name = "azure_whoami"), Description("Shows the currently logged-in Azure CLI identity and active subscription.")]
    public static async Task<string> WhoAmI()
        => await RunAz(["account", "show"]);

    [McpServerTool(Name = "azure_list_subscriptions"), Description("Lists Azure subscriptions visible to the logged-in identity.")]
    public static async Task<string> ListSubscriptions()
        => await RunAz(["account", "list", "--output", "table"]);

    [McpServerTool(Name = "azure_list_resource_groups"), Description("Lists resource groups in the active (or specified) subscription.")]
    public static async Task<string> ListResourceGroups(
        [Description("Subscription id or name. Omit to use the active subscription.")] string? subscription = null)
    {
        List<string> args = ["group", "list", "--output", "table"];
        if (!string.IsNullOrWhiteSpace(subscription)) args.AddRange(["--subscription", subscription]);
        return await RunAz(args);
    }

    [McpServerTool(Name = "azure_list_resources"), Description("Lists resources, optionally scoped to a resource group.")]
    public static async Task<string> ListResources(
        [Description("Resource group name to scope the listing to. Omit to list across the subscription.")] string? resourceGroup = null)
    {
        List<string> args = ["resource", "list", "--output", "table"];
        if (!string.IsNullOrWhiteSpace(resourceGroup)) args.AddRange(["--resource-group", resourceGroup]);
        return await RunAz(args);
    }

    [McpServerTool(Name = "bicep_build"), Description("Compiles a Bicep file to ARM JSON and reports errors/warnings, without deploying anything.")]
    public static async Task<string> BicepBuild(
        [Description("Path to the .bicep file, e.g. infra/main.bicep.")] string bicepFilePath)
        => await RunAz(["bicep", "build", "--file", bicepFilePath, "--stdout"]);

    [McpServerTool(Name = "bicep_what_if"), Description("Previews the resource changes a Bicep deployment would make, without applying them (az deployment ... what-if).")]
    public static async Task<string> BicepWhatIf(
        [Description("Resource group to evaluate the deployment against.")] string resourceGroup,
        [Description("Path to the .bicep entry point, e.g. infra/main.bicep.")] string bicepFilePath,
        [Description("Path to a .bicepparam or JSON parameters file. Optional.")] string? parametersFilePath = null)
    {
        List<string> args = ["deployment", "group", "what-if", "--resource-group", resourceGroup, "--template-file", bicepFilePath];
        if (!string.IsNullOrWhiteSpace(parametersFilePath)) args.AddRange(["--parameters", parametersFilePath]);
        return await RunAz(args, timeoutSeconds: 300);
    }

    [McpServerTool(Name = "azd_env_list"), Description("Lists Azure Developer CLI (azd) environments defined for the current project.")]
    public static async Task<string> AzdEnvList(
        [Description("Path to the azd project (containing azure.yaml). Defaults to the current directory.")] string? projectPath = null)
        => await RunAzd(["env", "list"], projectPath);

    [McpServerTool(Name = "azd_env_values"), Description("Shows the variables of an azd environment (excluding secrets stored in Key Vault).")]
    public static async Task<string> AzdEnvValues(
        [Description("Environment name. Omit to use the currently selected environment.")] string? environmentName = null,
        [Description("Path to the azd project. Defaults to the current directory.")] string? projectPath = null)
    {
        List<string> args = ["env", "get-values"];
        if (!string.IsNullOrWhiteSpace(environmentName)) args.AddRange(["-e", environmentName]);
        return await RunAzd(args, projectPath);
    }

    [McpServerTool(Name = "azd_package"), Description("Packages the application(s) for deployment without provisioning or deploying (azd package).")]
    public static async Task<string> AzdPackage(
        [Description("Path to the azd project. Defaults to the current directory.")] string? projectPath = null)
        => await RunAzd(["package"], projectPath, timeoutSeconds: 300);

    [McpServerTool(Name = "azd_provision"), Description("Provisions Azure infrastructure for the project via `azd provision`. Creates or modifies cloud resources — requires explicit confirmation.")]
    public static async Task<string> AzdProvision(
        [Description("Environment name to provision.")] string environmentName,
        [Description("Path to the azd project. Defaults to the current directory.")] string? projectPath = null,
        [Description("Must be true. Set only after the user has explicitly approved provisioning this environment.")] bool confirm = false)
    {
        if (!confirm) return ConfirmRequiredMessage;
        return await RunAzd(["provision", "-e", environmentName, "--no-prompt"], projectPath, timeoutSeconds: 900);
    }

    [McpServerTool(Name = "azd_deploy"), Description("Deploys application code for the project via `azd deploy`. Modifies a live environment — requires explicit confirmation.")]
    public static async Task<string> AzdDeploy(
        [Description("Environment name to deploy to.")] string environmentName,
        [Description("Specific service name from azure.yaml to deploy. Omit to deploy all services.")] string? serviceName = null,
        [Description("Path to the azd project. Defaults to the current directory.")] string? projectPath = null,
        [Description("Must be true. Set only after the user has explicitly approved deploying to this environment.")] bool confirm = false)
    {
        if (!confirm) return ConfirmRequiredMessage;
        List<string> args = ["deploy", "-e", environmentName, "--no-prompt"];
        if (!string.IsNullOrWhiteSpace(serviceName)) args.Add(serviceName);
        return await RunAzd(args, projectPath, timeoutSeconds: 900);
    }

    [McpServerTool(Name = "azd_down"), Description("Tears down provisioned Azure infrastructure via `azd down`. Deletes cloud resources — requires explicit confirmation.")]
    public static async Task<string> AzdDown(
        [Description("Environment name to tear down.")] string environmentName,
        [Description("Path to the azd project. Defaults to the current directory.")] string? projectPath = null,
        [Description("Must be true. Set only after the user has explicitly approved deleting this environment's resources.")] bool confirm = false)
    {
        if (!confirm) return ConfirmRequiredMessage;
        return await RunAzd(["down", "-e", environmentName, "--force", "--purge"], projectPath, timeoutSeconds: 900);
    }

    private static async Task<string> RunAz(List<string> args, int timeoutSeconds = 120)
    {
        var result = await ProcessRunner.RunAsync("az", args, workingDirectory: null, timeoutSeconds);
        return result.Format("az " + string.Join(' ', args));
    }

    private static async Task<string> RunAzd(List<string> args, string? projectPath, int timeoutSeconds = 120)
    {
        var workingDirectory = projectPath is not null && Directory.Exists(projectPath) ? projectPath : null;
        var result = await ProcessRunner.RunAsync("azd", args, workingDirectory, timeoutSeconds);
        return result.Format("azd " + string.Join(' ', args));
    }
}
