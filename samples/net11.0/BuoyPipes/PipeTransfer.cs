using System.IO.Pipes;
using Microsoft.Win32.SafeHandles;

namespace BuoyPipes;

// snippet:transfer
public static class PipeTransfer
{
    public static byte[] ReadReport(byte[] report)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(report.Length, 256);
        SafeFileHandle.CreateAnonymousPipe(out SafeFileHandle read, out SafeFileHandle write);
        using (read)
        using (write)
        {
            if (read.Type != FileHandleType.Pipe || write.Type != FileHandleType.Pipe)
            {
                throw new IOException("Expected two pipe ends.");
            }
            // A nonnegative offset is required but ignored for a pipe. No seeking occurs.
            RandomAccess.Write(write, report, fileOffset: 0);
            byte[] received = new byte[report.Length];
            int total = 0;
            while (total < received.Length)
            {
                int count = RandomAccess.Read(read, received.AsSpan(total), fileOffset: 0);
                if (count == 0) { throw new EndOfStreamException(); }
                total += count;
            }
            return received;
        }
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
        using NamedPipeClientStream reader = new(PipeDirection.In, isAsync, ownedHandle);
        int result = reader.ReadByte();
        return checked((byte)result);
    }
}
// endsnippet:borrowed
