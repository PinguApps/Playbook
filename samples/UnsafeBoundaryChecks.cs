using Playbook.Samples;

internal static class UnsafeBoundaryChecks
{
    public static void Run()
    {
        var status = new BeaconStatus { Reading = 4661 };
        Console.WriteLine($"Safe layout reading: {status.Reading}");
        foreach ((byte[] bytes, ushort expected) in new[]
        {
            (new byte[] { 0x35, 0x12 }, (ushort)0x1235),
            (new byte[] { 0, 0 }, (ushort)0),
            (new byte[] { 0xff, 0xff, 7 }, ushort.MaxValue)
        })
        {
            if (BeaconFrame.Decode(bytes) != expected) { throw new InvalidOperationException("Wrong beacon reading."); }
            if (bytes[0] != (byte)expected || bytes[1] != (byte)(expected >> 8)) { throw new InvalidOperationException("Input changed."); }
        }

        foreach (byte[] shortFrame in new[] { Array.Empty<byte>(), new byte[] { 1 } })
        {
            try
            {
                BeaconFrame.Decode(shortFrame);
                throw new InvalidOperationException("Truncated frame was accepted.");
            }
            catch (ArgumentException) { }
        }

        Console.WriteLine($"PASS: unsafe boundary decoding, safe layout field, unchanged input and truncated-frame rejection; runtime {Environment.Version}");
    }
}
