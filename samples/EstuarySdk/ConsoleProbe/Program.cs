using System.Diagnostics;
using System.Runtime.InteropServices;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

// This helper is launched with no attached console. Allocate and hide only its own.
Stream output = Console.OpenStandardOutput();
if (Native.GetConsoleWindow() != IntPtr.Zero) throw new InvalidOperationException("Refuse a shared console");
Native.FreeConsole(); // Detach this child from a windowless inherited console, if any.
if (!Native.AllocConsole()) throw new InvalidOperationException("Cannot allocate a private console");
Native.ShowWindow(Native.GetConsoleWindow(), 0);
try
{
    if (!Native.SetConsoleCP(uint.Parse(args[0])) || !Native.SetConsoleOutputCP(uint.Parse(args[0])))
        throw new InvalidOperationException("Cannot select the private code page");
    var start = new ProcessStartInfo(args[1]) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
    start.Environment["ESTUARY_PRIVATE_CONSOLE_OWNER"] = Environment.ProcessId.ToString();
    foreach (string argument in args.Skip(2)) start.ArgumentList.Add(argument);
    using var child = Process.Start(start)!;
    Task stdout = child.StandardOutput.BaseStream.CopyToAsync(output);
    Task stderr = child.StandardError.BaseStream.CopyToAsync(Console.OpenStandardError());
    if (!child.WaitForExit(240000)) { child.Kill(true); throw new TimeoutException(); }
    Task.WhenAll(stdout, stderr).GetAwaiter().GetResult();
    return child.ExitCode;
}
finally { Native.FreeConsole(); }

internal static partial class Native
{
    [LibraryImport("kernel32")] internal static partial IntPtr GetConsoleWindow();
    [LibraryImport("kernel32")] [return: MarshalAs(UnmanagedType.Bool)] internal static partial bool AllocConsole();
    [LibraryImport("kernel32")] [return: MarshalAs(UnmanagedType.Bool)] internal static partial bool FreeConsole();
    [LibraryImport("kernel32")] [return: MarshalAs(UnmanagedType.Bool)] internal static partial bool SetConsoleCP(uint page);
    [LibraryImport("kernel32")] [return: MarshalAs(UnmanagedType.Bool)] internal static partial bool SetConsoleOutputCP(uint page);
    [LibraryImport("user32")] [return: MarshalAs(UnmanagedType.Bool)] internal static partial bool ShowWindow(IntPtr window, int command);
}
