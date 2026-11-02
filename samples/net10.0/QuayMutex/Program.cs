using System.Runtime.InteropServices;

if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("This bounded check executes on Linux.");
string mode = args[0];
string name = args[1];
try
{
    // snippet:scope
    // Quay Lantern's default lock coordinates processes belonging to one user.
    using var gate = mode == "global"
        ? new Mutex(false, name, new NamedWaitHandleOptions { CurrentUserOnly = false, CurrentSessionOnly = false })
        : new Mutex(false, name);
    // endsnippet:scope
    bool entered = gate.WaitOne(0);
    Console.WriteLine($"RUNTIME={RuntimeInformation.FrameworkDescription};ENTERED={entered}");
    if (args.Length > 2 && args[2] == "hold")
    {
        if (!entered) return 22;
        Console.WriteLine("READY");
        Console.Out.Flush();
        Thread.Sleep(120000);
    }
    if (entered) gate.ReleaseMutex();
    return entered ? 0 : 20;
}
#pragma warning disable CA1508 // Real RC2 shared-storage failure is wrapped by a type initializer.
catch (Exception error) when (error is IOException or UnauthorizedAccessException
    || error is TypeInitializationException { InnerException: IOException or UnauthorizedAccessException })
{
    Console.WriteLine($"EXPECTED_STORAGE_FAILURE={error.GetType().Name};{error.Message}");
    return 21;
}
#pragma warning restore CA1508
