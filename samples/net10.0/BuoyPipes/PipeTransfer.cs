using System.IO.Pipes;
using Microsoft.Win32.SafeHandles;

namespace BuoyPipes;

// snippet:transfer
public static class PipeTransfer
{
    public static byte[] ReadReport(byte[] report)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(report.Length, 256);
        using AnonymousPipeServerStream writer = new(PipeDirection.Out, HandleInheritability.None);
        using AnonymousPipeClientStream reader = new(PipeDirection.In, writer.ClientSafePipeHandle);
        // This bounded message fits in the pipe; a large stream needs a concurrent writer.
        writer.Write(report);
        byte[] received = new byte[report.Length];
        reader.ReadExactly(received);
        return received;
    }
}
// endsnippet:transfer

// snippet:borrowed
public static class ConnectedReport
{
    // Transfer this SafePipeHandle instance to the reader. Do not use another stream
    // that owns it afterwards; disposing the reader closes the supplied instance.
    public static byte ReadOne(SafePipeHandle ownedHandle, bool isAsync)
    {
        using NamedPipeClientStream reader = new(PipeDirection.In, isAsync, isConnected: true, ownedHandle);
        int result = reader.ReadByte();
        return checked((byte)result);
    }
}
// endsnippet:borrowed
