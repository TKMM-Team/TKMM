using System.Diagnostics;

namespace Tkmm.Core.Helpers;

public static class NxProcessHelper
{
    public static StreamReader ReadCommand(string command)
    {
        Process process = new() {
            StartInfo = new ProcessStartInfo {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        
        return process.StandardOutput;
    }

    public static async ValueTask ExecAsync(string command, CancellationToken ct = default)
    {
        Process process = new() {
            StartInfo = new ProcessStartInfo {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync(ct);
    }

    public static void Exec(string command)
    {
        Process process = new() {
            StartInfo = new ProcessStartInfo {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
    }
}