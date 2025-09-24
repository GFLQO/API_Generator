using System;
using System.Diagnostics;

namespace ArchiForge.Utils
{
    public static class ShellRunner
    {
        public static void Run(string cmd, string workingDir)
        {
            var psi = new ProcessStartInfo("cmd", $"/c {cmd}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDir
            };

            var p = Process.Start(psi)!;
            p.WaitForExit();

            if (p.ExitCode != 0)
                throw new Exception($"Command failed: {cmd}");
        }
    }
}
