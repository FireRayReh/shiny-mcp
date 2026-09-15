using System.Diagnostics;
using System.Text;

namespace ShinyMcp.Server.Tools;

internal static class ProcessRunner
{
    /// <summary>
    /// Runs an external CLI process (dotnet, az, azd, bicep, ...) and captures its output.
    /// Works unchanged on Windows and macOS: <paramref name="fileName"/> is resolved via PATH
    /// by the OS process loader, so no platform-specific executable suffix is needed.
    /// </summary>
    public static async Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        int timeoutSeconds = 180,
        CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            if (!Directory.Exists(workingDirectory))
            {
                return new ProcessResult(-1, "", $"Working directory not found: {workingDirectory}", false);
            }

            psi.WorkingDirectory = workingDirectory;
        }

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            process.Start();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or FileNotFoundException)
        {
            return new ProcessResult(-1, "", $"Failed to start '{fileName}': {ex.Message}. Is it installed and on PATH?", false);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            return new ProcessResult(-1, stdout.ToString(), stderr.ToString(), true);
        }

        return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString(), false);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError, bool TimedOut)
{
    public bool Succeeded => !TimedOut && ExitCode == 0;

    public string Format(string command)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"$ {command}");
        sb.AppendLine(TimedOut ? "[timed out]" : $"[exit code {ExitCode}]");

        if (!string.IsNullOrWhiteSpace(StandardOutput))
        {
            sb.AppendLine().AppendLine("--- stdout ---").Append(StandardOutput);
        }

        if (!string.IsNullOrWhiteSpace(StandardError))
        {
            sb.AppendLine().AppendLine("--- stderr ---").Append(StandardError);
        }

        return sb.ToString();
    }
}
