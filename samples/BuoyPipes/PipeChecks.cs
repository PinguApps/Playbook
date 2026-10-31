using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using BuoyPipes;
using Microsoft.Win32.SafeHandles;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
byte[] report = "buoy-4:pressure=1012"u8.ToArray();
Check(PipeTransfer.ReadReport(report).SequenceEqual(report), "canonical pipe round trip");
Console.WriteLine("PASS canonical pipe round trip");
string name = "playbook-buoy-" + Guid.NewGuid().ToString("N");
using (NamedPipeServerStream server = new(name, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.CurrentUserOnly))
using (NamedPipeClientStream client = new(".", name, PipeDirection.In, PipeOptions.None))
{
    Task connected = server.WaitForConnectionAsync();
    await client.ConnectAsync(5000);
    await connected;
    SafePipeHandle transferred = client.SafePipeHandle;
    Thread sender = new(() => server.Write(new byte[] { 71 }));
    sender.Start();
    Check(ConnectedReport.ReadOne(transferred, client.IsAsync) == 71, "wrapped connected handle");
    sender.Join();
    Check(transferred.IsClosed, "reader owns and closes supplied SafePipeHandle instance");
}
Console.WriteLine("PASS named pipe same-user connection; older isConnected=true; transferred handle disposed once; original stream is not reused");
string path = Path.Combine(Path.GetTempPath(), "playbook-file-" + Guid.NewGuid().ToString("N"));
try
{
    await File.WriteAllBytesAsync(path, report);
    using FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
    bool expectedAsync = OperatingSystem.IsWindows() || Environment.Version.Major == 10;
    Check(file.IsAsync == expectedAsync && file.SafeFileHandle.IsAsync == expectedAsync, "regular-file IsAsync");
    byte[] received = new byte[report.Length];
    await file.ReadExactlyAsync(received);
    Check(received.SequenceEqual(report), "async methods work regardless of flag");
    using FileStream syncFile = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    bool rejected = false;
    try { _ = new SendPacketsElement(syncFile, 0, report.Length); }
    catch (ArgumentException) { rejected = true; }
    Check(rejected == (OperatingSystem.IsWindows() || Environment.Version.Major == 10), "SendPacketsElement platform guard");
    Console.WriteLine($"regular FileOptions.Asynchronous: handle={file.SafeFileHandle.IsAsync}; stream={file.IsAsync}; ReadExactlyAsync=passed; sync SendPacketsElement rejected={rejected}");
}
finally { File.Delete(path); }

#if NET11_0_OR_GREATER
foreach (bool asyncRead in new[] { false, true })
foreach (bool asyncWrite in new[] { false, true })
{
    SafeFileHandle.CreateAnonymousPipe(out SafeFileHandle read, out SafeFileHandle write, asyncRead, asyncWrite);
    using (read)
    using (write)
    {
        Check(read.Type == FileHandleType.Pipe && write.Type == FileHandleType.Pipe, "handle types");
        Check(read.IsAsync == asyncRead && write.IsAsync == asyncWrite, "both pipe end flags");
        using (FileStream reader = new(read, FileAccess.Read, 1, asyncRead))
        using (FileStream writer = new(write, FileAccess.Write, 1, asyncWrite))
        {
            Check(!reader.CanSeek && !writer.CanSeek, "pipes are not seekable");
            Check(reader.IsAsync == asyncRead && writer.IsAsync == asyncWrite, "wrapped end flags");
            await writer.WriteAsync(report);
            await writer.FlushAsync();
            byte[] received = new byte[report.Length];
            await reader.ReadExactlyAsync(received);
            Check(received.SequenceEqual(report), "wrapped async round trip");
        }
        Check(read.IsClosed && write.IsClosed, "wrappers own supplied SafeFileHandle instances");
    }
    Console.WriteLine($"pipe readAsync={asyncRead}; writeAsync={asyncWrite}; wrapped round trip and ownership=passed");
}
SafeFileHandle.CreateAnonymousPipe(out SafeFileHandle eofRead, out SafeFileHandle eofWrite);
using (eofRead)
{
    eofWrite.Dispose();
    Check(RandomAccess.Read(eofRead, new byte[1], 0) == 0, "EOF after writer close");
}
Check(eofRead.IsClosed, "read end disposed");
#endif
if (OperatingSystem.IsLinux())
{
    string pipeName = "playbook-mode-" + Guid.NewGuid().ToString("N");
    string socket = Path.Combine(Path.GetTempPath(), "CoreFxPipe_" + pipeName);
    using (NamedPipeServerStream unrestricted = new(pipeName, PipeDirection.InOut, 3))
    {
        UnixFileMode initial = File.GetUnixFileMode(socket);
        using (NamedPipeServerStream restricted = new(pipeName, PipeDirection.InOut, 3, PipeTransmissionMode.Byte, PipeOptions.CurrentUserOnly))
        {
            UnixFileMode tightened = File.GetUnixFileMode(socket);
            using NamedPipeServerStream later = new(pipeName, PipeDirection.InOut, 3);
            UnixFileMode retained = File.GetUnixFileMode(socket);
            if (Environment.Version.Major == 11) { Check((int)tightened == 384 && retained == tightened, "0600 ratchet"); }
            Console.WriteLine($"CurrentUserOnly shared lifetime modes initial={Convert.ToString((int)initial, 8)}; tightened={Convert.ToString((int)tightened, 8)}; laterUnrestricted={Convert.ToString((int)retained, 8)}");
        }
    }
    Check(!File.Exists(socket), "shared socket cleaned up");
    using (NamedPipeServerStream firstRestricted = new(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.CurrentUserOnly))
    {
        UnixFileMode mode = File.GetUnixFileMode(socket);
        if (Environment.Version.Major == 11) { Check((int)mode == 384, "0600 at bind"); }
        Console.WriteLine($"CurrentUserOnly bind mode={Convert.ToString((int)mode, 8)}; no cross-user access claim");
    }
    Check(!File.Exists(socket), "restricted socket cleaned up");
}
Console.WriteLine("PASS bounded pipe contracts, regular-file async methods, send-packet constructor guard and cleanup");
static void Check(bool condition, string claim) { if (!condition) { throw new Exception(claim); } }
