// snippet:complete
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace CwmSeedBank.Host
{
    public static class CancelSelection
    {
        [DllImport("kernel32.dll")] private static extern bool AllocConsole();
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("kernel32.dll")] private static extern bool GenerateConsoleCtrlEvent(uint signal, uint group);
        private delegate bool Handler(uint signal);
        [DllImport("kernel32.dll")] private static extern bool SetConsoleCtrlHandler(Handler handler, bool add);
        private static readonly Handler KeepControllerAlive = signal => true;

        public static int Main(string[] args)
        {
            // A private hidden console lets the real SDK receive Ctrl+C. No shell,
            // environment/PATH mutation, forced success or process-kill substitute.
            if (!AllocConsole()) return 90;
            ShowWindow(GetConsoleWindow(), 0);
            SetConsoleCtrlHandler(KeepControllerAlive, true);
            if (args[4] != "--affected-tests" && args[4] != "--collect-test-map") return 95;
            var start = new ProcessStartInfo(args[0], "test -c Release --no-ansi " + args[4])
            {
                WorkingDirectory = args[1], UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            using (Process process = Process.Start(start))
            {
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                for (int i = 0; i < 600 && !File.Exists(args[2]) && !process.HasExited; i++) Thread.Sleep(50);
                if (!File.Exists(args[2]) || !GenerateConsoleCtrlEvent(0, 0) || !process.WaitForExit(30000))
                {
                    if (!process.HasExited)
                    {
                        using (Process cleanup = Process.Start(new ProcessStartInfo(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe"),
                            "/PID " + process.Id + " /T /F") { UseShellExecute = false, CreateNoWindow = true }))
                            cleanup.WaitForExit();
                    }
                    return 93;
                }
                File.WriteAllText(args[3], stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
                File.WriteAllText(args[3] + ".exit", process.ExitCode.ToString());
                return process.ExitCode == 0 ? 94 : 0;
            }
        }
    }
}
// endsnippet:complete
